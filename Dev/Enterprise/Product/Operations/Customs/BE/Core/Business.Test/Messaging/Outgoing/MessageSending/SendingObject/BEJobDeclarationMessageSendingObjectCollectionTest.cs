using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(BEJobDeclarationMessageSendingObjectCollection<>))]
public abstract class BEJobDeclarationMessageSendingObjectCollectionTest<TSendingParent, TSendingObjectCollection, TSendingAction> : NonPersistentBusinessObjectCollectionTestCase<TSendingObjectCollection>
	where TSendingParent : BEJobDeclarationMessageSendingObjectParent<TSendingObjectCollection, TSendingAction>
	where TSendingObjectCollection : BEJobDeclarationMessageSendingObjectCollection<TSendingAction>
	where TSendingAction : BEJobDeclarationMessageSendingObject
{
	protected abstract Type ExpectedSendingObjectCollectionType { get; }

	public void TestGetSendingAction()
	{
		declaration.CustomsEntryHeaders.AddNew();
		var coll = GetCollectionToTest();
		coll.PopulateElements();
		AssertEquals(ExpectedSendingObjectCollectionType, coll[0].GetType());
	}

	public void TestAllowNew()
	{
		Assert(!GetCollectionToTest().AllowNew);
	}

	protected override TSendingObjectCollection GetCollectionToTest()
	{
		return (TSendingObjectCollection)Activator.CreateInstance(typeof(TSendingObjectCollection), declaration.ActiveEntryHeaders, Factory);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return (BusinessObject)Activator.CreateInstance(typeof(TSendingAction), header);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		header = declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader header;
}
