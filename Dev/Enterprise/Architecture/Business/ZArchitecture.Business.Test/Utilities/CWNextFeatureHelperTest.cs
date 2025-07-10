using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CWNextFeatureHelperTest : TestCaseWithFactory
	{
		public void TestIsEnabled_IsTrue_WhenFeatureControlPresent()
		{
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CWNext, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals("Should be true when feature control exists. The JSON value does not matter.", true, CWNextFeatureHelper.IsCWNextEnabled());
		}

		public void TestIsEnabled_IsFalse_WhenMissingFromFeatureControl()
		{
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CWNext, CancellationToken.None)).Returns(Task.FromResult((IFeatureData)null));

			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals("Should be false when feature control is missing.", false, CWNextFeatureHelper.IsCWNextEnabled());
		}

		public void TestIsEnabled_WhenOverriddenByEnvironmentVariableToTrue_ToMakeLifeEasierForDevelopers()
		{
			System.Environment.SetEnvironmentVariable("CWNext_Enabled", "tRuE");

			var featureControlMock = new Mock<IFeatureControlManager>();
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CWNext, CancellationToken.None)).Throws(new InvalidOperationException("Shouldn't get this far"));

			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals("Should be true when environment variable messes with things.", true, CWNextFeatureHelper.IsCWNextEnabled());
		}

		public void TestIsEnabled_WhenOverriddenByEnvironmentVariableToFalse_ToMakeLifeEasierForDevelopers()
		{
			System.Environment.SetEnvironmentVariable("CWNext_Enabled", "fAlSE");

			var featureControlMock = new Mock<IFeatureControlManager>();
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CWNext, CancellationToken.None)).Throws(new InvalidOperationException("Shouldn't get this far"));

			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals("Should be false when environment variable messes with things.", false, CWNextFeatureHelper.IsCWNextEnabled());
		}

		public void TestIsEnabled_IsFalse_WhenForceCW1HomeScreenFlagIsSet()
		{
			using var restoreArgs = StaticFieldTestHelper.CacheAndRestoreStaticCommandLineArguments();

			var options = new Hashtable
			{
				{ "-ForceCW1HomeScreen", false }
			};
			string[] args = { "-ForceCW1HomeScreen" };

			var oldCommandLineArguments = CommandLineArguments.UsedToLaunchApplication;

			try
			{
				CommandLineArguments.UsedToLaunchApplication = new CommandLineArguments(args, options);

				var featureControlMock = new Mock<IFeatureControlManager>();
				featureControlMock
					.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CWNext, CancellationToken.None))
					.Throws(new InvalidOperationException("This should not be called when the -ForceCW1HomeScreen flag is set."));

				using (ObjectFactory.Substitute(featureControlMock.Object))
				{
					AssertEquals("Should be false when the -ForceCW1HomeScreen flag is set.", false, CWNextFeatureHelper.IsCWNextEnabled());
				}
			}
			finally
			{
				//Must reset commmand line arguments at end of test
				CommandLineArguments.UsedToLaunchApplication = oldCommandLineArguments;
			}
		}

		public void TestIsEnabled_UsesFeatureControl_WhenForceCW1HomeScreenFlagIsNotSet()
		{
			using var restoreArgs = StaticFieldTestHelper.CacheAndRestoreStaticCommandLineArguments();

			var options = new Hashtable
			{
				{ "-ForceCW1HomeScreen", false }
			};

			var oldCommandLineArguments = CommandLineArguments.UsedToLaunchApplication;

			try
			{
				CommandLineArguments.UsedToLaunchApplication = new CommandLineArguments(Array.Empty<string>(), options);

				var featureDataMock = new Mock<IFeatureData>();
				var featureControlMock = new Mock<IFeatureControlManager>();
				featureControlMock
					.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CWNext, CancellationToken.None))
					.Returns(Task.FromResult(featureDataMock.Object));

				using (ObjectFactory.Substitute(featureControlMock.Object))
				{
					bool result = CWNextFeatureHelper.IsCWNextEnabled();

					featureControlMock.Verify(
						x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CWNext, CancellationToken.None),
						Times.Once,
						"Expected GetFeatureData to be called when -ForceCW1HomeScreen flag is not set."
					);

					AssertEquals("Should be true when the feature control check falls through.", true, result);
				}
			}
			finally
			{
				//Must reset commmand line arguments at end of test
				CommandLineArguments.UsedToLaunchApplication = oldCommandLineArguments;
			}
		}

		protected override void SetUp()
		{
			CWNextFeatureHelper.ResetIsCWNextEnabled();
		}

		protected override void TearDown()
		{
			System.Environment.SetEnvironmentVariable("CWNext_Enabled", string.Empty);
			CWNextFeatureHelper.ResetIsCWNextEnabled();
		}
	}
}
