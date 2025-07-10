using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.Wow.Testing
{
	public class ProductCsvRecordTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportPartsCsv()
		{
			new WowTestUtil().SetupDummyUnmatchOrgAndNotifyGroup(Factory);
			new WowTestUtil().DeleteAllOrders(Factory);
			ZQuery filter = new ZQuery();
			filter.OrderBy = WoolworthsProduct.Schema.OP_PartNum;
			OrgSupplierPartCollection parts = new OrgSupplierPartCollection(Factory, filter);
			parts.Load();
			parts.RemoveAndDeleteAll();
			using (StreamReader fileContents = new WowTestUtil().GetStreamReaderForTestFile("DataImportExport\\Parts\\Testing\\TestProducts.csv"))
			{
				ITransactionParticipant[] transactionActions;
				DataImporter.ImportDataToFactory(fileContents, "", new NotificationBuffer(null), SourceInfo.EmptySourceInfo, out transactionActions);
				parts.Load();
				// check the count, note that some PartNumS are duplicates so they will 'refresh'
				AssertEquals("Total number of parts now in factory", 10, parts.Count);
				foreach (WoolworthsProduct part in parts)
				{
					AssertNotNull("each part should have the wowimporter as the owner", part.RelatedOrganisations.FindByOrganisationPKAndRelationship(WowDataRegistry.Instance.DeclarationImporter, OrgPartRelation.RelationshipTypes.Owner));
				}

				// make sure the data was migrated correctly
				AssertEquals("Correct unit of quantity", Enterprise.Core.Constants.PkgUnit.Unit, parts[0].OP_StockKeepingUnit);
				new WowTestUtil().AssertBusinessPropsEquals(parts[0], ProductCsvRecord.PartPropertyMappings, null, new ZString("000048"), null, new ZDecimal(12), new ZDecimal(12), null, new ZString("246"), new ZString("DINNER PLATE 26CM,  SEA & SUN"), new ZString("S"), null, new ZDecimal(3.58), new ZString("LOOSE CROCKERY -  CUPS / SAUCERS"), null, null, new ZDecimal(45.5));
				AssertEquals("OP_WeightedCost", 1.79m, parts[0].OP_WeightedCost);
				new WowTestUtil().AssertBusinessPropsEquals(parts[9], ProductCsvRecord.PartPropertyMappings, null, new ZString("902061"), null, new ZDecimal(1), new ZDecimal(12), null, new ZString("376"), new ZString("HIGHLAND LEGEND     SCOTCH WHISKY 700ML"), new ZString("S"), null, new ZDecimal(221.63), new ZString("WHISKY"), null, null, new ZDecimal(23.5));
				AssertEquals("OP_WeightedCost", 0m, parts[9].OP_WeightedCost);
				AssertEquals("OP_QtyInStock populated", new ZDecimal(6324 + 5), parts[0].OP_QtyInStock);
				new WowTestUtil().AssertBusinessPropsEquals(parts[0].Locations[0], ProductCsvRecord.PartLocationPropertyMappings, null, null, new ZString("1899"), null, null, new ZString(""), null, null, null, new ZDecimal(6324), new ZDecimal(6324), new ZDecimal(1.79), null, (ZShort)16, (ZShort)5);
				new WowTestUtil().AssertBusinessPropsEquals(parts[0].Locations[1], ProductCsvRecord.PartLocationPropertyMappings, null, null, new ZString("1904"), null, null, new ZString(""), null, null, null, new ZDecimal(5), new ZDecimal(5), new ZDecimal(1.79), null, (ZShort)99, (ZShort)1);
				// make sure doing this a second time produces no changes
				new WowTestUtil().CheckNoChangesAfterRunningImportSecondTime(Factory, DataImporter, fileContents, new NotificationBuffer(null));
			}
		}

		public void TestPartBuyerSetWhenOrdersPlacedOnPart()
		{
			ProductCsvRecord partRecord = new ProductCsvRecord(new OCsvLine(new string[] { "1", "part1", "1899", "000000012", "000000012", "", "246", "", "", "00000000006324.0000", "000000001.7900", "", "16", "5", "43.5" }).ToString());
			new WowTestUtil().SetupDummyUnmatchOrgAndNotifyGroup(Factory);
			// set up some dummy organisations
			OrgHeader buyerOnOrders = Factory.New<OrgHeader>();
			buyerOnOrders.MainAddress.OA_Address1 = "x";
			buyerOnOrders.OH_Code = "---";
			OrgHeader someRandomBuyer = Factory.New<OrgHeader>();
			someRandomBuyer.MainAddress.OA_Address1 = "y";
			someRandomBuyer.OH_Code = "x-x";
			// set up some dummy order/lines
			WoolworthsOrder order = Factory.New<WoolworthsOrder>();
			order.JD_OrderNumber = "testorder1";
			order.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.SupplierPK = buyerOnOrders.PK;
			OrderLine line = order.OrderLines.AddNew();
			line.JO_LineNo = 1;
			// this part is using the fallback organisation, so it needs to be populated
			WoolworthsProduct part = Factory.New<WoolworthsProduct>();
			part.OP_PartNum = "part2";
			part.RelatedOrganisations.AddOrganisationIfNotExist(buyerOnOrders.PK, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();
			line.JO_Partno = "part2";
			order.BuyerPK = buyerOnOrders.PK;
			Factory.Save();
			partRecord.UpdateBusinessData(new BusinessObjectFactoryProvider(Factory), new NotificationBuffer(null));
			AssertEquals("Part should have the buyer from the order", buyerOnOrders.PK, part.RelatedOrganisations[0].OU_OH);
			AssertEquals("Part's related organisation should be of type buyer", OrgPartRelation.RelationshipTypes.Owner, part.RelatedOrganisations[0].OU_Relationship);
		}

		class TestWowDataImporter : WowDataImporter
		{
			public TestWowDataImporter(BusinessObjectFactory factory) : base(new SingleBusinessObjectFactoryProvider(factory))
			{
			}
		}

		TestWowDataImporter DataImporter
		{
			get
			{
				if (fDataImporter == null)
				{
					fDataImporter = new TestWowDataImporter(Factory);
				}

				return fDataImporter;
			}
		}

		TestWowDataImporter fDataImporter;
	}
}
