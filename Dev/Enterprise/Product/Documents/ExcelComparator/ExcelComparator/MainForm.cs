using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Enterprise.ExcelComparator
{
	public partial class MainForm : Form
	{
		public MainForm(string filePath1, string filePath2)
		{
			InitializeComponent();
			this.KeyPreview = true;
			this.FilePath1 = filePath1;
			this.FilePath2 = filePath2;
		}

		internal string FilePath1
		{
			get { return file1TextBox.Text; }
			set { file1TextBox.Text = value ?? ""; }
		}

		internal string FilePath2
		{
			get { return file2TextBox.Text; }
			set { file2TextBox.Text = value ?? ""; }
		}

		#region Event and KeyPress event handlers

		internal void file1ChangeButton_Click(object sender, EventArgs e)
		{
			ChangeFilePath(file1TextBox);
		}

		internal void file2ChangeButton_Click(object sender, EventArgs e)
		{
			ChangeFilePath(file2TextBox);
		}

		void compareButton_Click(object sender, EventArgs e)
		{
			Compare();
		}

		void closeButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void centralKeyPressHandler(object sender, KeyEventArgs e)
		{
			if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				compareButton_Click(sender, e);
			}
			else if (e.KeyCode == Keys.Escape)
			{
				e.Handled = true;
				closeButton_Click(sender, e);
			}
		}

		#endregion

#if DEBUG

		internal void SetCompareSectionsOptionForTesting(bool value)
		{
			compareSectionsCheckBox.Checked = value;
		}

#endif

		void ChangeFilePath(TextBox textBoxWithFilePathInIt)
		{
			try
			{
				this.openFileDialog.InitialDirectory = Path.GetDirectoryName(textBoxWithFilePathInIt.Text);
				this.openFileDialog.FileName = textBoxWithFilePathInIt.Text;
			}
			catch (ArgumentException) { } // Happens when the path is invalid.

			DialogResult result = DialogResult.OK;
#if DEBUG
			if (DialogsForTesting != null)
			{
				DialogsForTesting.Add(new Dialog() { Title = "Select File", Message = string.Format("InitialDirectory=[{0}]\r\nFileName=[{1}]", this.openFileDialog.InitialDirectory, this.openFileDialog.FileName) });
			}
			else
#endif
			{
				result = this.openFileDialog.ShowDialog(this);
			}

			if (result == DialogResult.OK)
			{
				textBoxWithFilePathInIt.Text = this.openFileDialog.FileName;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		internal void Compare()
		{
			using (var generator1 = new ComparableTextGenerator(FilePath1, 1))
			using (var generator2 = new ComparableTextGenerator(FilePath2, 2))
			{
				if (compareSectionsCheckBox.Checked)
				{
					generator1.AddOption(new CompareSectionsOption());
					generator2.AddOption(new CompareSectionsOption());
					generator1.AddOption(new InsertMissingSectionsOption(FilePath2));
					generator2.AddOption(new InsertMissingSectionsOption(FilePath1));
				}

				if (GenerateComparisonFiles(generator1, generator2))
				{
					if (generator1.Content == generator2.Content)
					{
						ShowDialog("Excel Comparator", "Excel Files have Identical comparable text content.", MessageBoxIcon.Information);
					}
					else
					{
						CallComparisonEngine(generator1.TempFileName, generator2.TempFileName);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule", Justification = "ConcurrentBag is only converted to array once threads have completed")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		bool GenerateComparisonFiles(params ComparableTextGenerator[] generators)
		{
			var errors = new ConcurrentBag<string>();
			Parallel.ForEach(generators, generator =>
			{
				if (!generator.SaveAsComparableTextTempFile())
				{
					errors.Add(generator.ErrorText);
				}
			});

			if (errors.Count != 0)
			{
				var errorMessage = string.Join("\r\n\r\n", errors);
				ShowDialog("Error Generating Comparison Files", errorMessage, MessageBoxIcon.Error);
				return false;
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void CallComparisonEngine(string filePath1, string filePath2)
		{
			IComparisonTool comparisonTool = ComparisonTools.FirstOrDefault(tool => tool.IsInstalled());
			if (comparisonTool == null)
			{
				ShowDialog("Error Starting Comparison Process", CouldNotFindAnyComparisonTool, MessageBoxIcon.Error);
			}
			else
			{
				comparisonTool.RunComparison(filePath1, filePath2);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		string CouldNotFindAnyComparisonTool
		{
			get
			{
				var comparisonToolPaths = ComparisonTools.Select(tool => tool.Path);

				return
@"Could not find any recognised comparison tools on the local drive.

Please install one of the utilities listed below:-
" + string.Join("\r\n", comparisonToolPaths);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Internal tools")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		internal List<IComparisonTool> ComparisonTools
		{
			get
			{
				if (comparisonTools == null)
				{
					comparisonTools = new List<IComparisonTool>();

					string storedComparisonPreference = FindComparisonToolPreferenceFromVisualStudio();
					if (!string.IsNullOrEmpty(storedComparisonPreference))
					{
						comparisonTools.Add(new ComparisonTool(storedComparisonPreference));
					}

					comparisonTools.Add(new ComparisonTool(@"C:\Program Files\Beyond Compare 4\BComp.exe"));
					comparisonTools.Add(new ComparisonTool(@"C:\Program Files (x86)\SourceGear\DiffMerge\DiffMerge.exe"));
					comparisonTools.Add(new ComparisonTool(@"C:\Program Files (x86)\Beyond Compare 3\BCompare.exe"));
					comparisonTools.Add(new ComparisonTool(@"C:\Program Files (x86)\Araxis\Araxis Merge v6.5\Compare.exe"));
					comparisonTools.Add(new ComparisonTool(@"C:\Program Files\Araxis\Araxis Merge v6.5\Compare.exe"));
					comparisonTools.Add(new ComparisonTool(@"C:\Program Files (x86)\KDiff3\kdiff3.exe"));
					comparisonTools.Add(new ComparisonTool(@"C:\Program Files\KDiff3\kdiff3.exe"));
					comparisonTools.Add(new ComparisonTool(@"C:\Program Files\SourceGear\Common\DiffMerge\sgdm.exe")); // For DiffMerge 4.2 64x
					comparisonTools.Add(new ComparisonTool(@"C:\Program Files\WinMerge\WinMergeU.exe")); // For WinMerge 2.16.40.0 x64
				}

				return comparisonTools;
			}
		}
		internal List<IComparisonTool> comparisonTools;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		string FindComparisonToolPreferenceFromVisualStudio()
		{
			var storedComparisonPreference = string.Empty;

			try
			{
				var comparisonPreferenceRegistryNames = new[]
				{
					@"SOFTWARE\Microsoft\VisualStudio\15.0\TeamFoundation\SourceControl\DiffTools\.*\Compare",
					@"SOFTWARE\Microsoft\VisualStudio\14.0\TeamFoundation\SourceControl\DiffTools\.*\Compare"
				};

				foreach (var comparisonPreferenceRegistryName in comparisonPreferenceRegistryNames)
				{
					var comparisonPreferenceRegistryKey = Registry.CurrentUser.OpenSubKey(comparisonPreferenceRegistryName);
					storedComparisonPreference = comparisonPreferenceRegistryKey?.GetValue("Command")?.ToString();

					if (!string.IsNullOrEmpty(storedComparisonPreference))
					{
						break;
					}
				}
			}
			catch (Exception e) when
			(
				e is ObjectDisposedException
				|| e is UnauthorizedAccessException
				|| e is System.Security.SecurityException
				|| e is System.IO.IOException
			)
			{
				// Let's feast on non life-threatening exceptions from registry. Nom nom nom!
			}

			return storedComparisonPreference;
		}

		#region ShowDialog(string errorTitle, string errorMessage) method with Dialog Supression for Testing

		void ShowDialog(string errorTitle, string errorMessage, MessageBoxIcon icon)
		{
#if DEBUG
			if (DialogsForTesting != null)
			{
				DialogsForTesting.Add(new Dialog() { Title = errorTitle, Message = errorMessage });
				return;
			}
#endif
			MessageBox.Show(this, errorMessage, errorTitle, MessageBoxButtons.OK, icon);
		}

#if DEBUG

		internal List<Dialog> DialogsForTesting;

		internal struct Dialog
		{
			public string Title;
			public string Message;
		}

#endif

		#endregion
	}
}
