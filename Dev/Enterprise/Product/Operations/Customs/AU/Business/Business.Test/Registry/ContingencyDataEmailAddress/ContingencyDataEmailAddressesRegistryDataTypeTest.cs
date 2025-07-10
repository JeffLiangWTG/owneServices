using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ContingencyDataEmailAddressesRegistryDataType))]
	sealed class ContingencyDataEmailAddressesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ContingencyDataEmailAddressesRegistryDataType>
	{
		protected override ContingencyDataEmailAddressesRegistryDataType GetNewDataType()
		{
			return new ContingencyDataEmailAddressesRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ContingencyDataEmailAddressesRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample1 = new ContingencyDataEmailAddressCollection();
			var address1 = sample1.AddNew();
			address1.Code = "1";
			address1.Description = (NoResString)"1@2.COM";

			var sample2 = new ContingencyDataEmailAddressCollection();
			var address2 = sample2.AddNew();
			address2.Code = "2";
			address2.Description = (NoResString)"2@3.COM";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample1, new ContingencyDataEmailAddressesRegistryDataType().Serialise(sample1)),
				new ValidSampleAndBinaryValueInDB(sample2, new ContingencyDataEmailAddressesRegistryDataType().Serialise(sample2))
			};
		}
	}
}
