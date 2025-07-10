using System;
using CargoWise.Common.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Modules.AutoRefresh
{
	public sealed class AutoRefreshManager : RegistryItemSet
	{
		#region Instance

		public static AutoRefreshManager Instance
		{
			get { return fInstance ?? (fInstance = new AutoRefreshManager()); }
		}
		[SuppressThreadStaticFieldMessage]
		static AutoRefreshManager fInstance;

		AutoRefreshManager()
		{
		}

		#endregion

		public override bool IsForProductivityWise => true;

		#region GetTimeoutDescription

		public string GetTimeoutDescription(byte timeOutInMinutes)
		{
			var result = new System.Text.StringBuilder();
			var span = new TimeSpan(0, timeOutInMinutes, 0);

			if (span.Hours == 1)
			{
				result.Append(Res.GetString("a2f1fb91-4de3-4fcd-ace1-434b5db382af", "Hour"));
			}
			else if (span.Hours > 1)
			{
				result.Append(Res.GetString("c7690ae6-bf29-4ed2-b868-31442d7421ef", "{0} Hours", span.Hours));
			}

			if (span.Minutes > 0)
			{
				if (result.Length > 0)
				{
					result.Append(" " + Res.GetString("ade742c3-0389-4ef4-9581-e661bfaeca6b", "and") + " ");
				}

				result.Append(
					(span.Minutes == 1) ?
					Res.GetString("0566cf2c-24e1-4560-9bdf-59a0ac6f2708", "1 Minute") :
					Res.GetString("09455b57-6452-467c-8f6d-719c0c27298e", "{0} Minutes", span.Minutes)
				);
			}

			return (result.Length > AutoRefreshBizO.AutoRefreshDescMaxLength) ?
				result.ToString(0, AutoRefreshBizO.AutoRefreshDescMaxLength) :
				result.ToString();
		}

		#endregion

		#region IsAutoRefreshEnabled

		public bool IsAutoRefreshEnabled(ModuleIdentifier id)
		{
			return
				EnvProxy.Instance.Security.AutoRefreshModuleGrids.IsAllowed &&
				GetAutoRefreshTimeOutFromRegistry(id).IsEnabled;
		}

		#endregion

		#region Get/Set AutoRefreshTimeOut

		public byte GetAutoRefreshTimeOut(ModuleIdentifier id)
		{
			return GetAutoRefreshTimeOutFromRegistry(id).RefreshTimeInMinutes;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public void SetAutoRefreshTimeOut(ModuleIdentifier id, bool isEnabled, byte refreshTimeOutInMinutes)
		{
			var env = EnvProxy.Instance;
			var timeOut = new AutoRefreshTimeOut(isEnabled, refreshTimeOutInMinutes);
			var item = GetAutoRefreshRegistryItem(id);
			item.SetValue(env.CurrentCompany.PK, Guid.Empty, env.CurrentUser.PK, timeOut);
		}

		AutoRefreshTimeOut GetAutoRefreshTimeOutFromRegistry(ModuleIdentifier id)
		{
			var env = EnvProxy.Instance;
			var item = GetAutoRefreshRegistryItem(id);
			return (AutoRefreshTimeOut)item.GetValueWithoutFallback(env.CurrentCompany.PK, Guid.Empty, env.CurrentUser.PK);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Non-semantic text")]
		IRegistryItem GetAutoRefreshRegistryItem(ModuleIdentifier id)
		{
			var key = "Auto-refresh timeout (" + id + ")";
			return GetItem(key, delegate
			{
				return new RegistryItemImpl(key, null, null, null, InstanceForLameRegistry, RegistryStorageFlags.CompanyDepartment);
			});
		}

		AutoRefreshRegistryDataType InstanceForLameRegistry
		{
			get { return fInstanceForLameRegistry ?? (fInstanceForLameRegistry = new AutoRefreshRegistryDataType()); }
		}

		AutoRefreshRegistryDataType fInstanceForLameRegistry;

		#endregion
	}
}
