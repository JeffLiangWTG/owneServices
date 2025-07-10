using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class APInvoiceConsolCostCollectionBizoTest : TestCaseWithFactory
	{
		public void TestGetConsolSummaryWhenFireListChanged()
		{
			var consol = TestObjectCreator.CreateConsol();
			Factory.Save();

			var collection = CollectionToTest;
			var collectionInfo = new ImportCollectionInfoImpl(collection)
			{
				new ImportPropertyInfoImpl<JobConsolCost>(JobConsolCostSchema.Constants.E6_ParentID) { HeaderText = "Guid" },
				new ImportPropertyInfoImpl<JobConsolCost>(JobConsolCostSchema.Constants.E6_ParentTableCode) { HeaderText = "Txt" },
				new ImportPropertyInfoImpl<JobConsolCost>(JobConsolCostSchema.Constants.E6_LocalCostAmount) { HeaderText = "Decimal" },
			};

			(collection as IBindingList).ListChanged += ListChanged_ForTestOnly;

			var mockery = new MockRepository(MockBehavior.Default);
			var wizard = new Mock<ImportWizard>(collectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, false)).Returns(new List<string[]>()
			{
				new string[] { consol.CostSupporter.PK.ToString(), consol.CostSupporter.Type, "1.00" },
				new string[] { consol.CostSupporter.PK.ToString(), consol.CostSupporter.Type, "2.00" },
				new string[] { consol.CostSupporter.PK.ToString(), consol.CostSupporter.Type, "3.00" },
			});

			wizard.Object.Mapping[0].AddFileColumnIndex(0); //E6_ParentID
			wizard.Object.Mapping[1].AddFileColumnIndex(1); //E6_ParentTableCode
			wizard.Object.Mapping[2].AddFileColumnIndex(2); //E6_LocalCostAmount

			AssertEquals(0, collection.Count);
			AssertEquals(0, collection.FireListChangedCount_ForTestOnly);
			AssertEquals(1, collection.ParentAPInvoice.ConsolCosting.ConsolSummary.UpdateCount_ForTestOnly);

			wizard.Object.ImportIntoCollection(collection);

			AssertEquals(3, collection.Count);
			AssertEquals(1, collection.FireListChangedCount_ForTestOnly);
			AssertEquals(2, collection.ParentAPInvoice.ConsolCosting.ConsolSummary.UpdateCount_ForTestOnly);
			AssertEquals(1, collection.ParentAPInvoice.ConsolCosting.ConsolSummary.Count);
			AssertEquals(6m, collection.ParentAPInvoice.ConsolCosting.ConsolSummary[0].LocalTotalAmount);
			AssertEquals(6m, LocalTotalAmount_WhenGetThroughListChanged);
		}

		public void ListChanged_ForTestOnly(object sender, ListChangedEventArgs e)
		{
			CollectionToTest.FireListChangedCount_ForTestOnly++;
			LocalTotalAmount_WhenGetThroughListChanged = CollectionToTest.ParentAPInvoice?.ConsolCosting?.ConsolSummary[0]?.LocalTotalAmount;
		}
		ZDecimal? LocalTotalAmount_WhenGetThroughListChanged;

		APInvoiceConsolCostCollection CollectionToTest
		{
			get
			{
				if (collectionToTest == null)
				{
					APInvoice inv = Factory.New<APInvoice>();
					collectionToTest = inv.ConsolCosting.ConsolCosts;
				}
				return collectionToTest;
			}
		}
		APInvoiceConsolCostCollection collectionToTest;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
