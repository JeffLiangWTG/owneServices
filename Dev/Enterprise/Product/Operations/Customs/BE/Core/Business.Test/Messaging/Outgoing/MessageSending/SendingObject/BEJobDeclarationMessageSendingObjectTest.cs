using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestsSubclassesOf(typeof(BEJobDeclarationMessageSendingObject))]
public abstract class BEJobDeclarationMessageSendingObjectTest<TSendingAction> : NonPersistentBusinessObjectTestCase
	where TSendingAction : BEJobDeclarationMessageSendingObject
{
	protected override BusinessObject GetNewBusinessObject() =>
		(TSendingAction)Activator.CreateInstance(typeof(TSendingAction), Entry);

	protected CusEntryHeader Entry
	{
		get
		{
			if (entry == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				entry = declaration.CustomsEntryHeaders.AddNew();
			}
			return entry;
		}
	}
	CusEntryHeader entry;

	protected CusEntryInstruction EntryInstruction
	{
		get
		{
			if (entryInstruction == null)
			{
				entryInstruction = Entry.Declaration.CustomsEntryInstructions.AddNew();
				Entry.CH_CEI_Instruction = entryInstruction.PK;
			}
			return entryInstruction;
		}
	}
	CusEntryInstruction entryInstruction;
}
