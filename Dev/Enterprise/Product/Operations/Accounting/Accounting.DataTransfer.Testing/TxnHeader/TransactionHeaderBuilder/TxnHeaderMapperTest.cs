using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public class TxnHeaderMapperTest : TestCaseWithFactory
	{
		public void TestGetBizObjTypeForARInvoice()
		{
			AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType.AR, Xsd.TxnType.INV, typeof(ARInvoice));
		}

		public void TestGetBizObjTypeForARCreditNote()
		{
			AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType.AR, Xsd.TxnType.CRD, typeof(ARCreditNote));
		}

		public void TestGetBizObjTypeForARAdjustmentNote()
		{
			AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType.AR, Xsd.TxnType.ADJ, typeof(ARAdjustmentNote));
		}

		public void TestGetBizObjTypeForARJournal()
		{
			AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType.AR, Xsd.TxnType.JNL, typeof(ARJournal));
		}

		public void TestGetBizObjTypeForARPayment()
		{
			AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType.AR, Xsd.TxnType.PAY, typeof(ARPayment));
		}

		public void TestGetBizObjTypeForARReceipt()
		{
			AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType.AR, Xsd.TxnType.REC, typeof(ARReceipt));
		}

		public void TestGetBizObjTypeForAPInvoice()
		{
			AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType.AP, Xsd.TxnType.INV, typeof(APInvoice));
		}

		public void TestGetBizObjTypeForAPCreditNote()
		{
			AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType.AP, Xsd.TxnType.CRD, typeof(APCreditNote));
		}

		public void TestGetBizObjTypeForAPAdjustmentNote()
		{
			AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType.AP, Xsd.TxnType.ADJ, typeof(APAdjustmentNote));
		}

		public void TestGetBizObjTypeForAPJournal()
		{
			AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType.AP, Xsd.TxnType.JNL, typeof(APJournal));
		}

		public void TestGetBizObjTypeForAPPayment()
		{
			AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType.AP, Xsd.TxnType.PAY, typeof(APPayment));
		}

		public void TestGetBizObjTypeForAPReceipt()
		{
			AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType.AP, Xsd.TxnType.REC, typeof(APReceipt));
		}

		public void TestGetBizObjTypeForInvalidType()
		{
			AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType.AP, Xsd.TxnType.TRF, null);
			AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType.AP, Xsd.TxnType.DPY, null);
		}

		public void TestGetBizObjTypeForCashbookDirectPayment()
		{
			AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType.CB, Xsd.TxnType.DPY, typeof(DirectPayment));
		}

		public void TestGetBizObjTypeForCashbookDirectReceipt()
		{
			AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType.CB, Xsd.TxnType.DRC, typeof(DirectReceipt));
		}

		void AssertCorrectTypeIsReturnedForLedgerAndTransactionType(Xsd.TxnLedgerType ledger,
			Xsd.TxnType transactionType, Type expectedType)
		{
			Xsd.TxnHeader xmlTxnHeader = new Xsd.TxnHeader();
			xmlTxnHeader.Ledger = ledger;
			xmlTxnHeader.TxnType = transactionType;
			var message = "Transaction Type for Ledger (" + ledger + "), TransactionType(" + transactionType + ").";
			AssertEquals(message, expectedType, TxnHeaderMapper.GetBizObjTypeFromIValueObject(xmlTxnHeader));
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestGetTxnHeaderLedgerForInvalidLedger()
		{
			TxnHeaderMapper.GetTxnHeaderLedger("BLA");
		}

		public void TestGetTxnHeaderLedgerForAR()
		{
			AssertEquals("Accounts Receivable Ledger Code", nameof(Xsd.TxnLedgerType.AR), TxnHeaderMapper.GetTxnHeaderLedger(ZArchitecture.Core.LedgerTypes.AccountsReceivable).ToString());
		}

		public void TestGetTxnHeaderLedgerForAP()
		{
			AssertEquals("Accounts Payable Ledger Code", nameof(Xsd.TxnLedgerType.AP), TxnHeaderMapper.GetTxnHeaderLedger(ZArchitecture.Core.LedgerTypes.AccountsPayable).ToString());
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestGetTxnHeaderTxnType_InvalidType()
		{
			TxnHeaderMapper.GetTxnHeaderTxnType("BLA");
		}

		public void TestGetTxnHeaderTxnType()
		{
			AssertEquals("Transaction Type", nameof(Xsd.TxnType.INV), TxnHeaderMapper.GetTxnHeaderTxnType(ZArchitecture.Core.TransactionTypes.Invoice).ToString());
			AssertEquals("Transaction Type", nameof(Xsd.TxnType.CRD), TxnHeaderMapper.GetTxnHeaderTxnType(ZArchitecture.Core.TransactionTypes.CreditNote).ToString());
			AssertEquals("Transaction Type", nameof(Xsd.TxnType.ADJ), TxnHeaderMapper.GetTxnHeaderTxnType(ZArchitecture.Core.TransactionTypes.AdjustmentNote).ToString());
			AssertEquals("Transaction Type", nameof(Xsd.TxnType.CTR), TxnHeaderMapper.GetTxnHeaderTxnType(ZArchitecture.Core.TransactionTypes.Contra).ToString());
			AssertEquals("Transaction Type", nameof(Xsd.TxnType.JNL), TxnHeaderMapper.GetTxnHeaderTxnType(ZArchitecture.Core.TransactionTypes.Journal).ToString());
			AssertEquals("Transaction Type", nameof(Xsd.TxnType.OPY), TxnHeaderMapper.GetTxnHeaderTxnType(ZArchitecture.Core.TransactionTypes.OpeningPayment).ToString());
			AssertEquals("Transaction Type", nameof(Xsd.TxnType.ORC), TxnHeaderMapper.GetTxnHeaderTxnType(ZArchitecture.Core.TransactionTypes.OpeningReceipt).ToString());
			AssertEquals("Transaction Type", nameof(Xsd.TxnType.PAY), TxnHeaderMapper.GetTxnHeaderTxnType(ZArchitecture.Core.TransactionTypes.Payment).ToString());
			AssertEquals("Transaction Type", nameof(Xsd.TxnType.REC), TxnHeaderMapper.GetTxnHeaderTxnType(ZArchitecture.Core.TransactionTypes.Receipt).ToString());
			AssertEquals("Transaction Type", nameof(Xsd.TxnType.TRF), TxnHeaderMapper.GetTxnHeaderTxnType(ZArchitecture.Core.TransactionTypes.Transfer).ToString());
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestGetTxnLineConsolOrJobType_InvalidType()
		{
			TxnHeaderMapper.GetTxnLineConsolOrJobType("BLA", new NotificationBuffer());
		}

		public void TestGetTxnLineConsolOrJobType()
		{
			JobInvoicingConsumerTypes consumerTypes = JobInvoicingConsumerTypes.New();

			Assert("Precondition: Consumer Types List should never be empty", consumerTypes.Count > 0);
			ZString codesNotMapped = ZString.Empty;

			foreach (JobInvoicingConsumerType consumerType in consumerTypes)
			{
				try
				{
					TxnHeaderMapper.GetTxnLineConsolOrJobType(consumerType.Code, new NotificationBuffer());
				}
				catch (ArgumentException)
				{
					codesNotMapped += "      [" + consumerType.Code + " - " + consumerType.Description + "]" + System.Environment.NewLine;
				}
			}

			ZString message = System.Environment.NewLine + System.Environment.NewLine;
			message += "The following JobInvoicingConsumerTypes are not mapped in the GetTxnLineConsolOrJobType() method. " + System.Environment.NewLine;
			message += System.Environment.NewLine + codesNotMapped + System.Environment.NewLine;
			message += "If you can create transactions (INV, CRD) for these consumer types, you should do the following to make sure the Accounting Export still works: " + System.Environment.NewLine;
			message += "   1. Add a new enumeration to TxnLineConsolOrJobType in the TxnLine XSD. " + System.Environment.NewLine;
			message += "   2. Add a new line to the class TransactionLineConsolOrJobTypeXmlMapping (which is in the file TransactionHeaderConsolOrJobTypeXmlMapping.cs - cos I'm a very nice man who codes very goodly. Imraan Khan) for your new Consumer Type." + System.Environment.NewLine;
			message += "   3. Add an Assert statement at the end of this test for the line you added in point 2." + System.Environment.NewLine;
			message += "   4. Check that your operations job type is included in database view 'ViewGenericJob'. " + System.Environment.NewLine;

			Assert(message, codesNotMapped.IsEmpty);

			NotificationBuffer buffer = new NotificationBuffer();

			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.ACR), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.CusMAWB.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.AGB), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.AgencyBooking.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.AGS), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.ACD), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.AgencyDetentionInvoice.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.AVA), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.AgencyVoyageAccounting.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.ASC), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.AgencySundryCharges.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.AWB), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.MasterAWB.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.BRK), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.Brokerage.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.CLL), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.CFSLoadList.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.CSH), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.CFSShipment.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.CSL), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.Consol.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.CTO), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.CTOCusMAWB.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.AHW), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.CTOCusImportHAWB.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.AHE), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.CTOCusExportHAWB.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.SHP), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.Shipment.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.QSH), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.QuotedBooking.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.TRN), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.LocalCartage.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.ABK), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.AgentBooking.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.TBM), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.TransportBooking.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.ATB), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.TransportBookingWithAgent.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.UBR), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.CusUnderbond.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.WIN), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.WarehouseInwards.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.WOU), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.WarehouseOutwards.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.WSJ), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.WarehouseAdHocServiceJob.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.WST), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.WarehouseStorage.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.CST), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.FCLStorage.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.WSC), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.WarehouseStocktake.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.WVO), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.WarehouseVASOrder.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.MAN), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.eManifest.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.TCW), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.TransportBookingConsignment.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.LTC), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.TransportConsignment.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.CAE), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.CAeManifest.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.WKI), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.WorkItem.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.WKP), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.Project.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.WKR), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.WorkRequest.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.TRC), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.TransitReceive.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.TDC), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.TransitDispatch.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.TDL), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.TransitDispatchLoadList.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.TRU), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.TransitReceiveTransportationUnit.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.TDU), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.TransitDispatchTransportationUnit.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.YRA), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.CYDReceiveAdvice.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.YRE), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.CYDReleaseAdvice.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.YTU), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.CYDTransportationUnit.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.YAO), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.CYDAdHocServiceOrder.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.MWO), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.MNRWorkOrderHeader.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.NCT), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.LPC), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.BRLPCO.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.STO), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.CustomsTemporaryStorage.Code, buffer).ToString());
			AssertEquals(nameof(Xsd.TxnLineConsolOrJobType.YPI), TxnHeaderMapper.GetTxnLineConsolOrJobType(JobInvoicingConsumerTypes.CYDPeriodicInvoicing.Code, buffer).ToString());
		}

		public void TestGetCodeForJobInvoicingConsumerType()
		{
			NotificationBuffer buffer = new NotificationBuffer();

			AssertEquals(JobInvoicingConsumerTypes.CusMAWB.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.ACR, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.AgencyBooking.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.AGB, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.AGS, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.MasterAWB.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.AWB, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.Brokerage.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.BRK, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.CFSLoadList.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.CLL, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.CFSShipment.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.CSH, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.Consol.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.CSL, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.CTOCusMAWB.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.CTO, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.Shipment.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.SHP, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.QuotedBooking.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.QSH, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.LocalCartage.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.TRN, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.AgentBooking.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.ABK, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.TransportBooking.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.TBM, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.CusUnderbond.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.UBR, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.WarehouseInwards.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.WIN, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.WarehouseOutwards.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.WOU, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.WarehouseStorage.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.WST, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.WarehouseAdHocServiceJob.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.WSJ, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.WarehouseVASOrder.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.WVO, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.FCLStorage.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.CST, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.WorkItem.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.WKI, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.Project.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.WKP, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.WorkRequest.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.WKR, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.NCT, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.BRLPCO.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.LPC, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.CustomsTemporaryStorage.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.STO, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.MNRWorkOrderHeader.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.MWO, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.CYDAdHocServiceOrder.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.YAO, buffer).ToString());
			AssertEquals(JobInvoicingConsumerTypes.CYDPeriodicInvoicing.Code, TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType.YPI, buffer).ToString());
		}

		public void TestGetDecimalFromXmlFinancialValueDoesNotRoundWhenNoCurrency()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZDecimal testValue = new ZDecimal(1.2345);
			Xsd.FinancialValue testFinancialValue = Xsd.FinancialValue.FromAmountAndCurrencyCode(testValue, "");

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
			ZDecimal result = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(factory, testFinancialValue, typeof(ARInvoice));
			AssertEquals("Should round to local currency when no currency on value", 1.23m, result);
			result = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(factory, testFinancialValue, typeof(ARInvoice), false);
			AssertEquals("Should not round to local currency when no currency on value if RoundToCurrency is false", 1.2345m, result);

			testFinancialValue.CurrencyCode = "JPY";
			result = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(factory, testFinancialValue, typeof(ARInvoice));
			AssertEquals("Should round to set currency when no currency on value", 1m, result);
			result = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(factory, testFinancialValue, typeof(ARInvoice), false);
			AssertEquals("Should not round to set currency when no currency on value if RoundToCurrency is false", 1.2345m, result);
		}

		public void TestGetDecimalFromXmlFinancialValueOverrideCurrentCompanysDecimalPlaceIfFinancialValueHasOne()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZDecimal testValue = new ZDecimal(1.248);
			Xsd.FinancialValue testFinancialValue = Xsd.FinancialValue.FromAmountAndCurrencyCode(testValue, "");

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";

			ZDecimal result = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(factory, testFinancialValue, typeof(ARInvoice));
			AssertEquals("1.25", result.ToString());
			result = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(factory, testFinancialValue);
			AssertEquals("1.25", result.ToString());

			testFinancialValue = Xsd.FinancialValue.FromAmountAndCurrencyCode(testValue, "KWD");
			result = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(factory, testFinancialValue, typeof(ARInvoice));
			AssertEquals("1.248", result.ToString());
			result = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(factory, testFinancialValue);
			AssertEquals("1.248", result.ToString());
		}

		public void TestSwapXmlFinancialValueSign()
		{
			ZDecimal testValue = new ZDecimal(1.243);
			Xsd.FinancialValue testFinancialValue = TxnHeaderMapper.GetXmlFinancialValue(testValue, GlbCompany.CurrentCompany.LocalCurrency, typeof(ARInvoice));

			Xsd.FinancialValue result = TxnHeaderMapper.SwapXmlFinancialValueSign(testFinancialValue);

			ZDecimal originalFinancialAmount = testFinancialValue.Value;
			ZDecimal swappedFinancialAmount = result.Value;

			AssertEquals(new ZDecimal(-1) * originalFinancialAmount, swappedFinancialAmount);
			AssertEquals(testFinancialValue.CurrencyCode, result.CurrencyCode);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestGetTxnLineLineType_Invalid()
		{
			TxnHeaderMapper.GetTxnLineLineType("BLA");
		}

		public void TestGetTxnLineLineType()
		{
			AssertEquals(Xsd.TxnLineLineType.CST, TxnHeaderMapper.GetTxnLineLineType(ZArchitecture.Core.TransactionLineTypes.Cost));
			AssertEquals(Xsd.TxnLineLineType.REV, TxnHeaderMapper.GetTxnLineLineType(ZArchitecture.Core.TransactionLineTypes.Revenue));
		}

		public void TestMultiplierForImportAndExport()
		{
			AssertEquals("Type: ARInvoice", 1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(ARInvoice)));
			AssertEquals("Type: ARInvoiceLine", 1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(ARInvoice)));

			AssertEquals("Type: ARCreditNote", -1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(ARCreditNote)));
			AssertEquals("Type: ARCreditNoteLine", -1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(ARCreditNoteLine)));

			AssertEquals("Type: ARAdjustmentNote", 1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(ARAdjustmentNote)));
			AssertEquals("Type: ARAdjustmentNoteLine", 1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(ARAdjustmentNoteLine)));

			AssertEquals("Type: APInvoice", -1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(APInvoice)));
			AssertEquals("Type: APInvoiceLine", -1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(APInvoiceLine)));

			AssertEquals("Type: APCreditNote", 1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(APCreditNote)));
			AssertEquals("Type: APCreditNoteLine", 1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(APCreditNoteLine)));

			AssertEquals("Type: APAdjustmentNote", -1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(APAdjustmentNote)));
			AssertEquals("Type: APAdjustmentNoteLine", -1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(APAdjustmentNoteLine)));

			AssertEquals("Type: ARJournal", 1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(ARJournal)));
			AssertEquals("Type: ARTransferToRow", 1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(ARTransferToRow)));
			AssertEquals("Type: ARTransferFromRow", -1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(ARTransferFromRow)));
			AssertEquals("Type: ARContraRow", -1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(ARContraRow)));
			AssertEquals("Type: ARReceipt", -1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(ARReceipt)));
			AssertEquals("Type: ARPayment", 1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(ARPayment)));

			AssertEquals("Type: APJournal", 1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(APJournal)));
			AssertEquals("Type: APTransferToRow", -1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(APTransferToRow)));
			AssertEquals("Type: APTransferFromRow", 1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(APTransferFromRow)));
			AssertEquals("Type: APContraRow", 1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(APContraRow)));
			AssertEquals("Type: APReceipt", -1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(APReceipt)));
			AssertEquals("Type: APPayment", 1M, TxnHeaderMapper.MultiplierForImportAndExport(typeof(APPayment)));
		}

		public void TestCrossLedgerGetBizObjTypeFromIValueObject()
		{
			AssertCorrectTypeIsReturnedForCrossLedger(Xsd.TxnLedgerType.AR, Xsd.TxnType.INV, typeof(ARInvoice), false);
			AssertCorrectTypeIsReturnedForCrossLedger(Xsd.TxnLedgerType.AR, Xsd.TxnType.CRD, typeof(ARCreditNote), false);
			AssertCorrectTypeIsReturnedForCrossLedger(Xsd.TxnLedgerType.AR, Xsd.TxnType.ADJ, typeof(ARAdjustmentNote), false);
			AssertCorrectTypeIsReturnedForCrossLedger(Xsd.TxnLedgerType.AP, Xsd.TxnType.INV, typeof(APInvoice), false);
			AssertCorrectTypeIsReturnedForCrossLedger(Xsd.TxnLedgerType.AP, Xsd.TxnType.CRD, typeof(APCreditNote), false);
			AssertCorrectTypeIsReturnedForCrossLedger(Xsd.TxnLedgerType.AP, Xsd.TxnType.ADJ, typeof(APAdjustmentNote), false);

			AssertCorrectTypeIsReturnedForCrossLedger(Xsd.TxnLedgerType.AR, Xsd.TxnType.INV, typeof(APInvoice), true);
			AssertCorrectTypeIsReturnedForCrossLedger(Xsd.TxnLedgerType.AR, Xsd.TxnType.CRD, typeof(APCreditNote), true);
			AssertCorrectTypeIsReturnedForCrossLedger(Xsd.TxnLedgerType.AR, Xsd.TxnType.ADJ, typeof(APAdjustmentNote), true);
			AssertCorrectTypeIsReturnedForCrossLedger(Xsd.TxnLedgerType.AP, Xsd.TxnType.INV, typeof(APInvoice), true);
			AssertCorrectTypeIsReturnedForCrossLedger(Xsd.TxnLedgerType.AP, Xsd.TxnType.CRD, typeof(APCreditNote), true);
			AssertCorrectTypeIsReturnedForCrossLedger(Xsd.TxnLedgerType.AP, Xsd.TxnType.ADJ, typeof(APAdjustmentNote), true);
		}

		void AssertCorrectTypeIsReturnedForCrossLedger(Xsd.TxnLedgerType ledger, Xsd.TxnType transactionType, Type expectedType, bool crossLedger)
		{
			Xsd.TxnHeader xmlTxnHeader = new Xsd.TxnHeader();
			xmlTxnHeader.Ledger = ledger;
			xmlTxnHeader.TxnType = transactionType;
			var message = "Transaction Type for Ledger (" + ledger + "), TransactionType(" + transactionType + ").";
			AssertEquals(message, expectedType, TxnHeaderMapper.GetBizObjTypeFromIValueObject(xmlTxnHeader, crossLedger));
		}

		public void TestGetXmlFinancialValueWith2Args()
		{
			ZDecimal testValue = new ZDecimal(1.2435);
			var testFinancial1 = TxnHeaderMapper.GetXmlFinancialValue(testValue, null);
			AssertEquals(new ZDecimal(1.244), testFinancial1.Value);
			var testFinancial2 = TxnHeaderMapper.GetXmlFinancialValue(testValue, RefCurrency.New(Factory));
			AssertEquals(new ZDecimal(1.24), testFinancial2.Value);
		}

		public void TestGetXmlFinancialValueWith3Args()
		{
			ZDecimal testValue = new ZDecimal(1.2435);
			var testFinancial1 = TxnHeaderMapper.GetXmlFinancialValue(testValue, null, typeof(ARInvoice));
			AssertEquals(new ZDecimal(1.244), testFinancial1.Value);
			var testFinancial2 = TxnHeaderMapper.GetXmlFinancialValue(testValue, RefCurrency.New(Factory), typeof(ARInvoice));
			AssertEquals(new ZDecimal(1.24), testFinancial2.Value);
		}
	}
}
