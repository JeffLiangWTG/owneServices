using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.ExitControl.Business.Testing;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(ExitMessageHeaderProviderForTest))]
	sealed class ExitMessageHeaderProviderBaseOnlyTest : ExitMessageHeaderProviderAbstractTest<ExitMessageHeaderProviderForTest>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ExitPresentationHeaderProvider(null));
		}

		public void TestInterchangeRecipientID()
		{
			CombineAssertions(() =>
			{
				AssertNull(Provider.InterchangeRecipientID);
				exitReport.CER_OfficeOfExit = "CO001";
				AssertEquals("ExitCustomsOffice", "CO001", Provider.InterchangeRecipientID);
			});
		}

		public void TestInterchangeSender_Carrier()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var carrierAddress = EORIHelperTest.GetOrgWithEORNumberAndEORIBranchAndAPINumber(Factory, "EOR1", "EBS1", "1111111111111111111111111");
			AssertInterchangeSender(carrierAddress.PK, "DEEOR1", "EBS1");
		}

		public void TestInterchangeSender_FallBackToRegistry()
		{
			var carrierAddress = EORIHelperTest.GetOrgWithEORNumberAndEORIBranch(Factory, "EOR1", "EBS1");
			AssertInterchangeSender(carrierAddress.PK, "DEEOR0", "0000");
		}

		public void TestAuthorizationNumber_Carrier()
		{
			var carrierAddress = GetOrg("1111111111111111111111111");
			AssertAuthorizationNumber(carrierAddress.PK, "1111111111111111111111111");
		}

		public void TestAuthorizationNumber_Registry()
		{
			var declarantAddress = GetOrg();
			AssertAuthorizationNumber(declarantAddress.PK, "0000000000000000000000000");
		}

		[TestDate(2019, 11, 08, 12, 11, 59)]
		public void TestPreparationDateAndTimeUtc()
		{
			AssertEquals(new DateTime(2019, 11, 08, 12, 11, 59), Provider.PreparationDateAndTimeUtc.DateAndTime);
		}

		public void TestMessageIdentification()
		{
			AssertEquals("<<SENDERS REFERENCE PLACE HOLDER>>", Provider.MessageIdentification);
		}

		protected override ExitMessageHeaderProviderForTest GetProvider() => new ExitMessageHeaderProviderForTest(exitReport);

		new IExitMessageHeader Provider => base.Provider;

		void AssertInterchangeSender(ZGuid declarantAddressPK, string expectedEOR, string expectedEBS)
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode, VersionNumber = AESVersionNumberList.Codes._30 } };
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var currentBranchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "DEEOR0"))
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(Guid.Empty, currentBranchPK, Guid.Empty, "0000"))
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(Guid.Empty, currentBranchPK, Guid.Empty, "0000000000000000000000000"))
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				exitReport.Header.CXH_OA_Carrier = declarantAddressPK;

				var result = Provider.InterchangeSender;
				CombineAssertions(() =>
				{
					AssertEquals("EOR", expectedEOR, result.EoriNumber);
					AssertEquals("EBS", expectedEBS, result.EoriBranchSuffix);
				});
			}
		}

		void AssertAuthorizationNumber(ZGuid declarantAddressPK, string expectedBIN)
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode, VersionNumber = AESVersionNumberList.Codes._30 } };
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var currentBranchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(Guid.Empty, currentBranchPK, Guid.Empty, "0000000000000000000000000"))
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				exitReport.Header.CXH_OA_Carrier = declarantAddressPK;

				var result = Provider.AuthorizationNumber;
				AssertEquals(expectedBIN, result);
			}
		}

		OrgAddress GetOrg(string apiNumber = null)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			if (!string.IsNullOrEmpty(apiNumber))
			{
				var apiCode = address.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, apiNumber, Core.Constants.CountryCodes.Germany);
				apiCode.OK_OA_PremisesAddress = address.PK;
			}
			return address;
		}

		protected override IExitMessageHeader GetMessageHeaderProvider() => new ExitMessageHeaderProviderForTest(CusExitReportTest.GetNewBusinessObject(Factory).report);
	}

	class ExitMessageHeaderProviderForTest : ExitMessageHeaderProvider
	{
		public ExitMessageHeaderProviderForTest(CusExitReport cusExitReport) : base(cusExitReport)
		{
		}

		public override IExitHeader ExitHeader => exitHeader ?? (exitHeader = new ExitHeaderProvider(cusExitReport));
		IExitHeader exitHeader;
	}
}
