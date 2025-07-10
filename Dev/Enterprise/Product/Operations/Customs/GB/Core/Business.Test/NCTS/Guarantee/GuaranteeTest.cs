using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(Guarantee))]
	public class GuaranteeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var cusBondData = Factory.New<Guarantee>();
			AssertType<GuaranteeLookups>(cusBondData.Lookups);
		}

		public void TestPW_OverrideReadOnly()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var guarantee = nctsHeader.Guarantees.AddNew();
			try
			{
				Env.Security.NctsDepartureAllowBondAmountOverride.IsAllowed = true;
				AssertEquals("Security, PW_OverrideReadOnly", false, guarantee.PW_OverrideInfo.ReadOnly);
				Env.Security.NctsDepartureAllowBondAmountOverride.IsAllowed = false;
				AssertEquals("Without security, PW_OverrideReadOnly", true, guarantee.PW_OverrideInfo.ReadOnly);
			}
			finally
			{
				Env.Security.NctsDepartureAllowBondAmountOverride.ClearOverriddenSecurityValue();
			}
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.MovementHeader.Guarantees.AddNew();
		}
	}
}
