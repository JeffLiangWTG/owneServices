using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using Moq;
using NUnit.Framework;

namespace Enterprise.Licensing.Billing.Business.Testing;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
public class FeatureDataTestAttribute : TestSetupAttribute
{
	public FeatureDataTestAttribute(string featureControlCode = default, string parameters = default)
	{
		featureControlCode1 = featureControlCode;
		parameters1 = parameters;
	}

	public override void SetUp(TestCase testCase)
	{
		var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
		var mockIFeatureData = new Mock<IFeatureData>();
		mockIFeatureData.Setup(x => x.Code).Returns(featureControlCode1);
		mockIFeatureData.Setup(x => x.Parameter).Returns(parameters1);
		mockIFeatureControlManager
			.Setup(x => x.GetFeatureDataAsync(featureControlCode1, CancellationToken.None))
			.Returns(Task.FromResult(mockIFeatureData.Object));

		substituteManager = ObjectFactory.Substitute(mockIFeatureControlManager.Object);
	}

	public override void TearDown(TestCase testCase)
	{
		substituteManager.Dispose();
	}

	IDisposable substituteManager;
	readonly string featureControlCode1;
	readonly string parameters1;
}
