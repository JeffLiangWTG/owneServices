using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection))]
	sealed class CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollectionTest : CodeDescriptionBoolCollectionAbstractTest<CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection>
	{
		public override void TestDefaultBoolForNewChild()
		{
			var collectionType = Collection.GetType();
			var collection = (CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection)Activator.CreateInstance(collectionType, new object[] { new ZArchitecture.Core.StaffAssignmentRoles() });
			AssertEquals("collection.AddNew().Bool", false, collection.AddNew().Bool);
			var clone = (CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection)collection.Clone(null, Factory);
			AssertEquals("clone.AddNew().Bool", false, clone.AddNew().Bool);
		}

		public void TestAllowNew()
		{
			AssertEquals("The AllowNew property should be false", false, Collection.AllowNew);
		}

		#region Implementation

		protected override CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection GetCollectionToTest() => new CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraString();

		new CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection Collection => new CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection();

		#endregion
	}
}
