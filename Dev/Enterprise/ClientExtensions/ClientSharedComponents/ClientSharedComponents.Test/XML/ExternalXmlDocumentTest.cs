using System;
using System.IO;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ClientSharedComponents.XML.Testing
{
	sealed class ExternalXmlDocumentTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConvertToValueObjects()
		{
			using (var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\Notes\Testing\PopulatedNote.xml"))
			{
				var xmlDoc = new ExternalXmlDocumentTestClass(reader, new NotificationBuffer());
				var notes = xmlDoc.ConvertToValueObjects();

				foreach (Xsd.NotesNote note in notes)
				{
					AssertNotNull(note);
					AssertEquals(typeof(Xsd.NotesNote), note.GetType());
					AssertEquals("TheNoteOfEternalSupriseAndFloundering", note.CustomNoteTypeName);
					AssertEquals("String Of Data somehow relating to this test", note.NoteData);
				}
			}
		}

		sealed class ExternalXmlDocumentTestClass : ExternalXmlDocument
		{
			public ExternalXmlDocumentTestClass(StreamReader externalXmlFile, INotifications notifications)
				: base(externalXmlFile, notifications)
			{
			}

			protected override XmlSchema DocumentSchema => XmlSchemaDefinitions.Instance.SingleNoteSchema;

			protected override string RootElementName => "Note";

			protected override Type RootElementType => typeof(Xsd.NotesNote);

			protected override IValueObject GetNewIValueObject() => new Xsd.NotesNote();
		}
	}
}
