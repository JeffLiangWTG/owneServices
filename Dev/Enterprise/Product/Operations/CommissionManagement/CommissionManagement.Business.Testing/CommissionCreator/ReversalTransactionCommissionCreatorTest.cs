using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public class ReversalInvoiceCommissionCreatorTest : CommissionCreatorTestCase
	{
		#region CreateCommissions

		[TestDate(2005, 5, 5)]
		public void TestCreateCommissions()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			var invoice1 = Factory.New<ARInvoice>();
			var invoice2 = Factory.New<ARInvoice>();
			var invoice1Reversing = new ARInvoiceReversing(invoice1);
			var invoice2Reversing = new ARInvoiceReversing(invoice2);
			invoice1Reversing.Reverse();
			invoice2Reversing.Reverse();
			var reversalInvoice1 = invoice1.ReverseInvoice;
			reversalInvoice1.AH_PostDate = new ZDateTime(2003, 3, 3);
			var reversalInvoice2 = invoice1.ReverseInvoice;
			reversalInvoice2.AH_PostDate = new ZDateTime(2003, 3, 3);

			var commissionHeader1A = Factory.New<AccCommissionHeader>();
			commissionHeader1A.CH0_AH_Source = invoice1.PK;
			commissionHeader1A.CH0_CA0 = Factory.NewWithValidTestData<OrgCommissionAgreement>().PK;
			commissionHeader1A.CH0_CommissionDate = new ZDate(2002, 2, 2);
			commissionHeader1A.CH0_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
			commissionHeader1A.CH0_GroupingSourceID = invoice1.PK;
			commissionHeader1A.CH0_GroupingSourceTableCode = invoice1.TablePrefix;
			commissionHeader1A.CH0_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			commissionHeader1A.CH0_OH_Debtor = Factory.NewWithValidTestData<OrgHeader>().PK;
			commissionHeader1A.CH0_Product = "AAA";
			commissionHeader1A.CH0_Service = "AAA";
			commissionHeader1A.CH0_SubModule = "AAA";
			commissionHeader1A.CH0_Mode = "AAA";
			commissionHeader1A.CH0_NKOrigin = "AUSYD";
			commissionHeader1A.CH0_NKDestination = "AUSYD";
			commissionHeader1A.CH0_SnapshotDateTime = new ZDateTime(2002, 2, 2);
			commissionHeader1A.CH0_SnapshotEventCode = AccCommissionHeaderSnapshotEventList.Codes.Posted;

			var header1A_Group1 = commissionHeader1A.LineGroups.AddNew();
			header1A_Group1.CLG_AC = chargeCode1.PK;
			header1A_Group1.CLG_RX_NKTransactionCurrency = "CUR";
			header1A_Group1.CLG_TransactionAmount = 50;
			header1A_Group1.CLG_RX_NKCommissionCurrency = "AUD";
			header1A_Group1.CLG_TotalCommissionableAmount = 100;

			var header1A_Group1_Line1 = header1A_Group1.Lines.AddNew();
			header1A_Group1_Line1.CL0_CAT = Factory.NewWithValidTestData<OrgCommissionAgreementRecipientRate>().PK;
			header1A_Group1_Line1.CL0_CommissionType = "PCT";
			header1A_Group1_Line1.CL0_GS_NKStaff = "ADL";
			header1A_Group1_Line1.CL0_OH_Party = ZGuid.Empty;
			header1A_Group1_Line1.CL0_RX_NKTransactionCurrency = "USD";
			header1A_Group1_Line1.CL0_TransactionAmount = 2000;
			header1A_Group1_Line1.CL0_RX_NKCommissionCurrency = "AUD";
			header1A_Group1_Line1.CL0_TotalCommissionableAmount = 4000;
			header1A_Group1_Line1.CL0_SharePortion = 1;
			header1A_Group1_Line1.CL0_ShareTotal = 2;
			header1A_Group1_Line1.CL0_ShareCommissionAmount = 2000;
			header1A_Group1_Line1.CL0_EntityPercentage = 50;
			header1A_Group1_Line1.CL0_EntityCommissionAmount = 1000;
			header1A_Group1_Line1.CL0_ApprovedDateTimeUtc = new ZDateTime(2002, 2, 2);
			header1A_Group1_Line1.CL0_PaidDateTimeUtc = new ZDateTime(2002, 2, 2);

			var header1A_Group1_Line2 = header1A_Group1.Lines.AddNew();
			header1A_Group1_Line2.CL0_GS_NKStaff = "SCW";

			var header1A_Group2 = commissionHeader1A.LineGroups.AddNew();
			header1A_Group2.CLG_AC = chargeCode2.PK;

			var header1A_Line1 = commissionHeader1A.Lines.AddNew();
			var header1A_Line2 = commissionHeader1A.Lines.AddNew();

			var commissionHeader1B = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader1B.CH0_AH_Source = invoice1.PK;
			commissionHeader1B.CH0_GroupingSourceTableCode = invoice1.TablePrefix;
			commissionHeader1B.CH0_GroupingSourceID = invoice1.PK;
			commissionHeader1B.CH0_Product = "BBB";
			commissionHeader1B.CH0_Service = "BBB";
			commissionHeader1B.CH0_SubModule = "BBB";
			commissionHeader1B.CH0_Mode = "BBB";
			commissionHeader1B.CH0_NKOrigin = "GBLON";
			commissionHeader1B.CH0_NKDestination = "GBLON";

			var overridenCommissionHeader1 = Factory.NewWithValidTestData<AccCommissionHeader>();
			overridenCommissionHeader1.CH0_AH_Source = invoice1.PK;
			overridenCommissionHeader1.CH0_GroupingSourceTableCode = invoice1.TablePrefix;
			overridenCommissionHeader1.CH0_GroupingSourceID = invoice1.PK;
			overridenCommissionHeader1.CH0_Product = "CCC";
			overridenCommissionHeader1.CH0_Service = "CCC";
			overridenCommissionHeader1.CH0_SubModule = "CCC";
			overridenCommissionHeader1.CH0_Mode = "CCC";
			overridenCommissionHeader1.CH0_NKOrigin = "USLAX";
			overridenCommissionHeader1.CH0_NKDestination = "USLAX";
			overridenCommissionHeader1.CH0_OverridenDateTimeUtc = new ZDateTime(2002, 2, 2);

			var creator = new ReversalTransactionCommissionCreator(reversalInvoice1);
			creator.CreateCommissions();
			Factory.Save();

			var reversalHeaders = new BusinessObjectFactory().Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, reversalInvoice1.PK));
			var reversalHeader1A = reversalHeaders.First(x => x.CH0_Product == "AAA");
			{
				AssertEquals(commissionHeader1A.CH0_GC, reversalHeader1A.CH0_GC);
				AssertEquals(reversalInvoice1.PK, reversalHeader1A.CH0_AH_Source);
				AssertEquals(commissionHeader1A.CH0_GroupingSourceTableCode, reversalHeader1A.CH0_GroupingSourceTableCode);
				AssertEquals(commissionHeader1A.CH0_GroupingSourceID, reversalHeader1A.CH0_GroupingSourceID);
				AssertEquals(commissionHeader1A.CH0_CA0, reversalHeader1A.CH0_CA0);
				AssertEquals(commissionHeader1A.CH0_OH_Debtor, reversalHeader1A.CH0_OH_Debtor);
				AssertEquals(commissionHeader1A.CH0_OH_Customer, reversalHeader1A.CH0_OH_Customer);
				AssertEquals(commissionHeader1A.CH0_Product, reversalHeader1A.CH0_Product);
				AssertEquals(commissionHeader1A.CH0_Service, reversalHeader1A.CH0_Service);
				AssertEquals(commissionHeader1A.CH0_SubModule, reversalHeader1A.CH0_SubModule);
				AssertEquals(commissionHeader1A.CH0_CommissionDate, reversalHeader1A.CH0_CommissionDate);
				AssertEquals(commissionHeader1A.CH0_Mode, reversalHeader1A.CH0_Mode);
				AssertEquals(commissionHeader1A.CH0_NKOrigin, reversalHeader1A.CH0_NKOrigin);
				AssertEquals(commissionHeader1A.CH0_NKDestination, reversalHeader1A.CH0_NKDestination);
				AssertEquals(commissionHeader1A.CH0_JobNumber, reversalHeader1A.CH0_JobNumber);
				AssertEquals(reversalInvoice1.AH_PostDate, reversalHeader1A.CH0_SnapshotDateTime);
				AssertEquals(AccCommissionHeaderSnapshotEventList.Codes.Posted, reversalHeader1A.CH0_SnapshotEventCode);

				var reversalHeader1A_Group1 = reversalHeader1A.LineGroups.First(x => x.CLG_AC == chargeCode1.PK);
				{
					AssertEquals(header1A_Group1.CLG_RX_NKTransactionCurrency, reversalHeader1A_Group1.CLG_RX_NKTransactionCurrency);
					AssertEquals(-header1A_Group1.CLG_TransactionAmount, reversalHeader1A_Group1.CLG_TransactionAmount);
					AssertEquals(header1A_Group1.CLG_RX_NKCommissionCurrency, reversalHeader1A_Group1.CLG_RX_NKCommissionCurrency);
					AssertEquals(-header1A_Group1.CLG_TotalCommissionableAmount, reversalHeader1A_Group1.CLG_TotalCommissionableAmount);

					var reversalHeader1A_Group1_Line1 = reversalHeader1A_Group1.Lines.First(x => x.CL0_GS_NKStaff == "ADL");
					{
						AssertEquals(header1A_Group1_Line1.PK, reversalHeader1A_Group1_Line1.CL0_BelongsToGroup);
						AssertEquals(header1A_Group1_Line1.CL0_CAT, reversalHeader1A_Group1_Line1.CL0_CAT);
						AssertEquals(header1A_Group1_Line1.CL0_GS_NKStaff, reversalHeader1A_Group1_Line1.CL0_GS_NKStaff);
						AssertEquals(header1A_Group1_Line1.CL0_OH_Party, reversalHeader1A_Group1_Line1.CL0_OH_Party);
						AssertEquals(header1A_Group1_Line1.CL0_RX_NKTransactionCurrency, reversalHeader1A_Group1_Line1.CL0_RX_NKTransactionCurrency);
						AssertEquals(-header1A_Group1_Line1.CL0_TransactionAmount, reversalHeader1A_Group1_Line1.CL0_TransactionAmount);
						AssertEquals(header1A_Group1_Line1.CL0_CommissionType, reversalHeader1A_Group1_Line1.CL0_CommissionType);
						AssertEquals(header1A_Group1_Line1.CL0_RX_NKCommissionCurrency, reversalHeader1A_Group1_Line1.CL0_RX_NKCommissionCurrency);
						AssertEquals(-header1A_Group1_Line1.CL0_TotalCommissionableAmount, reversalHeader1A_Group1_Line1.CL0_TotalCommissionableAmount);
						AssertEquals(header1A_Group1_Line1.CL0_SharePortion, reversalHeader1A_Group1_Line1.CL0_SharePortion);
						AssertEquals(header1A_Group1_Line1.CL0_ShareTotal, reversalHeader1A_Group1_Line1.CL0_ShareTotal);
						AssertEquals(-header1A_Group1_Line1.CL0_ShareCommissionAmount, reversalHeader1A_Group1_Line1.CL0_ShareCommissionAmount);
						AssertEquals(header1A_Group1_Line1.CL0_EntityPercentage, reversalHeader1A_Group1_Line1.CL0_EntityPercentage);
						AssertEquals(-header1A_Group1_Line1.CL0_EntityCommissionAmount, reversalHeader1A_Group1_Line1.CL0_EntityCommissionAmount);

						AssertEquals(ZDateTime.Empty, reversalHeader1A_Group1_Line1.CL0_ApprovedDateTimeUtc);
						AssertEquals(ZDateTime.Empty, reversalHeader1A_Group1_Line1.CL0_OverridenDateTimeUtc);
						AssertEquals(ZDateTime.Empty, reversalHeader1A_Group1_Line1.CL0_PaidDateTimeUtc);
						AssertEquals(ZDateTime.Empty, reversalHeader1A_Group1_Line1.CL0_CancelledDateTimeUtc);
					}

					var reversalHeader1A_Group1_Line2 = reversalHeader1A_Group1.Lines.First(x => x.CL0_GS_NKStaff == "SCW");
					{
						AssertEquals("Should automatically set to cancelled if the original line has not been paid yet", new ZDateTime(2005, 5, 5), header1A_Group1_Line2.CL0_CancelledDateTimeUtc);
						AssertEquals("Should automatically set to cancelled if the original line has not been paid yet", new ZDateTime(2005, 5, 5), reversalHeader1A_Group1_Line2.CL0_CancelledDateTimeUtc);
					}
					AssertEquals(2, reversalHeader1A_Group1.Lines.Count);
				}

				AssertNotNull(reversalHeader1A.LineGroups.FirstOrDefault(x => x.CLG_AC == chargeCode2.PK));
				AssertEquals(2, reversalHeader1A.LineGroups.Count);

				AssertEquals(2, reversalHeader1A.Lines.Count);
			}

			AssertNotNull(reversalHeaders.FirstOrDefault(x => x.CH0_Product == "BBB"));
			AssertNull("Should not create if commission header for original invoice is cancelled", reversalHeaders.FirstOrDefault(x => x.CH0_Product == "CCC"));
			AssertEquals(2, reversalHeaders.Length);

			creator.CreateCommissions();
			AssertEquals("Should not create reversal commissions again", 2, Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, reversalInvoice1.PK)).Length);
		}

		[TestDate(2005, 5, 5)]
		public void TestCreateCommissions_JobRelated()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			var customer = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "TEST00001";
			var job = new Job.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_RX_NKTransactionCurrency = "USD";
			invoice.AH_PostDate = new ZDateTime(2002, 2, 2);

			var invoiceReversing = new ARInvoiceReversing(invoice);
			invoiceReversing.Reverse();
			var reversalInvoice = invoice.ReverseInvoice;
			reversalInvoice.AH_PostDate = new ZDateTime(2003, 3, 3);

			var commissionHeader = Factory.New<AccCommissionHeader>();
			commissionHeader.CH0_AH_Source = invoice.PK;
			commissionHeader.CH0_CA0 = Factory.NewWithValidTestData<OrgCommissionAgreement>().PK;
			commissionHeader.CH0_CommissionDate = new ZDate(2002, 2, 2);
			commissionHeader.CH0_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
			commissionHeader.CH0_GroupingSourceID = job.PK;
			commissionHeader.CH0_GroupingSourceTableCode = job.TablePrefix;
			commissionHeader.CH0_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			commissionHeader.CH0_OH_Debtor = Factory.NewWithValidTestData<OrgHeader>().PK;
			commissionHeader.CH0_Product = "AAA";
			commissionHeader.CH0_Service = "AAA";
			commissionHeader.CH0_SubModule = "AAA";
			commissionHeader.CH0_Mode = "AAA";
			commissionHeader.CH0_NKOrigin = "AUSYD";
			commissionHeader.CH0_NKDestination = "AUSYD";
			commissionHeader.CH0_JobNumber = "TEST00001";
			commissionHeader.CH0_SnapshotDateTime = new ZDateTime(2002, 2, 2);
			commissionHeader.CH0_SnapshotEventCode = AccCommissionHeaderSnapshotEventList.Codes.Posted;

			var header_Group1 = commissionHeader.LineGroups.AddNew();
			header_Group1.CLG_AC = chargeCode1.PK;
			header_Group1.CLG_RX_NKTransactionCurrency = "CUR";
			header_Group1.CLG_TransactionAmount = 50;
			header_Group1.CLG_RX_NKCommissionCurrency = "AUD";
			header_Group1.CLG_TotalCommissionableAmount = 100;

			var header_Group1_Line1 = header_Group1.Lines.AddNew();
			header_Group1_Line1.CL0_CAT = Factory.NewWithValidTestData<OrgCommissionAgreementRecipientRate>().PK;
			header_Group1_Line1.CL0_CommissionType = "PCT";
			header_Group1_Line1.CL0_GS_NKStaff = "ADL";
			header_Group1_Line1.CL0_OH_Party = ZGuid.Empty;
			header_Group1_Line1.CL0_RX_NKTransactionCurrency = "USD";
			header_Group1_Line1.CL0_TransactionAmount = 2000;
			header_Group1_Line1.CL0_RX_NKCommissionCurrency = "AUD";
			header_Group1_Line1.CL0_TotalCommissionableAmount = 4000;
			header_Group1_Line1.CL0_SharePortion = 1;
			header_Group1_Line1.CL0_ShareTotal = 2;
			header_Group1_Line1.CL0_ShareCommissionAmount = 2000;
			header_Group1_Line1.CL0_EntityPercentage = 50;
			header_Group1_Line1.CL0_EntityCommissionAmount = 1000;
			header_Group1_Line1.CL0_ApprovedDateTimeUtc = new ZDateTime(2002, 2, 2);
			header_Group1_Line1.CL0_PaidDateTimeUtc = new ZDateTime(2002, 2, 2);

			var creator = new ReversalTransactionCommissionCreator(reversalInvoice);
			creator.CreateCommissions();
			Factory.Save();

			var reversalHeaders = new BusinessObjectFactory().Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, reversalInvoice.PK));
			var reversalHeader = reversalHeaders.First(x => x.CH0_Product == "AAA");
			{
				AssertEquals(commissionHeader.CH0_GC, reversalHeader.CH0_GC);
				AssertEquals(reversalInvoice.PK, reversalHeader.CH0_AH_Source);
				AssertEquals(commissionHeader.CH0_GroupingSourceTableCode, reversalHeader.CH0_GroupingSourceTableCode);
				AssertEquals(commissionHeader.CH0_GroupingSourceID, reversalHeader.CH0_GroupingSourceID);
				AssertEquals(commissionHeader.CH0_CA0, reversalHeader.CH0_CA0);
				AssertEquals(commissionHeader.CH0_OH_Debtor, reversalHeader.CH0_OH_Debtor);
				AssertEquals(commissionHeader.CH0_OH_Customer, reversalHeader.CH0_OH_Customer);
				AssertEquals(commissionHeader.CH0_Product, reversalHeader.CH0_Product);
				AssertEquals(commissionHeader.CH0_Service, reversalHeader.CH0_Service);
				AssertEquals(commissionHeader.CH0_SubModule, reversalHeader.CH0_SubModule);
				AssertEquals(commissionHeader.CH0_CommissionDate, reversalHeader.CH0_CommissionDate);
				AssertEquals(commissionHeader.CH0_Mode, reversalHeader.CH0_Mode);
				AssertEquals(commissionHeader.CH0_NKOrigin, reversalHeader.CH0_NKOrigin);
				AssertEquals(commissionHeader.CH0_NKDestination, reversalHeader.CH0_NKDestination);
				AssertEquals(commissionHeader.CH0_JobNumber, reversalHeader.CH0_JobNumber);
				AssertEquals(reversalInvoice.AH_PostDate, reversalHeader.CH0_SnapshotDateTime);
				AssertEquals(AccCommissionHeaderSnapshotEventList.Codes.Posted, reversalHeader.CH0_SnapshotEventCode);
			}
		}

		[TestDate(2005, 5, 5)]
		public void TestCreateCommissions_JobRevenueJournal()
		{
			var jrj = TestObjectCreator.CreateJobRevenueJournal(typeof(JobRevenueJournal), TestObjectCreator.CC1, Guid.Empty, 100);
			jrj.JournalLines[0].AL_GE = jrj.JournalLines[1].AL_GE = GlbDepartment.CurrentDepartment.PK;
			jrj.AH_PostDate = new ZDateTime(2003, 3, 3);

			jrj.GenerateReverseTransaction(true);
			var reversalJRJ1 = jrj.ReverseTransaction as TransactionHeader;
			reversalJRJ1.AH_TransactionBelongsToGroup = jrj.PK;
			reversalJRJ1.IsCancelled = true;
			Factory.Save();

			var creator = new ReversalTransactionCommissionCreator(reversalJRJ1 as ICommissionableTransaction);
			creator.CreateCommissions();
			Factory.Save();

			var reversalHeaders = new BusinessObjectFactory().Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, reversalJRJ1.PK));
			AssertEquals(0, reversalHeaders.Length);
		}

		[ExpectNoExceptions]
		public void TestCreateReversalTransactionCommissionsIfRequired_JobRevenueJournal()
		{
			var jrj = TestObjectCreator.CreateJobRevenueJournal(typeof(JobRevenueJournal), TestObjectCreator.CC1, Guid.Empty, 100);
			jrj.JournalLines[0].AL_GE = jrj.JournalLines[1].AL_GE = GlbDepartment.CurrentDepartment.PK;
			jrj.AH_PostDate = new ZDateTime(2003, 3, 3);
			jrj.IsCancelled = true;

			jrj.GenerateReverseTransaction(true);
			var reversalJRJ1 = jrj.ReverseTransaction as TransactionHeader;
			reversalJRJ1.AH_TransactionBelongsToGroup = jrj.PK;
			reversalJRJ1.IsCancelled = true;

			Factory.Save();

			ReversalTransactionCommissionCreator.AddReversalTransactionFetchHints(Factory, new ICommissionableTransaction[] { jrj });
			ReversalTransactionCommissionCreator.CreateReversalTransactionCommissionsIfRequired(jrj);
		}

		#endregion
	}
}
