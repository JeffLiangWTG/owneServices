using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[TestedType(typeof(CustomizableDataTranslationCollection))]
	sealed class CustomizableDataTranslationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CustomizableDataTranslationCollection>
	{
		protected override CustomizableDataTranslationCollection GetCollectionToTest()
		{
			return new CustomizableDataTranslationCollection();
		}

		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestAddNew()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert(true);
		}

		public override void TestTypedget_Item()
		{
			Assert(true);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			throw new NotImplementedException();
		}
	}
}
