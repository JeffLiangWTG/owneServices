using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Registry.Testing
{
	[TestedType(typeof(NctsDefaultTraderAtDestinationRegistryDataType))]
	sealed class NctsDefaultTraderAtDestinationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<NctsDefaultTraderAtDestinationRegistryDataType>
	{
		protected override NctsDefaultTraderAtDestinationRegistryDataType GetNewDataType() => new NctsDefaultTraderAtDestinationRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var organisation = factory.NewWithValidTestData<OrgHeader>();
			var nctsDefaultTraderAtDestination1 = new NctsDefaultTraderAtDestination(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), factory);
			nctsDefaultTraderAtDestination1.LeaveBlank = true;
			nctsDefaultTraderAtDestination1.TraderAtDestination = organisation.PK;
			var nctsDefaultTraderAtDestination2 = new NctsDefaultTraderAtDestination(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), factory);
			nctsDefaultTraderAtDestination2.LeaveBlank = false;
			nctsDefaultTraderAtDestination2.TraderAtDestination = organisation.PK;

			return new ValidSampleAndBinaryValueInDB[2]
			{
				new ValidSampleAndBinaryValueInDB(nctsDefaultTraderAtDestination1, new NctsDefaultTraderAtDestinationRegistryDataType().Serialise(nctsDefaultTraderAtDestination1)),
				new ValidSampleAndBinaryValueInDB(nctsDefaultTraderAtDestination2, new NctsDefaultTraderAtDestinationRegistryDataType().Serialise(nctsDefaultTraderAtDestination2))
			};
		}

		protected override string ExpectedEditorName => "NctsDefaultTraderAtDestinationRegistryItemEditor";
	}
}
