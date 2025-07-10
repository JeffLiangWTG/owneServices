using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ClientInTemplateSelectionCriteria))]
	sealed class ClientInTemplateSelectionCriteriaTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestMoveAndReset()
		{
			var original = ClientInTemplateSelectionCriteria.GetDefaultValueByCode("SHP");
			var clone = (ClientInTemplateSelectionCriteria)original.Clone(original.CurrentFallbackLevel, original.Factory);

			clone.SelectedItems.MoveItem(1, 0);

			AssertEquals("Items have been transposed", original.SelectedItems[1].OrgTypeCode, clone.SelectedItems[0].OrgTypeCode);
			AssertEquals("Items have been transposed", original.SelectedItems[0].OrgTypeCode, clone.SelectedItems[1].OrgTypeCode);

			var itemToMove = clone.SelectedItems[0];
			clone.SelectedItems.Remove(itemToMove);
			clone.AvailableItems.Add(itemToMove);

			AssertEquals("Item has been moved", itemToMove.OrgTypeCode, clone.AvailableItems[1].OrgTypeCode);

			clone.Reset();

			for (int i = 0; i < original.SelectedItems.Count; i++)
			{
				AssertEquals("SelectedItems are reset", original.SelectedItems[i].OrgTypeCode, clone.SelectedItems[i].OrgTypeCode);
			}

			for (int i = 0; i < original.AvailableItems.Count; i++)
			{
				AssertEquals("AvailableItems are reset", original.AvailableItems[i].OrgTypeCode, clone.AvailableItems[i].OrgTypeCode);
			}
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return ClientInTemplateSelectionCriteria.GetDefaultValueByCode("SHP");
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
