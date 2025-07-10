#if DEBUG
using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ClientSharedComponents;
using Enterprise.ClientSharedComponents.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.ELG
{
	public class ELGTestHelper : SharedTestHelper
	{
		public ELGTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ELGTestHelper()
			: base()
		{
		}

		#region Set Factory Items
		public OrgHeader AddOrg()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			PopulateOrg(org);
			Orgs.Add(org);
			return org;
		}

		public void PopulateOrg(OrgHeader org)
		{
			org.MainAddress.OA_Address1 = "Unit 3a";
			org.MainAddress.OA_Address2 = "72 O'Riordan Street";
			org.MainAddress.OA_City = "Alexandria";
			org.MainAddress.OA_Phone = "61 2 8001 2200";
			org.MiscServ.OM_ARCreditLimit = 100000m;

			OrgDebtorGroup orgDebtor = Factory.LoadTop1<OrgDebtorGroup>(new ZQuery(OrgDebtorGroupSchema.OJ_Code, "ASC"));
			org.MiscServ.OM_OJ_ARDebtorGroup = orgDebtor.PK;
			org.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromInvoiceDate;
			org.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 30;

			OrgStaffAssignments staffAssignment = org.StaffAssignments.AddNew();
			staffAssignment.O8_GS_NKPersonResponsible = "E";
			org.MiscServ.OM_EXDefaultIncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			org.OH_RL_NKClosestPort = "AUSYD";
			org.ARSettlementGroupPK = Factory.LoadFromUniqueKey<OrgHeader>(OrgHeaderSchema.OH_Code, (ZString)"NYKLIN").PK;
			org.MainAddress.OA_Fax = "61 2 9025 1199";
			org.MainAddress.OA_PostCode = "2015";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_Email = "support@cargowise.com";
			org.MainWebURL.PU_URL = "http://www.cargowise.com";
			org.CompanyData.SetARTaxApplicable(ZBool.False);
			org.PrimaryRegistrationNumber.Number = "41 065 894 724";
			org.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			org.OH_IsDebtor = ZBool.True;
		}

		public void NewPMG()
		{
			FindoOrCreateGroupLink(FindOrCreateUser("testuser123").PK);
		}

		GlbStaff FindOrCreateUser(string userLoginName)
		{
			GlbStaff staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, userLoginName));
			if (staff == null)
			{
				staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_LoginName = userLoginName;
				staff.StaffPlainTextPassword = "helloasl";
				staff.GS_FullName = "William Testuser";
				staff.GS_UserAddress1 = "29 Potsbourgth St";
				staff.GS_City = "Dullton";
				staff.GS_EmailAddress = "william.testuser@loco.con";
			}
			return staff;
		}

		GlbGroupLink FindoOrCreateGroupLink(ZGuid staffPK)
		{
			GlbGroupLink groupLink = Factory.LoadTop1<GlbGroupLink>(new ZQuery(GlbGroupLinkSchema.GK_GS, staffPK));
			if (groupLink == null)
			{
				groupLink = Factory.New<GlbGroupLink>();
				groupLink.GK_GS = staffPK;
				groupLink.GK_GG = GlobalGroup.PK;
			}
			return groupLink;
		}

		GlbGroup GlobalGroup
		{
			get { return globalGroup ?? (globalGroup = Factory.Load<GlbGroup>(rego.SagExportNotifyGroup)); }
		}
		GlbGroup globalGroup;

		public void NewInvoices(String ledger)
		{
			foreach (OrgHeader org in Orgs)
			{
				NewInvoice(org, ledger);
			}
		}

		void NewInvoice(OrgHeader org, String ledger)
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = org.PK;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_Desc = "This is a test description to see how the Export works";
			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			invoice.AH_Ledger = ledger;
			invoice.AH_PostDate = new ZDateTime(2006, 5, 4, 17, 23, 53);
			invoice.AH_InvoiceDate = new ZDateTime(2006, 5, 3, 12, 45, 12);
			invoice.AH_InvoiceTerm = "INV";
			invoice.AH_InvoiceTermDays = 15;
			invoice.AH_DueDate = ZDateTime.Now;
			invoice.AH_TransactionReference = "Shipment ABC123";
			invoice.AH_TransactionNum = org.OH_Code + "0011";
			invoice.AH_TransactionType = "INV";
			invoice.AH_OSTotal = 100m;
			invoice.AH_OSTaxAmount = 20m;
			invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			invoice.AH_JH = FindOrCreateJobHeader(CreateShipment()).PK;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;

			InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			PopulateInvoiceLine(line1, ZArchitecture.Core.Utilities.Round(100.00M / 2, 2), ZArchitecture.Core.Utilities.Round(10.00M / 2, 2), 1M, invoice.PK, invoice.AH_JH);

			InvoicingLineBase line2 = (InvoicingLineBase)invoice.Lines.AddNew();
			PopulateInvoiceLine(line2, ZArchitecture.Core.Utilities.Round(100.00M / 2, 2), ZArchitecture.Core.Utilities.Round(10.00M / 2, 2), 1M, invoice.PK, invoice.AH_JH);
		}

		void PopulateInvoiceLine(InvoicingLineBase line, decimal osExTaxAmount, decimal osTaxAmount, decimal exchangeRate, ZGuid headerPK, ZGuid jobPK)
		{
			sequence++;
			line.AL_AH = headerPK;
			line.AL_Desc = "Transaction Line Description";
			line.AL_ExchangeRate = exchangeRate;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_OSExTaxAmount = osExTaxAmount;
			line.AL_OSTaxAmount = osTaxAmount;
			line.AL_PostDate = new ZDateTime(2006, 5, 4, 17, 23, 53);
			line.AL_AC = FindOrCreateCharge("ZZCC" + sequence).PK;
			line.AL_JH = jobPK;

			TestObjectCreator.CreateJobCharge(line, line.Job, line.ChargeCode, TestObjectCreator.AUD);
			SetValidRegistryTransportAndChargeCodeCollectionItem(Core.Constants.TransportModes.Sea, line.AL_AC, "ABC", "XYZ");
		}
		int sequence;

		ForwardingShipment CreateShipment()
		{
			ForwardingShipment shipment = new BusinessObjectFactory().New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_GoodsDescription = "Goods Description";
			shipment.JS_ActualWeight = 2.33m;
			shipment.DocsAndCartage.JP_OrderItemsAsString = "orderreference,add";
			shipment.Factory.Save();
			return shipment;
		}

		JobHeader FindOrCreateJobHeader(ForwardingShipment shipment)
		{
			JobHeader jobHeader = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
			if (jobHeader == null)
			{
				JobHeader jobHeaderNew = new BusinessObjectFactory().NewJobForTesting<JobHeader>();
				jobHeaderNew.JH_GC = GlbCompany.CurrentCompany.PK;
				jobHeaderNew.JH_GB = GlbBranch.CurrentBranch.PK;
				jobHeaderNew.JH_GE = GlbDepartment.CurrentDepartment.PK;
				jobHeaderNew.JH_ParentID = shipment.PK;
				jobHeaderNew.JH_ParentTableCode = "JS";
				jobHeaderNew.JH_JobNum = shipment.JobNumber;
				jobHeaderNew.Factory.Save();
				jobHeader = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
			}
			return jobHeader;
		}

		public RefCurrency FindCurrency(string currencyCode)
		{
			return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode) ?? GlbBranch.CurrentBranch.Country.LocalCurrency;
		}

		public List<OrgHeader> Orgs
		{
			get { return orgs ?? (orgs = new List<OrgHeader>()); }
		}
		List<OrgHeader> orgs;
		#endregion

		#region Set Registry Items
		#region Set Valid Registry Items
		public override void SetValidRegistryAll()
		{
			SetValidRegistryBranchDepartmentCodeCollectionItem();
			SetValidRegistryDataTransfer(ZDateTime.Now.AddMinutes(-30));
			SetValidRegistryTransportAndChargeCodeCollectionItem();
			SetValidRegistrySageAccountCodeCollectionItem();
		}

		public void setInvalidRegistry(Guid companyPK)
		{
			rego.BranchDepartmentCodeCollectionItem.SetValue(companyPK, Guid.Empty, Guid.Empty, new BranchDepartmentCodeMappingRegistryBusinessObjectCollection());
		}

		public void SetValidRegistrySagDataExportEnabled()
		{
			SetRegistry(Env.TempPath);
		}

		public void SetRegistry(string directory)
		{
			DataTransferSwitchRegistryBusinessObject newValue = new DataTransferSwitchRegistryBusinessObject(Factory);
			newValue.EnableInterface = true;
			newValue.Directory = directory;
			newValue.Interval = 1;
			newValue.UpdateRuns(ZDateTime.Now);
			newValue.GroupPK = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL")).PK;
			DataTransferSwitchRegistryItemTest.UpdateValue(rego.SagDataTransferSwitchRegistryItem, newValue);
		}

		public void SetExportDirectoryForCompany(Guid companyPK, string directory)
		{
			DataTransferSwitchRegistryBusinessObject newValue = new DataTransferSwitchRegistryBusinessObject(Factory);
			newValue.EnableInterface = true;
			newValue.Directory = directory;
			newValue.Interval = 1;
			newValue.UpdateRuns(ZDateTime.Now);
			newValue.GroupPK = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL")).PK;
			rego.SagDataTransferSwitchRegistryItem.SetValue(companyPK, Guid.Empty, Guid.Empty, newValue);
		}

		public void SetEnableInterfaceValueForCompany(Guid companyPK, bool enable, string directory = "")
		{
			DataTransferSwitchRegistryBusinessObject newValue = new DataTransferSwitchRegistryBusinessObject(Factory);
			newValue.EnableInterface = enable;
			if (enable)
			{
				newValue.Directory = directory;
				newValue.Interval = 1;
				newValue.UpdateRuns(ZDateTime.Now);
				newValue.GroupPK = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL")).PK;
			}

			rego.SagDataTransferSwitchRegistryItem.SetValue(companyPK, Guid.Empty, Guid.Empty, newValue);
		}

		public void SetValidRegistryDataTransfer(ZDateTime nextRunDateTime)
		{
			DataTransferSwitchRegistryItemTest.UpdateValue(rego.SagDataTransferSwitchRegistryItem, GetValidDataTransferSwitchRegistryBusinessObject(nextRunDateTime));
		}

		#region BranchDepartmentCodeMapping
		internal void SetValidRegistryBranchDepartmentCodeCollectionItem()
		{
			SetValidRegistryBranchDepartmentCodeCollectionItem("SYD", "BRN", "ABC", "ASD");
			SetValidRegistryBranchDepartmentCodeCollectionItem("BNE", "BRN", "ABC", "ASD");
			SetValidRegistryBranchDepartmentCodeCollectionItem("DEM", "BRN", "ABC", "ASD");
			SetValidRegistryBranchDepartmentCodeCollectionItem("SIN", "BRN", "ABC", "ASD");
		}

		internal void SetValidRegistryBranchDepartmentCodeCollectionItem(string branch, string department, string profitCentre, string nominalDepartment)
		{
			BranchDepartmentCodeMappingRegistryBusinessObjectCollection branchDepartmentCodeMaps = rego.BranchDepartmentCodeCollection;
			BranchDepartmentCodeMappingRegistryBusinessObject branchDepartmentCodeMap = branchDepartmentCodeMaps.AddNew();
			branchDepartmentCodeMap.BranchCodePK = FindOrCreateBranch(branch).PK;
			branchDepartmentCodeMap.DepartmentCodePK = FindOrCreateDepartment(department).PK;
			branchDepartmentCodeMap.ProfitCentre = profitCentre;
			branchDepartmentCodeMap.NominalDepartment = nominalDepartment;
			rego.BranchDepartmentCodeCollectionItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, branchDepartmentCodeMaps);
		}
		#endregion

		#region TransportAndChargeCodeMapping
		internal void SetValidRegistryTransportAndChargeCodeCollectionItem()
		{
			SetValidRegistryTransportAndChargeCodeCollectionItem(Core.Constants.TransportModes.Sea, "FRT", "ABC", "XYZ");
		}

		internal void SetValidRegistryTransportAndChargeCodeCollectionItem(string transportModeCode, string chargeCode, string nominalCostCode, string nominalRevenueCode)
		{
			SetValidRegistryTransportAndChargeCodeCollectionItem(transportModeCode, FindOrCreateCharge(chargeCode).PK, nominalCostCode, nominalRevenueCode);
		}

		internal void SetValidRegistryTransportAndChargeCodeCollectionItem(string transportModeCode, ZGuid chargeCodePK, string nominalCostCode, string nominalRevenueCode)
		{
			TransportAndChargeCodeMappingRegistryBusinessObjectCollection transortAndChargeCodeMaps = rego.TransportAndChargeCodeCollection;
			TransportAndChargeCodeMappingRegistryBusinessObject transortAndChargeCodeMap = transortAndChargeCodeMaps.AddNew();
			transortAndChargeCodeMap.TransportModeCode = transportModeCode;
			transortAndChargeCodeMap.ChargeCodePK = chargeCodePK;
			transortAndChargeCodeMap.NominalCostCode = nominalCostCode;
			transortAndChargeCodeMap.NominalRevenueCode = nominalRevenueCode;
			rego.TransportAndChargeCodeCollectionItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, transortAndChargeCodeMaps);
		}
		#endregion

		#region SageAccountCodeMapping
		internal void SetValidRegistrySageAccountCodeCollectionItem()
		{
			SetValidRegistrySageAccountCodeCollectionItem("DEAENTSYD", Core.Constants.CurrencyCodes.Australia, LedgerTypes.AccountsReceivable, "DEANO.AUD.AR");
			SetValidRegistrySageAccountCodeCollectionItem("ABC", Core.Constants.CurrencyCodes.Australia, LedgerTypes.AccountsReceivable, "ABC.AUD.AR");
			SetValidRegistrySageAccountCodeCollectionItem("ABC", Core.Constants.CurrencyCodes.Australia, LedgerTypes.AccountsPayable, "ABC.AUD.AP");
			SetValidRegistrySageAccountCodeCollectionItem("XYZ", Core.Constants.CurrencyCodes.Australia, LedgerTypes.AccountsReceivable, "XYZ.AUD.AR");
			SetValidRegistrySageAccountCodeCollectionItem("XYZ", Core.Constants.CurrencyCodes.Australia, LedgerTypes.AccountsPayable, "XYZ.AUD.AP");
		}

		internal void SetValidRegistrySageAccountCodeCollectionItem(string orgHeaderCode, string currencyCode, string ledgerType, string sageAccountCode)
		{
			SetValidRegistrySageAccountCodeCollectionItem(FindOrCreateOrgHeader(orgHeaderCode).PK, FindCurrency(currencyCode).PK, ledgerType, sageAccountCode);
		}

		internal void SetValidRegistrySageAccountCodeCollectionItem(ZGuid orgHeaderPK, ZGuid currencyPK, string ledgerType, string sageAccountCode)
		{
			SageAccountCodeMappingRegistryBusinessObjectCollection sageAccountCodeMaps = rego.SageAccountCodeCollection;
			SageAccountCodeMappingRegistryBusinessObject sageAccountCodeMap = sageAccountCodeMaps.AddNew();
			sageAccountCodeMap.RefCurrencyPK = currencyPK;
			sageAccountCodeMap.OrgHeaderPK = orgHeaderPK;
			sageAccountCodeMap.LedgerType = ledgerType;
			sageAccountCodeMap.SageAccountCode = sageAccountCode;
			rego.SageAccountCodeCollectionItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sageAccountCodeMaps);
		}
		#endregion
		#endregion

		#region Set Invalid Registry Items
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		internal void SetInvalidRegistryBranchDepartmentCodeCollectionItem()
		{
			rego.BranchDepartmentCodeCollectionItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BranchDepartmentCodeMappingRegistryBusinessObjectCollection());
		}

		internal void SetInvalidRegistryTransportAndChargeCodeCollectionItem()
		{
			rego.TransportAndChargeCodeCollectionItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new TransportAndChargeCodeMappingRegistryBusinessObjectCollection());
		}

		internal void SetInvalidRegistrySageAccountCodeCollectionItem()
		{
			rego.SageAccountCodeCollectionItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new SageAccountCodeMappingRegistryBusinessObjectCollection());
		}
		#endregion

		static ELGDataRegistry rego
		{
			get { return ELGDataRegistry.Instance; }
		}

		public DataTransferSwitchRegistryBusinessObject GetValidDataTransferSwitchRegistryBusinessObject(ZDateTime nextRunTime)
		{
			DataTransferSwitchRegistryBusinessObject newValue = GetValidDataTransferSwitchRegistryBusinessObject();
			newValue.NextRunDateTime = nextRunTime;
			return newValue;
		}

		public DataTransferSwitchRegistryBusinessObject GetValidDataTransferSwitchRegistryBusinessObject(ZDateTime lastRunTime, ZDateTime nextRunTime)
		{
			DataTransferSwitchRegistryBusinessObject newValue = GetValidDataTransferSwitchRegistryBusinessObject();
			newValue.NextRunDateTime = nextRunTime;
			newValue.LastRunDateTime = lastRunTime;
			return newValue;
		}

		public DataTransferSwitchRegistryBusinessObject GetValidDataTransferSwitchRegistryBusinessObject()
		{
			DataTransferSwitchRegistryBusinessObject newValue = new DataTransferSwitchRegistryBusinessObject(Factory);
			newValue.EnableInterface = true;
			newValue.Directory = Env.TempPath;
			newValue.Interval = 1;
			newValue.UpdateRuns(ZDateTime.Now);
			newValue.GroupPK = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL")).PK;
			return newValue;
		}
		#endregion

		internal static ZString LastMessage
		{
			get { return UnitTestUserNotification.Instance.LastMessage.Text; }
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
	}
}
#endif
