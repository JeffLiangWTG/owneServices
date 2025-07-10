using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.ICS
{
	public class AsycudaManifestHeaderBaseTypeDecider : TypeDecider, Integration.Customs.GB.GBICS.IAsycudaManifestHeaderBaseTypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}
		public override Type GetTypeForNew()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var manifestType = row[ASYCUDA.Business.AsycudaManifestHeader.Schema.AMA_ManifestType].ToString().Trim();
			switch (manifestType)
			{
				case ICSManifestTypes.Codes.ICS:
					return typeof(Business.AsycudaManifestHeader);
				case ICSManifestTypes.Codes.SAS:
					return typeof(Business.AsycudaManifestHeaderSS);
				default:
					return typeof(Business.AsycudaManifestHeader);
			}
		}
	}
}
