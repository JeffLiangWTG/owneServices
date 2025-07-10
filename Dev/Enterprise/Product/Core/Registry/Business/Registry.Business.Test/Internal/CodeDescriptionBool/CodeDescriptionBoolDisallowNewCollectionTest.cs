using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolDisallowNewCollection))]
	sealed class CodeDescriptionBoolDisallowNewCollectionTest : CodeDescriptionBoolCollectionAbstractTest<CodeDescriptionBoolDisallowNewCollection>
	{
		public override void TestDefaultBoolForNewChild()
		{
			Type collectionType = Collection.GetType();
			CodeDescriptionBoolDisallowNewCollection collection = (CodeDescriptionBoolDisallowNewCollection)Activator.CreateInstance(collectionType, new object[] { new ZArchitecture.Core.StaffAssignmentRoles() });
			AssertEquals("collection.AddNew().Bool", false, collection.AddNew().Bool);
			CodeDescriptionBoolDisallowNewCollection clone = (CodeDescriptionBoolDisallowNewCollection)collection.Clone(null, Factory);
			AssertEquals("clone.AddNew().Bool", false, clone.AddNew().Bool);
		}

		public void TestAllowNew()
		{
			AssertEquals("The AllowNew property should be false", false, Collection.AllowNew);
		}

		#region Implementation

		protected override CodeDescriptionBoolDisallowNewCollection GetCollectionToTest()
		{
			return new CodeDescriptionBoolDisallowNewCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CodeDescriptionBoolDisallowNew();
		}

		new CodeDescriptionBoolDisallowNewCollection Collection
		{
			get { return new CodeDescriptionBoolDisallowNewCollection(); }
		}

		#endregion
	}
}
