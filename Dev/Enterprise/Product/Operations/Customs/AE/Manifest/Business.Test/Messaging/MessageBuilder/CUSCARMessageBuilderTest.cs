using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.AE.Registry;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.MasterFiles.Business;
using Moq;
using WTG.NUnit;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class CUSCARMessageBuilderTest : TestCaseWithFactory
{
	public void TestPopulateMessages() => CombineAssertions(() =>
	{
		var messageBuilder = new CUSCARMessageBuilder(CreateDataProviderForTest());
		var result = messageBuilder.PopulateMessages();
		var message = result.GetBuilderResults().Single().Message;
		NUnit.Framework.Assert.That(message, NUnit.Framework.Is.TypeOf<AEEDIMessage>());

		var expectedMessage = @"UNH+123H456+CUSCAR'
BGM+714+0001:2'
DTM+123:20240123:2'
LOC+123+AEXYZ'
RFF+BH:ABC123'
RFF+BM:XYZ456'
RFF+ACE:QQQ123'
NAD+CA+AE1234'
NAD+DDS+AE5678'
FTX+ABC+++XYZ 123'
GEI++NEG'
EQD++MKB0123+FCL+++5'
TSR++SVR++12'
MEA+++KGM:123.45'
SEL+SEAL123'
CNI+1'
CNT+:1'
RFF+AJW'
MOA+123:12.34:ABC'
MOA+123:12.34:XYZ'
LOC+123+AEABC'
LOC+123+AEXYZ'
GEI++28'
NAD+COX+++PAYER'
NAD+123+ABC:2++PTY1+XYZ 123+CITY+++AE'
CTA++COM'
COM+ABC XYZ.COM:EM'
NAD+123+ABC:2++PTY2+XYZ 123+CITY+++AE'
CTA++COM'
COM+ABC XYZ.COM:EM'
NAD+CN+ABC:2++PTY3+XYZ 123+CITY+++AE'
CTA++COM'
COM+ABC XYZ.COM:EM'
GID+1+123:ABC:::ABC DESC'
FTX+ABC+++XYZ 123'
MEA++WGT+KGM:12.34'
MEA++VOL+KGM:12.34'
SGP+CONT+123'
PCI++GOOD1'
CST++ID123'
LOC+27+AE'
GID+1+123:ABC:::ABC DESC'
FTX+ABC+++XYZ 123'
MEA++WGT+KGM:12.34'
MEA++VOL+KGM:12.34'
SGP+CONT+123'
PCI++GOODS2'
CST++ID123'
LOC+27+AE'
UNT+50+123H456'";

		NUnit.Framework.Assert.That(message.EM_MessageText, NUnit.Framework.Is.EqualTo(expectedMessage).Using(CustomComparers.TypeComparison));
		AssertNotNullOrEmpty("MessageInterpretation", message.EM_MessageInterpretation);
	});

	public void TestEM_GBInPopulateMessages() => CombineAssertions(() =>
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedArabEmirates;
			company1.GC_Code = "DAE";
			var proxy1 = Factory.New<OrgHeader>();
			proxy1.OH_Code = "Proxy1";
			proxy1.CustomsCodes.AddNew(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, "AABPVQA", Core.Constants.CountryCodes.UnitedArabEmirates);
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = company1.PK;
			branch1.GB_Code = "AUH";
			branch1.GB_BranchName = "AE - Branch 1";
			branch1.GB_OH_OrgProxy = proxy1.PK;
			Factory.Save();

			using (AECustomsRegistry.Instance.DefaultBranchForManifestSubmission.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "AUH"))
			{
				var messageBuilder = new CUSCARMessageBuilder(CreateDataProviderForTest());
				var result = messageBuilder.PopulateMessages();
				var message = result.GetBuilderResults().Single().Message;
				NUnit.Framework.Assert.That(message, NUnit.Framework.Is.TypeOf<AEEDIMessage>(), "This message should be AEEDIMessage");
				AssertEquals("AEEDIMessage's EM_GB value should be branch1", branch1.PK, message.EM_GB);
			}
		}
	});

	ICUSCARMessageProvider CreateDataProviderForTest()
	{
		var bill = Factory.New<AsycudaBill>();

		var mockMessageProvider = new Mock<ICUSCARMessageProvider>();
		mockMessageProvider.Setup(x => x.MessageHeader).Returns(GetMessageHeader());
		mockMessageProvider.Setup(x => x.MessageDetails).Returns(MessageBuilderTestUtils.GetMessageDetails());
		mockMessageProvider.Setup(x => x.BillIssueDate).Returns(MessageBuilderTestUtils.GetDateTimePeriod());
		mockMessageProvider.Setup(x => x.BillIssueLocation).Returns(MessageBuilderTestUtils.GetLocation("AEXYZ"));
		mockMessageProvider.Setup(x => x.References).Returns(GetReferences());
		mockMessageProvider.Setup(x => x.RelatedParties).Returns(GetParties());
		mockMessageProvider.Setup(x => x.DocumentUpdates).Returns(MessageBuilderTestUtils.GetFreeText());
		mockMessageProvider.Setup(x => x.IsNegotiable).Returns(true);
		mockMessageProvider.Setup(x => x.Payer).Returns("Payer");
		mockMessageProvider.Setup(x => x.ContainerInfos).Returns(GetTransportEquipmentInfos());
		mockMessageProvider.Setup(x => x.BillInfo).Returns(MessageBuilderTestUtils.GetConsignmentInfo());
		mockMessageProvider.Setup(x => x.Messages).Returns(new Messaging.Business.EDIMessageCollection(bill));
		mockMessageProvider.Setup(x => x.Factory).Returns(Factory);

		return mockMessageProvider.Object;
	}

	IMessageHeaderProvider GetMessageHeader()
	{
		var mockMessageHeader = new Mock<IMessageHeaderProvider>();
		mockMessageHeader.Setup(x => x.ReferenceNumber).Returns("123H456");
		mockMessageHeader.Setup(x => x.MessageType).Returns(MessageTypeList.CustomsCargoReportMessage);

		return mockMessageHeader.Object;
	}

	List<IReferenceProvider> GetReferences()
	{
		return new List<IReferenceProvider>
		{
			MessageBuilderTestUtils.GetReference("BH", "ABC123"),
			MessageBuilderTestUtils.GetReference("BM", "XYZ456"),
			MessageBuilderTestUtils.GetReference("ACE", "QQQ123")
		};
	}

	List<IPartyProvider> GetParties()
	{
		return new List<IPartyProvider>
		{
			MessageBuilderTestUtils.GetParty("CA", "AE1234"),
			MessageBuilderTestUtils.GetParty("DDS", "AE5678")
		};
	}

	List<ITransportEquipmentInfoProvider> GetTransportEquipmentInfos() => new() { MessageBuilderTestUtils.GetTransportEquipmentInfo() };
}
