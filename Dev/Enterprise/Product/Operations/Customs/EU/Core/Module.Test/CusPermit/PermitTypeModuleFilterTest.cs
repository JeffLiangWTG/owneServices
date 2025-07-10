using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(Customs.Module.PermitTypeModuleFilter))]
	class PermitTypeModuleFilterTest : Customs.Module.Testing.PermitTypeModuleFilterTest
	{
		public void TestGetNewValidation_ReturnType()
		{
			var permitTypeModuleFilter = new PermitTypeModuleFilter("Permit Type", null, () => new CodeDescriptionPairList());
			AssertType<PermitTypeModuleFilterValidation>(permitTypeModuleFilter.Validation);
		}

		public void TestCheckPropertyMaxLength()
		{
			var permitTypeModuleFilter = new PermitTypeModuleFilter("Permit Type", null, () => new CodeDescriptionPairList());
			AssertEquals(PermitTypeModuleFilter.Schema.Property1MaxLength, permitTypeModuleFilter.GetPossiblyCustomPropertyMaxLength(nameof(PermitTypeModuleFilter.Property1)));
			AssertEquals(PermitTypeModuleFilter.Schema.Property2MaxLength, permitTypeModuleFilter.GetPossiblyCustomPropertyMaxLength(nameof(PermitTypeModuleFilter.Property2)));
			AssertEquals(PermitTypeModuleFilter.Schema.Property3MaxLength, permitTypeModuleFilter.GetPossiblyCustomPropertyMaxLength(nameof(PermitTypeModuleFilter.Property3)));
		}

		public void TestCheckPropertyReadOnly()
		{
			var permitTypeModuleFilter = new PermitTypeModuleFilter("Permit Type", null, () => new CodeDescriptionPairList());
			Assert("User should not be able to edit Type directly.", permitTypeModuleFilter.GetPossiblyCustomPropertyReadOnly(nameof(PermitTypeModuleFilter.Property1)));
			Assert("User should not be able to edit SubType directly.", permitTypeModuleFilter.GetPossiblyCustomPropertyReadOnly(nameof(PermitTypeModuleFilter.Property2)));
			Assert("User should be able to edit FullType directly.", !permitTypeModuleFilter.GetPossiblyCustomPropertyReadOnly(nameof(PermitTypeModuleFilter.Property3)));
		}

		public void TestGetProperty3()
		{
			var permitTypeModuleFilter = new PermitTypeModuleFilter("Permit Type", null, () => new CodeDescriptionPairList());
			permitTypeModuleFilter.Property1 = "3LLB";
			permitTypeModuleFilter.Property2 = "231";
			AssertEquals("3LLB231", permitTypeModuleFilter.Property3);
		}

		public void TestSetProperty3()
		{
			var permitTypeModuleFilter = new PermitTypeModuleFilter("Permit Type", null, () => new CodeDescriptionPairList());
			permitTypeModuleFilter.Property3 = "3LLB231";
			AssertEquals("3LLB", permitTypeModuleFilter.Property1);
			AssertEquals("231", permitTypeModuleFilter.Property2);
		}

		public void TestValidateProperty1()
		{
			var permitTypeModuleFilter = new PermitTypeModuleFilter("Permit Type", null, () => new CodeDescriptionPairList());
			permitTypeModuleFilter.Property1 = "!@#$";
			AssertNoNotifications("CPH_Type should not do any validation as it is readonly.", permitTypeModuleFilter.Property1Info);

			permitTypeModuleFilter.Property1 = "";
			AssertNoNotifications("CPH_Type should not do any validation as it is readonly.", permitTypeModuleFilter.Property1Info);
		}

		public void TestValidateProperty2()
		{
			var permitTypeModuleFilter = new PermitTypeModuleFilter("Permit Type", null, () => new CodeDescriptionPairList());
			permitTypeModuleFilter.Property2 = "!@#";
			AssertNoNotifications("CPH_SubTyp should not do any validation as it is readonly.", permitTypeModuleFilter.Property2Info);

			permitTypeModuleFilter.Property2 = "";
			AssertNoNotifications("CPH_SubTyp should not do any validation as it is readonly.", permitTypeModuleFilter.Property2Info);
		}
	}
}
