using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class ETailCusOutturnHeaderDataObjectReader : CusOutturnHeaderDataObjectReader
	{
		public ETailCusOutturnHeaderDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		#region override
		protected override DepotCusOutturn FillSubShipmentCore(Shipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory, CusOutturnHeader outturnHeader)
		{
			var consignments = shipment.SubShipmentCollection;
			if (consignments != null && consignments.Count > 0)
			{
				foreach (var consignment in consignments)
				{
					var reader = new ETailDepotCusOutturnDataObjectReader(consignment, logger, factory, outturnHeader, dataObject, shipment);
					var outturn = reader.ReadIntoBusinessObject();
					if (ForwardingShipmentPK.HasValue && !ForwardingShipmentPK.Value.IsEmpty)
					{
						outturn.C5_ParentID = ForwardingShipmentPK.Value;
						outturn.C5_ParentTableCode = JobShipmentSchema.Constants.Prefix;
					}

					Helper.MarkProcessed(outturn);
				}
			}
			return null;
		}

		protected override ZGuid GetForwardingShipmentPk()
		{
			var forwardingShipmentUniqueConsignRef = dataObject.DataContext.DataSourceCollection
				.Where(n => n.Type.GetValueOrDefault() == nameof(DataContextType.ForwardingShipment))?.FirstOrDefault()?.Key;

			return forwardingShipmentUniqueConsignRef.HasValue ?
				new ZGuid(factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, forwardingShipmentUniqueConsignRef))?.FirstOrDefault()?.PK) : ZGuid.Empty;
		}
		#endregion
	}
}
