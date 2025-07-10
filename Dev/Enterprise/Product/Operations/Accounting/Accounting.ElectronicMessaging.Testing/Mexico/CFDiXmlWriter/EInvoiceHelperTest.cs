using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	public class EInvoiceHelperTest : TestCaseWithFactory
	{
		public void TestGetReasonOfCancellation()
		{
			var accTransactionHeaderReference = Factory.New<AccTransactionHeaderReference>();
			var arCreditNote = ObjectCreator.CreateARCreditNote("CRD001", ObjectCreator.AALSHI);
			var pivot = ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, status: EInvoicingPivotState.Sent);
			var batch = ObjectCreator.CreateEInvoicingBatchForPivot(pivot, 10, Constants.EInvoicingBatchState.Ready);

			var eInvoiceHelper = ObjectFactory.Get<IEInvoicingDependencyFactory>().GetMexicoEInvoicingDependencyFactory().GetEInvoiceHelper();

			var result = eInvoiceHelper.GetReasonOfCancellation(batch.PK);
			AssertEquals("There is not AccTransactionHeaderReference", ZString.Empty, result);

			accTransactionHeaderReference.AH1_AH = arCreditNote.PK;
			accTransactionHeaderReference.AH1_Reference = "02";
			accTransactionHeaderReference.AH1_Type = "KKK";
			Factory.Save();

			result = eInvoiceHelper.GetReasonOfCancellation(batch.PK);
			AssertEquals("There is AccTransactionHeaderReference but with wrong AH1_Type", ZString.Empty, result);

			accTransactionHeaderReference.AH1_Type = "MXR";
			Factory.Save();

			result = eInvoiceHelper.GetReasonOfCancellation(batch.PK);
			AssertEquals("There is AccTransactionHeaderReference with correct AH1_Type, reason of cancellation must be the value of AH1_Reference", "02", result);
		}

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
