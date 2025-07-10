using System.Diagnostics.CodeAnalysis;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects
{
	[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "False positive")]
	[XsdSchema("UniversalActivityRequest.xsd"), RootElement("UniversalActivityRequest")]
	public class ActivityRequest : TopLevelDataObject, IRequestDataObject
	{
		[NamespaceDependent(UniversalXmlInfo.Namespace_2011_11, typeof(Universal._2011_11.DataContext))]
		[NamespaceDependent(UniversalXmlInfo.Namespace_2012_11, typeof(Universal._2012_11.DataContext))]
		[ReferenceProperty]
		public override IDataContextDataObject DataContext { get; set; }
	}
}
