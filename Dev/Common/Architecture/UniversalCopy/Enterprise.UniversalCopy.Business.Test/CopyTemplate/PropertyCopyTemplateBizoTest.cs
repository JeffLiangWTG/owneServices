using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.Business.Testing
{
	[TestedType(typeof(PropertyCopyTemplateBizo))]
	class PropertyConfigurationBizoTest : ConfigurationNodeBizoTest<PropertyCopyTemplateBizo>
	{
		public override void TestHumanReadableName()
		{
			var propertyCopyBizo = GetNewCopyTemplateNodeBizo();

			AssertEquals(propertyCopyBizo.ParentEntity.Name, propertyCopyBizo.HumanReadableName);
		}

		public void TestNotification()
		{
			var propertyCopyBizo = new PropertyCopyTemplateBizo(new PropertyCopyTemplateNode
			{
				Id = Guid.NewGuid().ToString(),
				Name = "Test",
				PropertyType = "String"
			}, null);

			propertyCopyBizo.CopyTemplateNode.IsMandatory = true;

			propertyCopyBizo.RunPreSaveValidation();

			Assert(propertyCopyBizo.CopyMethodInfo.HasError($"{propertyCopyBizo.Name} is mandatory and should be initialized in some way (copied or set manually)."));
		}

		public void TestCopyMethod()
		{
			var propertyCopyBizo = GetNewCopyTemplateNodeBizo();

			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.None;
			AssertEquals(ZString.Empty, propertyCopyBizo.CopyMethod);
			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Empty;
			AssertEquals("EMP", propertyCopyBizo.CopyMethod);
			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Copy;
			AssertEquals("CPY", propertyCopyBizo.CopyMethod);
			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Value;
			AssertEquals("VAL", propertyCopyBizo.CopyMethod);
			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Default;
			AssertEquals("DEF", propertyCopyBizo.CopyMethod);
			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Property;
			AssertEquals("PRO", propertyCopyBizo.CopyMethod);
			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Macro;
			AssertEquals("MAC", propertyCopyBizo.CopyMethod);

			propertyCopyBizo.CopyMethod = ZString.Empty;
			AssertEquals(CopyMethod.None, propertyCopyBizo.CopyTemplateNode.CopyMethod);
			propertyCopyBizo.CopyMethod = "EMP";
			AssertEquals(CopyMethod.Empty, propertyCopyBizo.CopyTemplateNode.CopyMethod);
			propertyCopyBizo.CopyMethod = "CPY";
			AssertEquals(CopyMethod.Copy, propertyCopyBizo.CopyTemplateNode.CopyMethod);
			propertyCopyBizo.CopyMethod = "VAL";
			AssertEquals(CopyMethod.Value, propertyCopyBizo.CopyTemplateNode.CopyMethod);
			propertyCopyBizo.CopyMethod = "DEF";
			AssertEquals(CopyMethod.Default, propertyCopyBizo.CopyTemplateNode.CopyMethod);
			propertyCopyBizo.CopyMethod = "PRO";
			AssertEquals(CopyMethod.Property, propertyCopyBizo.CopyTemplateNode.CopyMethod);
			propertyCopyBizo.CopyMethod = "MAC";
			AssertEquals(CopyMethod.Macro, propertyCopyBizo.CopyTemplateNode.CopyMethod);

			propertyCopyBizo.CopyMethod = "XYZ";
			AssertEquals(CopyMethod.None, propertyCopyBizo.CopyTemplateNode.CopyMethod);
			AssertEquals("XYZ", propertyCopyBizo.CopyMethod);
		}

		public void TestValue()
		{
			var propertyCopyBizo = CreateNewCopyTemplateNodeBizo("Property1", "Int32");
			propertyCopyBizo.CopyMethod = "VAL";
			propertyCopyBizo.Value = "123";
			AssertEquals("123", propertyCopyBizo.Value);
			AssertEquals(123, propertyCopyBizo.CopyTemplateNode.Value);

			propertyCopyBizo.Value = "abcd";
			AssertEquals("abcd", propertyCopyBizo.Value);
			AssertEquals("abcd", propertyCopyBizo.WrongValue);
			AssertEquals(123, propertyCopyBizo.CopyTemplateNode.Value);

			propertyCopyBizo.CopyMethod = "MAC";
			propertyCopyBizo.Value = "abcd";
			AssertEquals("abcd", propertyCopyBizo.Value);
			AssertEquals(ZString.Empty, propertyCopyBizo.WrongValue);
			AssertEquals("abcd", propertyCopyBizo.CopyTemplateNode.Value);
		}

		public void TestRealDataType()
		{
			var propertyCopyBizo = CreateNewCopyTemplateNodeBizo("Property1", null);
			propertyCopyBizo.CopyMethod = "VAL";
			propertyCopyBizo.Value = "123";
			AssertEquals(typeof(object), propertyCopyBizo.RealDataType);
		}

		public void TestValueType()
		{
			var propertyCopyBizo = CreateNewCopyTemplateNodeBizo("Property1", "String");
			AssertEquals("String", propertyCopyBizo.ValueType);

			propertyCopyBizo.CopyTemplateNode.PropertyType = "Int32";
			AssertEquals("Int32", propertyCopyBizo.ValueType);
		}

		public void TestValueColumnType()
		{
			var propertyCopyBizo = CreateNewCopyTemplateNodeBizo("Property1", "String");
			AssertEquals(nameof(FieldType.Text), propertyCopyBizo.ValueColumnType);

			propertyCopyBizo.CopyTemplateNode.PropertyType = "Int32";
			AssertEquals(nameof(FieldType.Integer), propertyCopyBizo.ValueColumnType);

			propertyCopyBizo.CopyTemplateNode.PropertyType = "Decimal";
			AssertEquals(nameof(FieldType.Decimal), propertyCopyBizo.ValueColumnType);

			propertyCopyBizo.CopyTemplateNode.PropertyType = "Boolean";
			AssertEquals(nameof(FieldType.Boolean), propertyCopyBizo.ValueColumnType);

			propertyCopyBizo.CopyTemplateNode.PropertyType = "DateTime";
			AssertEquals(nameof(FieldType.DateTime), propertyCopyBizo.ValueColumnType);

			propertyCopyBizo.CopyTemplateNode.PropertyType = "DateTimeOffset";
			AssertEquals(nameof(FieldType.DateTimeOffset), propertyCopyBizo.ValueColumnType);

			propertyCopyBizo.CopyTemplateNode.PropertyType = "Time";
			AssertEquals(nameof(FieldType.Time), propertyCopyBizo.ValueColumnType);

			propertyCopyBizo.CopyTemplateNode.PropertyType = "Geography";
			AssertEquals(nameof(FieldType.Geography), propertyCopyBizo.ValueColumnType);

			propertyCopyBizo.CopyTemplateNode.PropertyType = "Guid";
			AssertEquals(nameof(FieldType.Guid), propertyCopyBizo.ValueColumnType);

			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Property;
			AssertEquals(nameof(FieldType.TextDropEdit), propertyCopyBizo.ValueColumnType);

			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Macro;
			AssertEquals(nameof(FieldType.TextMacro), propertyCopyBizo.ValueColumnType);
		}

		public void TestValue_ReadOnly()
		{
			var propertyCopyBizo = GetNewCopyTemplateNodeBizo();
			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.None;
			Assert(propertyCopyBizo.ValueInfo.ReadOnly);

			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Empty;
			Assert(propertyCopyBizo.ValueInfo.ReadOnly);

			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Copy;
			Assert(propertyCopyBizo.ValueInfo.ReadOnly);

			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Value;
			Assert(!propertyCopyBizo.ValueInfo.ReadOnly);

			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Default;
			Assert(propertyCopyBizo.ValueInfo.ReadOnly);

			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Property;
			Assert(!propertyCopyBizo.ValueInfo.ReadOnly);

			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Macro;
			Assert(!propertyCopyBizo.ValueInfo.ReadOnly);

			var otherPropertyCopyBizo = GetNewCopyTemplateNodeBizo();
			otherPropertyCopyBizo.CopyTemplateNode.PropertyType = "jiberish";
			otherPropertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Value;
			Assert("Cannot edit jiberish values", otherPropertyCopyBizo.IsUneditableType);
			Assert("Cannot edit jiberish values", otherPropertyCopyBizo.ValueInfo.ReadOnly);
		}

		public void TestValuesList()
		{
			var propertyCopyBizo = GetNewCopyTemplateNodeBizo();
			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.None;
			AssertNull(propertyCopyBizo.ValuesList);

			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Property;
			AssertEquals(1, propertyCopyBizo.ValuesList.Count);

			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Value;
			AssertNull(propertyCopyBizo.ValuesList);

			var propertiesList = new CodeDescriptionPairList();
			propertiesList.Add(new CodeDescriptionPair("Description 1", "Name 1"));
			propertiesList.Add(new CodeDescriptionPair("Description 2", "Name 2"));
			propertyCopyBizo.ValuesList = propertiesList;

			propertyCopyBizo.CopyTemplateNode.CopyMethod = CopyMethod.Value;
			AssertEquals(2, propertyCopyBizo.ValuesList.Count);
		}

		public void TestCaption()
		{
			var propertyCopyBizo = CreateNewCopyTemplateNodeBizo(JobShipmentSchema.Constants.JS_HouseBill, null);

			AssertNotEquals(JobShipmentSchema.Constants.JS_HouseBill, propertyCopyBizo.Description);
			AssertEquals(DataBoundResourceStrings.GetDataForProperty(null, JobShipmentSchema.Constants.JS_HouseBill).Caption, propertyCopyBizo.Description);
		}

		public void TestIsMandatory()
		{
			var propertyCopyBizo = GetNewCopyTemplateNodeBizo();

			propertyCopyBizo.CopyTemplateNode.IsMandatory = true;
			Assert(propertyCopyBizo.IsMandatory);

			propertyCopyBizo.CopyTemplateNode.IsMandatory = false;
			Assert(!propertyCopyBizo.IsMandatory);
		}

		public void TestValidation()
		{
			AssertEquals(typeof(PropertyCopyTemplateBizoValidation), GetNewCopyTemplateNodeBizo().Validation.GetType());
		}

		public void TestValidateCopyMethod()
		{
			var propertyCopyBizo = GetNewCopyTemplateNodeBizo();
			((PropertyCopyTemplateBizoValidation)propertyCopyBizo.Validation).ValidateCopyMethod();
			Assert(!propertyCopyBizo.CopyMethodInfo.HasNotifications());

			propertyCopyBizo.CopyMethod = "XYZ";
			((PropertyCopyTemplateBizoValidation)propertyCopyBizo.Validation).ValidateCopyMethod();
			Assert("Should have error about wrong copy method code.", propertyCopyBizo.CopyMethodInfo.HasErrors());

			propertyCopyBizo.CopyMethod = "CPY";
			((PropertyCopyTemplateBizoValidation)propertyCopyBizo.Validation).ValidateCopyMethod();
			Assert(!propertyCopyBizo.CopyMethodInfo.HasNotifications());

			propertyCopyBizo.CopyMethod = "VAL";
			((PropertyCopyTemplateBizoValidation)propertyCopyBizo.Validation).ValidateCopyMethod();
			Assert(!propertyCopyBizo.CopyMethodInfo.HasNotifications());

			var otherPropertyCopyBizo = GetNewCopyTemplateNodeBizo();
			otherPropertyCopyBizo.CopyMethod = "VAL";
			otherPropertyCopyBizo.CopyTemplateNode.PropertyType = "xyz";
			((PropertyCopyTemplateBizoValidation)otherPropertyCopyBizo.Validation).ValidateCopyMethod();
			AssertHasError(otherPropertyCopyBizo.CopyMethodInfo, $"{otherPropertyCopyBizo.Name} cannot be edited manually.");
		}

		public void TestValidateCopyMethodForMandatoryProperty()
		{
			var propertyCopyBizo = GetNewCopyTemplateNodeBizo();
			Assert("Precondition", !propertyCopyBizo.IsMandatory);

			var parentEntity = propertyCopyBizo.ParentEntity as RelatedEntityCopyTemplateBizo;
			AssertNotNull("Precondition", parentEntity);
			Assert("Precondition", !parentEntity.CopyTemplateNode.HasData());

			((PropertyCopyTemplateBizoValidation)propertyCopyBizo.Validation).ValidateCopyMethod();
			AssertNoNotifications(propertyCopyBizo.CopyMethodInfo);

			propertyCopyBizo.CopyTemplateNode.IsMandatory = true;
			((PropertyCopyTemplateBizoValidation)propertyCopyBizo.Validation).ValidateCopyMethod();
			AssertNoNotifications("Parent entity has no data - validation on property is not required", propertyCopyBizo.CopyMethodInfo);

			parentEntity.CopyMethod = "CPY";
			((PropertyCopyTemplateBizoValidation)propertyCopyBizo.Validation).ValidateCopyMethod();
			AssertHasErrors("Should have mandatory validation", propertyCopyBizo.CopyMethodInfo);

			propertyCopyBizo.CopyMethod = "CPY";
			((PropertyCopyTemplateBizoValidation)propertyCopyBizo.Validation).ValidateCopyMethod();
			AssertNoNotifications(propertyCopyBizo.CopyMethodInfo);
		}

		public void TestValidateValue()
		{
			var propertyCopyBizo = GetNewCopyTemplateNodeBizo();
			((PropertyCopyTemplateBizoValidation)propertyCopyBizo.Validation).ValidateValue();
			Assert(!propertyCopyBizo.ValueInfo.HasNotifications());

			propertyCopyBizo.CopyMethod = "PRO";
			((PropertyCopyTemplateBizoValidation)propertyCopyBizo.Validation).ValidateValue();
			Assert("Should have error about empty value.", propertyCopyBizo.ValueInfo.HasErrors());

			propertyCopyBizo.Value = "xyz";
			((PropertyCopyTemplateBizoValidation)propertyCopyBizo.Validation).ValidateValue();
			Assert("Should have error about wrong value (other property).", propertyCopyBizo.ValueInfo.HasErrors());

			propertyCopyBizo.Value = propertyCopyBizo.Name;
			((PropertyCopyTemplateBizoValidation)propertyCopyBizo.Validation).ValidateValue();
			Assert(!propertyCopyBizo.ValueInfo.HasNotifications());

			propertyCopyBizo = CreateNewCopyTemplateNodeBizo("Z0_Number", "Int32");
			propertyCopyBizo.CopyMethod = "VAL";
			propertyCopyBizo.Value = "abc";
			((PropertyCopyTemplateBizoValidation)propertyCopyBizo.Validation).ValidateValue();
			Assert("Should have error about wrong value (not integer).", propertyCopyBizo.ValueInfo.HasErrors());

			propertyCopyBizo.Value = "123";
			((PropertyCopyTemplateBizoValidation)propertyCopyBizo.Validation).ValidateValue();
			Assert(!propertyCopyBizo.ValueInfo.HasNotifications());
		}

		public void TestGuidCopyMethods()
		{
			var propertyCopyBizo = CreateNewCopyTemplateNodeBizo("Z0_Guid", "Guid");
			var copyMethods = propertyCopyBizo.CopyMethods;

			Assert(copyMethods.ContainsCode(PropertyCopyTemplateBizo.CopyMethodCodes.DoNotCopy));
			Assert(copyMethods.ContainsCode(PropertyCopyTemplateBizo.CopyMethodCodes.Copy));
			Assert(copyMethods.ContainsCode(PropertyCopyTemplateBizo.CopyMethodCodes.Empty));
			Assert(copyMethods.ContainsCode(PropertyCopyTemplateBizo.CopyMethodCodes.Default));
			Assert(copyMethods.ContainsCode(PropertyCopyTemplateBizo.CopyMethodCodes.Property));
			Assert(copyMethods.ContainsCode(PropertyCopyTemplateBizo.CopyMethodCodes.Value));
			Assert(!copyMethods.ContainsCode(PropertyCopyTemplateBizo.CopyMethodCodes.Macros));

			propertyCopyBizo = CreateNewCopyTemplateNodeBizo("Z0_Guid", "Guid");
			propertyCopyBizo.CopyTemplateNode.CustomCopyTemplateNode = "CustomCopyTemplateNode";
			copyMethods = propertyCopyBizo.CopyMethods;

			Assert(copyMethods.ContainsCode(PropertyCopyTemplateBizo.CopyMethodCodes.DoNotCopy));
			Assert(copyMethods.ContainsCode(PropertyCopyTemplateBizo.CopyMethodCodes.Copy));
			Assert(!copyMethods.ContainsCode(PropertyCopyTemplateBizo.CopyMethodCodes.Empty));
			Assert(!copyMethods.ContainsCode(PropertyCopyTemplateBizo.CopyMethodCodes.Default));
			Assert(!copyMethods.ContainsCode(PropertyCopyTemplateBizo.CopyMethodCodes.Property));
			Assert(!copyMethods.ContainsCode(PropertyCopyTemplateBizo.CopyMethodCodes.Value));
			Assert(!copyMethods.ContainsCode(PropertyCopyTemplateBizo.CopyMethodCodes.Macros));
		}

		public void TestValueGuid()
		{
			var propertyCopyBizo = CreateNewCopyTemplateNodeBizo("Z0_Guid", "Guid");
			propertyCopyBizo.CopyMethod = "VAL";
			AssertEquals("Guid", propertyCopyBizo.ValueType);

			((PropertyCopyTemplateBizoValidation)propertyCopyBizo.Validation).ValidateValue();
			Assert("Should have a module", propertyCopyBizo.ValueInfo.HasErrors());

			propertyCopyBizo.ModuleId = ModuleIDs.Organisation;

			((PropertyCopyTemplateBizoValidation)propertyCopyBizo.Validation).ValidateValue();
			Assert(!propertyCopyBizo.ValueInfo.HasErrors());
		}

		#region Implementation

		protected override PropertyCopyTemplateBizo GetNewCopyTemplateNodeBizo()
		{
			return CreateNewCopyTemplateNodeBizo("Z0_Code", "String");
		}

		static PropertyCopyTemplateBizo CreateNewCopyTemplateNodeBizo(string name, string type)
		{
			var relatedEntityCopyTemplateBizo = RelatedEntityCopyTemplateBizoTest.CreateNewCopyTemplateNodeBizo(
				"Dummy", DummyBizoSchema.Constants.Z0_Guid, DummyBizoSchema.Constants.TableName, null);

			return CreateNewCopyTemplateNodeBizo(name, type, relatedEntityCopyTemplateBizo);
		}

		internal static PropertyCopyTemplateBizo CreateNewCopyTemplateNodeBizo(string name, string type, EntityCopyTemplateBizo parent)
		{
			var propertyCopyTemplateBizo = new PropertyCopyTemplateBizo(CreateNewNode(name, type), parent);
			parent.PropertyNodes.Add(propertyCopyTemplateBizo);

			return propertyCopyTemplateBizo;
		}

		static PropertyCopyTemplateNode CreateNewNode(string name, string type)
		{
			return
				new PropertyCopyTemplateNode
				{
					Id = Guid.NewGuid().ToString(),
					Name = name,
					PropertyType = type
				};
		}

		#endregion
	}
}
