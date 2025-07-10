using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Client.EDI.Billing.ServiceTasks
{
	public abstract class BillingServiceTask : ServiceProviderImpl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected void UpdateBillingDb()
		{
			if (Globals.IsTest)
			{
				return;
			}

			string sql =
@"
delete from BillingLicenceDatabaseCache;

-- insert everything since we don't have last edit time fields to restrict it to recent changes
WITH EnterprisesBeingTornDown as
(
SELECT LE_PK, WKP_Module
  FROM [ediProd].[dbo].[WorkProject]
  JOIN [ediprod].[dbo].[ClientWorkProject] ON WKP_PK = CWP_WKP
  JOIN dbo.LicenceHeader ON LA_PK = CWP_LA
  JOIN dbo.LicenceDatabase ON LA_LD = LD_PK
  JOIN dbo.LicenceEnterprise ON LD_LE = LE_PK
  WHERE WKP_Module In ('HTD', 'PTD', 'HTT') AND WKP_Status NOT IN('CLS', 'CAN') AND CWP_LA IS NOT NULL
),
DatabasesBeingTornDown as
(
	SELECT DISTINCT LA_LD DTD_LD
	FROM EnterprisesBeingTornDown TD
	JOIN dbo.LicenceEnterprise LE ON TD.LE_PK = LE.LE_PK
	JOIN dbo.LicenceDatabase ON LD_LE = LE.LE_PK
	JOIN dbo.LicenceHeader ON LA_LD = LD_PK
	WHERE LD_Product In ('CW1', 'CWN', 'CGW', 'PRW') AND (WKP_Module <> 'HTT' OR LD_LicenceType <> 'PRD')
),
EnterprisesStillOnboarding as
(
	SELECT LE_PK, WKP_Module
	FROM [ediProd].[dbo].[WorkProject]
	JOIN [ediprod].[dbo].[ClientWorkProject] ON WKP_PK = CWP_WKP
	JOIN dbo.LicenceHeader ON LA_PK = CWP_LA
	JOIN dbo.LicenceDatabase ON LA_LD = LD_PK
	JOIN dbo.LicenceEnterprise ON LD_LE = LE_PK
	WHERE WKP_Module IN('ONB', 'LCN', 'TST', 'DOB', 'SHA', 'ONP') AND WKP_Status NOT IN('CLS', 'CAN') AND CWP_LA IS NOT NULL
),
DatabasesStillOnboarding as
(
	SELECT DISTINCT LA_LD DOB_LD
	FROM EnterprisesStillOnboarding TD
	JOIN dbo.LicenceEnterprise LE ON TD.LE_PK = LE.LE_PK
	JOIN dbo.LicenceDatabase ON LD_LE = LE.LE_PK
	JOIN dbo.LicenceHeader ON LA_LD = LD_PK
	WHERE LD_Product In ('CW1', 'CWN', 'CGW', 'PRW') AND (WKP_Module <> 'TST' OR LD_LicenceType <> 'PRD') 
)

insert BillingLicenceDatabaseCache(EnterpriseCode, ServerCode, SystemId, DatabaseNumber, HostedLocation, TenantId, Product, IsActive, IsTeardownInProgress, LicenceType, Category, IsInternal)
select LE_EnterpriseCode, LD_ServerCode, LD_SystemId, LD_DatabaseNumber, LD_HostedLocation, LD_TenantID, LD_Product,
CASE WHEN DOB_LD is not NULL THEN 0 ELSE LD_IsActive END IsActive, CASE WHEN DTD_LD is not NULL THEN 1 ELSE 0 END IsTeardownInProgress,
LD_LicenceType, CASE WHEN LD_PRODUCT = 'SPM' THEN 'SHP' ELSE '' END Category,
LE_IsInternal
from dbo.LicenceDatabase
join dbo.LicenceEnterprise on LD_LE = LE_PK
join dbo.ViewLicenceDatabaseSystemId v on LicenceDatabase.LD_PK = v.LD_PK
left join DatabasesBeingTornDown ON DTD_LD = LicenceDatabase.LD_PK
left join DatabasesStillOnboarding ON DOB_LD = LicenceDatabase.LD_PK
where (LE_IsInternal = 0 and LD_Product in ('ENT', 'CW1', 'CWN', 'CGW', 'GLW', 'PRW', 'SMF', 'WTA', 'SPM') and (LD_Product not in ('SMF', 'WTA', 'SPM') or LD_TenantID <> ''))
OR (LE_IsInternal = 1 and LD_Product = 'SMF' and LD_TenantID <> '');

delete from BillingClientCompanyCache;

-- insert anything that has become valid in the last few days
insert BillingClientCompanyCache(DatabaseNumber, LCC_PK, CompanyCode, ValidFromUtc, CountryCode)
SELECT LD_DatabaseNumber, LCC_PK, LCC_Code, LCC_CodeValidFromUtc, LCC_RN_NKCountryCode
FROM dbo.ClientCompany
join dbo.LicenceDatabase on LCC_LD = LD_PK
join dbo.LicenceEnterprise on LD_LE = LE_PK
where LD_Product in ('ENT', 'CW1', 'CWN', 'CGW', 'GLW', 'PRW') and LE_IsInternal = 0
union all
SELECT LD_DatabaseNumber, LCC_PK, CCH_Code, CCH_CodeValidFromUtc, LCC_RN_NKCountryCode
FROM dbo.ClientCompany
join dbo.LicenceDatabase on LCC_LD = LD_PK
join dbo.LicenceEnterprise on LD_LE = LE_PK
join dbo.ClientCompanyCodeHistory on CCH_LCC = LCC_PK
where LD_Product in ('ENT', 'CW1', 'CWN', 'CGW', 'GLW', 'PRW') and LE_IsInternal = 0 and CCH_CodeValidFromUtc > DATEADD(DAY, -7, getutcdate());


EXECUTE BillingUpdateChargeable NULL, 202110, 0, 0, 1, 0, 0 with recompile;
";
			using (DbCommand cmd = Db.Connection.Command(sql))
			{
				ExecuteNonQuery(cmd, "Update BillingTransaction", 0);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		protected void ExecuteNonQuery(string sql, string description, int timeoutMinutes = 30)
		{
			using (DbCommand cmd = Db.Connection.Command(sql))
			{
				ExecuteNonQuery(cmd, description, timeoutMinutes);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected virtual void ExecuteNonQuery(DbCommand cmd, string description, int timeoutMinutes = 30)
		{
			ServiceLogger.Log(LogType.Information, "Begin " + description);

			try
			{
				cmd.CommandTimeout = timeoutMinutes * 60;
				cmd.ExecuteNonQuery();
			}
			catch (System.Data.Common.DbException ex)
			{
				HandleException(ex, description);
			}

			ServiceLogger.Log(LogType.Information, "End " + description);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected void HandleException(Exception ex, string msg)
		{
			ServiceLogger.Log(LogType.Error, "Error " + msg, ex);
			try
			{
				EmailDef mail = new EmailDef();
				mail.Subject = this.GetType().Name + ' ' + ex.Message;
				mail.Body = msg + "\r\n\r\n" + ex.ToString();
				Env.OutgoingMailManager.CreateAndSave(mail, EDIDataRegistry.Instance.InternalNotificationGroup.Value, GroupSourceLocator.GetFromRegistryItem(EDIDataRegistry.Instance.InternalNotificationGroup));
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
			}
		}

		public override abstract void RunTask(CancellationToken youMustReactToThisToken);
	}
}
