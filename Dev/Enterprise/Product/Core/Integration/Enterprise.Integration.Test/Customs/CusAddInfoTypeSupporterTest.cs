using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Integration.Testing
{
	public static class CusAddInfoTypeSupporterTest
	{
		public static void AssertType(this Customs.ICusAddInfoTypeSupporter supporter, Type expectedType, ZString code)
		{
			Argument.NotNull(supporter, nameof(supporter));
			Type type = null;
			supporter.GetCusAddInfoTypes()?.TryGetValue(code, out type);
			NUnit.Framework.Assertion.AssertEquals(code, expectedType, type);
		}
	}
}
