using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	sealed class OperationalActionFieldDescriptorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAll()
		{
			using (FieldDescriptor1.GetValidationSuspender())
			{
				FieldDescriptor1.Order = ZByte.Zero;
			}

			AssertNoErrors(FieldDescriptor1);
			FieldDescriptor1.Validation.ValidateAll();
			AssertHasErrors(FieldDescriptor1.FieldCaptionInfo);
			AssertHasErrors(FieldDescriptor1.FieldNameInfo);
			AssertHasErrors(FieldDescriptor1.OrderInfo);
		}

		public void TestValidateFieldCaption()
		{
			TestValidateProperty(OperationalActionFieldDescriptor.Schema.FieldCaption, ZString.Empty, (ZString)"x");
		}

		public void TestValidateFieldName_WhenFieldIsCustomFieldFromOtherCompany()
		{
			var customFieldDescriptor = GenerateCustomFieldUnderDifferentCompany();
			customFieldDescriptor.Validation.ValidateFieldName();

			AssertNoErrors(customFieldDescriptor.FieldNameInfo);
			AssertHasWarning(customFieldDescriptor.FieldNameInfo, "This field is a custom field from the following templates not available in current context:\r\nCustomFieldTemplate\r\n");
		}

		public void TestValidateFieldName()
		{
			const string error1 = "Please enter a Field Name.";
			const string error2 = "Specifying the same field more than once is not meaningful without also specifying filters.";
			const string error3 = "Select a valid field.";
			const string error4 = "This field cannot be bulk updated.";
			FieldDescriptor1.FieldName = ZString.Empty;
			AssertHasError(FieldDescriptor1.FieldNameInfo, error1);
			FieldDescriptor1.FieldName = DummyBizoSchema.Constants.Z0_VarCharMax;
			AssertNoErrors(FieldDescriptor1.FieldNameInfo);
			FieldDescriptor2.FieldName = DummyBizoSchema.Constants.Z0_VarCharMax;
			AssertHasError(FieldDescriptor2.FieldNameInfo, error2);
			FieldDescriptor1.Filter = "Z0_Code == \"COD\"";
			FieldDescriptor1.Validation.ValidateFieldName();
			AssertHasError(FieldDescriptor1.FieldNameInfo, error2);
			FieldDescriptor2.Filter = "Z0_Code != \"COD\"";
			FieldDescriptor1.Validation.ValidateFieldName();
			AssertNoErrors(FieldDescriptor1.FieldNameInfo);
			FieldDescriptor1.Filter = ZString.Empty;
			FieldDescriptor1.Validation.ValidateFieldName();
			AssertHasError(FieldDescriptor1.FieldNameInfo, error2);
			FieldDescriptor1.FieldName = "ZZZ";
			AssertHasError(FieldDescriptor1.FieldNameInfo, error3);
			FieldDescriptor1.FieldName = "Z0_Calculated";
			AssertHasError(FieldDescriptor1.FieldNameInfo, error4);
		}

		public void TestValidateFieldName_HasChanges()
		{
			FieldDescriptor1.FieldName = DummyBizoSchema.Constants.Z0_VarCharMax;
			AssertNoErrors(FieldDescriptor1.FieldNameInfo);
			using (FieldDescriptor1.GetValidationSuspender())
			{
				FieldDescriptor1.FieldName = "ZZZ";
			}
			FieldDescriptor1.HasChanges = false;
			FieldDescriptor1.Validation.Parent.HasChanges = false;
			FieldDescriptor1.RunPreSaveValidation();
			AssertNoErrors(FieldDescriptor1.FieldNameInfo);
			using (FieldDescriptor1.GetValidationSuspender())
			{
				FieldDescriptor1.FieldName = "Z0_Calculated";
			}
			FieldDescriptor1.HasChanges = false;
			FieldDescriptor1.Validation.Parent.HasChanges = false;
			FieldDescriptor1.RunPreSaveValidation();
			AssertNoErrors(FieldDescriptor1.FieldNameInfo);
			const string errorMessage = "Select a valid field.";
			FieldDescriptor1.FieldName = "ZZZ";
			AssertHasError(FieldDescriptor1.FieldNameInfo, errorMessage);
		}

		public void TestDefaultValueDateTime()
		{
			FieldDescriptor1.FieldName = DummyBusinessObject.Schema.Z0_Date;
			FieldDescriptor1.DefaultingStrategy = "FXD";
			fieldDescriptor1.DefaultValue = "Not a date";
			AssertHasError(FieldDescriptor1.DefaultValueInfo, "Please pick a valid date.");
		}

		public void TestDefaultValueDateTimeOffset()
		{
			FieldDescriptor1.FieldName = DummyBusinessObject.Schema.Z0_DateTimeOffset;
			FieldDescriptor1.DefaultingStrategy = "FXD";
			fieldDescriptor1.DefaultValue = "Not a date";
			AssertHasError(FieldDescriptor1.DefaultValueInfo, "Please pick a valid date.");
		}

		public void TestDefaultValueTime()
		{
			FieldDescriptor1.FieldName = DummyBusinessObject.Schema.Z0_Time;
			FieldDescriptor1.DefaultingStrategy = "FXD";
			fieldDescriptor1.DefaultValue = "Not a time";
			AssertHasError(FieldDescriptor1.DefaultValueInfo, "Please pick a valid time.");
		}

		public void TestDefaultValueGeography()
		{
			FieldDescriptor1.FieldName = DummyBusinessObject.Schema.Z0_Geography;
			FieldDescriptor1.DefaultingStrategy = "FXD";
			fieldDescriptor1.DefaultValue = "Not a geography";
			AssertHasError(FieldDescriptor1.DefaultValueInfo, "Please pick a valid geography.");
		}

		public void TestValidateOrder()
		{
			TestValidateProperty(OperationalActionFieldDescriptor.Schema.Order, ZByte.Zero, (ZByte)1);
		}

		public void TestValidateDefaultingStrategy_WhenCustomFieldFromOtherCompany()
		{
			var customFieldDescriptor = GenerateCustomFieldUnderDifferentCompany();
			customFieldDescriptor.Validation.ValidateDefaultingStrategy();

			AssertEquals("DefaultingStrategy field should be readonly if list is empty", true, customFieldDescriptor.DefaultingStrategyInfo.ReadOnly);
			AssertNoErrors(customFieldDescriptor.DefaultingStrategyInfo);
		}

		public void TestValidateDefaultingStrategy()
		{
			FieldDescriptor1.FieldName = DummyBusinessObject.Schema.Z0_Code;
			FieldDescriptor1.DefaultingStrategy = "FXD";
			AssertNoNotifications(FieldDescriptor1.DefaultingStrategyInfo);
			FieldDescriptor1.DefaultingStrategy = "XXX";
			AssertHasError(FieldDescriptor1.DefaultingStrategyInfo, "Enter a valid Default Method.");
			FieldDescriptor1.DefaultingStrategy = "";
			AssertNoNotifications(FieldDescriptor1.DefaultingStrategyInfo);
		}

		public void TestFilter()
		{
			FieldDescriptor1.Filter = "Other.Z0_Number";
			AssertHasError(FieldDescriptor1.FilterInfo, "unexpected end of expression");
			FieldDescriptor1.Filter = "";
			AssertNoErrors(FieldDescriptor1.FilterInfo);
			FieldDescriptor1.Filter = "Other.Z0_Int == \"0\"";
			AssertHasError(FieldDescriptor1.FilterInfo, "'Other.Z0_Int' is not a valid field.");
			FieldDescriptor1.Filter = "Other.Z0_Number == \"0\"";
			AssertNoErrors(FieldDescriptor1.FilterInfo);
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

		void TestValidateProperty(string propertyName, IZType emptyValue, IZType validValue)
		{
			ZPropertyInfo fieldDescriptor1PropertyInfo = FieldDescriptor1.ZPropertyInfoHash[propertyName];
			ZPropertyInfo fieldDescriptor2PropertyInfo = FieldDescriptor2.ZPropertyInfoHash[propertyName];
			fieldDescriptor1PropertyInfo.Value = emptyValue;
			AssertHasErrorContaining(fieldDescriptor1PropertyInfo, "Please enter a ");
			fieldDescriptor1PropertyInfo.Value = validValue;
			AssertNoErrors(fieldDescriptor1PropertyInfo);
			fieldDescriptor2PropertyInfo.Value = validValue;
			AssertHasError(fieldDescriptor2PropertyInfo, "The " + fieldDescriptor1PropertyInfo.HumanReadableName + " has been duplicated and must be unique.");
		}

		OperationalActionFieldDescriptor FieldDescriptor1
		{
			get
			{
				return fieldDescriptor1 ?? (fieldDescriptor1 = FieldDescriptors.AddNew());
			}
		}

		OperationalActionFieldDescriptor fieldDescriptor1;

		OperationalActionFieldDescriptor FieldDescriptor2
		{
			get
			{
				return fieldDescriptor2 ?? (fieldDescriptor2 = FieldDescriptors.AddNew());
			}
		}

		OperationalActionFieldDescriptor fieldDescriptor2;
		OperationalActionFieldDescriptorCollection FieldDescriptors
		{
			get
			{
				return fieldDescriptors ?? (fieldDescriptors = new OperationalActionFieldDescriptorCollection(Action));
			}
		}

		OperationalActionFieldDescriptorCollection fieldDescriptors;

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
