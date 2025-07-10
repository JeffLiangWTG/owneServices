using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ClientInTemplateSelectionCriteriaOrgTypesCollection))]
	sealed class ClientInTemplateSelectionCriteriaOrgTypesCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ClientInTemplateSelectionCriteriaOrgTypesCollection>
	{
		public void TestGetValueByCode()
		{
			var collection = ClientInTemplateSelectionCriteria.GetDefaultValueByCode("SHP").SelectedItems;
			AssertNotNull("LOC exists", collection.GetValueByCode("LOC"));
			AssertNull("XYZ doesn't exist", collection.GetValueByCode("XYZ"));
		}

		public void TestContainsCode()
		{
			var collection = ClientInTemplateSelectionCriteria.GetDefaultValueByCode("SHP").SelectedItems;
			AssertEquals("Contains CON", true, collection.ContainsCode("CON"));
			AssertEquals("Doesn't contain XYZ", false, collection.ContainsCode("XYZ"));
		}

		public void TestMove()
		{
			var original = ClientInTemplateSelectionCriteria.GetDefaultValueByCode("SHP");
			original.SelectedItems.Add(original.AvailableItems.GetValueByCode("CPY"));

			var clone = (ClientInTemplateSelectionCriteria)original.Clone(original.CurrentFallbackLevel, original.Factory);

			clone.SelectedItems.MoveItem(1, 2);

			AssertEquals("Item 0 is unchanged", original.SelectedItems[0].OrgTypeCode, clone.SelectedItems[0].OrgTypeCode);
			AssertEquals("Item 1 moves to 2", original.SelectedItems[1].OrgTypeCode, clone.SelectedItems[2].OrgTypeCode);
			AssertEquals("Item 2 moves to 1", original.SelectedItems[2].OrgTypeCode, clone.SelectedItems[1].OrgTypeCode);
		}

		#region Implementation

		protected override ClientInTemplateSelectionCriteriaOrgTypesCollection GetCollectionToTest()
		{
			return new ClientInTemplateSelectionCriteriaOrgTypesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ClientInTemplateSelectionCriteriaOrgType();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
