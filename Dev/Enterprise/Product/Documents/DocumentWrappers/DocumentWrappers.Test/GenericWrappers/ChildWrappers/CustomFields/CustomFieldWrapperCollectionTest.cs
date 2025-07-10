using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CustomFieldWrapperCollection))]
	sealed class CustomFieldWrapperCollectionTest : GenericWrapperCollectionTest<CustomFieldWrapperCollection>
	{
		#region TestCustomIndexerListAttribute

		public void TestCustomIndexerListAttribute()
		{
			var list = CustomIndexerListAttribute.GetListForIndexer(typeof(CustomFieldWrapperCollection));
			AssertEquals("Indexer should be mapped correctly with attribute.", typeof(CustomFieldsList), list.GetType());
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.CustomLabels), list);
		}

		#endregion

		#region TestIndexer

		public void TestIndexer()
		{
			var collection = new CustomFieldWrapperCollection(Factory);
			AssertNull("Empty index will return null.", collection[""]);

			var customField1 = new CustomLabelInfo(Constants.CustomLabels.Organisation.CustomAttribute1, "", typeof(ZString), (NoResString)"", (NoResString)"", CustomLabelStyles.UpperCase, null, Factory);
			var customField2 = new CustomLabelInfo(Constants.CustomLabels.Organisation.CustomAttribute2, "", typeof(ZString), (NoResString)"", (NoResString)"", CustomLabelStyles.UpperCase, null, Factory);
			var wrapper1 = new CustomFieldWrapper(null, customField1, Factory);
			var wrapper2 = new CustomFieldWrapper(null, customField2, Factory);
			collection.Add(wrapper1);
			collection.Add(wrapper2);
			AssertEquals("Indexer should match on Field Name and return correct wrapper.", wrapper1, collection[Constants.CustomLabels.Organisation.CustomAttribute1]);
			AssertEquals("Indexer should match on Field Name and return correct wrapper.", wrapper2, collection[Constants.CustomLabels.Organisation.CustomAttribute2]);
			AssertNull("If field name is not in Collection should return null.", collection[Constants.CustomLabels.Organisation.CustomAttribute3]);
			AssertNull("If field name is not in Collection should return null.", collection["XXX"]);
		}

		#endregion

		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new CustomFieldWrapper(null, null, Factory);
		}

		protected override CustomFieldWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new CustomFieldWrapperCollection(Factory);
		}

		#endregion
	}
}
