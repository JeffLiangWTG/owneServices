using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.Customs
{
	sealed class FJCustomsRunDocsTest : CustomsRunDocsTest
	{
		ZString StoredCountry;

		protected override void SetUp()
		{
			StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Fiji);
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
				BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
				declaration.Invoices.AddNew();
				return declaration;
			}
		}
	}
}
