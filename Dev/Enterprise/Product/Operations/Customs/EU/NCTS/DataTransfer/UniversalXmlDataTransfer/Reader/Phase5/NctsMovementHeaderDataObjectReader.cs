using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase5
{
	public abstract class NctsMovementHeaderDataObjectReader : NctsHeaderCommonDataObjectReader
	{
		protected NctsMovementHeaderDataObjectReader(UniversalShipment headerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(headerDataObject, logger, factory)
		{
		}

		protected sealed override ZString ApplicationCode => CusInBondApplicationCodeList.Codes.NCTS5;

		protected sealed override void AddAdditionalFilterForExistingMatching(ZDBOnlyQuery query)
		{
			query.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, HeaderType);
		}

		protected override NctsHeader GetNewBusinessObject()
		{
			var result = base.GetNewBusinessObject();
			result.SetMovementType(HeaderType);
			return result;
		}

		protected abstract ZString HeaderType { get; }
	}
}
