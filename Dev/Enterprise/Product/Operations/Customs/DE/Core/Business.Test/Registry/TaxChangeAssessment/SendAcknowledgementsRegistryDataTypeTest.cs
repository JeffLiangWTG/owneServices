using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Registry.Testing
{
	[TestedType(typeof(SendAcknowledgementsRegistryDataType))]
	class SendAcknowledgementsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SendAcknowledgementsRegistryDataType>
	{
		protected override SendAcknowledgementsRegistryDataType GetNewDataType() => new SendAcknowledgementsRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new CargoWise.EntityFramework.BusinessObjectFactory();
			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			company.GC_Code = "DE1";
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "DE1OH";
			var br1 = company.Branches.AddNew();
			br1.GB_Code = "BR1";
			br1.GB_OH_OrgProxy = orgHeader.PK;
			br1.OrgProxy.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "0001", Core.Constants.CountryCodes.Germany);
			factory.Save();

			var fallbackLevel = new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var collection = new SendAcknowledgementsRegistryCollection()
			{
				new SendAcknowledgementsRegistry(fallbackLevel, factory) { EBSCode = "0001", SendGroupPK = Core.Constants.Groups.PostMastersGroupPK }
			};
			var emptyCollection = new SendAcknowledgementsRegistryCollection();

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(emptyCollection, DataType.Serialise(emptyCollection))
			};
		}

		protected override string ExpectedEditorName => "SendAcknowledgementsRegistryItemEditor";
	}
}
