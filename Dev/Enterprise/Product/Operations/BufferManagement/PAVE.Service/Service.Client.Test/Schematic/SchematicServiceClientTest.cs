using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;
using Moq;

namespace Enterprise.BufferManagement.Service.Client.Test
{
	public class SchematicServiceClientTest : PAVEServiceClientTestCase
	{
		public void TestProcessTransferRules_ShouldCallSchematicServiceDirectly_WhenUseWebServicesIsFalse()
		{
			var schematicServiceMock = new Mock<ISchematicService>();

			IEnumerable<Guid> transferablePKsProcessed = null;

			schematicServiceMock.Setup(m => m.ProcessTransferRules(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<ILogger>()))
				.Callback((IEnumerable<Guid> transferablePKs, ILogger logger) =>
				{
					transferablePKsProcessed = transferablePKs;
					AssertEquals(loggerMock.Object, logger);
				})
				.Verifiable();

			ObjectFactory.Substitute(schematicServiceMock.Object);

			var toProcessPKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
			new SchematicServiceClient().ProcessTransferRules(toProcessPKs, loggerMock.Object);

			schematicServiceMock.Verify(m => m.ProcessTransferRules(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<ILogger>()), Times.Once);
			httpClientFactoryMock.Verify(m => m.Create(), Times.Never);
			AssertContainsExactElementsInAnyOrder(toProcessPKs, transferablePKsProcessed);
		}

		#region SetUp

		readonly IBMSRegistry BMSRegistry = ObjectFactory.Get<IBMSRegistry>();
		Mock<IPAVEHttpClient> httpClientMock;
		Mock<IPAVEHttpClientFactory> httpClientFactoryMock;

		protected override void SetUp()
		{
			base.SetUp();

			httpClientMock = new Mock<IPAVEHttpClient>();
			httpClientFactoryMock = new Mock<IPAVEHttpClientFactory>();
			httpClientFactoryMock.Setup(f => f.Create())
				.Returns(httpClientMock.Object)
				.Verifiable();

			ObjectFactory.Substitute(httpClientFactoryMock.Object);
		}

		#endregion

		#region Test Case Overrides

		protected override string ExpectedClassNameInLogs => nameof(SchematicServiceClient);

		protected override IPAVEService GetClient() => new SchematicServiceClient();

		#endregion

	}
}
