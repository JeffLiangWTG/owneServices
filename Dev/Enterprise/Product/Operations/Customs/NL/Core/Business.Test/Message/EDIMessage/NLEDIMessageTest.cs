using System.Reflection;
using CargoWise.Customs.NL.MessageDefinitions.DMS.Response_1p30;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(NLEDIMessage))]
sealed class NLEDIMessageTest : EDIMessageTest
{
	public void TestValidation()
	{
		var message = Factory.New<NLEDIMessage>();
		AssertType<NLEDIMessageValidation>(message.Validation);
	}

	public void TestMessageDefaults()
	{
		AssertEquals("EM_ApplicationCode", EDIInterchange.ApplicationCodes.NLCustoms, message.EM_ApplicationCode);
	}

	public void TestShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride()
	{
		var nlEdiMessage = Factory.New<NLEDIMessage>();
		var overrideValue = nlEdiMessage.GetType().BaseType
			.GetMethod("ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride", BindingFlags.NonPublic | BindingFlags.Instance)
			.Invoke(nlEdiMessage, System.Array.Empty<object>());
		AssertEquals(true, overrideValue);
	}

	public void TestMessageNum()
	{
		CombineAssertions(() =>
		{
			Factory.Save();
			AssertEquals("1st Generated Number", "00000000000001", message.EM_MessageNum);

			var message2 = Factory.New<NLEDIMessage>();
			Factory.Save();
			AssertEquals("2nd Generated Number", "00000000000002", message2.EM_MessageNum);

			var message3 = Factory.New<NLEDIMessage>();
			message3.EM_MessageNum = "00000000000100";
			Factory.Save();
			AssertEquals("2nd Generated Number", "00000000000100", message3.EM_MessageNum);
		});
	}

	public void TestMessageNumberPlaceHolderOverride()
	{
		var message = Factory.New<NLEDIMessage>();
		var messageNumberPlaceHolderOverride = message.GetType().GetProperty("MessageNumberPlaceHolderOverride", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(message);
		AssertEquals("&lt;&lt;MSGNO PLACEHOLDER&gt;&gt;", messageNumberPlaceHolderOverride);
	}

	public void TestEntryStatusFields()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands, "Netherlands");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs status");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "TS1", "TEST1 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "TS2", "TEST2 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		CombineAssertions(() =>
		{
			var entryStatusProperty = typeof(NLEDIMessage).GetField("entryStatus", BindingFlags.NonPublic | BindingFlags.Instance);

			var nlEdiMessage = Factory.New<NLEDIMessage>();
			entryStatusProperty.SetValue(nlEdiMessage, string.Empty);
			AssertEquals(ZString.Empty, nlEdiMessage.EntryStatus);
			AssertEquals(ZString.Empty, nlEdiMessage.EntryStatusDescription);

			var nlEDIMessageTS1 = Factory.New<NLEDIMessage>();
			entryStatusProperty.SetValue(nlEDIMessageTS1, "TS1");
			AssertEquals("TS1", nlEDIMessageTS1.EntryStatus);
			AssertEquals("TEST1 DES", nlEDIMessageTS1.EntryStatusDescription);

			var nlEDIMessageTS2 = Factory.New<NLEDIMessage>();
			entryStatusProperty.SetValue(nlEDIMessageTS2, "TS2");
			AssertEquals("TS2", nlEDIMessageTS2.EntryStatus);
			AssertEquals("TEST2 DES", nlEDIMessageTS2.EntryStatusDescription);
		});
	}

	public void TestSendersReference()
	{
		var getSendersReference = typeof(NLEDIMessage).GetMethod("GetSendersReference", BindingFlags.NonPublic | BindingFlags.Instance);
		var nlEdiMessage = Factory.New<NLEDIMessage>();
		nlEdiMessage.EM_MessageNum = "0000123456";
		var sendersReference = getSendersReference.Invoke(nlEdiMessage, System.Array.Empty<object>());
		AssertEquals("0000123456", sendersReference);
	}

	public void TestTypeDecider()
	{
		AssertType<EDIMessageTypeDecider>(NLEDIMessage.TypeDecider);
	}

	public void TestCustomsMessageRemarks()
	{
		var nlEdiMessage = (NLEDIMessage)GetNewBusinessObject();

		AssertEquals("NLEDIMessage.Notes.HasNotes", false, nlEdiMessage.Notes.HasNotes);
		AssertEquals("NLEDIMessage.CustomsMessageRemarks", "", nlEdiMessage.CustomsMessageRemarks);
		AssertEquals("NLEDIMessage.Notes.HasNotes", false, nlEdiMessage.Notes.HasNotes);

		nlEdiMessage.CustomsMessageRemarks = "YAYAYA";
		AssertEquals("NLEDIMessage.CustomsMessageRemarks", "YAYAYA", nlEdiMessage.CustomsMessageRemarks);
		AssertEquals("NLEDIMessage.Notes.HasNotes", true, nlEdiMessage.Notes.HasNotes);

		nlEdiMessage.CustomsMessageRemarks = "NANANA";
		AssertEquals("NLEDIMessage.CustomsMessageRemarks", "NANANA", nlEdiMessage.CustomsMessageRemarks);
		AssertEquals("NLEDIMessage.Notes.HasNotes", true, nlEdiMessage.Notes.HasNotes);

		nlEdiMessage.CustomsMessageRemarks = "";
		AssertEquals("NLEDIMessage.Notes.HasNotes", false, nlEdiMessage.Notes.HasNotes);
		AssertEquals("NLEDIMessage.CustomsMessageRemarks", "", nlEdiMessage.CustomsMessageRemarks);
		AssertEquals("NLEDIMessage.Notes.HasNotes", false, nlEdiMessage.Notes.HasNotes);

		nlEdiMessage.CustomsMessageRemarks = "EXT";
		AssertEquals("NLEDIMessage.StatementType for 'EXT'", "EXT", nlEdiMessage.StatementType);
		AssertEquals("NLEDIMessage.StatementDescription for 'EXT'", ZString.Empty, nlEdiMessage.StatementDescription);

		nlEdiMessage.CustomsMessageRemarks = "EXT|Reference";
		AssertEquals("NLEDIMessage.StatementType for 'EXT|Reference'", "EXT", nlEdiMessage.StatementType);
		AssertEquals("NLEDIMessage.StatementDescription for 'EXT|Reference'", "Reference", nlEdiMessage.StatementDescription);
	}

	protected override void SetUp()
	{
		base.SetUp();
		message = Factory.New<NLEDIMessage>();
	}

	NLEDIMessage message;
}

[TestedType(typeof(NLEDIMessage<MetaData>))]
sealed class NLEDIMessageGenericTests : EnterpriseBusinessObjectTestCase
{
}
