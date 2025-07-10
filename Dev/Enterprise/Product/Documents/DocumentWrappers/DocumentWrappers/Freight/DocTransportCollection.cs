using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocTransportCollection : DocumentWrapperCollection<DocTransport>
	{
		public DocTransportCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocTransportCollection(TransportCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public DocTransportCollection(RoutingCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		protected override DocumentWrapper WrapObject(object objectToWrap)
		{
			Transport transport = objectToWrap as Transport;
			if (transport != null)
			{
				return (DocumentWrapper)TypeOfElements.InvokeMember("New", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { transport.Parent, transport, Factory });
			}
			return base.WrapObject(objectToWrap);
		}
	}
}
