using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OverrideImmuneCodeDescriptionBool))]
	class OverrideImmuneCodeDescriptionBoolTest : CodeDescriptionBoolTest
	{
		public override void TestReadOnlyStates()
		{
			var result = (OverrideImmuneCodeDescriptionBool)GetNewBusinessObject();

			result.SystemDefined = false;
			AssertEquals("CodeInfo.ReadOnly", false, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", false, result.DescriptionInfo.ReadOnly);
			AssertEquals("BoolInfo.ReadOnly", false, result.BoolInfo.ReadOnly);

			result.SystemDefined = true;
			AssertEquals("CodeInfo.ReadOnly", true, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", true, result.DescriptionInfo.ReadOnly);
			AssertEquals("BoolInfo.ReadOnly", true, result.BoolInfo.ReadOnly);

			result.ReadOnly = true;
			result.ReadOnly = false;
			AssertEquals("CodeInfo.ReadOnly", true, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", true, result.DescriptionInfo.ReadOnly);
			AssertEquals("BoolInfo.ReadOnly", true, result.BoolInfo.ReadOnly);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (OverrideImmuneCodeDescriptionBool)GetNewBusinessObject();

			result.CodeMaxLength = 5;
			result.Code = "TSCOD";
			result.Description = (NoResString)"DescriptionA";
			result.SystemDefined = true;
			result.CodeList = new CodeDescriptionPairList();

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OverrideImmuneCodeDescriptionBool();
		}

		#endregion
	}
}
