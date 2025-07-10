using System;
using System.Globalization;

namespace Enterprise.ZArchitecture.Environment
{
	public class MYCustomsRegistry
	{
		public MYCustomsRegistry(RawDataRegistry rawRegistry)
		{
			RawRegistry = rawRegistry;
		}

		public bool UseRankAlphaForK4K5
		{
			get { return (bool)RawRegistry.MYCustomsUseRankAlphaForK4K5.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.MYCustomsUseRankAlphaForK4K5.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string MYMessageOutputDirectory
		{
			get { return (string)RawRegistry.MYMessageOutputDirectory.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { RawRegistry.MYMessageOutputDirectory.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		public string ImportContinuingPermission
		{
			get { return (string)RawRegistry.MYCustomsImportContinuingPermission.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public string MYCustomsSenderID_ForCompany(Guid companyPK)
		{
			return (string)RawRegistry.MYCustomsSenderID.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
		}

		public string MYCustomsSenderID
		{
			get { return (string)RawRegistry.MYCustomsSenderID.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.MYCustomsSenderID.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "for logging only")]
		public string CheckSenderIdIsConfigured()
		{
			if (!string.IsNullOrEmpty(MYCustomsSenderID))
			{
				return string.Empty;
			}

			return string.Format(CultureInfo.InvariantCulture, "The registry setting '{0}' has not been configured.", RawRegistry.MYCustomsSenderID.GetLocationInEnglish());
		}

		public bool IsTestMode
		{
			get { return (bool)RawRegistry.MYIsTestMode.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.MYIsTestMode.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool Use2006Version
		{
			get { return (bool)RawRegistry.MYUse2006Version.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.MYUse2006Version.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string MYDagangPassword
		{
			get { return (string)RawRegistry.MYDagangPassword.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.MYDagangPassword.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string MYSenderPassword
		{
			get { return (string)RawRegistry.MYSenderPassword.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.MYSenderPassword.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		readonly RawDataRegistry RawRegistry;
	}
}
