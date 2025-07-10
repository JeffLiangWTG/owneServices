using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OutturnRespPartyIDCodeDescriptionPairListRegistryDataType))]
	sealed class OutturnRespPartyIDCodeDescriptionPairListRegistryDataTypeTest : CodeDescriptionPairListRegistryDataTypeTest
	{
		public void TestABNandCCPValidation()
		{
			var dataType = (OutturnRespPartyIDCodeDescriptionPairListRegistryDataTypeForTest)GetNewDataType();
			CodeDescriptionPairList list = new CodeDescriptionPairList();

			list.AddPair("B123B", "21003980131");
			AssertExceptionThrown("The entered ABN is not valid.\n\nAn ABN is a specially provided number built with a checksum to ensure its validity. An error on this field indicates that the given number is DEFINITELY INVALID.",
				typeof(RegistryValidationException), delegate
				{ dataType.ValidateCoreForTesting(null, list, Guid.Empty, Guid.Empty, Guid.Empty); });

			list.Clear();
			list.AddPair("B123B", "21003980130");
			AssertNoExceptionThrown(delegate
			{ dataType.ValidateCoreForTesting(null, list, Guid.Empty, Guid.Empty, Guid.Empty); });

			list.Clear();
			list.AddPair("B123B", "");
			AssertExceptionThrown("ABN cannot be empty.", typeof(RegistryValidationException), delegate
			{ dataType.ValidateCoreForTesting(null, list, Guid.Empty, Guid.Empty, Guid.Empty); });

			list.Clear();
			list.AddPair("BLAH", "21003980130");
			AssertExceptionThrown("Invalid establishment code format. The format could be any of NNNNA, ANNNA or AANNA ('A' means alphabetical and 'N' indicates numeric).",
				typeof(RegistryValidationException), delegate
				{ dataType.ValidateCoreForTesting(null, list, Guid.Empty, Guid.Empty, Guid.Empty); });

			list.Clear();
			list.AddPair("B123F", "21003980130");
			AssertExceptionThrown("Invalid Establishment Code, expected checksum is: A",
				typeof(RegistryValidationException), delegate
				{ dataType.ValidateCoreForTesting(null, list, Guid.Empty, Guid.Empty, Guid.Empty); });

			list.Clear();
			list.AddPair("B123B", "21003980130");
			AssertNoExceptionThrown(delegate
			{ dataType.ValidateCoreForTesting(null, list, Guid.Empty, Guid.Empty, Guid.Empty); });
		}

		protected override CodeDescriptionPairListRegistryDataType GetNewDataType()
		{
			return new OutturnRespPartyIDCodeDescriptionPairListRegistryDataTypeForTest(5);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			CodeDescriptionPairList list1 = new CodeDescriptionPairList();
			CodeDescriptionPairList list2 = new CodeDescriptionPairList();
			list2.AddPair("B123B", "21003980130");

			return new ValidSampleAndBinaryValueInDB[]
			{
					new ValidSampleAndBinaryValueInDB(list1, list1.ToXMLByteArray()),
					new ValidSampleAndBinaryValueInDB(list2, list2.ToXMLByteArray())
			};
		}
	}
}
