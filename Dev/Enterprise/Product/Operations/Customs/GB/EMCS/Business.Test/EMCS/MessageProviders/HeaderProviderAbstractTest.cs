using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestsSubclassesOf(typeof(HeaderProvider))]
	abstract class HeaderProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T> where T : HeaderProvider
	{
		protected T HeaderProvider => headerProvider ?? (headerProvider = GetHeaderProvider());
		protected T headerProvider;

		protected abstract T GetHeaderProvider();

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
		}
		protected EMCSJobDeclaration emcsDeclaration;

		protected OrgHeader GetPartyGuarantorOrg(ZString exciseNumber, ZString vatNumber)
		{
			var orgHeader = GetPartyTraderExciseNumberOrg(exciseNumber);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, vatNumber, Core.Constants.CountryCodes.UnitedKingdom);
			orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			return orgHeader;
		}

		protected OrgHeader GetPartyTraderExciseNumberOrg(ZString exciseNumber)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, exciseNumber, Core.Constants.CountryCodes.Greece);
			return orgHeader;
		}

		protected OrgHeader GetPartyVatNumberOrg(ZString vatNumber)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, vatNumber, Core.Constants.CountryCodes.UnitedKingdom);
			orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			return orgHeader;
		}

		protected OrgHeader GetPartyTraderIdOrg(ZString traderId)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, traderId, Core.Constants.CountryCodes.Greece);
			return orgHeader;
		}

		protected OrgHeader GetPartyConsigneeOrg(ZString traderId, ZString eoriNumber)
		{
			var orgHeader = GetPartyTraderExciseNumberOrg(traderId);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriNumber, Core.Constants.CountryCodes.Greece);
			return orgHeader;
		}

		protected override T GetProvider() => HeaderProvider;
	}
}
