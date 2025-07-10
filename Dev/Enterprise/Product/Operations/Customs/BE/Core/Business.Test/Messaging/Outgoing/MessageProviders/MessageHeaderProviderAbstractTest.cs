using System;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestsSubclassesOf(typeof(MessageHeaderProvider))]
public abstract class MessageHeaderProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T> where T : MessageHeaderProvider
{
	public void TestMessageType()
	{
		AssertEquals(MessageType, Provider.MessageType);
	}

	protected abstract string MessageType { get; }

	protected virtual bool IncludeEntryInstructionAndInvoiceHeader => false;

	protected override T GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
		cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		GlbStaff.CurrentUser.GS_WorkPhone = "1234567890";
		var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_Style = "C1";
		cusEntryHeader.CH_CEI_Instruction = instruction.PK;
		messageSendingAction = new BEJobDeclarationMessageSendingObjectForTesting(cusEntryHeader);
		provider = (T)Activator.CreateInstance(typeof(T), new object[] { messageSendingAction });
	}
	protected JobDeclaration jobDeclaration;
	protected CusEntryHeader cusEntryHeader;
	protected BEJobDeclarationMessageSendingObject messageSendingAction;
	protected T provider;
}
