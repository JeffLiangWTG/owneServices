using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.Licencing.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiCommissionAgreementCompanyWrapper))]
	class EdiCommissionAgreementCompanyWrapperTest : EdiCommissionAgreementTreeBizObjWrapperTestCase<EdiCommissionAgreementCompanyWrapper>
	{
		public void TestSelected()
		{
			var clientCompany = Factory.New<ClientCompany>();
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			var wrapper = new EdiCommissionAgreementCompanyWrapper(customization, clientCompany);
			wrapper.Selected = true;
			AssertContainsExactElementsInAnyOrder(new[] { clientCompany }, customization.CompanyPivots.Select(x => x.ClientCompany));
			wrapper.Selected = false;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ClientCompany>(), customization.CompanyPivots.Select(x => x.ClientCompany));
			AssertEquals(false, clientCompany.IsDeleted);
		}

		public void TestCode()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "AAA";
			var wrapper = new EdiCommissionAgreementCompanyWrapper(customization, clientCompany);
			AssertEquals("AAA", wrapper.Code);
		}

		public void TestDescription()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Name = "A Big Company";
			var wrapper = new EdiCommissionAgreementCompanyWrapper(customization, clientCompany);
			AssertEquals("A Big Company", wrapper.Description);
		}

		#region Overrides
		protected override EdiCommissionAgreementCompanyWrapper GetNewWrapper(EdiCommissionAgreementCustomization customization, IEnumerable<EdiCommissionAgreementTreeBizObjWrapper> children)
		{
			return new EdiCommissionAgreementCompanyWrapper(customization, Factory.New<ClientCompany>());
		}

		protected override EdiCommissionAgreementTreeBizObjWrapper GetNewChildWrapper(EdiCommissionAgreementCustomization customization)
		{
			return null;
		}
		#endregion
	}
}
