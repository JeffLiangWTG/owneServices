using System;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.GUI
{
	internal sealed partial class DocumentDeliveryControl : ZUserControl
	{
		public DocumentDeliveryControl()
		{
			InitializeComponent();

			deliverDocumentsInOneEmailCheckBox.CheckStateChanged += DeliverDocumentsInOneEmailCheckBoxOnCheckStateChanged;
		}

		void DeliverDocumentsInOneEmailCheckBoxOnCheckStateChanged(object sender, EventArgs e)
		{
			var overrideEmailVisible = false;
			if (DataSource is OperationalActionRunner operationalActionRunner)
			{
				overrideEmailVisible = operationalActionRunner.OverrideRecipientEmailEnabled;
			}

			overrideEmailCheckBox.Visible = overrideEmailVisible;
			emailTextBox.Visible = overrideEmailVisible;
		}
	}
}
