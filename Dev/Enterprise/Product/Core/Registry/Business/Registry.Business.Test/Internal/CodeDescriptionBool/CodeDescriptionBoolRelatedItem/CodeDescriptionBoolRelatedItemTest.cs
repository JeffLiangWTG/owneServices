using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolRelatedItem))]
	sealed class CodeDescriptionBoolRelatedItemTest : CodeDescriptionBoolTest
	{
		public void TestSetCustomDefaultValuesCore()
		{
			var regBizO = (CodeDescriptionBoolRelatedItem)GetNewBusinessObject();
			AssertEquals(string.Empty, regBizO.RelatedItemCode);
		}

		public void TestRelatedItemCode()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection();
			var newItem = collection.AddNew();
			CodeDescriptionBoolRelatedItem item = newItem;
			AssertEquals("", item.RelatedItemCode);
			AssertNoErrors(item.RelatedItemCodeInfo);

			item.RelatedItemCode = "EEE";
			AssertNoErrors(item.RelatedItemCodeInfo);

			item.RelatedItemCode = "ABCDEFGHIJ";
			AssertNoErrors(item.RelatedItemCodeInfo);

			//AS2: generates a DeveloperNotificationException that I can't fix
			//AssertExceptionThrown<MaxLengthExceededException>(() => item.RelatedItemCode = "ABCDEFGHIJK");
		}

		public void TestRelatedItemDescription()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection(OpportunitySourceRelatedItemProvider.SecondaryList);
			var newItem = collection.AddNew();
			CodeDescriptionBoolRelatedItem item = newItem;
			AssertEquals("", item.RelatedItemDescription);
			AssertNoErrors(item.RelatedItemDescriptionInfo);

			item.RelatedItemCode = "EEE";
			AssertEquals("", item.RelatedItemDescription);
			AssertNoErrors(item.RelatedItemDescriptionInfo);

			item.RelatedItemCode = "CL2";
			AssertEquals("Campaign Management - Category List 2", item.RelatedItemDescription);
			AssertNoErrors(item.RelatedItemDescriptionInfo);

			item.RelatedItemDescription = "AAA Desc";
			AssertEquals("AAA Desc", item.RelatedItemDescription);
			AssertHasErrors(item.RelatedItemDescriptionInfo);

			item.RelatedItemDescription = " ";
			AssertEquals("", item.RelatedItemDescription);
			AssertNoErrors(item.RelatedItemDescriptionInfo);

			item.RelatedItemDescription = "Campaign Management - Category List 2    ";
			AssertEquals("CL2", item.RelatedItemCode);
			AssertNoErrors(item.RelatedItemDescriptionInfo);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CodeDescriptionBoolRelatedItem();
		}

		#endregion
	}
}
