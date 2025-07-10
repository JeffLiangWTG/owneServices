using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsDocketContainerCollection))]
	sealed class DocWhsDocketContainerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocWhsDocketContainerCollection>
	{
		#region Business Object Overrides

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			WhsDocketContainer docketContainer = Factory.New<WhsDocketContainer>();
			return DocWhsDocketContainer.New(docketContainer, Factory);
		}

		protected override DocWhsDocketContainerCollection GetCollectionToTest()
		{
			return new DocWhsDocketContainerCollection(Factory);
		}

		#endregion

		#region Collections

		public void TestToIDocSimpleContainerCollection()
		{
			var containerCollection = GetCollectionToTest();
			DocWhsDocketContainer container1 = GetDocWhsDocketContainer("C0001", "SN00001", "20GP");
			DocWhsDocketContainer container2 = GetDocWhsDocketContainer("C0002", "SN00002", "20GP");
			DocWhsDocketContainer container3 = GetDocWhsDocketContainer("C0003", "SN00003", "40GP");
			containerCollection.Add(container1);
			containerCollection.Add(container2);
			containerCollection.Add(container3);

			IDocSimpleContainerCollection testCollection = containerCollection.ToIDocSimpleContainerCollection();
			AssertEquals("All containers should be included", 3, testCollection.Count);

			for (int i = 0; i < testCollection.Count; i++)
			{
				AssertEqualInformation(containerCollection[i], testCollection[i]);
			}
		}

		#endregion

		#region Implementation

		DocWhsDocketContainer GetDocWhsDocketContainer(ZString containerNumber, ZString sealNumber, ZString containerType)
		{
			WhsDocketContainer container = Factory.New<WhsDocketContainer>();
			container.WC_ContainerNum = containerNumber;
			container.WC_SealNum = sealNumber;

			RefContainer docRefContainer = Factory.New<RefContainer>();
			docRefContainer.RC_Code = containerType;
			container.WC_RC = docRefContainer.PK;

			return DocWhsDocketContainer.New(container, Factory);
		}

		void AssertEqualInformation(DocWhsDocketContainer docketContainer, IDocSimpleContainer iDocContainer)
		{
			AssertEquals("Should have same Container Number", docketContainer.ContainerNumber, iDocContainer.ContainerNumber);
			AssertEquals("Should have same Seal Number", docketContainer.SealNumber, iDocContainer.SealNumber);
			AssertEquals("Should have same Type", docketContainer.Type, iDocContainer.Type);
		}

		#endregion
	}
}
