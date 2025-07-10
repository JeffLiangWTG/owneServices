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
	[TestedType(typeof(NctsDefaultPrincipalRegistryDataType))]
	class NctsDefaultPrincipalRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<NctsDefaultPrincipalRegistryDataType>
	{
		protected override NctsDefaultPrincipalRegistryDataType GetNewDataType() => new NctsDefaultPrincipalRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var organisation = factory.NewWithValidTestData<OrgHeader>();
			var nctsDefaultPrincipal1 = new NctsDefaultPrincipal(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), factory);
			nctsDefaultPrincipal1.LeaveBlank = true;
			nctsDefaultPrincipal1.Principal = organisation.PK;
			var nctsDefaultPrincipal2 = new NctsDefaultPrincipal(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), factory);
			nctsDefaultPrincipal2.LeaveBlank = false;
			nctsDefaultPrincipal2.Principal = organisation.PK;

			return new ValidSampleAndBinaryValueInDB[2]
			{
				new ValidSampleAndBinaryValueInDB(nctsDefaultPrincipal1, new NctsDefaultPrincipalRegistryDataType().Serialise(nctsDefaultPrincipal1)),
				new ValidSampleAndBinaryValueInDB(nctsDefaultPrincipal2, new NctsDefaultPrincipalRegistryDataType().Serialise(nctsDefaultPrincipal2))
			};
		}

		protected override string ExpectedEditorName => "NctsDefaultPrincipalRegistryItemEditor";
	}
}
