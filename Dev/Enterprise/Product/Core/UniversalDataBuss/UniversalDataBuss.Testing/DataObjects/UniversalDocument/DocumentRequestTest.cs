using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Application;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(DocumentRequest))]
	class DocumentRequestTest : TopLevelDataObjectTestCase<DocumentRequest>
	{
		public void TestMemoryUsage()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var documentRequest = new DocumentRequest(DefaultDataObjectWriterStrategy.TestInstance); //
				documentRequest.SetFilterCollection(() => new List<DocumentFilter>());
				var documentFilter = new DocumentFilter();
				documentFilter.Type = DocumentFilterType.BranchCode;

				documentFilter.Value = new string('a', 100000000);
				documentRequest.FilterCollection.Add(documentFilter);

				using (var stream = new CargoWise.IO.Shim.SubStreamableStream())
				{
					var xmlWriter = ObjectFactory.Get<IXmlWriter>();

					xmlWriter.WriteXML(documentRequest, stream); // Write so we load all the stuff.
					GC.Collect();
					Assert(GC.TryStartNoGCRegion(200000000));
					try
					{
						var startMemory = GC.GetTotalMemory(false);
						xmlWriter.WriteXML(documentRequest, stream);
						var endMemory = GC.GetTotalMemory(false);
						AssertCloseEnough(startMemory, endMemory, 100000);
					}
					finally
					{
						GC.EndNoGCRegion();
					}
				}
			}
		}

		public void TestWrite()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var documentRequest = new DocumentRequest();

				using (var stream = new CargoWise.IO.Shim.SubStreamableStream())
				{
					var xmlWriter = ObjectFactory.Get<IXmlWriter>();
					xmlWriter.WriteXML(documentRequest, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized UberShipmentRequest", EmptyDocumentRequestXml.Trim(), result);
					}
				}
			}
		}

		const string EmptyDocumentRequestXml =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <DocumentRequest>
  </DocumentRequest>
</UniversalDocumentRequest>";
	}
}
