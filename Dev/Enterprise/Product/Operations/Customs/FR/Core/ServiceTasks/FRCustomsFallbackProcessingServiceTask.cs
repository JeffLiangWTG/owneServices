using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.ServiceTasks;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	FRCustomsFallbackProcessingServiceTask.Code,
	FRCustomsFallbackProcessingServiceTask.Description,
	"FRC",
	typeof(FRCustomsFallbackProcessingServiceTask),
	MinimumPeriod = "15Minutes",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.France
	+ "," + Enterprise.Core.Constants.CountryCodes.FrenchGuyana
	+ "," + Enterprise.Core.Constants.CountryCodes.Guadeloupe
	+ "," + Enterprise.Core.Constants.CountryCodes.Martinique
	+ "," + Enterprise.Core.Constants.CountryCodes.Mayotte
	+ "," + Enterprise.Core.Constants.CountryCodes.Reunion
	+ "," + Enterprise.Core.Constants.CountryCodes.SaintMartin
	+ "," + Enterprise.Core.Constants.CountryCodes.SaintBarthelemy,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15Minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(FRCustomsFallbackProcessingServiceTask.Code,
	CusEntryNumSchema.Constants.TableName,
	new[]
	{
		CusEntryNumSchema.Constants.CE_EntryType + "=" + CusEntryHeader.Schema.FallbackEntryType,
		CusEntryNumSchema.Constants.CE_EntryStatus + "=" + DeltaGFallbackStatusList.Codes.PPW,
		CusEntryNumSchema.Constants.CE_RN_NKCountryCode + "=" + Enterprise.Core.Constants.CountryCodes.France,
		CusEntryNumSchema.Constants.CE_Category + "=" + CusEntryNumber.Categories.CustomsPermitClearanceNumber
	},
	"FR Customs Entries awaiting email")]

[assembly: HostedServiceBusinessObjectBinding(FRCustomsFallbackProcessingServiceTask.Code,
	CusEntryNumSchema.Constants.TableName,
	new[]
	{
		CusEntryNumSchema.Constants.CE_EntryType + "=" + CusEntryHeader.Schema.FallbackEntryType,
		CusEntryNumSchema.Constants.CE_EntryStatus + "=" + DeltaGFallbackStatusList.Codes.PPW,
		CusEntryNumSchema.Constants.CE_RN_NKCountryCode + "=" + Enterprise.Core.Constants.CountryCodes.FrenchGuyana,
		CusEntryNumSchema.Constants.CE_Category + "=" + CusEntryNumber.Categories.CustomsPermitClearanceNumber
	},
	"GF Customs Entries awaiting email")]

[assembly: HostedServiceBusinessObjectBinding(FRCustomsFallbackProcessingServiceTask.Code,
	CusEntryNumSchema.Constants.TableName,
	new[]
	{
		CusEntryNumSchema.Constants.CE_EntryType + "=" + CusEntryHeader.Schema.FallbackEntryType,
		CusEntryNumSchema.Constants.CE_EntryStatus + "=" + DeltaGFallbackStatusList.Codes.PPW,
		CusEntryNumSchema.Constants.CE_RN_NKCountryCode + "=" + Enterprise.Core.Constants.CountryCodes.Guadeloupe,
		CusEntryNumSchema.Constants.CE_Category + "=" + CusEntryNumber.Categories.CustomsPermitClearanceNumber
	},
	"GP Customs Entries awaiting email")]

[assembly: HostedServiceBusinessObjectBinding(FRCustomsFallbackProcessingServiceTask.Code,
	CusEntryNumSchema.Constants.TableName,
	new[]
	{
		CusEntryNumSchema.Constants.CE_EntryType + "=" + CusEntryHeader.Schema.FallbackEntryType,
		CusEntryNumSchema.Constants.CE_EntryStatus + "=" + DeltaGFallbackStatusList.Codes.PPW,
		CusEntryNumSchema.Constants.CE_RN_NKCountryCode + "=" + Enterprise.Core.Constants.CountryCodes.Martinique,
		CusEntryNumSchema.Constants.CE_Category + "=" + CusEntryNumber.Categories.CustomsPermitClearanceNumber
	},
	"MQ Customs Entries awaiting email")]

[assembly: HostedServiceBusinessObjectBinding(FRCustomsFallbackProcessingServiceTask.Code,
	CusEntryNumSchema.Constants.TableName,
	new[]
	{
		CusEntryNumSchema.Constants.CE_EntryType + "=" + CusEntryHeader.Schema.FallbackEntryType,
		CusEntryNumSchema.Constants.CE_EntryStatus + "=" + DeltaGFallbackStatusList.Codes.PPW,
		CusEntryNumSchema.Constants.CE_RN_NKCountryCode + "=" + Enterprise.Core.Constants.CountryCodes.Mayotte,
		CusEntryNumSchema.Constants.CE_Category + "=" + CusEntryNumber.Categories.CustomsPermitClearanceNumber
	},
	"YT Customs Entries awaiting email")]

[assembly: HostedServiceBusinessObjectBinding(FRCustomsFallbackProcessingServiceTask.Code,
	CusEntryNumSchema.Constants.TableName,
	new[]
	{
		CusEntryNumSchema.Constants.CE_EntryType + "=" + CusEntryHeader.Schema.FallbackEntryType,
		CusEntryNumSchema.Constants.CE_EntryStatus + "=" + DeltaGFallbackStatusList.Codes.PPW,
		CusEntryNumSchema.Constants.CE_RN_NKCountryCode + "=" + Enterprise.Core.Constants.CountryCodes.Reunion,
		CusEntryNumSchema.Constants.CE_Category + "=" + CusEntryNumber.Categories.CustomsPermitClearanceNumber
	},
	"RE Customs Entries awaiting email")]

[assembly: HostedServiceBusinessObjectBinding(FRCustomsFallbackProcessingServiceTask.Code,
	CusEntryNumSchema.Constants.TableName,
	new[]
	{
		CusEntryNumSchema.Constants.CE_EntryType + "=" + CusEntryHeader.Schema.FallbackEntryType,
		CusEntryNumSchema.Constants.CE_EntryStatus + "=" + DeltaGFallbackStatusList.Codes.PPW,
		CusEntryNumSchema.Constants.CE_RN_NKCountryCode + "=" + Enterprise.Core.Constants.CountryCodes.SaintMartin,
		CusEntryNumSchema.Constants.CE_Category + "=" + CusEntryNumber.Categories.CustomsPermitClearanceNumber
	},
	"MF Customs Entries awaiting email")]

[assembly: HostedServiceBusinessObjectBinding(FRCustomsFallbackProcessingServiceTask.Code,
	CusEntryNumSchema.Constants.TableName,
	new[]
	{
		CusEntryNumSchema.Constants.CE_EntryType + "=" + CusEntryHeader.Schema.FallbackEntryType,
		CusEntryNumSchema.Constants.CE_EntryStatus + "=" + DeltaGFallbackStatusList.Codes.PPW,
		CusEntryNumSchema.Constants.CE_RN_NKCountryCode + "=" + Enterprise.Core.Constants.CountryCodes.SaintBarthelemy,
		CusEntryNumSchema.Constants.CE_Category + "=" + CusEntryNumber.Categories.CustomsPermitClearanceNumber
	},
	"BL Customs Entries awaiting email")]

namespace Enterprise.Customs.FR.ServiceTasks
{
	public class FRCustomsFallbackProcessingServiceTask : CustomsServiceTask
	{
		public const string Code = "FRF";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string Description = "FR Customs Fallback Processor";

		protected override void RunTaskCore(CancellationToken token)
		{
			var factory = new BusinessObjectFactory();

			foreach (var branch in ServiceTaskHelper.GetOneActiveBranchPerCompanies())
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						new PpwFallbackEmailSender(factory, Logger).DoEverything(branch.GB_RN_NKCountryCode);
						new PpsMarkerAsCleared(factory).DoEverything(branch.GB_RN_NKCountryCode);
						new PdsRegulariser(factory, branch, Logger).DoEverything(branch.GB_RN_NKCountryCode);
					});
				}
			}
		}
	}
}
