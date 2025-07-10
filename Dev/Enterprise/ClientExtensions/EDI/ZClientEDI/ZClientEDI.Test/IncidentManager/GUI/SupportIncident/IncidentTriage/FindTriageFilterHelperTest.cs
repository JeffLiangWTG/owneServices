using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(FindTriageFilterHelper))]
	public class FindTriageFilterHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTriageCollectionDescriptionKeywords()
		{
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_SupportDescription = "Hello there Alex";
			triage1.IMT_Type = IncidentTriageTypes.Codes.Service;
			var triage2 = Factory.NewWithValidTestData<IncidentTriage>();
			triage2.IMT_SupportDescription = "Hello banana";
			triage2.IMT_Type = IncidentTriageTypes.Codes.Service;
			var triage3 = Factory.NewWithValidTestData<IncidentTriage>();
			triage3.IMT_SupportDescription = "Hello there General";
			triage3.IMT_Type = IncidentTriageTypes.Codes.Service;
			var triage4 = Factory.NewWithValidTestData<IncidentTriage>();
			triage4.IMT_SupportDescription = "Bananas";
			triage4.IMT_Type = IncidentTriageTypes.Codes.Service;
			var triage5 = Factory.NewWithValidTestData<IncidentTriage>();
			triage5.IMT_SupportDescription = "Hello";
			triage5.IMT_Type = IncidentTriageTypes.Codes.Compliance;
			Factory.Save();

			Helper.DescriptionKeyword1 = "Hello";
			Helper.DescriptionKeyword2 = "there";
			Helper.DescriptionKeyword3 = "Alex";
			Helper.NodeType = IncidentTriageTypes.Codes.Service;

			Helper.RefreshTriageCollection();
			var collection = Helper.TriageCollection;
			AssertEquals("Should have 3 matches via description filters + type filter", 3, collection.Count);
			AssertEquals("Should rank triage1 highest since it matches all 3 keywords", triage1.PK, collection[0].PK);
			AssertEquals("Should rank triage3 second since it matches 2 keywords", triage3.PK, collection[1].PK);
			AssertEquals("Should rank triage2 lowest since it matches 1 keyword", triage2.PK, collection[2].PK);

			Helper.DescriptionKeyword1 = null;
			Helper.RefreshTriageCollection();
			AssertEquals("Should have 2 matches via description filters + type filter", 2, collection.Count);
			AssertEquals("Should rank triage1 highest since it matches all 2 keywords", triage1.PK, collection[0].PK);
			AssertEquals("Should rank triage3 second since it matches 1 keyword", triage3.PK, collection[1].PK);

			Helper.DescriptionKeyword2 = " ";
			Helper.RefreshTriageCollection();
			AssertEquals("Should have 1 match via description filters + type filter", 1, collection.Count);
			AssertEquals("Should rank triage1 with keyword Alex", triage1.PK, collection[0].PK);
		}

		public void TestTriageCollectionShowInternalOnly()
		{
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_IsInternal = true;
			var triage2 = Factory.NewWithValidTestData<IncidentTriage>();
			triage2.IMT_IsInternal = false;
			Factory.Save();

			Helper.ShowInternalOnly = true;
			Helper.RefreshTriageCollection();
			var collection = Helper.TriageCollection;
			AssertEquals("Should have only return triage1", 1, collection.Count);
			AssertEquals("Should have only return triage1", triage1.PK, collection[0].PK);

			Helper.ShowInternalOnly = false;
			Helper.RefreshTriageCollection();
			AssertEquals("Should return all triages", 2, collection.Count);
		}

		public void TestTriageCollectionNodeType()
		{
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_Type = IncidentTriageTypes.Codes.Compliance;
			var triage2 = Factory.NewWithValidTestData<IncidentTriage>();
			triage2.IMT_Type = IncidentTriageTypes.Codes.Service;
			Factory.Save();

			Helper.NodeType = IncidentTriageTypes.Codes.Compliance;
			Helper.RefreshTriageCollection();
			var collection = Helper.TriageCollection;
			AssertEquals("Should have only return triage1", 1, collection.Count);
			AssertEquals("Should have only return triage1", triage1.PK, collection[0].PK);

			Helper.NodeType = IncidentTriageTypes.Codes.Service;
			Helper.RefreshTriageCollection();
			AssertEquals("Should have only return triage2", 1, collection.Count);
			AssertEquals("Should have only return triage2", triage2.PK, collection[0].PK);
		}

		public void TestTriageCollectionProduct()
		{
			var productCollection = new SystemProductCollection();
			var product1 = productCollection.AddNew();
			product1.Code = "ZZZ";
			product1.Description = "AAA Aardvark";
			product1.Enabled = true;

			var product2 = productCollection.AddNew();
			product2.Code = "AAA";
			product2.Description = "ZZZ Zebra";
			product2.Enabled = true;

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productCollection);

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_Product = product1.Code;
			var triage2 = Factory.NewWithValidTestData<IncidentTriage>();
			triage2.IMT_Product = product2.Code;
			Factory.Save();

			Helper.Product = product1.Code;
			Helper.RefreshTriageCollection();
			var collection = Helper.TriageCollection;
			AssertEquals("Should have only return triage1", 1, collection.Count);
			AssertEquals("Should have only return triage1", triage1.PK, collection[0].PK);

			Helper.Product = product2.Code;
			Helper.RefreshTriageCollection();
			AssertEquals("Should have only return triage2", 1, collection.Count);
			AssertEquals("Should have only return triage2", triage2.PK, collection[0].PK);
		}

		public void TestTriageCollectionProductArea()
		{
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_ProductArea = ProductAreaList.Codes.ARC;
			var triage2 = Factory.NewWithValidTestData<IncidentTriage>();
			triage2.IMT_ProductArea = ProductAreaList.Codes.CUS;
			Factory.Save();

			Helper.ProductArea = ProductAreaList.Codes.ARC;
			Helper.RefreshTriageCollection();
			var collection = Helper.TriageCollection;
			AssertEquals("Should have only return triage1", 1, collection.Count);
			AssertEquals("Should have only return triage1", triage1.PK, collection[0].PK);

			Helper.ProductArea = ProductAreaList.Codes.CUS;
			Helper.RefreshTriageCollection();
			AssertEquals("Should have only return triage2", 1, collection.Count);
			AssertEquals("Should have only return triage2", triage2.PK, collection[0].PK);
		}

		public void TestRefreshTriageCollection()
		{
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_SupportDescription = "Hello there";
			Factory.Save();

			var collection = Helper.TriageCollection;
			AssertEquals("Should not have loaded collection yet", 0, collection.Count);

			Helper.RefreshTriageCollection();
			AssertEquals("Should have loaded collection", 1, collection.Count);

			Helper.DescriptionKeyword1 = "other";
			AssertEquals("Should not have refreshed collection yet", 1, collection.Count);
			Helper.RefreshTriageCollection();
			AssertEquals("Should have refreshed collection", 0, collection.Count);

			Helper.DescriptionKeyword2 = "hello";
			AssertEquals("Should not have refreshed collection yet", 0, collection.Count);
			Helper.RefreshTriageCollection();
			AssertEquals("Should have refreshed collection", 1, collection.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Helper = new FindTriageFilterHelper(incident);
		}

		protected override BusinessObject GetNewBusinessObject() => Helper;

		FindTriageFilterHelper Helper;
	}
}
