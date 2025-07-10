using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class ReportColumnSettingKeys
	{
		public ReportColumnSettingKeys(string description, ZGuid reportID, ZGuid linkID) : this(description, reportID, linkID, Env.CurrentCompany.Code)
		{
		}

		public ReportColumnSettingKeys(string description, ZGuid reportID, ZGuid linkID, string companyCode)
		{
			fDescription = description;
			fReportID = reportID;
			fLinkID = linkID;
			fCompanyCode = companyCode;
		}

		readonly string fDescription;
		readonly ZGuid fReportID;
		readonly ZGuid fLinkID;
		readonly string fCompanyCode;

		public string Description
		{
			get { return fDescription; }
		}

		public ZGuid ReportID
		{
			get { return fReportID; }
		}

		public ZGuid LinkID
		{
			get { return fLinkID; }
		}

		public string CompanyCode
		{
			get { return fCompanyCode; }
		}
	}

	public class ReportColumnSettingsRetriever
	{
		readonly ReportColumnSettingRegistryItem Registry = new ReportColumnSettingRegistryItem();

		readonly SavedReportColumnSettingKeyRetreiver SavedKeyRetreiver = new SavedReportColumnSettingKeyRetreiver();

		public List<ReportColumnSettingKeys> GetSavedColumnSettingKeysForCurrentCompany(ZGuid reportID)
		{
			return SavedKeyRetreiver.SavedColumnSettingKeysForCurrentCompany(reportID);
		}

		public List<ReportColumnSettingKeys> GetSavedColumnSettingKeysForAllCompanies(ZGuid reportID)
		{
			return SavedKeyRetreiver.SavedColumnSettingKeys(reportID, true);
		}

		public string Get(string description, ZGuid reportPK, ZGuid clientPK, string companyCode)
		{
			return Registry.GetValueWithoutFallback(description, reportPK, clientPK, companyCode);
		}

		public string Get(string description, ZGuid reportPK, ZGuid clientPK)
		{
			return Registry.GetValueWithoutFallback(description, reportPK, clientPK);
		}

		public string Get(string description, ZGuid reportPK)
		{
			return this.Get(description, reportPK, ZGuid.Empty);
		}

		public void Set(string description, ZGuid reportPK, ZGuid clientPK, string companyCode, string value)
		{
			Registry.SetValue(description, reportPK, clientPK, companyCode, value);
		}

		public void Set(string description, ZGuid reportPK, ZGuid clientPK, string value)
		{
			Registry.SetValue(description, reportPK, clientPK, value);
		}

		public void Set(string description, ZGuid reportPK, string value)
		{
			this.Set(description, reportPK, ZGuid.Empty, value);
		}

		public void Delete(String description, ZGuid reportPK, ZGuid clientPK)
		{
			Registry.DeleteRecord(description, reportPK, clientPK);
		}

		public void Delete(string description, ZGuid reportPK)
		{
			this.Delete(description, reportPK, ZGuid.Empty);
		}
	}
}
