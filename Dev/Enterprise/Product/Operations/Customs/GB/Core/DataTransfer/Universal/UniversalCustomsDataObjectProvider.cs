using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.AirManifest;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.Customs.DataTransfer.Universal;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	public class UniversalCustomsDataObjectProvider : EU.DataTransfer.Universal.UniversalCustomsDataObjectProvider, UniversalShipment.IUniversalCustomsDataObjectProvider
	{
		#region IUniversalCustomsDataObjectProvider Members

		protected override ICodeDescriptionPairList TableSpecificCusAddInfoTypeListCore(ZString tableCode, string dataContext)
		{
			return new CusAddInfoTypeListProvider().TableSpecificCusAddInfoTypeList(tableCode, dataContext);
		}

		protected override ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReaderCore(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment)
		{
			return new DeclarationDataObjectReader(declarationDataObject, logger, factory, shipment);
		}

		protected override ITopLevelDataObjectWriter GetNewDeclarationDataObjectWriterCore(IDataWritingManager manager)
		{
			return new DeclarationDataObjectWriter(manager);
		}

		protected override ICodeDescriptionPairList TableSpecificCusReferenceTypeListCore(ZString tableCode, string dataContext) => CusReferenceTypeListProvider.TableSpecificCusReferenceTypeList(tableCode, dataContext);

		protected override EU.DataTransfer.Universal.CusSupportingInfoTypeListProvider GetCusSupportingInfoTypeListProvider()
		{
			return new CusSupportingInfoTypeListProvider();
		}

		protected override EU.DataTransfer.Universal.CusCodeDataTypeAndCodeListProvider GetCusCodeDataTypeAndCodeListProvider()
		{
			return new CusCodeDataTypeAndCodeListProvider();
		}

		protected override IEnumerable<ITopLevelDataObjectReader> GetNewAirManifestDataObjectReadersCore(Shipment mawbDataObject, Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory, bool singleHAWBCheck)
		{
			return Enumerable.Empty<ITopLevelDataObjectReader>(); //Force GB Readers to be empty, even if EU change their default.
		}

		protected override ITopLevelDataObjectWriter GetNewAirManifestDataObjectWriterCore(IDataWritingManager manager)
		{
			return new CusMAWBDataObjectWriter(manager);
		}

		protected override ITopLevelDataObjectWriter GetNewAirManifestLineDataObjectWriterCore(IDataWritingManager manager, AirManifestDataObjectWriterHelper helper)
		{
			return new CusHAWBDataObjectWriter(manager, helper == null ? null : new AirManifestDataObjectWriterHelper(helper));
		}

		#endregion
	}
}
