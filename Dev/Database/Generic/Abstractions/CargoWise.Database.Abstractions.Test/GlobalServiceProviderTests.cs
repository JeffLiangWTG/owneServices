using System;
using NUnit.Framework;

namespace CargoWise.Database.Abstractions.Test
{
	public class GlobalServiceProviderTests
	{
		[Test]
		public void ThrowsWhenNotConfigured()
		{
			Assert.That(() => _ = GlobalServiceProvider.Instance, Throws.TypeOf<InvalidOperationException>());
		}

		[Test]
		public void TryGetFailsWhenNotConfigured()
		{
			Assert.That(GlobalServiceProvider.TryGetInstance(out var provider), Is.EqualTo(false));
			Assert.That(provider, Is.Null);
		}

		[Test]
		public void ConfiguresServiceProvider()
		{
			var mock = new MockServiceProvider { Id = 1 };
			using (GlobalServiceProvider.Configure(mock))
			{
				var provider = GlobalServiceProvider.Instance;
				Assert.That(provider, Is.Not.Null);
				Assert.That(provider, Is.TypeOf<MockServiceProvider>());
				Assert.That(((MockServiceProvider)provider).Id, Is.EqualTo(1));

				Assert.That(GlobalServiceProvider.TryGetInstance(out var provider2), Is.EqualTo(true));
				Assert.That(provider2, Is.SameAs(provider));
			}
		}

		[Test]
		public void CanNestDifferentProviders()
		{
			var mock1 = new MockServiceProvider { Id = 1 };
			var mock2 = new MockServiceProvider { Id = 2 };
			var mock3 = new MockServiceProvider { Id = 3 };

			using (GlobalServiceProvider.Configure(mock1))
			{
				AssertMockServiceProvider(GlobalServiceProvider.Instance, expectedId: 1);

				using (GlobalServiceProvider.Configure(mock2))
				{
					AssertMockServiceProvider(GlobalServiceProvider.Instance, expectedId: 2);

					using (GlobalServiceProvider.Configure(mock3))
					{
						AssertMockServiceProvider(GlobalServiceProvider.Instance, expectedId: 3);
					}

					AssertMockServiceProvider(GlobalServiceProvider.Instance, expectedId: 2);
				}

				AssertMockServiceProvider(GlobalServiceProvider.Instance, expectedId: 1);
			}
		}

		[Test]
		public void CannotNestOutOfOrder()
		{
			var mock1 = new MockServiceProvider { Id = 1 };
			var mock2 = new MockServiceProvider { Id = 2 };

			var disposable1 = GlobalServiceProvider.Configure(mock1);
			AssertMockServiceProvider(GlobalServiceProvider.Instance, expectedId: 1);

			var disposable2 = GlobalServiceProvider.Configure(mock2);
			AssertMockServiceProvider(GlobalServiceProvider.Instance, expectedId: 2);

			Assert.That(disposable1.Dispose, Throws.TypeOf<InvalidOperationException>());

			AssertMockServiceProvider(GlobalServiceProvider.Instance, expectedId: 2);
			disposable2.Dispose();

			AssertMockServiceProvider(GlobalServiceProvider.Instance, expectedId: 1);
			disposable1.Dispose();
		}

		void AssertMockServiceProvider(IServiceProvider provider, int expectedId)
		{
			Assert.That(provider, Is.Not.Null);
			Assert.That(provider, Is.TypeOf<MockServiceProvider>());
			Assert.That(((MockServiceProvider)provider).Id, Is.EqualTo(expectedId));
		}

		IDisposable configureForTestDisposable;

		[SetUp]
		protected void SetUp()
		{
			configureForTestDisposable = GlobalServiceProvider.Configure(null);
		}

		[TearDown]
		protected void TearDown()
		{
			configureForTestDisposable.Dispose();
		}

		sealed class MockServiceProvider : IServiceProvider
		{
			public int Id { get; set; }

			public object GetService(Type serviceType) => null;
		}
	}
}
