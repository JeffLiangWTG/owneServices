using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	class FRDocAddressControlTest : TestCaseWithFactory
	{
		public void TestEORI()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supplier = CreateOrganisationWithAddressAndEori("SUPPLIER", "Supplier Name", "Supplier Address", "111111111111");
			declaration.JE_OH_Supplier = supplier.PK;
			using (var form = new JobDeclarationForm(declaration))
			using (var docAddressControl = new FRDocAddressControl())
			{
				form.Controls.Add(docAddressControl);
				form.Show();
				Assert("Supplier Address should contain the EORI.", form.Controls.Find("AddressLabel", true).Any(x => x.Text == "SUPPLIER ADDRESS\nFRANCE\nEORI: FR111111111111"));
			}
		}

		OrgHeader CreateOrganisationWithAddressAndEori(ZString orgCode, ZString fullName, ZString address, ZString eori)
		{
			var fr = Factory.Load<RefCountry>(Core.Constants.CountryGuids.France);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = orgCode;
			orgHeader.OH_FullName = fullName;
			orgHeader.MainAddress.OA_Address1 = address;
			orgHeader.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, fr, eori);
			return orgHeader;
		}
	}
}
