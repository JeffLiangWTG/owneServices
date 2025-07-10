using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Testing
{
	public class StringEnumTest : TestCase
	{
		/// <summary>
		/// Tests GetStringValue (Static implementation)
		/// </summary>
		public void TestStaticGetStringValue()
		{
			//Expect to retrieve a string value
			AssertEquals("Third Value", EnumUtil.GetDescription(EnumWithStrings.Fox));
			//No string value to retrieve
			AssertNull(EnumUtil.GetDescription(EnumWithoutStrings.Seashells));
			//String values exist but not for this enum value
			AssertNull(EnumUtil.GetDescription(EnumPartialStrings.Frost));
		}

		/// <summary>
		/// Tests GetStringValue caching (Static implementation)
		/// </summary>
		public void TestStaticGetStringValueCaching()
		{
			//Expect to retrieve a string value (and cache this value)
			AssertEquals("Third Value", EnumUtil.GetDescription(EnumWithStrings.Fox));

			//Expect to retrieve a different value (as this is from a different enum)
			AssertEquals("3rd Value", EnumUtil.GetDescription(IdenticalEnumWithDifferentStrings.Fox));

			//Expect to retrieve both values again (cached)
			AssertEquals("Third Value", EnumUtil.GetDescription(EnumWithStrings.Fox));
			AssertEquals("3rd Value", EnumUtil.GetDescription(IdenticalEnumWithDifferentStrings.Fox));
		}

		/// <summary>
		/// Parse a string value to retrieve an enum value
		/// </summary>
		public void TestStaticParse()
		{
			//Case Sensitive (not found)
			AssertNull(EnumUtil.Parse(typeof(EnumPartialStrings), "jacK be nImbLe"));
			//Case Sensitive (found)
			AssertEquals(EnumPartialStrings.Jack, EnumUtil.Parse(typeof(EnumPartialStrings), "Jack be nimble"));
			//Case insensitive (found)
			AssertEquals(EnumPartialStrings.Jack, EnumUtil.Parse(typeof(EnumPartialStrings), "jacK be nImbLe", true));
		}

		/// <summary>
		/// Try a non-enum type to generate an ArgumentException
		/// </summary>
		/// <remarks>Expect exception as supplied type must be an Enum.  Type is System.Decimal</remarks>
		[ExpectException(typeof(ArgumentException))]
		public void TestParseExpectArgumentException()
		{
			EnumUtil.Parse(typeof(decimal), "spoof");
		}

		/// <summary>
		/// Test whether a given string is defined within the given enum
		/// </summary>
		public void TestStaticIsStringDefined()
		{
			Assert(!EnumUtil.IsStringDefined(typeof(EnumWithStrings), "My fair Lady"));
			Assert(EnumUtil.IsStringDefined(typeof(EnumPartialStrings), "jack BE NIMble", true));
		}

		#region Instance Tests

		/// <summary>
		/// Create new instance from non enum type - expect argument exception
		/// </summary>
		/// <remarks>Expect exception as supplied type must be an Enum.  Type is System.String</remarks>
		[ExpectException(typeof(ArgumentException))]
		public void TestInstanceConstructorExpectArgumentException()
		{
			new EnumUtil(typeof(string));
		}

		/// <summary>
		/// Create new instance and return enum value from the given string value
		/// </summary>
		public void TestInstanceGetStringValue()
		{
			EnumUtil stringEnum = new EnumUtil(typeof(EnumWithStrings));
			AssertEquals("Fourth Value", stringEnum.GetStringValue("Lazy"));
			//Expect null as this value doesn't exist
			AssertNull(stringEnum.GetStringValue("clearly not there"));
		}

		/// <summary>
		/// Test retrieving an array of string values from a supplied enum
		/// </summary>
		public void TestInstanceGetStringValues()
		{
			EnumUtil stringEnum = new EnumUtil(typeof(EnumWithoutStrings));
			AssertEquals(0, stringEnum.GetStringValues().Length);

			stringEnum = new EnumUtil(typeof(EnumPartialStrings));
			AssertEquals(1, stringEnum.GetStringValues().Length);
			AssertEquals("Jack be nimble", stringEnum.GetStringValues().GetValue(0).ToString());
		}

		/// <summary>
		/// Test whether the given string is defined using instance methods
		/// </summary>
		public void TestInstanceIsStringDefined()
		{
			EnumUtil stringEnum = new EnumUtil(typeof(EnumWithStrings));
			Assert(!stringEnum.IsStringDefined("Something that's not there"));
			Assert(!stringEnum.IsStringDefined("first value"));
			Assert(stringEnum.IsStringDefined("First Value"));
		}

		/// <summary>
		/// Test basic property get access
		/// </summary>
		public void TestInstancePropertyEnum()
		{
			EnumUtil stringEnum = new EnumUtil(typeof(EnumWithoutStrings));
			Assert(stringEnum.EnumType.IsEnum);
		}

		/// <summary>
		/// Test all values return in a comma delimited string.
		/// </summary>
		public void TestValuesAsString()
		{
			string expectedValues = "First Value, Second Value, Third Value, Fourth Value, Fifth Value";
			AssertEquals(expectedValues, EnumUtil.GetDescriptions(typeof(EnumWithStrings)));

			ArrayList values = new ArrayList();
			foreach (FieldInfo fieldInfo in typeof(EnumWithStrings).GetFields().Where(f => f.IsStatic).OrderByDescending(f => f.MetadataToken))
			{
				EnumDescriptionAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(EnumDescriptionAttribute), false) as EnumDescriptionAttribute[];
				if (attributes.Length > 0)
				{
					values.Add(attributes[0].Value);
				}
			}
			AssertEquals("Descending order should work as well", "Fifth Value, Fourth Value, Third Value, Second Value, First Value", string.Join(", ", values.ToArray()));
		}

		#endregion

		#region Enums

		enum EnumWithStrings
		{
			[EnumDescription("First Value")]
			Quick,
			[EnumDescription("Second Value")]
			Brown = 1,
			[EnumDescription("Third Value")]
			Fox = 8,
			[EnumDescription("Fourth Value")]
			Lazy,
			[EnumDescription("Fifth Value")]
			Dog = 7
		}

		enum IdenticalEnumWithDifferentStrings
		{
			[EnumDescription("1st Value")]
			Quick,
			[EnumDescription("2nd Value")]
			Brown = 1,
			[EnumDescription("3rd Value")]
			Fox = 8,
			[EnumDescription("4th Value")]
			Lazy,
			[EnumDescription("5th Value")]
			Dog = 7
		}

		enum EnumWithoutStrings
		{
			She = 0,
			Sells = 1,
			Seashells = 4,
			Seashore = 5
		}

		enum EnumPartialStrings
		{
			Jumping,
			[EnumDescription("Jack be nimble")]
			Jack,
			Frost
		}

		#endregion
	}
}
