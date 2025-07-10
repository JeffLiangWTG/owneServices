using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class JobDeclarationMessageSendingObjectAbstractFactoryTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var declaration = Factory.New<JobDeclaration>();
		var sendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when entryHeader parameter is null", () => GetNewSendingObjectFactory(null, sendingObjectParent));
			AssertExceptionThrown<ArgumentNullException>("Exception expected when jobDeclarationMessageSendingObjectParent parameter is null", () => GetNewSendingObjectFactory(declaration.CustomsEntryHeaders.AddNew(), null));
			AssertExceptionThrown<ArgumentNullException>("Exception expected when entryHeader.Declaration is null", () => GetNewSendingObjectFactory(Factory.New<CusEntryHeader>(), sendingObjectParent));
		});
	}

	public abstract void TestTryGetNewMessageSendingObject();

	protected abstract JobDeclarationMessageSendingObjectAbstractFactory GetNewSendingObjectFactory(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent sendingObjectParent);
}
