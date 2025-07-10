using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.CH.DeclarationActivation.Business;

[SystemDefinedValues]
public class DeclarationActivationReport(BusinessObjectFactory factory, DataRow row) : CusExitReport(factory, row), IDateOfValuationProvider
{
	public new class Schema : AutoCusExitReport.Schema
	{
		public new const int CER_OfficeOfExportMaxLength = 8;
		public new const int CER_LocationMaxLength = 12;
		public new const int CER_TransportIDMaxLength = 35;
		public new const int CER_TransportTypeMaxLength = 2;
		public const string CommunicationLanguage = nameof(DeclarationActivationReport.CommunicationLanguage);
		public const int CommunicationLanguageMaxLength = 2;
		public const string EdecOriginalTraderUID = nameof(DeclarationActivationReport.EdecOriginalTraderUID);
		public const int EdecOriginalTraderUIDMaxLength = 17;
		public const string NextProcedure = nameof(DeclarationActivationHeader.NextProcedure);
		public const int NextProcedureMaxLength = 35;
	}

	[List(nameof(Lookups) + "." + nameof(Lookups.TransportModeList))]
	public override ZString CER_TransportMode { get => base.CER_TransportMode; set => base.CER_TransportMode = value; }

	[List(nameof(Lookups) + "." + nameof(Lookups.DeclarationTimeCodeList))]
	public override ZString CER_AdditionalDeclarationType { get => base.CER_AdditionalDeclarationType; set => base.CER_AdditionalDeclarationType = value; }

	[List(nameof(Lookups) + "." + nameof(Lookups.CustomsOfficeList))]
	[MaxLength(Schema.CER_OfficeOfExportMaxLength)]
	public override ZString CER_OfficeOfExport { get => base.CER_OfficeOfExport; set => base.CER_OfficeOfExport = value; }

	[MaxLength(Schema.CER_LocationMaxLength)]
	[List(nameof(Lookups) + "." + nameof(Lookups.AuthorizationsList))]
	public override ZString CER_Location { get => base.CER_Location; set => base.CER_Location = value; }

	[MaxLength(Schema.CER_TransportIDMaxLength)]
	public override ZString CER_TransportID { get => base.CER_TransportID; set => base.CER_TransportID = value; }

	[MaxLength(Schema.CER_TransportTypeMaxLength)]
	[List(nameof(Lookups) + "." + nameof(Lookups.TransportTypeList))]
	public override ZString CER_TransportType { get => base.CER_TransportType; set => base.CER_TransportType = value; }

	[List(nameof(Lookups) + "." + nameof(Lookups.ActivationTypeList))]
	public override ZString CER_Type { get => base.CER_Type; set => base.CER_Type = value; }

	public ZString TypeDescription => Lookups.ActivationTypeList.GetDescriptionFromCode(CER_Type);

	[List(nameof(Lookups) + "." + nameof(Lookups.MessageStatusList))]
	[ReadOnly(true)]
	public override ZString CER_MessageStatus { get => base.CER_MessageStatus; set => base.CER_MessageStatus = value; }

	public ZString MessageStatusDescription => Lookups.MessageStatusList.GetDescriptionFromCode(CER_MessageStatus);

	[List(nameof(Lookups) + "." + nameof(Lookups.CustomsStatusList))]
	[ReadOnly(true)]
	public override ZString CER_Status { get => base.CER_Status; set => base.CER_Status = value; }

	public ZString CustomsStatusDescription => Lookups.CustomsStatusList.GetDescriptionFromCode(CER_Status);

	[List(nameof(Lookups) + "." + nameof(Lookups.NextProcedureList))]
	[MaxLength(Schema.NextProcedureMaxLength)]
	public ZString NextProcedure
	{
		get => this.GetSystemDefinedValue<ZString>(GenAddOnHelper.NextProcedure);
		set
		{
			if (this.NextProcedure != value)
			{
				CheckMaximumLength(NextProcedureInfo, value);
				this.SetSystemDefinedValue(GenAddOnHelper.NextProcedure, value);
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateNextProcedure();
			}
			NextProcedureInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo NextProcedureInfo => GetZPropertyInfo(Schema.NextProcedure);

	public ZString NextProcedureDescription => Lookups.NextProcedureList.GetDescriptionFromCode(NextProcedure);

	[List(nameof(Lookups) + "." + nameof(Lookups.CommunicationLanguageList))]
	[MaxLength(Schema.CommunicationLanguageMaxLength)]
	public ZString CommunicationLanguage
	{
		get => this.GetSystemDefinedValue<ZString>(GenAddOnHelper.CommunicationLanguage);
		set
		{
			if (this.CommunicationLanguage != value)
			{
				CheckMaximumLength(CommunicationLanguageInfo, value);
				this.SetSystemDefinedValue(GenAddOnHelper.CommunicationLanguage, value);
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateCommunicationLanguage();
			}
			CommunicationLanguageInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo CommunicationLanguageInfo => GetZPropertyInfo(Schema.CommunicationLanguage);

	[MaxLength(Schema.EdecOriginalTraderUIDMaxLength)]
	public ZString EdecOriginalTraderUID
	{
		get => this.GetSystemDefinedValue<ZString>(GenAddOnHelper.EdecOriginalTraderUID);
		set
		{
			if (EdecOriginalTraderUID != value)
			{
				CheckMaximumLength(EdecOriginalTraderUIDInfo, value);
				this.SetSystemDefinedValue(GenAddOnHelper.EdecOriginalTraderUID, value);
			}
			EdecOriginalTraderUIDInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo EdecOriginalTraderUIDInfo => GetZPropertyInfo(nameof(EdecOriginalTraderUID));

	public ZDateTime DateOfValuation => ZDateTime.Now;

	public bool IsEdecActivation => CER_Type == ActivationTypeList.Codes.Edec;

	public new DeclarationActivationReportLookups Lookups => (DeclarationActivationReportLookups)base.Lookups;

	override protected CusExitReportLookups GetNewLookups() => new DeclarationActivationReportLookups(this);

	new DeclarationActivationReportValidation Validation => (DeclarationActivationReportValidation)base.Validation;

	override protected CusExitReportValidation GetNewValidation() => new DeclarationActivationReportValidation(this);
}
