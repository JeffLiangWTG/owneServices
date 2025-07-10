using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.Client.EDI.Telematics;
using Enterprise.Integration;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors;
using Moq;
using NUnit.Framework;

namespace ZClientEDI.Test.Telematics
{
	class TelematicsExtensionTest : TransactionedTestCase
	{
		public static IEnumerable<Type> ExtensionTypes { get; } = new[]
		{
			typeof(ServerRegistrationRequestMessageProcessor),
		};

		public void TestRegistersProcessors()
		{
			// Arrange
			var factoryMock = new Mock<ITelematicsXmlMessageTypeProcessorsFactory>();
			var loggerMock = new Mock<ILogger>();
			var types = new List<Type>();

			factoryMock
				.Setup(factory => factory.AddProcessorFunction(It.IsAny<Func<ILogger, ITelematicsXmlMessageProcessor>>()))
				.Callback<Func<ILogger, ITelematicsXmlMessageProcessor>>(func => types.Add(func(loggerMock.Object).GetType()));

			using (ObjectFactory.Substitute(factoryMock.Object))
			{
				// Act
				TelematicsExtension.RegisterEdiProcessors();
			}

			// Assert
			factoryMock.Verify(factory => factory.GetProcessors(It.IsAny<ILogger>()), Times.Never);
			factoryMock.Verify(factory => factory.AddProcessorFunction(It.IsAny<Func<ILogger, ITelematicsXmlMessageProcessor>>()), Times.Exactly(ExtensionTypes.Count()));
			foreach (var type in ExtensionTypes)
			{
				AssertCollectionContains(type.FullName, type, types);
			}
		}
	}
}
