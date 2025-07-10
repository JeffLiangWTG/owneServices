using System;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestsSubclassesOf(typeof(BEJobDeclarationMessageSendingObjectValidation<>))]
public abstract class BEJobDeclarationMessageSendingObjectValidationTest<TSendingAction> : MessageSendingObjectValidationTest
	where TSendingAction : BEJobDeclarationMessageSendingObject
{
	public void TestValidateTypeOfEntry_Mandatory()
	{
		const string message = "Type of Entry is mandatory.";
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var entry = declaration.CustomsEntryHeaders.AddNew();
		entry.CH_CEI_Instruction = instruction.PK;

		var testItem = (TSendingAction)Activator.CreateInstance(typeof(TSendingAction), entry);
		testItem.TypeOfEntry = "";
		testItem.ShouldSend = true;
		AssertHasError("is empty", testItem.TypeOfEntryInfo, message);

		testItem = (TSendingAction)Activator.CreateInstance(typeof(TSendingAction), entry);
		testItem.TypeOfEntry = "fil";
		testItem.ShouldSend = true;
		AssertNoError("is filled in", testItem.TypeOfEntryInfo, message);

		testItem.TypeOfEntry = ZString.Empty;
		AssertHasError("is empty", testItem.TypeOfEntryInfo, message);
	}

	public abstract void TestValidateTypeOfEntry_Valid();
}
