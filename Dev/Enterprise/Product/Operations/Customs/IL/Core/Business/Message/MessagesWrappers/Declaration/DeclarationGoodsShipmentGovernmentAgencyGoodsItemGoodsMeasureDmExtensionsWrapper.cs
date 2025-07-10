using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureDmExtensionsWrapper : IDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureDmExtensions
	{
		public DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureDmExtensionsWrapper(ZString measureQualifier)
		{
			this.measureQualifier = measureQualifier;
		}

		public ICodeType MeasureQualifier => CodeTypeWrapper.NewOrNull(measureQualifier);

		readonly ZString measureQualifier;
	}
}
