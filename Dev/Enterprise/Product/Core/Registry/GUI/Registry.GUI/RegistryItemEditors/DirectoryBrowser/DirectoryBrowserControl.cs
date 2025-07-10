using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class DirectoryBrowserControl : ZUserControl
	{
		public DirectoryBrowserControl()
		{
			InitializeComponent();
		}

		public string GetValue()
		{
			return DirectoryTextBox.Text;
		}

		public void SetValue(string value)
		{
			DirectoryTextBox.Text = value;
		}

		#region Implementation

		void DirectoryBrowserButton_Click(object sender, EventArgs e)
		{
			string newText = GetFolderBrowserText();
			if (!string.IsNullOrEmpty(newText))
			{
				DirectoryTextBox.Text = newText;
			}
		}

		protected virtual string GetFolderBrowserText()
		{
			var folderBrowserDialog = new ZFolderBrowserDialog();
			string result = "";

			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					result = folderBrowserDialog.UnmappedSelectedPath;
				}
				catch (NotSupportedException e)
				{
					result = (NoResString)"Error: " + e.Message;
				}
			}

			return result;
		}

		#endregion
	}
}
