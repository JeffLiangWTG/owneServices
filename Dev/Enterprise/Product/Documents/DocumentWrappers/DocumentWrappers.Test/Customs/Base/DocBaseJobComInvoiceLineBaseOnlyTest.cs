using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	public class DocBaseJobComInvoiceLineBaseOnlyTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertTypeForNew<AU.DocJobComInvoiceLine>(Core.Constants.CountryCodes.Australia);
			AssertTypeForNew<General.DocJobComInvoiceLine>(Core.Constants.CountryCodes.Fiji);
			AssertTypeForNew<NZ.DocJobComInvoiceLine>(Core.Constants.CountryCodes.NewZealand);

			void AssertTypeForNew<TDocJobCominvoiceLine>(string countryCode)
				where TDocJobCominvoiceLine : DocBaseJobComInvoiceLine
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
					var wrapper = DocBaseJobComInvoiceLine.New(invoiceLine, Factory);
					Assert($"Expected DocBaseJobComInvoiceLine.New() to return a DocumentWrapper of type [{typeof(TDocJobCominvoiceLine)}] for a [{countryCode}] country context but one of type [{wrapper.GetType()}] was returned.", typeof(TDocJobCominvoiceLine).IsInstanceOfType(wrapper));
				}
			}
		}
	}
}
