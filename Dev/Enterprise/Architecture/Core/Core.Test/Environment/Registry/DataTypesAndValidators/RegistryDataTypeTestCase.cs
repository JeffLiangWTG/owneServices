using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestsSubclassesOf(typeof(IRegistryDataType))]
	public abstract class RegistryDataTypeTestCase<T> : TransactionedTestCase where T : IRegistryDataType
	{
		public void TestDefaultWithoutExplicitDefault()
		{
			IRegistryItem item = GetNewRegistryItemWithDefaultDefaultValue();
			object defaultValue = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertValuesEqualForCheckingDefault("Default value without explicit default.", DataType.DefaultValue, defaultValue);
		}

		public void TestDefaultWithExplicitDefault()
		{
			foreach (ValidSampleAndBinaryValueInDB sampleDefault in GetValidSamples())
			{
				IRegistryItem item = GetNewRegistryItem(sampleDefault.ValidSample);
				object newValue = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				AssertValuesEqualForCheckingDefault("Default value with explicit default.", sampleDefault.ValidSample, newValue);
			}
		}

		public void TestSerialiseDeserialise()
		{
			foreach (ValidSampleAndBinaryValueInDB sample in GetValidSamples())
			{
				IRegistryItem item = GetNewRegistryItemWithDefaultDefaultValue();
				byte[] bytes = item.DataType.Serialise(sample.ValidSample);
				object deserialisedValue = item.DataType.Deserialise(bytes);
				AssertValuesEqual("Value deserialised correctly.", sample.ValidSample, deserialisedValue);
			}
		}

		[DeveloperOnlyTest]
		public void TestSerialiseValidSamples()
		{
			foreach (var sample in GetValidSamples())
			{
				var item = GetNewRegistryItemWithDefaultDefaultValue();
				var bytes = item.DataType.Serialise(sample.ValidSample);
				AssertSequencesEqual($"Value {sample.ValidSample} serialised correctly.", sample.BinaryValue, bytes);
			}
		}

		public virtual void TestGetGuidValue()
		{
			IRegistryItem item = GetNewRegistryItemWithDefaultDefaultValue();
			AssertEquals("should be an empty guid", Guid.Empty, item.DataType.GetGuidValue(new Guid()));
		}

		public virtual void TestISDefaultImmutable()
		{
			IRegistryItem item = GetNewRegistryItemWithDefaultDefaultValue();

			if (item.DataType.IsDefaultValueImmutable)
			{
				var defaultValue = item.DataType.DefaultValue;
				var type = defaultValue.GetType();
				Assert(string.Format(CultureInfo.InvariantCulture, "Type must be immutable but was {0}", type.FullName),
					type.GetCustomAttribute<ImmutableAttribute>() != null
					|| type.Namespace == "System.Collections.Immutable"
					|| defaultValue is string
					|| defaultValue is int
					|| defaultValue is decimal
					|| defaultValue is bool
					|| defaultValue is Guid
					|| defaultValue is DateTime);
			}
			else
			{
				Assert(true);
			}
		}

		public virtual void TestGetSetValidValues()
		{
			int index = 0;

			var samples = GetValidSamples();

			foreach (ValidSampleAndBinaryValueInDB validSample in samples)
			{
				IRegistryItem item = GetNewRegistryItemWithDefaultDefaultValue(++index);
				item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, validSample.ValidSample);

				object readValue = item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				AssertValuesEqual("New value set.", validSample.ValidSample, readValue);
			}
		}

		[ExpectNoExceptions]
		public virtual void TestValuesAreEqual()
		{
			var dataType = GetNewDataType();
			var samples = GetValidSamples();
			var grouped = samples
				.Zip(samples, (a, b) => Tuple.Create(a.ValidSample, b.ValidSample))
				.ToList();

			var nonNull = grouped.Where(tuple => tuple.Item1 != null).Take(2).ToList();

			if (RegistryDataTypeTestHelper.IsInBaseLine(GetType()))
			{
				var nonNullValidValue = nonNull.Single();// If you add more valid examples, remove the class from the baseline rather than changing this to .First()
				AssertValuesAreEqualForBaseLine(dataType, nonNullValidValue.Item1, nonNullValidValue.Item2);
			}
			else
			{
				Assert("PRE: There must be at least two valid, non null values. Ensure that your GetValidSamples() override returns at least TWO pairs of object/byte. (" + GetType().Name + ")", nonNull.Count > 1);
				AssertValuesAreEqualWorks(dataType, nonNull[0], nonNull[1]);
			}
		}

		static void AssertValuesAreEqualWorks(T dataType, Tuple<object, object> first, Tuple<object, object> second)
		{
			Assert("Same items should be same", dataType.ValuesAreEqual(first.Item1, first.Item2));
			Assert("Items should be equal to their clone", dataType.ValuesAreEqual(first.Item1, dataType.CloneValue(first.Item2)));
			Assert("Different items should be different", !dataType.ValuesAreEqual(first.Item1, second.Item1));

			if (dataType.AllowNull)
			{
				Assert("Null should be considered - a is null", !dataType.ValuesAreEqual(null, first.Item1));
				Assert("Null should be considered - b is null", !dataType.ValuesAreEqual(first.Item1, null));
				Assert("Null should be considered - both are null", dataType.ValuesAreEqual(null, null));
			}
		}

		public void AssertValuesAreEqualForBaseLine(T dataType, object nonNullValidValue, object anotherReferenceToTheNonNullValue)
		{
			Assert("Value should be equal to itself", dataType.ValuesAreEqual(nonNullValidValue, nonNullValidValue));
			Assert("Value should be equal another reference of itself", dataType.ValuesAreEqual(nonNullValidValue, anotherReferenceToTheNonNullValue));

			var cloneOfValue = dataType.CloneValue(nonNullValidValue);
			Assert("Value should be equal to it's clone", dataType.ValuesAreEqual(nonNullValidValue, cloneOfValue));

			if (dataType.AllowNull)
			{
				Assert("Null should be considered - a is null", !dataType.ValuesAreEqual(null, nonNullValidValue));
				Assert("Null should be considered - b is null", !dataType.ValuesAreEqual(nonNullValidValue, null));
				Assert("Null should be considered - both are null", dataType.ValuesAreEqual(null, null));
			}
		}

		public void TestValuesAreEqualWhenOneIsNull()
		{
			var dataType = DataType;
			if (dataType.AllowNull)
			{
				Assert("Should be seen as equal", dataType.ValuesAreEqual(null, GetNullRepresentation()));
				Assert("Should be seen as equal", dataType.ValuesAreEqual(GetNullRepresentation(), null));
				Assert("Should be seen as equal", dataType.ValuesAreEqual(GetNullRepresentation(), GetNullRepresentation()));
				Assert("Should be seen as equal", dataType.ValuesAreEqual(null, null));
			}
			else
			{
				Assert(true); //No nulls to test
			}
		}

		public void TestNullRepresentation()
		{
			var dataType = GetNewDataType();
			Assert("Should be seen as null representation", !dataType.AllowNull || dataType.IsNullDataRepresentation(GetNullRepresentation()));
		}

		public void TestGetNullRepresentationDoesntReturnTheSameReference()
		{
			var dataType = GetNewDataType();
			if (dataType.AllowNull)
			{
				var firstReference = GetNullRepresentation();
				var secondReference = GetNullRepresentation();
				Assert("We dont want to be doing reference comparisons, so please provide a copy of your null representation", !ReferenceEquals(firstReference, secondReference));
			}
			else
			{
				Assert("We dont have nulls, so there is nothing to test", true);
			}
		}

		protected virtual object GetNullRepresentation()
		{
			if (GetNewDataType().AllowNull)
			{
				throw new NotImplementedException("You have IsNull = true, please provide a null representation for testing");
			}
			else
			{
				throw new InvalidOperationException("Broken test - Can't get a null representation for a datatype that doesnt allow nulls");
			}
		}

		[ExpectNoExceptions]
		public void TestSetInvalidValuesThrowsValidationException()
		{
			foreach (object invalidSample in GetInvalidSamples())
			{
				IRegistryItem item = GetNewRegistryItemWithDefaultDefaultValue();
				try
				{
					item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, invalidSample);
					Fail("Expected validation exception for value '" + invalidSample.ToString() + "'.");
				}
				catch (RegistryValidationException)
				{
				}
			}
		}

		public void TestNoAdditionalMembersOnTypedRegistryItem()
		{
			var registryItemType = GetNewRegistryItemWithDefaultDefaultValue().GetType();
			var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;
			var registryItemInterfaceProperties = new HashSet<string>(
						 GetInterfacePropertiesIncludingBases(typeof(IRegistryItemInternals))
						.Concat(GetInterfacePropertiesIncludingBases(typeof(IMultilingualRegistryItem)))
						.Select(property => property.Name)
			);

			string failureMessage =
				"You should only provide strongly-typed constructors in a strongly-typed RegistryItem. If you need additional " +
				"operations on the item, put them on the implementation of " + nameof(IRegistryDataType) + ". The issue is " +
				"that if people want to sub-class a RegistryItem to provide additional functionality, they may not sub-class the " +
				"strongly typed RegistryItem (as they would have to sub-class ALL strongly typed RegistryItems and provide the " +
				"additional functionality in many places).";

			foreach (PropertyInfo info in registryItemType.GetProperties(bindingFlags))
			{
				if (!registryItemInterfaceProperties.Contains(info.Name) && info.DeclaringType != typeof(RegistryItemWrapper))
				{
					Fail(failureMessage + " (" + info.Name + ")");
				}
			}

			foreach (FieldInfo info in registryItemType.GetFields(bindingFlags))
			{
				if (!registryItemInterfaceProperties.Contains(info.Name) && info.DeclaringType != typeof(RegistryItemWrapper))
				{
					Fail(failureMessage + " (" + info.Name + ")");
				}
			}

			Assert(true);
		}

		public void TestIsValidatedOnSetEvenIfEqualDefaultValue()
		{
			AssertEquals("IsValidatedOnSetEvenIfEqualDefaultValue", IsValidatedOnSetEvenIfEqualDefaultValue, DataType.IsValidatedOnSetEvenIfEqualDefaultValue);
		}

		public void TestCloneValue()
		{
			ValidSampleAndBinaryValueInDB[] validSamples = GetValidSamples();
			Assert("There should be at least one valid sample.", validSamples.Length > 0);

			foreach (ValidSampleAndBinaryValueInDB validSample in validSamples)
			{
				if (validSample.ValidSample != null)
				{
					object originalValue = validSample.ValidSample;
					object clonedValue = DataType.CloneValue(originalValue);
					AssertValuesEqual("CloneValue()", originalValue, clonedValue);

					bool canCloneBeSameInstance = originalValue.GetType().IsValueType || originalValue is string || originalValue.GetType().GetCustomAttribute<ImmutableAttribute>() != null;

					if (!canCloneBeSameInstance)
					{
						AssertEquals("Clone should be a different instance if using a reference type.", false, ReferenceEquals(originalValue, clonedValue));
					}
				}
			}

			AssertNull("CloneValue(null)", DataType.CloneValue(null));
		}

		public void TestSerialisedValueInDBDoesntChangeForExistingClients()
		{
			foreach (ValidSampleAndBinaryValueInDB validSample in GetValidSamples())
			{
				IRegistryItem item = GetNewRegistryItemWithDefaultDefaultValue();
				try
				{
					object readSample = item.DataType.Deserialise(validSample.BinaryValue);
					// Hint: Check that what you supply in GetNewRegistryItemWithDefaultDefaultValue() corresponds with the byte array you give in GetValidSamples(), e.g. check same number of samples.
					AssertValuesEqual("Binary value incorrect, this may cause problems for existing clients with real data in their DB.", validSample.ValidSample, readSample);
				}
				catch
				{
					byte[] actualBytes = DataType.Serialise(validSample.ValidSample);

					string byteComparisonMessage =
						"<br/>" +
						"Binary value incorrect:<br/>" +
						"<br/>" +
						"Expected:<BR>" +
						"<div style=\"overflow: scroll; width: 95%; height: 200px; background-color: rgb(230,245,230);\">" +
						GetHtmlSafeString(validSample.BinaryValue) +
						"<br/><br/>" +
						GetBinaryValueAsCSharpCode(validSample.BinaryValue) +
						"</div>" +
						"<br/>Actual:<br/>" +
						"<div style=\"overflow: scroll; width: 95%; height: 200px; background-color: rgb(255,230,230);\">" +
						GetHtmlSafeString(actualBytes) +
						"<br/><br/>" +
						GetBinaryValueAsCSharpCode(actualBytes) +
						"</div>" +
						"<br/><br/><b>" +
						"DONT change the unit test to reflect the expected value unless you are SURE you didn't change the " +
						"binary serialisation format of the data type otherwise existing clients may be broken." +
						"<br/>" +
						"Look at the code for this test in RegistryDataType.cs for instructions on how to actually get the byte[] value to paste into your GetValidSamples() method." +
						"</b><br/>" +
						"";

					/* To get the byte[] value to paste into your GetValidSamples() method, uncomment the following line, put a breakpoint on it, and copy the string value that it produces.
					   You might also find it easier to comment the Globals.Message.Show and throw lines so that you can see each value in the loop. */

					var bytes = string.Join(",", actualBytes.Select(x => x.ToString()));

					//Alternatively, to be able to read what you test, use the xmlString instead of byte[] for the ValidSampleAndBinaryValueInDB 2nd parameter
					var xmlString = new UnicodeEncoding(false, false).GetString(actualBytes);

					Globals.Message.ShowDeveloperErrorAlways(byteComparisonMessage, "");
					throw;
				}
			}
		}

		string GetHtmlSafeString(byte[] data)
		{
			StringBuilder builder = new StringBuilder(Encoding.Unicode.GetString(data));
			builder.Replace("><", ">\n<");
			builder.Replace("&", "&amp;");
			builder.Replace("<", "&lt;");
			builder.Replace(">", "&gt;");
			builder.Replace("\n", "<br/>");
			return builder.ToString();
		}

		public void TestCannotAllowNullIfIsValueTypeOrArray()
		{
			if (DataType.DataType.IsValueType || (DataType.DataType.IsArray) && (DataType.DataType != typeof(byte[])))
			{
				AssertEquals("AllowNull", false, DataType.AllowNull);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDefaultEditorInfo()
		{
			if (DataType.HasDefaultEditorInfo)
			{
				AssertNotNull("DefaultEditorInfo should not be null.", DataType.DefaultEditorInfo);
			}
			else
			{
				Assert(true);
			}
		}

		#region Implementation

		protected T DataType
		{
			get
			{
				if (dataType == null)
				{
					dataType = GetNewDataType();
				}
				return dataType;
			}
		}

		protected virtual object[] GetInvalidSamples()
		{
			return Array.Empty<object>();
		}

		protected virtual void AssertValuesEqual(string message, object lhs, object rhs)
		{
			AssertEquals(message, lhs, rhs);
		}

		protected virtual void AssertValuesEqualForCheckingDefault(string message, object lhs, object rhs)
		{
			AssertValuesEqual(message, lhs, rhs);
		}

		protected virtual bool IsValidatedOnSetEvenIfEqualDefaultValue
		{
			get { return false; }
		}

		IRegistryItem GetNewRegistryItemWithDefaultDefaultValue(int index = 0)
		{
			string name = "TestItem";

			if (index > 0)
			{
				name = name + index;
			}

			return new RegistryItemImpl(name, (NoResString)"TestCategory/TestSubCat", (NoResString)"Caption", (NoResString)"Hint", DataType, RegistryStorageFlags.All);
		}

		protected IRegistryItem GetNewRegistryItem(object defaultValue)
		{
			return new RegistryItemImpl("TestItem", (NoResString)"TestCategory/TestSubCat", (NoResString)"Caption", (NoResString)"Hint", DataType, RegistryStorageFlags.All, defaultValue);
		}

		PropertyInfo[] GetInterfacePropertiesIncludingBases(Type type)
		{
			List<PropertyInfo> result = new List<PropertyInfo>();
			result.AddRange(type.GetProperties());
			foreach (Type interfaceType in type.GetInterfaces())
			{
				result.AddRange(GetInterfacePropertiesIncludingBases(interfaceType));
			}
			return result.ToArray();
		}

		string GetBinaryValueAsCSharpCode(byte[] bytes)
		{
			string result = "";
			result += "new byte[]<BR>";
			result += "{<BR>";

			string byteString = "    ";
			int current = 0;
			foreach (byte aByte in bytes)
			{
				if (!string.IsNullOrWhiteSpace(byteString))
				{
					byteString += ",";
				}

				if (current++ > 64)
				{
					byteString += "<BR>    ";
					current = 0;
				}
				byteString += (int)aByte;
			}

			result += "    " + byteString + "<BR>";
			result += "};<BR>";
			return result;
		}

		T dataType;
		protected abstract T GetNewDataType();
		protected abstract ValidSampleAndBinaryValueInDB[] GetValidSamples();

		#endregion
	}
}
