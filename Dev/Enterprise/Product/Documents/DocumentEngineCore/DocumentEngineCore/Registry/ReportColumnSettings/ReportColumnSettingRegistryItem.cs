using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngineCore.Registry
{
#if DEBUG
	public
#else
	internal
#endif
	class ReportColumnSettingStringRegistryItem : StringRegistryItem
	{
		internal ReportColumnSettingStringRegistryItem(string name)
			: base(name, null, null, null, RegistryStorageFlags.All, RegistryOptions.IsHidden | RegistryOptions.NotCached)
		{
		}
	}

	internal class ReportColumnSettingRegistryItem
	{
		public string GetValueWithoutFallback(String description, ZGuid reportID, ZGuid linkID, string companyCode)
		{
			ReportColumnSettingStringRegistryItem item = AddORGet(description, companyCode);
			return item.GetValueWithoutFallback(reportID.ToGuid(), Guid.Empty, linkID.IsEmpty ? Guid.Empty : linkID.ToGuid());
		}

		public string GetValueWithoutFallback(String description, ZGuid reportID, ZGuid linkID)
		{
			return GetValueWithoutFallback(description, reportID, linkID, string.Empty);
		}

		public void SetValue(String description, ZGuid reportID, ZGuid linkID, string companyCode, string value)
		{
			ReportColumnSettingStringRegistryItem item = AddORGet(description, companyCode);
			item.SetValue(reportID.ToGuid(), Guid.Empty, linkID.IsEmpty ? Guid.Empty : linkID.ToGuid(), value);
		}

		public void SetValue(String description, ZGuid reportID, ZGuid linkID, string value)
		{
			SetValue(description, reportID, linkID, string.Empty, value);
		}

		public void DeleteRecord(string description, ZGuid reportID, ZGuid linkID)
		{
			ReportColumnSettingStringRegistryItem item = AddORGet(description);
			((IRegistryItemInternals)item).DeleteRecord(reportID.ToGuid(), Guid.Empty, linkID.IsEmpty ? Guid.Empty : linkID.ToGuid());
		}

		readonly Dictionary<string, ReportColumnSettingStringRegistryItem> values = new Dictionary<string, ReportColumnSettingStringRegistryItem>();

		ReportColumnSettingStringRegistryItem AddORGet(string description, string companyCode)
		{
			return AddORGetCore(Prefixmanager.GetKey(description, companyCode));
		}

		ReportColumnSettingStringRegistryItem AddORGet(string description)
		{
			return AddORGet(description, string.Empty);
		}

		ReportColumnSettingStringRegistryItem AddORGetCore(string key)
		{
			ReportColumnSettingStringRegistryItem result;
			if (!values.ContainsKey(key))
			{
				result = new ReportColumnSettingStringRegistryItem(key);
				values.Add(key, result);
			}
			else
			{
				result = values[key];
			}
			return result;
		}

		readonly ReportColumnSettingRegistryPrefixHelper Prefixmanager = new ReportColumnSettingRegistryPrefixHelper();
	}

	public class ReportColumnSettingRegistryPrefixHelper
	{
		public string GetKey(string description, string companyCode)
		{
			if (string.IsNullOrEmpty(companyCode))
			{
				return GetKey(description);
			}
			return GetKeyCore(BuildRegistryKeyPrefix(companyCode), description);
		}

		public string GetKey(string description)
		{
			return GetKeyCore(RegistryKeyPrefixForCurrentCompany, description);
		}

		string GetKeyCore(string prefix, string description)
		{
			return prefix + description;
		}

		public string GetCompanyCodeFromKey(string key)
		{
			return key.Substring(RegistryKeyPrefix.Length, key.IndexOf('$', RegistryKeyPrefix.Length + 1) - RegistryKeyPrefix.Length);
		}

		public string GetDescriptionFromKey(string key)
		{
			int descriptionStart = key.IndexOf('$', RegistryKeyPrefix.Length + 1) + 1;
			return key.Substring(descriptionStart, key.Length - descriptionStart);
		}

		public string RegistryKeyPrefixForCurrentCompany
		{
			get { return BuildRegistryKeyPrefix(Env.CurrentCompany.Code); }
		}

		string BuildRegistryKeyPrefix(string companyCode)
		{
			return RegistryKeyPrefix + companyCode + "$";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public const string RegistryKeyPrefix = "RPT_CFG$";
	}

	internal class SavedReportColumnSettingKeyRetreiver
	{
		public List<ReportColumnSettingKeys> SavedColumnSettingKeysForCurrentCompany(ZGuid reportID)
		{
			return SavedColumnSettingKeys(reportID, false);
		}

		public List<ReportColumnSettingKeys> SavedColumnSettingKeys(ZGuid reportID, bool allCompanies)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(StmData));
			if (allCompanies)
			{
				query.AddToFilter(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, ReportColumnSettingRegistryPrefixHelper.RegistryKeyPrefix);
			}
			else
			{
				query.AddToFilter(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, PrefixHelper.RegistryKeyPrefixForCurrentCompany);
			}
			query.AddToFilter(StmDataSchema.SD_Owner, reportID);
			StmData[] registryItems = factory.Load<StmData>(query);
			List<ReportColumnSettingKeys> result = new List<ReportColumnSettingKeys>();
			foreach (StmData item in registryItems)
			{
				result.Add(new ReportColumnSettingKeys(PrefixHelper.GetDescriptionFromKey(item.SD_Name), item.SD_Owner, item.SD_DepartmentGuid, PrefixHelper.GetCompanyCodeFromKey(item.SD_Name)));
			}
			return result;
		}
		readonly ReportColumnSettingRegistryPrefixHelper PrefixHelper = new ReportColumnSettingRegistryPrefixHelper();
	}
}
