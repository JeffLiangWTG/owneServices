using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionForRegistryBusinessObjectTest))]
	public class CodeDescriptionForRegistryBusinessObjectTestCase : RegistryBusinessObjectTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CodeDescriptionForRegistryBusinessObjectTest();
		}

		public override void TestMaxDescriptionLength()
		{
			var obj = (CodeDescriptionForRegistryBusinessObjectTest)GetNewBusinessObject();
			var result = obj.DescriptionInfo.MaxLength;
			AssertEquals(256, result);
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			var cloneObject = clone as Business.CodeDescription<ZByte>;
			AssertEquals("Code", "TSCOD", cloneObject.Code);
			AssertEquals("Description", "DescriptionA", cloneObject.Description);
			AssertEquals("CodeMaxLength", 5, cloneObject.CodeMaxLength);
			Assert("SystemDefined", cloneObject.SystemDefined);
			AssertNotNull("CodeList", cloneObject.CodeList);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (CodeDescriptionForRegistryBusinessObjectTest)GetNewBusinessObject();

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

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
