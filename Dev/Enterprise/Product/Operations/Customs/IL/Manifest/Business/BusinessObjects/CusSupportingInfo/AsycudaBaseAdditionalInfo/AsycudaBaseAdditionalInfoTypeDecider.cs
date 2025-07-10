using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaBaseAdditionalInfoTypeDecider : TypeDecider, ICusSupportingInfoTypeSubTypeSupporter
	{
		public Type GetTypeBySubType(ZString subType)
		{
			return GetTypeBy(subType);
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var subType = row?[CusSupportingInfo.Schema.CSI_SubType]?.ToString();
			return GetTypeBy(subType);
		}

		public override Type GetTypeForNew()
		{
			return null;
		}

		static Type GetTypeBy(ZString subType)
			=> (string)subType switch
			{
				AdditionalInfoSubTypeList.Codes.AdditionalInformation => typeof(AsycudaAdditionalInfo),
				AdditionalInfoSubTypeList.Codes.TransportDocument => typeof(AsycudaTransportDocumentInfo),
				_ => null,
			};
	}
}
