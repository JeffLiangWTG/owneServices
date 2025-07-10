using System;
using Enterprise.Customs.BE.Business.CusTempStorage;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestsSubclassesOf(typeof(PNTSMessageHeaderProvider))]
public abstract class PNTSMessageHeaderProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T> where T : PNTSMessageHeaderProvider
{
	protected override T GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		sendingObject = new TemporaryStorageMessageSendingObject(temporaryStorageHeader);
		provider = (T)Activator.CreateInstance(typeof(T), new object[] { sendingObject });
	}

	protected TemporaryStorageHeader temporaryStorageHeader;
	protected TemporaryStorageMessageSendingObject sendingObject;
	protected T provider;
}
