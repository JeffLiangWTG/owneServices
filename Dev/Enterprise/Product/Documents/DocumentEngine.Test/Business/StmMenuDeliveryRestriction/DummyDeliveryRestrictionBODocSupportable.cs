using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore;

namespace Enterprise.DocumentEngine.Business.Testing
{
	sealed class DummyDeliveryRestrictionBODocSupportable : DummyBODocSupportable, IOriginDestinationForDocumentDeliveryRestriction
	{
		public DummyDeliveryRestrictionBODocSupportable(BusinessObjectFactory factory, DataRow dataRow) : base(factory, dataRow)
		{
		}

		public string OriginCountryCode { get; }

		public string DestinationCountryCode { get; }
	}
}
