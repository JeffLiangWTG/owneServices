using System.Diagnostics;
using NUnit.Framework;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;

namespace Enterprise.ServiceManager.Shared.Testing
{
	sealed class ProcessWrapperFactoryTest
	{
		[Test]
		public void TestCreateProcessWithGivenStartInfo()
		{
			// Arrange
			var processStartInfo = new ProcessStartInfo()
			{
				RedirectStandardError = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				FileName = "NotImportant",
			};

			// Act
			var result = factory!.Create(processStartInfo, enableRaisingEvents);

			// Assert
			Assert.That(result.StartInfo.RedirectStandardOutput, Is.True);
			Assert.That(result.StartInfo.RedirectStandardError, Is.True);
			Assert.That(result.StartInfo.RedirectStandardInput, Is.True);
			Assert.That(result.StartInfo.FileName, Is.EqualTo("NotImportant"));
		}

		[Test]
		public void TestCreateProcessWithGivenParameter()
		{
			Assert.Multiple(() =>
			{
				TestCreateProcessWithGivenParameter(startInfo!, true);
				TestCreateProcessWithGivenParameter(startInfo!, false);
			});
		}

		[Test]
		public void TestCreateProcessWithDefaultValues()
		{
			// Arrange
			// Act
			var result = factory!.Create(startInfo!);

			// Assert
			Assert.That(result.EnableRaisingEvents, Is.False);
		}

		void TestCreateProcessWithGivenParameter(ProcessStartInfo startInfo, bool enableRaisingEvents)
		{
			// Arrange
			// Act
			var result = factory!.Create(startInfo, enableRaisingEvents);

			// Assert
			Assert.That(result.StartInfo, Is.EqualTo(startInfo));
			Assert.That(result.EnableRaisingEvents, Is.EqualTo(enableRaisingEvents));
		}

		[SetUp]
		public void SetUp()
		{
			startInfo = new ProcessStartInfo();
			enableRaisingEvents = true;
			factory = new ProcessWrapperFactory();
		}

		IProcessFactory? factory;
		ProcessStartInfo? startInfo;
		bool enableRaisingEvents;
	}
}
