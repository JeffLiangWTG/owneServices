using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehousePeriodicInvoiceWrapper))]
	sealed class WarehousePeriodicInvoiceTest : WarehouseJobGenericWrapperTest
	{
		#region Test Properties

		#region TestJobChargeLines

		protected override void TestJobChargeLinesCore()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			WhsInvoice.ET_OH_Client = org.PK;

			Job job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = WhsInvoice.PK;
			job.JH_ParentTableCode = JobStorageSchema.Constants.Prefix;

			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "CC1";
			chargeCode2.AC_Code = "CC2";
			chargeCode1.AC_Desc = "CC1DESC";
			chargeCode2.AC_Desc = "CC2DESC";
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSInwards;
			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;

			ARInvoice.AH_JH = job.PK;
			AccTransactionLines aRLine = Factory.New<AccTransactionLines>();
			aRLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			aRLine.AL_AH = ARInvoice.PK;

			JobCharge jobCharge1 = job.Charges.AddNew();
			JobCharge jobCharge2 = job.Charges.AddNew();
			jobCharge1.JR_AC = chargeCode1.PK;
			jobCharge2.JR_AC = chargeCode2.PK;
			jobCharge1.JR_AL_ARLine = aRLine.PK;
			jobCharge2.JR_AL_ARLine = aRLine.PK;

			org.MiscServ.OM_IMInvoiceDetailReportSort = InvoiceDetailReportSortList.Codes.ChargeCode;
			AssertEquals(2, DocWrapper.JobChargeLines.Count);
			AssertEquals(jobCharge1, (JobCharge)DocWrapper.JobChargeLines[0].WrappedObject);
			AssertEquals(jobCharge2, (JobCharge)DocWrapper.JobChargeLines[1].WrappedObject);

			AssertEquals("CC1 (CC1DESC)", DocWrapper.JobChargeLines[0].GroupDesc);
			AssertEquals("CC2 (CC2DESC)", DocWrapper.JobChargeLines[1].GroupDesc);

			JobChargeCollectionTest.AssertNoJobChargesContainedInJobChargeCollection(Factory);
		}

		#endregion

		#region TestJobChargeLinesCore_JobChargeAttributes

		public void TestJobChargeLinesCore_JobChargeAttributes()
		{
			var org = Factory.New<OrgHeader>();
			WhsInvoice.ET_OH_Client = org.PK;
			org.MiscServ.OM_IMInvoiceDetailReportSort = "AT3";
			org.MiscServ.OM_IMPartAttrib3Name = "Color";

			Job job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = WhsInvoice.PK;
			job.JH_ParentTableCode = JobStorageSchema.Constants.Prefix;

			var chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSInwards;

			ARInvoice.AH_JH = job.PK;
			AccTransactionLines aRLine = Factory.New<AccTransactionLines>();
			aRLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			aRLine.AL_AH = ARInvoice.PK;

			var jobCharge1 = job.Charges.AddNew();
			jobCharge1.JR_AC = chargeCode1.PK;
			jobCharge1.JR_AL_ARLine = aRLine.PK;
			var attrib = jobCharge1.JobChargeAttributes.AddNew();
			attrib.EC_Name = JobChargeAttribTypeList.Codes.Attrib3;
			attrib.EC_Value = "Red";

			AssertEquals(1, DocWrapper.JobChargeLines.Count);
			AssertEquals(jobCharge1, (JobCharge)DocWrapper.JobChargeLines[0].WrappedObject);

			AssertEquals("COLOR: Red", DocWrapper.JobChargeLines[0].GroupDesc);

			JobChargeCollectionTest.AssertNoJobChargesContainedInJobChargeCollection(Factory);
		}

		#endregion

		#region TestConsolidatedInvoiceRef

		protected override void TestConsolidatedInvoiceRefCore()
		{
			DocWrapper.MockConsolidatedInvoiceRef = "TESTConsolidatedInvoiceRef";
			AssertEquals("Incorrect ConsolidatedInvoiceRef", "TESTConsolidatedInvoiceRef", DocWrapper.ConsolidatedInvoiceRef);
		}

		#endregion

		#region TestDebtorCodeAndName

		protected override void TestDebtorCodeAndNameCore()
		{
			DocWrapper.MockDebtorCodeAndName = "TESTDebtorCodeAndName";
			AssertEquals("Incorrect DebtorCodeAndName", "TESTDebtorCodeAndName", DocWrapper.DebtorCodeAndName);
		}

		#endregion

		#region TestJobChargeLinesSort

		public void TestJobChargeLinesSort()
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = WhsInvoice.PK;
			job.JH_ParentTableCode = JobStorageSchema.Constants.Prefix;

			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "CC1";
			chargeCode2.AC_Code = "CC2";

			WhsReceive receive = Factory.New<WhsReceive>();
			WhsOrder order1 = Factory.New<WhsOrder>();
			WhsOrder order2 = Factory.New<WhsOrder>();
			WhsOrder order3 = Factory.New<WhsOrder>();
			receive.WD_DocketID = "I1";
			order1.WD_DocketID = "O1";
			order2.WD_DocketID = "O2";
			order3.WD_DocketID = "O3";
			receive.WD_ExternalReference = "INW1";
			order1.WD_ExternalReference = "ORD1";
			order2.WD_ExternalReference = "ORD2";
			order3.WD_ExternalReference = "ORD3";
			order1.WD_FinalisedDate = ZDateTimeOffset.Today.AddDays(-1);
			order2.WD_FinalisedDate = ZDateTimeOffset.Today;
			order3.WD_FinalisedDate = ZDateTimeOffset.Today.AddDays(-1);

			ARInvoice.AH_JH = job.PK;
			AccTransactionLines aRLine = Factory.New<AccTransactionLines>();
			aRLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			aRLine.AL_AH = ARInvoice.PK;

			JobCharge jobCharge1 = job.Charges.AddNew();
			JobCharge jobCharge2 = job.Charges.AddNew();
			JobCharge jobCharge3 = job.Charges.AddNew();
			JobCharge jobCharge4 = job.Charges.AddNew();
			jobCharge1.JR_AC = chargeCode2.PK;
			jobCharge2.JR_AC = chargeCode1.PK;
			jobCharge3.JR_AC = chargeCode2.PK;
			jobCharge4.JR_AC = chargeCode2.PK;
			jobCharge1.JR_AL_ARLine = aRLine.PK;
			jobCharge2.JR_AL_ARLine = aRLine.PK;
			jobCharge3.JR_AL_ARLine = aRLine.PK;
			jobCharge4.JR_AL_ARLine = aRLine.PK;
			jobCharge1.JR_OrderReference = order2.WD_DocketID;
			jobCharge2.JR_OrderReference = receive.WD_DocketID;
			jobCharge3.JR_OrderReference = order3.WD_DocketID;
			jobCharge4.JR_OrderReference = order1.WD_DocketID;

			AssertEquals(4, DocWrapper.JobChargeLines.Count);
			AssertEquals("Charge2 Should be 1st", jobCharge2, (JobCharge)DocWrapper.JobChargeLines[0].WrappedObject);
			AssertEquals("Charge4 Should be 2nd", jobCharge4, (JobCharge)DocWrapper.JobChargeLines[1].WrappedObject);
			AssertEquals("Charge3 Should be 3rd", jobCharge3, (JobCharge)DocWrapper.JobChargeLines[2].WrappedObject);
			AssertEquals("Charge1 Should be 4th", jobCharge1, (JobCharge)DocWrapper.JobChargeLines[3].WrappedObject);

			JobChargeCollectionTest.AssertNoJobChargesContainedInJobChargeCollection(Factory);
		}

		#endregion

		#region TestInvoiceNumber

		protected override void TestInvoiceNumberCore()
		{
			ARInvoice.AH_TransactionNum = ARInvoice.AH_ConsolidatedInvoiceRef = "I00100";
			AssertEquals("I00100", DocWrapper.InvoiceNumber);
		}

		#endregion

		#region TestCurrencyCode

		protected override void TestCurrencyCodeCore()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = "MAT";
			ARInvoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			AssertEquals("MAT", DocWrapper.CurrencyCode);
		}

		#endregion

		#region TestAccountCode

		protected override void TestAccountCodeCore()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTCODE";
			ARInvoice.AH_OH = org.PK;
			AssertEquals("TESTCODE", DocWrapper.AccountCode);
		}

		#endregion

		#region TestFromDate

		protected override void TestFromDateCore()
		{
			ZDateTime date = ZDateTime.Today.AddDays(1);
			WhsInvoice.ET_StorageFromDate = date;
			AssertEquals(date, DocWrapper.FromDate);
		}

		#endregion

		#region TestToDate

		protected override void TestToDateCore()
		{
			ZDateTime date = ZDateTime.Today.AddDays(2);
			WhsInvoice.ET_StorageToDate = date;
			AssertEquals(date, DocWrapper.ToDate);
		}

		#endregion

		#region TestWarehouseName

		protected override void TestWarehouseNameCore()
		{
			WhsWarehouse whs = Factory.New<WhsWarehouse>();
			whs.WW_WarehouseName = "WHS1NAME";
			WhsInvoice.ET_WW = whs.PK;
			AssertEquals("WAREHOUSE:", DocWrapper.WarehouseName.Label);
			AssertEquals("WHS1NAME", DocWrapper.WarehouseName.Value);
		}

		#endregion

		#region  TestReportDesc

		protected override void TestReportDescriptionCore()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			WhsInvoice.ET_OH_Client = org.PK;
			AssertEquals("Breakdown of Invoice charges by Invoice / Job Type. Sorted by Breakdown + Job Date + Reference + Product Code.", DocWrapper.ReportDescription);
		}

		public void TestReportDescription_WithSerialNumberReportSorting_OM_IMInvoiceDetailReportSort()
		{
			var org = Factory.New<OrgHeader>();
			org.MiscServ.OM_IMInvoiceDetailReportSort = JobChargeAttribTypeList.Codes.SerialNumber;
			WhsInvoice.ET_OH_Client = org.PK;

			AssertEquals("Breakdown of Invoice charges by Invoice / Serial Number. Sorted by Breakdown + Job Date + Reference + Product Code.", DocWrapper.ReportDescription);
		}

		public void TestReportDescription_WithSerialNumberReportSorting_OM_IMInvoiceDetailReportSort2()
		{
			var org = Factory.New<OrgHeader>();
			org.MiscServ.OM_IMInvoiceDetailReportSort2 = JobChargeAttribTypeList.Codes.SerialNumber;
			WhsInvoice.ET_OH_Client = org.PK;

			AssertEquals("Breakdown of Invoice charges by Invoice / Job Type / Serial Number. Sorted by Breakdown + Job Date + Reference + Product Code.", DocWrapper.ReportDescription);
		}

		public void TestReportDescription_WithSerialNumberReportSorting_OM_IMInvoiceDetailReportSort3()
		{
			var org = Factory.New<OrgHeader>();
			org.MiscServ.OM_IMInvoiceDetailReportSort3 = JobChargeAttribTypeList.Codes.SerialNumber;
			WhsInvoice.ET_OH_Client = org.PK;

			AssertEquals("Breakdown of Invoice charges by Invoice / Job Type / Serial Number. Sorted by Breakdown + Job Date + Reference + Product Code.", DocWrapper.ReportDescription);
		}

		public void TestReportDescription_WithSerialNumberReportSorting_SerialNumberOnAllInvoiceDetailReportSort()
		{
			var org = Factory.New<OrgHeader>();
			org.MiscServ.OM_IMInvoiceDetailReportSort = JobChargeAttribTypeList.Codes.SerialNumber;
			org.MiscServ.OM_IMInvoiceDetailReportSort2 = JobChargeAttribTypeList.Codes.SerialNumber;
			org.MiscServ.OM_IMInvoiceDetailReportSort3 = JobChargeAttribTypeList.Codes.SerialNumber;
			WhsInvoice.ET_OH_Client = org.PK;

			AssertEquals("Breakdown of Invoice charges by Invoice / Serial Number / Serial Number / Serial Number. Sorted by Breakdown + Job Date + Reference + Product Code.", DocWrapper.ReportDescription);
		}

		#endregion

		#endregion

		#region Overridden Abstract Members

		#region TestWarehouseBOWrapper_FactoryCached

		protected override bool ImplementsWarehouseCore => false;

		#endregion

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WarehousePeriodicInvoiceWrapper(null, (WhsInvoice)bizO, Factory);
		}

		protected override Base.GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			WhsInvoice invoice = Factory.New<WhsInvoice>();
			return new WarehousePeriodicInvoiceWrapper(null, invoice, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CarrierAccount :  is null
CarrierServiceLevel :  is null
Client :  is null
ClientRequestedBillToParty :  is null
CODAmount :  is null
CODType :  is null
ConfirmationInstructions : 
Consignee :  is null
ConsigneeAddress :  is null
Consignor :  is null
CubicSent :  is null
CustomerReference : 
CustomsStatus :  is null
Destination :  is null
DistributionCentreAddress :  is null
DropMode :  is null
DropOffAddress :  is null
FinalisedDate : 
Forwarder :  is null
FulfillRule :  is null
GoodsBillToAddress :  is null
HandlingInstructions : 
IncoTerm :  is null
Insurance :  is null
JobClient :  is null
PackagesSent :  is null
PickingInstructions : 
PickMethod : 
PickNo : 
PickNumberReference : 
PickOption :  is null
PickUpAddress :  is null
PrimaryBarcode : 
Registry : (No Default Field Value Available on Registry)
RequiredDate : 
SalesChannel :  is null
SecondaryReference : 
ServiceLevel :  is null
SOPCarrierServiceLevel : 
SOPOrderNumber : 
SOPRequiredDate : 
SOPSpecialInstructions : 
SOPStagingAreaName : 
SOPTransportCompany : 
SplitNumber : 
StagingAreaName : 
StagingLocationString : 
Status : 
Supplier :  is null
SupplierBuyerLink :  is null
SupplierDocAddress :  is null
TotalExtendedLinePrice :  is null
TotalLoadedPackages : 
TotalLoadedUnits : 
TotalLoadedVolume : 
TotalLoadedWeight : 
TotalOuterPackagesVolume : 
TotalOuterPackagesWeight : 
TransportationUnit :  is null
TransportBillToAddress :  is null
TransportCoAddress :  is null
TransportCompany :  is null
TransportReference : 
VehicleReference : 
Warehouse :  is null
WarehouseName : 
WeightSent :  is null
WhoCreated : 
WhoFinalised :
";
			}
		}

		protected override BusinessObject GetNewWhsBusinessObject()
		{
			return WhsInvoice;
		}

		#endregion

		#region Implementation

		#region Org

		OrgHeader Org
		{
			get { return org ?? (Factory.NewWithValidTestData<OrgHeader>()); }
			set { org = value; }
		}
		OrgHeader org;

		#endregion

		#region ARInvoice

		ARInvoice ARInvoice
		{
			get
			{
				if (arInvoice == null)
				{
					arInvoice = Factory.NewWithValidTestData<ARInvoice>();
					arInvoice.AH_Desc = "INV1";
					arInvoice.AH_OH = Org.PK;
				}
				return arInvoice;
			}
			set { arInvoice = value; }
		}
		ARInvoice arInvoice;

		#endregion

		#region DocARInvoice

		DocARInvoice DocARInvoice
		{
			get
			{
				return DocARInvoice.New(ARInvoice, Factory);
			}
		}

		#endregion

		#region WhsInvoice

		WhsInvoice WhsInvoice
		{
			get
			{
				if (whsinvoice == null)
				{
					whsinvoice = Factory.NewWithValidTestData<WhsInvoice>();
					whsinvoice.ET_StorageJobNumber = "INV1";
				}
				return whsinvoice;
			}
			set { whsinvoice = value; }
		}
		WhsInvoice whsinvoice;

		#endregion

		#region PeriodicInvoiceWrapper

		WarehousePeriodicInvoiceWrapper DocWrapper
		{
			get
			{
				if (docWrapper == null)
				{
					docWrapper = new WarehousePeriodicInvoiceWrapper(DocARInvoice, WhsInvoice, Factory);
					docWrapper.MockInvoicingBase = ARInvoice;
				}
				return docWrapper;
			}
			set { docWrapper = value; }
		}
		WarehousePeriodicInvoiceWrapper docWrapper;

		#endregion

		#endregion
	}
}
