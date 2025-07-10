using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(JobDeclarationMessageSendingObjectParent))]
sealed class JobDeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestParentDeclaration()
	{
		var objectParent = (JobDeclarationMessageSendingObjectParent)GetNewBusinessObject();
		AssertType<JobDeclaration>(objectParent.ParentDeclaration);
	}

	public void TestMessageSendingObjectProperties()
	{
		var declaration = Factory.New<JobDeclaration>();
		var actualPropertyNames = new JobDeclarationMessageSendingObjectParent(declaration).MessageSendingObjectProperties.Select(x => x.PropertyName).ToArray();

		var expectedPropertyNamesInOrder = new ZString[] { "CombinedCustomsMessageSubType", "DeclarationType", "DeclarationDescription", "EntryStatus", "MessageStatus", "CH_BGMReference" };
		AssertArrayEqualsByElements(expectedPropertyNamesInOrder, actualPropertyNames);
	}

	public void TestCustomsMessageSendingMode()
	{
		var declaration = Factory.New<JobDeclaration>();
		var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);
		AssertEquals(0, messageSendingObjectParent.SendingObjectsCollection.Count);
		AssertEquals("No MessageSendingObjects ", ZString.Empty, messageSendingObjectParent.CustomsMessageSendingMode);

		declaration.JE_MessageType = "IMP";
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader1.MergedLines.AddNew();
		entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;

		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryHeader2.MergedLines.AddNew();
		entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;

		messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);
		AssertEquals(2, messageSendingObjectParent.SendingObjectsCollection.Count);
		AssertEquals("Expected empty CustomsMessageSendingMode ", ZString.Empty, messageSendingObjectParent.CustomsMessageSendingMode);

		messageSendingObjectParent.CustomsMessageSendingMode = "XXX";
		AssertEquals("Expected XXX CustomsMessageSendingMode", "XXX", messageSendingObjectParent.CustomsMessageSendingMode);
		CombineAssertions("XXX CustomsMessageSendingMode is expected also to child objects", () =>
		{
			AssertEquals("SendingObjectsCollection[0]", "XXX", messageSendingObjectParent.SendingObjectsCollection[0].CustomsMessageSendingMode);
			AssertEquals("SendingObjectsCollection[1]", "XXX", messageSendingObjectParent.SendingObjectsCollection[1].CustomsMessageSendingMode);
		});

		var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
		var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
		entryHeader3.MergedLines.AddNew();
		entryHeader3.CH_CEI_Instruction = entryInstruction3.PK;
		var imMessageSendingObject = new IMMessageSendingObject(entryHeader3, new JobDeclarationMessageSendingObjectParent(declaration));
		messageSendingObjectParent.SendingObjectsCollection.Add(imMessageSendingObject);
		AssertEquals(3, messageSendingObjectParent.SendingObjectsCollection.Count);
		AssertEquals("Only for test purposes (this is not a real world case). When a new item is added, its CustomsMessageSendingMode is not defaulted from other child objects", ZString.Empty, messageSendingObjectParent.SendingObjectsCollection[2].CustomsMessageSendingMode);
	}

	public void TestGetNewSendingObjectFactory()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var sendingObjectParent = new JobDeclarationMessageSendingObjectParentForTest(declaration);
		AssertType<JobDeclarationMessageSendingObjectFactory>("Sending Object Factory Type", sendingObjectParent.GetNewSendingObjectFactoryExposed(entryHeader, sendingObjectParent));
	}

	public void TestMessageSendingOjectsShouldSend()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		var entryInstructionA = declaration.CustomsEntryInstructions.AddNew();
		entryInstructionA.CEI_Description = "A";

		var entryInstructionB = declaration.CustomsEntryInstructions.AddNew();
		entryInstructionB.CEI_Description = "B";

		var entryInstructionC = declaration.CustomsEntryInstructions.AddNew();
		entryInstructionC.CEI_Description = "C";

		var entryHeaderA = declaration.CustomsEntryHeaders.AddNew();
		entryHeaderA.CH_CEI_Instruction = entryInstructionA.PK;
		entryHeaderA.MergedLines.AddNew();
		var entryHeaderB = declaration.CustomsEntryHeaders.AddNew();
		entryHeaderB.CH_CEI_Instruction = entryInstructionB.PK;
		entryHeaderB.MergedLines.AddNew();
		var entryHeaderC = declaration.CustomsEntryHeaders.AddNew();
		entryHeaderC.CH_CEI_Instruction = entryInstructionC.PK;
		entryHeaderC.MergedLines.AddNew();

		var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);

		var sendingObjectCollection = messageSendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>();
		var sendingObjectA = sendingObjectCollection.SingleOrDefault(x => x.Header.EntryInstruction.CEI_Description == "A");
		var sendingObjectB = sendingObjectCollection.SingleOrDefault(x => x.Header.EntryInstruction.CEI_Description == "B");
		var sendingObjectC = sendingObjectCollection.SingleOrDefault(x => x.Header.EntryInstruction.CEI_Description == "C");

		CombineAssertions("When A selected to Send", () =>
		{
			sendingObjectA.ShouldSend = true;
			AssertEquals("Message sending object A, ShouldSend", true, sendingObjectA.ShouldSend);
			AssertEquals("Message sending object B, ShouldSend", false, sendingObjectB.ShouldSend);
			AssertEquals("Message sending object C, ShouldSend", false, sendingObjectC.ShouldSend);
		});

		CombineAssertions("When B selected to Send", () =>
		{
			sendingObjectB.ShouldSend = true;
			AssertEquals("Message sending object A, ShouldSend", false, sendingObjectA.ShouldSend);
			AssertEquals("Message sending object B, ShouldSend", true, sendingObjectB.ShouldSend);
			AssertEquals("Message sending object C, ShouldSend", false, sendingObjectC.ShouldSend);
		});

		CombineAssertions("When C and A selected to Send", () =>
		{
			sendingObjectC.ShouldSend = true;
			sendingObjectA.ShouldSend = true;
			AssertEquals("Message sending object A, ShouldSend", true, sendingObjectA.ShouldSend);
			AssertEquals("Message sending object B, ShouldSend", false, sendingObjectB.ShouldSend);
			AssertEquals("Message sending object C, ShouldSend", false, sendingObjectC.ShouldSend);
		});
	}

	public void TestMessageSendingOjectsShouldNotUpdateAfterDisposeIsCalled()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		var entryInstructionA = declaration.CustomsEntryInstructions.AddNew();
		entryInstructionA.CEI_Description = "A";

		var entryInstructionB = declaration.CustomsEntryInstructions.AddNew();
		entryInstructionB.CEI_Description = "B";

		var entryInstructionC = declaration.CustomsEntryInstructions.AddNew();
		entryInstructionC.CEI_Description = "C";

		var entryHeaderA = declaration.CustomsEntryHeaders.AddNew();
		entryHeaderA.CH_CEI_Instruction = entryInstructionA.PK;
		entryHeaderA.MergedLines.AddNew();
		var entryHeaderB = declaration.CustomsEntryHeaders.AddNew();
		entryHeaderB.CH_CEI_Instruction = entryInstructionB.PK;
		entryHeaderB.MergedLines.AddNew();
		var entryHeaderC = declaration.CustomsEntryHeaders.AddNew();
		entryHeaderC.CH_CEI_Instruction = entryInstructionC.PK;
		entryHeaderC.MergedLines.AddNew();

		JobDeclarationMessageSendingObject sendingObjectA = null;
		JobDeclarationMessageSendingObject sendingObjectB = null;
		JobDeclarationMessageSendingObject sendingObjectC = null;

		using (var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration))
		{
			var sendingObjectCollection = messageSendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>();

			sendingObjectA = sendingObjectCollection.SingleOrDefault(x => x.Header.EntryInstruction.CEI_Description == "A");
			sendingObjectB = sendingObjectCollection.SingleOrDefault(x => x.Header.EntryInstruction.CEI_Description == "B");
			sendingObjectC = sendingObjectCollection.SingleOrDefault(x => x.Header.EntryInstruction.CEI_Description == "C");

			CombineAssertions("When A selected to Send others should be unselected", () =>
			{
				sendingObjectA.ShouldSend = true;

				AssertEquals("Message sending object A, ShouldSend", true, sendingObjectA.ShouldSend);
				AssertEquals("Message sending object B, ShouldSend", false, sendingObjectB.ShouldSend);
				AssertEquals("Message sending object C, ShouldSend", false, sendingObjectC.ShouldSend);
			});

			CombineAssertions("When B selected to Send others should be unselected", () =>
			{
				sendingObjectB.ShouldSend = true;

				AssertEquals("Message sending object A, ShouldSend", false, sendingObjectA.ShouldSend);
				AssertEquals("Message sending object B, ShouldSend", true, sendingObjectB.ShouldSend);
				AssertEquals("Message sending object C, ShouldSend", false, sendingObjectC.ShouldSend);
			});

			CombineAssertions("When C and A selected to Send others should be unselected", () =>
			{
				sendingObjectC.ShouldSend = true;
				sendingObjectA.ShouldSend = true;

				AssertEquals("Message sending object A, ShouldSend", true, sendingObjectA.ShouldSend);
				AssertEquals("Message sending object B, ShouldSend", false, sendingObjectB.ShouldSend);
				AssertEquals("Message sending object C, ShouldSend", false, sendingObjectC.ShouldSend);
			});

			sendingObjectA.ShouldSend = false;
		}

		CombineAssertions("When A selected to Send others should be unselected", () =>
		{
			sendingObjectA.ShouldSend = true;

			AssertEquals("Message sending object A, ShouldSend", true, sendingObjectA.ShouldSend);
			AssertEquals("Message sending object B, ShouldSend", false, sendingObjectB.ShouldSend);
			AssertEquals("Message sending object C, ShouldSend", false, sendingObjectC.ShouldSend);
		});

		CombineAssertions("When B selected to Send others should not change", () =>
		{
			sendingObjectB.ShouldSend = true;

			AssertEquals("Message sending object A, ShouldSend", true, sendingObjectA.ShouldSend);
			AssertEquals("Message sending object B, ShouldSend", true, sendingObjectB.ShouldSend);
			AssertEquals("Message sending object C, ShouldSend", false, sendingObjectC.ShouldSend);
		});

		CombineAssertions("When C selected to Send others should not change", () =>
		{
			sendingObjectC.ShouldSend = true;

			AssertEquals("Message sending object A, ShouldSend", true, sendingObjectA.ShouldSend);
			AssertEquals("Message sending object B, ShouldSend", true, sendingObjectB.ShouldSend);
			AssertEquals("Message sending object C, ShouldSend", true, sendingObjectC.ShouldSend);
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		return new JobDeclarationMessageSendingObjectParent(declaration);
	}

	sealed class JobDeclarationMessageSendingObjectParentForTest : JobDeclarationMessageSendingObjectParent
	{
		public JobDeclarationMessageSendingObjectParentForTest(JobDeclaration declaration) : base(declaration)
		{
		}

		internal JobDeclarationMessageSendingObjectAbstractFactory GetNewSendingObjectFactoryExposed(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent sendingObjectParent) => GetNewSendingObjectFactory(entryHeader, sendingObjectParent);
	}
}
