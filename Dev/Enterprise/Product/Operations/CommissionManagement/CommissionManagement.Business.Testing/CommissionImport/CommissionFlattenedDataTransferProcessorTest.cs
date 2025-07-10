using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public class CommissionFlattenedDataTransferProcessorTest : TestCaseWithFactory
	{
		[TestDate(2002, 2, 2)]
		public void TestImport()
		{
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";
			var scwStaff = Factory.NewWithValidTestData<GlbStaff>();
			scwStaff.GS_Code = "SCW";
			var xxxParty = Factory.NewWithValidTestData<OrgHeader>();
			xxxParty.OH_Code = "XXX";
			var yyyParty = Factory.NewWithValidTestData<OrgHeader>();
			yyyParty.OH_Code = "YYY";

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new JobHeader.Loader(shipment).TryCreate();

			var existingInvoiceCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			existingInvoiceCommissionHeader.CH0_GC = GlbCompany.CurrentCompany.PK;
			existingInvoiceCommissionHeader.CH0_GroupingSourceID = invoice.PK;
			existingInvoiceCommissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			var existingInvoiceCommissionLine = existingInvoiceCommissionHeader.Lines.AddNew();
			existingInvoiceCommissionLine.CL0_GS_NKStaff = "ADL";
			existingInvoiceCommissionLine.CL0_CommissionType = CommissionTypes.Codes.FIX;
			existingInvoiceCommissionLine.CL0_EntityCommissionAmount = 100;

			Factory.Save();

			var flattenedCollection = new CommissionFlattenedCollection(Factory);
			var invItem1 = flattenedCollection.AddNew();
			invItem1.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			invItem1.GroupingSourceCode = invoice.AH_TransactionNum;
			invItem1.InvoiceType = TransactionTypes.Invoice;
			invItem1.StaffCode = "ADL";
			invItem1.CH0_CommissionDate = new ZDateTime(2002, 1, 1);
			invItem1.CL0_CommissionType = CommissionTypes.Codes.FIX;
			invItem1.CommissionCurrencyCode = "AUD";
			invItem1.EntityCommissionAmount = 100;

			var invItem2 = flattenedCollection.AddNew();
			invItem2.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			invItem2.GroupingSourceCode = invoice.AH_TransactionNum;
			invItem2.InvoiceType = TransactionTypes.Invoice;
			invItem2.PartyCode = "XXX";
			invItem2.CH0_CommissionDate = new ZDateTime(2002, 1, 1);
			invItem2.CL0_CommissionType = CommissionTypes.Codes.FIX;
			invItem2.CommissionCurrencyCode = "AUD";
			invItem2.EntityCommissionAmount = 200;

			var invItem3 = flattenedCollection.AddNew();
			invItem3.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			invItem3.GroupingSourceCode = invoice.AH_TransactionNum;
			invItem3.InvoiceType = TransactionTypes.Invoice;
			invItem3.StaffCode = "ADL";
			invItem3.CH0_CommissionDate = new ZDateTime(2002, 2, 1);
			invItem3.CL0_CommissionType = CommissionTypes.Codes.FIX;
			invItem3.CommissionCurrencyCode = "AUD";
			invItem3.EntityCommissionAmount = 300;

			var invItem4 = flattenedCollection.AddNew();
			invItem4.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			invItem4.GroupingSourceCode = invoice.AH_TransactionNum;
			invItem4.InvoiceType = TransactionTypes.Invoice;
			invItem4.PartyCode = "XXX";
			invItem4.CH0_CommissionDate = new ZDateTime(2002, 2, 1);
			invItem4.CL0_CommissionType = CommissionTypes.Codes.FIX;
			invItem4.CommissionCurrencyCode = "AUD";
			invItem4.EntityCommissionAmount = 400;

			var jobItem1 = flattenedCollection.AddNew();
			jobItem1.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			jobItem1.GroupingSourceCode = job.JH_JobNum;
			jobItem1.StaffCode = "ADL";
			jobItem1.CH0_CommissionDate = new ZDateTime(2002, 1, 1);
			jobItem1.CL0_CommissionType = CommissionTypes.Codes.FIX;
			jobItem1.CommissionCurrencyCode = "AUD";
			jobItem1.EntityCommissionAmount = 500;

			var jobItem2 = flattenedCollection.AddNew();
			jobItem2.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			jobItem2.GroupingSourceCode = job.JH_JobNum;
			jobItem2.PartyCode = "XXX";
			jobItem2.CH0_CommissionDate = new ZDateTime(2002, 1, 1);
			jobItem2.CL0_CommissionType = CommissionTypes.Codes.FIX;
			jobItem2.CommissionCurrencyCode = "AUD";
			jobItem2.EntityCommissionAmount = 600;

			var commissionLineCollection = new AccCommissionLineCollection(Factory);
			var flattenedCollectionInfo = new CommissionImportCollectionInfo(flattenedCollection, false);
			var processor = new CommissionFlattenedDataTransferProcessor(commissionLineCollection, flattenedCollectionInfo);

			var existingCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery());
			var existingCommissionLines = Factory.Load<AccCommissionLine>(new ZQuery());
			processor.Import();
			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 6, processor.NewCount);
				AssertEquals("OverriddenCount", 0, processor.OverriddenCount);
				AssertEquals("IgnoredCount", 0, processor.IgnoredCount);
				AssertEquals("ErrorCount", 0, processor.ErrorCount);
			});

			var newCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionHeaders.Select(x => x.PK)));
			var newCommissionLines = Factory.Load<AccCommissionLine>(new ZQuery(AccCommissionLineSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionLines.Select(x => x.PK)));
			AssertEquals("Each row should have been imported as a new commission line", 6, newCommissionLines.Length);
			AssertEquals("The commission lines should have been grouped into commission headers if have the same source and commission date", 3, newCommissionHeaders.Length);

			CombineAssertions(() =>
			{
				var newInvoiceCommissionHeader_2002_1_1 = newCommissionHeaders.Single(x => x.CH0_GroupingSourceID == invoice.PK && x.CH0_CommissionDate == new ZDateTime(2002, 1, 1));
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode, invoice.TablePrefix, newInvoiceCommissionHeader_2002_1_1.CH0_GroupingSourceTableCode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_AH_Source, ZGuid.Empty, newInvoiceCommissionHeader_2002_1_1.CH0_AH_Source);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GC, GlbCompany.CurrentCompany.PK, newInvoiceCommissionHeader_2002_1_1.CH0_GC);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Customer, ZGuid.Empty, newInvoiceCommissionHeader_2002_1_1.CH0_OH_Customer);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Debtor, ZGuid.Empty, newInvoiceCommissionHeader_2002_1_1.CH0_OH_Debtor);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CA0, ZGuid.Empty, newInvoiceCommissionHeader_2002_1_1.CH0_CA0);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Product, ZString.Empty, newInvoiceCommissionHeader_2002_1_1.CH0_Product);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Service, ZString.Empty, newInvoiceCommissionHeader_2002_1_1.CH0_Service);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SubModule, ZString.Empty, newInvoiceCommissionHeader_2002_1_1.CH0_SubModule);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Mode, ZString.Empty, newInvoiceCommissionHeader_2002_1_1.CH0_Mode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_NKOrigin, ZString.Empty, newInvoiceCommissionHeader_2002_1_1.CH0_NKOrigin);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_NKDestination, ZString.Empty, newInvoiceCommissionHeader_2002_1_1.CH0_NKDestination);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OverridenDateTimeUtc, ZDateTime.Empty, newInvoiceCommissionHeader_2002_1_1.CH0_OverridenDateTimeUtc);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_JobNumber, string.Empty, newInvoiceCommissionHeader_2002_1_1.CH0_JobNumber);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotDateTime, new ZDateTime(2002, 2, 2), newInvoiceCommissionHeader_2002_1_1.CH0_SnapshotDateTime);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotEventCode, AccCommissionHeaderSnapshotEventList.Codes.Imported, newInvoiceCommissionHeader_2002_1_1.CH0_SnapshotEventCode);

				AssertEquals(2, newInvoiceCommissionHeader_2002_1_1.Lines.Count);
				{
					var adlLine = newInvoiceCommissionHeader_2002_1_1.Lines.Single(x => x.CL0_GS_NKStaff == "ADL");
					AssertEquals(AccCommissionLineSchema.Constants.CL0_CommissionType, CommissionTypes.Codes.FIX, adlLine.CL0_CommissionType);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKCommissionCurrency, "AUD", adlLine.CL0_RX_NKCommissionCurrency);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityPercentage, (ZDecimal)100, adlLine.CL0_EntityPercentage);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityCommissionAmount, (ZDecimal)100, adlLine.CL0_EntityCommissionAmount);
				}
				{
					var xxxLine = newInvoiceCommissionHeader_2002_1_1.Lines.Single(x => x.CL0_OH_Party == xxxParty.PK);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_CommissionType, CommissionTypes.Codes.FIX, xxxLine.CL0_CommissionType);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKCommissionCurrency, "AUD", xxxLine.CL0_RX_NKCommissionCurrency);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityPercentage, (ZDecimal)100, xxxLine.CL0_EntityPercentage);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityCommissionAmount, (ZDecimal)200, xxxLine.CL0_EntityCommissionAmount);
				}
			});

			CombineAssertions(() =>
			{
				var newInvoiceCommissionHeader_2002_2_1 = newCommissionHeaders.Single(x => x.CH0_GroupingSourceID == invoice.PK && x.CH0_CommissionDate == new ZDateTime(2002, 2, 1));
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode, invoice.TablePrefix, newInvoiceCommissionHeader_2002_2_1.CH0_GroupingSourceTableCode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_AH_Source, ZGuid.Empty, newInvoiceCommissionHeader_2002_2_1.CH0_AH_Source);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GC, GlbCompany.CurrentCompany.PK, newInvoiceCommissionHeader_2002_2_1.CH0_GC);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Customer, ZGuid.Empty, newInvoiceCommissionHeader_2002_2_1.CH0_OH_Customer);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Debtor, ZGuid.Empty, newInvoiceCommissionHeader_2002_2_1.CH0_OH_Debtor);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CA0, ZGuid.Empty, newInvoiceCommissionHeader_2002_2_1.CH0_CA0);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Product, ZString.Empty, newInvoiceCommissionHeader_2002_2_1.CH0_Product);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Service, ZString.Empty, newInvoiceCommissionHeader_2002_2_1.CH0_Service);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SubModule, ZString.Empty, newInvoiceCommissionHeader_2002_2_1.CH0_SubModule);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Mode, ZString.Empty, newInvoiceCommissionHeader_2002_2_1.CH0_Mode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_NKOrigin, ZString.Empty, newInvoiceCommissionHeader_2002_2_1.CH0_NKOrigin);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_NKDestination, ZString.Empty, newInvoiceCommissionHeader_2002_2_1.CH0_NKDestination);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OverridenDateTimeUtc, ZDateTime.Empty, newInvoiceCommissionHeader_2002_2_1.CH0_OverridenDateTimeUtc);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_JobNumber, string.Empty, newInvoiceCommissionHeader_2002_2_1.CH0_JobNumber);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotDateTime, new ZDateTime(2002, 2, 2), newInvoiceCommissionHeader_2002_2_1.CH0_SnapshotDateTime);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotEventCode, AccCommissionHeaderSnapshotEventList.Codes.Imported, newInvoiceCommissionHeader_2002_2_1.CH0_SnapshotEventCode);

				AssertEquals(2, newInvoiceCommissionHeader_2002_2_1.Lines.Count);
				{
					var adlLine = newInvoiceCommissionHeader_2002_2_1.Lines.Single(x => x.CL0_GS_NKStaff == "ADL");
					AssertEquals(AccCommissionLineSchema.Constants.CL0_CommissionType, CommissionTypes.Codes.FIX, adlLine.CL0_CommissionType);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKCommissionCurrency, "AUD", adlLine.CL0_RX_NKCommissionCurrency);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityPercentage, (ZDecimal)100, adlLine.CL0_EntityPercentage);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityCommissionAmount, (ZDecimal)300, adlLine.CL0_EntityCommissionAmount);
				}
				{
					var xxxLine = newInvoiceCommissionHeader_2002_2_1.Lines.Single(x => x.CL0_OH_Party == xxxParty.PK);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_CommissionType, CommissionTypes.Codes.FIX, xxxLine.CL0_CommissionType);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKCommissionCurrency, "AUD", xxxLine.CL0_RX_NKCommissionCurrency);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityPercentage, (ZDecimal)100, xxxLine.CL0_EntityPercentage);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityCommissionAmount, (ZDecimal)400, xxxLine.CL0_EntityCommissionAmount);
				}
			});

			CombineAssertions(() =>
			{
				var newJobCommissionHeader = newCommissionHeaders.Single(x => x.CH0_GroupingSourceID == job.PK && x.CH0_CommissionDate == new ZDateTime(2002, 1, 1));
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode, job.TablePrefix, newJobCommissionHeader.CH0_GroupingSourceTableCode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_AH_Source, ZGuid.Empty, newJobCommissionHeader.CH0_AH_Source);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GC, GlbCompany.CurrentCompany.PK, newJobCommissionHeader.CH0_GC);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Customer, ZGuid.Empty, newJobCommissionHeader.CH0_OH_Customer);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Debtor, ZGuid.Empty, newJobCommissionHeader.CH0_OH_Debtor);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CA0, ZGuid.Empty, newJobCommissionHeader.CH0_CA0);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Product, ZString.Empty, newJobCommissionHeader.CH0_Product);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Service, ZString.Empty, newJobCommissionHeader.CH0_Service);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SubModule, ZString.Empty, newJobCommissionHeader.CH0_SubModule);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Mode, ZString.Empty, newJobCommissionHeader.CH0_Mode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_NKOrigin, ZString.Empty, newJobCommissionHeader.CH0_NKOrigin);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_NKDestination, ZString.Empty, newJobCommissionHeader.CH0_NKDestination);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OverridenDateTimeUtc, ZDateTime.Empty, newJobCommissionHeader.CH0_OverridenDateTimeUtc);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotDateTime, new ZDateTime(2002, 2, 2), newJobCommissionHeader.CH0_SnapshotDateTime);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotEventCode, AccCommissionHeaderSnapshotEventList.Codes.Imported, newJobCommissionHeader.CH0_SnapshotEventCode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_JobNumber, job.JH_JobNum, newJobCommissionHeader.CH0_JobNumber);

				AssertEquals(2, newJobCommissionHeader.Lines.Count);
				{
					var adlLine = newJobCommissionHeader.Lines.Single(x => x.CL0_GS_NKStaff == "ADL");
					AssertEquals(AccCommissionLineSchema.Constants.CL0_CommissionType, CommissionTypes.Codes.FIX, adlLine.CL0_CommissionType);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKCommissionCurrency, "AUD", adlLine.CL0_RX_NKCommissionCurrency);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityPercentage, (ZDecimal)100, adlLine.CL0_EntityPercentage);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityCommissionAmount, (ZDecimal)500, adlLine.CL0_EntityCommissionAmount);
				}
				{
					var xxxLine = newJobCommissionHeader.Lines.Single(x => x.CL0_OH_Party == xxxParty.PK);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_CommissionType, CommissionTypes.Codes.FIX, xxxLine.CL0_CommissionType);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_RX_NKCommissionCurrency, "AUD", xxxLine.CL0_RX_NKCommissionCurrency);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityPercentage, (ZDecimal)100, xxxLine.CL0_EntityPercentage);
					AssertEquals(AccCommissionLineSchema.Constants.CL0_EntityCommissionAmount, (ZDecimal)600, xxxLine.CL0_EntityCommissionAmount);
				}
			});
			{
				Factory.Save();

				flattenedCollectionInfo.ImportType = CommissionImportTypeList.Codes.Add;
				existingCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery());
				existingCommissionLines = Factory.Load<AccCommissionLine>(new ZQuery());
				processor.Import();
				CombineAssertions(() =>
				{
					AssertEquals("NewCount", 6, processor.NewCount);
					AssertEquals("OverriddenCount", 0, processor.OverriddenCount);
					AssertEquals("IgnoredCount", 0, processor.IgnoredCount);
					AssertEquals("ErrorCount", 0, processor.ErrorCount);
				});

				foreach (var commissionLine in newCommissionLines)  // these are the previously imported lines
				{
					AssertEquals("Should not have overridden existing values", false, commissionLine.IsOverriden);
				}

				newCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionHeaders.Select(x => x.PK)));
				newCommissionLines = Factory.Load<AccCommissionLine>(new ZQuery(AccCommissionLineSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionLines.Select(x => x.PK)));
				AssertEquals("Should have added each row again as a new commission line", 6, newCommissionLines.Length);
				AssertEquals("The commission lines should be grouped into new commission headers", 3, newCommissionHeaders.Length);
			}

			{
				Factory.Save();

				var previousUnoverridenCommissionLines = newCommissionLines.Where(x => !x.IsOverriden);
				flattenedCollectionInfo.ImportType = CommissionImportTypeList.Codes.OverrideIfChangedOtherwiseSkip;
				invItem1.EntityCommissionAmount = 50;
				jobItem1.CommissionCurrencyCode = "USD";
				existingCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery());
				existingCommissionLines = Factory.Load<AccCommissionLine>(new ZQuery());
				processor.Import();

				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("OverriddenCount", 2, processor.OverriddenCount);
				AssertEquals("IgnoredCount", 4, processor.IgnoredCount);
				AssertEquals("ErrorCount", 0, processor.ErrorCount);

				var lineForInvItem1 = newCommissionLines.Single(x => x.CL0_GS_NKStaff == "ADL" && x.CL0_EntityCommissionAmount == 100);
				AssertEquals(true, lineForInvItem1.IsOverriden);
				var lineForJobItem1 = newCommissionLines.Single(x => x.CL0_GS_NKStaff == "ADL" && x.CL0_EntityCommissionAmount == 500);
				AssertEquals(true, lineForJobItem1.IsOverriden);
				foreach (var commissionLine in previousUnoverridenCommissionLines.Except(new[] { lineForInvItem1, lineForJobItem1 }))
				{
					AssertEquals("Should have ignored the values that haven't changed", false, commissionLine.IsOverriden);
				}
				newCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionHeaders.Select(x => x.PK)));
				newCommissionLines = Factory.Load<AccCommissionLine>(new ZQuery(AccCommissionLineSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionLines.Select(x => x.PK)));
				AssertEquals("Should have overrided commissions for items that where commission currency or amount has changed (i.e. create reveral lines for previous commission lines AND also create the new commission lines)", (2 * 2) + 2, newCommissionLines.Length);
				AssertEquals("The new commission lines should be grouped into new commission headers", 2, newCommissionHeaders.Length);
			}
		}

		public void TestImport_WithInvalidCompanyCode()
		{
			var commissionLineCollection = new AccCommissionLineCollection(Factory);
			var flattenedCollection = new CommissionFlattenedCollection(Factory);
			var flattenedCollectionInfo = new CommissionImportCollectionInfo(flattenedCollection, false);
			var processor = new CommissionFlattenedDataTransferProcessor(commissionLineCollection, flattenedCollectionInfo);

			var flattened = flattenedCollection.AddNew();
			flattened.CompanyCode = "XXX";

			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("OverriddenCount", 0, processor.OverriddenCount);
				AssertEquals("IgnoredCount", 0, processor.IgnoredCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
				AssertMultilineASCIIEquals("Logs",
@"Line 1: Invalid company code 'XXX'",
					string.Join(System.Environment.NewLine, processor.Logs));
			});
		}

		public void TestImport_WithInvalidGroupingSource()
		{
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionNum = "0002222";
			var commissionLineCollection = new AccCommissionLineCollection(Factory);
			var flattenedCollection = new CommissionFlattenedCollection(Factory);
			var flattenedCollectionInfo = new CommissionImportCollectionInfo(flattenedCollection, false);
			var processor = new CommissionFlattenedDataTransferProcessor(commissionLineCollection, flattenedCollectionInfo);

			var flattened = flattenedCollection.AddNew();
			flattened.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			flattened.GroupingSourceCode = "0002222";
			flattened.InvoiceType = TransactionTypes.CreditNote;

			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("OverriddenCount", 0, processor.OverriddenCount);
				AssertEquals("IgnoredCount", 0, processor.IgnoredCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
				AssertMultilineASCIIEquals("Logs",
					string.Format("Line 1: Invalid job / transaction (Company = {0}, Transaction # = 0002222, Transaction Type = CRD)", GlbCompany.CurrentCompany.GC_Code),
					string.Join(System.Environment.NewLine, processor.Logs));
			});
		}

		public void TestImport_ForCurrentCompanyOnly_ButWithGroupingSourceInDifferentCompany()
		{
			var differentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionNum = "0002222";
			invoice.AH_GC = differentCompany.PK;
			var commissionLineCollection = new AccCommissionLineCollection(Factory);
			var flattenedCollection = new CommissionFlattenedCollection(Factory);
			var flattenedCollectionInfo = new CommissionImportCollectionInfo(flattenedCollection, true);
			var processor = new CommissionFlattenedDataTransferProcessor(commissionLineCollection, flattenedCollectionInfo);

			var flattened = flattenedCollection.AddNew();
			flattened.CompanyCode = differentCompany.GC_Code;
			flattened.GroupingSourceCode = "0002222";
			flattened.InvoiceType = TransactionTypes.Invoice;

			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("OverriddenCount", 0, processor.OverriddenCount);
				AssertEquals("IgnoredCount", 0, processor.IgnoredCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
				AssertMultilineASCIIEquals("Logs",
					"Line 1: Invalid job / transaction (Transaction # = 0002222, Transaction Type = INV)",
					string.Join(System.Environment.NewLine, processor.Logs));
			});
		}

		public void TestImport_WithInvalidEntityCodes()
		{
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionNum = "0002222";
			var commissionLineCollection = new AccCommissionLineCollection(Factory);
			var flattenedCollection = new CommissionFlattenedCollection(Factory);
			var flattenedCollectionInfo = new CommissionImportCollectionInfo(flattenedCollection, false);
			var processor = new CommissionFlattenedDataTransferProcessor(commissionLineCollection, flattenedCollectionInfo);

			var flattenedParty = flattenedCollection.AddNew();
			flattenedParty.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			flattenedParty.GroupingSourceCode = "0002222";
			flattenedParty.InvoiceType = TransactionTypes.Invoice;
			flattenedParty.PartyCode = "XXX";

			var flattenedStaff = flattenedCollection.AddNew();
			flattenedStaff.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			flattenedStaff.GroupingSourceCode = "0002222";
			flattenedStaff.InvoiceType = TransactionTypes.Invoice;
			flattenedStaff.StaffCode = "XXX";

			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("OverriddenCount", 0, processor.OverriddenCount);
				AssertEquals("IgnoredCount", 0, processor.IgnoredCount);
				AssertEquals("ErrorCount", 2, processor.ErrorCount);
				AssertMultilineASCIIEquals("Logs",
@"Line 1: Invalid organization code 'XXX'
Line 2: Invalid staff code 'XXX'",
					string.Join(System.Environment.NewLine, processor.Logs));
			});
		}

		public void TestImport_WithNoEntity()
		{
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionNum = "0002222";
			var commissionLineCollection = new AccCommissionLineCollection(Factory);
			var flattenedCollection = new CommissionFlattenedCollection(Factory);
			var flattenedCollectionInfo = new CommissionImportCollectionInfo(flattenedCollection, false);
			var processor = new CommissionFlattenedDataTransferProcessor(commissionLineCollection, flattenedCollectionInfo);

			var flattened = flattenedCollection.AddNew();
			flattened.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			flattened.GroupingSourceCode = "0002222";
			flattened.InvoiceType = TransactionTypes.Invoice;
			flattened.PartyCode = "";
			flattened.StaffCode = "";

			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("OverriddenCount", 0, processor.OverriddenCount);
				AssertEquals("IgnoredCount", 0, processor.IgnoredCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
				AssertMultilineASCIIEquals("Logs",
@"Line 1: No entity entered",
					string.Join(System.Environment.NewLine, processor.Logs));
			});
		}

		public void TestImport_WithNoCommissionDate()
		{
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionNum = "0002222";
			var commissionLineCollection = new AccCommissionLineCollection(Factory);
			var flattenedCollection = new CommissionFlattenedCollection(Factory);
			var flattenedCollectionInfo = new CommissionImportCollectionInfo(flattenedCollection, false);
			var processor = new CommissionFlattenedDataTransferProcessor(commissionLineCollection, flattenedCollectionInfo);

			var flattened = flattenedCollection.AddNew();
			flattened.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			flattened.GroupingSourceCode = "0002222";
			flattened.InvoiceType = TransactionTypes.Invoice;
			flattened.StaffCode = GlbStaff.CurrentUser.GS_Code;
			flattened.CH0_CommissionDate = ZDateTime.Empty;

			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("OverriddenCount", 0, processor.OverriddenCount);
				AssertEquals("IgnoredCount", 0, processor.IgnoredCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
				AssertMultilineASCIIEquals("Logs",
@"Line 1: No recognition date entered",
					string.Join(System.Environment.NewLine, processor.Logs));
			});
		}

		public void TestImport_WithInvalidCommissionType()
		{
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionNum = "0002222";
			var commissionLineCollection = new AccCommissionLineCollection(Factory);
			var flattenedCollection = new CommissionFlattenedCollection(Factory);
			var flattenedCollectionInfo = new CommissionImportCollectionInfo(flattenedCollection, false);
			var processor = new CommissionFlattenedDataTransferProcessor(commissionLineCollection, flattenedCollectionInfo);

			var flattened = flattenedCollection.AddNew();
			flattened.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			flattened.GroupingSourceCode = "0002222";
			flattened.InvoiceType = TransactionTypes.Invoice;
			flattened.StaffCode = GlbStaff.CurrentUser.GS_Code;
			flattened.CH0_CommissionDate = new ZDateTime(2002, 2, 2);
			flattened.CL0_CommissionType = "XXX";

			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("OverriddenCount", 0, processor.OverriddenCount);
				AssertEquals("IgnoredCount", 0, processor.IgnoredCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
				AssertMultilineASCIIEquals("Logs",
@"Line 1: Invalid commission type 'XXX'",
					string.Join(System.Environment.NewLine, processor.Logs));
			});
		}

		public void TestImport_WithInvalidCommissionCurrencyCode()
		{
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionNum = "0002222";
			var commissionLineCollection = new AccCommissionLineCollection(Factory);
			var flattenedCollection = new CommissionFlattenedCollection(Factory);
			var flattenedCollectionInfo = new CommissionImportCollectionInfo(flattenedCollection, false);
			var processor = new CommissionFlattenedDataTransferProcessor(commissionLineCollection, flattenedCollectionInfo);

			var flattened = flattenedCollection.AddNew();
			flattened.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			flattened.GroupingSourceCode = "0002222";
			flattened.InvoiceType = TransactionTypes.Invoice;
			flattened.StaffCode = GlbStaff.CurrentUser.GS_Code;
			flattened.CH0_CommissionDate = new ZDateTime(2002, 2, 2);
			flattened.CL0_CommissionType = CommissionTypes.Codes.FIX;
			flattened.CommissionCurrencyCode = "ZZZ";

			processor.Import();

			CombineAssertions(() =>
			{
				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("OverriddenCount", 0, processor.OverriddenCount);
				AssertEquals("IgnoredCount", 0, processor.IgnoredCount);
				AssertEquals("ErrorCount", 1, processor.ErrorCount);
				AssertMultilineASCIIEquals("Logs",
@"Line 1: Invalid commission currency code 'ZZZ'",
					string.Join(System.Environment.NewLine, processor.Logs));
			});
		}

		public void TestImport_WithMultipleForSameEntity_AndTransaction_AndRecognitionDate_AndCommissionType()
		{
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";
			var invoice = Factory.NewWithValidTestData<ARInvoice>();

			Factory.Save();

			var flattenedCollection = new CommissionFlattenedCollection(Factory);
			var invItem1 = flattenedCollection.AddNew();
			invItem1.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			invItem1.GroupingSourceCode = invoice.AH_TransactionNum;
			invItem1.InvoiceType = TransactionTypes.Invoice;
			invItem1.StaffCode = "ADL";
			invItem1.CH0_CommissionDate = new ZDateTime(2002, 1, 1);
			invItem1.CL0_CommissionType = CommissionTypes.Codes.FIX;
			invItem1.CommissionCurrencyCode = "AUD";
			invItem1.EntityCommissionAmount = 100;

			var invItem2 = flattenedCollection.AddNew();
			invItem2.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			invItem2.GroupingSourceCode = invoice.AH_TransactionNum;
			invItem2.InvoiceType = TransactionTypes.Invoice;
			invItem2.StaffCode = "ADL";
			invItem2.CH0_CommissionDate = new ZDateTime(2002, 1, 1);
			invItem2.CL0_CommissionType = CommissionTypes.Codes.FIX;
			invItem2.CommissionCurrencyCode = "AUD";
			invItem2.EntityCommissionAmount = 300;

			var commissionLineCollection = new AccCommissionLineCollection(Factory);
			var flattenedCollectionInfo = new CommissionImportCollectionInfo(flattenedCollection, false);
			var processor = new CommissionFlattenedDataTransferProcessor(commissionLineCollection, flattenedCollectionInfo);

			var existingCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery());
			var existingCommissionLines = Factory.Load<AccCommissionLine>(new ZQuery());
			processor.Import();
			AssertEquals("NewCount", 2, processor.NewCount);
			AssertEquals("OverriddenCount", 0, processor.OverriddenCount);
			AssertEquals("IgnoredCount", 0, processor.IgnoredCount);
			AssertEquals("ErrorCount", 0, processor.ErrorCount);

			var newCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionHeaders.Select(x => x.PK)));
			var newCommissionLines = Factory.Load<AccCommissionLine>(new ZQuery(AccCommissionLineSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionLines.Select(x => x.PK)));
			AssertEquals(1, newCommissionHeaders.Length);
			AssertEquals(2, newCommissionLines.Length);
			{
				Factory.Save();

				flattenedCollectionInfo.ImportType = CommissionImportTypeList.Codes.OverrideIfChangedOtherwiseSkip;
				invItem1.EntityCommissionAmount = 50;
				invItem2.CommissionCurrencyCode = "USD";

				existingCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery());
				existingCommissionLines = Factory.Load<AccCommissionLine>(new ZQuery());
				processor.Import();

				AssertEquals("NewCount", 0, processor.NewCount);
				AssertEquals("OverriddenCount", 2, processor.OverriddenCount);
				AssertEquals("IgnoredCount", 0, processor.IgnoredCount);
				AssertEquals("ErrorCount", 0, processor.ErrorCount);
				foreach (var commissionLine in newCommissionLines)  // these are the previously imported lines
				{
					AssertEquals("Should have overridden existing values", true, commissionLine.IsOverriden);
				}

				newCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionHeaders.Select(x => x.PK)));
				newCommissionLines = Factory.Load<AccCommissionLine>(new ZQuery(AccCommissionLineSchema.PK, SQLComparisonOperator.NotEqual, existingCommissionLines.Select(x => x.PK)));
				AssertEquals("Should have added commissions again AND mark existing commissions as overriden", 1, newCommissionHeaders.Length);
				AssertEquals("Should have added commissions again AND mark existing commissions as overriden", 2 * 2, newCommissionLines.Length);
			}
		}
	}
}
