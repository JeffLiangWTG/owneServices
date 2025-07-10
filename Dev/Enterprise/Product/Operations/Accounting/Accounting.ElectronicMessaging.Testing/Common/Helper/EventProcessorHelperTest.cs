using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class EventProcessorHelperTest : TestCaseWithFactory
	{
		[TestDate(2023, 8, 10)]
		public void TestGetBatchInfo()
		{
			var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(5), TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(5), TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arInvoice1, Core.Constants.EInvoicingPivotState.Batched);
			var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arInvoice2, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			var company = GlbCompany.CurrentCompany;
			var factoryInstance = Factory._Instance;

			var info = EventProcessorHelper.GetBatchInfo(invoicingBatch);
			var expectedInfo = $@"Batch Info:
	PK = {invoicingBatch.PK}
	Type = AccEInvoicingBatch
	Types around row = AccEInvoicingBatch
	Factory Instance = {invoicingBatch.Factory._Instance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = False
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None

Properties:
 AIB_BatchNumber = 1
 AIB_EHubAllocatedNumber = 
 AIB_GC = {company.PK} ({company.GC_Code})
 AIB_GovernmentAllocatedNumber = 
 AIB_Status = RDY
 AIB_SystemCreateTimeUtc = 10-Aug-23 00:00:00
 AIB_SystemCreateUser = E
 AIB_SystemLastEditTimeUtc = 10-Aug-23 00:00:00
 AIB_SystemLastEditUser = E

Batch has 2 pivot(s):
Pivot #1:
	PK = {pivot1.PK}
	Type = AccEInvoicingTransactionPivot
	Types around row = AccEInvoicingTransactionPivot
	Factory Instance = {factoryInstance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = False
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None

Properties:
 AIP_ActionType = SUB
 AIP_AIB = {invoicingBatch.PK}
 AIP_ErrorDescription = 
 AIP_GC = {company.PK} ({company.GC_Code})
 AIP_IsNotifiedByEmail = N
 AIP_LastResponseReceivedUtc = 
 AIP_LastSentTimeUtc = 
 AIP_ParentID = {arInvoice1.PK}
 AIP_ParentTableCode = AH
 AIP_RN_NKCountryCode = AU
 AIP_Status = BCH
 AIP_SystemCreateTimeUtc = 10-Aug-23 00:00:00
 AIP_SystemCreateUser = E
 AIP_SystemLastEditTimeUtc = 10-Aug-23 00:00:00
 AIP_SystemLastEditUser = E

Pivot Parent transaction Info:
Header: PK = {arInvoice1.PK}, Ledger = AR, Transaction Type = INV, Invoice Date = 10-Aug-23 00:00:00, Post Date = 10-Aug-23 00:00:00, Invoice Amount = 100.00, GST Amount = 10.00, OS Total = 110.00, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 110.00, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 00001000, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = Yes, Is Deleted = No, Has Changes = No, Business Contexts = None, System Create Time = 10-Aug-23 00:00:00, System Create User = E, System Last Edit Time = 10-Aug-23 00:00:00, System Last Edit User = E.

Pivot #2:
	PK = {pivot2.PK}
	Type = AccEInvoicingTransactionPivot
	Types around row = AccEInvoicingTransactionPivot
	Factory Instance = {factoryInstance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = False
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None

Properties:
 AIP_ActionType = SUB
 AIP_AIB = {invoicingBatch.PK}
 AIP_ErrorDescription = 
 AIP_GC = {company.PK} ({company.GC_Code})
 AIP_IsNotifiedByEmail = N
 AIP_LastResponseReceivedUtc = 
 AIP_LastSentTimeUtc = 
 AIP_ParentID = {arInvoice2.PK}
 AIP_ParentTableCode = AH
 AIP_RN_NKCountryCode = AU
 AIP_Status = BCH
 AIP_SystemCreateTimeUtc = 10-Aug-23 00:00:00
 AIP_SystemCreateUser = E
 AIP_SystemLastEditTimeUtc = 10-Aug-23 00:00:00
 AIP_SystemLastEditUser = E

Pivot Parent transaction Info:
Header: PK = {arInvoice2.PK}, Ledger = AR, Transaction Type = INV, Invoice Date = 10-Aug-23 00:00:00, Post Date = 10-Aug-23 00:00:00, Invoice Amount = 100.00, GST Amount = 10.00, OS Total = 110.00, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 110.00, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 00001001, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = Yes, Is Deleted = No, Has Changes = No, Business Contexts = None, System Create Time = 10-Aug-23 00:00:00, System Create User = E, System Last Edit Time = 10-Aug-23 00:00:00, System Last Edit User = E.
";

			AssertMultilineASCIIEquals(expectedInfo, info);
		}

		[TestDate(2023, 8, 10)]
		public void TestGetPivotsInfo()
		{
			var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(5), TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(5), TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arInvoice1, Core.Constants.EInvoicingPivotState.Batched);
			var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arInvoice2, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			var company = GlbCompany.CurrentCompany;
			var factoryInstance = Factory._Instance;

			var info = EventProcessorHelper.GetPivotsInfo(new[] { pivot1, pivot2 });
			var expectedInfo = $@"
Pivot #1:
	PK = {pivot1.PK}
	Type = AccEInvoicingTransactionPivot
	Types around row = AccEInvoicingTransactionPivot
	Factory Instance = {factoryInstance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = False
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None

Properties:
 AIP_ActionType = SUB
 AIP_AIB = {invoicingBatch.PK}
 AIP_ErrorDescription = 
 AIP_GC = {company.PK} ({company.GC_Code})
 AIP_IsNotifiedByEmail = N
 AIP_LastResponseReceivedUtc = 
 AIP_LastSentTimeUtc = 
 AIP_ParentID = {arInvoice1.PK}
 AIP_ParentTableCode = AH
 AIP_RN_NKCountryCode = AU
 AIP_Status = BCH
 AIP_SystemCreateTimeUtc = 10-Aug-23 00:00:00
 AIP_SystemCreateUser = E
 AIP_SystemLastEditTimeUtc = 10-Aug-23 00:00:00
 AIP_SystemLastEditUser = E

Pivot Parent transaction Info:
Header: PK = {arInvoice1.PK}, Ledger = AR, Transaction Type = INV, Invoice Date = 10-Aug-23 00:00:00, Post Date = 10-Aug-23 00:00:00, Invoice Amount = 100.00, GST Amount = 10.00, OS Total = 110.00, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 110.00, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 00001000, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = Yes, Is Deleted = No, Has Changes = No, Business Contexts = None, System Create Time = 10-Aug-23 00:00:00, System Create User = E, System Last Edit Time = 10-Aug-23 00:00:00, System Last Edit User = E.

Pivot #2:
	PK = {pivot2.PK}
	Type = AccEInvoicingTransactionPivot
	Types around row = AccEInvoicingTransactionPivot
	Factory Instance = {factoryInstance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = False
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None

Properties:
 AIP_ActionType = SUB
 AIP_AIB = {invoicingBatch.PK}
 AIP_ErrorDescription = 
 AIP_GC = {company.PK} ({company.GC_Code})
 AIP_IsNotifiedByEmail = N
 AIP_LastResponseReceivedUtc = 
 AIP_LastSentTimeUtc = 
 AIP_ParentID = {arInvoice2.PK}
 AIP_ParentTableCode = AH
 AIP_RN_NKCountryCode = AU
 AIP_Status = BCH
 AIP_SystemCreateTimeUtc = 10-Aug-23 00:00:00
 AIP_SystemCreateUser = E
 AIP_SystemLastEditTimeUtc = 10-Aug-23 00:00:00
 AIP_SystemLastEditUser = E

Pivot Parent transaction Info:
Header: PK = {arInvoice2.PK}, Ledger = AR, Transaction Type = INV, Invoice Date = 10-Aug-23 00:00:00, Post Date = 10-Aug-23 00:00:00, Invoice Amount = 100.00, GST Amount = 10.00, OS Total = 110.00, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 110.00, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 00001001, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = Yes, Is Deleted = No, Has Changes = No, Business Contexts = None, System Create Time = 10-Aug-23 00:00:00, System Create User = E, System Last Edit Time = 10-Aug-23 00:00:00, System Last Edit User = E.
";

			AssertMultilineASCIIEquals(expectedInfo, info);
		}

		[TestDate(2023, 8, 10)]
		public void TestGetPivotsInfo_WhenPivotIsNotLinkedToBatchAndTransaction()
		{
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ActionType = EInvoicingPivotActionType.Submit;
			Factory.Save();

			var company = GlbCompany.CurrentCompany;
			var factoryInstance = Factory._Instance;

			var info = EventProcessorHelper.GetPivotsInfo(new[] { pivot });
			var expectedInfo = $@"
Pivot #1:
	PK = {pivot.PK}
	Type = AccEInvoicingTransactionPivot
	Types around row = AccEInvoicingTransactionPivot
	Factory Instance = {factoryInstance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = False
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None

Properties:
 AIP_ActionType = SUB
 AIP_AIB = {ZGuid.Empty}
 AIP_ErrorDescription = 
 AIP_GC = {company.PK} ({company.GC_Code})
 AIP_IsNotifiedByEmail = N
 AIP_LastResponseReceivedUtc = 
 AIP_LastSentTimeUtc = 
 AIP_ParentID = {ZGuid.Empty}
 AIP_ParentTableCode = AH
 AIP_RN_NKCountryCode = AU
 AIP_Status = QUE
 AIP_SystemCreateTimeUtc = 10-Aug-23 00:00:00
 AIP_SystemCreateUser = E
 AIP_SystemLastEditTimeUtc = 10-Aug-23 00:00:00
 AIP_SystemLastEditUser = E

Pivot Parent transaction Info:

";

			AssertMultilineASCIIEquals(expectedInfo, info);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
