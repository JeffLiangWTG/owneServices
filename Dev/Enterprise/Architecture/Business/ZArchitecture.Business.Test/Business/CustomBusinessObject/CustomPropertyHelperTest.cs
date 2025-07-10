using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CustomPropertyHelperTest : TestCase
	{
		public void TestCustomPropertyHelperGeneratePropertyIdentifierSimpleCase()
		{
			var identifier = CustomPropertyHelper.GeneratePropertyIdentifier("Some Name", typeof(ZString));
			CustomPropertyHelper.ExtractPropertyNameAndTypeFromIdentifier(identifier, out var reversed);
			Assert("Some Name".Equals(reversed.PropertyName, StringComparison.OrdinalIgnoreCase));
			AssertEquals(nameof(ZString), reversed.TypeName);
		}

		public void TestCustomPropertyHelperGeneratePropertyIdentifierWithDot()
		{
			var identifier = CustomPropertyHelper.GeneratePropertyIdentifier("Some.Name", typeof(ZInt));
			CustomPropertyHelper.ExtractPropertyNameAndTypeFromIdentifier(identifier, out var reversed);
			Assert("Some.Name".Equals(reversed.PropertyName, StringComparison.OrdinalIgnoreCase));
			AssertEquals(nameof(ZInt), reversed.TypeName);
		}

		public void TestCustomPropertyHelperGeneratePropertyIdentifierWithPlus()
		{
			var identifier = CustomPropertyHelper.GeneratePropertyIdentifier("Some+Name", typeof(ZInt));
			CustomPropertyHelper.ExtractPropertyNameAndTypeFromIdentifier(identifier, out var reversed);
			Assert("Some+Name".Equals(reversed.PropertyName, StringComparison.OrdinalIgnoreCase));
			AssertEquals(nameof(ZInt), reversed.TypeName);
		}

		public void TestCustomPropertyHelperGeneratePropertyIdentifierWith_()
		{
			var identifier = CustomPropertyHelper.GeneratePropertyIdentifier("Some_Name", typeof(ZInt));
			CustomPropertyHelper.ExtractPropertyNameAndTypeFromIdentifier(identifier, out var reversed);
			Assert("Some_Name".Equals(reversed.PropertyName, StringComparison.OrdinalIgnoreCase));
			AssertEquals(nameof(ZInt), reversed.TypeName);
		}

		public void TestCustomPropertyHelperGeneratePropertyIdentifierWithEscapeChar()
		{
			var identifier = CustomPropertyHelper.GeneratePropertyIdentifier("Some`Name", typeof(ZInt));
			CustomPropertyHelper.ExtractPropertyNameAndTypeFromIdentifier(identifier, out var reversed);
			Assert("Some`Name".Equals(reversed.PropertyName, StringComparison.OrdinalIgnoreCase));
			AssertEquals(nameof(ZInt), reversed.TypeName);

			identifier = CustomPropertyHelper.GeneratePropertyIdentifier("Some``Name", typeof(ZInt));
			CustomPropertyHelper.ExtractPropertyNameAndTypeFromIdentifier(identifier, out reversed);
			Assert("Some``Name".Equals(reversed.PropertyName, StringComparison.OrdinalIgnoreCase));
			AssertEquals(nameof(ZInt), reversed.TypeName);

			identifier = CustomPropertyHelper.GeneratePropertyIdentifier("Some```Name", typeof(ZInt));
			CustomPropertyHelper.ExtractPropertyNameAndTypeFromIdentifier(identifier, out reversed);
			Assert("Some```Name".Equals(reversed.PropertyName, StringComparison.OrdinalIgnoreCase));
			AssertEquals(nameof(ZInt), reversed.TypeName);

			identifier = CustomPropertyHelper.GeneratePropertyIdentifier("Some````Name", typeof(ZInt));
			CustomPropertyHelper.ExtractPropertyNameAndTypeFromIdentifier(identifier, out reversed);
			Assert("Some````Name".Equals(reversed.PropertyName, StringComparison.OrdinalIgnoreCase));
			AssertEquals(nameof(ZInt), reversed.TypeName);
		}

		public void TestCustomPropertyHelperGeneratePropertyIdentifierWithAllCP1252()
		{
			var chars = new List<byte>();
			for (char i = (char)0; i <= (char)255; ++i)
			{
				chars.Add((byte)i);
			}

			var allTheChars = Encoding.GetEncoding(1252).GetString(chars.ToArray());

			var identifier = CustomPropertyHelper.GeneratePropertyIdentifier(allTheChars, typeof(ZInt));
			CustomPropertyHelper.ExtractPropertyNameAndTypeFromIdentifier(identifier, out var reversed);
			Assert(allTheChars.Equals(reversed.PropertyName, StringComparison.OrdinalIgnoreCase));
			AssertEquals(nameof(ZInt), reversed.TypeName);
		}
	}
}
