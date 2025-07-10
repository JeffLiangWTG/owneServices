using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestsSubclassesOf(typeof(SplitProvider))]
	abstract class SplitProviderProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T> where T : SplitProvider
	{
		protected T SplitProvider => splitProvider ?? (splitProvider = GetSplitProvider());
		protected T splitProvider;

		protected abstract T GetSplitProvider();

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			emcsInvoiceHeader = emcsDeclaration.Invoices.AddNew();
		}
		protected EMCSJobDeclaration emcsDeclaration;
		protected EMCSJobComInvoiceHeader emcsInvoiceHeader;

		protected OrgHeader GetPartyTraderExciseNumberOrg(ZString exciseNumber)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, exciseNumber, Core.Constants.CountryCodes.Portugal);
			return orgHeader;
		}

		protected OrgHeader GetPartyVatNumberOrg(ZString vatNumber)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, vatNumber, Core.Constants.CountryCodes.UnitedKingdom);
			orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			return orgHeader;
		}

		protected override T GetProvider() => SplitProvider;
	}
}
