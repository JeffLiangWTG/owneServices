using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class DynamicDataUXmlWriter : IXmlWriter
	{
		public DynamicDataUXmlWriter(IDynamicData dynamicData, string userDefinedXmlNamespace)
		{
			Argument.NotNull(dynamicData, nameof(dynamicData));

			this.dynamicData = dynamicData;
			this.userDefinedXmlNamespace = userDefinedXmlNamespace;
		}

		readonly IDynamicData dynamicData;
		readonly string userDefinedXmlNamespace;

		void IXmlWriter.WriteXML(IDataObject dataStructure, SubStreamableStream outputStream, string nameSpace, IDataOverrideProvider overrideProvider)
		{
			var ns = UniversalXmlInfo.Namespace_2012_11;

			var builder = new DataObjectBuilder(ns, dynamicData);
			var dataObject = builder.Build();

			if (dataStructure is Shipment fromShipment
				&& dataObject is Shipment toShipment)
			{
				toShipment.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);
				CopyNotes(fromShipment, toShipment);
			}

			var writer = new UniversalDataBuss.XmlIO.XmlWriting.XmlWriter();

			if (!string.IsNullOrWhiteSpace(userDefinedXmlNamespace))
			{
				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					var xmlReplacer = new XmlNamespaceReplacer();
					writer.WriteXML(dataObject, stream, ns, builder);

					var newNamespace = xmlReplacer.GetCustomNamespace(ns, userDefinedXmlNamespace);
					xmlReplacer.ReplaceXmlWithNamespace(stream, outputStream, newNamespace);
				}
			}
			else
			{
				writer.WriteXML(dataObject, outputStream, ns, builder);
			}

			outputStream.Seek(0, SeekOrigin.Begin);
		}

		#region CopySpecialNote

		void CopyNotes(Shipment from, Shipment to)
		{
			if (from.NoteCollection == null
				|| from.NoteCollection.Count == 0)
			{
				return;
			}

			if (to.NoteCollection == null)
			{
				to.SetNoteCollection(() => new UniversalDataBuss.DataObjects.Core.DataObjectList<Note>(from.NoteCollection));
			}
			else
			{
				var existingNotes = new HashSet<string>(
					to
					.NoteCollection
					.Select(n => n.Description.ToString()));

				foreach (var note in from.NoteCollection)
				{
					if (!existingNotes.Contains(note.Description))
					{
						to.NoteCollection.Add(note);
					}
				}
			}
		}

		#endregion
	}
}
