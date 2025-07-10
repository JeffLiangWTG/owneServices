using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class CommunicationMetaDataWrapperTest : DataProviderTestCase<CommunicationMetaDataWrapper>
{
	public void TestApplicationReferenceId()
	{
		AssertEquals("Communication Meta Data Wrapper - ApplicationReferenceID", "EH00001", wrapper.ApplicationReferenceId);
	}

	public void TestCommunicationsAgreementID()
	{
		AssertEquals("Communication Meta Data Wrapper - CommunicationsAgreementID", NLEDIMessage.MessageNumberPlaceHolder, wrapper.CommunicationsAgreementID);
	}

	[TestDate(2021, 12, 07, 14, 05, 26)]
	public void TestPreparationDateTime()
	{
		AssertEquals("Communication Meta Data Wrapper - PreparationDateTime", "20211207140526Z", wrapper.PreparationDateTime);
	}

	public void TestRecipientID()
	{
		var messageVersionRegistryCollection = new MessageVersionRegistryCollection
		{
			new MessageVersionRegistry { DomainCode = MessageVersionRegistry.DMSDomainCode, TargetSystemName = "TESTOVERRIDE" }
		};
		using (NLCustomsRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
		{
			AssertEquals("Communication Meta Data Wrapper - RecipientID", "TESTOVERRIDE", wrapper.RecipientID);
		}
	}

	public void TestSenderID()
	{
		var currentCompanyOrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		var senderIDCollection = new SenderInfoCollection();
		var senderID = senderIDCollection.AddNew();
		senderID.OrganizationPK = currentCompanyOrgHeader;
		senderID.SenderID = "TestSenderID";
		senderID.DefaultSenderID = true;

		using (NLCustomsRegistry.Instance.SenderIDs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, senderIDCollection))
		{
			AssertEquals("Communication Meta Data Wrapper - SenderID", "TestSenderID", wrapper.SenderID);
		}
	}

	[TestDate(2021, 10, 8, 14, 15, 30)]
	public void TestCommunicationMetaData()
	{
		CombineAssertions(() =>
		{
			AssertEquals("CommunicationsMetaData ApplicationReferenceID", "EH00001", wrapper.ApplicationReferenceId);
			AssertEquals("CommunicationMetaData CommunicationsAgreementID must be filled", NLEDIMessage.MessageNumberPlaceHolder, wrapper.CommunicationsAgreementID);
			AssertEquals("CommunicationsMetaData PreparationDateTime", "20211008141530Z", wrapper.PreparationDateTime);
			AssertEquals("CommunicationsMetaData Recipient ID", "DMS.NL", wrapper.RecipientID);
		});
	}

	protected override CommunicationMetaDataWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "EH00001";
		wrapper = new CommunicationMetaDataWrapper(entryHeader);
	}

	CommunicationMetaDataWrapper wrapper;
}
