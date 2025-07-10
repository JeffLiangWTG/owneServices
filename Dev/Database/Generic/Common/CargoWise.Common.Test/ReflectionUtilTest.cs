using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	class ReflectionUtilTest : TestCase
	{
		public void TestGetTypeFromAssemblyFixupDotsBetweenNestedType()
		{
			AssertEquals("GetTypeFromAssemblyFixupDotsBetweenNestedType", typeof(ReflectionUtil.InternalClass.NestedPublic), ReflectionUtil.GetTypeFromAssemblyFixupDotsBetweenNestedType(typeof(ReflectionUtil).Assembly, typeof(ReflectionUtil.InternalClass.NestedPublic).FullName.Replace("+", ".")));
		}
	}
}
