using System;
using System.Net;
using System.Threading;
using Enterprise.ServiceManager.Host.Http;
using Moq;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Host.Testing.Core.Http
{
	class RequestQueueTest
	{
		[Test]
		public void TestEnqueueRequest()
		{
			// Arrange
			using var requestQueue = GetDefaultRequestQueue();

			// Act
			// Assert
			Assert.DoesNotThrow(() => requestQueue.Add(Mock.Of<IHttpListenerContext>(), CancellationToken.None));
		}

		[Test]
		public void TestDequeueRequest()
		{
			// Arrange
			using var requestQueue = GetDefaultRequestQueue();

			// Act
			// Assert
			Assert.Multiple(() =>
			{
				Assert.DoesNotThrow(() => requestQueue.Add(Mock.Of<IHttpListenerContext>(), CancellationToken.None));
				Assert.DoesNotThrow(() =>
				{
					var request = requestQueue.Take(CancellationToken.None);
				});
			});
		}

		[Test]
		public void TestRequestsAreDequeuedInFifoOrder()
		{
			// Arrange
			var firstRequest = new FakeHttpListenerContext("First");
			var secondRequest = new FakeHttpListenerContext("Second");
			using var requestQueue = GetDefaultRequestQueue();

			// Act
			requestQueue.Add(firstRequest, CancellationToken.None);
			requestQueue.Add(secondRequest, CancellationToken.None);

			var taken = requestQueue.Take(CancellationToken.None) as FakeHttpListenerContext;

			// Assert
			Assert.That(taken?.Identifier, Is.EqualTo("First"));
		}

		[Test]
		public void TestDequeueRespondsToCancellation()
		{
			// Arrange
			using var requestQueue = GetDefaultRequestQueue();
			using var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));

			// Act
			// Assert
			Assert.Throws<OperationCanceledException>(() => requestQueue.Take(cancellationTokenSource.Token));
		}

		RequestQueue GetDefaultRequestQueue()
		{
			return new RequestQueue();
		}

		class FakeHttpListenerContext : IHttpListenerContext
		{
			public FakeHttpListenerContext(string identifier)
			{
				this.identifier = identifier;
			}

			public string Identifier => identifier;
			public HttpListenerRequest Request => throw new NotImplementedException();
			public HttpListenerResponse Response => throw new NotImplementedException();
			public HttpListenerContext Context => throw new NotImplementedException();

			internal string identifier;
		}
	}
}
