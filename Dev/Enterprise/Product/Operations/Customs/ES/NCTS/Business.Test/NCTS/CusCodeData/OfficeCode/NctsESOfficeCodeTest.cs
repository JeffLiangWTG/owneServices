using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsESOfficeCode))]
	public class NctsESOfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<NctsESOfficeCode>
	{
		public void TestValidation_Departure()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var officeCode = Factory.New<NctsESOfficeCode>();
			officeCode.CY_ParentTableCode = header.TablePrefix;
			officeCode.CY_ParentID = header.PK;

			CombineAssertions(() =>
			{
				AssertType("NCTS4", typeof(NctsEuOfficeCodeValidation), officeCode.Validation);

				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

				officeCode.CY_ParentTableCode = header.MovementHeader.TablePrefix;
				officeCode.CY_ParentID = header.MovementHeader.PK;
				AssertType("NCTS5", typeof(NctsEuOfficeCodePhase5DepartureValidation), officeCode.Validation);
			});
		}

		public void TestValidation_Arrival()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var officeCode = Factory.New<NctsESOfficeCode>();
			officeCode.CY_ParentTableCode = header.TablePrefix;
			officeCode.CY_ParentID = header.PK;

			CombineAssertions(() =>
			{
				AssertType("NCTS4", typeof(NctsEuOfficeCodeValidation), officeCode.Validation);

				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

				officeCode.CY_ParentTableCode = header.ArrivalMovementHeader.TablePrefix;
				officeCode.CY_ParentID = header.ArrivalMovementHeader.PK;
				AssertType("NCTS5", typeof(NctsEuOfficeCodePhase5Validation), officeCode.Validation);
			});
		}

		public void TestCY_Code_ReadOnly()
		{
			CombineAssertions(() =>
			{
				(var departureMovement, var customsOfficeForDeparture) = NctsMovementPhase5ForTest();
				customsOfficeForDeparture.CY_Order = 3;

				customsOfficeForDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfLocation;
				AssertEquals("CY_Order is not 1, BM_CustomStatus is not PRE and CY_Code is not DEP", false, customsOfficeForDeparture.CY_CodeInfo.ReadOnly);

				departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.PreLodged;
				AssertEquals("CY_Order is not 1, BM_CustomStatus is PRE and CY_Code is not DEP", false, customsOfficeForDeparture.CY_CodeInfo.ReadOnly);

				customsOfficeForDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				AssertEquals("CY_Order is not 1, BM_CustomStatus is PRE and CY_Code is DEP", false, customsOfficeForDeparture.CY_CodeInfo.ReadOnly);

				customsOfficeForDeparture.CY_Order = 1;
				AssertEquals("CY_Order is 1, BM_CustomStatus is PRE and CY_Code is DEP", true, customsOfficeForDeparture.CY_CodeInfo.ReadOnly);

				departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
				AssertEquals("BM_CustomStatus is not PRE", false, customsOfficeForDeparture.CY_CodeInfo.ReadOnly);
			});
		}

		public void TestCY_Data_ReadOnly()
		{
			CombineAssertions(() =>
			{
				(var departureMovement, var customsOfficeForDeparture) = NctsMovementPhase5ForTest();
				customsOfficeForDeparture.CY_Order = 3;

				customsOfficeForDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfLocation;
				AssertEquals("CY_Order is not 1, BM_CustomStatus is not PRE and CY_Code is not DEP", false, customsOfficeForDeparture.CY_DataInfo.ReadOnly);

				departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.PreLodged;
				AssertEquals("CY_Order is not 1, BM_CustomStatus is PRE and CY_Code is not DEP", false, customsOfficeForDeparture.CY_DataInfo.ReadOnly);

				customsOfficeForDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				AssertEquals("CY_Order is not 1, BM_CustomStatus is PRE and CY_Code is DEP", false, customsOfficeForDeparture.CY_DataInfo.ReadOnly);

				customsOfficeForDeparture.CY_Order = 1;
				AssertEquals("CY_Order is 1, BM_CustomStatus is PRE and CY_Code is DEP", true, customsOfficeForDeparture.CY_DataInfo.ReadOnly);

				departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
				AssertEquals("BM_CustomStatus is not PRE", false, customsOfficeForDeparture.CY_DataInfo.ReadOnly);
			});
		}

		public void TestCY_Date_ReadOnly()
		{
			CombineAssertions(() =>
			{
				(var departureMovement, var customsOfficeForDeparture) = NctsMovementPhase5ForTest();
				customsOfficeForDeparture.CY_Order = 3;

				customsOfficeForDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfLocation;
				AssertEquals("CY_Order is not 1, BM_CustomStatus is not PRE and CY_Code is not DEP", false, customsOfficeForDeparture.CY_DateInfo.ReadOnly);

				departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.PreLodged;
				AssertEquals("CY_Order is not 1, BM_CustomStatus is PRE and CY_Code is not DEP", false, customsOfficeForDeparture.CY_DateInfo.ReadOnly);

				customsOfficeForDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				AssertEquals("CY_Order is not 1, BM_CustomStatus is PRE and CY_Code is DEP", false, customsOfficeForDeparture.CY_DateInfo.ReadOnly);

				customsOfficeForDeparture.CY_Order = 1;
				AssertEquals("CY_Order is 1, BM_CustomStatus is PRE and CY_Code is DEP", true, customsOfficeForDeparture.CY_DateInfo.ReadOnly);

				departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
				AssertEquals("BM_CustomStatus is not PRE", false, customsOfficeForDeparture.CY_DateInfo.ReadOnly);
			});
		}

		protected override IEnumerable<NctsESOfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var customsOffice = header.MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Data = "D";
			yield return customsOffice;
		}

		protected override BusinessObject GetNewBusinessObject() => departureMovement.CustomsOffices.AddNew();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return header.MovementHeader.CustomsOffices.AddNew();
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new LightValidationTesterNctsESOfficeCode(bizObjToTest);

		(NctsDepartureMovementHeader departureMovement, NctsESOfficeCode customsOfficeForDeparture) NctsMovementPhase5ForTest()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var departureMovement = header.MovementHeader;
			var customsOfficeForDeparture = departureMovement.CustomsOfficesForDeparture.AddNew();
			return (departureMovement, customsOfficeForDeparture);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			departureMovement = header.MovementHeader;
			departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;
		}
		NctsHeader header;
		NctsDepartureMovementHeader departureMovement;
	}

	class LightValidationTesterNctsESOfficeCode : LightValidationTester
	{
		public LightValidationTesterNctsESOfficeCode(BusinessObject bo) : base(bo)
		{
		}

		protected override bool ShouldTestProperty(ZPropertyInfo info)
		{
			// The validation do have a relationship with JobDocAddress, see NctsEuOfficeCodeValidation.CheckCY_Data, here we do reference header.DestinationTrader/SecurityConsignor/Principal...
			// But it can be overweight if we mark NctsEuOfficeCode as needing validation when we set properties in JobDocAddress,
			// So I suppressed the JobDocAddress here.
			if (info.BizObj.GetType() == typeof(JobDocAddress) || info.BizObj.GetType() == typeof(NctsESOfficeCode))
			{
				return false;
			}
			return base.ShouldTestProperty(info);
		}
	}
}
