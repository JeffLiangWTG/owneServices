using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(OperationalActionFieldDescriptor))]
	sealed class OperationalActionFieldDescriptorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTemplateNamesForCustomFieldNotInCurrentContext_WhenNullWorkflowType()
		{
			var newFieldDescriptor = (OperationalActionFieldDescriptor)GetNewBusinessObject();

			AssertNull(newFieldDescriptor.Context.WorkflowType);
			AssertNotNull(newFieldDescriptor.TemplateNamesForCustomFieldNotInCurrentContext);
			AssertEquals("Null workflow type will not load template names", newFieldDescriptor.TemplateNamesForCustomFieldNotInCurrentContext.Count, 0);
		}

		public void TestTemplateNamesForCustomFieldNotInCurrentContext_WhenEmptyFieldName()
		{
			var contextWithWorkflowType = new OperationalActionContext(ActionSupporter, "Module Name", "XXX");
			var actionWithWorkflowType = Factory.New<OperationalAction>();
			actionWithWorkflowType.Context = contextWithWorkflowType;
			var customFieldDescriptor = new OperationalActionFieldDescriptor(actionWithWorkflowType);

			AssertNotNull(customFieldDescriptor.Context.WorkflowType);
			AssertNullOrEmpty(customFieldDescriptor.FieldName);
			AssertNotNull(customFieldDescriptor.TemplateNamesForCustomFieldNotInCurrentContext);
			AssertEquals("Empty field name will not load template names", customFieldDescriptor.TemplateNamesForCustomFieldNotInCurrentContext.Count, 0);
		}

		public void TestTemplateNamesForCustomFieldNotInCurrentContext()
		{
			var customFieldDescriptor = GenerateCustomFieldUnderDifferentCompany();

			AssertNotNull(customFieldDescriptor.TemplateNamesForCustomFieldNotInCurrentContext);
			AssertEquals(1, customFieldDescriptor.TemplateNamesForCustomFieldNotInCurrentContext.Count);
			AssertEquals("CustomFieldTemplate", customFieldDescriptor.TemplateNamesForCustomFieldNotInCurrentContext.First());
		}

		public void TestDefaultingStrategyReadOnly()
		{
			var customFieldDescriptor = GenerateCustomFieldUnderDifferentCompany();
			customFieldDescriptor.FieldName = "Wrong";

			AssertEquals(true, customFieldDescriptor.DefaultingStrategyInfo.ReadOnly);
		}

		public void TestReadOnly()
		{
			Action.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			Action.SU_IsSystemDefined = false;
			AssertEquals("Should not be readonly as not system defined", false, FieldDescriptor.ReadOnly);
			Action.SU_IsSystemDefined = true;
			AssertEquals("Should be readonly as now system defined", true, FieldDescriptor.ReadOnly);
			Action.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("Should not be readonly as editing of system defined actions is now allowed", false, FieldDescriptor.ReadOnly);
		}

		public void TestMaxLengths()
		{
			AssertEquals("FieldCaptionInfo.MaxLength", OperationalActionFieldSupporter.MaxDefaultCaptionLength, FieldDescriptor.FieldCaptionInfo.MaxLength);
			AssertEquals("FieldNameInfo.MaxLength", OperationalActionFieldSupporter.MaxFieldLength, FieldDescriptor.FieldNameInfo.MaxLength);
		}

		public void TestActionSupportable()
		{
			OperationalActionSupporter actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter;
			OperationalAction action = Factory.New<OperationalAction>();
			action.Context = Context;
			OperationalActionFieldDescriptor fieldDescriptor = new OperationalActionFieldDescriptor(action);
			AssertEquals("ActionSupportable", Context, fieldDescriptor.Context);
		}

		public void TestDefaultCaptionFromName()
		{
			using (IMockResourceStringCache mockCache = Res.UseMockData())
			{
				mockCache.Put("Common|Validation|InvalidPropertySelection", new ResourceStringData("Common|Validation|InvalidPropertySelection", string.Empty, string.Empty, "Selection {0}", string.Empty));
				mockCache.Put("DummyBizo|Z0_VarCharMax", new ResourceStringData("DummyBizo|Z0_VarCharMax", string.Empty, string.Empty, "Text Caption", string.Empty));
				mockCache.Put("DummyBizo|Z0_Date", null);
				FieldDescriptor.FieldName = "";
				FieldDescriptor.FieldCaption = "";
				FieldDescriptor.FieldName = "Z0_VarCharMax";
				AssertEquals("Set the caption from the resource strings", "Text Caption", FieldDescriptor.FieldCaption);
				fieldDescriptor.FieldName = "Z0_AnotherNumber";
				AssertEquals("Clear the caption", "", FieldDescriptor.FieldCaption);
				fieldDescriptor.FieldName = "CustomField";
				AssertEquals("CustomField", FieldDescriptor.FieldCaption);
			}
		}

		public void TestRunPreSaveValidation()
		{
			AssertNoErrors(FieldDescriptor);
			FieldDescriptor.RunPreSaveValidation();
			AssertHasErrors(FieldDescriptor.FieldNameInfo);
			AssertHasErrors(FieldDescriptor.FieldCaptionInfo);
		}

		public void TestSerialization()
		{
			const string expectedXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + "<OperationalActionFieldDescriptor>\n" + "<FieldName>Name</FieldName>\n" + "<FieldCaption>Caption</FieldCaption>\n" + "<Filter>Filter</Filter>\n" + "<Order>10</Order>\n" + "<EmptyBehaviour>MAN</EmptyBehaviour>\n" + "<Default strategy=\"\" />\n" + "</OperationalActionFieldDescriptor>\n" + "";
			FieldDescriptor.FieldCaption = "Caption";
			FieldDescriptor.FieldName = "Name";
			FieldDescriptor.Filter = "Filter";
			FieldDescriptor.Order = 10;
			FieldDescriptor.EmptyBehaviour = EmptyBehaviourList.Codes.Mandatory;
			byte[] serializedValue;
			using (MemoryStream stream = new MemoryStream())
			using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement(OperationalActionFieldDescriptorCollection.Schema.XmlElementName);
				((IXmlSerializable)FieldDescriptor).WriteXml(writer);
				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();
				serializedValue = stream.ToArray();
			}

			AssertMultilineASCIIEquals("", ((char)65279) + expectedXml, System.Text.Encoding.UTF8.GetString(serializedValue).Replace("><", ">\n<"));
			OperationalActionFieldDescriptor anotherFieldDescriptor = (OperationalActionFieldDescriptor)GetNewBusinessObject();
			using (MemoryStream stream = new MemoryStream(serializedValue))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				((IXmlSerializable)anotherFieldDescriptor).ReadXml(reader);
			}

			AssertEquals("FieldCaption", "Caption", anotherFieldDescriptor.FieldCaption);
			AssertEquals("FieldName", "Name", anotherFieldDescriptor.FieldName);
			AssertEquals("Filter", "Filter", anotherFieldDescriptor.Filter);
			AssertEquals("Order", (ZByte)10, anotherFieldDescriptor.Order);
			AssertEquals("EmptyBehaviour", EmptyBehaviourList.Codes.Mandatory, anotherFieldDescriptor.EmptyBehaviour);
		}

		[TestDate(2008, 09, 01)]
		public void TestDefaultValue()
		{
			FieldDescriptor.FieldName = AutoDummyBizo.Schema.Z0_Date;
			FieldDescriptor.DefaultingStrategy = "DAY";
			FieldDescriptor.DefaultValue = "5";
			AssertEquals("DAY", FieldDescriptor.DefaultingStrategy);
			AssertEquals("5", FieldDescriptor.DefaultValue);
			AssertEquals(false, FieldDescriptor.DefaultValueInfo.ReadOnly);
			AssertEquals(4, FieldDescriptor.DefaultValueInfo.MaxLength);
			AssertEquals(new ZDateTime(2008, 09, 06), FieldDescriptor.GetDefaultValue<ZDateTime>());

			FieldDescriptor.DefaultingStrategy = "FXD";
			AssertEquals("FXD", FieldDescriptor.DefaultingStrategy);
			AssertEquals("", FieldDescriptor.DefaultValue);
			AssertEquals(false, FieldDescriptor.DefaultValueInfo.ReadOnly);
			AssertEquals(15, FieldDescriptor.DefaultValueInfo.MaxLength);
			AssertEquals(ZDateTime.Empty, FieldDescriptor.GetDefaultValue<ZDateTime>());

			FieldDescriptor.DefaultValue = "05-FEB-81";
			AssertEquals("FXD", FieldDescriptor.DefaultingStrategy);
			AssertEquals("05-FEB-81", FieldDescriptor.DefaultValue);
			AssertEquals(false, FieldDescriptor.DefaultValueInfo.ReadOnly);
			AssertEquals(new ZDateTime(1981, 02, 05), FieldDescriptor.GetDefaultValue<ZDateTime>());

			FieldDescriptor.FieldName = "Blat";
			AssertEquals("", FieldDescriptor.DefaultingStrategy);
			AssertEquals("", FieldDescriptor.DefaultValue);
			AssertEquals(true, FieldDescriptor.DefaultValueInfo.ReadOnly);
			AssertEquals(ZDateTime.Empty, FieldDescriptor.GetDefaultValue<ZDateTime>());
		}

		[TestDate(2008, 09, 01)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestDefaultValue_DateTimeOffsetVersion()
		{
			TestDateAttribute.UseUNLOCO = true;
			AssertEquals(new ZDateTime(2008, 09, 01, 0, 0, 0), ZDateTime.UtcNow);
			AssertEquals(new ZDateTime(2008, 09, 01, 10, 0, 0), ZDateTime.Now);
			FieldDescriptor.FieldName = DummyBusinessObject.Schema.Z0_DateTimeOffset;
			FieldDescriptor.DefaultingStrategy = "DAY";
			FieldDescriptor.DefaultValue = "5";
			AssertEquals("DAY", FieldDescriptor.DefaultingStrategy);
			AssertEquals("5", FieldDescriptor.DefaultValue);
			AssertEquals(false, FieldDescriptor.DefaultValueInfo.ReadOnly);
			AssertEquals(new ZDateTimeOffset(2008, 09, 06, 10, 0, 0, TimeSpan.FromHours(10)), FieldDescriptor.GetDefaultValue<ZDateTimeOffset>());
			FieldDescriptor.DefaultingStrategy = "FXD";
			AssertEquals("FXD", FieldDescriptor.DefaultingStrategy);
			AssertEquals("", FieldDescriptor.DefaultValue);
			AssertEquals(false, FieldDescriptor.DefaultValueInfo.ReadOnly);
			AssertEquals(ZDateTimeOffset.Empty, FieldDescriptor.GetDefaultValue<ZDateTimeOffset>());
			fieldDescriptor.DefaultValue = "05-FEB-81";
			AssertEquals("FXD", FieldDescriptor.DefaultingStrategy);
			AssertEquals("05-FEB-81", FieldDescriptor.DefaultValue);
			AssertEquals(false, FieldDescriptor.DefaultValueInfo.ReadOnly);
			AssertEquals(new ZDateTimeOffset(1981, 02, 05, 0, 0, 0, TimeSpan.FromHours(10)), FieldDescriptor.GetDefaultValue<ZDateTimeOffset>());
			FieldDescriptor.FieldName = "Blat";
			AssertEquals("", FieldDescriptor.DefaultingStrategy);
			AssertEquals("", FieldDescriptor.DefaultValue);
			AssertEquals(true, FieldDescriptor.DefaultValueInfo.ReadOnly);
			AssertEquals(ZDateTimeOffset.Empty, FieldDescriptor.GetDefaultValue<ZDateTimeOffset>());
		}

		public void TestDefaultValue_GeographyVersion()
		{
			FieldDescriptor.FieldName = DummyBusinessObject.Schema.Z0_Geography;
			FieldDescriptor.DefaultingStrategy = "FXD";
			FieldDescriptor.DefaultValue = "POINT (-121 48)";
			AssertEquals("FXD", FieldDescriptor.DefaultingStrategy);
			AssertEquals("POINT (-121 48)", FieldDescriptor.DefaultValue);
			AssertEquals(false, FieldDescriptor.DefaultValueInfo.ReadOnly);
			AssertEquals(new ZGeography("POINT (-121 48)"), FieldDescriptor.GetDefaultValue<ZGeography>());
		}

		public void TestDefaultValue_MaxLength_WhenCustomFieldFromOtherContextAndDefaultValueIsEmpty()
		{
			var customFieldDescriptor = GenerateCustomFieldUnderDifferentCompany();

			AssertNullOrEmpty(customFieldDescriptor.DefaultValue);
			AssertEquals(int.MaxValue, customFieldDescriptor.DefaultValueInfo.MaxLength);
		}

		public void TestDefaultValue_MaxLength_WhenCustomFieldFromOtherContextAndDefaultValueIsNotEmpty()
		{
			const string defaultValue = "DefaultValue";
			var customFieldDescriptor = GenerateCustomFieldUnderDifferentCompany();
			customFieldDescriptor.DefaultValue = defaultValue;

			AssertNotNullOrEmpty(customFieldDescriptor.DefaultValue);
			AssertEquals(defaultValue.Length, customFieldDescriptor.DefaultValueInfo.MaxLength);
		}

		public void TestDefaultValue_MaxLength_WhenCustomFieldFromOtherContextAndDefaultStrategyIsEmpty()
		{
			var customFieldDescriptor = GenerateCustomFieldUnderDifferentCompany();
			customFieldDescriptor.DefaultingStrategy = ZString.Empty;

			AssertEquals(0, customFieldDescriptor.DefaultValue.Length);
			AssertEquals(1, customFieldDescriptor.DefaultValueInfo.MaxLength);
		}

		public void TestSerializationOfOversizedDefaultValue()
		{
			const string storedXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + "<OperationalActionFieldDescriptor>\n" + "<FieldName>Name</FieldName>\n" + "<FieldCaption>Caption</FieldCaption>\n" + "<Filter>Filter</Filter>\n" + "<Order>10</Order>\n" + "<EmptyBehaviour>MAN</EmptyBehaviour>\n" + "<Default strategy=\"\">XXX</Default>\n" + "</OperationalActionFieldDescriptor>\n" + "";
			var fieldDescriptor = (OperationalActionFieldDescriptor)GetNewBusinessObject();
			using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(storedXml)))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				((IXmlSerializable)fieldDescriptor).ReadXml(reader);
			}

			AssertEquals("X", fieldDescriptor.DefaultValue);
		}

		public void TestOrder_RevalidateDuplicateBizo()
		{
			var collection = Action.FieldDescriptors;
			collection.RemoveAndDeleteAll();
			collection.AddNew().Order = 2;
			collection.AddNew().Order = 2;
			Assert("Validation error for the second bizo", collection[1].HasErrors);
			collection[0].Order = 1;
			Assert("The second bizo has been revalidated", !collection[1].HasErrors);
		}

		public void TestFieldName_NoChangeIfContainingTrailingSpaces()
		{
			var customFieldDescriptor = GenerateCustomFieldUnderDifferentCompany();

			AssertEquals("FXD", customFieldDescriptor.DefaultingStrategy);
			AssertNotNull(customFieldDescriptor.TemplateNamesForCustomFieldNotInCurrentContext);
			AssertGreaterThan(customFieldDescriptor.TemplateNamesForCustomFieldNotInCurrentContext.Count, 0);

			var originalFieldName = customFieldDescriptor.FieldName;
			customFieldDescriptor.FieldName += "   ";

			AssertEquals(originalFieldName, customFieldDescriptor.FieldName);
			AssertEquals("DefaultingStrategy is not reset because FieldName value is not changed", "FXD", customFieldDescriptor.DefaultingStrategy);
			AssertNotNull(customFieldDescriptor.TemplateNamesForCustomFieldNotInCurrentContext);
			AssertGreaterThan("TemplateNamesForCustomFieldNotInCurrentContext will not be reset as field name does not change", customFieldDescriptor.TemplateNamesForCustomFieldNotInCurrentContext.Count, 0);
		}

		public void TestFieldName_RelevantPropertiesAreResetWhenFieldNameIsChanged()
		{
			var customFieldDescriptor = GenerateCustomFieldUnderDifferentCompany();

			AssertEquals("FXD", customFieldDescriptor.DefaultingStrategy);
			AssertNotNull(customFieldDescriptor.TemplateNamesForCustomFieldNotInCurrentContext);
			AssertGreaterThan(customFieldDescriptor.TemplateNamesForCustomFieldNotInCurrentContext.Count, 0);

			customFieldDescriptor.FieldName = "Changed";
			AssertNullOrEmpty("DefaultingStrategy is cleared", customFieldDescriptor.DefaultingStrategy);
			AssertNotNull(customFieldDescriptor.TemplateNamesForCustomFieldNotInCurrentContext);
			AssertEquals("TemplateNamesForCustomFieldNotInCurrentContext will be reset as field name changes", customFieldDescriptor.TemplateNamesForCustomFieldNotInCurrentContext.Count, 0);
		}

		#region Implementation

		OperationalActionFieldDescriptor GenerateCustomFieldUnderDifferentCompany()
		{
			var anotherCompany = (BusinessObject)Factory.LoadTop1<IGlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, EnvProxy.Instance.CurrentCompany.PK));
			AssertNotNull("Precondition: should find another company", anotherCompany);

			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_Name = "CustomFieldTemplate";
			processTaskTemplate.P0_ProcessType = "XXX";
			processTaskTemplate.P0_GC = anotherCompany.PK;

			var def = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			def.XC_Name = "AAA";
			def.XC_Type = "STR";

			var contextWithWorkflowType = new OperationalActionContext(ActionSupporter, "Module Name", "XXX");
			var actionWithWorkflowType = Factory.New<OperationalAction>();
			actionWithWorkflowType.Context = contextWithWorkflowType;
			var customFieldDescriptor = new OperationalActionFieldDescriptor(actionWithWorkflowType);

			using (customFieldDescriptor.GetValidationSuspender())
			{
				customFieldDescriptor.FieldName = "AAA";
				customFieldDescriptor.DefaultingStrategy = "FXD";
				customFieldDescriptor.Order = 2;

				Factory.Save();
			}

			return customFieldDescriptor;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OperationalActionFieldDescriptor(Action);
		}

		OperationalActionFieldDescriptor FieldDescriptor
		{
			get
			{
				return fieldDescriptor ?? (fieldDescriptor = (OperationalActionFieldDescriptor)GetNewBusinessObject());
			}
		}

		OperationalActionFieldDescriptor fieldDescriptor;
		OperationalAction Action
		{
			get
			{
				if (action == null)
				{
					action = Factory.New<OperationalAction>();
					action.Context = Context;
				}

				return action;
			}
		}

		OperationalAction action;

		OperationalActionContext Context
		{
			get
			{
				return context ?? (context = new OperationalActionContext(ActionSupporter, "Module Name"));
			}
		}

		OperationalActionContext context;

		OperationalActionSupporter ActionSupporter
		{
			get
			{
				return actionSupporter ?? (actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter);
			}
		}

		OperationalActionSupporter actionSupporter;

		#endregion
	}
}
