using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	[DefaultDataSourceBindingMember(null)]
	[DefaultBindingProperty("CFIAPGAHeader")]
	public partial class CFIAUserControl : ZUserControl
	{
		public CFIAUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();
			InitializeLazyCreate(isOnInvoiceLine);
		}

		protected void InitializeLazyCreate(bool isOnInvoiceLine)
		{
			LPCOGridUserControl.RemoveExceptAvailableColumns(CFIAPGAHeader.AvailableLPCOFields);

			if (!isOnInvoiceLine)
			{
				DetailsGroupBox.Controls.Remove(DeliveryAddressUserControl);
			}
		}

		public new CFIAPGAHeader CurrentDataItem => base.CurrentDataItem as CFIAPGAHeader;

		void AIRSToolLinkButton_Click(object sender, EventArgs e)
		{
			var tariff = CurrentDataItem?.RequirementsParent?.Tariff ?? ZString.Empty;
			if (tariff.IsEmpty)
			{
				Globals.Message.ShowWarning(Res.GetString("43CA70F4-A813-40E5-A655-55A7EDE1E332", "Classification Number is not entered."));
			}
			else
			{
				var navigator = new AIRSWebpageNavigator(CurrentDataItem.Factory, tariff);
				using (var selectionForm = new AIRSWebPageNaviagtorForm(navigator))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(selectionForm) == DialogResult.OK)
					{
						CurrentDataItem.CopyAIRSToCFIA(navigator);
					}
				}
			}
		}
	}
}
