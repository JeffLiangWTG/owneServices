using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.CH.Business.Testing;

public static class GenAddOnTestHelper
{
	public static void AssertGetterSetter(ZPropertyInfo propertyInfo)
	{
		GenAddOnHelper.FindOrMakeNewAddOn(propertyInfo.Name, propertyInfo.BizObj, out var column);
		column.XA_Data = "X";
		AssertEquals($"Property {propertyInfo.Name} GenAddOnColumn Getter", "X", propertyInfo.Value);
		propertyInfo.SetValueFromString("Y");
		AssertEquals($"Property {propertyInfo.Name} GenAddOnColumn Setter", "Y", column.XA_Data);
	}
}
