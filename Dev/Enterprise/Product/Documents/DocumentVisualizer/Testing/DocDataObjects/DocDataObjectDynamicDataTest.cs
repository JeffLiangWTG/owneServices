using System.ComponentModel;
using System.Data;
using System.Xml.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.DocDataObjects.Testing
{
	sealed class DocDataObjectDynamicDataTest : TestCaseWithFactory
	{
		#region TestGetPropertyValue

		public void TestGetPropertyValue()
		{
			const string macro = "\"<RelatedDummy.Z0_Description>\"";

			var related = Factory.New<DummyBusinessObject>();
			related.Z0_Description = "Alibaba";

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Guid = related.PK;

			var dynamicData = dummy.MakeDynamic(dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var expr = macro.CreateExpression();
			var result = expr.Evaluate(dynamicData);

			AssertMultilineASCIIEquals("expected no errors",
				"",
				expr.ToFormatString());

			AssertEquals("macro result", "Alibaba", result);
		}

		public void TestGettngNoExistingPropertyDoesNotThrowAnException()
		{
			var related = Factory.New<DummyBusinessObject>();

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Guid = related.PK;

			var dynamicData = dummy.MakeDynamic(dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			AssertNoExceptionThrown(() => dynamicData.GetDynamicProperty("Z0_NoSuchProperty"));
		}

		#endregion

		#region TestSetPropertyValueThenReset()

		public void TestSetPropertyValueThenReset()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = ZString.Empty;

			var dynamicData = dummy.MakeDynamic(dynamicDataFactory: new DocDataObjectDynamicDataFactory());
			var desc = dynamicData.GetDynamicProperty(nameof(dummy.Z0_Description));

			AssertEquals("DynamicData Value (before setting new value)", ZString.Empty, desc.Value);
			AssertEquals("DynamicData HasChanges (before setting new value)", false, desc.HasChanges);
			AssertEquals("DynamicData IsOverridden (before setting new value)", false, desc.IsOverridden);
			AssertEquals("Value on the underlying data source (before setting new value)", ZString.Empty, dummy.Z0_Description);

			desc.SetValue("Alibaba");

			AssertEquals("DynamicData Value (after setting value)", "Alibaba", desc.Value);
			AssertEquals("DynamicData HasChanges (after setting value)", true, desc.HasChanges);
			AssertEquals("DynamicData IsOverridden (after setting value)", true, desc.IsOverridden);
			AssertEquals("value has been proxied to the underlying data source (after setting value)", "Alibaba", dummy.Z0_Description);

			desc.CancelChanges();

			AssertEquals("DynamicData Value (after reset)", ZString.Empty, desc.Value);
			AssertEquals("DynamicData HasChanges (after reset)", true, desc.HasChanges);
			AssertEquals("DynamicData IsOverridden (after reset)", false, desc.IsOverridden);
			AssertEquals("Value on the underlying data source (after reset)", ZString.Empty, dummy.Z0_Description);
		}

		#endregion

		#region TestSetPropertyValueThenReset_PropertyWithNoSetter

		public void TestSetPropertyValueThenReset_PropertyWithNoSetter()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			AssertEquals("prerequisite: Z0_Calculated value", 5, dummy.Z0_Calculated);

			var dynamicData = dummy.MakeDynamic(dynamicDataFactory: new DocDataObjectDynamicDataFactory());
			var calculated = dynamicData.GetDynamicProperty(nameof(dummy.Z0_Calculated));

			AssertEquals("DynamicData Value (before setting new value)", 5, calculated.Value);
			AssertEquals("DynamicData HasChanges (before setting new value)", false, calculated.HasChanges);
			AssertEquals("DynamicData IsOverridden (before setting new value)", false, calculated.IsOverridden);

			calculated.SetValue(9);

			AssertEquals("DynamicData Value (after setting value)", 5, calculated.Value);
			AssertEquals("DynamicData HasChanges (after setting value)", true, calculated.HasChanges);
			AssertEquals("DynamicData IsOverridden (after setting value)", false, calculated.IsOverridden);

			calculated.CancelChanges();

			AssertEquals("DynamicData Value (after reset)", 5, calculated.Value);
			AssertEquals("DynamicData HasChanges (after reset)", true, calculated.HasChanges);
			AssertEquals("DynamicData IsOverridden (after reset)", false, calculated.IsOverridden);
		}

		#endregion

		#region TestResetOverride

		public void TestResetOverride()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = ZString.Empty;

			AssertEquals("Z0_Description", ZString.Empty, dummy.Z0_Description);

			var dynamicData = dummy.MakeDynamic(dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var desc = dynamicData.GetDynamicProperty(nameof(dummy.Z0_Description));

			desc.SetValue("Alibaba");

			AssertEquals("prerequisite: DynamicData Value", "Alibaba", desc.Value);
			AssertEquals("prerequisite: DynamicData HasChanges", true, desc.HasChanges);
			AssertEquals("prerequisite: DynamicData IsOverridden", true, desc.IsOverridden);
			AssertEquals("prerequisite: value has been proxied to the underlying data source", "Alibaba", dummy.Z0_Description);

			dynamicData.CancelChanges();

			AssertEquals("DynamicData Value", ZString.Empty, desc.Value);
			AssertEquals("DynamicData HasChanges", true, desc.HasChanges);
			AssertEquals("DynamicData IsOverridden", false, desc.IsOverridden);
			AssertEquals("value has been proxied to the underlying data source", ZString.Empty, dummy.Z0_Description);
		}

		#endregion

		#region TestMergeFromOverride

		public void TestMergeFromOverride_BusinessObject()
		{
			const string macro = "\"<Z0_Description>\"";

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = ZString.Empty;

			var dynamicData = dummy.MakeDynamic(dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var xml = XDocument.Parse(
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""Z0_Description"">
    <Value>Alibaba</Value>
  </Property>
</Entity>");

			dynamicData.MergeDataFromXml(xml);

			var expr = macro.CreateExpression();
			expr.Evaluate(dynamicData);

			AssertMultilineASCIIEquals("expected no errors",
				string.Empty,
				expr.ToFormatString());

			var desc = dynamicData.GetDynamicProperty(nameof(dummy.Z0_Description));

			AssertEquals("DynamicData Value", "Alibaba", desc.Value);
			AssertEquals("DynamicData HasChanges", false, desc.HasChanges);
			AssertEquals("DynamicData IsOverridden", true, desc.IsOverridden);
			AssertEquals("value was pushed to DummyBusinessObject.Z0_Description", "Alibaba", dummy.Z0_Description);
		}

		public void TestMergeFromOverride_DocDataObject()
		{
			const string macro = "\"<Text>\"";

			var dummy = new DummyDocDataObject("my-id-id");
			dummy.Text = ZString.Empty;

			var dynamicData = dummy.MakeDocDataDynamic();

			var xml = XDocument.Parse(
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""Text"">
    <Value>Alibaba</Value>
  </Property>
</Entity>");

			dynamicData.MergeDataFromXml(xml);

			var expr = macro.CreateExpression();
			expr.Evaluate(dynamicData);

			AssertMultilineASCIIEquals("expected no errors",
				string.Empty,
				expr.ToFormatString());

			var desc = dynamicData.GetDynamicProperty(nameof(dummy.Text));

			AssertEquals("DynamicData Value", "Alibaba", desc.Value);
			AssertEquals("DynamicData HasChanges", false, desc.HasChanges);
			AssertEquals("DynamicData IsOverridden", true, desc.IsOverridden);
			AssertEquals("value was pushed to the DummyDocDataObject.Text", "Alibaba", dummy.Text);
		}

		public void TestMergeFromOverride_BusinessObject_OverrideMatchedCurrentValue()
		{
			const string macro = "\"<Z0_Description>\"";

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "Alibaba";

			var dynamicData = dummy.MakeDynamic(dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var xml = XDocument.Parse(
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""Z0_Description"">
    <Value>Alibaba</Value>
  </Property>
</Entity>");

			dynamicData.MergeDataFromXml(xml);

			var expr = macro.CreateExpression();
			expr.Evaluate(dynamicData);

			AssertMultilineASCIIEquals("expected no errors",
				string.Empty,
				expr.ToFormatString());

			var desc = dynamicData.GetDynamicProperty(nameof(dummy.Z0_Description));

			AssertEquals("DynamicData Value", "Alibaba", desc.Value);
			AssertEquals("DynamicData HasChanges", false, desc.HasChanges);
			AssertEquals("DynamicData IsOverridden", false, desc.IsOverridden);
			AssertEquals("value was pushed to DummyBusinessObject.Z0_Description", "Alibaba", dummy.Z0_Description);
		}

		public void TestMergeFromOverride_DocDataObject_OverrideMatchedCurrentValue()
		{
			const string macro = "\"<Text>\"";

			var dummy = new DummyDocDataObject("my-id");
			dummy.Text = "Alibaba";

			var dynamicData = dummy.MakeDocDataDynamic();

			var xml = XDocument.Parse(
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""Text"">
    <Value>Alibaba</Value>
  </Property>
</Entity>");

			dynamicData.MergeDataFromXml(xml);

			var expr = macro.CreateExpression();
			expr.Evaluate(dynamicData);

			AssertMultilineASCIIEquals("expected no errors",
				string.Empty,
				expr.ToFormatString());

			var desc = dynamicData.GetDynamicProperty(nameof(dummy.Text));

			AssertEquals("DynamicData Value", "Alibaba", desc.Value);
			AssertEquals("DynamicData HasChanges", false, desc.HasChanges);
			AssertEquals("DynamicData IsOverridden", false, desc.IsOverridden);
			AssertEquals("value was pushed to DocDataObject.Text", "Alibaba", dummy.Text);
		}

		public void TestMergeFromOverride_DocDataObject_OnePropertyUpdatesAnother()
		{
			const string macro = "\"<Code> - <CodeDescription>\"";

			DummyDocDataObject CreateDummy()
			{
				var res = new DummyDocDataObject("my-id");
				res.CodeInfo.ValueChanged += (s, e) =>
				{
					res.CodeDescription = $"{res.Code} description";
				};
				return res;
			}

			var dummy = CreateDummy();
			var dynamicData = dummy.MakeDocDataDynamic();
			var code = dynamicData.GetDynamicProperty(nameof(dummy.Code));
			var codeDescription = dynamicData.GetDynamicProperty(nameof(dummy.CodeDescription));

			code.SetValue("AAA");

			AssertEquals("DynamicData (Code) Value", "AAA", code.Value);
			AssertEquals("DynamicData (CodeDescription) Value", "AAA description", codeDescription.Value);

			var xml = dynamicData.GetOverriddenValuesXml();

			AssertMultilineASCIIEquals("override xml",
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Id>my-id</Id>
  <Property Name=""Code"">
    <Value>AAA</Value>
  </Property>
  <Property Name=""CodeDescription"">
    <Value>AAA description</Value>
  </Property>
</Entity>",
				xml.ToXmlString());

			dummy = CreateDummy();
			dynamicData = dummy.MakeDocDataDynamic();

			dynamicData.MergeDataFromXml(xml);

			var expr = macro.CreateExpression();
			expr.Evaluate(dynamicData);

			AssertMultilineASCIIEquals("expected no errors",
				string.Empty,
				expr.ToFormatString());

			code = dynamicData.GetDynamicProperty(nameof(dummy.Code));
			codeDescription = dynamicData.GetDynamicProperty(nameof(dummy.CodeDescription));

			AssertEquals("Code DynamicData Value", "AAA", code.Value);
			AssertEquals("Code DynamicData HasChanges", false, code.HasChanges);
			AssertEquals("Code DynamicData IsOverridden", true, code.IsOverridden);
			AssertEquals("Code value has been proxied to the underlying data source", "AAA", dummy.Code);

			AssertEquals("CodeDescription DynamicData Value", "AAA description", codeDescription.Value);
			AssertEquals("CodeDescription DynamicData HasChanges", false, codeDescription.HasChanges);
			AssertEquals("CodeDescription DynamicData IsOverridden", true, codeDescription.IsOverridden);
			AssertEquals("CodeDescription value has been proxied to the underlying data source", "AAA description", dummy.CodeDescription);

			dynamicData.CancelChanges();

			AssertEquals("Code DynamicData Value has been reset to original", "", code.Value);
			AssertEquals("CodeDescription DynamicData Value has been reset to original", "", codeDescription.Value);

			AssertEquals("Code value has been proxied to the underlying data source", "", dummy.Code);
			AssertEquals("CodeDescription value has been proxied to the underlying data source", "", dummy.CodeDescription);
		}

		public void TestMergeFromOverride_DocDataObject_OnePropertyUpdatesAnother_WithUserChanges()
		{
			const string macro = "\"<Code> - <CodeDescription>\"";

			DummyDocDataObject CreateDummy()
			{
				var res = new DummyDocDataObject("my-id");
				res.CodeInfo.ValueChanged += (s, e) =>
				{
					res.CodeDescription = $"{res.Code} description";
				};

				return res;
			}

			var dummy = CreateDummy();
			var dynamicData = dummy.MakeDocDataDynamic();
			var code = dynamicData.GetDynamicProperty(nameof(dummy.Code));
			var codeDescription = dynamicData.GetDynamicProperty(nameof(dummy.CodeDescription));

			code.SetValue("AAA");
			codeDescription.SetValue("updated by user");

			AssertEquals("DynamicData (Code) Value", "AAA", code.Value);
			AssertEquals("DynamicData (CodeDescription) Value", "updated by user", codeDescription.Value);

			var xml = dynamicData.GetOverriddenValuesXml();

			AssertMultilineASCIIEquals("override xml",
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Id>my-id</Id>
  <Property Name=""Code"">
    <Value>AAA</Value>
  </Property>
  <Property Name=""CodeDescription"">
    <Value>updated by user</Value>
  </Property>
</Entity>",
				xml.ToXmlString());

			dummy = CreateDummy();
			dynamicData = dummy.MakeDocDataDynamic();

			dynamicData.MergeDataFromXml(xml);

			var expr = macro.CreateExpression();
			expr.Evaluate(dynamicData);

			AssertMultilineASCIIEquals("expected no errors",
				string.Empty,
				expr.ToFormatString());

			code = dynamicData.GetDynamicProperty(nameof(dummy.Code));
			codeDescription = dynamicData.GetDynamicProperty(nameof(dummy.CodeDescription));

			AssertEquals("Code DynamicData Value", "AAA", code.Value);
			AssertEquals("Code DynamicData HasChanges", false, code.HasChanges);
			AssertEquals("Code DynamicData IsOverridden", true, code.IsOverridden);
			AssertEquals("Code value has been proxied to the underlying data source", "AAA", dummy.Code);

			AssertEquals("CodeDescription DynamicData Value", "updated by user", codeDescription.Value);
			AssertEquals("CodeDescription DynamicData HasChanges", false, codeDescription.HasChanges);
			AssertEquals("CodeDescription DynamicData IsOverridden", true, codeDescription.IsOverridden);
			AssertEquals("CodeDescription value has been proxied to the underlying data source", "updated by user", dummy.CodeDescription);

			dynamicData.CancelChanges();

			AssertEquals("Code DynamicData Value has been reset to original", "", code.Value);
			AssertEquals("CodeDescription DynamicData Value has been reset to original", "", codeDescription.Value);

			AssertEquals("Code value has been proxied to the underlying data source", "", dummy.Code);
			AssertEquals("CodeDescription value has been proxied to the underlying data source", "", dummy.CodeDescription);
		}

		#endregion

		#region TestUpdatingRelatedPropertyValue

		public void TestUpdatingRelatedPropertyValue()
		{
			var dummier = Factory.New<DummierBusinessObject>();

			var dynamicData = dummier.MakeDynamic(dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var code = dynamicData.GetDynamicProperty(nameof(dummier.Z0_Code));
			var desc = dynamicData.GetDynamicProperty(nameof(dummier.Z0_Description));

			AssertEquals("prerequisite: Z0_Code DynamicData Value", ZString.Empty, code.Value);
			AssertEquals("prerequisite: Z0_Description DynamicData Value", ZString.Empty, desc.Value);

			AssertEquals("prerequisite: Z0_Code", ZString.Empty, dummier.Z0_Code);
			AssertEquals("prerequisite: Z0_Description", ZString.Empty, dummier.Z0_Description);

			AssertEquals("prerequisite: Z0_Code IsOverridden", false, code.IsOverridden);
			AssertEquals("prerequisite: Z0_Description IsOverridden", false, desc.IsOverridden);

			var descOnValueChangeFired = false;
			desc.SubscribeOnValueChanged("xxx", () => descOnValueChangeFired = true);

			code.SetValue("AAA");

			AssertEquals("Z0_Code DynamicData Value", "AAA", code.Value);
			AssertEquals("Z0_Description DynamicData Value has been updated from Z0_Code setter", "AAA - description", desc.Value);

			AssertEquals("Z0_Code underlying data source has been updated", "AAA", dummier.Z0_Code);
			AssertEquals("Z0_Description underlying data source has been updated", "AAA - description", dummier.Z0_Description);

			AssertEquals("Z0_Code is marked as overridden", true, code.IsOverridden);
			AssertEquals("Z0_Description is marked as overridden", true, desc.IsOverridden);

			Assert("Z0_Description OnValueChanged fired", descOnValueChangeFired);
		}

		#endregion

		#region TestAcceptChanges

		public void TestAcceptChanges()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "AAA";

			var dynamicData = dummy.MakeDynamic(dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var code = dynamicData.GetDynamicProperty(nameof(DummyBusinessObject.Z0_Code));
			AssertEquals("Z0_Code is not overridden", false, code.IsOverridden);

			code.SetValue("BBB");
			AssertEquals("Z0_Code is overridden", true, code.IsOverridden);

			code.AcceptChanges();
			AssertEquals("Z0_Code is not overridden", false, code.IsOverridden);

			dummy.Z0_Code = "CCC";
			AssertEquals("Z0_Code is overridden", true, code.IsOverridden);

			code.AcceptChanges();
			AssertEquals("Z0_Code is not overridden", false, code.IsOverridden);
		}

		#endregion

		#region Metadata

		public void TestIdentifier()
		{
			var dummier = Factory.New<DummierBusinessObject>();

			var dynamicData = dummier.MakeDynamic(metaDataProvider: new DocDataObjectMetaDataProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			AssertEquals("BizObj Identifier", dummier.PK, dynamicData.GetMetaData<object>(Core.MetaDataType.Identifier));

			var code = dynamicData.GetDynamicProperty(nameof(dummier.Z0_Code));

			AssertEquals("Property Identifier", null, code.GetMetaData<object>(Core.MetaDataType.Identifier));
		}

		public void TestNaturalKey()
		{
			var dummier = Factory.New<DummierBusinessObject>();

			var dynamicData = dummier.MakeDynamic(metaDataProvider: new DocDataObjectMetaDataProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var collection = dynamicData.GetDynamicProperty(nameof(dummier.Collection));

			AssertEquals("NaturalKey", "Z0_ChildOnly", collection.GetMetaData<string>(Core.MetaDataType.NaturalKey));
		}

		public void TestIsReadOnly_PropertyWithGetterAndSetter()
		{
			var dummier = Factory.New<DummierBusinessObject>();

			var dynamicData = dummier.MakeDynamic(metaDataProvider: new DocDataObjectMetaDataProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var code = dynamicData.GetDynamicProperty(nameof(dummier.Z0_Code));

			AssertEquals("IsReadOnly", false, code.GetMetaData<bool>(Core.MetaDataType.IsReadOnly));
		}

		public void TestIsReadOnly_PropertyWithJustGetter()
		{
			var dummier = Factory.New<DummierBusinessObject>();

			var dynamicData = dummier.MakeDynamic(metaDataProvider: new DocDataObjectMetaDataProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var propertyWithJustGetter = dynamicData.GetDynamicProperty(nameof(dummier.PropertyWithJustGetter));

			AssertEquals("IsReadOnly", true, propertyWithJustGetter.GetMetaData<bool>(Core.MetaDataType.IsReadOnly));
		}

		public void TestIsReadOnly_PropertyMarkedAsReadOnly()
		{
			var dummier = Factory.New<DummierBusinessObject>();

			var dynamicData = dummier.MakeDynamic(metaDataProvider: new DocDataObjectMetaDataProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var propertyMarkedAsReadOnly = dynamicData.GetDynamicProperty(nameof(dummier.PropertyMarkedAsReadOnly));

			AssertEquals("IsReadOnly", true, propertyMarkedAsReadOnly.GetMetaData<bool>(Core.MetaDataType.IsReadOnly));
		}

		public void TestMaxLength()
		{
			var dummier = Factory.New<DummierBusinessObject>();

			var dynamicData = dummier.MakeDynamic(metaDataProvider: new DocDataObjectMetaDataProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var propertyWithMaxLength = dynamicData.GetDynamicProperty(nameof(dummier.PropertyWithMaxLength));

			AssertEquals("PropertyWithMaxLength", 12, propertyWithMaxLength.GetMetaData<int>(Core.MetaDataType.MaxLength));
		}

		public void TestListDataSource()
		{
			var dummier = Factory.New<DummierBusinessObject>();

			var dynamicData = dummier.MakeDynamic(metaDataProvider: new DocDataObjectMetaDataProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var propertyWithListLookup = dynamicData.GetDynamicProperty(nameof(dummier.PropertyWithListLookup));

			AssertEquals("ListDataSource", dummier.List1, propertyWithListLookup.GetMetaData<object>(Core.MetaDataType.ListDataSource));
		}

		public void TestListDataSource_Dynamic()
		{
			var dummier = Factory.New<DummierBusinessObject>();
			dummier.UseList2ForPropertyWithDynamicListLookup = false;

			var dynamicData = dummier.MakeDynamic(metaDataProvider: new DocDataObjectMetaDataProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var propertyWithDynamicListLookup = dynamicData.GetDynamicProperty(nameof(dummier.PropertyWithDynamicListLookup));

			AssertEquals("ListDataSource", dummier.List1, propertyWithDynamicListLookup.GetMetaData<object>(Core.MetaDataType.ListDataSource));

			dummier.UseList2ForPropertyWithDynamicListLookup = true;

			AssertEquals("ListDataSource", dummier.List2, propertyWithDynamicListLookup.GetMetaData<object>(Core.MetaDataType.ListDataSource));
		}

		public void TestPropertyMarkedAsIgnoreChanges()
		{
			var dummier = Factory.New<DummierBusinessObject>();

			var dynamicData = dummier.MakeDynamic(metaDataProvider: new DocDataObjectMetaDataProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var propertyMarkedAsIgnoreChanges = dynamicData.GetDynamicProperty(nameof(dummier.PropertyMarkedAsIgnoreChanges));
			var propertyNotMarkedAsIgnoreChanges = dynamicData.GetDynamicProperty(nameof(dummier.Z0_Code));

			AssertEquals("IsNonOverridable (property with IgnoreChanges attribute)", true, propertyMarkedAsIgnoreChanges.GetMetaData<object>(Core.MetaDataType.IsNonOverridable));
			AssertEquals("SuppressHasChanges (property with IgnoreChanges attribute)", true, propertyMarkedAsIgnoreChanges.GetMetaData<object>(Core.MetaDataType.SuppressHasChanges));

			AssertEquals("IsNonOverridable (property without IgnoreChanges attribute)", false, propertyNotMarkedAsIgnoreChanges.GetMetaData<object>(Core.MetaDataType.IsNonOverridable));
			AssertEquals("SuppressHasChanges (property without IgnoreChanges attribute)", false, propertyNotMarkedAsIgnoreChanges.GetMetaData<object>(Core.MetaDataType.SuppressHasChanges));
		}

		public void TestBindTo()
		{
			var dummier = Factory.New<DummierBusinessObject>();
			dummier.UseList2ForPropertyWithDynamicListLookup = false;

			var dynamicData = dummier.MakeDynamic(metaDataProvider: new DocDataObjectMetaDataProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var codeDescProperty = dynamicData.GetDynamicProperty(nameof(dummier.CodeDesc));
			var stringProperty = dynamicData.GetDynamicProperty(nameof(dummier.Z0_Code));

			AssertEquals("BindTo", nameof(ICodeDescription.Code), codeDescProperty.GetMetaData<string>(Core.MetaDataType.BindTo));
			AssertNull("BindTo", stringProperty.GetMetaData<object>(Core.MetaDataType.BindTo));
		}

		public void TestDateTimeFormat()
		{
			var dummier = Factory.New<DummierBusinessObject>();

			var dynamicData = dummier.MakeDynamic(metaDataProvider: new DocDataObjectMetaDataProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());

			var propertyWithDateTimeFormat = dynamicData.GetDynamicProperty(nameof(dummier.PropertyWithDateTimeFormat));
			var metadata = propertyWithDateTimeFormat.GetMetaData<KDateTimeFormat>(Core.MetaDataType.DateTimeFormat);

			AssertEquals($"{Core.MetaDataType.DateTimeFormat} metadata", KDateTimeFormat.Time, metadata);
		}

		#endregion

		#region Nested types

		sealed class DummierBusinessObject : DummyBusinessObject
		{
			public DummierBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();

				Z0_Code = ZString.Empty;
			}

			public override ZString Z0_Code
			{
				get => base.Z0_Code;
				set
				{
					base.Z0_Code = value;
					Z0_Description = !value.IsEmpty
						? (ZString)$"{value} - description"
						: ZString.Empty;
				}
			}

			[NaturalKey("Z0_ChildOnly")]
			public new DummyChildBusinessObjectCollection Collection => base.Collection;

			#region PropertyWithJustGetter

			public ZString PropertyWithJustGetter => "aaa";

			public ZPropertyInfo PropertyWithJustGetterInfo => GetZPropertyInfo(nameof(PropertyWithJustGetter));

			#endregion

			#region PropertyMarkedAsReadOnly

			[ReadOnly(true)]
			public ZString PropertyMarkedAsReadOnly
			{
				get => propertyMarkedAsReadOnly;
				set => propertyMarkedAsReadOnly = value;
			}
			string propertyMarkedAsReadOnly;

			public ZPropertyInfo PropertyMarkedAsReadOnlyInfo => GetZPropertyInfo(nameof(PropertyMarkedAsReadOnly));

			#endregion

			#region PropertyWithMaxLength

			[MaxLength(NotificationTypes.MessageError, 12)]
			public ZString PropertyWithMaxLength { get; set; }

			public ZPropertyInfo PropertyWithMaxLengthInfo => GetZPropertyInfo(nameof(PropertyWithMaxLength));

			#endregion

			#region PropertyWithListLookup

			[List(nameof(List1))]
			public ZString PropertyWithListLookup
			{
				get => propertyWithListLookup;
				set => propertyWithListLookup = value;
			}

			string propertyWithListLookup;

			public ZPropertyInfo PropertyWithListLookupInfo => GetZPropertyInfo(nameof(PropertyWithListLookup));

			#endregion

			#region PropertyReturningDifferentListDependingOnCondition

			[List(nameof(DynamicList))]
			public ZString PropertyWithDynamicListLookup
			{
				get => propertyWithDynamicListLookup;
				set => propertyWithDynamicListLookup = value;
			}

			string propertyWithDynamicListLookup;

			public ZPropertyInfo PropertyWithDynamicListLookupInfo => GetZPropertyInfo(nameof(PropertyWithDynamicListLookup));

			public ZBool UseList2ForPropertyWithDynamicListLookup { get; set; }

			public ICodeDescriptionPairList DynamicList => UseList2ForPropertyWithDynamicListLookup
				? List2
				: List1;

			#endregion

			#region PropertyMarkedAsIgnoreChanges

			[IgnoreChanges]
			public ZString PropertyMarkedAsIgnoreChanges { get; set; }

			public ZPropertyInfo PropertyMarkedAsIgnoreChangesInfo => GetZPropertyInfo(nameof(PropertyMarkedAsIgnoreChanges));

			#endregion

			#region PropertyWithDateTimeFormat

			[DateTimeFormat(KDateTimeFormat.Time)]
			public ZDateTime PropertyWithDateTimeFormat { get; set; }

			public ZPropertyInfo PropertyWithDateTimeFormatInfo => GetZPropertyInfo(nameof(PropertyWithDateTimeFormat));

			#endregion

			public ICodeDescription CodeDesc { get; set; }

			#region Lists

			public ICodeDescriptionPairList List1 => list1 ?? (list1 = new CodeDescriptionPairList(OLookUpEditType.TransportType));
			ICodeDescriptionPairList list1;

			public ICodeDescriptionPairList List2 => list2 ?? (list2 = new CodeDescriptionPairList(OLookUpEditType.ContainerType));
			ICodeDescriptionPairList list2;

			#endregion
		}

		#endregion
	}
}
