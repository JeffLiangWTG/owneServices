using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class CalloutLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBillToOrganisationList()
		{
			AssertNotNull(Lookups.BillToOrganisationList);
			AssertEquals(Factory, Lookups.BillToOrganisationList.Factory);
		}

		public void TestBillToCountryList()
		{
			AssertNotNull(Lookups.BillToCountryList);
			AssertEquals(Factory, Lookups.BillToCountryList.Factory);
		}

		public void TestImporterOrganisationList()
		{
			AssertNotNull(Lookups.ImporterOrganisationList);
			AssertEquals(Factory, Lookups.ImporterOrganisationList.Factory);
		}

		public void TestImporterCountryList()
		{
			AssertNotNull(Lookups.ImporterCountryList);
			AssertEquals(Factory, Lookups.ImporterCountryList.Factory);
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		CalloutLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new CalloutLookups(Callout);
				}

				return fLookups;
			}
		}

		Callout Callout
		{
			get
			{
				if (fCallout == null)
				{
					fCallout = Factory.New<Callout>();
				}

				return fCallout;
			}
		}

		CalloutLookups fLookups;
		Callout fCallout;
		#endregion
	}
}
