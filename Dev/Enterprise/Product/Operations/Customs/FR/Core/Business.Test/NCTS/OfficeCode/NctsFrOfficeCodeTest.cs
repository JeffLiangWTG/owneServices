using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NctsFrOfficeCode))]
	sealed class NctsFrOfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<NctsFrOfficeCode>
	{
		public void TestNoNullObjectReferenceIssueWhenClone()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var customsOffice = header.CustomsOffices.AddNew();
			customsOffice.CY_Code = "D";

			CombineAssertions(() =>
			{
				customsOffice.CY_ParentID = ZGuid.Empty;
				AssertNoExceptionThrown("No exception when no NctsHeader linked", () => customsOffice.Clone());

				var movementHeader = header.MovementHeader;
				movementHeader.ChargePaymentOrDestinationID = "123";
				customsOffice.CY_ParentID = header.PK;
				var cloneOffice = customsOffice.Clone();
				var cloneHeader = ((NctsFrOfficeCode)cloneOffice).Header;
				AssertEquals("No exception and Not default ChargePaymentOrDestinationID but still original value", "123", cloneHeader.MovementHeader.ChargePaymentOrDestinationID);
			});
		}

		protected override IEnumerable<NctsFrOfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var customsOffice = header.MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Data = "D";
			yield return customsOffice;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return header.MovementHeader.CustomsOffices.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return header.MovementHeader.CustomsOffices.AddNew();
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new LightValidationTesterNctsFrOfficeCode(bizObjToTest);
	}

	class LightValidationTesterNctsFrOfficeCode : LightValidationTester
	{
		public LightValidationTesterNctsFrOfficeCode(BusinessObject bo) : base(bo)
		{
		}

		/// <summary>
		/// The validation do have a relationship with JobDocAddress, see NctsEuOfficeCodeValidation.CheckCY_Data, here we do reference header.DestinationTrader/SecurityConsignor/Principal...
		/// But it can be overweight if we mark NctsEuOfficeCode as needing validation when we set properties in JobDocAddress,
		/// So I suppressed the JobDocAddress here.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		protected override bool ShouldTestProperty(ZPropertyInfo info) => !(info.BizObj.GetType() == typeof(Enterprise.MasterFiles.Business.JobDocAddress) || info.BizObj.GetType() == typeof(NctsFrOfficeCode)) && base.ShouldTestProperty(info);
	}
}
