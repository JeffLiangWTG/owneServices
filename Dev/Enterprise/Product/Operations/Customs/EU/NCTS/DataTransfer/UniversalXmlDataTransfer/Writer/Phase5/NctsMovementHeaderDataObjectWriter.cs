using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase5
{
	public abstract class NctsMovementHeaderDataObjectWriter : NctsHeaderCommonDataObjectWriter
	{
		protected NctsMovementHeaderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected sealed override CodeDescriptionPair GetMessagingApplicationCode(NctsHeader headerBO) => new CodeDescriptionPair() { Code = CusInBondApplicationCodeList.Codes.NCTS5, Description = CusInBondApplicationCodeList.Descriptions.NCTS5 };

		protected sealed override void PopulateDataObjectCore(NctsHeader headerBO, Shipment headerData)
		{
			PopulateDataObjectMain(headerBO, headerData);
		}

		protected virtual void PopulateDataObjectMain(NctsHeader headerBO, Shipment headerData) { }
	}
}
