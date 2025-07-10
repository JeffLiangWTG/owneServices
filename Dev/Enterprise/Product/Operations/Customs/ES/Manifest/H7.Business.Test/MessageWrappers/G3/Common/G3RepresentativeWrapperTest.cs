using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class G3RepresentativeWrapperTest : DataProviderTestCase<G3RepresentativeWrapper>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNotNull("Expected non-null wrapper when representative is not null", wrapper);

				wrapper = G3RepresentativeWrapper.New(null);
				AssertNull("Expected null wrapper when representative is null", wrapper);
			});
		}

		public void TestIdNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Id", ZString.Empty, wrapper.IdNumber);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("Still Expect empty Id", ZString.Empty, wrapper.IdNumber);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected NIF Id", "NIF22222222", wrapper.IdNumber);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", wrapper.IdNumber);

				OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI Id with country code", "FR22222222", wrapper.IdNumber);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI Id with country code not repeated", "ES22222222", wrapper.IdNumber);
			});
		}

		public void TestStatus()
		{
			AssertEquals("Expected filled Status", "2", wrapper.Status);
		}

		public void TestName()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Name when CUS Contact is null", string.Empty, wrapper.Name);

				var contact = orgHeader.Contacts.AddNew();
				var allocation = contact.Allocations.AddNew();
				allocation.PC_Type = "CUS";
				AssertEquals("Expected empty Name when CUS Contact Name is null", string.Empty, wrapper.Name);

				contact.OC_ContactName = "cname";
				AssertEquals("Expected filled Name when CUS Contact Name is available", "cname", wrapper.Name);
			});
		}

		public void TestCommunication()
		{
			CombineAssertions(() =>
			{
				var contact = orgHeader.Contacts.AddNew();
				contact.OC_Email = "TEST@123.com";
				var allocation = contact.Allocations.AddNew();
				allocation.PC_Type = "CUS";

				var communication = wrapper.Communication;
				AssertNotNull("Expected filled Communication", communication);
				AssertEquals("Expected filled Communication Type", "EM", communication.CommunicationType);
				AssertSame("Expected cached Communication", communication, wrapper.Communication);

				var header = Factory.New<OrgHeader>();
				wrapper = G3RepresentativeWrapper.New(header);
				AssertNull("Expected empty Communication when no CUS contact", wrapper.Communication);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			wrapper = G3RepresentativeWrapper.New(orgHeader);
		}

		protected override G3RepresentativeWrapper GetProvider()
		{
			return wrapper;
		}

		G3RepresentativeWrapper wrapper;
		OrgHeader orgHeader;
	}
}
