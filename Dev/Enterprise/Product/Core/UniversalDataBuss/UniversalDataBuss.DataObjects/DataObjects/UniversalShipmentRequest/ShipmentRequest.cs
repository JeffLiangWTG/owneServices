using System.Diagnostics.CodeAnalysis;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "False positive")]
	[XsdSchema("UniversalShipmentRequest.xsd"), RootElement("UniversalShipmentRequest")]
	public class ShipmentRequest : TopLevelDataObject, IRequestDataObject
	{
		[NamespaceDependent(UniversalXmlInfo.Namespace_2011_11, typeof(_2011_11.DataContext))]
		[NamespaceDependent(UniversalXmlInfo.Namespace_2012_11, typeof(_2012_11.DataContext))]
		[ReferenceProperty]
		public override IDataContextDataObject DataContext { get; set; }

		public MessageProfile MessageProfile { get; set; }
	}
}
