using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(FeatureControl))]
	sealed class FeatureControlTest : ValueProviderTest
	{
		static IDisposable MockFeatureControl(string featureCode, bool enabled)
		{
			var featureControlMock = new Mock<IFeatureControlManager>();
			var featureDataMock = new Mock<IFeatureData>();
			featureControlMock.Setup(x => x.GetFeatureDataAsync(featureCode, CancellationToken.None)).Returns(Task.FromResult(enabled ? featureDataMock.Object : null));
			return ObjectFactory.Substitute(featureControlMock.Object);
		}

		public void TestIsResponsibleForReplacing()
		{
			CombineAssertions(() =>
			{
				AssertIsResponsibleForReplacing("<FeatureControl(ICEGHGCAL)>");
				AssertIsResponsibleForReplacing("< FeatureControl ( ICEGHGCAL ) >");
				AssertNotResponsibleForReplacing("<FeatureControl>");
				AssertNotResponsibleForReplacing("<FeatureControl()>");
				AssertNotResponsibleForReplacing("<AutoHeight>");
			});
		}

		public void TestReplacement()
		{
			const string testFeatureCode = "ICEGHGCAL";

			using (MockFeatureControl(testFeatureCode, false))
			{
				AssertEquals("Macro should return 'N' when feature is disabled",
					"N",
					ValueProviderToTest.GetReplacement($"<FeatureControl({testFeatureCode})>", Report));
			}

			using (MockFeatureControl(testFeatureCode, true))
			{
				AssertEquals("Macro should return 'Y' when feature is enabled",
					"Y",
					ValueProviderToTest.GetReplacement($"<FeatureControl({testFeatureCode})>", Report));
			}
		}

		public void TestEmptyFeatureCode()
		{
			AssertEquals("Macro should return 'N' when feature code is empty",
				"N",
				ValueProviderToTest.GetReplacement("<FeatureControl()>", Report));
			Assert(Report.ErrorManager.HasErrors);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new FeatureControl();
		}
	}
}
