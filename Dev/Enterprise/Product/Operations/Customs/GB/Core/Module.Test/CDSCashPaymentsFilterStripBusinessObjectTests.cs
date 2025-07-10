using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.Testing
{
	[TestedType(typeof(CDSCashPaymentsFilterStripBusinessObject))]
	class CDSCashPaymentsFilterStripBusinessObjectTests : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CDSCashPaymentsFilterStripBusinessObject();

		JobDeclaration CreateJobDeclarationWithPayInfo(ZDecimal paymentAmount, ZDateTime paymentDate, ZString paymentReference, ZString paymentStatus, ZDate receiptDate, string transactionType = "CAS")
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "ABC";
			var payInfo = cusEntryHeader.EntryPayInfos.AddNew();
			payInfo.C9_PaymentDate = paymentDate;
			payInfo.C9_PaymentAmount = paymentAmount;
			payInfo.C9_PaymentReference = paymentReference;
			payInfo.C9_PaymentStatus = paymentStatus;
			payInfo.C9_ReceiptDate = receiptDate;
			payInfo.C9_TransactionType = transactionType;
			return declaration;
		}

		public void TestImporterFilter()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterStrip[CDSCashPaymentsFilterStripBusinessObject.Filters.Importer];
			AssertNotNull(CDSCashPaymentsFilterStripBusinessObject.Filters.Importer, filter);
			filter.IsActive = true;
			filter.Property = ZGuid.BrettsGuid;
			AssertContains("JE_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
			AssertContains("C9_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
		}

		public void TestMRNClusterKey()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip[CDSCashPaymentsFilterStripBusinessObject.Filters.MRN];
			AssertNotNull(CDSCashPaymentsFilterStripBusinessObject.Filters.MRN, filter);
			filter.IsActive = true;
			filter.Property = "Y";
			AssertContains("CH_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
			AssertContains("C9_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
		}

		public void TestDUCRFilter()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip[CDSCashPaymentsFilterStripBusinessObject.Filters.Ducr];

			var declaration1 = CreateJobDeclarationWithPayInfo(100m, ZDateTime.Now, ZString.Empty, ZString.Empty, ZDate.Empty);
			var header1 = declaration1.ActiveEntryHeaders[0];
			header1.CH_BGMReference = "ABC";

			var declaration2 = CreateJobDeclarationWithPayInfo(200m, ZDateTime.Now, ZString.Empty, ZString.Empty, ZDate.Empty);
			var header2 = declaration2.ActiveEntryHeaders[0];
			header2.CH_BGMReference = "XYZ";

			Factory.Save();

			filter.IsActive = true;
			filter.Property = header1.CH_BGMReference;
			var filteredInfos = Factory.Load(typeof(CusEntryPayInfo), filter.FilterBusinessObject.Filter);
			AssertEquals(1, filteredInfos.Length);
			var filteredPayInfo = (CusEntryPayInfo)filteredInfos[0];
			AssertEquals(header1.CH_BGMReference, filteredPayInfo.EntryHeader.CH_BGMReference);
			AssertEquals(100m, filteredPayInfo.C9_PaymentAmount);
		}

		public void TestMRNFilter()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip[CDSCashPaymentsFilterStripBusinessObject.Filters.MRN];

			var declaration1 = CreateJobDeclarationWithPayInfo(100m, ZDateTime.Now, ZString.Empty, ZString.Empty, ZDate.Empty);
			var header1 = declaration1.ActiveEntryHeaders[0];
			var entryNumber1 = Factory.New<CusEntryNumber>();
			entryNumber1.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNumber1.CE_EntryNum = "GBEntry1";
			entryNumber1.CE_ParentID = header1.PK;
			entryNumber1.CE_ParentTable = header1.TableName;

			var declaration2 = CreateJobDeclarationWithPayInfo(200m, ZDateTime.Now, ZString.Empty, ZString.Empty, ZDate.Empty);
			var header2 = declaration2.ActiveEntryHeaders[0];
			var entryNumber2 = Factory.New<CusEntryNumber>();
			entryNumber2.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNumber2.CE_EntryNum = "GBEntry2";
			entryNumber2.CE_ParentID = header2.PK;
			entryNumber2.CE_ParentTable = header2.TableName;
			Factory.Save();

			filter.IsActive = true;
			filter.Property = entryNumber1.CE_EntryNum;
			var filteredInfos = Factory.Load(typeof(CusEntryPayInfo), filter.FilterBusinessObject.Filter);
			AssertEquals(1, filteredInfos.Length);
			var filteredPayInfo = (CusEntryPayInfo)filteredInfos[0];
			AssertEquals(entryNumber1.CE_EntryNum, filteredPayInfo.MRN);
			AssertEquals(100m, filteredPayInfo.C9_PaymentAmount);
		}

		public void TestPaymentDate()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleDateFilter)filterStrip[CDSCashPaymentsFilterStripBusinessObject.Filters.PaymentDate];
			AssertNotNull(CDSCashPaymentsFilterStripBusinessObject.Filters.PaymentDate, filter);
			var setDateTime = new ZDateTime(2022, 4, 20, 12, 0, 0);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = setDateTime;
			filter.Property2 = setDateTime;
			CreateJobDeclarationWithPayInfo(300m, setDateTime, ZString.Empty, ZString.Empty, ZDate.Empty);
			CreateJobDeclarationWithPayInfo(400m, ZDateTime.Now, ZString.Empty, ZString.Empty, ZDate.Empty);
			var filteredInfos = Factory.Load(typeof(CusEntryPayInfo), filter.FilterBusinessObject.Filter);
			AssertEquals(1, filteredInfos.Length);
			var filteredPayInfo = (CusEntryPayInfo)filteredInfos[0];
			AssertEquals(setDateTime, filteredPayInfo.C9_PaymentDate);
		}

		public void TestPaymentReference()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip[CDSCashPaymentsFilterStripBusinessObject.Filters.PaymentReference];
			AssertNotNull(CDSCashPaymentsFilterStripBusinessObject.Filters.PaymentReference, filter);
			filter.IsActive = true;
			filter.Property = "GB1Reference";
			CreateJobDeclarationWithPayInfo(500m, DateTime.Now, filter.Property, ZString.Empty, ZDate.Empty);
			CreateJobDeclarationWithPayInfo(600m, DateTime.Now, ZString.Empty, ZString.Empty, ZDate.Empty);
			var filteredInfos = Factory.Load(typeof(CusEntryPayInfo), filter.FilterBusinessObject.Filter);
			AssertEquals(1, filteredInfos.Length);
			var filteredPayInfo = (CusEntryPayInfo)filteredInfos[0];
			AssertEquals(filter.Property, filteredPayInfo.C9_PaymentReference);
		}

		public void TestPaymentStatus()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip[CDSCashPaymentsFilterStripBusinessObject.Filters.PaymentStatus];
			AssertNotNull(CDSCashPaymentsFilterStripBusinessObject.Filters.PaymentStatus, filter);
			filter.IsActive = true;
			filter.Property = Customs.Business.CusEntryPayInfoStatusList.Codes.Clear;
			CreateJobDeclarationWithPayInfo(700m, DateTime.Now, ZString.Empty, Customs.Business.CusEntryPayInfoStatusList.Codes.Clear, ZDate.Empty);
			CreateJobDeclarationWithPayInfo(800m, DateTime.Now, ZString.Empty, Customs.Business.CusEntryPayInfoStatusList.Codes.Pending, ZDate.Empty);
			var filteredInfos = Factory.Load(typeof(CusEntryPayInfo), filter.FilterBusinessObject.Filter);
			AssertEquals(1, filteredInfos.Length);
			var filteredPayInfo = (CusEntryPayInfo)filteredInfos[0];
			AssertEquals(Customs.Business.CusEntryPayInfoStatusList.Codes.Clear, filteredPayInfo.C9_PaymentStatus);
		}

		public void TestReceiptDate()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleDateFilter)filterStrip[CDSCashPaymentsFilterStripBusinessObject.Filters.ReceiptDate];
			AssertNotNull(CDSCashPaymentsFilterStripBusinessObject.Filters.ReceiptDate, filter);
			var setDate = new ZDate(2022, 4, 20);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = setDate;
			filter.Property2 = setDate;
			CreateJobDeclarationWithPayInfo(300m, ZDateTime.Now, ZString.Empty, ZString.Empty, setDate);
			CreateJobDeclarationWithPayInfo(400m, ZDateTime.Now, ZString.Empty, ZString.Empty, ZDate.Empty);
			var filteredInfos = Factory.Load(typeof(CusEntryPayInfo), filter.FilterBusinessObject.Filter);
			AssertEquals(1, filteredInfos.Length);
			var filteredPayInfo = (CusEntryPayInfo)filteredInfos[0];
			AssertEquals(setDate, filteredPayInfo.C9_ReceiptDate);
		}

		public void TestTransactionType()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip[CDSCashPaymentsFilterStripBusinessObject.Filters.TransactionType];
			AssertNotNull(CDSCashPaymentsFilterStripBusinessObject.Filters.PaymentReference, filter);
			filter.IsActive = true;
			filter.Property = "PVA";
			CreateJobDeclarationWithPayInfo(500m, DateTime.Now, filter.Property, ZString.Empty, ZDate.Empty);
			CreateJobDeclarationWithPayInfo(600m, DateTime.Now, ZString.Empty, ZString.Empty, ZDate.Empty, "PVA");
			var filteredInfos = Factory.Load(typeof(CusEntryPayInfo), filter.FilterBusinessObject.Filter);
			AssertEquals(1, filteredInfos.Length);
			var filteredPayInfo = (CusEntryPayInfo)filteredInfos[0];
			AssertEquals(600m, filteredPayInfo.C9_PaymentAmount);
		}
	}
}
