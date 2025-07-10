using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Registry.Business.Testing
{
	[TestedType(typeof(NettingCycleListRegistryDataType))]
	internal class NettingCycleListRegistryDataTypeTest : CodeDescriptionPairListRegistryDataTypeTest
	{
		public void TestConstructor()
		{
			NettingCycleListRegistryDataType localDataType = new NettingCycleListRegistryDataType();
			AssertEquals("Has to be set to 9 in the constructor", 9, localDataType.CodeMaxLength);
		}

		[ExpectNoExceptions]
		public void TestValidate_NoErrors()
		{
			NettingCycleListRegistryDataType localDataType = new NettingCycleListRegistryDataType();
			localDataType.Validate(null, ValidList, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectExceptionMessage(typeof(RegistryValidationException), "The following data is not a valid Netting Cycle Date (date format should be dd-MMM-yy, i.e. 05-JAN-05):\r\nasdfasdf\r\n36-APR-05\r\n2-JAN-05\r\nlkjsdfj")]
		public void TestValidate_WithErrors()
		{
			NettingCycleListRegistryDataType localDataType = new NettingCycleListRegistryDataType();
			localDataType.Validate(null, InvalidList, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		protected override CodeDescriptionPairListRegistryDataType GetNewDataType()
		{
			return new NettingCycleListRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			CodeDescriptionPairList emptyList = new CodeDescriptionPairList();
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(emptyList, emptyList.ToXMLByteArray()), new ValidSampleAndBinaryValueInDB(ValidList, ValidList.ToXMLByteArray()) };
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { InvalidList };
		}

		CodeDescriptionPairList ValidList
		{
			get
			{
				if (fValidList == null)
				{
					fValidList = new CodeDescriptionPairList();
					fValidList.AddPair("05-JAN-05");
					fValidList.AddPair("06-FEB-05");
					fValidList.AddPair("07-MAR-05");
				}

				return fValidList;
			}
		}

		CodeDescriptionPairList InvalidList
		{
			get
			{
				if (fInvalidList == null)
				{
					fInvalidList = new CodeDescriptionPairList();
					fInvalidList.AddPair("05-jan-05");
					fInvalidList.AddPair("asdfasdf");
					fInvalidList.AddPair("36-APR-05");
					fInvalidList.AddPair("01-FEB-05");
					fInvalidList.AddPair("2-JAN-05");
					fInvalidList.AddPair("06-MAY-05");
					fInvalidList.AddPair(" lkjsdfj");
				}

				return fInvalidList;
			}
		}

		CodeDescriptionPairList fValidList;
		CodeDescriptionPairList fInvalidList;
	}
}
