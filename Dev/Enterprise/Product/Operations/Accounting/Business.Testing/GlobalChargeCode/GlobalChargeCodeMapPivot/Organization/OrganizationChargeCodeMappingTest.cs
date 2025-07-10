using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(OrganizationChargeCodeMapping))]
	internal class OrganizationChargeCodeMappingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestChargeCodePivotCollection()
		{
			var currentCompanyChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var globalChargeCodeValid = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCodeValid.YG_OH = org.PK;
			var validPivot = globalChargeCodeValid.PivotCollection.AddNew();
			validPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			validPivot.YP_AC = currentCompanyChargeCode.PK;

			var globalChargeCodeValid2 = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCodeValid2.YG_OH = org.PK;
			var validPivot2 = globalChargeCodeValid2.PivotCollection.AddNew();
			validPivot2.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			validPivot2.YP_AC = currentCompanyChargeCode.PK;

			Factory.Save();

			var mapping = new OrganizationChargeCodeMapping(Factory);
			AssertEquals(0, mapping.ChargeCodePivotCollection.Count);
			Assert(mapping.ChargeCodePivotCollection.ReadOnly);

			mapping.Organization_PK = org.PK;
			AssertEquals(2, mapping.ChargeCodePivotCollection.Count);
			Assert(mapping.ChargeCodePivotCollection.Contains(validPivot.PK));
			Assert(mapping.ChargeCodePivotCollection.Contains(validPivot2.PK));
			Assert(!mapping.ChargeCodePivotCollection.ReadOnly);

			mapping.Organization_PK = ZGuid.Invalid;
			AssertEquals(0, mapping.ChargeCodePivotCollection.Count);
			Assert(mapping.ChargeCodePivotCollection.ReadOnly);
		}
	}
}
