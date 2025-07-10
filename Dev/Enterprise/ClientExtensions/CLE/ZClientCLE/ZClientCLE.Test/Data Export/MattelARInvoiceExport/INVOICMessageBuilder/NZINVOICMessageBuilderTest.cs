using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.FormalEntry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using CoreCustoms = Enterprise.Customs.Business;
using NZ = Enterprise.Customs.NZ.Registry;

namespace Enterprise.Client.CLE.MattelARInvoiceExport.Testing
{
	public class NZINVOICMessageBuilderTest : INVOICMessageBuilderTest
	{
		public void TestExportingBuyerConsol()
		{
			NZINVOICMessageBuilderForTest builder = new NZINVOICMessageBuilderForTest(InvoiceRecord, Factory);
			AssertEquals("Preconditions: consol is not a buyer consol", true, !Shipment.ArrivalConsol.IsBuyersConsol);
			JobComInvoiceHeader invoice = JobDec.Invoices[0];
			AssertEquals("invoice is not link to a particular bill", true, invoice.JZ_CU_RelatedHouseBill.IsEmpty);
			AssertEquals("IsJobComInvoiceLinkedToShipment is true", true, builder.IsJobComInvoiceLinkedToShipment(invoice));
			Shipment.ArrivalConsol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			Factory.Save();
			AssertEquals("Preconditions: consol is a buyer consol", true, Shipment.ArrivalConsol.IsBuyersConsol);
			AssertEquals("IsJobComInvoiceLinkedToShipment is false", false, builder.IsJobComInvoiceLinkedToShipment(invoice));
			Bill hBill = JobDec.Bills.FindByBillNumberAndType(Shipment.JS_HouseBill, BillTypeList.Codes.HouseBill);
			invoice.JZ_CU_RelatedHouseBill = hBill.PK;
			Factory.Save();
			AssertEquals("IsJobComInvoiceLinkedToShipment is true", true, builder.IsJobComInvoiceLinkedToShipment(invoice));
		}

		protected override INVOICMessageBuilder MesgBuilder
		{
			get
			{
				return new NZINVOICMessageBuilder(InvoiceRecord, Factory);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("NZ");
			SetupNZRegistryForCustomsChargeAccountCodes();
		}

		protected JobDeclaration JobDec;
		protected override string ExpectedResult
		{
			get
			{
				return "NZINV_200612121212_JOBDEC.EDI";
			}
		}

		protected override ZString Origin
		{
			get
			{
				return "AUSYD";
			}
		}

		protected override ZString Dest
		{
			get
			{
				return "NZAKL";
			}
		}

		protected override CoreCustoms.BaseJobDeclaration SetupDeclaration()
		{
			JobDec = Factory.New<JobDeclaration>();
			JobDec.DisableDefaultPackingInformation = true;
			JobDec.JE_OH_Supplier = Supplier.PK;
			JobDec.JE_OH_Importer = Importer.PK;
			JobDec.JE_MasterBill = "MasterBill";
			JobDec.JE_HouseBill = "HouseBill";
			JobDec.JE_RL_NKFinalDestination = "NZAKL";
			JobDec.JE_RL_NKOrigin = "AUSYD";
			JobDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			JobDec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			JobDec.JE_OwnerRef = "OwnerRef";
			JobDec.JE_TotalVolume = 20.25m;
			JobDec.JE_VoyageFlightNo = "Voyage";
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Vessel";
			vessel.RV_LloydsNumber = "Lloyds";
			vessel.RV_VesselType = Core.Constants.VesselType.CargoVessel;
			JobDec.JE_VesselName = vessel.RV_Code;
			Bill mBill = JobDec.Bills.FindByBillNumberAndType(Consol.JK_MasterBillNum, BillTypeList.Codes.MasterBill);
			if (mBill == null)
			{
				mBill = JobDec.Bills.AddNew();
				mBill.CU_BillType = BillTypeList.Codes.MasterBill;
				mBill.CU_BillNum = Consol.JK_MasterBillNum;
			}

			Bill hBill = JobDec.Bills.AddNew();
			hBill.CU_BillType = BillTypeList.Codes.HouseBill;
			hBill.CU_HouseBill = Shipment.JS_HouseBill;
			hBill.CU_MasterBill = Consol.JK_MasterBillNum;
			Factory.Save();
			//Setup Job Commercial Invoice
			JobComInvoiceHeader invoiceHeader = JobDec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			JobComInvoiceLine line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_Description = "Shoes";
			line1.JI_PartNo = "PartNo1";
			line1.JI_LinePrice = 200m;
			JobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_Description = "Pencils";
			line2.JI_PartNo = "PartNo2";
			line2.JI_LinePrice = 500m;
			JobHeader header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentID = JobDec.PK;
			header.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_JobNum = JobDec.JE_DeclarationReference;
			Factory.Save();
			Action<ARInvoice> extraActionDSB = (x) =>
			{
				CreateInvoiceLine(x, header.PK, 1000m, 10m, ChargeCodeDisbursementDuty.PK);
				CreateInvoiceLine(x, header.PK, 233m, 2.33m, Env.Registry.FreightChargeCode);
			};
			ARInvoice dSBInvoice = CreateInvoice(header.PK, true, Importer.PK, "00DSBINV", false, extraActionDSB);
			Factory.Save();
			Action<ARInvoice> extraActionFinal = (x) =>
			{
				CreateInvoiceLine(x, header.PK, 1000m, 100m, Env.Registry.FreightChargeCode);
			};
			ARInvoice finalInvoice = CreateInvoice(header.PK, false, Importer.PK, "00FINALINV", false, extraActionFinal);
			ARInvoices = new InvoicingBase[] { dSBInvoice, finalInvoice };
			LineMerger merger = new LineMerger(JobDec);
			merger.DoMerge();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(JobDec.MergeManager);
			AssertNotNull("Entry Header shouldn't be null", JobDec.CustomsEntryHeaders[0]);
			CusEntryLine entryLine1 = (CusEntryLine)JobDec.CustomsEntryHeaders[0].MergedLines.FindByPK(line1.JI_CL.ToGuid());
			entryLine1.Fees.AddOrUpdate(NZ.EntryChargeTypeList.Codes.Duty, 100m);
			entryLine1.CL_DutyPercent = 5m;
			Factory.Save();
			return JobDec;
		}

		EntryChargeTypeList SetupNZRegistryForCustomsChargeAccountCodes()
		{
			EntryChargeTypeList chargeTypeList = new NZ.EntryChargeTypeList();
			EntryChargeTypeSettingCollection chargeTypeSettings = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			EntryChargeTypeSetting chargeTypeSettingDuty = chargeTypeSettings.AddNew();
			chargeTypeSettingDuty.ChargeType = NZ.EntryChargeTypeList.Codes.Duty;
			chargeTypeSettingDuty.AC_ChargeCode = ChargeCodeDisbursementDuty.PK;
			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeTypeSettings);
			return chargeTypeList;
		}

		class NZINVOICMessageBuilderForTest : NZINVOICMessageBuilder
		{
			public NZINVOICMessageBuilderForTest(JobInvoiceRecord invoiceRecord, BusinessObjectFactory factory) : base(invoiceRecord, factory)
			{
			}

			public new bool IsJobComInvoiceLinkedToShipment(CoreCustoms.BaseJobComInvoiceHeader invoice)
			{
				return base.IsJobComInvoiceLinkedToShipment(invoice);
			}
		}
	}
}
