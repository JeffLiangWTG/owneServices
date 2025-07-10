using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CADCorrectionMessageSendingActionCollection))]
	sealed class CADCorrectionMessageSendingActionCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<CADCorrectionMessageSendingAction>
	{
		protected override CusSupportingInfoCollection<CADCorrectionMessageSendingAction> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.CA.CAJobMessageTypeList.Codes.Import;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_CommoditySequence = 3;
			entryLine.CL_GoodsShipmentSequence = 4;
			var wrapper = new CADCorrectionMessageSendingActionWrapper(cadEntry);
			return new CADCorrectionMessageSendingActionCollection(wrapper);
		}

		public void TestAllowNewAndRemove()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.CA.CAJobMessageTypeList.Codes.Import;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var cusEntryLine = cadEntry.AllEntryLines.AddNew();
			var wrapper = new CADCorrectionMessageSendingActionWrapper(cadEntry);
			Assert("AllowNewCore is true", wrapper.SendingActions.AllowNew);
			Assert("AllowRemoveCore is true", wrapper.SendingActions.AllowRemove);

			Assert("AllowNewCore is false", !cusEntryLine.AmendmentDetails.AllowNew);
			Assert("AllowRemoveCore is false", !cusEntryLine.AmendmentDetails.AllowRemove);
		}

		public void TestSetDefaultsForNewChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.CA.CAJobMessageTypeList.Codes.Import;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			var wrapper = new CADCorrectionMessageSendingActionWrapper(cadEntry);
			var action = wrapper.SendingActions.AddNew();
			action.CSI_ParentID = entryLine.PK;
			action.CSI_ParentTableCode = CusEntryLineSchema.Constants.Prefix;
			entryLine.RefreshAmendmentDetails();
			AssertEquals("CSI_LineNo is 1", 1, action.CSI_LineNo);
			action = wrapper.SendingActions.AddNew();
			AssertEquals("CSI_LineNo is 2", 2, action.CSI_LineNo);
		}
	}
}
