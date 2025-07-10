using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.Business.Testing
{
	[TestedType(typeof(RelatedEntityCopyTemplateBizo))]
	class RelatedEntityCopyTemplateBizoTest : EntityCopyTemplateBizoTest<RelatedEntityCopyTemplateBizo>
	{
		public void TestCopyMethod()
		{
			var relatedCopyBizo = GetNewCopyTemplateNodeBizo();

			relatedCopyBizo.CopyTemplateNode.CopyMethod = RelatedEntityCopyMethod.None;
			AssertEquals(ZString.Empty, relatedCopyBizo.CopyMethod);
			relatedCopyBizo.CopyTemplateNode.CopyMethod = RelatedEntityCopyMethod.Copy;
			AssertEquals("CPY", relatedCopyBizo.CopyMethod);
			relatedCopyBizo.CopyTemplateNode.CopyMethod = RelatedEntityCopyMethod.Link;
			AssertEquals("LNK", relatedCopyBizo.CopyMethod);
			relatedCopyBizo.CopyTemplateNode.CopyMethod = RelatedEntityCopyMethod.LinkCopied;
			AssertEquals("LCP", relatedCopyBizo.CopyMethod);

			relatedCopyBizo.CopyMethod = ZString.Empty;
			AssertEquals(RelatedEntityCopyMethod.None, relatedCopyBizo.CopyTemplateNode.CopyMethod);
			relatedCopyBizo.CopyMethod = "CPY";
			AssertEquals(RelatedEntityCopyMethod.Copy, relatedCopyBizo.CopyTemplateNode.CopyMethod);
			relatedCopyBizo.CopyMethod = "LNK";
			AssertEquals(RelatedEntityCopyMethod.Link, relatedCopyBizo.CopyTemplateNode.CopyMethod);
			relatedCopyBizo.CopyMethod = "LCP";
			AssertEquals(RelatedEntityCopyMethod.LinkCopied, relatedCopyBizo.CopyTemplateNode.CopyMethod);

			relatedCopyBizo.CopyMethod = "XYZ";
			AssertEquals(RelatedEntityCopyMethod.None, relatedCopyBizo.CopyTemplateNode.CopyMethod);
			AssertEquals("XYZ", relatedCopyBizo.CopyMethod);
		}

		public void TestCopyMethodRunsValidationOnChildrenWhenIsNotCopy()
		{
			var relatedCopyBizo = GetNewCopyTemplateNodeBizo();
			var propertyCopyBizo = PropertyConfigurationBizoTest.CreateNewCopyTemplateNodeBizo("Z0_Code", "String", relatedCopyBizo);
			propertyCopyBizo.CopyTemplateNode.IsMandatory = true;

			propertyCopyBizo.RunPreSaveValidation();
			AssertNoNotifications("Parent is not selected for copy - no validation on property", propertyCopyBizo.CopyMethodInfo);

			relatedCopyBizo.CopyMethod = "CPY";
			AssertNoNotifications("No automatic validation on property", propertyCopyBizo.CopyMethodInfo);

			propertyCopyBizo.RunPreSaveValidation();
			AssertHasErrors("Should have mandatory notification", propertyCopyBizo.CopyMethodInfo);

			relatedCopyBizo.CopyMethod = "LNK";
			AssertNoNotifications("Property should be re-validated", propertyCopyBizo.CopyMethodInfo);

			relatedCopyBizo.CopyMethod = "";
			AssertNoNotifications("Same - no notification", propertyCopyBizo.CopyMethodInfo);
		}

		public void TestEntityFilter()
		{
			var relatedCopyBizo = GetNewCopyTemplateNodeBizo();
			AssertNull(relatedCopyBizo.EntityFilter);

			var filter = new EntityFilter();
			AssertExceptionThrown(typeof(NotSupportedException), () => relatedCopyBizo.EntityFilter = filter);
		}

		public void TestValidation()
		{
			AssertEquals(typeof(RelatedEntityCopyTemplateBizoValidation), GetNewCopyTemplateNodeBizo().Validation.GetType());
		}

		public void TestValidateCopyMethod()
		{
			var relatedCopyBizo = GetNewCopyTemplateNodeBizo();
			((RelatedEntityCopyTemplateBizoValidation)relatedCopyBizo.Validation).ValidateCopyMethod();
			Assert(!relatedCopyBizo.CopyMethodInfo.HasNotifications());

			relatedCopyBizo.CopyMethod = "XYZ";
			((RelatedEntityCopyTemplateBizoValidation)relatedCopyBizo.Validation).ValidateCopyMethod();
			Assert("Should have error about wrong copy method code.", relatedCopyBizo.CopyMethodInfo.HasErrors());

			relatedCopyBizo.CopyMethod = "CPY";
			((RelatedEntityCopyTemplateBizoValidation)relatedCopyBizo.Validation).ValidateCopyMethod();
			Assert(!relatedCopyBizo.CopyMethodInfo.HasNotifications());

			relatedCopyBizo.CopyMethod = "LNK";
			((RelatedEntityCopyTemplateBizoValidation)relatedCopyBizo.Validation).ValidateCopyMethod();
			Assert(!relatedCopyBizo.CopyMethodInfo.HasNotifications());

			relatedCopyBizo.CopyMethod = "LCP";
			((RelatedEntityCopyTemplateBizoValidation)relatedCopyBizo.Validation).ValidateCopyMethod();
			Assert(!relatedCopyBizo.CopyMethodInfo.HasNotifications());
		}

		public void TestValidateCopyMethodWithRestriction()
		{
			var relatedCopyBizo = GetNewCopyTemplateNodeBizo();

			relatedCopyBizo.CopyMethod = "CPY";
			((RelatedEntityCopyTemplateBizoValidation)relatedCopyBizo.Validation).ValidateCopyMethod();
			AssertNoNotifications(relatedCopyBizo.CopyMethodInfo);
			relatedCopyBizo.CopyTemplateNode.DisableCopyMethodCopy = true;
			((RelatedEntityCopyTemplateBizoValidation)relatedCopyBizo.Validation).ValidateCopyMethod();
			AssertHasError(relatedCopyBizo.CopyMethodInfo, $"{relatedCopyBizo.Name} cannot be copied.");

			relatedCopyBizo.CopyMethod = "LNK";
			((RelatedEntityCopyTemplateBizoValidation)relatedCopyBizo.Validation).ValidateCopyMethod();
			AssertNoNotifications(relatedCopyBizo.CopyMethodInfo);
			relatedCopyBizo.CopyTemplateNode.DisableCopyMethodLink = true;
			((RelatedEntityCopyTemplateBizoValidation)relatedCopyBizo.Validation).ValidateCopyMethod();
			AssertHasError(relatedCopyBizo.CopyMethodInfo, $"{relatedCopyBizo.Name} cannot be linked.");

			relatedCopyBizo.CopyMethod = "LCP";
			((RelatedEntityCopyTemplateBizoValidation)relatedCopyBizo.Validation).ValidateCopyMethod();
			AssertHasError(relatedCopyBizo.CopyMethodInfo, $"{relatedCopyBizo.Name} cannot be linked.");
			relatedCopyBizo.CopyTemplateNode.AllowCopyMethodLinkCopiedWhenLinkIsDisabled = true;
			((RelatedEntityCopyTemplateBizoValidation)relatedCopyBizo.Validation).ValidateCopyMethod();
			AssertNoNotifications(relatedCopyBizo.CopyMethodInfo);
		}

		public void TestValidateCopyMethodForNonCopiedParent()
		{
			var parent = CreateNewCopyTemplateNodeBizo("Parent", "aaa", "bbb", null);
			var child = CreateNewCopyTemplateNodeBizo("Child", "xxx", "yyy", parent);
			((RelatedEntityCopyTemplateBizoValidation)child.Validation).ValidateCopyMethod();
			Assert(!child.CopyMethodInfo.HasNotifications());

			child.CopyMethod = "CPY";
			((RelatedEntityCopyTemplateBizoValidation)child.Validation).ValidateCopyMethod();
			AssertHasWarning(child.CopyMethodInfo, "To copy this Child please select a copy method on its parent Parent");

			parent.CopyMethod = "XYZ";
			((RelatedEntityCopyTemplateBizoValidation)child.Validation).ValidateCopyMethod();
			Assert(!child.CopyMethodInfo.HasNotifications());
		}

		public void TestValidateMandatoryRelatedEntity()
		{
			var parent = CreateNewCopyTemplateNodeBizo("Parent", "aaa", "bbb", null);
			var child = CreateNewCopyTemplateNodeBizo("Child", "xxx", "yyy", parent);
			((RelatedEntityCopyTemplateBizoValidation)child.Validation).ValidateCopyMethod();
			Assert(!child.CopyMethodInfo.HasNotifications());

			child.CopyTemplateNode.IsMandatory = true;
			child.CopyMethod = "";
			((RelatedEntityCopyTemplateBizoValidation)child.Validation).ValidateCopyMethod();
			AssertHasError(child.CopyMethodInfo, "Related entity Child is mandatory to be copied.");
		}

		public void TestValidationErrorIfNothingToCopy()
		{
			var propertyNode = new PropertyCopyTemplateNode { CopyMethod = CopyMethod.None, Name = "X" };

			var innerNode = new EntityCopyTemplateNode { Name = "Dummy" };
			innerNode.Nodes.Add(propertyNode);

			var relatedEntityNode = new RelatedEntityCopyTemplateNode { InnerNode = innerNode, Name = "Dummy" };
			var relatedBizo = new RelatedEntityCopyTemplateBizo(relatedEntityNode, relatedEntityNode, null);

			relatedBizo.CopyMethod = RelatedEntityCopyTemplateBizo.CopyMethodCodes.DoNotCopy;
			((RelatedEntityCopyTemplateBizoValidation)relatedBizo.Validation).ValidateCopyMethod();
			AssertNoErrors(relatedBizo.CopyMethodInfo);
			relatedBizo.Validation.ValidateAll();
			AssertNoErrors(relatedBizo.CopyMethodInfo);

			relatedBizo.CopyMethod = RelatedEntityCopyTemplateBizo.CopyMethodCodes.Link;
			((RelatedEntityCopyTemplateBizoValidation)relatedBizo.Validation).ValidateCopyMethod();
			AssertNoErrors(relatedBizo.CopyMethodInfo);
			relatedBizo.Validation.ValidateAll();
			AssertNoErrors(relatedBizo.CopyMethodInfo);

			relatedBizo.CopyMethod = RelatedEntityCopyTemplateBizo.CopyMethodCodes.LinkCopied;
			((RelatedEntityCopyTemplateBizoValidation)relatedBizo.Validation).ValidateCopyMethod();
			AssertNoErrors(relatedBizo.CopyMethodInfo);
			relatedBizo.Validation.ValidateAll();
			AssertNoErrors(relatedBizo.CopyMethodInfo);

			relatedBizo.CopyMethod = RelatedEntityCopyTemplateBizo.CopyMethodCodes.Copy;
			((RelatedEntityCopyTemplateBizoValidation)relatedBizo.Validation).ValidateCopyMethod();
			AssertNoErrors(relatedBizo.CopyMethodInfo);
			relatedBizo.Validation.ValidateAll();
			AssertHasError("Should have error about nothing selected to copy.", relatedBizo.CopyMethodInfo, "There is nothing selected on Dummy to copy.");

			propertyNode.CopyMethod = CopyMethod.Copy;
			((RelatedEntityCopyTemplateBizoValidation)relatedBizo.Validation).ValidateCopyMethod();
			AssertNoErrors(relatedBizo.CopyMethodInfo);
			relatedBizo.Validation.ValidateAll();
			AssertNoErrors(relatedBizo.CopyMethodInfo);
		}

		public void TestValidationErrorIfNothingToCopyOnSpecialNodes()
		{
			var relatedEntityNode = new RelatedEntityCopyTemplateNode { InnerNode = new EntityCopyTemplateNode { Name = "Dummy" }, Name = "Dummy" };
			var relatedBizo = new RelatedEntityCopyTemplateBizo(relatedEntityNode, relatedEntityNode, null);

			relatedBizo.CopyMethod = RelatedEntityCopyTemplateBizo.CopyMethodCodes.Copy;
			((RelatedEntityCopyTemplateBizoValidation)relatedBizo.Validation).ValidateCopyMethod();
			AssertHasError("Should have error about cannot copy.", relatedBizo.CopyMethodInfo, $"{relatedBizo.Name} cannot be copied.");
			relatedBizo.Validation.ValidateAll();
			AssertHasError("Should have error about cannot copy.", relatedBizo.CopyMethodInfo, $"{relatedBizo.Name} cannot be copied.");

			relatedBizo = new RelatedEntityCopyTemplateBizo(relatedEntityNode, relatedEntityNode, null);

			relatedBizo.CopyTemplateNode.CanCopyWithZeroNodes = true;
			relatedBizo.CopyMethod = RelatedEntityCopyTemplateBizo.CopyMethodCodes.Copy;
			((RelatedEntityCopyTemplateBizoValidation)relatedBizo.Validation).ValidateCopyMethod();
			AssertNoErrors(relatedBizo.CopyMethodInfo);
			relatedBizo.Validation.ValidateAll();
			AssertNoErrors(relatedBizo.CopyMethodInfo);
		}

		#region Implementation

		protected override RelatedEntityCopyTemplateBizo GetNewCopyTemplateNodeBizo()
		{
			return CreateNewCopyTemplateNodeBizo("Parent", DummyBizoSchema.Constants.Z0_Guid, DummyBizoSchema.Constants.TableName, null);
		}

		internal static RelatedEntityCopyTemplateBizo CreateNewCopyTemplateNodeBizo(string name, string relatedPropertyName, string relatedTableName, EntityCopyTemplateBizo parent)
		{
			return new RelatedEntityCopyTemplateBizo(CreateNewNode(name, relatedPropertyName, relatedTableName), null, parent);
		}

		static RelatedEntityCopyTemplateNode CreateNewNode(string name, string relatedPropertyName, string relatedTableName)
		{
			return
				new RelatedEntityCopyTemplateNode
				{
					Id = Guid.NewGuid().ToString(),
					Name = name,
					RelatedPropertyName = relatedPropertyName,
					RelatedEntityTableName = relatedTableName
				};
		}

		protected override void SetEntityCopyTemplateBizoToHaveData(RelatedEntityCopyTemplateBizo entityCopyTemplateBizo)
		{
			entityCopyTemplateBizo.CopyMethod = "CPY";
		}

		#endregion
	}
}
