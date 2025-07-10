using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CertificateManager : Customs.Business.BatchProcessor.CertificateManager, Integration.Customs.AU.ICertificateManager
	{
		public CertificateManager(BusinessObjectFactory factory) : base()
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		protected override byte[] GetCompanyCertificateData()
		{
			return Env.Registry.AUCCompanyCertificateData;
		}

		protected override IRegistryItem GetRawCompanyCertificateData()
		{
			return Env.Registry.RawRegistry.AUCCompanyCertificateData;
		}

		protected override string GetCompanyCertificatePassword()
		{
			return Env.Registry.AUCCompanyCertificatePassword;
		}

		protected override byte[] GetCustomsCertificateData()
		{
			return GetCertificateData(AUConstants.RefSysConfigCodes.AUCryptCrt, AUConstants.RefSysConfigCodes.AUCryptCrtNew);
		}

		protected override byte[] GetTrustPointCertificateData()
		{
			return GetCertificateData(AUConstants.RefSysConfigCodes.AUTrustCrt, AUConstants.RefSysConfigCodes.AUTrustCrtNew);
		}

		byte[] GetCertificateData(string mainCode, string alternateCode)
		{
			var now = ZDateTime.Now;
			var data = LoadRefSysConfigBinaryValue(mainCode, now) ?? LoadRefSysConfigBinaryValue(alternateCode, now);
			return data ?? ZBlob.Empty;
		}

		byte[] LoadRefSysConfigBinaryValue(ZString configCode, ZDateTime activeDate)
		{
			byte[] result = null;

			if (!configCode.IsEmpty && activeDate.IsValid)
			{
				var configEntry = RefSysConfigLoader.Load(configCode, activeDate);
				if (configEntry != null && configEntry.ZRC_EndDate >= activeDate)
				{
					result = configEntry.ZRC_BinaryValue;
				}
			}

			return result;
		}

		RefSysConfig.Loader RefSysConfigLoader => refSysConfigLoader ?? (refSysConfigLoader = new RefSysConfig.Loader(factory));
		RefSysConfig.Loader refSysConfigLoader;

		#region ICertificateManager

		string Integration.Customs.AU.ICertificateManager.CompanyCertificatePassword => GetCompanyCertificatePassword();

		byte[] Integration.Customs.AU.ICertificateManager.CompanyCertificateData => GetCompanyCertificateData();

		byte[] Integration.Customs.AU.ICertificateManager.CustomsCertificateData => GetCustomsCertificateData();

		byte[] Integration.Customs.AU.ICertificateManager.TrustPointCertificateData => GetTrustPointCertificateData();

		#endregion
	}
}
