using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using CargoWise.Bi.Development.SsasBuilder;
using CargoWise.Main.Navigation.WPF.Test;

namespace CargoWise.Bi.Development.Automation.Manager
{
	#region SuppressResourceStringsCheckRegion

	[TestExcludeWPFControlFromBasher]
	public partial class LoadFromTabularModel : Window, IDisposable
	{
		public string ModelName { get; private set; }

		public string AnalysisServer { get; private set; }
		public string DatabaseName { get; private set; }

		public string[] GetTableNames()
		{
			return tableNames;
		}
		string[] tableNames;

		public LoadFromTabularModel()
		{
			InitializeComponent();
			analysisServerTextBox.Text = SsasProjectBuilder.AnalysisServerName;
			LoadDatabaseList();
		}

		void AddButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(analysisServerTextBox.Text) ||
				string.IsNullOrEmpty(databaseTextBox.Text) ||
				string.IsNullOrEmpty(modelNameTextBox.Text) ||
				string.IsNullOrEmpty(tableTextBox.Text))
			{
				MessageBox.Show("All fields are required.", "Missing field"); // Not using ZorKArchitecture
			}
			else
			{
				AnalysisServer = analysisServerTextBox.Text;
				DatabaseName = databaseTextBox.Text;
				if (tableTextBox.Text == "Import All Tables")
				{
					tableNames = SsasProjectBuilder.LoadTableList(analysisServerTextBox.Text, databaseTextBox.Text).ToArray();
				}
				else
				{
					tableNames = new[] { tableTextBox.Text };
				}
				Close();
			}
		}

		void CancelButtion_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		void AnalysisServerTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			LoadDatabaseList();
		}

		void LoadDatabaseList()
		{
			if (!string.IsNullOrEmpty(analysisServerTextBox.Text))
			{
				databaseTextBox.ItemsSource = SsasProjectBuilder.LoadDatabaseList(analysisServerTextBox.Text);
			}
		}

		void AnalysisServerTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			SsasProjectBuilder.AnalysisServerName = analysisServerTextBox.Text;
		}

		void DatabaseTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			LoadTableList();
		}

		void LoadTableList()
		{
			if (!string.IsNullOrEmpty(analysisServerTextBox.Text) && !string.IsNullOrEmpty(databaseTextBox.Text))
			{
				var tableList = new List<string> { "Import All Tables" };
				tableList.AddRange(SsasProjectBuilder.LoadTableList(analysisServerTextBox.Text, databaseTextBox.Text));
				tableTextBox.ItemsSource = tableList;
				tableTextBox.SelectedIndex = 0;
			}
		}

		void ModelNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			ModelName = modelNameTextBox.Text;
		}

		#region Dispose

		bool disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
				}
				disposed = true;
			}
		}

		#endregion

		void DatabaseTextBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				var regex = new Regex(@"(_|\b)(?<modelName>[A-Za-z]*Model)(_|\b)", RegexOptions.IgnoreCase);
				var match = regex.Match((string)e?.AddedItems[0]);
				if (match.Success)
				{
					modelNameTextBox.Text = match.Groups["modelName"].Value;
				}
			}
		}
	}
	#endregion
}
