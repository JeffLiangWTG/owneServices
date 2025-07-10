using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CADPreSaveDialogStrategyTest : TestCaseWithFactory
	{
		public void TestShouldRunPreSaveAction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_GoodsShipmentSequence = 1;
			entryLine.CL_CommoditySequence = 1;
			entryLine.ConfirmedFees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 1m);
			entryLine.ConfirmedFees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 2m);

			var tester = new CADPreSaveDialogStrategy(declaration);
			tester.ShowPreSaveDialogs(ContinueWithSave.Yes);
			AssertEquals("DialogShown", typeof(CADCorrectionMessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestShouldRunPreSaveActionForLVSDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_GoodsShipmentSequence = 1;
			entryLine.CL_CommoditySequence = 1;
			entryLine.ConfirmedFees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 1m);
			entryLine.ConfirmedFees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 2m);

			var tester = new CADPreSaveDialogStrategy(declaration);
			tester.ShowPreSaveDialogs(ContinueWithSave.Yes);
			AssertEquals("DialogShown", typeof(CADCorrectionMessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}
	}
}
