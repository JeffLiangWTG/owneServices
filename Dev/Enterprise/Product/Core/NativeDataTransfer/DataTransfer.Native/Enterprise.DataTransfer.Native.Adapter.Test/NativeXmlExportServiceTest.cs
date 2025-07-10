using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Adapter
{
	public class NativeXmlExportServiceTest : TransactionedTestCase
	{
		public void TestExportShipmentIncludesOnlyItsConsols()
		{
			var factory = new BusinessObjectFactory();
			var shipment1 = factory.NewWithValidTestData<ForwardingShipment>();
			var consol1 = shipment1.Consols.AddNew();
			consol1.FillWithValidTestData();
			var consol2 = shipment1.Consols.AddNew();
			consol2.FillWithValidTestData();
			var shipment2 = factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.Consols.AddNew().FillWithValidTestData();
			factory.Save();

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(Export(shipment1))))
			using (var reader = new XmlTextReader(stream))
			{
				var doc = XDocument.Load(reader);
				var nsManager = new XmlNamespaceManager(reader.NameTable);
				XNamespace ns = "http://www.cargowise.com/Schemas/Native/2011/11";
				nsManager.AddNamespace("p", ns.NamespaceName);
				var consols = doc.XPathSelectElements("//p:Native/p:Body/p:Shipment/p:JobShipment/p:JobConsolCollection/p:JobConsol", nsManager);
				AssertEquals(2, consols.Count());
				AssertNoExceptionThrown(() => consols.Single(consol => consol.Element(ns + "PK").Value == consol1.PK.ToString()));
				AssertNoExceptionThrown(() => consols.Single(consol => consol.Element(ns + "PK").Value == consol2.PK.ToString()));
			}
		}

		public void TestExportSelfReference()
		{
			var factory = new BusinessObjectFactory();
			var groupHeader = factory.LoadTop1<Enterprise.Integration.Customs.AU.IJobComInvoiceGroupHeader>(new ZQuery());
			AssertNull("Precondition", groupHeader);
			var declaration = factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			groupHeader = factory.LoadTop1<Enterprise.Integration.Customs.AU.IJobComInvoiceGroupHeader>(new ZQuery());
			AssertNotNull("Group header was created automatically", groupHeader);
			groupHeader.JZ_JE = declaration.PK;
			groupHeader.JZ_Volume = 100;
			var header1 = factory.New<Enterprise.Integration.Customs.AU.IJobComInvoiceHeader>();
			header1.JZ_JE = declaration.PK;
			header1.JZ_JZ_GroupInvoiceFK = groupHeader.PK;
			header1.JZ_Volume = 10;
			header1.JZ_InvoiceNumber = "1";
			var header2 = factory.New<Enterprise.Integration.Customs.AU.IJobComInvoiceHeader>();
			header2.JZ_JE = declaration.PK;
			header2.JZ_JZ_GroupInvoiceFK = groupHeader.PK;
			header2.JZ_Volume = 20;
			header2.JZ_InvoiceNumber = "2";
			factory.Save();

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(Export(declaration))))
			using (var reader = new XmlTextReader(stream))
			{
				var doc = XDocument.Load(reader);
				var nsManager = new XmlNamespaceManager(reader.NameTable);
				XNamespace ns = "http://www.cargowise.com/Schemas/Native/2011/11";
				nsManager.AddNamespace("p", ns.NamespaceName);
				var headers = doc.XPathSelectElements("//p:Native/p:Body/p:Declaration/p:JobDeclaration/p:JobComInvoiceHeaderCollection/p:JobComInvoiceHeader", nsManager);
				AssertEquals(3, headers.Count());
				var groupHeaderElement = headers.Single(header => header.Element(ns + "PK").Value == groupHeader.PK.ToString());

				var groupInvoiceFK = "<GroupInvoiceFK TableName=\"JobComInvoiceHeader\" xmlns=\"http://www.cargowise.com/Schemas/Native/2011/11\" />";
				AssertEquals(groupInvoiceFK, groupHeaderElement.XPathSelectElement("./p:GroupInvoiceFK", nsManager).ToString());
				var header1Element = headers.Single(header => header.Element(ns + "PK").Value == header1.PK.ToString());
				AssertEquals("Group Header Reference",
					groupHeader.PK.ToString(),
					header1Element.XPathSelectElement("./p:GroupInvoiceFK[@TableName='JobComInvoiceHeader']/p:PK", nsManager).Value);
				var header2Element = headers.Single(header => header.Element(ns + "PK").Value == header2.PK.ToString());
				AssertEquals("Group Header Reference",
					groupHeader.PK.ToString(),
					header2Element.XPathSelectElement("./p:GroupInvoiceFK[@TableName='JobComInvoiceHeader']/p:PK", nsManager).Value);
			}
		}

		[RequiresSTA]
		[GuiTest]
		public void TestExport()
		{
			const string content = "This is test content";
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(content);
				writer.Flush();

				var businessObjects = new[] { businessObject.Object };
				fileLocator.Setup(x => x.GetFileStream(businessObjects, out fileName)).Returns(File.OpenWrite(fileName));
				serializer.Setup(x => x.Export(businessObjects, null, null)).Returns(stream);

				exporter.Export(businessObjects);
				AssertEquals("File should be created", true, File.Exists(fileName));
				AssertEquals("All content Serializer created from Business Objects must be written to File", content, File.ReadAllText(fileName));

				AssertEquals("changes doesnt get set because adding logs throws error", false, (businessObject.Object.HasChanges));
			}
		}

		[RequiresSTA]
		[GuiTest]
		public void TestExport_ActualBusinessObject()
		{
			var factory = new BusinessObjectFactory();
			var shipment1 = factory.NewWithValidTestData<ForwardingShipment>();
			const string content = "This is test content";
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(content);
				writer.Flush();

				var businessObjects = new[] { shipment1 };
				fileLocator.Setup(x => x.GetFileStream(businessObjects, out fileName)).Returns(File.OpenWrite(fileName));
				serializer.Setup(x => x.Export(businessObjects, null, null)).Returns(stream);

				exporter.Export(businessObjects);
				AssertEquals("File should be created", true, File.Exists(fileName));
				AssertEquals("All content Serializer created from Business Objects must be written to File", content, File.ReadAllText(fileName));

				AssertEquals(true, shipment1.HasChanges);
			}
		}

		[RequiresSTA]
		[GuiTest]
		public void TestExportWithSave_ActualBusinessObject_Single()
		{
			var factory = new BusinessObjectFactory();
			var shipment1 = factory.NewWithValidTestData<ForwardingShipment>();
			const string content = "This is test content";
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(content);
				writer.Flush();

				var businessObjects = new[] { shipment1 };
				fileLocator.Setup(x => x.GetFileStream(businessObjects, out fileName)).Returns(File.OpenWrite(fileName));
				serializer.Setup(x => x.Export(businessObjects, null, null)).Returns(stream);

				exporter.ExportWithSave(businessObjects);
				AssertEquals("File should be created", true, File.Exists(fileName));
				AssertEquals("All content Serializer created from Business Objects must be written to File", content, File.ReadAllText(fileName));

				AssertEquals(false, shipment1.HasChanges);
			}
		}

		[RequiresSTA]
		[GuiTest]
		public void TestExportWithSave_ActualBusinessObject_Multiple()
		{
			var firstFactory = new BusinessObjectFactory();
			var shipment1 = firstFactory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "1";
			var anotherFactory = new BusinessObjectFactory();
			var shipment2 = anotherFactory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "2";
			const string content = "This is test content";
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(content);
				writer.Flush();

				var businessObjects = new[] { shipment1, shipment2 };
				fileLocator.Setup(x => x.GetFileStream(businessObjects, out fileName)).Returns(File.OpenWrite(fileName));
				serializer.Setup(x => x.Export(businessObjects, null, null)).Returns(stream);

				exporter.ExportWithSave(businessObjects);
				AssertEquals("File should be created", true, File.Exists(fileName));
				AssertEquals("All content Serializer created from Business Objects must be written to File", content, File.ReadAllText(fileName));

				AssertEquals(false, shipment1.HasChanges);
				AssertEquals(false, shipment2.HasChanges);
			}
		}

		DataRow Filter(DataTable table) => table.Rows.Cast<DataRow>().First();

		[RequiresSTA]
		[GuiTest]
		public void TestExportWithSave_ActualBusinessObject_Multiple_WithFilter()
		{
			var firstFactory = new BusinessObjectFactory();
			var shipment1 = firstFactory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "1";
			var anotherFactory = new BusinessObjectFactory();
			var shipment2 = anotherFactory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "2";
			const string content = "This is test content";

			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(content);
				writer.Flush();

				var businessObjects = new[] { shipment1, shipment2 };
				fileLocator.Setup(x => x.GetFileStream(businessObjects, out fileName)).Returns(File.OpenWrite(fileName));
				serializer.Setup(x => x.Export(businessObjects, null, Filter)).Returns(stream);

				exporter.ExportWithSave(businessObjects, Filter);
				AssertEquals("File should be created", true, File.Exists(fileName));
				AssertEquals("All content Serializer created from Business Objects must be written to File", content, File.ReadAllText(fileName));

				AssertEquals(false, shipment1.HasChanges);
				AssertEquals(false, shipment2.HasChanges);
			}
		}

		[RequiresSTA]
		[GuiTest]
		public void TestExportWithSave()
		{
			const string content = "This is test content";
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(content);
				writer.Flush();

				var businessObjects = new[] { businessObject.Object };
				fileLocator.Setup(x => x.GetFileStream(businessObjects, out fileName)).Returns(File.OpenWrite(fileName));
				serializer.Setup(x => x.Export(businessObjects, null, null)).Returns(stream);

				exporter.ExportWithSave(businessObjects);
				AssertEquals("File should be created", true, File.Exists(fileName));
				AssertEquals("All content Serializer created from Business Objects must be written to File", content, File.ReadAllText(fileName));

				AssertEquals(false, businessObject.Object.HasChanges);
			}
		}

		public void TestExport_EmptyFileName()
		{
			IEnumerable<IBusiness> businessObjects = new[] { businessObject.Object };
			string fileName = string.Empty;
			fileLocator.Setup(x => x.GetFileStream(businessObjects, out fileName)).Returns((FileStream)null);

			exporter.Export(businessObjects);
			AssertEquals("File should not be created", false, File.Exists(fileName));
		}

		[ExpectNoExceptions]
		public void TestExport_ValidationFail()
		{
			IEnumerable<IBusiness> businessObjects = Array.Empty<IBusiness>();
			const string errorMsg = "Error Message";

			validator.Setup(v => v.Validate(businessObjects)).Throws(new ArgumentException(errorMsg));
			logger.Setup(l => l.ShowError(errorMsg));

			exporter.Export(businessObjects);

			validator.Verify(v => v.Validate(businessObjects), Times.Once());
			logger.Verify(l => l.ShowError(errorMsg), Times.Once);
		}

		public void TestExport_ReferencedCodeDoesntExist()
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = "XXXXXXXXXXXX";
			orgHeader.OH_RL_NKClosestPort = "XXXXX"; // a port code which doesn't exist
			factory.Save();

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(Export(orgHeader))))
			using (var reader = new XmlTextReader(stream))
			{
				var doc = XDocument.Load(reader);
				var nsManager = new XmlNamespaceManager(reader.NameTable);
				XNamespace ns = "http://www.cargowise.com/Schemas/Native/2011/11";
				nsManager.AddNamespace("p", ns.NamespaceName);
				var closestPortNode = doc.XPathSelectElements("//p:Native/p:Body/p:Organization/p:OrgHeader/p:ClosestPort", nsManager);
				var codeNodes = closestPortNode.Elements(ns + "Code");
				AssertEquals(1, codeNodes.Count());
				AssertEquals("XXXXX", codeNodes.ElementAt(0).Value);
			}
		}

		public void TestExport_OptionalEntity()
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = "XXXXXXXXXXXX";
			var orgPatternMatchOverride = factory.NewWithValidTestData<OrgPatternMatchOverride>();
			orgPatternMatchOverride.OO_OH = orgHeader.PK;
			factory.Save();

			using (OrganisationsDataRegistry.Instance.EnableOrgPatternMatchOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(Export(orgHeader))))
			using (var reader = new XmlTextReader(stream))
			{
				var doc = XDocument.Load(reader);
				var nsManager = new XmlNamespaceManager(reader.NameTable);
				XNamespace ns = "http://www.cargowise.com/Schemas/Native/2011/11";
				nsManager.AddNamespace("p", ns.NamespaceName);
				var orgPatternMatchOverrideCollection = doc.XPathSelectElements("//p:Native/p:Body/p:Organization/p:OrgHeader/p:OrgPatternMatchOverrideCollection", nsManager);
				AssertEquals(1, orgPatternMatchOverrideCollection.Count());
				var orgPatternMatchOverrides = orgPatternMatchOverrideCollection.Elements(ns + "OrgPatternMatchOverride");
				AssertEquals(1, orgPatternMatchOverrides.Count());
			}

			using (OrganisationsDataRegistry.Instance.EnableOrgPatternMatchOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(Export(orgHeader))))
			using (var reader = new XmlTextReader(stream))
			{
				var doc = XDocument.Load(reader);
				var nsManager = new XmlNamespaceManager(reader.NameTable);
				XNamespace ns = "http://www.cargowise.com/Schemas/Native/2011/11";
				nsManager.AddNamespace("p", ns.NamespaceName);
				var orgPatternMatchOverrideCollection = doc.XPathSelectElements("//p:Native/p:Body/p:Organization/p:OrgHeader/p:OrgPatternMatchOverrideCollection", nsManager);
				AssertEquals(false, orgPatternMatchOverrideCollection.Any());
			}
		}

		#region Implementation

		static string Export(IBusiness obj)
		{
			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var serializer = new NativeXmlSerializer() { Converter = converter };
			var exporter = new NativeXmlExportService() { Serializer = serializer };
			var tempFile = TempForTest.GetTempFileName();
			using (var stream = new FileStream(tempFile, FileMode.Open))
			{
				exporter.Export(new[] { obj }, stream);
			}
			var result = File.ReadAllText(tempFile);
			File.Delete(tempFile);
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();

			mocks = new MockRepository(MockBehavior.Default);
			fileLocator = mocks.Create<ISaveFileLocator>();
			serializer = mocks.Create<IBusinessSerializer>();
			validator = mocks.Create<IExportValidator>();
			logger = mocks.Create<IUserNotification>();
			businessObject = mocks.Create<IBusiness>();
			exporter = new NativeXmlExportService
			{
				Serializer = serializer.Object,
				Validator = validator.Object,
				FileLocator = fileLocator.Object,
				Logger = logger.Object
			};
			fileName = Path.Combine(EnvProxy.Instance.TempPath, Guid.NewGuid().ToString());
		}

		protected override void TearDown()
		{
			base.TearDown();
			FileExtensions.DeleteFileIfExists(fileName);
		}

		NativeXmlExportService exporter;
		Mock<IBusinessSerializer> serializer;
		Mock<IBusiness> businessObject;
		Mock<ISaveFileLocator> fileLocator;
		Mock<IExportValidator> validator;
		Mock<IUserNotification> logger;
		MockRepository mocks;
		string fileName;

		#endregion
	}
}
