using System;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(EmailAddressTypeListRegistryDataType))]
	sealed class EmailAddressTypeListRegistryDataTypeTest : RegistryDataTypeTestCase<EmailAddressTypeListRegistryDataType>
	{
		public void TestValidate_DescriptionConfictsWithReserved()
		{
			var dataType = new EmailAddressTypeListRegistryDataType(20, false);
			var list = new CodeDescriptionPairList();
			list.AddPair("IMP", "Import");
			list.AddPair("MIT", "MAIN");

			AssertExceptionThrown<RegistryValidationException>("Conflict with reserved description", "The description 'MAIN' cannot be used as it is reserved for the default staff email address.", () => dataType.Validate(null, list, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestValidate_CodeConfictsWithReserved()
		{
			var dataType = new EmailAddressTypeListRegistryDataType(20, false);
			var list = new CodeDescriptionPairList();
			list.AddPair("IMP", "Import");
			list.AddPair("MAI", "Primary");

			AssertExceptionThrown<RegistryValidationException>("Conflict with reserved code", "The code 'MAI' cannot be used as it is reserved for the default staff email address.", () => dataType.Validate(null, list, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override EmailAddressTypeListRegistryDataType GetNewDataType()
		{
			return new EmailAddressTypeListRegistryDataType(20);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var list1 = new CodeDescriptionPairList();
			var list2 = new CodeDescriptionPairList();
			list2.AddPair("IMP", "Import");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(list1, list1.ToXMLByteArray()),
				new ValidSampleAndBinaryValueInDB(list2, list2.ToXMLByteArray())
			};
		}

		protected override object[] GetInvalidSamples()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("MAI", "MAIN");

			return new object[] { list };
		}
	}
}
