using System.IO;
using System.Xml;
using System.Xml.Schema;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Xml.Testing
{
	public abstract class ZXmlSchemaBaseTest : TestCaseWithFactory
	{
		public ZXmlSchemaBaseTest(bool useNamespaceManager)
		{
			this.UseNamespaceManager = useNamespaceManager;
		}

		public void TestImplicitOperatorZXmlSchemaToXmlSchema()
		{
			ZXmlSchema zSchema = new ZXmlSchema();
			XmlSchema schema = zSchema;
			AssertNotNull(schema);
		}

		public void TestImplicitOperatorXmlSchemaToZXmlSchema()
		{
			XmlSchema schema = new XmlSchema();
			ZXmlSchema zSchema = schema;
			AssertNotNull(zSchema);
		}

		public void TestElements()
		{
			ZXmlSchema schema = new XmlSchema();
			AssertNotNull("Elements property", schema.Elements);
		}

		public void TestItems()
		{
			ZXmlSchema schema = new XmlSchema();
			AssertNotNull("Items property", schema.Items);
		}

		public void TestIncludes()
		{
			ZXmlSchema schema = new ZXmlSchema();
			AssertNotNull("Includes property", schema.Includes);
		}

		readonly bool UseNamespaceManager;

		#region Read

		public void TestReadStream()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				ZXmlSchema dummySchema = new ZXmlSchema();
				dummySchema.Write(stream);
				stream.Flush();
				stream.Position = 0;

				ZXmlSchema schema = ZXmlSchema.Read(stream, null);
				AssertNotNull("Should read the schema successfully", schema);
			}
		}

		public void TestReadTextReader()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				ZXmlSchema dummySchema = new ZXmlSchema();
				dummySchema.Write(stream);
				stream.Flush();
				stream.Position = 0;

				ZXmlSchema schema = ZXmlSchema.Read(new StreamReader(stream), null);
				AssertNotNull("Should read the schema successfully", schema);
			}
		}

		#endregion

		#region Write

		public void TestWriteStream()
		{
			ZXmlSchema schema = new ZXmlSchema();
			using (MemoryStream stream = new MemoryStream())
			{
				if (UseNamespaceManager)
				{
					schema.Write(stream, new XmlNamespaceManager(new NameTable()));
				}
				else
				{
					schema.Write(stream);
				}
				Assert("Should write to the stream", stream.ToArray().Length > 0);
			}
		}

		public void TestWriteXmlWriter()
		{
			ZXmlSchema schema = new ZXmlSchema();
			StringWriter writer = new StringWriter();
			XmlTextWriter xmlWriter = new XmlTextWriter(writer);

			if (UseNamespaceManager)
			{
				schema.Write(xmlWriter, new XmlNamespaceManager(new NameTable()));
			}
			else
			{
				schema.Write(xmlWriter);
			}
			xmlWriter.Flush();
			Assert("Should write to the xml writer", writer.GetStringBuilder().Length > 0);
		}

		public void TestWriteTextWriter()
		{
			ZXmlSchema schema = new ZXmlSchema();
			StringWriter writer = new StringWriter();
			if (UseNamespaceManager)
			{
				schema.Write(writer, new XmlNamespaceManager(new NameTable()));
			}
			else
			{
				schema.Write(writer);
			}
			Assert("Should write to the text writer", writer.GetStringBuilder().Length > 0);
		}

		#endregion
	}
}
