using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.DeclarationActivation.Business;

public class DeclarationActivationHeader(BusinessObjectFactory factory, DataRow row) : CusExitHeader(factory, row), IDateOfValuationProvider
{
	public new class Schema : AutoCusExitHeader.Schema
	{
		public new const int CXH_JobReferenceMaxLength = 11;
		public const string JobReference = nameof(DeclarationActivationHeader.CXH_JobReference);
		public const string OwnerReference = nameof(DeclarationActivationHeader.CXH_OwnerReference);
		public const string MessageStatusDescription = nameof(DeclarationActivationHeader.MessageStatusDescription);
	}

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|CXH_OwnerReference", Caption = "Owners Reference", ShortCaption = "Owners Ref.")]
	[MaxLength(Schema.CXH_OwnerReferenceMaxLength)]
	public override ZString CXH_OwnerReference { get => base.CXH_OwnerReference; set => base.CXH_OwnerReference = value; }

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|CXH_OH_Exporter", Caption = "Exporter/Client", ShortCaption = "Exporter/Cli.")]
	public override ZGuid CXH_OH_Exporter
	{
		get => base.CXH_OH_Exporter;
		set
		{
			var oldValue = CXH_OH_Exporter;
			base.CXH_OH_Exporter = value;
			if (!IsCopying && oldValue != value)
			{
				SetDefaultEdecOriginalTraderUID();
			}
		}
	}

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|ExporterCode", Caption = "Exporter/Client Code", ShortCaption = "Exporter/Cli. Code")]
	public ZString ExporterCode => Exporter?.OH_Code ?? ZString.Empty;

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|ExporterName", Caption = "Exporter/Client Name", ShortCaption = "Exporter/Cli. Name")]
	public ZString ExporterName => Exporter?.OH_FullName ?? ZString.Empty;

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|CXH_GS_NKCustomsAgent", Caption = "Broker")]
	[List(nameof(Lookups) + "." + nameof(Lookups.CustomsAgents))]
	public override ZString CXH_GS_NKCustomsAgent { get => base.CXH_GS_NKCustomsAgent; set => base.CXH_GS_NKCustomsAgent = value; }

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|CXH_JobReference", Caption = "Job Number", ShortCaption = "Job #")]
	public override ZString CXH_JobReference { get => base.CXH_JobReference; set => base.CXH_JobReference = value; }

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|CER_TransportMode", Caption = "Transport Mode", ShortCaption = "Trans. Mode")]
	[List(nameof(Report) + "." + nameof(DeclarationActivationReport.Lookups) + "." + nameof(DeclarationActivationReportLookups.TransportModeList))]
	public ZString CER_TransportMode { get => Report.CER_TransportMode; set => report.CER_TransportMode = value; }

	public ZPropertyInfo CER_TransportModeInfo => GetWrappedZPropertyInfo(nameof(CER_TransportMode), _ => Report.CER_TransportModeInfo);

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|CER_TransportType", Caption = "Type of ID", ShortCaption = "Typ. of ID")]
	[List(nameof(Report) + "." + nameof(DeclarationActivationReport.Lookups) + "." + nameof(DeclarationActivationReportLookups.TransportTypeList))]
	public ZString CER_TransportType { get => Report.CER_TransportType; set => Report.CER_TransportType = value; }

	public ZPropertyInfo CER_TransportTypeInfo => GetWrappedZPropertyInfo(nameof(CER_TransportType), _ => Report.CER_TransportTypeInfo);

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|CER_OfficeOfExport", Caption = "Customs Office", ShortCaption = "Customs Off.")]
	[List(nameof(Report) + "." + nameof(DeclarationActivationReport.Lookups) + "." + nameof(DeclarationActivationReportLookups.CustomsStatusList))]
	public ZString CER_OfficeOfExport { get => Report.CER_OfficeOfExport; set => Report.CER_OfficeOfExport = value; }

	public ZPropertyInfo CER_OfficeOfExportInfo => GetWrappedZPropertyInfo(nameof(CER_OfficeOfExport), _ => Report.CER_OfficeOfExportInfo);

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|CER_Location", Caption = "Goods Location", ShortCaption = "Goods Loc.")]
	[List(nameof(Report) + "." + nameof(DeclarationActivationReport.Lookups) + "." + nameof(DeclarationActivationReportLookups.AuthorizationsList))]
	public ZString CER_Location { get => Report.CER_Location; set => Report.CER_Location = value; }

	public ZPropertyInfo CER_LocationInfo => GetWrappedZPropertyInfo(nameof(CER_Location), _ => Report.CER_LocationInfo);

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|CER_RN_NKTransportNationality", Caption = "Nationality", ShortCaption = "Nat.")]
	public ZString CER_RN_NKTransportNationality { get => Report.CER_RN_NKTransportNationality; set => Report.CER_RN_NKTransportNationality = value; }

	public ZPropertyInfo CER_RN_NKTransportNationalityInfo => GetWrappedZPropertyInfo(nameof(CER_RN_NKTransportNationality), _ => Report.CER_RN_NKTransportNationalityInfo);

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|CER_TransportID", Caption = "Transport ID", ShortCaption = "Trans. ID")]
	public ZString CER_TransportID { get => Report.CER_TransportID; set => Report.CER_TransportID = value; }

	public ZPropertyInfo CER_TransportIDInfo => GetWrappedZPropertyInfo(nameof(CER_TransportID), _ => Report.CER_TransportIDInfo);

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|CER_Type", Caption = "Activation Type", ShortCaption = "Act. Type")]
	[List(nameof(Report) + "." + nameof(DeclarationActivationReport.Lookups) + "." + nameof(DeclarationActivationReportLookups.ActivationTypeList))]
	public ZString CER_Type { get => Report.CER_Type; set => Report.CER_Type = value; }

	public ZPropertyInfo CER_TypeInfo => GetWrappedZPropertyInfo(nameof(CER_Type), _ => Report.CER_TypeInfo);

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|TypeDescription", Caption = "Activation Type Description", ShortCaption = "Act. Type Desc.")]
	public ZString TypeDescription => Report.TypeDescription;

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationReport|CER_AdditionalDeclarationType", Caption = "Declaration Time Code", ShortCaption = "Decl. Time Code")]
	[List(nameof(Report) + nameof(Report.Lookups) + "." + nameof(DeclarationActivationReportLookups.DeclarationTimeCodeList))]
	public ZString CER_AdditionalDeclarationType { get => Report.CER_AdditionalDeclarationType; set => Report.CER_AdditionalDeclarationType = value; }

	public ZPropertyInfo CER_AdditionalDeclarationTypeInfo => GetWrappedZPropertyInfo(nameof(CER_AdditionalDeclarationType), _ => Report.CER_AdditionalDeclarationTypeInfo);

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|CER_MessageStatus", Caption = "Message Status", ShortCaption = "Msg. Status")]
	public ZString CER_MessageStatus => Report.CER_MessageStatus;

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|MessageStatusDescription", Caption = "Message Status Description", ShortCaption = "Msg. Status Desc.")]
	public ZString MessageStatusDescription => Report.MessageStatusDescription;

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|CER_Status", Caption = "Customs Status", ShortCaption = "Cust. Status")]
	public ZString CER_Status => Report.CER_Status;

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|CustomsStatusDescription", Caption = "Customs Status Description", ShortCaption = "Cust. Status Desc.")]
	public ZString CustomsStatusDescription => Report.CustomsStatusDescription;

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|MovementReference", Caption = "Goods Declaration Ref. Nr.", ShortCaption = "GDRN")]
	public ZString CXC_MovementReference { get => Consignment.CXC_MovementReference; set => Consignment.CXC_MovementReference = value; }

	public ZPropertyInfo MovementReferenceInfo => GetWrappedZPropertyInfo(nameof(CXC_MovementReference), _ => Consignment.CXC_MovementReferenceInfo);

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|CXC_ReferenceNumber", Caption = "Air Waybill")]
	public ZString CXC_ReferenceNumber { get => Consignment.CXC_ReferenceNumber; set => Consignment.CXC_ReferenceNumber = value; }

	public ZPropertyInfo CXC_ReferenceNumberInfo => GetWrappedZPropertyInfo(nameof(CXC_ReferenceNumber), _ => Consignment.CXC_ReferenceNumberInfo);

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|NextProcedure", Caption = "Next Procedure", ShortCaption = "Next Proc.")]
	public ZString NextProcedure { get => Report.NextProcedure; set => Report.NextProcedure = value; }

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|NextProcedureDescription", Caption = "Next Procedure Description", ShortCaption = "Next Proc. Desc.")]
	public ZString NextProcedureDescription => Report.NextProcedureDescription;

	public ZPropertyInfo NextProcedureInfo => GetWrappedZPropertyInfo(nameof(NextProcedure), _ => Report.NextProcedureInfo);

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|CommunicationLanguage", Caption = "Language", ShortCaption = "Lang.")]
	public ZString CommunicationLanguage { get => Report.CommunicationLanguage; set => Report.CommunicationLanguage = value; }

	public ZPropertyInfo CommunicationLanguageInfo => GetWrappedZPropertyInfo(nameof(CommunicationLanguage), _ => Report.CommunicationLanguageInfo);

	[ResourceStringData("CH.DeclarationActivation.DeclarationActivationHeader|EdecOriginalTraderUID", Caption = "Original Trader UID", ShortCaption = "Orig. Trader UID")]
	public ZString EdecOriginalTraderUID { get => Report.EdecOriginalTraderUID; set => Report.EdecOriginalTraderUID = value; }

	public ZPropertyInfo EdecOriginalTraderUIDInfo => GetWrappedZPropertyInfo(nameof(EdecOriginalTraderUID), _ => Report.EdecOriginalTraderUIDInfo);

	public DeclarationActivationConsignment Consignment
	{
		get
		{
			if (consignment == null)
			{
				consignment = LoadOrCreateConsignment(true);
			}
			return consignment;
		}
	}
	DeclarationActivationConsignment consignment;

	DeclarationActivationConsignment LoadOrCreateConsignment(bool createIfNotExists)
	{
		var query = new ZQuery(CusExitConsignmentSchema.CXC_CXH_Header, PK);
		if (!IsDeleted)
		{
			query.AddToFilter(CusExitConsignmentSchema.CXC_ClusterKey, CXH_ClusterKey);
		}
		consignment = Factory.LoadTop1<DeclarationActivationConsignment>(query);
		if (consignment == null && createIfNotExists)
		{
			consignment = Factory.New<DeclarationActivationConsignment>();
			consignment.CXC_CXH_Header = PK;
			RegisterEditableChildObject(consignment);
		}
		return consignment;
	}

	public DeclarationActivationReport Report
	{
		get
		{
			if (report == null)
			{
				report = LoadOrCreateReport(true);
			}
			return report;
		}
	}
	DeclarationActivationReport report;

	DeclarationActivationReport LoadOrCreateReport(bool createIfNotExists)
	{
		var query = new ZQuery(CusExitReportSchema.CER_CXH_Header, PK);
		if (!IsDeleted)
		{
			query.AddToFilter(CusExitReportSchema.CER_ClusterKey, CXH_ClusterKey);
		}
		report = Factory.LoadTop1<DeclarationActivationReport>(query);
		if (report == null && createIfNotExists)
		{
			report = Factory.New<DeclarationActivationReport>();
			report.CER_CXH_Header = PK;
			report.CER_CXC_Consignment = Consignment.PK;
			RegisterEditableChildObject(report);
		}
		return report;
	}

	public override void OnSaving()
	{
		base.OnSaving();
		PopulateJobNumber();
	}

	protected override void OnSavingForDelete()
	{
		base.OnSavingForDelete();
		if (LoadOrCreateConsignment(false) != null)
		{
			Consignment.Delete();
		}
		if (LoadOrCreateReport(false) != null)
		{
			Report.Delete();
		}
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CXH_ApplicationCode = CusExitHeaderApplicationCodeList.Codes.CHDeclarationActivation;
		CXH_GS_NKCustomsAgent = GlbStaff.CurrentUser.GS_Code;
		SetDefaultEdecOriginalTraderUID();
	}

	void SetDefaultEdecOriginalTraderUID()
	{
		if (Report.EdecOriginalTraderUID.IsEmpty)
		{
			Report.EdecOriginalTraderUID = Exporter.GetUIDNumber();
		}
	}

	void PopulateJobNumber()
	{
		PopulateNumberPropertyIfRequired(CXH_JobReferenceInfo,
			x => (ZString)Env.NumberFountains.CHDeclarationActivationJobNumber().GetNextFormatted(Factory));
	}

	protected override CusExitHeaderValidation GetNewValidation() => new DeclarationActivationHeaderValidation(this);

	new public DeclarationActivationHeaderValidation Validation => (DeclarationActivationHeaderValidation)base.Validation;

	public ZDateTime DateOfValuation => Report.DateOfValuation;

	protected override ZString HumanReadableNameCore => Res.GetString("Enterprise.Customs.CH.DeclarationActivation.DeclarationActivationHeader|HumanReadableName", "Declaration Activation");

	protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new DeclarationActivationHeaderFetchStrategy(this);
}
