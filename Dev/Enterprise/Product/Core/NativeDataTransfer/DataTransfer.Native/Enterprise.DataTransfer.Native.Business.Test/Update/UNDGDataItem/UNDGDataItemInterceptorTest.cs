using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.UNDGDataItem
{
	class UNDGDataItemInterceptorTest : TestCaseWithFactory
	{
		public void TestMatchingWithUNNOOnly()
		{
			var sessionServices = new AncillaryImportServices();
			var undgDataItem = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem"), sessionServices);
			undgDataItem["PK"] = ZGuid.NewZGuid();

			var undgSubs = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem.UNDGSubstance"), sessionServices);
			undgSubs["UNNO"] = "2005";

			undgDataItem.ParentCollection.Add(undgSubs);

			var order = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart"), sessionServices);
			order.ChildrenCollection.Add(undgDataItem);

			InvokeInterceptor(order, sessionServices);

			var expectedPK = UNDGSubstanceLoader.LoadSubstances(Factory, "2005", "", "IMO").First().PK;
			AssertEquals(expectedPK, undgSubs.InternalPK);
		}

		public void TestMatchingWithValidPKOnly()
		{
			var sessionServices = new AncillaryImportServices();
			var undgDataItem = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem"), sessionServices);
			undgDataItem["PK"] = ZGuid.NewZGuid();

			var realPk = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;

			var undgSubs = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem.UNDGSubstance"), sessionServices);
			undgSubs["PK"] = realPk;

			undgDataItem.ParentCollection.Add(undgSubs);

			var order = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart"), sessionServices);
			order.ChildrenCollection.Add(undgDataItem);

			InvokeInterceptor(order, sessionServices);

			var expectedPK = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertEquals(expectedPK, undgSubs.InternalPK);
		}

		public void TestMatchingWithoutStandard()
		{
			var sessionServices = new AncillaryImportServices();
			var undgDataItem = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem"), sessionServices);
			undgDataItem["PK"] = ZGuid.NewZGuid();

			var undgSubs = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem.UNDGSubstance"), sessionServices);
			undgSubs["UNNO"] = "0004";
			undgSubs["Variant"] = "a";

			undgDataItem.ParentCollection.Add(undgSubs);

			var order = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart"), sessionServices);
			order.ChildrenCollection.Add(undgDataItem);

			InvokeInterceptor(order, sessionServices);

			var expectedPK = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertEquals(expectedPK, undgSubs.InternalPK);
		}

		public void TestMatchingIATRecordByCode()
		{
			var sessionServices = new AncillaryImportServices();
			var undgDataItem = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem"), sessionServices);
			undgDataItem["PK"] = ZGuid.NewZGuid();

			var undgSubs = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem.UNDGSubstance"), sessionServices);
			undgSubs["Code"] = "0007";
			undgSubs["UniqueRecordId"] = "ANY";
			undgSubs["Standard"] = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;

			undgDataItem.ParentCollection.Add(undgSubs);

			var order = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart"), sessionServices);
			order.ChildrenCollection.Add(undgDataItem);

			var query = new ZQuery(UNDGSubstanceSchema.DG_Standard, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA);
			query.AddToFilter(UNDGSubstanceSchema.DG_Code, "0007");
			var iataSubstance = Factory.LoadTop1<UNDGSubstance>(query) ?? Factory.New<UNDGSubstance>();
			if (iataSubstance.DG_Code.IsEmpty)
			{
				iataSubstance.DG_Code = "0007";
				iataSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
				Factory.Save();
			}

			InvokeInterceptor(order, sessionServices);

			var expectedPKIATStandard = iataSubstance.PK;
			AssertEquals(expectedPKIATStandard, undgSubs.InternalPK);
		}

		public void TestMatchAccordingToStandard()
		{
			var sessionServices = new AncillaryImportServices();
			var undgDataItemIMO = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem"), sessionServices);
			undgDataItemIMO["PK"] = ZGuid.NewZGuid();
			var undgDataItemIATA = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem"), sessionServices);
			undgDataItemIATA["PK"] = ZGuid.NewZGuid();

			var undgSubsIMO = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem.UNDGSubstance"), sessionServices);
			undgSubsIMO["Code"] = "0007";
			undgSubsIMO["Variant"] = "";
			undgSubsIMO["UNNO"] = "0007";
			undgSubsIMO["Standard"] = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var undgSubsIATA = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem.UNDGSubstance"), sessionServices);
			undgSubsIATA["UniqueRecordId"] = "999";
			undgSubsIATA["Standard"] = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;

			undgDataItemIMO.ParentCollection.Add(undgSubsIMO);
			undgDataItemIATA.ParentCollection.Add(undgSubsIATA);

			var order = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart"), sessionServices);
			order.ChildrenCollection.Add(undgDataItemIMO);
			order.ChildrenCollection.Add(undgDataItemIATA);

			InvokeInterceptor(order, sessionServices);

			var expectedPKIMOStandard = UNDGSubstanceLoader.LoadSubstances(Factory, "0007", "", "IMO").First().PK;
			AssertEquals(expectedPKIMOStandard, undgSubsIMO.InternalPK);

			var query = new ZQuery(UNDGSubstanceSchema.DG_UniqueRecordId, "999");
			query.AddToFilter(new ZQuery(UNDGSubstanceSchema.DG_Standard, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA));
			var iataSubstance = Factory.LoadTop1<UNDGSubstance>(query) ?? Factory.New<UNDGSubstance>();
			if (iataSubstance.DG_UniqueRecordId.IsEmpty)
			{
				iataSubstance.DG_UniqueRecordId = "999";
				iataSubstance.DG_Standard = "IAT";
				Factory.Save();
			}

			InvokeInterceptor(order, sessionServices);
			var expectedPKIATStandard = iataSubstance.PK;
			AssertEquals(expectedPKIATStandard, undgSubsIATA.InternalPK);
		}

		public void TestUNDGSubstancePivotEntity_CreatedWhenMissing()
		{
			var sessionServices = new AncillaryImportServices();
			var undgDataItem = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem"), sessionServices);
			undgDataItem["PK"] = ZGuid.NewZGuid();

			var undgSubs = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem.UNDGSubstance"), sessionServices);
			undgSubs["UNNO"] = "2005";

			undgDataItem.ParentCollection.Add(undgSubs);

			var order = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart"), sessionServices);
			order.ChildrenCollection.Add(undgDataItem);

			var undgSubsPivotExists = undgDataItem.ChildrenCollection.Any(x => x.EntityName == UNDGSubstancePivotSchema.Constants.TableName);
			Assert("PRE: UNDGSubstancePivot does not exist in entity child collection", !undgSubsPivotExists);

			InvokeInterceptor(order, sessionServices);

			var undgSubsPivots = undgDataItem.ChildrenCollection.Where(x => x.EntityName == UNDGSubstancePivotSchema.Constants.TableName);
			AssertEquals("UNDGDataItemInterceptor creates a UNDGSubstancePivot entity when missing", 1, undgSubsPivots.Count());
		}

		public void TestUNDGSubstancePivotEntity_CreatedWhenMissing_MatchesSubstanceAttributesInDB()
		{
			var sessionServices = new AncillaryImportServices();
			var undgDataItem = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem"), sessionServices);
			undgDataItem["PK"] = ZGuid.NewZGuid();

			var undgSubsInDatabase = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First();
			var undgSubs = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem.UNDGSubstance"), sessionServices);
			undgSubs["PK"] = undgSubsInDatabase.PK;

			undgDataItem.ParentCollection.Add(undgSubs);

			var order = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart"), sessionServices);
			order.ChildrenCollection.Add(undgDataItem);

			var undgSubsPivotExists = undgDataItem.ChildrenCollection.Any(x => x.EntityName == UNDGSubstancePivotSchema.Constants.TableName);
			Assert("PRE: UNDGSubstancePivot does not exist in entity child collection", !undgSubsPivotExists);

			InvokeInterceptor(order, sessionServices);

			AssertNullOrEmpty("PRE: UNDGSubstanceEntity does not have UNNO details", undgSubs.GetPropertyOrBlankString("UNNO"));

			var undgSubsPivots = undgDataItem.ChildrenCollection.Where(x => x.EntityName == UNDGSubstancePivotSchema.Constants.TableName);
			AssertEquals("PRE: UNDGDataItemInterceptor creates a UNDGSubstancePivot entity when missing", 1, undgSubsPivots.Count());

			var undgSubsPivot = undgSubsPivots.First();
			CombineAssertions("Created UNDGSubstancePivot always matches UNDGSubstance details in database", () =>
			{
				AssertEquals(undgSubsInDatabase.DG_UNNO, undgSubsPivot["UNNO"]);
				AssertEquals(undgSubsInDatabase.DG_Variant, undgSubsPivot["Variant"]);
				AssertEquals(undgSubsInDatabase.DG_Standard, undgSubsPivot["Standard"]);
				Assert((bool)undgSubsPivot["IsDefault"]);

				AssertEquals(UNDGDataItemSchema.Constants.Prefix, undgSubsPivot["ParentTableCode"]);
			});
		}

		public void TestUNDGSubstancePivotEntity_IsNotCreated_WhenItAlreadyExists()
		{
			var sessionServices = new AncillaryImportServices();
			var undgDataItem = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem"), sessionServices);
			undgDataItem["PK"] = ZGuid.NewZGuid();

			var undgSubs = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem.UNDGSubstance"), sessionServices);
			undgSubs["UNNO"] = "3005";

			undgDataItem.ParentCollection.Add(undgSubs);

			var undgSubsPivot = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem.UNDGSubstancePivot"), sessionServices);
			undgSubsPivot["ParentTableCode"] = UNDGDataItemSchema.Constants.Prefix;
			undgSubsPivot["UNNO"] = "3005";

			undgDataItem.ChildrenCollection.Add(undgSubsPivot);

			var order = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart"), sessionServices);
			order.ChildrenCollection.Add(undgDataItem);

			var undgSubsPivotCountBeforeImport = undgDataItem.ChildrenCollection.Count(x => x.EntityName == UNDGSubstancePivotSchema.Constants.TableName);
			AssertGreaterThan("PRE: UNDGSubstancePivot exists in entity child collection", undgSubsPivotCountBeforeImport, 0);

			InvokeInterceptor(order, sessionServices);

			var undgSubsPivotCountAfterImport = undgDataItem.ChildrenCollection.Count(x => x.EntityName == UNDGSubstancePivotSchema.Constants.TableName);
			AssertEquals("UNDGDataItemInterceptor does not create a UNDGSubstancePivot entity when it already exists", undgSubsPivotCountBeforeImport, undgSubsPivotCountAfterImport);
		}

		public void TestUNDGSubstancePivotEntity_IsNotCreated_WhenThereIsNoSubstance()
		{
			var sessionServices = new AncillaryImportServices();
			var undgDataItem = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.UNDGDataItem"), sessionServices);
			undgDataItem["PK"] = ZGuid.NewZGuid();

			var order = new Entity(TestUtil.FindEntityDefinition("Product", "OrgSupplierPart"), sessionServices);
			order.ChildrenCollection.Add(undgDataItem);

			var undgSubsExists = undgDataItem.ParentCollection.Any(x => x.EntityName == UNDGSubstanceSchema.Constants.TableName);
			Assert("PRE: UNDGSubstance does not exist in entity parent collection", !undgSubsExists);

			InvokeInterceptor(order, sessionServices);

			var undgSubsPivotExists = undgDataItem.ChildrenCollection.Any(x => x.EntityName == UNDGSubstancePivotSchema.Constants.TableName);
			Assert("UNDGDataItemInterceptor does not create a UNDGSubstancePivot entity when no UNDGSubstance existed", !undgSubsPivotExists);
		}

		void InvokeInterceptor(IEntity order, AncillaryImportServices sessionServices)
		{
			var productSet = new EntitySet("Product");
			productSet.Root = order;
			var context = new UpdateContext(sessionServices, new FactoryProvider());
			var setting = new UNDGDataItemSetting { Context = context };

			var interceptor = new UNDGDataItemInterceptor(setting, sessionServices);
			interceptor.Function = e => { };
			interceptor.Invoke(productSet);
		}
	}
}
