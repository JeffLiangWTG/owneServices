using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RegistrationNumberCodeWrapperCollection))]
	sealed class RegistrationNumberCodeWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<RegistrationNumberCodeWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new RegistrationNumberCodeWrapper(null, Factory);
		}

		protected override RegistrationNumberCodeWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new RegistrationNumberCodeWrapperCollection((OrgCusCodeCollection)null, Factory);
		}

		public void TestRegistrationNumberCodeWrapperCollectionWithCodes()
		{
			OrgHeader organistation = Factory.New<OrgHeader>();
			organistation.CustomsCodes.AddNew("GST", "12345678");
			RegistrationNumberCodeWrapperCollection wrapper = new RegistrationNumberCodeWrapperCollection(organistation.CustomsCodes, Factory);
			AssertEquals("wrapper.Count", 1, wrapper.Count);
			AssertEquals("wrapper[0].RegistrationNumberOrCode", "12345678", wrapper[0].RegistrationNumberOrCode);
			AssertEquals("wrapper[0].Type.Code", "GST", wrapper[0].Type.Code);
		}

		public void TestTypeCountryIndexer()
		{
			OrgHeader organistation = Factory.New<OrgHeader>();

			OrgCusCode cuscode1 = organistation.CustomsCodes.AddNew("GST", "12345678");
			cuscode1.OK_RN_NKCodeCountry = "ER";

			OrgCusCode cuscode2 = organistation.CustomsCodes.AddNew("GST", "66677666");
			cuscode2.OK_RN_NKCodeCountry = "NL";

			RegistrationNumberCodeWrapperCollection wrapper = new RegistrationNumberCodeWrapperCollection(organistation.CustomsCodes, Factory);
			AssertEquals("wrapper['GST'].RegistrationNumberOrCode", "12345678", wrapper["GST"].RegistrationNumberOrCode);
			AssertNull("wrapper['ABC']", wrapper["ABC"]);
			AssertEquals("wrapper['NL:GST']", "66677666", wrapper["NL:GST"].RegistrationNumberOrCode);
		}

		public void TestConstructor_OrgAddress()
		{
			OrgHeader organistation = Factory.New<OrgHeader>();
			OrgAddress address2 = organistation.Addresses.AddNew();
			OrgCusCode cuscode1 = organistation.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "123");
			cuscode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
			cuscode1.OK_OA_PremisesAddress = organistation.MainAddress.PK;

			OrgCusCode cuscode2 = organistation.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.CustomsOfficeCode, "567");
			cuscode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
			cuscode2.OK_OA_PremisesAddress = address2.PK;

			RegistrationNumberCodeWrapperCollection wrapper = new RegistrationNumberCodeWrapperCollection(address2.CustomsCodes, Factory);
			AssertEquals("Count", 1, wrapper.Count);
			AssertEquals("RegistrationNumberOrCode", "567", wrapper[Core.Constants.CountryCodes.Canada + ":" + OrgCusCode.CACodeTypes.CustomsOfficeCode].RegistrationNumberOrCode);
		}

		public void TestEORI()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.CustomsCodes.AddNew("TRN", "12000");
			organisation.CustomsCodes.AddNew("CCC", "24680");

			RegistrationNumberCodeWrapperCollection wrapper = new RegistrationNumberCodeWrapperCollection(organisation, Factory);
			AssertEquals("wrapper['CCC'].RegistrationNumberOrCode", "24680", wrapper["CCC"].RegistrationNumberOrCode);
			AssertNull("wrapper['ABC']", wrapper["ABC"]);
			AssertEquals("wrapper['EORI'] = 12000", GlbCompany.CurrentCompany.Country.Code + "12000", wrapper["EORI"].RegistrationNumberOrCode);

			organisation.CustomsCodes.AddNew("EOR", "99999");
			wrapper = new RegistrationNumberCodeWrapperCollection(organisation, Factory);
			AssertEquals("wrapper['EORI'] returns EOR code in preference to TRN code", GlbCompany.CurrentCompany.Country.Code + "99999", wrapper["EORI"].RegistrationNumberOrCode);
		}

		public void TestConsumptionTax()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.CustomsCodes.AddNew("VAT", "12345", Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "GB"));
			organisation.CustomsCodes.AddNew("ABN", "67890", Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU"));
			organisation.CustomsCodes.AddNew("CCC", "decoy", Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU"));

			RegistrationNumberCodeWrapperCollection wrapper = new RegistrationNumberCodeWrapperCollection(organisation, Factory);

			ZString savedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";

				AssertEquals("Return ABN where no country specified", "67890", wrapper["CONSUMPTIONTAX"].RegistrationNumberOrCode);
				AssertEquals("Returns VAT where GB is specified", "12345", wrapper["GB:CONSUMPTIONTAX"].RegistrationNumberOrCode);
				AssertNull("Returns null for invalid country", wrapper["XX:CONSUMPTIONTAX"]);
				AssertNull("Returns null for unspecified country", wrapper[":CONSUMPTIONTAX"]);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = savedCountry;
			}
		}
	}
}
