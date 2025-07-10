using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolDisallowNewCodeReadOnlyCollection))]
	sealed class CodeDescriptionBoolDisallowNewCodeReadOnlyCollectionTest : CodeDescriptionBoolCollectionAbstractTest<CodeDescriptionBoolDisallowNewCodeReadOnlyCollection>
	{
		public override void TestDefaultBoolForNewChild()
		{
			var collectionType = Collection.GetType();
			var collection = (CodeDescriptionBoolDisallowNewCodeReadOnlyCollection)Activator.CreateInstance(collectionType, new object[] { new ZArchitecture.Core.StaffAssignmentRoles() });
			AssertEquals("collection.AddNew().Bool", false, collection.AddNew().Bool);
			var clone = (CodeDescriptionBoolDisallowNewCodeReadOnlyCollection)collection.Clone(null, Factory);
			AssertEquals("clone.AddNew().Bool", false, clone.AddNew().Bool);
		}

		public void TestAllowNew()
		{
			AssertEquals("The AllowNew property should be false", false, Collection.AllowNew);
		}

		#region Implementation

		protected override CodeDescriptionBoolDisallowNewCodeReadOnlyCollection GetCollectionToTest() => new CodeDescriptionBoolDisallowNewCodeReadOnlyCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CodeDescriptionBoolDisallowNewCodeReadOnly();

		new CodeDescriptionBoolDisallowNewCollection Collection => new CodeDescriptionBoolDisallowNewCodeReadOnlyCollection();

		#endregion
	}
}
