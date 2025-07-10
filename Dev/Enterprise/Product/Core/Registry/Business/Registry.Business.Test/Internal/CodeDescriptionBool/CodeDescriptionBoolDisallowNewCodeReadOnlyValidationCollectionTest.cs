using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection))]
	sealed class CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollectionTest : CodeDescriptionBoolCollectionAbstractTest<CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection>
	{
		public override void TestDefaultBoolForNewChild()
		{
			var collectionType = Collection.GetType();
			var collection = (CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection)Activator.CreateInstance(collectionType, new object[] { new ZArchitecture.Core.StaffAssignmentRoles() });
			AssertEquals("collection.AddNew().Bool", false, collection.AddNew().Bool);
			var clone = (CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection)collection.Clone(null, Factory);
			AssertEquals("clone.AddNew().Bool", false, clone.AddNew().Bool);
		}

		public void TestAllowNew()
		{
			AssertEquals("The AllowNew property should be false", false, Collection.AllowNew);
		}

		#region Implementation

		protected override CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection GetCollectionToTest() => new CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CodeDescriptionBoolDisallowNewCodeReadOnlyValidation();

		new CodeDescriptionBoolDisallowNewCollection Collection => new CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection();

		#endregion
	}
}
