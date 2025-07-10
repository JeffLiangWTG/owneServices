using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay
{
	class CFEXmlWriter : TransactionBatchToXmlWriter
	{
		public CFEXmlWriter()
		{
			cfeBuilder_constructorInitializedOnly = new CFEBuilder();
		}

		protected override XmlWriterSettings Settings()
		{
			var settings = base.Settings();
			settings.Indent = true;

			return settings;
		}

		protected override void WriteDocumentBody(XmlWriter writer, TransactionInfo transactionInfo, ZString messageType, AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings)
		{
			var eInvoice = CFEBuilder.BuildCFEInfo(transactionInfo);
			var serializer = new XmlSerializer(eInvoice.GetType()); // This is sample client application which does not use ZArchitecture
			serializer.Serialize(writer, eInvoice);
		}

		protected override void WriteXmlToStreamCore(TransactionInfo transactionInfo, Stream stream, ZString messageType, AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings)
		{
			var doc = new XmlDocument();
			using (var tmpStream = new MemoryStream())
			{
				base.WriteXmlToStreamCore(transactionInfo, tmpStream, messageType, accBatch, notifications, warnings);

				tmpStream.Position = 0;
				using (var reader = new StreamReader(tmpStream, true))
				{
					var xml = reader.ReadToEnd();
					doc.LoadXml(xml);
				}
			}

			PostProcessorFormatDocumentDecimals(doc);

			using (XmlWriter writer = XmlWriter.Create(stream, Settings()))
			{
				doc.WriteContentTo(writer);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void PostProcessorFormatDocumentDecimals(XmlDocument doc)
		{
			var eFacXpath = $"/*[local-name()='CFE']/*[local-name()='eFact']";
			var eTckXpath = $"/*[local-name()='CFE']/*[local-name()='eTck']";

			var searchingFields = new List<(string xPathField, string xPathFormat)> {
				($"{eFacXpath}/*[local-name()='Encabezado']/*[local-name()='Totales']/*[local-name()='TpoCambio']", "F3"),
				($"{eTckXpath}/*[local-name()='Encabezado']/*[local-name()='Totales']/*[local-name()='TpoCambio']", "F3"),
				($"{eFacXpath}/*[local-name()='Detalle']/*[local-name()='Item']/*[local-name()='MontoItem']", "F2"),
				($"{eTckXpath}/*[local-name()='Detalle']/*[local-name()='Item']/*[local-name()='MontoItem']", "F2"),
				($"{eFacXpath}/*[local-name()='Encabezado']/*[local-name()='Totales']/*[local-name()='MntIVATasaBasica']", "F2"),
				($"{eTckXpath}/*[local-name()='Encabezado']/*[local-name()='Totales']/*[local-name()='MntIVATasaBasica']", "F2"),
				($"{eFacXpath}/*[local-name()='Encabezado']/*[local-name()='Totales']/*[local-name()='MntIVATasaMin']", "F2"),
				($"{eTckXpath}/*[local-name()='Encabezado']/*[local-name()='Totales']/*[local-name()='MntIVATasaMin']", "F2"),
				($"{eTckXpath}/*[local-name()='Detalle']/*[local-name()='Item']/*[local-name()='MontoItem']", "F2"),
				($"{eFacXpath}/*[local-name()='Encabezado']/*[local-name()='Totales']/*[local-name()='MntNoGrv']", "F2"),
				($"{eTckXpath}/*[local-name()='Encabezado']/*[local-name()='Totales']/*[local-name()='MntNoGrv']", "F2"),
				($"{eFacXpath}/*[local-name()='Encabezado']/*[local-name()='Totales']/*[local-name()='MntExpoyAsim']", "F2"),
				($"{eTckXpath}/*[local-name()='Encabezado']/*[local-name()='Totales']/*[local-name()='MntExpoyAsim']", "F2"),
				($"{eFacXpath}/*[local-name()='Encabezado']/*[local-name()='Totales']/*[local-name()='MntNetoIvaTasaMin']", "F2"),
				($"{eTckXpath}/*[local-name()='Encabezado']/*[local-name()='Totales']/*[local-name()='MntNetoIvaTasaMin']", "F2"),
				($"{eFacXpath}/*[local-name()='Encabezado']/*[local-name()='Totales']/*[local-name()='MntNetoIVATasaBasica']", "F2"),
				($"{eTckXpath}/*[local-name()='Encabezado']/*[local-name()='Totales']/*[local-name()='MntNetoIVATasaBasica']", "F2")
			};

			foreach (var field in searchingFields)
			{
				var nodes = doc.DocumentElement?.SelectNodes(field.xPathField);
				foreach (XmlNode node in nodes)
				{
					if (decimal.TryParse(node.InnerText, out decimal decimalValue))
					{
						node.InnerText = decimalValue.ToString(field.xPathFormat, CultureInfo.InvariantCulture);
					}
				}
			}
		}

		#endregion

		ICFEBuilder CFEBuilder => cfeBuilder_constructorInitializedOnly;
		ICFEBuilder cfeBuilder_constructorInitializedOnly;

#if DEBUG
		public void SubstituteCFEBuilder_ForTestOnly(ICFEBuilder replacement) => cfeBuilder_constructorInitializedOnly = replacement;
		public ICFEBuilder CFEBuilder_ExposedForTestOnly => CFEBuilder;
#endif
	}
}
