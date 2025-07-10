using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
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
			if (ImportedDeclaration.JE_PaymentMethod == ZString.Empty)
			{
				ImportedDeclaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Default;
			}
		}
	}
}
