using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ClientInTemplateSelectionCriteriaCollection))]
	sealed class ClientInTemplateSelectionCriteriaCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ClientInTemplateSelectionCriteriaCollection>
	{
		public void TestGetValueByCode()
		{
			var collection = ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultCollectionValue;
			AssertEquals("SHP exists", "SHP", collection.GetValueByCode("SHP").ProcessTaskCode);
			AssertNull("XYZ doesn't exist", collection.GetValueByCode("XYZ"));
		}

		#region Implementation

		protected override ClientInTemplateSelectionCriteriaCollection GetCollectionToTest()
		{
			return new ClientInTemplateSelectionCriteriaCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ClientInTemplateSelectionCriteria();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
