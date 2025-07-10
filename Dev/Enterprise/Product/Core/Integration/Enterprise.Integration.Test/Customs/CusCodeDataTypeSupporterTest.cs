using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Integration.Testing
{
	public static class CusCodeDataTypeSupporterTest
	{
		public static void AssertType(this Customs.ICusCodeDataTypeSupporter supporter, Type expectedType, ZString code)
		{
			Argument.NotNull(supporter, nameof(supporter));
			Type type = null;
			supporter.GetCusCodeDataTypes()?.TryGetValue(code, out type);
			NUnit.Framework.Assertion.AssertEquals(code, expectedType, type);
		}
	}
}
