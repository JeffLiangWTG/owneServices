using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.Business.Testing
{
	[TestedType(typeof(CollectionCopyTemplateBizo))]
	class CollectionCopyTemplateBizoTest : EntityCopyTemplateBizoTest<CollectionCopyTemplateBizo>
	{
		public void TestNoFilterNotification()
		{
			var collectionCopyBizo = GetNewCopyTemplateNodeBizo();

			collectionCopyBizo.CopyMethod = "FLT";
			collectionCopyBizo.FilterStripBizo = new FilterBusinessObjectForTest();

			collectionCopyBizo.RunPreSaveValidation();
			Assert(collectionCopyBizo.CopyMethodInfo.HasError($"Collection split {collectionCopyBizo.Name} is defined as Filtered, but no filter is set up."));
		}

		public void TestCopyMethod()
		{
			var collectionCopyBizo = GetNewCopyTemplateNodeBizo();

			collectionCopyBizo.CopyTemplateNode.CopyMethod = CollectionCopyMethod.None;
			AssertEquals(ZString.Empty, collectionCopyBizo.CopyMethod);
			collectionCopyBizo.CopyTemplateNode.CopyMethod = CollectionCopyMethod.All;
			AssertEquals("ALL", collectionCopyBizo.CopyMethod);
			collectionCopyBizo.CopyTemplateNode.CopyMethod = CollectionCopyMethod.Filter;
			AssertEquals("FLT", collectionCopyBizo.CopyMethod);

			collectionCopyBizo.CopyMethod = ZString.Empty;
			AssertEquals(CollectionCopyMethod.None, collectionCopyBizo.CopyTemplateNode.CopyMethod);
			collectionCopyBizo.CopyMethod = "ALL";
			AssertEquals(CollectionCopyMethod.All, collectionCopyBizo.CopyTemplateNode.CopyMethod);
			collectionCopyBizo.CopyMethod = "FLT";
			AssertEquals(CollectionCopyMethod.Filter, collectionCopyBizo.CopyTemplateNode.CopyMethod);

			collectionCopyBizo.CopyMethod = "XYZ";
			AssertEquals(CollectionCopyMethod.None, collectionCopyBizo.CopyTemplateNode.CopyMethod);
			AssertEquals("XYZ", collectionCopyBizo.CopyMethod);
		}

		public void TestCopyMethodRunsValidationOnChildrenWhenSetToNone()
		{
			var collectionCopyBizo = GetNewCopyTemplateNodeBizo();
			var propertyCopyBizo = PropertyConfigurationBizoTest.CreateNewCopyTemplateNodeBizo("Z0_Code", "String", collectionCopyBizo);
			propertyCopyBizo.CopyTemplateNode.IsMandatory = true;

			propertyCopyBizo.RunPreSaveValidation();
			AssertNoNotifications("Parent is not selected for copy - no validation on property", propertyCopyBizo.CopyMethodInfo);

			collectionCopyBizo.CopyMethod = "ALL";
			AssertNoNotifications("No automatic validation on property", propertyCopyBizo.CopyMethodInfo);

			propertyCopyBizo.RunPreSaveValidation();
			AssertHasErrors("Should have mandatory notification", propertyCopyBizo.CopyMethodInfo);

			collectionCopyBizo.CopyMethod = "FLT";
			AssertHasErrors("Still should have notification", propertyCopyBizo.CopyMethodInfo);

			collectionCopyBizo.CopyMethod = "";
			AssertNoNotifications("Property should be re-validated", propertyCopyBizo.CopyMethodInfo);
		}

		public void TestCopyMethods()
		{
			var collectionCopyBizo = GetNewCopyTemplateNodeBizo();

			collectionCopyBizo.EntityFilter = null;
			collectionCopyBizo.ResetCopyMethodsForTest();
			collectionCopyBizo.IsSplitCollection = true;
			AssertEquals(2, collectionCopyBizo.CopyMethods.Count);
			AssertEquals("", collectionCopyBizo.CopyMethods[0].Code);
			AssertEquals("FLT", collectionCopyBizo.CopyMethods[1].Code);

			collectionCopyBizo.EntityFilter = new EntityFilter();
			collectionCopyBizo.ResetCopyMethodsForTest();
			collectionCopyBizo.IsSplitCollection = false;
			AssertEquals(2, collectionCopyBizo.CopyMethods.Count);
			AssertEquals("", collectionCopyBizo.CopyMethods[0].Code);
			AssertEquals("ALL", collectionCopyBizo.CopyMethods[1].Code);

			collectionCopyBizo.EntityFilter.FilterTypeId = EntityFilterTypeIds.MandatoryExpressionFilter;
			collectionCopyBizo.ResetCopyMethodsForTest();
			AssertEquals(2, collectionCopyBizo.CopyMethods.Count);
			AssertEquals("", collectionCopyBizo.CopyMethods[0].Code);
			AssertEquals("CPY", collectionCopyBizo.CopyMethods[1].Code);

			collectionCopyBizo.EntityFilter.FilterTypeId = "ABC";
			collectionCopyBizo.ResetCopyMethodsForTest();
			AssertEquals(2, collectionCopyBizo.CopyMethods.Count);
			AssertEquals("", collectionCopyBizo.CopyMethods[0].Code);
			AssertEquals("ALL", collectionCopyBizo.CopyMethods[1].Code);
		}

		public void TestEntityFilter()
		{
			var filter1 = new EntityFilter();
			var filter2 = new EntityFilter();

			var collectionCopyBizo = GetNewCopyTemplateNodeBizo();

			collectionCopyBizo.CopyTemplateNode.Filter = filter1;
			AssertSame(filter1, collectionCopyBizo.EntityFilter);

			collectionCopyBizo.EntityFilter = filter2;
			AssertSame(filter2, collectionCopyBizo.CopyTemplateNode.Filter);
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CollectionCopyTemplateBizoValidation), GetNewCopyTemplateNodeBizo().Validation.GetType());
		}

		public void TestValidateCopyMethod()
		{
			var collectionCopyBizo = GetNewCopyTemplateNodeBizo();
			((CollectionCopyTemplateBizoValidation)collectionCopyBizo.Validation).ValidateCopyMethod();
			Assert(!collectionCopyBizo.CopyMethodInfo.HasNotifications());

			collectionCopyBizo.CopyMethod = "XYZ";
			((CollectionCopyTemplateBizoValidation)collectionCopyBizo.Validation).ValidateCopyMethod();
			Assert("Should have error about wrong copy method code.", collectionCopyBizo.CopyMethodInfo.HasErrors());

			collectionCopyBizo.CopyMethod = "ALL";
			((CollectionCopyTemplateBizoValidation)collectionCopyBizo.Validation).ValidateCopyMethod();
			Assert(!collectionCopyBizo.CopyMethodInfo.HasNotifications());
		}

		public void TestValidateCopyMethodForNonCopiedParent()
		{
			var parent = CreateNewCopyTemplateNodeBizo("Parent", "aaa", "bbb", null);
			var child = CreateNewCopyTemplateNodeBizo("Child", "xxx", "yyy", parent);
			((CollectionCopyTemplateBizoValidation)child.Validation).ValidateCopyMethod();
			Assert(!child.CopyMethodInfo.HasNotifications());

			child.CopyMethod = "ALL";
			((CollectionCopyTemplateBizoValidation)child.Validation).ValidateCopyMethod();
			AssertHasWarning(child.CopyMethodInfo, "To copy this Child please select a copy method on its parent Parent");

			parent.CopyMethod = "XYZ";
			((CollectionCopyTemplateBizoValidation)child.Validation).ValidateCopyMethod();
			Assert(!child.CopyMethodInfo.HasNotifications());
		}

		public void TestValidationErrorIfNothingToCopy()
		{
			var propertyNode = new PropertyCopyTemplateNode { CopyMethod = CopyMethod.None, Name = "X" };

			var innerNode = new EntityCopyTemplateNode { Name = "Dummies" };
			innerNode.Nodes.Add(propertyNode);

			var collectionNode = new CollectionCopyTemplateNode { InnerNode = innerNode, Name = "Dummies" };
			var collectionBizo = new CollectionCopyTemplateBizo(collectionNode, collectionNode, null);

			collectionBizo.IsSplitCollection = true;

			collectionBizo.CopyMethod = CollectionCopyTemplateBizo.CopyMethodCodes.DoNotCopy;
			((CollectionCopyTemplateBizoValidation)collectionBizo.Validation).ValidateCopyMethod();
			AssertNoErrors(collectionBizo.CopyMethodInfo);
			collectionBizo.Validation.ValidateAll();
			AssertNoErrors(collectionBizo.CopyMethodInfo);

			collectionBizo.CopyMethod = CollectionCopyTemplateBizo.CopyMethodCodes.Copy;
			((CollectionCopyTemplateBizoValidation)collectionBizo.Validation).ValidateCopyMethod();
			AssertNoErrors(collectionBizo.CopyMethodInfo);
			collectionBizo.Validation.ValidateAll();
			AssertHasError("Should have error about nothing selected to copy.", collectionBizo.CopyMethodInfo, "There is nothing selected on Dummies to copy.");

			collectionBizo.CopyMethod = CollectionCopyTemplateBizo.CopyMethodCodes.Filtered;
			((CollectionCopyTemplateBizoValidation)collectionBizo.Validation).ValidateCopyMethod();
			AssertNoErrors(collectionBizo.CopyMethodInfo);
			collectionBizo.Validation.ValidateAll();
			AssertHasError("Should have error about nothing selected to copy.", collectionBizo.CopyMethodInfo, "There is nothing selected on Dummies to copy.");

			propertyNode.CopyMethod = CopyMethod.Copy;
			((CollectionCopyTemplateBizoValidation)collectionBizo.Validation).ValidateCopyMethod();
			AssertNoErrors(collectionBizo.CopyMethodInfo);
			collectionBizo.Validation.ValidateAll();
			AssertNoErrors(collectionBizo.CopyMethodInfo);

			collectionBizo.CopyMethod = CollectionCopyTemplateBizo.CopyMethodCodes.Copy;
			((CollectionCopyTemplateBizoValidation)collectionBizo.Validation).ValidateCopyMethod();
			AssertNoErrors(collectionBizo.CopyMethodInfo);
			collectionBizo.Validation.ValidateAll();
			AssertNoErrors(collectionBizo.CopyMethodInfo);
		}

		public void TestValidationErrorIfNoFilterSetUp()
		{
			var propertyNode = new PropertyCopyTemplateNode { CopyMethod = CopyMethod.None, Name = "X" };

			var innerNode = new EntityCopyTemplateNode { Name = "Dummies" };
			innerNode.Nodes.Add(propertyNode);

			var collectionNode = new CollectionCopyTemplateNode { InnerNode = innerNode, Name = "Dummies" };
			var collectionBizo = new CollectionCopyTemplateBizo(collectionNode, collectionNode, null);

			collectionBizo.IsSplitCollection = true;

			collectionBizo.CopyMethod = CollectionCopyTemplateBizo.CopyMethodCodes.Filtered;
			collectionBizo.FilterStripBizo = new FilterBusinessObjectForTest();
			((CollectionCopyTemplateBizoValidation)collectionBizo.Validation).ValidateCopyMethod();
			AssertHasErrors(collectionBizo.CopyMethodInfo);
			collectionBizo.Validation.ValidateAll();
			AssertHasErrors(collectionBizo.CopyMethodInfo);
		}

		public void TestOrderErrorMessage()
		{
			var innerNode = new EntityCopyTemplateNode { Name = "Dummies" };
			var collectionNode = new CollectionCopyTemplateNode { InnerNode = innerNode, Name = "Dummies" };
			var parent = new CollectionCopyTemplateBizo(collectionNode, collectionNode, null);

			var collectionBizo1 = CreateNewCopyTemplateNodeBizo("Child1", "zzz", "www", parent);
			collectionBizo1.IsSplitCollection = true;
			collectionBizo1.Order = 1;
			collectionBizo1.CopyMethod = CollectionCopyTemplateBizo.CopyMethodCodes.Filtered;

			var collectionBizo2 = CreateNewCopyTemplateNodeBizo("Child2", "zzz", "www", parent);
			collectionBizo2.IsSplitCollection = true;
			collectionBizo2.Order = 1;
			collectionBizo2.CopyMethod = CollectionCopyTemplateBizo.CopyMethodCodes.Filtered;

			innerNode.Nodes.Add(collectionBizo1.CopyTemplateNode);
			innerNode.Nodes.Add(collectionBizo2.CopyTemplateNode);

			((CollectionCopyTemplateBizoValidation)collectionBizo2.Validation).ValidateOrder();
			AssertHasError("Should have an error", collectionBizo2.OrderInfo, "There are several collection split parts with same order number.");
		}

		#region Implementation

		class FilterBusinessObjectForTest : FilterBusinessObject
		{
			public FilterBusinessObjectForTest()
				: base(new BusinessObjectFactory(), new DataTable().NewRow())
			{ }

			public override ZQuery Filter => new ZQuery();

			protected sealed override void SetPKAndDefaults()
			{ }
		}

		protected override CollectionCopyTemplateBizo GetNewCopyTemplateNodeBizo()
		{
			return CreateNewCopyTemplateNodeBizo("Collection", DummyBizoSchema.Constants.Z0_Guid, DummyBizoSchema.Constants.TableName, null);
		}

		CollectionCopyTemplateBizo CreateNewCopyTemplateNodeBizo(string collectionName, string itemPropertyName, string itemTableName, EntityCopyTemplateBizo parent)
		{
			return new CollectionCopyTemplateBizo(CreateNewNode(collectionName, itemPropertyName, itemTableName), null, parent);
		}

		CollectionCopyTemplateNode CreateNewNode(string collectionName, string itemPropertyName, string itemTableName)
		{
			return
				new CollectionCopyTemplateNode
				{
					Id = Guid.NewGuid().ToString(),
					Name = collectionName,
					ItemPropertyName = itemPropertyName,
					ItemsTableName = itemTableName
				};
		}

		protected override void SetEntityCopyTemplateBizoToHaveData(CollectionCopyTemplateBizo entityCopyTemplateBizo)
		{
			entityCopyTemplateBizo.CopyMethod = "ALL";
		}

		#endregion
	}
}
