using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestsSubclassesOf(typeof(AESMessageHeaderProvider))]
	public abstract class AESMessageHeaderProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : AESMessageHeaderProvider
	{
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
		}
		protected JobDeclaration declaration;
		protected CusEntryHeader entryHeader;

		protected OrgAddress GetOrgWithEORNumberAndEORIBranch(ZString eoriNumber, ZString ebsNumber)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriNumber, Core.Constants.CountryCodes.Greece);
			var address = orgHeader.Addresses.AddNew();
			address.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			var ebsCode = orgHeader.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, ebsNumber, Core.Constants.CountryCodes.Germany);
			ebsCode.OK_OA_PremisesAddress = address.PK;
			return address;
		}
	}
}
