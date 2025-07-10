using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class CUSCARMessageProviderTest : Customs.Business.Testing.DataProviderTestCase<CUSCARMessageProvider>
{
	[ExpectNoExceptions]
	public void TestMessageHeader() => CombineAssertions(() =>
	{
		var provider = GetProvider();
		var messageHeaderProvider = provider.MessageHeader;
		NUnit.Framework.Assert.That(messageHeaderProvider, Is.TypeOf<CUSCARMessageHeaderProvider>());
		NUnit.Framework.Assert.That(messageHeaderProvider.MessageType, Is.EqualTo(MessageTypeList.CustomsCargoReportMessage.ToString()));
	});

	[ExpectNoExceptions]
	public void TestMessageDetails() => NUnit.Framework.Assert.That(GetProvider().MessageDetails, Is.TypeOf<MessageDetailsProvider>());

	[ExpectNoExceptions]
	public void TestBillIssueDate() => CombineAssertions(() =>
	{
		NUnit.Framework.Assert.That(GetProvider().BillIssueDate, Is.Null, "Issue Date not set");

		header.AMA_MasterBillIssueDate = new ZDate(2024, 07, 01);
		var issueDate = GetProvider().BillIssueDate;
		NUnit.Framework.Assert.That(issueDate.DateTimePeriodFunctionCode, Is.EqualTo(DateOrTimeOrPeriodFunctionCodeQualifierList.BillOfLadingDate.ToString()), "Function Code");
		NUnit.Framework.Assert.That(issueDate.DateTimePeriodText, Is.EqualTo("20240701"), "Text");
		NUnit.Framework.Assert.That(issueDate.DateTimePeriodFormat, Is.EqualTo(DateOrTimeOrPeriodFormatCodeList.Ccyymmdd.ToString()), "Format");
	});

	[ExpectNoExceptions]
	public void TestBillIssueLocation() => CombineAssertions(() =>
	{
		header.AMA_RL_NKPortOfLoading = "AEABC";
		var issueLocation = GetProvider().BillIssueLocation;
		NUnit.Framework.Assert.That(issueLocation.LocationFunctionCode, Is.EqualTo(LocationFunctionCodeQualifierList.PlaceOfDocumentIssue.ToString()), "Code");
		NUnit.Framework.Assert.That(issueLocation.LocationIdentifier, Is.EqualTo("AEABC"), "Identifier");
	});

	[ExpectNoExceptions]
	public void TestReferences() => CombineAssertions(() =>
	{
		header.AMA_MasterBill = "MBL123";
		bill.ABL_BillNumber = "HBL456";
		bill.ABL_SplitBillNumber = "ABC123";
		bill.ABL_BolType = ShipmentTypes.StandardHouse;
		var references = GetProvider().References;
		NUnit.Framework.Assert.That(references.Count, Is.EqualTo(3), "References count");
		AssertReference(ReferenceCodeQualifierList.HouseBillOfLadingNumber.ToString(), "MBL123");
		AssertReference(ReferenceCodeQualifierList.BillOfLadingNumber.ToString(), "HBL456");
		AssertReference(ReferenceCodeQualifierList.RelatedDocumentNumber.ToString(), "ABC123");

		bill.ABL_BolType = ShipmentTypes.CoLoadMaster;
		bill.ABL_SplitBillNumber = ZString.Empty;
		references = GetProvider().References;
		AssertReference(ReferenceCodeQualifierList.BillOfLadingNumber, "HBL456");
		NUnit.Framework.Assert.That(references.Count, Is.EqualTo(2), "References count");
		return;

		void AssertReference(string code, string identifier)
		{
			var reference = references.First(x => x.ReferenceIdentifier == identifier);
			NUnit.Framework.Assert.That(reference.ReferenceCode, Is.EqualTo(code), $"{identifier} Code");
		}
	});

	[ExpectNoExceptions]
	public void TestRelatedParties() => CombineAssertions(() =>
	{
		header.AMA_OA_Carrier = CreateOrgWithAddress("ABC").PK;
		bill.ABL_OA_Forwarder = CreateOrgWithAddress("XYZ").PK;

		var relatedParties = GetProvider().RelatedParties;
		AssertParty(PartyFunctionCodeQualifierList.Carrier.ToString(), "ABC1234");
		Assert("no CLD bills", !relatedParties.Any(p => p.PartyFunctionCode == PartyFunctionCodeQualifierList.ConsigneesFreightForwarder));

		bill.ABL_BolType = ShipmentTypes.CoLoadMaster;
		relatedParties = GetProvider().RelatedParties;
		AssertParty(PartyFunctionCodeQualifierList.ConsigneesFreightForwarder.ToString(), "XYZ1234");

		bill.ABL_BolType = ShipmentTypes.StandardHouse;
		relatedParties = GetProvider().RelatedParties;
		Assert("STD bills", !relatedParties.Any(p => p.PartyFunctionCode == PartyFunctionCodeQualifierList.ConsigneesFreightForwarder));
		return;

		void AssertParty(string code, string identifier)
		{
			var party = relatedParties.First(x => x.PartyIdentifier == identifier);
			NUnit.Framework.Assert.That(party.PartyFunctionCode, Is.EqualTo(code), $"{identifier} Code");
		}

		OrgAddress CreateOrgWithAddress(ZString orgCode)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = orgCode;
			CreateMPCICodeForOrg(org);
			return org.Addresses.AddNew();
		}
	});

	[ExpectNoExceptions]
	public void TestDocumentUpdates() => CombineAssertions(() =>
	{
		messageItem.SubjectCode = "ABC";
		var documentUpdates = GetProvider().DocumentUpdates;
		NUnit.Framework.Assert.That(documentUpdates,
			Is.Not.EqualTo(default(IFreeTextProvider)),
			"Subject Code set - should not be [null]");
		NUnit.Framework.Assert.That(documentUpdates, Is.TypeOf<FreeTextProvider>(), "DocumentUpdates type");

		messageItem.SubjectCode = ZString.Empty;
		messageItem.Subject = "XYZ";
		NUnit.Framework.Assert.That(GetProvider().DocumentUpdates,
			Is.Not.EqualTo(default(IFreeTextProvider)),
			"Subject is set - should not be [null]");

		messageItem.Subject = ZString.Empty;
		NUnit.Framework.Assert.That(GetProvider().DocumentUpdates, Is.Null, "DocumentUpdates not needed");
	});

	[ExpectNoExceptions]
	public void TestIsNegotiable() => CombineAssertions(() =>
	{
		var isNegotiable = GetProvider().IsNegotiable;
		NUnit.Framework.Assert.That(isNegotiable, Is.EqualTo(false), "Negotiable? not set");

		bill.Negotiable = NegotiableList.Codes.No;
		isNegotiable = GetProvider().IsNegotiable;
		NUnit.Framework.Assert.That(isNegotiable, Is.EqualTo(false), "Negotiable? is no");

		bill.Negotiable = NegotiableList.Codes.Yes;
		isNegotiable = GetProvider().IsNegotiable;
		NUnit.Framework.Assert.That(isNegotiable, Is.EqualTo(true), "Negotiable? is yes");
	});

	[ExpectNoExceptions]
	public void TestPayer() => CombineAssertions(() =>
	{
		var payer = GetProvider().Payer;
		NUnit.Framework.Assert.That(payer, Is.EqualTo(string.Empty), "Payer not set");

		bill.Payer = "ABC123";
		payer = GetProvider().Payer;
		NUnit.Framework.Assert.That(payer, Is.EqualTo("ABC123"), "Payer set");
	});

	[ExpectNoExceptions]
	public void TestContainerInfos() => CombineAssertions(() =>
	{
		CreateLinkedContainer();
		CreateLinkedContainer();
		var containerInfos = GetProvider().ContainerInfos;
		NUnit.Framework.Assert.That(containerInfos.Count, Is.EqualTo(2), "ContainerInfos Count");
		NUnit.Framework.Assert.That(containerInfos.All(x => x is TransportEquipmentInfoProvider container), Is.True,
			"ContainerInfo Type");

		var packUnderContainer1 = bill.Packs.AddNew();
		packUnderContainer1.ContainerPK = header.Containers[0].PK;
		containerInfos = GetProvider().ContainerInfos;
		NUnit.Framework.Assert.That(containerInfos.Count, Is.EqualTo(2), "ContainerInfos Count");
		return;

		void CreateLinkedContainer()
		{
			var pack = bill.Packs.AddNew();
			var container = header.Containers.AddNew();
			pack.ContainerPK = container.PK;
		}
	});

	[ExpectNoExceptions]
	public void TestGetContainerInfosWhenContainerIsNull()
	{
		NUnit.Framework.Assert.That(GetProvider().ContainerInfos.Count, Is.EqualTo(0), "ContainerInfos Count should be 0.");
	}

	[ExpectNoExceptions]
	public void TestBillInfo() => NUnit.Framework.Assert.That(GetProvider().BillInfo, Is.TypeOf<ConsignmentInfoProvider>());

	protected override CUSCARMessageProvider GetProvider()
	{
		return new CUSCARMessageProvider(messageItem);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<AsycudaManifestHeader>();
		bill = header.Bills.AddNew();
		var messageChooser = new MessageChooser(header, new[] { bill }, false);
		messageItem = new MessageChooserItem(messageChooser, bill, false);
	}
	AsycudaManifestHeader header;
	AsycudaBill bill;
	MessageChooserItem messageItem;

	void CreateMPCICodeForOrg(OrgHeader orgHeader)
	{
		var code = orgHeader.CustomsCodes.AddNew();
		code.OK_CodeType = OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber;
		code.OK_RN_NKCodeCountry = CountryCodes.UnitedArabEmirates;
		code.OK_CustomsRegNo = orgHeader.OH_Code + "1234";
	}
}
