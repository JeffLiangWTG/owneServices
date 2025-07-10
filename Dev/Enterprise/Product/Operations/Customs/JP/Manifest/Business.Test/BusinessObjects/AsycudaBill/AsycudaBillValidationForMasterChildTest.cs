using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.JP.Manifest.Business.Test;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillValidationForMasterChild))]
	sealed class AsycudaBillValidationForMasterChildTest : AsycudaBillValidationTest
	{
		public void TestCheckABL_GoodsLocation()
		{
			var code = TestDataHelper.CreateJapanBondedAreaCode(Factory);

			Bill.ABL_GoodsLocation = "QQ";
			AssertHasMessageError(Bill.ABL_GoodsLocationInfo, ListValidation.InvalidCodeMessageError);

			Bill.ABL_GoodsLocation = code;
			AssertNoMessageError(Bill.ABL_GoodsLocationInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckABL_GoodsLocation_IsSendNVC01BondedLocationAmendment()
		{
			var info = Bill.ABL_GoodsLocationInfo;
			Bill.Validation.ValidateABL_GoodsLocation();
			AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

			var context = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.NVC01, Action = JPMessageActionList.Codes.Five };
			using (Header.SetCurrentMessageSendingContext(context))
			{
				Bill.Validation.ValidateABL_GoodsLocation();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckABL_E_DEP()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Bill.ABL_E_DEPInfo);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			Bill.ABL_E_DEP = ZDateTime.Empty;
			Bill.Validation.ValidateABL_A_DEP();
			AssertNoMessageErrorContaining(Bill.ABL_E_DEPInfo, MandatoryValidation.YouHaveNotEntered);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			Bill.Validation.ValidateABL_A_DEP();
			AssertNoMessageErrorContaining(Bill.ABL_E_DEPInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override AsycudaBill GetAsycudaBillForTest() => Header.MasterBill;
	}
}
