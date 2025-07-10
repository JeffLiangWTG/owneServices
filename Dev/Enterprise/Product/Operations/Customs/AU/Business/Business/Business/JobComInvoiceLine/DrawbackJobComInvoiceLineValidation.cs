using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DrawbackJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public DrawbackJobComInvoiceLineValidation(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected JobComInvoiceLine InvoiceLine
		{
			get { return Parent; }
		}

		protected override bool IsTariffMandatory
		{
			get { return false; }
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();
			if (!Parent.JI_Tariff.IsEmpty && (Parent.ImportTariff == null || (!AUCClassWrapper.UseCustomsReferenceData && InvoiceLine.StatClassificationWrapper == null)))
			{
				Parent.JI_TariffInfo.AddMessageError("Invalid Tariff/Stat code combination for " + Parent.JI_Tariff + ", for the date " + Parent.DrawbackLineEffectiveDutyDate.ToString("d"));
			}
		}

		public override ZString CustomsUQ => InvoiceLine.Factory.GetValue(ref cachedCustomsUQ, () => InvoiceLine.StatClassificationWrapper?.QuantityUnit ?? ZString.Empty);
		CachedProperty<ZString> cachedCustomsUQ;
	}
}
