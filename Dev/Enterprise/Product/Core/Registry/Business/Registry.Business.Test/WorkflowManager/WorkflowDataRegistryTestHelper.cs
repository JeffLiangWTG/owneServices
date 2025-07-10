using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Testing
{
	sealed class WorkflowDataRegistryTestHelper
	{
		internal static void SetupTaskTypesRegistry()
		{
			var categorisedWorkflowTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var category1 = categorisedWorkflowTaskTypes.AddNew();
			category1.Code = "WKI";
			category1.Description = (NoResString)"Work Item";

			var taskType1 = category1.TaskTypes.AddNew();
			taskType1.Code = "INV";
			taskType1.Description = (NoResString)"Investigation";

			var taskType2 = category1.TaskTypes.AddNew();
			taskType2.Code = "SHV";
			taskType2.Description = (NoResString)"Shelf?";

			var taskType3 = category1.TaskTypes.AddNew();
			taskType3.Code = "COD";
			taskType3.Description = (NoResString)"Coding";

			var taskType4 = category1.TaskTypes.AddNew();
			taskType4.Code = "CRF";
			taskType4.Description = (NoResString)"Code Review";

			var category2 = categorisedWorkflowTaskTypes.AddNew();
			category2.Code = "SMS";
			category2.Description = (NoResString)"Something Else";

			var taskType5 = category2.TaskTypes.AddNew();
			taskType5.Code = "TST";
			taskType5.Description = (NoResString)"Test";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedWorkflowTaskTypes);
		}

		internal static T SerializeAndDeserialize<T>(T workflowTaskType)
			where T : new()
		{
			var builder = new StringBuilder();
			var settings = new XmlWriterSettings { OmitXmlDeclaration = true };

			using (var writer = XmlWriter.Create(builder, settings))
			{
				writer.WriteStartElement("root");
				((IXmlSerializable)workflowTaskType).WriteXml(writer);
				writer.WriteEndElement();
			}

			var newRecord = new T();

			using (var sReader = new StringReader(builder.ToString()))
			using (var reader = XmlReader.Create(sReader))
			{
				reader.Read();
				((IXmlSerializable)newRecord).ReadXml(reader);
			}

			return newRecord;
		}
	}
}
