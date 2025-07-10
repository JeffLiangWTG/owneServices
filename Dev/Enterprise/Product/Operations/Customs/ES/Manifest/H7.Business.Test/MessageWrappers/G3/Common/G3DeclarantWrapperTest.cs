using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class G3DeclarantWrapperTest : DataProviderTestCase<G3DeclarantWrapper>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNotNull("Expected non-null wrapper when declarant is not null", wrapper);

				wrapper = G3DeclarantWrapper.New(null);
				AssertNull("Expected null wrapper when declarant is null", wrapper);
			});
		}

		public void TestIdNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Id", ZString.Empty, wrapper.IdNumber);

				orgheader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("Still Expect empty Id", ZString.Empty, wrapper.IdNumber);

				orgheader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgheader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected NIF Id", "NIF22222222", wrapper.IdNumber);

				orgheader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgheader.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", wrapper.IdNumber);

				OrgCusCode eoriCusCode = orgheader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI Id with country code", "FR22222222", wrapper.IdNumber);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI Id with country code not repeated", "ES22222222", wrapper.IdNumber);
			});
		}

		public void TestName()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled Name when IdNumber is empty", "name", wrapper.Name);

				var cuscode = orgheader.CustomsCodes.AddNew();
				cuscode.OK_CodeType = "EOR";
				cuscode.OK_CustomsRegNo = "12345";
				AssertEquals("Expected empty Name when IdNumber is not empty", ZString.Empty, wrapper.Name);
			});
		}

		public void TestFullAddress()
		{
			CombineAssertions(() =>
			{
				var address = wrapper.FullAddress;
				AssertNotNull("Expected filled Full Address", address);
				AssertEquals("Expected filled Full Address street", "address 1 address 2", address.Street);

				AssertSame("Expected cached Full Address", address, wrapper.FullAddress);
			});
		}

		public void TestCommunication()
		{
			CombineAssertions(() =>
			{
				var communication = wrapper.Communication;
				AssertNotNull("Expected filled Communication", communication);
				AssertEquals("Expected filled Communication Type", "EM", wrapper.Communication.CommunicationType);
				AssertSame("Expected cached Communication", communication, wrapper.Communication);

				var header = Factory.New<OrgHeader>();
				wrapper = G3DeclarantWrapper.New(header.MainAddress);
				AssertNull("Expected empty Communication when no CUS contact", wrapper.Communication);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			orgheader = Factory.New<OrgHeader>();
			orgheader.OH_FullName = "name";
			var contact = orgheader.Contacts.AddNew();
			contact.OC_Email = "TEST@123.com";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = "CUS";
			var address = orgheader.MainAddress;
			address.OA_Address1 = "address 1";
			address.OA_Address2 = "address 2";
			wrapper = G3DeclarantWrapper.New(address);
		}

		protected override G3DeclarantWrapper GetProvider()
		{
			return wrapper;
		}

		G3DeclarantWrapper wrapper;
		OrgHeader orgheader;
	}
}
