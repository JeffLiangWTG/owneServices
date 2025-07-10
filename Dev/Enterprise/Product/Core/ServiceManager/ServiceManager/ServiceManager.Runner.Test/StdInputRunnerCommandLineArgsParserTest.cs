using System;
using System.IO;
using System.Threading;
using Enterprise.ServiceManager.Runner;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test
{
	class StdInputRunnerCommandLineArgsParserTest
	{
		[SetUp]
		public void SetUp()
		{
			Console.SetOut(TextWriter.Null);
		}

		[TearDown]
		public void TearDown()
		{
			Console.SetOut(Console.Out);
		}

		[Test]
		public void TestInvalidArguments()
		{
			var configProvider = new Mock<IClientHostedServiceAttributeProvider>();
			var tstConfig = new Mock<IHostedServiceAttribute>();
			tstConfig.Setup(m => m.TypeAssemblyName).Returns("TestAssembly");
			configProvider.Setup(m => m.GetClientHostedServiceAttribute("TST")).Returns(tstConfig.Object);
			var parser = new StdInputRunnerCommandLineArgsParser(configProvider.Object);

			var exception = Assert.Throws<ArgumentException>(() => parser.Parse(string.Empty), "Expected exception not thrown");
			Assert.That(exception.Message, Is.EqualTo("No arguments specified."));

			exception = Assert.Throws<ArgumentException>(() => parser.Parse(StdInputRunnerCommandLineArgsParser.ProcessCommands.Run), "Expected exception not thrown");
			Assert.That(exception.Message, Is.EqualTo("No code specified."));

			exception = Assert.Throws<ArgumentException>(() => parser.Parse($"{StdInputRunnerCommandLineArgsParser.ProcessCommands.Run} BLA"), "Expected exception not thrown");
			Assert.That(exception.Message, Is.EqualTo("Cannot determine assembly name from code 'BLA'. (Make sure you have executed AssemblyMetaDataExtractor.exe, e.g. via QGL)"));

			exception = Assert.Throws<ArgumentException>(() => parser.Parse("BLA"), "Expected exception not thrown");
			Assert.That(exception.Message, Is.EqualTo("Cannot determine assembly name from code 'BLA'. (Make sure you have executed AssemblyMetaDataExtractor.exe, e.g. via QGL)"));
		}

		[Test]
		public void TestValidArguments()
		{
			var configProvider = new Mock<IClientHostedServiceAttributeProvider>();
			var tstConfig = new Mock<IHostedServiceAttribute>();
			tstConfig.Setup(m => m.TypeAssemblyName).Returns("TestAssembly");
			configProvider.Setup(m => m.GetClientHostedServiceAttribute("TST")).Returns(tstConfig.Object);
			var parser = new StdInputRunnerCommandLineArgsParser(configProvider.Object);

			var commandInfo = parser.Parse(StdInputRunnerCommandLineArgsParser.ProcessCommands.Stop);
			Assert.That(commandInfo, Is.InstanceOf<IStopCommandInfo>());

			commandInfo = parser.Parse(StdInputRunnerCommandLineArgsParser.ProcessCommands.Exit);
			Assert.That(commandInfo, Is.InstanceOf<IStopCommandInfo>());

			commandInfo = parser.Parse($"{StdInputRunnerCommandLineArgsParser.ProcessCommands.Run} TST");
			Assert.That(commandInfo, Is.InstanceOf<IRunCommandInfo>());
			Assert.That(((IRunCommandInfo)commandInfo).Code, Is.EqualTo("TST"));
			Assert.That(((IRunCommandInfo)commandInfo).AssemblyName, Is.EqualTo("TestAssembly"));

			commandInfo = parser.Parse("TST");
			Assert.That(commandInfo, Is.InstanceOf<IRunCommandInfo>());
			Assert.That(((IRunCommandInfo)commandInfo).Code, Is.EqualTo("TST"));
			Assert.That(((IRunCommandInfo)commandInfo).AssemblyName, Is.EqualTo("TestAssembly"));
		}

		[Test]
		public void TestNoConfigurationWarning()
		{
			var configProvider = new Mock<IClientHostedServiceAttributeProvider>();

			var tstConfig1 = new Mock<IHostedServiceAttribute>();
			tstConfig1.Setup(m => m.TypeAssemblyName).Returns("TestAssembly");
			tstConfig1.Setup(m => m.Type).Returns(typeof(MockServiceTaskWithConfig));
			tstConfig1.Setup(m => m.Code).Returns("TS1");

			var tstConfig2 = new Mock<IHostedServiceAttribute>();
			tstConfig2.Setup(m => m.TypeAssemblyName).Returns("TestAssembly");
			tstConfig2.Setup(m => m.Type).Returns(typeof(MockServiceTaskWithNoConfig));
			tstConfig2.Setup(m => m.Code).Returns("TS2");

			configProvider.Setup(m => m.GetClientHostedServiceAttribute("TS1")).Returns(tstConfig1.Object);
			configProvider.Setup(m => m.GetClientHostedServiceAttribute("TS2")).Returns(tstConfig2.Object);

			var parser = new StdInputRunnerCommandLineArgsParser(configProvider.Object);

			using (var sr = new StringWriter())
			{
				Console.SetOut(sr);
				var commandInfo1 = parser.Parse("TS1");
				var commandInfo2 = parser.Parse("TS2");

				var consoleOut = sr.ToString();

				Assert.That(consoleOut, Does.Contain("TS1 service task will run with the configuration settings from the task schedule. \"-configString:\" argument can be used to override it."));
				Assert.That(consoleOut, Does.Not.Contain("TS2"));
				Console.SetOut(Console.Out);
			}
		}

		class MockServiceTaskWithConfig : ServiceProviderImpl, IServiceTaskConfigurationUser
		{
			public string ConfigString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public override void RunTask(CancellationToken youMustReactToThisToken)
			{
				throw new NotImplementedException();
			}
		}

		class MockServiceTaskWithNoConfig : ServiceProviderImpl
		{
			public override void RunTask(CancellationToken youMustReactToThisToken)
			{
				throw new NotImplementedException();
			}
		}
	}
}
