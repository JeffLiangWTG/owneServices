using System;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SoapHeaderProviderTest : ICS2BaseMessageProviderTest<SoapHeaderProvider>
	{
		public void TestBinaryFiles()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();

			var provider = new SoapHeaderProvider(manifestHeader, interchange, Array.Empty<IBinaryFile>());
			AssertEquals(0, provider.BinaryFiles.Count);

			provider = new SoapHeaderProvider(manifestHeader, interchange);
			AssertEquals(0, provider.BinaryFiles.Count);
		}

		public void TestFromPartyValue()
		{
			SetupSystemCode();

			CombineAssertions(() =>
			{
				var provider = Provider;
				var systemCode = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID;

				using (ICS2CustomsDataRegistry.Instance.IncludeMemberStateInSiteID.SetTemporaryValue(manifestHeader.ProfileCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("Should include the member state in the final ID.", $"{systemCode}@DE966882266928828@DE", provider.FromPartyValue);
				}

				using (ICS2CustomsDataRegistry.Instance.IncludeMemberStateInSiteID.SetTemporaryValue(manifestHeader.ProfileCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("Should exclude the member state in the final ID.", $"{systemCode}@DE966882266928828", provider.FromPartyValue);
				}

				using (ICS2CustomsDataRegistry.Instance.SenderPartyId.SetTemporaryValue(manifestHeader.ProfileCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABC"))
				{
					AssertEquals("Should use the value from SenderPartyId.", "ABC", provider.FromPartyValue);
				}
			});
		}

		[TestDate(2023, 06, 29, 10, 59, 59)]
		public void TestFromPartyValueWithLegacySenderMemberState()
		{
			SetupSystemCode();

			var provider = Provider;
			using (ICS2CustomsDataRegistry.Instance.IncludeMemberStateInSiteID.SetTemporaryValue(manifestHeader.ProfileCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var systemCode = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID;
				AssertEquals("Should return the legacy state when the time is before 2023-06-29 11:00.", $"{systemCode}@DE966882266928828@DA", provider.FromPartyValue);
			}
		}

		[TestDate(2023, 07, 01)]
		public void TestFromPartyValueWithIntendedCodeType()
		{
			SetupSystemCode();

			var cacheKey = $"RefSysConfig-ICS2INCSYS{ZDate.Today.ToShortDateString()}";

			var sysConfig = Factory.New<RefSysConfig>();
			sysConfig.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SSYS;
			sysConfig.ZRC_StartDate = new ZDateTime(1900, 01, 01);
			sysConfig.ZRC_EndDate = new ZDateTime(2079, 06, 06);
			sysConfig.ZRC_StringValue = "SYSTST";

			var codeTypeConfig = Factory.New<RefSysConfig>();
			codeTypeConfig.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2INCSYS;
			codeTypeConfig.ZRC_StartDate = new ZDateTime(1900, 01, 01);
			codeTypeConfig.ZRC_EndDate = new ZDateTime(2079, 06, 06);

			CombineAssertions(() =>
			{
				var systemCode = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID;

				AssertFromPartyValue(MessageProviderHelper.IntendedCodeTypes.Yes, $"{systemCode}@DE966882266928828");
				AssertFromPartyValue(MessageProviderHelper.IntendedCodeTypes.No, $"DE966882266928828");
				AssertFromPartyValue(MessageProviderHelper.IntendedCodeTypes.Wtg, $"SYSTST@DE966882266928828");
			});

			void AssertFromPartyValue(string intendedCodeType, string expectedFromPartyValue)
			{
				codeTypeConfig.ZRC_StringValue = intendedCodeType;

				Factory.Save();
				Factory.ClearCachedValue<ZString>(cacheKey);

				AssertEquals(intendedCodeType, expectedFromPartyValue, Provider.FromPartyValue);
			}
		}

		void SetupSystemCode()
		{
			var stateChangeTime = new ZDateTime(2023, 06, 29, 11, 00, 00);

			new[]
			{
				MessageProviderHelper.RefSysConfigCodes.ICS2SID,
				MessageProviderHelper.RefSysConfigCodes.ICS2SMS1,
				MessageProviderHelper.RefSysConfigCodes.ICS2SMS2,
				MessageProviderHelper.RefSysConfigCodes.ICS2SSYS,
				MessageProviderHelper.RefSysConfigCodes.ICS2INCSYS
			}.ForEach(c =>
			{
				var configType = Factory.New<RefSysConfigType>();
				configType.ZRT_ConfigCode = c;
				configType.ZRT_Description = configType.ZRT_LongDescription = $"{c} For Testing";
			});

			var idConfig = Factory.New<RefSysConfig>();
			idConfig.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SID;
			idConfig.ZRC_StartDate = new ZDateTime(1900, 01, 01);
			idConfig.ZRC_EndDate = new ZDateTime(2079, 06, 06);
			idConfig.ZRC_StringValue = "DE966882266928828";

			var memberState1Config = Factory.New<RefSysConfig>();
			memberState1Config.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SMS1;
			memberState1Config.ZRC_StartDate = new ZDateTime(1900, 01, 01);
			memberState1Config.ZRC_EndDate = stateChangeTime;
			memberState1Config.ZRC_StringValue = "DA";

			var memberState2Config = Factory.New<RefSysConfig>();
			memberState2Config.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SMS2;
			memberState2Config.ZRC_StartDate = stateChangeTime;
			memberState2Config.ZRC_EndDate = new ZDateTime(2079, 06, 06);
			memberState2Config.ZRC_StringValue = "DE";
		}

		public void TestSpecificCircumstance()
		{
			var ediInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			ediInterchange.ContainedMessages.RemoveAndDeleteAll();

			var message = ediInterchange.ContainedMessages.AddNew();
			message.EM_MessageType = "F24";

			var provider = new SoapHeaderProvider(manifestHeader, ediInterchange);
			AssertEquals("F24", provider.SpecificCircumstance);

			ediInterchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.TST;
			AssertNullOrEmpty(provider.SpecificCircumstance);
		}

		public void TestMessageId()
		{
			var ediInterchange = Factory.NewWithValidTestData<EDIInterchange>();

			var provider = new SoapHeaderProvider(manifestHeader, ediInterchange);
			AssertEquals($"{ediInterchange.PK}@xttest-eidas-gwy.wisegrid.net", provider.MessageId);

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			AssertEquals($"{ediInterchange.PK}@xt-eidas-gwy.wisegrid.net", provider.MessageId);

			ediInterchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.TST;
			AssertEquals(SoapHeaderProvider.TestMessageId, provider.MessageId);
		}

		public void TestPartInfoHref()
		{
			var ediInterchange = Factory.NewWithValidTestData<EDIInterchange>();

			var provider = new SoapHeaderProvider(manifestHeader, ediInterchange);
			AssertEquals($"cid:attachment1@xttest-eidas-gwy.wisegrid.net", provider.PartInfoHref);

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			AssertEquals($"cid:attachment1@xt-eidas-gwy.wisegrid.net", provider.PartInfoHref);
		}

		public void TestNoLRNForTST()
		{
			var ediInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			ediInterchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.TST;

			IICS2MessageHeader provider = new SoapHeaderProvider(manifestHeader, ediInterchange);
			AssertNull(provider.LRN);
		}

		protected override SoapHeaderProvider GetProvider()
		{
			var ediInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "SDR";
			manifestHeader.AMA_CustomsProfile = company.GC_Code;
			return new SoapHeaderProvider(manifestHeader, ediInterchange);
		}
	}
}
