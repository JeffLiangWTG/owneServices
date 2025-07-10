using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Registry.Business.SystemDataRegistry;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EmailAddressesAllowedForImportDataType))]
	sealed class EmailAddressesAllowedForImportDataTypeTest : RegistryDataTypeTestCase<EmailAddressesAllowedForImportDataType>
	{
		protected override EmailAddressesAllowedForImportDataType GetNewDataType()
		{
			return new EmailAddressesAllowedForImportDataType(256);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var list1 = new CodeDescriptionPairList();
			list1.AddPair("test@domain.com", "Test email 1");
			list1.AddPair("email@company.com", "Test email 2");

			var list2 = new CodeDescriptionPairList();
			list2.AddPair("test-*@domain.com", "Test email with wildcard");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(list1, new EmailAddressesAllowedForImportDataType(256).Serialise(list1)),
				new ValidSampleAndBinaryValueInDB(list2, new EmailAddressesAllowedForImportDataType(256).Serialise(list2))
			};
		}
	}
}
