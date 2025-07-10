using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ServiceManager.Runner;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test;

class EnvironmentCheckerStrategyTest
{
	[SetUp]
	public void SetUp()
	{
		environmentCheckersMock = new Mock<IEnumerable<IEnvironmentChecker>>();
		environmentCheckerStrategy = new EnvironmentCheckerStrategy(environmentCheckersMock.Object);
		serviceTaskMock = new Mock<IServiceTaskHandler>();
		serviceTaskConfigMock = new Mock<IHostedServiceAttribute>();
		serviceTaskConfigMock.Setup(x => x.Code).Returns("xxx");
		serviceTaskConfigMock.Setup(x => x.TypeName).Returns("typeName1");
		serviceTaskConfigMock.Setup(x => x.TypeAssemblyName).Returns("assemblyName1");
		serviceTaskMock.Setup(x => x.HostedServiceAttribute).Returns(serviceTaskConfigMock.Object);
	}

	[Test]
	public void TestWrongConstructorParams()
	{
		var result = Assert.Throws<ArgumentNullException>(() => new EnvironmentCheckerStrategy(null));
		Assert.That(result.ParamName, Is.EqualTo("environmentCheckers"));
	}

	[Test]
	public void TestAllEnvironmentCheckersAreInitialized()
	{
		// Arrange
		var checkerMocks = Enumerable.Range(0, 3)
			.Select(i => new Mock<IEnvironmentChecker>())
			.ToArray();
		environmentCheckersMock
			.Setup(checkers => checkers.GetEnumerator())
			.Returns(() => checkerMocks.Select(mock => mock.Object).GetEnumerator());

		// Act
		environmentCheckerStrategy.Initialize(It.IsAny<IRunCommandInfo>());

		// Assert
		Assert.DoesNotThrow(() =>
		{
			foreach (var checkerMock in checkerMocks)
			{
				checkerMock.Verify(checker => checker.Initialize(It.IsAny<IRunCommandInfo>()), Times.Once);
				checkerMock.VerifyNoOtherCalls();
			}
		});
	}

	[Test]
	public void TestExecuteOnServiceTaskCompletionCallsCheckOnServiceTaskCompletion()
	{
		// Arrange
		var checkerMocks = Enumerable.Range(0, 3)
			.Select(i => new Mock<IEnvironmentChecker>())
			.ToArray();
		environmentCheckersMock
			.Setup(checkers => checkers.GetEnumerator())
			.Returns(() => checkerMocks.Select(mock => mock.Object).GetEnumerator());

		// Act
		environmentCheckerStrategy.ExecuteOnServiceTaskCompletion(serviceTaskMock.Object);

		// Assert
		Assert.DoesNotThrow(() =>
		{
			foreach (var checkerMock in checkerMocks)
			{
				checkerMock.Verify(checker => checker.CheckOnServiceTaskCompletion(serviceTaskMock.Object), Times.Once);
				checkerMock.VerifyNoOtherCalls();
			}
		});
	}

	[Test]
	public void TestExecuteOnServiceTaskExceptionCallsCheckOnServiceTaskException()
	{
		// Arrange
		var checkerMocks = Enumerable.Range(0, 3)
			.Select(i => new Mock<IEnvironmentChecker>())
			.ToArray();
		environmentCheckersMock
			.Setup(checkers => checkers.GetEnumerator())
			.Returns(() => checkerMocks.Select(mock => mock.Object).GetEnumerator());

		// Act
		environmentCheckerStrategy.ExecuteOnServiceTaskException(serviceTaskMock.Object);

		// Assert
		Assert.DoesNotThrow(() =>
		{
			foreach (var checkerMock in checkerMocks)
			{
				checkerMock.Verify(checker => checker.CheckOnServiceTaskException(serviceTaskMock.Object), Times.Once);
				checkerMock.VerifyNoOtherCalls();
			}
		});
	}

	[Test]
	public void TestInitializationsAndCheckOnServiceTaskCompletionAreInSameOrder()
	{
		// Arrange
		const int count = 3;
		var initializeCallList = new List<int>();
		var checkOnServiceTaskCompletionCallList = new List<int>();
		var checkerMocks = Enumerable.Range(0, count)
			.Select(i =>
			{
				var checkerMock = new Mock<IEnvironmentChecker>();
				checkerMock
					.Setup(checker => checker.Initialize(It.IsAny<IRunCommandInfo>()))
					.Callback(() =>
					{
						initializeCallList.Add(i);
					});
				checkerMock
					.Setup(checker => checker.CheckOnServiceTaskCompletion(serviceTaskMock.Object))
					.Callback(() =>
					{
						checkOnServiceTaskCompletionCallList.Add(i);
					});
				return checkerMock;
			})
			.ToArray();
		environmentCheckersMock
			.Setup(checkers => checkers.GetEnumerator())
			.Returns(() => checkerMocks.Select(mock => mock.Object).GetEnumerator());

		// Act
		environmentCheckerStrategy.Initialize(It.IsAny<IRunCommandInfo>());
		environmentCheckerStrategy.ExecuteOnServiceTaskCompletion(serviceTaskMock.Object);

		// Assert
		Assert.That(checkOnServiceTaskCompletionCallList, Is.EqualTo(initializeCallList));
	}

	[Test]
	public void TestInitializationsAndCheckOnServiceTaskExceptionAreInSameOrder()
	{
		// Arrange
		const int count = 3;
		var initializeCallList = new List<int>();
		var checkOnServiceTaskExceptionCallList = new List<int>();
		var checkerMocks = Enumerable.Range(0, count)
			.Select(i =>
			{
				var checkerMock = new Mock<IEnvironmentChecker>();
				checkerMock
					.Setup(checker => checker.Initialize(It.IsAny<IRunCommandInfo>()))
					.Callback(() =>
					{
						initializeCallList.Add(i);
					});
				checkerMock
					.Setup(checker => checker.CheckOnServiceTaskException(serviceTaskMock.Object))
					.Callback(() =>
					{
						checkOnServiceTaskExceptionCallList.Add(i);
					});
				return checkerMock;
			})
			.ToArray();
		environmentCheckersMock
			.Setup(checkers => checkers.GetEnumerator())
			.Returns(() => checkerMocks.Select(mock => mock.Object).GetEnumerator());

		// Act
		environmentCheckerStrategy.Initialize(It.IsAny<IRunCommandInfo>());
		environmentCheckerStrategy.ExecuteOnServiceTaskException(serviceTaskMock.Object);

		// Assert
		Assert.That(checkOnServiceTaskExceptionCallList, Is.EqualTo(initializeCallList));
	}

	[Test]
	public void TestExecuteOnServiceTaskCompletionCatchesAndRethrowsEnvironmentCorruptedException()
	{
		// Arrange
		var checkerMock = new Mock<IEnvironmentChecker>();
		checkerMock
			.Setup(c => c.CheckOnServiceTaskCompletion(serviceTaskMock.Object))
			.Throws(new DbConnectionDisposerCorruptedException(serviceTaskMock.Object.HostedServiceAttribute));
		var checkerMocks = new[] { checkerMock };
		environmentCheckersMock
			.Setup(checkers => checkers.GetEnumerator())
			.Returns(() => checkerMocks.Select(mock => mock.Object).GetEnumerator());

		environmentCheckerStrategy.Initialize(It.IsAny<IRunCommandInfo>());

		// Act, Assert
		Assert.That(() =>
		{
			environmentCheckerStrategy.ExecuteOnServiceTaskCompletion(serviceTaskMock.Object);
		}, Throws.InstanceOf<EnvironmentCorruptedException>());
	}

	[Test]
	public void TestExecuteOnServiceTaskCompletionThrowsEnvironmentCheckerExceptionForOtherInnerException()
	{
		// Arrange
		var checkerMock = new Mock<IEnvironmentChecker>();
		checkerMock
			.Setup(c => c.CheckOnServiceTaskCompletion(serviceTaskMock.Object))
			.Throws<ArgumentNullException>();
		var checkerMocks = new[] { checkerMock };
		environmentCheckersMock
			.Setup(checkers => checkers.GetEnumerator())
			.Returns(() => checkerMocks.Select(mock => mock.Object).GetEnumerator());

		environmentCheckerStrategy.Initialize(It.IsAny<IRunCommandInfo>());

		// Act, Assert
		var exception = Assert.Throws<EnvironmentCheckerException>(() =>
		{
			environmentCheckerStrategy.ExecuteOnServiceTaskCompletion(serviceTaskMock.Object);
		});
		var innerException = exception.InnerException;
		Assert.That(innerException, NUnit.Framework.Is.TypeOf<ArgumentNullException>());
	}

	[Test]
	public void TestExecuteOnServiceTaskExceptionCatchesAndRethrowsEnvironmentCorruptedException()
	{
		// Arrange
		var checkerMock = new Mock<IEnvironmentChecker>();
		checkerMock
			.Setup(c => c.CheckOnServiceTaskException(serviceTaskMock.Object))
			.Throws(new UndisposedSqlLockException(serviceTaskMock.Object.HostedServiceAttribute, new List<string>() { "Key1" }));
		var checkerMocks = new[] { checkerMock };
		environmentCheckersMock
			.Setup(checkers => checkers.GetEnumerator())
			.Returns(() => checkerMocks.Select(mock => mock.Object).GetEnumerator());

		environmentCheckerStrategy.Initialize(It.IsAny<IRunCommandInfo>());

		// Act, Assert
		Assert.That(() =>
		{
			environmentCheckerStrategy.ExecuteOnServiceTaskException(serviceTaskMock.Object);
		}, Throws.InstanceOf<EnvironmentCorruptedException>());
	}

	[Test]
	public void TestExecuteOnServiceTaskExceptionThrowsEnvironmentCheckerExceptionForOtherInnerException()
	{
		// Arrange
		var checkerMock = new Mock<IEnvironmentChecker>();
		checkerMock
			.Setup(c => c.CheckOnServiceTaskException(serviceTaskMock.Object))
			.Throws<ArgumentNullException>();
		var checkerMocks = new[] { checkerMock };
		environmentCheckersMock
			.Setup(checkers => checkers.GetEnumerator())
			.Returns(() => checkerMocks.Select(mock => mock.Object).GetEnumerator());

		environmentCheckerStrategy.Initialize(It.IsAny<IRunCommandInfo>());

		// Act, Assert
		var exception = Assert.Throws<EnvironmentCheckerException>(() =>
		{
			environmentCheckerStrategy.ExecuteOnServiceTaskException(serviceTaskMock.Object);
		});
		var innerException = exception.InnerException;
		Assert.That(innerException, Is.TypeOf<ArgumentNullException>());
	}

	Mock<IEnumerable<IEnvironmentChecker>> environmentCheckersMock;
	EnvironmentCheckerStrategy environmentCheckerStrategy;
	Mock<IServiceTaskHandler> serviceTaskMock;
	Mock<IHostedServiceAttribute> serviceTaskConfigMock;
}
