using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.MFI.CaroTrans.Testing
{
	public class CaroTransOrderDataImporterTest : FlatFileDataImporterTestCase
	{
		public void TestExtractToDataAdapter()
		{
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testFileBytes = resourceRetriever.GetBytes("Import.CaroTransOrderTestFile.cti");
				CaroTransOrderFlatFileConverter converter = new CaroTransOrderFlatFileConverter(new NotificationBuffer(), Factory);
				Xsd.Orders xsdOrders = new Xsd.Orders();
				using (StreamReader dataReader = new StreamReader(new MemoryStream(testFileBytes)))
				{
					converter.ImportFlatFile(xsdOrders, new CsvFlatFileFormat(), dataReader);
				}

				MockCaroTransOrderDataImporter importer = new MockCaroTransOrderDataImporter(Factory);
				bool anyOrderImported = importer.ExtractToDataAdapter(xsdOrders, new NotificationBuffer());
				AssertEquals("No order imported", false, anyOrderImported);
				OrgPatternMatchOverride orgMatch = SetOrgMatch(xsdOrders.Order[0].OrderDetail.Buyer.OwnerCode);
				anyOrderImported = importer.ExtractToDataAdapter(xsdOrders, new NotificationBuffer());
				AssertEquals("Some orders imported", true, anyOrderImported);
			}
		}

		public void TestOrderDoesNotGetUpdatedIfItIsAttachedToAShipment()
		{
			Xsd.Orders xsdOrders = new Xsd.Orders();
			Xsd.Order xsdOrder = xsdOrders.Order.AddNew();
			xsdOrder.OrderDetail = new Xsd.OrderOrderDetail();
			xsdOrder.OrderDetail.Buyer = new Xsd.Organisation();
			xsdOrder.OrderDetail.Buyer.OwnerCode = "Buyer";
			xsdOrder.OrderDetail.ConfirmNumber = "R55";
			xsdOrder.OrderIdentifier.OrderNumber = "P000001";
			OrgPatternMatchOverride orgMatch = SetOrgMatch(xsdOrder.OrderDetail.Buyer.OwnerCode);
			Order orderBizObj = Factory.New<Order>();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			orderBizObj.BuyerPK = importContext.FindOrganisationPK(xsdOrder.OrderDetail.Buyer, null, OrganisationTypes.None);
			orderBizObj.JD_JS = Factory.New(typeof(ForwardingShipment)).PK;
			Factory.Save();
			MockCaroTransOrderDataImporter importer = new MockCaroTransOrderDataImporter(Factory);
			importer.ExtractToDataAdapter(xsdOrders, importContext);
			AssertEquals("HasChanges", false, orderBizObj.HasChanges);
			AssertEquals("BookingConfRef", "", orderBizObj.JD_BookingConfRef);
			orderBizObj.JD_JS = ZGuid.Empty;
			Factory.Save();
			importer.ExtractToDataAdapter(xsdOrders, importContext);
			AssertEquals("HasChanges", true, orderBizObj.HasChanges);
			AssertEquals("BookingConfRef", "R55", orderBizObj.JD_BookingConfRef);
		}

		#region Implementation
		protected override FlatFileDataImporter GetDataImporter()
		{
			return new CaroTransOrderDataImporter();
		}

		protected OrgPatternMatchOverride SetOrgMatch(string code)
		{
			OrgPatternMatchOverride orgMatch = Factory.New<OrgPatternMatchOverride>();
			orgMatch.OO_ForeignCode = code;
			orgMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			OrgHeader localOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.OrgProxy.PK));
			orgMatch.OO_LocalGuid = localOrg.PK;
			orgMatch.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			return orgMatch;
		}

		public class MockCaroTransOrderDataImporter : CaroTransOrderDataImporter
		{
			public MockCaroTransOrderDataImporter(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notify, out ITransactionParticipant[] additionalTransactionActions)
			{
				base.ImportDataToFactoryCore(dataReader, attachmentFileName, notify, out additionalTransactionActions);
				return false;
			}

			public new bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
			{
				return base.ExtractToDataAdapter(xSD, notifications);
			}
		}

		EmbeddedResourceRetriever ResourceRetriever;
		string TestFilePath;
		protected override string PathToTestFile { get { return TestFilePath; } }
		protected override void SetUp()
		{
			base.SetUp();
			ResourceRetriever = new EmbeddedResourceRetriever();
			TestFilePath = ResourceRetriever.SaveResourceToFile("Import.CaroTransOrderTestFile.cti");
		}
		protected override void TearDown()
		{
			base.TearDown();
			ResourceRetriever.Dispose();
		}
		#endregion
	}
}
