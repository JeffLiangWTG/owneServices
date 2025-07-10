using System;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing;

public sealed class BGMReferenceCounterTestListener : BaseTestListener
{
	public override void BeforeEachTest(DateTime startTime)
	{
		BGMReferenceCounterProvider.Instance.Value = Mock.Of<IBGMReferenceCounterProvider>(x => x.GetBGMReferenceCounter(It.IsAny<ZGuid>(), It.IsAny<int>()) == 1);
	}

	public override void AfterEachTest(DateTime endTime)
	{
		BGMReferenceCounterProvider.Instance.ResetValue();
	}
}
