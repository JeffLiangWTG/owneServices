using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HouseBillOfLadingTermsAndConditionsCollection))]
	sealed class HouseBillOfLadingTermsAndConditionsCollectionTest : RegistryImageCollectionTest<HouseBillOfLadingTermsAndConditionsCollection>
	{
		public void TestFindByCodeAndDeliveryMode()
		{
			HouseBillOfLadingTermsAndConditions element1 = Collection.AddNew();
			HouseBillOfLadingTermsAndConditions element2 = Collection.AddNew();
			HouseBillOfLadingTermsAndConditions element3 = Collection.AddNew();

			element1.Code = "abc";
			element1.Description = (NoResString)"abc1";
			element1.DeliveryMode = nameof(PrintCopyType.ALL);

			element2.Code = "abc";
			element2.Description = (NoResString)"abc2";
			element2.DeliveryMode = nameof(PrintCopyType.EML);

			element3.Code = "xyz";
			element3.Description = (NoResString)"xyz1";
			element3.DeliveryMode = nameof(PrintCopyType.ALL);

			HouseBillOfLadingTermsAndConditions obtainedElement = Collection.FindByCodeAndDeliveryMode("abc", nameof(PrintCopyType.ALL));
			AssertEquals("Code", "abc", obtainedElement.Code);
			AssertEquals("Description", "abc1", obtainedElement.Description);
			AssertEquals("DeliveryMode", nameof(PrintCopyType.ALL), obtainedElement.DeliveryMode);

			obtainedElement = Collection.FindByCodeAndDeliveryMode("abc", "arrrrrr");
			AssertEquals("Code", "abc", obtainedElement.Code);
			AssertEquals("Description", "abc1", obtainedElement.Description);
			AssertEquals("DeliveryMode", nameof(PrintCopyType.ALL), obtainedElement.DeliveryMode);

			obtainedElement = Collection.FindByCodeAndDeliveryMode("abc", nameof(PrintCopyType.EML));
			AssertEquals("Code", "abc", obtainedElement.Code);
			AssertEquals("Description", "abc2", obtainedElement.Description);
			AssertEquals("DeliveryMode", nameof(PrintCopyType.EML), obtainedElement.DeliveryMode);

			obtainedElement = Collection.FindByCodeAndDeliveryMode("xyz", nameof(PrintCopyType.ALL));
			AssertEquals("Code", "xyz", obtainedElement.Code);
			AssertEquals("Description", "xyz1", obtainedElement.Description);
			AssertEquals("DeliveryMode", nameof(PrintCopyType.ALL), obtainedElement.DeliveryMode);

			obtainedElement = Collection.FindByCodeAndDeliveryMode("xyz", "waaaaaa");
			AssertEquals("Code", "xyz", obtainedElement.Code);
			AssertEquals("Description", "xyz1", obtainedElement.Description);
			AssertEquals("DeliveryMode", nameof(PrintCopyType.ALL), obtainedElement.DeliveryMode);

			obtainedElement = Collection.FindByCodeAndDeliveryMode("gah", "bah");
			Assert("Nothing should be found", obtainedElement == null);
		}

		#region Implementation

		protected override HouseBillOfLadingTermsAndConditionsCollection GetCollectionToTest()
		{
			return new HouseBillOfLadingTermsAndConditionsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new HouseBillOfLadingTermsAndConditions
			{
				ImagePkForTest = ZGuid.NewZGuid(),
				Code = nextCode++.ToString()
			};
		}

		int nextCode;

		#endregion
	}
}
