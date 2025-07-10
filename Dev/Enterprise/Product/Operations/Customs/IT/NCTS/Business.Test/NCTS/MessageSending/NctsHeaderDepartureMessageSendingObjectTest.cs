using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.SAD;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

abstract class NctsHeaderDepartureMessageSendingObjectTest<TMessageSendingObject> : NonPersistentBusinessObjectTestCase
	where TMessageSendingObject : NctsHeaderDepartureMessageSendingObject
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When nctsHeader is null", () => GetMessageSendingObject(null));
		AssertExceptionThrown<ArgumentException>("When nctsHeader is not a departure job", () => GetMessageSendingObject(Factory.New<NctsHeader>()));
	}

	public void TestDepartureStatus()
	{
		nctsMovementHeader.BM_CustomsStatus = ZString.Empty;
		AssertEquals(nameof(messageSendingObject.DepartureStatus), ZString.Empty, messageSendingObject.DepartureStatus);

		nctsMovementHeader.BM_CustomsStatus = "XYZ";
		AssertEquals(nameof(messageSendingObject.DepartureStatus), "XYZ", messageSendingObject.DepartureStatus);
	}

	public void TestDefaultCombinedCustomsMessageSubType()
	{
		AssertEquals("Default CombinedCustomsMessageSubType", ExpectedSubType, messageSendingObject.CombinedCustomsMessageSubType);
	}

	public void TestMessageStatus()
	{
		nctsHeader.BH_MessageStatus = ZString.Empty;
		AssertEquals(nameof(messageSendingObject.MessageStatus), ZString.Empty, messageSendingObject.MessageStatus);

		nctsHeader.BH_MessageStatus = "XYZ";
		AssertEquals(nameof(messageSendingObject.MessageStatus), "XYZ", messageSendingObject.MessageStatus);
	}

	public void TestReferenceNumber()
	{
		nctsHeader.BH_JobReference = ZString.Empty;
		AssertEquals(nameof(messageSendingObject.ReferenceNumber), ZString.Empty, messageSendingObject.ReferenceNumber);

		nctsHeader.BH_JobReference = "XYZ";
		AssertEquals(nameof(messageSendingObject.ReferenceNumber), "XYZ", messageSendingObject.ReferenceNumber);
	}

	public void TestFallbackProcedure()
	{
		var iSadMessageSendingObject = (ISadMessageSendingObject)messageSendingObject;
		messageSendingObject.CustomsMessageSendingMode = ZString.Empty;
		AssertEquals($"When CustomsMessageSendingMode is empty, {nameof(iSadMessageSendingObject.FallbackProcedure)}", false, iSadMessageSendingObject.FallbackProcedure);

		messageSendingObject.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.FallbackProcedure;
		AssertEquals($"When CustomsMessageSendingMode is FBK, {nameof(iSadMessageSendingObject.FallbackProcedure)}", true, iSadMessageSendingObject.FallbackProcedure);

		messageSendingObject.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.AutomaticProcedure;
		AssertEquals($"When CustomsMessageSendingMode is ATM, {nameof(iSadMessageSendingObject.FallbackProcedure)}", false, iSadMessageSendingObject.FallbackProcedure);

		messageSendingObject.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.ManualProcedure;
		AssertEquals($"When CustomsMessageSendingMode is MAN, {nameof(iSadMessageSendingObject.FallbackProcedure)}", false, iSadMessageSendingObject.FallbackProcedure);
	}

	public void TestDeclarantTaxNumber()
	{
		var iSadMessageSendingObject = (ISadMessageSendingObject)messageSendingObject;
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		new AccountCollectionTestBuilder(company.PK.ToGuid())
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		nctsHeader.BH_CustomsProfile = ZString.Empty;
		AssertEquals($"When BH_CustomsProfile is empty, {nameof(iSadMessageSendingObject.DeclarantTaxNumber)}", ZString.Empty, iSadMessageSendingObject.DeclarantTaxNumber);

		nctsHeader.BH_CustomsProfile = "9999";
		AssertEquals($"When BH_CustomsProfile is invalid, {nameof(iSadMessageSendingObject.DeclarantTaxNumber)}", ZString.Empty, iSadMessageSendingObject.DeclarantTaxNumber);

		nctsHeader.BH_CustomsProfile = "1234";
		AssertEquals($"When BH_CustomsProfile is valid, {nameof(iSadMessageSendingObject.DeclarantTaxNumber)}", "11111111111", iSadMessageSendingObject.DeclarantTaxNumber);
	}

	public void TestISadCustomsMessageGeneratorValuesProviderMembers()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		new AccountCollectionTestBuilder(company.PK.ToGuid())
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		nctsHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "IT137100");
		nctsHeader.BH_CustomsProfile = "1234-DEC1";
		nctsHeader.Subscriber = "XXX";

		var customsMessage = (ISadOutgoingCustomsMessageGeneratorValuesProvider)messageSendingObject;
		CombineAssertions(() =>
		{
			AssertEquals("ApplicationReference", "1234:XXX:IT137100", customsMessage.GetApplicationReference());
			var fountainProvider = customsMessage.FountainProvider;
			AssertType<NctsHeaderCustomsMessageFountainProvider>("FountainProvider", fountainProvider);
			AssertSame("Cached", fountainProvider, customsMessage.FountainProvider);
			AssertEquals("SubType", ExpectedSubType, customsMessage.GetSubType());
			AssertSame("Parent", nctsHeader, customsMessage.Parent);
		});
	}

	protected abstract string ExpectedSubType { get; }

	public void TestValidation()
	{
		AssertType<NctsHeaderDepartureMessageSendingObjectValidation>("Validation Type", messageSendingObject.Validation);
	}

	public void TestIEntryMessageSendingObjectInfo()
	{
		nctsHeader.BH_JobReference = "A00";
		nctsHeader.BH_MessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
		nctsMovementHeader.BM_CustomsStatus = EU.NCTS.Business.NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;
		CombineAssertions(() =>
		{
			var entryMessageSendingObjectInfo = (IEntryMessageSendingObjectInfo)GetNewBusinessObject();
			AssertEquals("EntryStatusAllowsSending", true, entryMessageSendingObjectInfo.EntryStatusAllowsSending);
			AssertEquals("EntryReference", "A00", entryMessageSendingObjectInfo.EntryReference);
			AssertEquals("EntryMessageStatus", EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationNotSent, entryMessageSendingObjectInfo.EntryMessageStatus);
			AssertEquals("EntryCustomsStatus", EU.NCTS.Business.NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, entryMessageSendingObjectInfo.EntryCustomsStatus);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => GetMessageSendingObject(nctsHeader);

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		nctsMovementHeader = nctsHeader.MovementHeader;
		messageSendingObject = GetMessageSendingObject(nctsHeader);
	}

	protected abstract TMessageSendingObject GetMessageSendingObject(NctsHeader nctsHeader);

	protected NctsHeader nctsHeader;
	protected NctsDepartureMovementHeader nctsMovementHeader;
	protected TMessageSendingObject messageSendingObject;
}
