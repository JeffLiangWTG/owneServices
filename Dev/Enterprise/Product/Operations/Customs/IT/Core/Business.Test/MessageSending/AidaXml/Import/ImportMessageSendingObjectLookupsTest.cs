using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class ImportMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception when parent is null", () => new ImportMessageSendingObjectLookups(null));
	}

	public void TestCancellationOrAmendmentReasonList()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var parent = new JobDeclarationMessageSendingObjectParent(declaration);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			var sendingObject = new ImportMessageSendingObject(entryHeader, parent);
			sendingObject.MessageType = "CAN";
			AssertType<Ucc6ImportCancellationReasonList>("Type",
				sendingObject.Lookups.CancellationOrAmendmentReasonList);
		}
	}

	public void TestCancellationAndAmendmentLegislativeReferenceList()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var parent = new JobDeclarationMessageSendingObjectParent(declaration);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			var sendingObject = new ImportMessageSendingObject(declaration.CustomsEntryHeaders.AddNew(), parent);
			AssertType<Ucc6ImportCancellationAndAmendmentLegislativeReferenceList>("Type", sendingObject.Lookups.CancellationAndAmendmentLegislativeReferenceList);
		}
	}
}
