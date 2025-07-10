using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class ImportJobDeclaration : Customs.Business.ImportJobDeclaration
	{
		public ImportJobDeclaration(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void PerformCountrySpecificImporting()
		{
			base.PerformCountrySpecificImporting();
			var importedDeclaration = ImportedDeclaration as JobDeclaration;
			if (importedDeclaration != null)
			{
				importedDeclaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Default;
				importedDeclaration.UpdateMergeBy();
			}
		}
	}
}
