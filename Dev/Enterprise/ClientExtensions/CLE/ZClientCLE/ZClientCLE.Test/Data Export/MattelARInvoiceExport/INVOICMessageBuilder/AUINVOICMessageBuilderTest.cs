using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.CLE.MattelARInvoiceExport.Testing
{
	public class AUINVOICMessageBuilderTest : INVOICMessageBuilderTest
	{
		protected override INVOICMessageBuilder MesgBuilder
		{
			get
			{
				return new AUINVOICMessageBuilder(InvoiceRecord, Factory);
			}
		}

		protected override string ExpectedResult
		{
			get
			{
				return "AUINV_200612121212_JOBDEC.EDI";
			}
		}

		protected override ZString Dest
		{
			get
			{
				return "AUSYD";
			}
		}

		protected override ZString Origin
		{
			get
			{
				return "NZAKL";
			}
		}

		protected override BaseJobDeclaration SetupDeclaration()
		{
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			jobDec.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			jobDec.DisableDefaultPackingInformation = true;
			jobDec.JE_OH_Supplier = Supplier.PK;
			jobDec.JE_OH_Importer = Importer.PK;
			jobDec.JE_MasterBill = "MasterBill";
			jobDec.JE_HouseBill = "HouseBill";
			jobDec.JE_RL_NKFinalDestination = "AUSYD";
			jobDec.JE_RL_NKOrigin = "NZAKL";
			jobDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			jobDec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			jobDec.JE_OwnerRef = "OwnerRef";
			jobDec.JE_TotalVolume = 20.25m;
			jobDec.JE_VoyageFlightNo = "Voyage";
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Vessel";
			vessel.RV_LloydsNumber = "Lloyds";
			vessel.RV_VesselType = Core.Constants.VesselType.CargoVessel;
			jobDec.JE_VesselName = vessel.RV_Code;
			//SetupContainer
			RefContainer cont40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			CusContainer container1 = jobDec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "Container1";
			container1.CO_RC = cont40GP.PK;
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			RefContainer cont20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			CusContainer container2 = jobDec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "Container2";
			container2.CO_RC = cont20GP.PK;
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			Customs.Business.Bill mBill = jobDec.Bills.FindByBillNumberAndType(Consol.JK_MasterBillNum, BillTypeList.Codes.MasterBill);
			if (mBill == null)
			{
				mBill = jobDec.Bills.AddNew();
				mBill.CU_BillType = BillTypeList.Codes.MasterBill;
				mBill.CU_BillNum = Consol.JK_MasterBillNum;
			}

			Customs.Business.Bill hBill = jobDec.Bills.AddNew();
			hBill.CU_HouseBill = Shipment.JS_HouseBill;
			hBill.CU_MasterBill = Consol.JK_MasterBillNum;
			Customs.AU.Declaration.Business.CusEntryHeader cusEntry = jobDec.CustomsEntryHeaders.AddNew();
			cusEntry.Charges.AddNew(CusEntryChargeTypeList.Codes.DutyAmount, 100m);
			Customs.AU.Declaration.Business.CusEntryLine entryLine1 = cusEntry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			Customs.AU.Declaration.Business.CusEntryLine entryLine2 = cusEntry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			//Setup Job Commercial Invoice
			JobComInvoiceHeader invoiceHeader = jobDec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			invoiceHeader.JZ_CU_RelatedHouseBill = hBill.PK;
			JobComInvoiceLine line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_Description = "Shoes";
			line1.JI_PartNo = "PartNo1";
			line1.JI_LinePrice = 200m;
			line1.JI_CL = entryLine1.PK;
			JobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_Description = "Pencils";
			line2.JI_PartNo = "PartNo2";
			line2.JI_LinePrice = 500m;
			line2.JI_CL = entryLine2.PK;
			jobDec.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();
			JobHeader header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentID = jobDec.PK;
			header.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_JobNum = jobDec.JE_DeclarationReference;
			Factory.Save();
			ARInvoice dSBInvoice = null;
			Action<ARInvoice> extraSetupDSB = (x) =>
			{
				CreateInvoiceLine(x, header.PK, 1000m, 10m, ChargeCodeDisbursementDuty.PK);
				CreateInvoiceLine(x, header.PK, 233m, 2.33m, Env.Registry.FreightChargeCode);
				x.AH_OutstandingAmount = x.AH_InvoiceAmount + x.AH_GSTAmount;
			};
			dSBInvoice = CreateInvoice(header.PK, true, Importer.PK, "00DSBINV", false, extraSetupDSB);
			Factory.Save();
			ARInvoice finalInvoice = null;
			Action<ARInvoice> extraSetupFinal = (x) =>
			{
				CreateInvoiceLine(x, header.PK, 1000m, 100m, Env.Registry.FreightChargeCode);
				x.AH_OutstandingAmount = x.AH_InvoiceAmount + x.AH_GSTAmount;
			};
			finalInvoice = CreateInvoice(header.PK, false, Importer.PK, "00FINALINV", false, extraSetupFinal);
			Factory.Save();
			ARInvoices = new InvoicingBase[] { dSBInvoice, finalInvoice };
			return jobDec;
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("AU");
			SetupAURegistryForCustomsChargeAccountCodes();
		}

		EntryChargeTypeList SetupAURegistryForCustomsChargeAccountCodes()
		{
			EntryChargeTypeList chargeTypeList = new CusEntryChargeTypeList();
			EntryChargeTypeSettingCollection chargeTypeSettings = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			EntryChargeTypeSetting chargeTypeSettingDuty = chargeTypeSettings.AddNew();
			chargeTypeSettingDuty.ChargeType = CusEntryChargeTypeList.Codes.DutyAmount;
			chargeTypeSettingDuty.AC_ChargeCode = ChargeCodeDisbursementDuty.PK;
			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeTypeSettings);
			return chargeTypeList;
		}
	}
}
