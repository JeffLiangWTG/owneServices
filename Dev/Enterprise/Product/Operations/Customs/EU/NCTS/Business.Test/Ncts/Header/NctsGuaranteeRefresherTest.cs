using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsGuaranteeRefresher))]
	public class NctsGuaranteeRefresherTest : TestCaseWithFactory
	{
		public void TestGuaranteeRefresherClearExistingGuarantees()
		{
			var org1 = CreateOrgHeader("ORG1");
			var org2 = CreateOrgHeader("ORG2");
			var guaranteeHeader1 = CreateGuaranteeHeader("19860101", EUGuaranteeTypeList.Codes.COD);
			var guaranteeHeader2 = CreateGuaranteeHeader("19860102", EUGuaranteeTypeList.Codes.COD);

			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			var movementHeader = nctsHeader.MovementHeader;

			var refresher = nctsHeader.GuaranteeRefresher;
			movementHeader.Guarantees.RemoveAndDeleteAll();
			guaranteeHeader1.CPH_OH_PermitHolder = org1.PK;
			guaranteeHeader2.CPH_OH_PermitHolder = org2.PK;

			refresher.PopulateGuaranteeWithFallbacks();
			AssertEquals(1, movementHeader.Guarantees.Count);

			nctsHeader.Principal.E2_OA_Address = org2.MainAddress.PK;
			AssertEquals("Total guarantees when previous guarantees are not cleared", 2, movementHeader.Guarantees.Count);

			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfigurationClearExistingGuaranteesConfiguration(Factory, true))
			{
				refresher.PopulateGuaranteeWithFallbacks();
				AssertEquals("Total guarantees when previous guarantees are cleared", 1, movementHeader.Guarantees.Count);
			}

			OrgHeader CreateOrgHeader(ZString code)
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = code;
				return orgHeader;
			}

			CusGuaranteeHeader CreateGuaranteeHeader(ZString guaranteeNumber, ZString guaranteeType)
			{
				var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				guaranteeHeader.CPH_Number = guaranteeNumber;
				guaranteeHeader.CPH_Type = guaranteeType;
				return guaranteeHeader;
			}
		}
	}
}
