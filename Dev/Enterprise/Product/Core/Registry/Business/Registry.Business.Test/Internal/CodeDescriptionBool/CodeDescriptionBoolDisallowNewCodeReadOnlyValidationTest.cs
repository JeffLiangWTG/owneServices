using CargoWise.Data;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolDisallowNewCodeReadOnlyValidation))]
	sealed class CodeDescriptionBoolDisallowNewCodeReadOnlyValidationTest : CodeDescriptionBoolDisallowNewTest
	{
		protected override void AssertCloneValues(RegistryBusinessObject clone1)
		{
			var clone = clone1 as CodeDescriptionBoolDisallowNewCodeReadOnlyValidation;
			AssertEquals("Code", "TSCOD", clone.Code);
			AssertEquals("Description", "DescriptionA", clone.Description);
			AssertEquals("CodeMaxLength", 5, clone.CodeMaxLength);
			Assert("SystemDefined", clone.SystemDefined);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (CodeDescriptionBoolDisallowNewCodeReadOnlyValidation)GetNewBusinessObject();
			result.CodeMaxLength = 5;
			result.Code = "TSCOD";
			result.Description = (NoResString)"DescriptionA";
			result.SystemDefined = true;
			return result;
		}

		public override void TestReadOnlyStates()
		{
			var result = (CodeDescriptionBool)GetNewBusinessObject();

			result.SystemDefined = false;
			AssertEquals("CodeInfo.ReadOnly", false, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", false, result.DescriptionInfo.ReadOnly);
			AssertEquals("BoolInfo.ReadOnly", false, result.BoolInfo.ReadOnly);

			result.SystemDefined = true;
			AssertEquals("CodeInfo.ReadOnly", true, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", true, result.DescriptionInfo.ReadOnly);
			AssertEquals("BoolInfo.ReadOnly", true, result.BoolInfo.ReadOnly);
		}

		public void TestValidateBoolAvoidValidationIfSettingTrue()
		{
			var item = (CodeDescriptionBoolDisallowNewCodeReadOnlyValidation)GetNewBusinessObject();

			var hits = Db.Connection.ExecutedCommandCount;
			item.Bool = true;
			AssertEquals("No need to execute any commands for default value", hits, Db.Connection.ExecutedCommandCount);
		}

		public void TestSystemDefinedAndCodeReadOnly()
		{
			var item = (CodeDescriptionBoolDisallowNewCodeReadOnlyValidation)GetNewBusinessObject();
			item.Code = "C01";
			item.Description = (NoResString)"Custom Exchange Rate 01";
			item.IsCodeReadOnly = true;
			AssertEquals("Item code should be readonly.", true, item.CodeInfo.ReadOnly);

			item = (CodeDescriptionBoolDisallowNewCodeReadOnlyValidation)GetNewBusinessObject();
			item.Code = "SEL";
			item.Description = (NoResString)"Sell Exchange Rate";
			item.SystemDefined = true;
			item.Bool = true;

			AssertEquals("Item code should be readonly.", true, item.CodeInfo.ReadOnly);
			AssertEquals("Item code should be readonly.", true, item.DescriptionInfo.ReadOnly);

			item = (CodeDescriptionBoolDisallowNewCodeReadOnlyValidation)GetNewBusinessObject();
			item.Code = "SEL";
			item.Description = (NoResString)"Custom Exchange Rate 02";
			item.Bool = true;

			AssertEquals("Item code should be readonly.", false, item.CodeInfo.ReadOnly);
			AssertEquals("Item code should be readonly.", false, item.DescriptionInfo.ReadOnly);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;
	}
}
