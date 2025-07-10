using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5FEAmendmentDetailsManagerTest : AmendmentDetailsManagerTest
	{
		[TestDate(2024, 5, 1)]
		public override void TestAmendmentType()
		{
			var entry = new TestDataSetupHelper(Factory).Create929SnapShot(BondedFactoryUseCodeList.Codes.A);

			var sendingObj = new GOVCBR5FEAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(DutyTaxCorrectionCodeList.Codes.X + DeclarationCorrectionCodeList.Codes.X, sendingObj.AmendmentType);

			entry.Declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._01;

			sendingObj = new GOVCBR5FEAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(DutyTaxCorrectionCodeList.Codes.X + DeclarationCorrectionCodeList.Codes.D, sendingObj.AmendmentType);

			entry.Declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._33;
			entry.Declaration.JE_TradeIndicatorWithKP = SouthNorthTradeYNCodeList.Codes.A;

			sendingObj = new GOVCBR5FEAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(DutyTaxCorrectionCodeList.Codes.O + DeclarationCorrectionCodeList.Codes.X, sendingObj.AmendmentType);

			entry.Declaration.UnderbondMovementArrivalDate = new ZDateTime("2024-02-01 12:30:00");

			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_ProcessDate = new ZDateTime(2024, 2, 1);
			statement1.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement1.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			var line = statement1.StatementLines.AddNew();
			line.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			Factory.Save();

			sendingObj = new GOVCBR5FEAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(DutyTaxCorrectionCodeList.Codes.O + DeclarationCorrectionCodeList.Codes.D, sendingObj.AmendmentType);

			var vat = entry.Charges[0];
			vat.C1_ChargeAmount = vat.C1_ChargeAmount + 1m;
			sendingObj = new GOVCBR5FEAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(DutyTaxCorrectionCodeList.Codes.O + DeclarationCorrectionCodeList.Codes.D, sendingObj.AmendmentType);

			line.B3_EntryNum = "1234520000045M";
			statement1.B2_PaymentAuthorizationDate = ZDateTime.Today.AddMonths(-5);
			Factory.Save();
			entry.Reload();
			sendingObj = new GOVCBR5FEAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(DutyTaxCorrectionCodeList.Codes.A + DeclarationCorrectionCodeList.Codes.D, sendingObj.AmendmentType);

			statement1.B2_PaymentAuthorizationDate = ZDateTime.Today.AddMonths(-6);
			Factory.Save();
			entry.Reload();
			sendingObj = new GOVCBR5FEAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(DutyTaxCorrectionCodeList.Codes.A + DeclarationCorrectionCodeList.Codes.D, sendingObj.AmendmentType);

			statement1.B2_PaymentAuthorizationDate = ZDateTime.Today.AddMonths(-7);
			Factory.Save();
			entry.Reload();
			sendingObj = new GOVCBR5FEAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(DutyTaxCorrectionCodeList.Codes.B + DeclarationCorrectionCodeList.Codes.D, sendingObj.AmendmentType);

			vat.C1_ChargeAmount = vat.C1_ChargeAmount - 2m;
			sendingObj = new GOVCBR5FEAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(DutyTaxCorrectionCodeList.Codes.C + DeclarationCorrectionCodeList.Codes.D, sendingObj.AmendmentType);
		}

		public void TestPaymentDateFromStatement()
		{
			var entry = new TestDataSetupHelper(Factory).Create929SnapShot(ZString.Empty);
			var sendingObj = new GOVCBR5FEAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5FE);

			AssertEquals(ZDateTime.Empty, sendingObj.PaymentDate);

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			statement.B2_PaymentAuthorizationDate = new ZDateTime(2024, 02, 20);
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1234520000045M";
			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementType = StatementHeaderTypeList.Codes.Normal;
			statement2.B2_PaymentAuthorizationDate = new ZDateTime(2024, 02, 27);
			var statementLine2 = statement2.StatementLines.AddNew();
			statementLine2.B3_EntryNum = "1234520000045M";
			statementLine2.B3_EntryType = SharedJobMessageTypeList.Codes.Import;

			Factory.Save();

			sendingObj = new GOVCBR5FEAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5FE);

			AssertEquals(new ZDateTime(2024, 02, 20), sendingObj.PaymentDate);
		}

		public override void TestAmendedItems()
		{
			TestDataTypeIsDate(new TestDataSetupHelper(Factory).Create929SnapShot(BondedFactoryUseCodeList.Codes.A), "20231028", "20231128");
			TestDataTypeIsDate(new TestDataSetupHelper(Factory).Create929SnapShot(BondedFactoryUseCodeList.Codes.B), "20231028123000", "20231128123000");
			TestDeleteEntity(new TestDataSetupHelper(Factory).Create929SnapShot(ZString.Empty));

			void TestDataTypeIsDate(CusEntryHeader entry, ZString beforeValue, ZString afterValue)
			{
				AssertEquals(new ZDateTime("2023-10-26 12:30:00"), entry.Declaration.JE_DateOfArrival);
				AssertEquals(new ZDateTime("2023-10-28 12:30:00"), entry.EntryInstruction.CEI_BondedFactoryArrivalDate);
				AssertEquals(new ZDateTime("2023-10-27 12:30:00"), entry.Declaration.UnderbondMovementArrivalDate);

				AssertEquals(3, entry.MergedLines.Count);
				var entryLine3 = entry.MergedLines.FirstOrDefault(x => x.CL_LineNumber == 3);
				AssertEquals(1, entryLine3.InvoiceLines.Count);
				var invoiceLine3 = (JobComInvoiceLine)entryLine3.InvoiceLines[0];
				AssertEquals(new ZDateTime("2023-10-29 12:30:00"), invoiceLine3.CertificateOfOriginData.CSI_DateOfIssue);
				AssertEquals(new ZDateTime("2023-10-30 12:30:00"), invoiceLine3.GAApprovalDataCollection.FirstOrDefault().CSI_DateOfIssue);

				entry.Declaration.JE_DateOfArrival = new ZDateTime("2023-11-26 12:30:00");
				entry.EntryInstruction.CEI_BondedFactoryArrivalDate = new ZDateTime("2023-11-28 12:30:00");
				entry.Declaration.UnderbondMovementArrivalDate = new ZDateTime("2023-11-27 12:30:00");
				invoiceLine3.CertificateOfOriginData.CSI_DateOfIssue = new ZDateTime("2023-11-29 12:30:00");
				invoiceLine3.GAApprovalDataCollection.FirstOrDefault().CSI_DateOfIssue = new ZDateTime("2023-11-30 12:30:00");

				var amendmentManager = new GOVCBR5FEAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5FE);
				var amendedItems = amendmentManager.AmendedItems.OrderBy(x => x.DataItemID).ToArray();
				AssertEquals("A604", amendedItems[0].DataItemID);
				AssertEquals(EntityAmendType.Update, amendedItems[0].AmendType);
				AssertEquals("20231026", amendedItems[0].BeforeValue);
				AssertEquals("20231126", amendedItems[0].AfterValue);

				AssertEquals("A605", amendedItems[1].DataItemID);
				AssertEquals(EntityAmendType.Update, amendedItems[1].AmendType);
				AssertEquals("20231027", amendedItems[1].BeforeValue);
				AssertEquals("20231127", amendedItems[1].AfterValue);

				AssertEquals("A704", amendedItems[2].DataItemID);
				AssertEquals(EntityAmendType.Update, amendedItems[2].AmendType);
				AssertEquals(beforeValue, amendedItems[2].BeforeValue);
				AssertEquals(afterValue, amendedItems[2].AfterValue);

				AssertEquals("D107", amendedItems[3].DataItemID);
				AssertEquals(EntityAmendType.Update, amendedItems[3].AmendType);
				AssertEquals("20231030", amendedItems[3].BeforeValue);
				AssertEquals("20231130", amendedItems[3].AfterValue);

				AssertEquals("F104", amendedItems[4].DataItemID);
				AssertEquals(EntityAmendType.Update, amendedItems[4].AmendType);
				AssertEquals("20231029", amendedItems[4].BeforeValue);
				AssertEquals("20231129", amendedItems[4].AfterValue);
			}

			void TestDeleteEntity(CusEntryHeader entry)
			{
				entry.PivotsToContainers.Cast<CusContainerEntryHeaderPivot>().LastOrDefault().Delete();
				entry.EntryInstruction.OnlineOrders.Cast<OnlineOrder>().LastOrDefault().Delete();
				entry.MergedLines.FirstOrDefault(x => x.CL_LineNumber == 1).Delete();
				entry.MergedLines.FirstOrDefault(x => x.CL_LineNumber == 2).InvoiceLines.LastOrDefault().Delete();
				entry.Charges.FirstOrDefault().Delete();
				var entryLine3 = entry.MergedLines.FirstOrDefault(x => x.CL_LineNumber == 3);
				AssertEquals(1, entryLine3.InvoiceLines.Count);
				var invoiceLine3 = (JobComInvoiceLine)entryLine3.InvoiceLines[0];
				entryLine3.ImmediateDeliveries.Cast<ImmediateDelivery>().LastOrDefault().Delete();
				entryLine3.NonGADetailCollection.Cast<NonGADetail>().LastOrDefault().Delete();
				invoiceLine3.GAApprovalDataCollection.Cast<GAApproval>().LastOrDefault().Delete();
				entryLine3.PreviousExpDecLineCollection.Cast<PreviousExpDecLine>().LastOrDefault().Delete();

				var amendmentManager = new GOVCBR5FEAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5FE);
				var amendedItems = amendmentManager.AmendedItems.OrderBy(x => x.DataItemID).ToArray();

				AssertEquals("A819", amendedItems[0].DataItemID);
				AssertEquals(EntityAmendType.Update, amendedItems[0].AmendType);

				AssertEquals("A824", amendedItems[1].DataItemID);
				AssertEquals(EntityAmendType.Update, amendedItems[1].AmendType);

				AssertEquals("B111", amendedItems[2].DataItemID);
				AssertEquals(EntityAmendType.Delete, amendedItems[2].AmendType);

				AssertEquals("C109", amendedItems[3].DataItemID);
				AssertEquals(EntityAmendType.Delete, amendedItems[3].AmendType);

				AssertEquals("D109", amendedItems[4].DataItemID);
				AssertEquals(EntityAmendType.Delete, amendedItems[4].AmendType);

				AssertEquals("E106", amendedItems[5].DataItemID);
				AssertEquals(EntityAmendType.Delete, amendedItems[5].AmendType);

				AssertEquals("G106", amendedItems[6].DataItemID);
				AssertEquals(EntityAmendType.Delete, amendedItems[6].AmendType);

				AssertEquals("H103", amendedItems[7].DataItemID);
				AssertEquals(EntityAmendType.Delete, amendedItems[7].AmendType);

				AssertEquals("I103", amendedItems[8].DataItemID);
				AssertEquals(EntityAmendType.Delete, amendedItems[8].AmendType);

				AssertEquals("J103", amendedItems[9].DataItemID);
				AssertEquals(EntityAmendType.Delete, amendedItems[9].AmendType);
			}
		}

		public void TestDataItemIDList()
		{
			var list = Factory.GetCachedValue<ImportAmendmentDataItemIDList>();
			var entry = new TestDataSetupHelper(Factory).Create929SnapShot(ZString.Empty);
			var amendmentManager = new GOVCBR5FEAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertSame(list, amendmentManager.DataItemIDList);
		}

		public override void TestAmendmentVersion()
		{
			AssertNotNull("5FEMessageSender is not ready.", new TestDataSetupHelper(Factory).Create929SnapShot(ZString.Empty));
		}
	}
}
