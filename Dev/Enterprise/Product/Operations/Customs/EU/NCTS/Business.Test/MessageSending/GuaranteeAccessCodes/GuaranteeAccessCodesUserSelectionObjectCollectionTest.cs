using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(GuaranteeAccessCodesUserSelectionObjectCollection))]
	sealed class GuaranteeAccessCodesUserSelectionObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GuaranteeAccessCodesUserSelectionObjectCollection>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new GuaranteeAccessCodesUserSelectionObjectCollection(null));
		}

		public override void TestDelete()
		{
			Assert("Not supporting deleting, this check is not required.", true);
		}

		public override void TestAdd()
		{
			Assert("Not supporting adding, this check is not required.", true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert("Not supporting removing, this check is not required.", true);
		}

		public override void TestTypedget_Item()
		{
			Assert("Will not be used", true);
		}

		public void TestPopulateElements()
		{
			var nctsHeader = GetNewNctsHeader();
			var collection = new GuaranteeAccessCodesUserSelectionObjectCollection(nctsHeader);
			collection.PopulateElements();
			AssertSame(nctsHeader, collection[0].Guarantee.NctsHeader);
		}

		protected override GuaranteeAccessCodesUserSelectionObjectCollection GetCollectionToTest()
			=> new GuaranteeAccessCodesUserSelectionObjectCollection(GetNewNctsHeader());

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<GuaranteeAccessCodesUserSelectionObject>();
		}

		NctsHeader GetNewNctsHeader()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var cusGuarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			cusGuarantee.CPH_StartDate = ZDate.Today.AddMonths(-1);
			cusGuarantee.CPH_EndDate = ZDate.Today.AddMonths(1);
			cusGuarantee.CPH_Number = "GUA1";
			cusGuarantee.CPH_OH_PermitHolder = org1.PK;
			cusGuarantee.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			cusGuarantee.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			cusGuarantee.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			var nctsGuarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
			nctsGuarantee1.PW_BondAmount = 10m;
			nctsGuarantee1.PW_BondNumber = "GUA1";

			return nctsHeader;
		}
	}
}
