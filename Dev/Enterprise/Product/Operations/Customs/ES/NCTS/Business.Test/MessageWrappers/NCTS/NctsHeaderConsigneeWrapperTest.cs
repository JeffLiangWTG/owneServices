using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NctsHeaderConsigneeWrapperTest : WrapperHelperTest<NctsHeaderConsigneeWrapper>
	{
		public void TestConsigneeId()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = HeaderData.ImporterCode;
				orgHeader.OH_FullName = OrgHeaderData.Name;
				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				var orgAddress = orgHeader.MainAddress;
				orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;

				nctsHeader.Consignee.OrganisationPK = orgHeader.PK;

				wrapper = NctsHeaderConsigneeWrapper.New(nctsHeader.Consignee);
				AssertEquals("Expected Empty Id if Consignee country is not from EU country", ZString.Empty, wrapper.Id);

				orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

				wrapper = NctsHeaderConsigneeWrapper.New(nctsHeader.Consignee);
				AssertEquals("Expected Id if Consignee country is from EU country", "NIF22222222", wrapper.Id);

				orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;

				wrapper = NctsHeaderConsigneeWrapper.New(nctsHeader.Consignee);
				AssertEquals("Expected Id if Consignee country is Spain", "NIF22222222", wrapper.Id);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.FillWithValidTestData();

			orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderData.ConsigneeCode;
			orgHeader.OH_FullName = OrgHeaderData.Name;
			orgHeader.Addresses.AddNew();

			nctsHeader.Consignee.OrganisationPK = orgHeader.PK;

			wrapper = NctsHeaderConsigneeWrapper.New(nctsHeader.Consignee);
		}

		NctsHeader nctsHeader;
		OrgHeader orgHeader;
		NctsHeaderConsigneeWrapper wrapper;

		protected override NctsHeaderConsigneeWrapper GetProvider() => wrapper;
	}
}
