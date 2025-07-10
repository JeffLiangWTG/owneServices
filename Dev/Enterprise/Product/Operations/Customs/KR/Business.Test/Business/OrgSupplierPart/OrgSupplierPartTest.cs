using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(OrgSupplierPart))]
	sealed class OrgSupplierPartTest : Customs.Business.Testing.OrgSupplierPartTest
	{
		public void TestCustomsCountryCodeIsCorrect()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var part = Factory.New<OrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				AssertEquals(Core.Constants.CountryCodes.KoreaSouth, pivot.CI_RN_NKCountry);
			}
		}

		#region GetNewBusinessObject
		protected override BusinessObject GetNewBusinessObject()
		{
			return OrgSupplierPart.New(Factory);
		}
		#endregion
	}
}
