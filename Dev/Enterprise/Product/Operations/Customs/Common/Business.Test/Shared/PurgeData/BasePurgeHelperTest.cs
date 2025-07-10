using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	abstract class BasePurgeHelperTest : TestCaseWithFactory
	{
		protected void SetupDummyData(DummyBusinessObject obj)
		{
			obj.Z0_AnotherDate = ZDateTime.BrettsBirthday;
			obj.Z0_AnotherNumber = 999;
			obj.Z0_AnotherDecimal = 300.05m;
			obj.Z0_Description = "BBBB";

			obj.Z0_Guid = ZGuid.BrettsGuid;
			obj.Z0_Bool = true;
			obj.Z0_Code = "AAAA";
			obj.Z0_SmallDateTime = ZDateTime.BrettsBirthday;
			obj.Z0_Number = 2017;
			obj.Z0_Short = 9;
			obj.Z0_Money = 15.08m;
		}

		[ExpectNoExceptions]
		protected void AssertDummyData(DummyBusinessObject obj, ZGuid guidValue, ZString codeValue, ZBool boolValue, ZShort shortValue, ZInt numberValue, ZString description, ZDecimal decimalValue, ZDateTime smallDateTime)
		{
			NUnit.Framework.Assert.That(obj.Z0_Description, Is.EqualTo(description), "obj.Z0_Description");

			NUnit.Framework.Assert.That(obj.Z0_AnotherDate, Is.EqualTo(ZDateTime.BrettsBirthday), "obj.Z0_AnotherDate");
			NUnit.Framework.Assert.That(obj.Z0_AnotherDecimal, Is.EqualTo(300.05m).Using(CustomComparers.TypeComparison), "obj.Z0_AnotherDecimal");
			NUnit.Framework.Assert.That(obj.Z0_AnotherNumber, Is.EqualTo(999).Using(CustomComparers.TypeComparison), "obj.Z0_AnotherNumber");

			NUnit.Framework.Assert.That(obj.Z0_Guid, Is.EqualTo(guidValue), "obj.Z0_Guid");
			NUnit.Framework.Assert.That(obj.Z0_Code, Is.EqualTo(codeValue), "obj.Z0_Code");
			NUnit.Framework.Assert.That(obj.Z0_Bool, Is.EqualTo(boolValue), "obj.Z0_Bool");
			NUnit.Framework.Assert.That(obj.Z0_Short, Is.EqualTo(shortValue), "obj.Z0_Short");
			NUnit.Framework.Assert.That(obj.Z0_Number, Is.EqualTo(numberValue), "obj.Z0_Number");
			NUnit.Framework.Assert.That(obj.Z0_Money, Is.EqualTo(decimalValue), "obj.Z0_Money");
			NUnit.Framework.Assert.That(obj.Z0_SmallDateTime, Is.EqualTo(smallDateTime), "obj.Z0_SmallDateTime");
		}
	}
}
