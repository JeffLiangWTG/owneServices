using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class LockerForEditingSendingObjectTest : TestCaseWithFactory
{
	public void TestContructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when ParentSendingObject is null", () => new LockerForEditingSendingObject(null, null));
	}

	public void TestBuiltInDeclarationLockedForEditingSendingObjectCollections()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_JE = declaration.PK;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var entry1 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entry1.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine1.PK;
		entry1.CH_JE = declaration.PK;
		entry1.CH_CEI_Instruction = instruction.PK;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			AssertLockedOutgoingMessageGeneratorProvidersForSad(declaration, invoiceLine, entry1, entryLine1);

			invoiceLine.JI_CEI = ZGuid.Empty;
			declaration.DoMerge();

			var sendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var lockedOutgoingMessageGeneratorProviders = GetLockedForEditingSendingObjectCollection(sendingObjectParent);

			AssertEquals("LockedOutgoingMessageGeneratorProviders Count [Status: ERO, Entry Instruction is null]", 1, lockedOutgoingMessageGeneratorProviders.Count());
			AssertType<EmptyJobDeclarationMessageSendingObject>(lockedOutgoingMessageGeneratorProviders.ElementAt(0));
		}
	}

	public void TestInterfacedDeclarationLockedForEditingSendingObjectCollections()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_JE = declaration.PK;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var entry1 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entry1.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine1.PK;
		entry1.CH_JE = declaration.PK;
		entry1.CH_CEI_Instruction = instruction.PK;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			AssertLockedOutgoingMessageGeneratorProvidersForSad(declaration, invoiceLine, entry1, entryLine1);
		}
	}

	public void TestLockedForEditingSendingObjectCollections()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entry = declaration.CustomsEntryHeaders.AddNew();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_JE = declaration.PK;
		entry.CH_CEI_Instruction = instruction.PK;

		declaration.JE_MessageType = "IMP";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			var sendingObjectParent = new Ucc6JobDeclarationMessageSendingObjectParent(declaration);

			AssertHasNoLockedOutgoingMessageGeneratorProviders("Empty", sendingObjectParent);

			entry.CH_Status = "AWO";
			AssertLockedOutgoingMessageGeneratorProviders<ImportMessageSendingObject>("AWO", sendingObjectParent);

			entry.CH_Status = "ACO";
			entry.CH_EntryStatus = "REG";
			AssertLockedOutgoingMessageGeneratorProviders<ImportMessageSendingObject>("REG", sendingObjectParent);

			entry.CH_Status = "";
			entry.CH_EntryStatus = "DEP";
			AssertHasNoLockedOutgoingMessageGeneratorProviders("DEP", sendingObjectParent);
		}
	}

	public void TestLockedForEditingSendingObjectCollections_WhenDeclarationsAreInDifferentFactory()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entry1 = declaration.CustomsEntryHeaders.AddNew();
		entry1.CH_CEI_Instruction = declaration.CustomsEntryInstructions.AddNew().PK;
		entry1.MergedLines.AddNew();
		var entry2 = declaration.CustomsEntryHeaders.AddNew();
		entry2.CH_CEI_Instruction = declaration.CustomsEntryInstructions.AddNew().PK;
		entry2.MergedLines.AddNew();

		entry1.CH_EntryStatus = "ECC";
		entry2.CH_EntryStatus = "ECC";
		Factory.Save();

		var declarationInDifferentFactory = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declarationInDifferentFactory, configurationValue: true))
		{
			var sendingObjectParent = new Ucc6JobDeclarationMessageSendingObjectParent(declarationInDifferentFactory);

			var lockerForEditing = new LockerForEditingSendingObject(sendingObjectParent, declaration);
			var lockedProviders = lockerForEditing.GetLockedOutgoingMessageGeneratorProviders();
			AssertEquals("When both Entries in current declaration are locked for edit", 2, lockedProviders.Count());
			AssertArrayEqualsByElements("Locked providers", new[] { entry1.PK, entry2.PK }, lockedProviders.Select(x => x.Parent.PK).ToArray());

			entry1.CH_EntryStatus = "AMG";
			lockerForEditing = new LockerForEditingSendingObject(sendingObjectParent, declaration);
			lockedProviders = lockerForEditing.GetLockedOutgoingMessageGeneratorProviders();
			AssertEquals("When only one Entry in current declaration is locked for edit", 1, lockerForEditing.GetLockedOutgoingMessageGeneratorProviders().Count());
			AssertEquals("Locked provider", entry2.PK, lockedProviders.ElementAt(0).Parent.PK);
		}
	}

	void AssertLockedOutgoingMessageGeneratorProvidersForSad(JobDeclaration declaration, JobComInvoiceLine invoiceLine, CusEntryHeader entry1, CusEntryLine entryLine1)
	{
		var sendingObject = new JobDeclarationMessageSendingObjectParent(declaration);

		CombineAssertions(() =>
		{
			AssertHasNoLockedOutgoingMessageGeneratorProviders("Empty", sendingObject);

			entry1.CH_Status = "AWO";
			declaration.JE_MessageType = "IMP";
			AssertLockedOutgoingMessageGeneratorProviders<IMMessageSendingObject>("AWO", sendingObject);

			entry1.CH_Status = "ERO";
			AssertHasNoLockedOutgoingMessageGeneratorProviders("ERO", sendingObject);

			entry1.CH_Status = "ACO";
			entry1.CH_EntryStatus = "";
			AssertLockedOutgoingMessageGeneratorProviders<IMMessageSendingObject>("ACO", sendingObject);

			entry1.CH_Status = "ACO";
			entry1.CH_EntryStatus = "REG";
			AssertLockedOutgoingMessageGeneratorProviders<IMMessageSendingObject>("REG", sendingObject);

			entry1.CH_EntryStatus = "ICC";
			AssertLockedOutgoingMessageGeneratorProviders<IMMessageSendingObject>("ICC", sendingObject);

			var previousDocument1 = invoiceLine.PreviousDocuments.AddNew();
			previousDocument1.CSI_Procedure = "A3";
			var previousDocument2 = invoiceLine.PreviousDocuments.AddNew();
			previousDocument2.CSI_Procedure = "MRN";
			declaration.ResetApportionedPreviousDocuments();

			entry1.CH_EntryStatus = "NBR";
			entryLine1.ZG_NBStatus = "NBR";
			AssertImLockedOutgoingMessageGeneratorProvidersAndNbMessages("NBR", expectedNbMessageCount: 0);

			entryLine1.ZG_NBStatus = "NBA";
			AssertImLockedOutgoingMessageGeneratorProvidersAndNbMessages("NBA", expectedNbMessageCount: 1);

			entryLine1.ZG_NBStatus = "NBS";
			AssertImLockedOutgoingMessageGeneratorProvidersAndNbMessages("NBS", expectedNbMessageCount: 1);

			declaration.JE_MessageType = "EXP";
			entry1.CH_Status = "AWO";
			var lockedOutgoingMessageGeneratorProviders = GetLockedForEditingSendingObjectCollection(sendingObject);
			AssertEquals("[Status: ERO], LockedOutgoingMessageGeneratorProviders Count", 1, lockedOutgoingMessageGeneratorProviders.Count());
		});

		void AssertImLockedOutgoingMessageGeneratorProvidersAndNbMessages(string status, int expectedNbMessageCount)
		{
			var lockedOutgoingMessageGeneratorProviders = GetLockedForEditingSendingObjectCollection(sendingObject);
			AssertEquals($"[EntryStatus: {status}], LockedForEditingSendingObjectCollection Count", 1, lockedOutgoingMessageGeneratorProviders.Count());

			AssertType<IMMessageSendingObject>($"[EntryStatus: {status}], Element of LockedForEditingSendingObjectCollection", lockedOutgoingMessageGeneratorProviders.Single());
			var imMessageSendingObject = lockedOutgoingMessageGeneratorProviders.Cast<IMMessageSendingObject>().Single();
			AssertEquals("NBMessages count in IMMessageChangedStatusDeterminerSendingObject", expectedNbMessageCount, imMessageSendingObject.NBMessages.Count());
		}
	}

	void AssertLockedOutgoingMessageGeneratorProviders<T>(string status, JobDeclarationMessageSendingObjectParent sendingObject)
	{
		var lockedOutgoingMessageGeneratorProviders = GetLockedForEditingSendingObjectCollection(sendingObject);
		AssertEquals($"[Status: {status}], LockedOutgoingMessageGeneratorProviders Count", 1, lockedOutgoingMessageGeneratorProviders.Count());
		AssertType<T>($"[Status: {status}], Element of LockedForEditingSendingObjectCollection", lockedOutgoingMessageGeneratorProviders.Single());
	}

	void AssertHasNoLockedOutgoingMessageGeneratorProviders(string status, JobDeclarationMessageSendingObjectParent sendingObject)
	{
		var lockedOutgoingMessageGeneratorProviders = GetLockedForEditingSendingObjectCollection(sendingObject);
		AssertEquals($"[Status: {status}], LockedOutgoingMessageGeneratorProviders Count", 0, lockedOutgoingMessageGeneratorProviders.Count());
	}

	IEnumerable<IOutgoingCustomsMessageGeneratorValuesProvider> GetLockedForEditingSendingObjectCollection(JobDeclarationMessageSendingObjectParent sendingObjectParent)
	{
		var lockerForEditingSendingObject = new LockerForEditingSendingObject(sendingObjectParent, sendingObjectParent.ParentDeclaration);
		return lockerForEditingSendingObject.GetLockedOutgoingMessageGeneratorProviders();
	}
}
