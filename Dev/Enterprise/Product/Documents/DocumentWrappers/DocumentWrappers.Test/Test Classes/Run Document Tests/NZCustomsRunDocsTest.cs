using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.Customs
{
	sealed class NZCustomsRunDocsTest : CustomsRunDocsTest
	{
		ZString StoredCountry;

		protected override void SetUp()
		{
			StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.NewZealand);
			base.SetUp();
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(StoredCountry);
			base.TearDown();
		}

		public override BusinessObject GetBusinessObject
		{
			get
			{
				Enterprise.Customs.NZ.Business.Declaration.JobDeclaration declaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration.New(Factory);
				declaration.Invoices.AddNew();
				return declaration;
			}
		}
	}
}
