using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaManifestHeaderTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			TypeDecider typeDecider = null;
			var applicationCode = (row != null) ? new ZString(row[AsycudaManifestHeader.Schema.AMA_ApplicationCode]) : ZString.Empty;
			switch (applicationCode)
			{
#if DEBUG
				case ApplicationCodeTypeList.Constants.ManifestBase:
					return typeof(AsycudaManifestHeader);
#endif
				case ApplicationCodeTypeList.Codes.TemporaryStorage:
					typeDecider = (TypeDecider)ObjectFactory.Get<Integration.Customs.EU.ITemporaryStorageHeaderTypeDecider>();
					return typeDecider.GetTypeForLoad(row, factory);
				case ApplicationCodeTypeList.Codes.EuH7:
					typeDecider = (TypeDecider)ObjectFactory.Get<Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeaderTypeDecider>();
					return typeDecider.GetTypeForLoad(row, factory);
				case ApplicationCodeTypeList.Codes.ZAOutturnAndGateInOrOut:
					return ObjectFactory.GetType<Integration.Customs.ZA.IAsycudaManifestHeader>();
				default:
					typeDecider = (TypeDecider)ObjectFactory.Get<Integration.Customs.ASYCUDA.IAsycudaManifestHeaderTypeDecider>();
					return typeDecider.GetTypeForLoad(row, factory);
			}
		}

		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaManifestHeader);
		}

		public override Type GetTypeForNew()
		{
			return typeof(AsycudaManifestHeader);
		}
	}
}
