#if DEBUG
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class ReadOnlyMemberAttributeTests : TestCase
	{
		public void TestPropertyName()
		{
			const string expected = "propertyName";
			var typeInfo = typeof(ReadOnlyMemberAttribute).GetTypeInfo();
			var actual = typeInfo.DeclaredConstructors.First().GetParameters()[0].Name;

			AssertEquals(expected, actual);
		}

		public void TestIfNullParameter()
		{
			AssertExceptionThrown<ArgumentNullException>(SetParameterNull);
		}

		void SetParameterNull()
		{
			_ = new ReadOnlyMemberAttribute(null);
		}
	}
}

#endif
