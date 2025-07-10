using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.Application;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	internal static class XMLMessageTestHelper
	{
		public static XDocument GetXml(StmALog dex)
		{
			var pivot = GenPivot.LoadRelation1Pivot(dex, Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage);

			if (pivot == null)
			{
				return null;
			}

			var message = dex.Factory.Load<IEDIMessage>(pivot.XX_Relation2ID);

			var xml = XDocument.Parse(message.EM_MessageText);

			var triggerDate = xml.Descendants(xml.Root.Name.Namespace + "TriggerDate").FirstOrDefault();
			if (triggerDate != null)
			{
				triggerDate.Value = "";
			}

			return xml;
		}

		public static string ValidateXMLAgainstSchema(IDataObject dataObject, XDocument document)
		{
			var generator = ObjectFactory.Get<IUniversalXsdGenerator>();
			var commonSchema = XElement.Parse(generator.GetXsdOutput(dataObject.GetType().Assembly));
			var shipmentSchema = XElement.Parse(generator.GetXsdOutput(dataObject.GetType()));
			var errors = string.Empty;

			using (var shipmentSchemaReader = shipmentSchema.CreateReader())
			using (var commonSchemaReader = commonSchema.CreateReader())
			{
				var schemaSet = new XmlSchemaSet();
				schemaSet.Add(SchemaVersionManager.Current.Namespace, shipmentSchemaReader);
				schemaSet.Add(SchemaVersionManager.Current.Namespace, commonSchemaReader);

				document.Validate(schemaSet, (o, e) =>
				{
					errors = e.Message;
				});
			}

			return errors;
		}
	}
}
