using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ResString = Enterprise.Licensing.Module.ResString;

namespace Enterprise.Licencing.Module
{
	public class LicenceUsageFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			var usageTimeUTC = result.AddDateFilter("Usage Time UTC", StmActivityLogSchema.S7_OpenDateTimeUtc, false);
			usageTimeUTC.MultilingualDescription = ResString.GetMultilingualString("Licencing|LicenceUsageFilter|UsageTime", "Usage Time UTC");
			usageTimeUTC.UserEntersUtcValue = true;
			result.AddDateFilter("Usage Time (Local)", StmActivityLogSchema.S7_OpenDateTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("Licencing|LicenceUsageFilter|LocalUsageTime", "Usage Time (Local)");
			result.AddNkFilter("Staff", StmActivityLogSchema.S7_GS_NKUser, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("Licencing|LicenceUsageFilter|Staff", "Staff");
			result.AddTextFilter("Module", StmActivityLogSchema.S7_FormCaption, LicenceModuleList).MultilingualDescription = ResString.GetMultilingualString("Licencing|LicenceUsageFilter|Module", "Module");
			var licenceTypeFilter = result.AddTextFilter("Licence Type", GetLicenceTypeQuery, LicenceTypes);
			licenceTypeFilter.MaxLength = 3; //Maximum length of ModuleLicenceType
			licenceTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Licencing|LicenceUsageFilter|LicenceType", "License Type");
			licenceTypeFilter.ErrorOnCodeNotPresent = true;
			result.AddGuidFilter("Branch", ModuleIDs.GlbBranch, StmActivityLogSchema.S7_ParentID, new GlbBranchDependentCollection(GlbCompany.CurrentCompany, Factory)).MultilingualDescription = ResString.GetMultilingualString("Licencing|LicenceUsageFilter|Branch", "Branch");
			return result;
		}

		ZQuery GetLicenceTypeQuery(ZString input)
		{
			return new ZQuery(StmActivityLogSchema.S7_MouseClicks, (int)Enum.Parse(typeof(ModuleLicenceType), input));
		}

		#region Lookups

		CodeDescriptionPairList LicenceModuleList
		{
			get
			{
				if (licenceModuleList == null)
				{
					licenceModuleList = new CodeDescriptionPairList();
					licenceModuleList.AddPair("", "");

					LicenceCheckpoint[] licences = Env.Licence.GetAllCheckpoints();

					foreach (LicenceCheckpoint licence in licences)
					{
						licenceModuleList.AddPair(licence.Name, licence.DisplayName);
					}

					licenceModuleList.Sort();
				}

				return licenceModuleList;
			}
		}
		CodeDescriptionPairList licenceModuleList;

		LicenceTypes LicenceTypes
		{
			get { return licenceTypes ?? (licenceTypes = new LicenceTypes()); }
		}
		LicenceTypes licenceTypes;

		#endregion
	}
}
