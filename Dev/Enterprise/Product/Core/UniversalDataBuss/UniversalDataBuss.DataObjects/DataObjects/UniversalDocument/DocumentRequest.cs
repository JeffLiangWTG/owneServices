using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "False positive")]
	[XsdSchema("UniversalDocumentRequest.xsd"), RootElement("UniversalDocumentRequest")]
	public partial class DocumentRequest : TopLevelDataObject, IRequestDataObject
	{
		public DocumentRequest()
		{
		}

		public DocumentRequest(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[NamespaceDependent(UniversalXmlInfo.Namespace_2011_11, typeof(_2011_11.DataContext))]
		[NamespaceDependent(UniversalXmlInfo.Namespace_2012_11, typeof(_2012_11.DataContext))]
		[ReferenceProperty]
		public override IDataContextDataObject DataContext { get; set; }

		public ZBool? ReturnDocumentDescriptionsOnly { get; set; }

		public List<DocumentFilter> FilterCollection { get; set; }
	}
}
