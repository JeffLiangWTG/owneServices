using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class ExportMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception when parent is null",
			() => new ExportMessageSendingObjectLookups(null));
	}

	public void TestCancellationOrAmendmentReasonList()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var parent = new JobDeclarationMessageSendingObjectParent(declaration);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			var sendingObject = new ExportMessageSendingObject(entryHeader, parent);
			sendingObject.MessageType = "CAN";
			AssertType<Ucc6ExportCancellationReasonList>("Type - UCC6", sendingObject.Lookups.CancellationOrAmendmentReasonList);
			AssertEquals("CancellationOrAmendmentReasonList",
				"A - Incorrect Registration\r\nB - Change of customs destination\r\nJ - Cancellation of export at the request of a party",
				sendingObject.Lookups.CancellationOrAmendmentReasonList.ElementsAsString);
		}
	}

	public void TestCancellationAndAmendmentLegislativeReferenceList_Cancellation()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var parent = new JobDeclarationMessageSendingObjectParent(declaration);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			var sendingObject = new ExportMessageSendingObject(declaration.CustomsEntryHeaders.AddNew(), parent);
			sendingObject.MessageType = "CAN";
			AssertType<Ucc6ExportCancellationLegislativeReferenceList>("Type - UCC6", sendingObject.Lookups.CancellationAndAmendmentLegislativeReferenceList);
			AssertEquals("2 - CDU Art. 174\r\n3 - RD 2015/2446 Art. 148\r\n4 - RD 2015/2446 Art. 248", sendingObject.Lookups.CancellationAndAmendmentLegislativeReferenceList.ElementsAsString);
		}
	}

	public void TestCancellationAndAmendmentLegislativeReferenceList_Amendment()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var parent = new JobDeclarationMessageSendingObjectParent(declaration);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			var sendingObject = new ExportMessageSendingObject(declaration.CustomsEntryHeaders.AddNew(), parent);
			sendingObject.MessageType = "AMD";
			AssertType<Ucc6ExportAmendmentLegislativeReferenceList>("Type - UCC6", sendingObject.Lookups.CancellationAndAmendmentLegislativeReferenceList);
			AssertEquals("1 - CDU Art. 173", sendingObject.Lookups.CancellationAndAmendmentLegislativeReferenceList.ElementsAsString);
		}
	}
}
