using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.GUI
{
	public partial class CustomizableDataTranslationForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public CustomizableDataTranslationForm()
		{
			InitializeComponent();
		}

		public CustomizableDataTranslationForm(CustomizableDataTranslationPage bo)
			: base(bo)
		{
			InitializeComponent();

			ZFormPostingButtonsStrategy.SetupPosting(this, zPostingButtonsUserControl);

			this.Text = Res.GetString("1bf5ccfe-be19-406f-bb9d-03ec7529249c", "Translations: {0}", bo.Description);

			new ColumnResizer(translationsOfCurrentValueGrid, CustomizableDataTranslationEntry.Schema.English, CustomizableDataTranslationEntry.Schema.Translation);
			new RowResizer(translationsOfCurrentValueGrid, CustomizableDataTranslationEntry.Schema.English, CustomizableDataTranslationEntry.Schema.Translation);

			new ColumnResizer(allValuesGrid, CustomizableDataTranslationEntry.Schema.English, CustomizableDataTranslationEntry.Schema.Translation);
			new RowResizer(allValuesGrid, CustomizableDataTranslationEntry.Schema.English, CustomizableDataTranslationEntry.Schema.Translation);

			translationsOfCurrentValueGrid.AfterBind += new EventHandler(translationsOfCurrentValueGrid_AfterBind);
			allValuesGrid.AfterBind += new EventHandler(allValuesGrid_AfterBind);

			ZFormMenuStrategy.AddAdornments(this);
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("9BE878A3-186B-4321-96FE-A667CA53A04B", "Bulk Export Text"), OnExport);
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("BC3C56FB-A653-443E-A038-ACAFA98EB30F", "Bulk Import Text"), OnImport);
		}

		protected void OnExport(object sender, EventArgs e)
		{
			var dir = GetDirectory(Res.GetString("7A28D63B-0AF5-480F-B64B-977259C56512", "Please select the folder into which the exported files should be saved. Existing files will be overridden."));
			if (!string.IsNullOrEmpty(dir))
			{
				var export = Page.ExportEntries();
				var errors = new List<string>();
				var invalid = Path.GetInvalidFileNameChars();
				var prefix = Page.Description;

				foreach (char c in invalid)
				{
					prefix = prefix.Replace(c.ToString(), "");
				}

				foreach (var entry in export)
				{
					string filename = string.Format(CultureInfo.InvariantCulture, "{0}-{1}.csv", prefix, entry.Key);

					string path = Path.Combine(dir, filename);

					if (!SaveFile(path, entry.Value.ToString()))
					{
						errors.Add(filename);
					}
				}

				string message = Res.GetString("B987E65C-F3BA-49FB-AFB5-959123CC1060", "Export has been completed. The files can be found at {0}.", dir);

				if (errors.Count > 0)
				{
					message += " ";
					message += Res.GetString("962E5E7A-333B-4795-8900-BA30425A6D65", "The files for the following languages were not saved: {0}.", string.Join(", ", errors.ToArray()));
				}

				Globals.Message.Show(message);
			}
		}

		protected virtual bool SaveFile(string path, string content)
		{
			try
			{
				#if !WINZOR
				if (isNeedingToUseEnterpriseChannel)
				{
					return RemoteFileDialog.SaveFile(path, Encoding.UTF8.GetBytes(content));
				}
				File.WriteAllText(path, content, Encoding.UTF8);
				#else
				return RemoteFileDialog.SaveFile(path, Encoding.UTF8.GetBytes(content));
				#endif
			}
			catch (IOException)
			{
				return false;
			}
			#if !WINZOR
			return true;
			#endif
		}

#if DEBUG
		public
#endif
		bool isNeedingToUseEnterpriseChannel;

		protected virtual string GetDirectory(string description)
		{
			string directory = string.Empty;
			isNeedingToUseEnterpriseChannel = false;

			using (var browser = new ZFolderBrowserDialog())
			{
				browser.ShowNewFolderButton = true;
				browser.Description = description;

				browser.RequireMappablePath = true;
				if (browser.ShowDialog(null, false) == DialogResult.OK)
				{
					if (browser.IsNeedingToUseEnterpriseChannel)
					{
						isNeedingToUseEnterpriseChannel = true;
						directory = browser.UnmappedSelectedPath;
					}
					else
					{
						directory = browser.MappedSelectedPath;
					}
				}
			}

			return directory;
		}

		protected void OnImport(object sender, EventArgs e)
		{
			var dir = GetDirectory(Res.GetString("38B06113-3ABA-438E-80EB-91D4E34EB33E", "Please select the folder with the files to import."));
			if (!string.IsNullOrEmpty(dir))
			{
				var files = isNeedingToUseEnterpriseChannel ? RemoteFileDialog.ListDirectoryFiles(dir, ".csv")
				: Directory.EnumerateFileSystemEntries(dir, "*.csv", SearchOption.TopDirectoryOnly).ToArray();
				if (files == null)
				{
					Globals.Message.ShowError(Res.GetString("8B04C0B9-5849-4F94-970C-4DB4070B77F5", "The selected path cannot be used, please select a new path."));
				}
				else
				{
					var errors = new List<string>();
					foreach (var entry in files)
					{
						try
						{
							using (var importFileStream = GetImportFileStream(entry))
							{
								var error = Page.ImportEntries(importFileStream, entry);
								if (!string.IsNullOrEmpty(error))
								{
									errors.Add(error);
								}
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							errors.Add(Res.GetString("2E32CE9D-289D-435C-8B51-15460014517A", "Unable to import {0}. Error: {1}",
								entry, ex.Message));
						}
					}

					if (errors.Count > 0)
					{
						Globals.Message.ShowError(Res.GetString("D5C4FAD5-1CAA-4D25-9AA6-2C5973A85033", "Import completed with errors: {0}", string.Join(", ", errors.ToArray())));
					}
					else
					{
						Globals.Message.Show(Res.GetString("F42E4EB1-98AB-496A-BC6D-B6C788522200", "Import has been successful"));
					}
				}
			}
		}

		Stream GetImportFileStream(string fileName)
		{
			if (isNeedingToUseEnterpriseChannel)
			{
				return new MemoryStream(RemoteFileDialog.OpenFile(fileName));
			}
			return File.OpenRead(fileName);
		}

		void translationsOfCurrentValueGrid_AfterBind(object sender, EventArgs e)
		{
			translationsOfCurrentValueGrid.ListManager.CurrentChanged += new EventHandler(translationsOfCurrentValueGrid_CurrentChanged);
		}

		void allValuesGrid_AfterBind(object sender, EventArgs e)
		{
			allValuesGrid.ListManager.CurrentChanged += new EventHandler(allValuesGrid_CurrentChanged);

			changingCurrent = true;
			UpdateTranslationsOfCurrentGrid();
			UpdateAllValuesGrid();
			changingCurrent = false;
		}

		CustomizableDataTranslationPage Page
		{
			get { return (CustomizableDataTranslationPage)BusinessEntity; }
		}

		void translationsOfCurrentValueGrid_CurrentChanged(object sender, EventArgs e)
		{
			if (!changingCurrent)
			{
				var current = translationsOfCurrentValueGrid.ListManager.GetCurrent() as CustomizableDataTranslationEntry;
				if (current != null)
				{
					changingCurrent = true;
					Page.CurrentLanguage = current.Language;
					UpdateAllValuesGrid();
					changingCurrent = false;
				}
			}
		}

		void allValuesGrid_CurrentChanged(object sender, EventArgs e)
		{
			if (!changingCurrent)
			{
				var current = allValuesGrid.ListManager.GetCurrent() as CustomizableDataTranslationEntry;
				if (current != null)
				{
					changingCurrent = true;
					Page.CurrentCaption = current.Caption;
					UpdateTranslationsOfCurrentGrid();
					changingCurrent = false;
				}
			}
		}

		bool changingCurrent;

		void UpdateAllValuesGrid()
		{
			string currentLanguageDescription = new CodeDescriptionPairList(OLookUpEditType.Language).GetDescriptionFromCode(Page.CurrentLanguage);
			allValuesLabel.Text = Res.GetString("d54fc5ec-930f-451b-acee-944bd3925759", "All values in {0}", currentLanguageDescription);
			allValuesGrid.Columns[CustomizableDataTranslationEntry.Schema.Translation].ColumnStyle.HeaderText = currentLanguageDescription;
			for (int i = 0; i < allValuesGrid.ListManager.List.Count; i++)
			{
				if (((CustomizableDataTranslationEntry)allValuesGrid.ListManager.List[i]).Caption.ResourceKey == Page.CurrentCaption.ResourceKey)
				{
					allValuesGrid.ListManager.Position = i;
					break;
				}
			}
		}

		void UpdateTranslationsOfCurrentGrid()
		{
			for (int i = 0; i < translationsOfCurrentValueGrid.ListManager.List.Count; i++)
			{
				if (((CustomizableDataTranslationEntry)translationsOfCurrentValueGrid.ListManager.List[i]).Language == Page.CurrentLanguage)
				{
					translationsOfCurrentValueGrid.ListManager.Position = i;
					break;
				}
			}
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override void SaveInternal()
		{
			Page.Save();
		}
	}
}
