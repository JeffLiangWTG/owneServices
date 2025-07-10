using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocPackLocationForDocument : DocumentWrapper
	{
		DocPackLocationForDocument(PackLocationForDocument location, BusinessObjectFactory factoryToWrap)
			: base(location, factoryToWrap)
		{
		}

		public static DocPackLocationForDocument New(PackLocationForDocument location, BusinessObjectFactory factoryToWrap)
		{
			DocPackLocationForDocument result = null;

			if (location != null)
			{
				result = new DocPackLocationForDocument(location, factoryToWrap);
			}

			return result;
		}

		PackLocationForDocument Location
		{
			get { return (PackLocationForDocument)WrappedObject; }
		}

		#region Properties

		public ZString ShipmentNumber
		{
			get { return Location.ShipmentNumber; }
		}

		public ZString Destination
		{
			get { return Location.Destination; }
		}

		public ZString Consignor
		{
			get { return Location.ConsignorName; }
		}

		public ZInt PackCount
		{
			get { return Location.PackCount; }
		}

		public ZString PackType
		{
			get { return Location.PackType; }
		}

		public ZString WhsLocation
		{
			get { return Location.WhsLocation; }
		}

		#endregion
	}
}
