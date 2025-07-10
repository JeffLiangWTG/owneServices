using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	public static class BusinessObjectTestHelper
	{
		public static void AssertHumanReadableNameAndMaxLength(this ZPropertyInfo info, string humanReadableName = "", int maxLength = -1)
		{
			if (!string.IsNullOrWhiteSpace(humanReadableName))
			{
				Assertion.AssertEquals(humanReadableName, info.HumanReadableName);
			}

			if (maxLength > 0)
			{
				Assertion.AssertEquals(maxLength, info.MaxLength);
			}
		}
	}
}
