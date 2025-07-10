using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public abstract class BaseAdditionalInfo : ImportExportAwareSupportingInfo
	{
		protected BaseAdditionalInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString KeyToDeterimeUniqueness => CSI_Code + CSI_Description;

		public ZBool IsAnAdditionalReference => CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.AdditionalReference);
		public ZBool IsAnAdditionalInformation => CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
		public ZBool IsATransportDocument => CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.TransportDocument);
	}
}
