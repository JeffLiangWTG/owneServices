using System;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestsSubclassesOf(typeof(AESMessageHeaderProvider))]
public abstract class AESMessageHeaderProviderAbstractTest<T> : MessageHeaderProviderAbstractTest<T> where T : AESMessageHeaderProvider
{
	protected override void SetUp()
	{
		jobDeclaration = Factory.New<JobDeclaration>();
		cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		GlbStaff.CurrentUser.GS_WorkPhone = "1234567890";
		var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_Style = "C1";
		cusEntryHeader.CH_CEI_Instruction = instruction.PK;
		messageSendingAction = new ExportEntryMessageSendingAction(cusEntryHeader);
		provider = (T)Activator.CreateInstance(typeof(T), new object[] { messageSendingAction });
	}

	protected new ExportEntryMessageSendingAction messageSendingAction;
}
