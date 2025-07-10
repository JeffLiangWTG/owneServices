using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class DuimpRegisterProviderTest : TestCaseWithFactory
	{
		public void TestTotalItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MergedLines.AddNew();
			entryHeader.MergedLines.AddNew();

			var sendingObject = new DuimpMessageSendingObject(entryHeader);
			sendingObject.MessageType = ImportEntryActionCodeList.Codes.REG;

			var dataProvider = new DuimpRegisterProvider(sendingObject);
			AssertEquals(2, dataProvider.TotalItem);
		}

		public void TestPayment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader1.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate(ChargeTypesList.Codes.DTY, 10.1m);
			entryLine1.Fees.AddOrUpdate(Constants.RateTypes.PIS, 20.1m);

			var sendingObject = new DuimpMessageSendingObject(entryHeader1);
			var dataProvider = new DuimpRegisterProvider(sendingObject);
			dataProvider = new DuimpRegisterProvider(sendingObject);
			AssertEquals(2, dataProvider.Payments.Count());

			var entryLine2 = entryHeader1.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(Constants.RateTypes.Antidumping, 1.6m);
			dataProvider = new DuimpRegisterProvider(sendingObject);
			AssertEquals(3, dataProvider.Payments.Count());

			var entryLine3 = entryHeader1.MergedLines.AddNew();
			entryLine3.Fees.AddOrUpdate(Constants.RateTypes.Antidumping, 1.6m);
			dataProvider = new DuimpRegisterProvider(sendingObject);
			AssertEquals(3, dataProvider.Payments.Count());

			var entryLine4 = entryHeader1.MergedLines.AddNew();
			entryLine4.Fees.AddOrUpdate("XXX", 4.1m);
			dataProvider = new DuimpRegisterProvider(sendingObject);
			AssertEquals(3, dataProvider.Payments.Count());

			entryLine4.Fees.AddOrUpdate(Constants.RateTypes.Cofins, 0m);
			dataProvider = new DuimpRegisterProvider(sendingObject);
			AssertEquals(3, dataProvider.Payments.Count());

			entryLine4.Fees.AddOrUpdate(Constants.RateTypes.Cofins, 8.1m);
			dataProvider = new DuimpRegisterProvider(sendingObject);
			AssertEquals(4, dataProvider.Payments.Count());

			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			var entryLine5 = entryHeader2.MergedLines.AddNew();
			entryLine5.Fees.AddOrUpdate(Constants.RateTypes.Cofins, 4.1m);
			dataProvider = new DuimpRegisterProvider(sendingObject);
			AssertEquals(4, dataProvider.Payments.Count());
		}
	}
}
