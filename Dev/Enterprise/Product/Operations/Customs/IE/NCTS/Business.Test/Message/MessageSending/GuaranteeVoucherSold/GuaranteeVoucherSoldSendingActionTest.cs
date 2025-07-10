using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(GuaranteeVoucherSoldSendingAction))]
	sealed class GuaranteeVoucherSoldSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHeader() => AssertType<CusGuaranteeHeader>(GetNewGuaranteeVoucherSoldSendingAction().Header);

		public void TestSenderType()
		{
			var sendingObj = GetNewGuaranteeVoucherSoldSendingAction();
			AssertType("Should have defined a correct SenderType.", typeof(GuaranteeMessageSender), sendingObj.CreateSender());
		}

		public void TestLookups()
		{
			var sendingObj = GetNewGuaranteeVoucherSoldSendingAction();
			AssertType("Should have created a correct Lookups.", typeof(GuaranteeVoucherSoldSendingActionLookups), sendingObj.Lookups);
		}

		public void TestValidationType()
		{
			var sendingObj = GetNewGuaranteeVoucherSoldSendingAction();
			AssertType("Should have created a correct Validation.", typeof(GuaranteeVoucherSoldSendingActionValidation), sendingObj.Validation);
		}

		public void TestHolderOfTransitProcedure_Caption()
		{
			var sendingObj = GetNewGuaranteeVoucherSoldSendingAction();
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObj.HolderOfTransitProcedureInfo);
			AssertEquals("HolderOfTransitProcedureInfo caption", "Transit Holder", resData.Caption);
		}

		public void TestTIRCarnet_Caption()
		{
			var sendingObj = GetNewGuaranteeVoucherSoldSendingAction();
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObj.TIRCarnetInfo);
			AssertEquals("TIRCarnetInfo caption", "TIR Carnet", resData.Caption);
		}

		public void TestCustomsOfficeOfGuarantee_Caption()
		{
			var sendingObj = GetNewGuaranteeVoucherSoldSendingAction();
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObj.CustomsOfficeOfGuaranteeInfo);
			AssertEquals("CustomsOfficeOfGuaranteeInfo caption", "Guarantee Office", resData.Caption);
		}

		public void TestVoucherAmount_Caption()
		{
			var sendingObj = GetNewGuaranteeVoucherSoldSendingAction();
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObj.VoucherAmountInfo);
			AssertEquals("VoucherAmountInfo caption", "Voucher Amount", resData.Caption);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusGuaranteeHeader>();
			return new GuaranteeVoucherSoldSendingAction(header);
		}

		GuaranteeVoucherSoldSendingAction GetNewGuaranteeVoucherSoldSendingAction() => (GuaranteeVoucherSoldSendingAction)GetNewBusinessObject();
	}
}
