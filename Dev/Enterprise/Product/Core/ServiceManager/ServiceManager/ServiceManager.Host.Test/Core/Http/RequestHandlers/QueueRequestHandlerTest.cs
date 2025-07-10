using System;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.DataContracts;

namespace Enterprise.ServiceManager.Host.Testing.Core.Http.RequestHandlers
{
	[TestedType(typeof(QueueRequestHandler))]
	class QueueRequestHandlerTest : RequestHandlerBaseTest
	{
		protected override Uri GetExpectedUri()
		{
			return new Uri($"http://localhost:7070/cargowise/processController/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/queueStatus");
		}

		public void TestArgumentNullExceptions()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new QueueRequestHandler(null, Mock.Of<IJsonConverter>(), GetExpectedUri().OriginalString));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("statusProvider"));

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new QueueRequestHandler(Mock.Of<IQueueStatusProvider>(), null, GetExpectedUri().OriginalString));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("jsonConverter"));
			});
		}

		[ExpectNoExceptions]
		public void TestHandleWithQueueStatusReturnsJson()
		{
			// Arrange
			var expectedList = Enumerable.Range(0, 5)
				.Select(o => new QueueDTO($"queue{o}", $"task{o}", true, o, o, o, o))
				.ToArray();

			queueStatusProviderFactoryMock
				.Setup(o => o.GetQueueStatus())
				.Returns(new QueueListDTO(expectedList));

			// Act
			var result = handler.Handle(null);

			// Assert
			var list = JsonConvert.DeserializeObject<QueueListDTO>(result).QueueList;
			NUnit.Framework.Assert.That(list.Count(), Is.EqualTo(expectedList.Length));
		}

		[ExpectNoExceptions]
		public void TestHandleWithQueueStatusReturnsEmptyWhenOperationCanceledExceptionThrows()
		{
			// Arrange
			queueStatusProviderFactoryMock
				.Setup(o => o.GetQueueStatus())
				.Throws<OperationCanceledException>();

			// Act
			var result = handler.Handle(null);

			// Assert
			var list = JsonConvert.DeserializeObject<QueueListDTO>(result).QueueList;
			NUnit.Framework.Assert.That(list.Count(), Is.EqualTo(0));
		}

		protected override void SetUp()
		{
			base.SetUp();

			queueStatusProviderFactoryMock = new Mock<IQueueStatusProvider>();
			handler = new QueueRequestHandler(queueStatusProviderFactoryMock.Object, new JsonNetConverter(), "localhost");
		}

		Mock<IQueueStatusProvider> queueStatusProviderFactoryMock;
	}
}
