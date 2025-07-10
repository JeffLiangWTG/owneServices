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
	[TestedType(typeof(NctsDefaultConsignorConsigneeRegistryDataType))]
	class NctsDefaultConsignorConsigneeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<NctsDefaultConsignorConsigneeRegistryDataType>
	{
		protected override NctsDefaultConsignorConsigneeRegistryDataType GetNewDataType() => new NctsDefaultConsignorConsigneeRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var organisation = factory.NewWithValidTestData<OrgHeader>();
			var nctsDefaultConsignorConsignee1 = new NctsDefaultConsignorConsignee(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), factory);
			nctsDefaultConsignorConsignee1.LeaveBlank = true;
			var nctsDefaultConsignorConsignee2 = new NctsDefaultConsignorConsignee(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), factory);
			nctsDefaultConsignorConsignee2.LeaveBlank = false;
			nctsDefaultConsignorConsignee2.ValueFrom = true;
			nctsDefaultConsignorConsignee2.Consignor = true;

			return new ValidSampleAndBinaryValueInDB[2]
			{
				new ValidSampleAndBinaryValueInDB(nctsDefaultConsignorConsignee1, new NctsDefaultConsignorConsigneeRegistryDataType().Serialise(nctsDefaultConsignorConsignee1)),
				new ValidSampleAndBinaryValueInDB(nctsDefaultConsignorConsignee2, new NctsDefaultConsignorConsigneeRegistryDataType().Serialise(nctsDefaultConsignorConsignee2))
			};
		}

		protected override string ExpectedEditorName => "NctsDefaultConsignorConsigneeRegistryItemEditor";
	}
}
