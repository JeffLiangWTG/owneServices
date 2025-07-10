using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Chief.Declaration
{
	internal class ChiefJobComInvoiceLineValueSetStrategy : JobComInvoiceLineValueSetStrategy
	{
		public ChiefJobComInvoiceLineValueSetStrategy(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected override string SupportingDocumentTypeForValueMethodOne => "N934";

		protected override void CreateSupportingDocumentForValueMethodOne()
		{
			CreateSupportingDocument(SupportingDocumentTypeForValueMethodOne);
		}

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValueSetCore(valueThatHasChanged, oldValue);
			switch (valueThatHasChanged.Name)
			{
				case JobComInvoiceLine.Schema.JI_PrimaryPreference:
					HandlePrimaryPreferenceChanged(valueThatHasChanged, oldValue);
					break;
			}
		}

		void HandlePrimaryPreferenceChanged(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			if (invoiceLine.UseUniversalTariff)
			{
				var preferenceToA00Creator = new ChiefPreferenceToA00Creator(invoiceLine, valueThatHasChanged.Value);
				preferenceToA00Creator.CreateOrUpdateA00();
			}
		}
	}
}

