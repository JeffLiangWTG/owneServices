using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class ThumbnailSizeContextMenuTest : TransactionedTestCase
	{
		public void TestContextMenu()
		{
			using (ThumbNailSizeContextMenu testMenu = new ThumbNailSizeContextMenu())
			{
				AssertEquals("Context menu created with one top level item", 1, testMenu.MenuItems.Count);
				Assert("First item is submenu", testMenu.MenuItems[0].IsParent);
				Assert("Submenu is not empty", testMenu.MenuItems[0].MenuItems.Count > 0);
			}
		}

		public void TestRegistryLoadingAndSavingFromMenu()
		{
			DMThumbnailSettingsStruct modifiedRegistry = Env.Registry.DMThumbnailSettings;
			modifiedRegistry.NumberOfThumbnails = 5;

			Env.Registry.DMThumbnailSettings = modifiedRegistry;
			ThumbNailSizeContextMenu testMenu = new ThumbNailSizeContextMenu();
			AssertEquals("Got the correct value from the registry", 5, testMenu.NumberOfThumbnailsPerRow);

			// set the registry value to something else
			modifiedRegistry.NumberOfThumbnails = 3;
			Env.Registry.DMThumbnailSettings = modifiedRegistry;

			testMenu.Dispose();
			AssertEquals("Got the setting from the thumbnail menu on dispose", 5, Env.Registry.DMThumbnailSettings.NumberOfThumbnails);
		}

		public void TestDisposeShouldIgnoreSqlException()
		{
			var testMenu = new ThumbnailSizeContextMenuExposed();

			var error = SqlExceptionBuilder.CreateSqlError(-2, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "Timeout expired", "", 0);
			var sqlException = SqlExceptionBuilder.CreateSqlException(error);

			testMenu.SaveSettingsHook = () => { throw sqlException; };

			AssertEquals(false, testMenu.IsDisposed);
			AssertNoExceptionThrown(testMenu.Dispose);
			AssertEquals(true, testMenu.IsDisposed);
		}

		public void TestNumberOfThumbnailsPerRowOnLoad()
		{
			using (ThumbNailSizeContextMenu testMenu = new ThumbNailSizeContextMenu())
			{
				AssertEquals("Number of thumbnails should be same as registry on load", Env.Registry.DMThumbnailSettings.NumberOfThumbnails, testMenu.NumberOfThumbnailsPerRow);
			}
		}

		public void TestNumberOfThumbnailsChangesOnMenuItemSelect()
		{
			using (ThumbnailSizeContextMenuExposed testMenu = new ThumbnailSizeContextMenuExposed())
			{
				AssertEquals("Number of thumbnails should be same as registry on load", Env.Registry.DMThumbnailSettings.NumberOfThumbnails, testMenu.NumberOfThumbnailsPerRow);

				testMenu.CallMenuItemClicked(testMenu.MenuItems[0].MenuItems[3], EventArgs.Empty);
				AssertEquals("New number of thumbnails should have changed", 2, testMenu.NumberOfThumbnailsPerRow);
			}
		}

		public void TestDBHitsForSaveSettings()
		{
			var setting = Env.Registry.DMThumbnailSettings;
			setting.ThumbNailViewActive = true;
			setting.NumberOfThumbnails = 0;
			Env.Registry.DMThumbnailSettings = setting;

			using (var testMenu = new ThumbnailSizeContextMenuExposed())
			{
				AssertEquals(0, testMenu.NumberOfThumbnailsPerRow);

				var commandCount = Db.Connection.ExecutedCommandCount;
				testMenu.SaveSettingsForTest();
				var newCommandCount = Db.Connection.ExecutedCommandCount;

				AssertEquals("Should not execute update DMThumbnailSettings command", commandCount, newCommandCount);
			}
		}
	}
}
