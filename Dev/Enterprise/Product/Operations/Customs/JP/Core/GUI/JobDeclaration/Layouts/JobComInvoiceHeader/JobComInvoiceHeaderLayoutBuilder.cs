using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;

namespace Enterprise.Customs.JP.GUI
{
	public class JobComInvoiceHeaderLayoutBuilder : CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>
	{
		public JobComInvoiceHeaderControlBag JPControlBag => JobComInvoiceHeaderControlBag.Instance;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(CommonBag.InvoiceCurrLandedCostExRateCalcEdit, x => !x.JobDeclaration?.IsExport ?? true);
		}

		protected override void SetDefaultCaptions()
		{
			base.SetDefaultCaptions();
			SetCaption(CommonBag.IncoTermPlaceTextBox, GetIncoTermPlaceTextBoxCaption);
		}

		static ResourceStringData GetIncoTermPlaceTextBoxCaption(JobComInvoiceHeader header)
		{
			return Res.GetData("d1a27d24-78fa-4989-994b-a1fbbb38bb66", "Agreed Place", "Place where the seller will deliver the goods to the carrier or another person nominated, based on an agreement between the parties.");
		}
	}
}
