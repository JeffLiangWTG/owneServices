using System;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class GUACODMessageHeaderProviderTest : Customs.Business.Testing.DataProviderTestCase<GUACODMessageHeaderProvider>
	{
		[TestDate(2022, 5, 3, 16, 35, 22)]
		public void TestPreparationDateAndTimeUtc()
		{
			AssertEquals(new DateTime(2022, 5, 3, 16, 35, 22), Provider.PreparationDateAndTimeUtc.DateAndTime);
		}

		public void TestInterchangeSender_OrgProxy()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);

			var proxy = GlbBranch.CurrentBranch.OrgProxy;
			proxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR1", Core.Constants.CountryCodes.Germany);
			proxy.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "EBS1", Core.Constants.CountryCodes.Germany);
			proxy.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "9877700310012345123456000", Core.Constants.CountryCodes.Germany);

			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "DE0123456REG"))
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6777"))
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6800000310012345123456000"))
			{
				AssertEquals("EORINumber", "DEEOR1", Provider.InterchangeSender.EoriNumber);
				AssertEquals("EORIBranch", "EBS1", Provider.InterchangeSender.EoriBranchSuffix);
			}
		}

		public void TestInterchangeSender_Registry()
		{
			var proxy = GlbBranch.CurrentBranch.OrgProxy;
			proxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR1", Core.Constants.CountryCodes.Germany);
			proxy.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "EBS1", Core.Constants.CountryCodes.Germany);

			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "DE0123456REG"))
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6777"))
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6800000310012345123456000"))
			{
				AssertEquals("EORINumber", "DE0123456REG", Provider.InterchangeSender.EoriNumber);
				AssertEquals("EORIBranch", "6777", Provider.InterchangeSender.EoriBranchSuffix);
			}
		}

		public void TestAuthenticationNumber_OrgProxy()
		{
			var proxy = GlbBranch.CurrentBranch.OrgProxy;
			proxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR1", Core.Constants.CountryCodes.Germany);
			proxy.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "EBS1", Core.Constants.CountryCodes.Germany);
			proxy.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "9877700310012345123456000", Core.Constants.CountryCodes.Germany);

			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "DE0123456REG"))
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6777"))
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6800000310012345123456000"))
			{
				CombineAssertions(() =>
				{
					AssertEquals("9877700310012345123456000", Provider.AuthenticationNumber);
				});
			}
		}

		public void TestAuthenticationNumber_Registry()
		{
			var proxy = GlbBranch.CurrentBranch.OrgProxy;
			proxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR1", Core.Constants.CountryCodes.Germany);
			proxy.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "EBS1", Core.Constants.CountryCodes.Germany);

			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "DE0123456REG"))
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6777"))
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6800000310012345123456000"))
			{
				AssertEquals("6800000310012345123456000", Provider.AuthenticationNumber);
			}
		}

		public void TestInterchangeRecipientID()
		{
			AssertEquals("DE003302", Provider.InterchangeRecipientID);
		}

		public void TestMessageIdentification()
		{
			AssertEquals(Provider.MessageIdentification, "<<SENDERS REFERENCE PLACE HOLDER>>");
		}

		public void TestAuthorisedConsignee()
		{
			AssertNull(Provider.AuthorisedConsignee);
		}

		public void TestHeader()
		{
			AssertNotNull(Provider.Header);
		}

		protected override void SetUp()
		{
			base.SetUp();
			guarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			sendAccessCode = new SendAccessCodeViewModel(guarantee);
			sendAccessCode.OfficeOfGuarantee = "DE003302";
		}
		SendAccessCodeViewModel sendAccessCode;
		CusGuaranteeHeader guarantee;

		protected override GUACODMessageHeaderProvider GetProvider() => new GUACODMessageHeaderProvider(sendAccessCode);
	}
}
