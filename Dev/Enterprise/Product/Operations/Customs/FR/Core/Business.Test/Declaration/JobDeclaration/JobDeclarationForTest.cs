using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class JobDeclarationForTest : JobDeclaration
	{
		public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override EU.Business.Declaration.VATDeferStrategy GetVATDeferStrategyCore()
		{
			return new VATDeferStrategyForTest(this);
		}
	}

	class VATDeferStrategyForTest : VATDeferStrategy
	{
		public VATDeferStrategyForTest(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
		{
		}

		protected override void DefaultPaymentMethod()
		{
			// There's a Reflection test failing UT: http://crikey.wtg.zone/TestResults/f177fe9f-9d85-4a98-89a6-102e3e88a61b
			// It used to pass ok, because prior do change tone in FR.JobDeclaration l.635,  failing test would not enter EU.VATDeferStrategy.DefaultPaymentMethod().
			// Since change was done, it checks for Declaration.CountryCode, and since it's AU (Because it is executing the reflection test),  Enterprise.MasterFiles.Business OrgImpAddInfoTypeDecider.GetRegionSpecificTypeForNewOrgAddInfo reports an error.
			if (Declaration.CountryCode == Core.Constants.CountryCodes.France)
			{
				base.DefaultPaymentMethod();
			}
		}
	}
}
