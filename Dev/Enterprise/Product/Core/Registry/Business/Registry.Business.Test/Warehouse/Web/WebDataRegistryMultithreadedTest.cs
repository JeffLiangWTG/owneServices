using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[UseSnapshotProtection]
	sealed class WebDataRegistryMultithreadedTest : TestCase
	{
		public void TestWebTrackerCustomizationsAreCachedAndAvailableDuringUpgradeInWebThreads()
		{
			var theme = "CLS";
			var imageName = "image.jpg";
			var image = new byte[] { 0, 1, 2 };
			var css = "body { color: black; }";
			WebDataRegistry.Instance.WebTrackerTheme.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[] { new WebTrackerTheme("", theme) });
			WebDataRegistry.Instance.WebTrackerCustomImages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerCustomImage[] { new WebTrackerCustomImage(imageName, "", image) });
			WebDataRegistry.Instance.WebTrackerCustomCss.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerCustomCss[] { new WebTrackerCustomCss("", css) });

			AssertEquals("Precondition", theme, WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebTrackerTheme, "webtracker.com"));
			AssertEquals("Precondition", image, WebDataRegistry.Instance.GetWebCustomImage(WebDataRegistry.Instance.WebTrackerCustomImages, "webtracker.com", imageName));
			AssertEquals("Precondition", css, WebDataRegistry.Instance.GetWebCustomCss(WebDataRegistry.Instance.WebTrackerCustomCss, "webtracker.com"));

			using (var adminConnection = Db.NewAdminConnection())
			{
				AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
				DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
				try
				{
					var task = new Task(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							AssertExceptionThrown(typeof(DatabaseUpgradeInProgressException), () => Db.Connection.EnsureIsOpen());
							AssertEquals(theme, WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebTrackerTheme, "webtracker.com"));
							AssertEquals(image, WebDataRegistry.Instance.GetWebCustomImage(WebDataRegistry.Instance.WebTrackerCustomImages, "webtracker.com", imageName));
							AssertEquals(css, WebDataRegistry.Instance.GetWebCustomCss(WebDataRegistry.Instance.WebTrackerCustomCss, "webtracker.com"));
						}
					});
					task.Start();
					task.Wait();
				}
				finally
				{
					adminConnection.ResetLockout();
				}
			}
		}

		public void TestGetWebTrackerThemeIsThreadSafe()
		{
			WebDataRegistry.Instance.WebTrackerTheme.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[] { new WebTrackerTheme("", "XXX") });
			RegistryItemDictionary.Instance.PurgeAll();
			var threads = new Thread[100];
			for (int i = 0; i < threads.Length; i++)
			{
				threads[i] = i % 20 == 0
					? new Thread(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							RegistryItemDictionary.Instance.PurgeAll();
						}
					})
					{ Name = "PurgeAll" }
					: new Thread(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							AssertEquals("XXX", WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebTrackerTheme, "webtracker.com"));
						}
					})
					{ Name = "GetWebTrackerTheme" };
				threads[i].Start();
			}
			for (int i = 0; i < threads.Length; i++)
			{
				threads[i].Join();
			}
		}

		public void TestWebCFSCustomizationsAreCachedAndAvailableDuringUpgradeInWebThreads()
		{
			var theme = "CLS";
			var imageName = "image.jpg";
			var image = new byte[] { 0, 1, 2 };
			var css = "body { color: black; }";
			WebDataRegistry.Instance.WebCFSTheme.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[] { new WebTrackerTheme("", theme) });
			WebDataRegistry.Instance.WebCFSCustomImages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerCustomImage[] { new WebTrackerCustomImage(imageName, "", image) });
			WebDataRegistry.Instance.WebCFSCustomCss.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerCustomCss[] { new WebTrackerCustomCss("", css) });

			AssertEquals("Precondition", theme, WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebCFSTheme, "webcfs.com"));
			AssertEquals("Precondition", image, WebDataRegistry.Instance.GetWebCustomImage(WebDataRegistry.Instance.WebCFSCustomImages, "webcfs.com", imageName));
			AssertEquals("Precondition", css, WebDataRegistry.Instance.GetWebCustomCss(WebDataRegistry.Instance.WebCFSCustomCss, "webcfs.com"));

			using (var adminConnection = Db.NewAdminConnection())
			{
				AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
				DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
				try
				{
					var task = new Task(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							AssertExceptionThrown(typeof(DatabaseUpgradeInProgressException), () => Db.Connection.EnsureIsOpen());
							AssertEquals(theme, WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebCFSTheme, "webcfs.com"));
							AssertEquals(image, WebDataRegistry.Instance.GetWebCustomImage(WebDataRegistry.Instance.WebCFSCustomImages, "webcfs.com", imageName));
							AssertEquals(css, WebDataRegistry.Instance.GetWebCustomCss(WebDataRegistry.Instance.WebCFSCustomCss, "webcfs.com"));
						}
					});
					task.Start();
					task.Wait();
				}
				finally
				{
					adminConnection.ResetLockout();
				}
			}
		}

		public void TestGetWebCFSThemeIsThreadSafe()
		{
			WebDataRegistry.Instance.WebCFSTheme.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[] { new WebTrackerTheme("", "XXX") });
			RegistryItemDictionary.Instance.PurgeAll();
			var threads = new Thread[100];
			for (int i = 0; i < threads.Length; i++)
			{
				threads[i] = i % 20 == 0
					? new Thread(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							RegistryItemDictionary.Instance.PurgeAll();
						}
					})
					{ Name = "PurgeAll" }
					: new Thread(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							AssertEquals("XXX", WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebCFSTheme, "webcfs.com"));
						}
					})
					{ Name = "GetWebcfsTheme" };
				threads[i].Start();
			}
			for (int i = 0; i < threads.Length; i++)
			{
				threads[i].Join();
			}
		}
	}
}
