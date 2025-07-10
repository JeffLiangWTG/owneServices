using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.BillCustomisationStrategies;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BillOfLadingNumberCustomisationElementView))]
	sealed class BillOfLadingNumberCustomisationElementViewTest : NonPersistentBusinessObjectCollectionTestCase<BillOfLadingNumberCustomisationElementView>
	{
		public void TestFilter()
		{
			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();
			BillOfLadingNumberCustomisationElementCollection innerCollection = new BillOfLadingNumberCustomisationElementCollection(customisation);
			BillOfLadingNumberCustomisationElementView outerCollection = new BillOfLadingNumberCustomisationElementView(innerCollection);

			var standardElement1 = new BillOfLadingNumberCustomisationElement(customisation, new CommonElementStrategy("key1", "Standard 1", "", NumberCustomisationElementCategories.Standard, 3, null));
			var standardElement2 = new BillOfLadingNumberCustomisationElement(customisation, new CommonElementStrategy("key2", "Standard 2", "", NumberCustomisationElementCategories.Standard, 3, null));
			var linerAgencyElement1 = new BillOfLadingNumberCustomisationElement(customisation, new CommonElementStrategy("key1", "Liner & Agency 1", "", NumberCustomisationElementCategories.LinerAgency, 3, null));
			var linerAgencyElement2 = new BillOfLadingNumberCustomisationElement(customisation, new CommonElementStrategy("key2", "Liner & Agency 2", "", NumberCustomisationElementCategories.LinerAgency, 3, null));

			innerCollection.Add(standardElement1);
			innerCollection.Add(linerAgencyElement1);

			outerCollection.SetCategories(NumberCustomisationElementCategories.None);
			AssertContainsExactElementsInAnyOrder("None",
				(e) => e.ElementName,
				System.Array.Empty<BillOfLadingNumberCustomisationElement>(),
				outerCollection.ToArray<BillOfLadingNumberCustomisationElement>());

			outerCollection.SetCategories(NumberCustomisationElementCategories.Standard);
			AssertContainsExactElementsInAnyOrder("Standard Only",
				(e) => e.ElementName,
				new BillOfLadingNumberCustomisationElement[] { standardElement1 },
				outerCollection.ToArray<BillOfLadingNumberCustomisationElement>());

			outerCollection.SetCategories(NumberCustomisationElementCategories.LinerAgency);
			AssertContainsExactElementsInAnyOrder("Liner & Agency Only",
				(e) => e.ElementName,
				new BillOfLadingNumberCustomisationElement[] { linerAgencyElement1 },
				outerCollection.ToArray<BillOfLadingNumberCustomisationElement>());

			innerCollection.Add(standardElement2);
			innerCollection.Add(linerAgencyElement2);

			outerCollection.SetCategories(NumberCustomisationElementCategories.LinerAgency);
			AssertContainsExactElementsInAnyOrder("Liner & Agency Only",
				(e) => e.ElementName,
				new BillOfLadingNumberCustomisationElement[] { linerAgencyElement1, linerAgencyElement2 },
				outerCollection.ToArray<BillOfLadingNumberCustomisationElement>());

			outerCollection.SetCategories(NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency);
			AssertContainsExactElementsInAnyOrder("Both Standard and Liner & Agency",
				(e) => e.ElementName,
				new BillOfLadingNumberCustomisationElement[] { linerAgencyElement1, linerAgencyElement2, standardElement1, standardElement2 },
				outerCollection.ToArray<BillOfLadingNumberCustomisationElement>());

			innerCollection.Remove(linerAgencyElement2);
			AssertContainsExactElementsInAnyOrder("Both Standard and Liner & Agency",
				(e) => e.ElementName,
				new BillOfLadingNumberCustomisationElement[] { linerAgencyElement1, standardElement1, standardElement2 },
				outerCollection.ToArray<BillOfLadingNumberCustomisationElement>());
		}

		public void TestHasIncludedElementWithCheckDigit()
		{
			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();
			BillOfLadingNumberCustomisationElementCollection innerCollection = new BillOfLadingNumberCustomisationElementCollection(customisation);
			BillOfLadingNumberCustomisationElementView outerCollection = new BillOfLadingNumberCustomisationElementView(innerCollection);

			var standardElement1 = new BillOfLadingNumberCustomisationElement(customisation, new CommonElementStrategy("key1", "Standard 1", "", NumberCustomisationElementCategories.Standard, 3, null));
			var standardElement2 = new BillOfLadingNumberCustomisationElement(customisation, new CommonElementStrategy("key2", "Standard 2", "", NumberCustomisationElementCategories.Standard, 3, null));
			var linerAgencyElement1 = new BillOfLadingNumberCustomisationElement(customisation, new CommonElementStrategy("key1", "Liner & Agency 1", "", NumberCustomisationElementCategories.LinerAgency, 3, null));
			var linerAgencyElement2 = new BillOfLadingNumberCustomisationElement(customisation, new CommonElementStrategy("key2", "Liner & Agency 2", "", NumberCustomisationElementCategories.LinerAgency, 3, null));

			innerCollection.Add(standardElement1);
			innerCollection.Add(linerAgencyElement1);

			standardElement1.Include = true;
			standardElement1.CheckDigit = false;
			linerAgencyElement1.Include = true;
			linerAgencyElement1.CheckDigit = false;

			outerCollection.SetCategories(NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency);
			Assert(!outerCollection.HasIncludedElementWithCheckDigit);

			standardElement1.CheckDigit = true;
			linerAgencyElement1.CheckDigit = false;
			Assert(outerCollection.HasIncludedElementWithCheckDigit);

			standardElement1.CheckDigit = false;
			linerAgencyElement1.CheckDigit = true;
			Assert(outerCollection.HasIncludedElementWithCheckDigit);

			standardElement1.CheckDigit = true;
			linerAgencyElement1.CheckDigit = true;
			Assert(outerCollection.HasIncludedElementWithCheckDigit);

			standardElement1.CheckDigit = false;
			linerAgencyElement1.CheckDigit = false;
			Assert(!outerCollection.HasIncludedElementWithCheckDigit);
		}

		#region Implementation

		protected override BillOfLadingNumberCustomisationElementView GetCollectionToTest()
		{
			return new BillOfLadingNumberCustomisationElementView(new BillOfLadingNumberCustomisation().UnFilteredElements);
		}

		int itemsMade;
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var item = new BillOfLadingNumberCustomisationElement(new BillOfLadingNumberCustomisation(), new NullElementStrategy());
			item.Order = (byte)(itemsMade++);

			return item;
		}

		#endregion
	}
}
