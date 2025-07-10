using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection))]
	sealed class CodeDescriptionBoolDisallowNewWithDefaultDisabledCollectionTest : CodeDescriptionBoolCollectionAbstractTest<CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection>
	{
		public override void TestDefaultBoolForNewChild()
		{
			Type collectionType = Collection.GetType();
			CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection collection = (CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection)Activator.CreateInstance(collectionType, new object[] { new ZArchitecture.Core.StaffAssignmentRoles() });
			AssertEquals("collection.AddNew().Bool", false, collection.AddNew().Bool);
			CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection clone = (CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection)collection.Clone(null, Factory);
			AssertEquals("clone.AddNew().Bool", false, clone.AddNew().Bool);
		}

		public void TestAllowNew()
		{
			AssertEquals("The AllowNew property should be false", false, Collection.AllowNew);
		}

		#region Implementation

		protected override CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection GetCollectionToTest()
		{
			return new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CodeDescriptionBoolDisallowNewWithDefaultDisabled();
		}

		new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection Collection
		{
			get { return new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(); }
		}

		#endregion
	}
}
