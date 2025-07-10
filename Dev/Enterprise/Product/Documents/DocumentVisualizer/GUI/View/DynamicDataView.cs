using System;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Models;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	partial class DynamicDataView : ZChildForm
	{
		public DynamicDataView(IDocument document, DataViewModel.DataType dataType, string userDefinedNamespace = "", string dataContext = "")
			: base(new DataViewModel(document, dataType, userDefinedNamespace, dataContext))
		{
			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			var data = (DataViewModel)DataSource;

			refreshButton.Click += (s, args) => data.UpdateText();
			copyToClipboardButton.Click += (s, args) => CopyTextToClipboard();
			closeButton.Click += (s, args) => Close();

			data.UpdateText();
		}

		void CopyTextToClipboard()
		{
			if (!string.IsNullOrEmpty(dataView.Text))
			{
				SafeClipboard.SetText(dataView.Text);
			}
		}
	}
}
