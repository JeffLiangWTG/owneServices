using System;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.BE.Business.Testing.AESMessageHeaderProviderBaseOnlyTest;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(AESMessageHeaderProvider))]
sealed class AESMessageHeaderProviderBaseOnlyTest : AESMessageHeaderProviderAbstractTest<AESMessageHeaderProviderForTest>
{
	public void TestLanguageCode()
	{
		jobDeclaration.JE_DeclarationLanguage = "NL";
		AssertEquals("NL", Provider.LanguageCode);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("exportEntryMessageSendingAction mandatory", () => new AESMessageHeaderProviderForTest(null));
	}

	protected override string MessageType => Constants.BECMessageTypes.Outgoing.CC511C;

	public void TestMessageSender() => CombineAssertions(() =>
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = organisation.MainAddress;
		orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
		var customsCode1 = orgAddress.CustomsCodes.AddNew();
		customsCode1.OK_CodeType = "EOR";
		customsCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Belgium;
		customsCode1.OK_CustomsRegNo = "R1234";
		GlbCompany.CurrentCompany.GC_OH_OrgProxy = organisation.PK;
		Factory.Save();
		AssertEquals("AES.BE", Provider.MessageSender);
	});

	public void TestMessageSender_OverriddenRegistry()
	{
		var messageVersionRegistryCollection = new MessageVersionRegistryCollection
		{
			new MessageVersionRegistry { DomainCode = SendMessageTypes.Codes.AES, TargetSystemName = "AES.OVERRIDE" }
		};
		using (BECustomsRegistry.Instance.SenderIDs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
		{
			AssertEquals("AES.OVERRIDE", Provider.MessageSender);
		}
	}

	public void TestMessageRecipient() => CombineAssertions(() =>
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = organisation.MainAddress;
		orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
		var customsCode1 = orgAddress.CustomsCodes.AddNew();
		customsCode1.OK_CodeType = "EOR";
		customsCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Belgium;
		customsCode1.OK_CustomsRegNo = "R1234";
		GlbCompany.CurrentCompany.GC_OH_OrgProxy = organisation.PK;
		Factory.Save();
		AssertEquals("AES.BE", Provider.MessageRecipient);
	});

	public void TestMessageRecipient_OverriddenRegistry()
	{
		var messageVersionRegistryCollection = new MessageVersionRegistryCollection
		{
			new MessageVersionRegistry { DomainCode = SendMessageTypes.Codes.AES, TargetSystemName = "AES.OVERRIDE" }
		};
		using (BECustomsRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
		{
			AssertEquals("AES.OVERRIDE", Provider.MessageRecipient);
		}
	}

	public void TestPreparationDateTime()
	{
		CombineAssertions(() =>
		{
			AssertEquals("milliseconds", 0, Provider.PreparationDateTime.Millisecond);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, provider.PreparationDateTime.Kind);
		});
	}

	public void TestMessageIdentification()
	{
		AssertEquals(AESMessage.MessageNumberPlaceHolder, Provider.MessageIdentification);
	}

	public void TestCorrelationIdentifier()
	{
		var cidEntryNumber = CusEntryNumber.LoadOrCreate(cusEntryHeader, CusEntryNumberTypes.EU.CorrelationIdentifier, cusEntryHeader.CountryCode);
		cidEntryNumber.CE_EntryLineReference = "CorrelationID";
		AssertEquals("CorrelationID", Provider.CorrelationIdentifier);
	}

	public void TestExportOperation()
	{
		CombineAssertions(() =>
		{
			AssertType<ExportOperationProvider>("ExportOperation type", provider.ExportOperation);
			AssertNotNull("ExportOperation is not null", provider.ExportOperation);
		});
	}

	protected override AESMessageHeaderProviderForTest GetProvider() => provider;

	internal class AESMessageHeaderProviderForTest : AESMessageHeaderProvider
	{
		public AESMessageHeaderProviderForTest(ExportEntryMessageSendingAction messageSendingAction) : base(messageSendingAction)
		{
		}

		public override string MessageType => "CC511C";
	}
}
