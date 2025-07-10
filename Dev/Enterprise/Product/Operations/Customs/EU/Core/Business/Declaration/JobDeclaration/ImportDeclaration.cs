using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business
{
	public class ImportDeclaration : Customs.Business.ImportJobDeclaration
	{
		public ImportDeclaration(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void PerformCountrySpecificImporting()
		{
			base.PerformCountrySpecificImporting();
			ImportedDeclaration.JE_OH_Supplier = Declaration.JE_OH_Supplier;
			ImportedDeclaration.JE_OH_Importer = Declaration.JE_OH_Importer;
		}
	}
}
