using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestsSubclassesOf(typeof(BEJobDeclarationMessageSendingObjectLookups<>))]
public abstract class BEJobDeclarationMessageSendingObjectLookupsTest<TSendingObjectLookups, TSendingAction> : TestCaseWithFactory
	where TSendingObjectLookups : BEJobDeclarationMessageSendingObjectLookups<TSendingAction>
	where TSendingAction : BEJobDeclarationMessageSendingObject
{
	public abstract void TestEntryTypeList();

	protected override void SetUp()
	{
		base.SetUp();

		action = (TSendingAction)Activator.CreateInstance(typeof(TSendingAction), Entry);
		lookups = (TSendingObjectLookups)Activator.CreateInstance(typeof(TSendingObjectLookups), action);
	}
	protected TSendingAction action;
	protected TSendingObjectLookups lookups;

	CusEntryHeader Entry
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
}
