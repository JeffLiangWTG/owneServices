using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Enterprise.ZArchitecture.Environment.Registry;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(RegistryExportForm))]
	sealed class RegistryExportFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new RegistryExportForm(new RegistryComparisonBusinessObject(Array.Empty<IRegistryItem>(), new DefaultOverrideLevel(), new SystemOverrideLevel()), new RegistryItemSaveHandler());
		}

		IRegistryItem NewItem(string code = null)
		{
			return new StringRegistryItem(code ?? ZGuid.NewZGuid().ToString(), (NoResString)"My/Node/Catagory", (NoResString)"", (NoResString)"", RegistryStorageFlags.All, "DefaultValue");
		}

		#endregion

		//Using Rhino mocks here will leak the RegistryExportForm instance, and make the disposable leak listener fail the test
		class DummySaveHandler : IRegistryItemSaveHandler
		{
			readonly IRegistryItem[] expectedItemsToSave;
			readonly IOverrideLevel expectedLevelToSaveFor;
			readonly Exception exceptionToThrow;

			public DummySaveHandler(IOverrideLevel expectedLevelToSaveFor, IRegistryItem[] expectedItemsToSave)
			{
				this.expectedLevelToSaveFor = expectedLevelToSaveFor;
				this.expectedItemsToSave = expectedItemsToSave;
				this.exceptionToThrow = null;
			}

			public DummySaveHandler(Exception exceptionToThrow)
			{
				this.expectedLevelToSaveFor = null;
				this.expectedItemsToSave = null;
				this.exceptionToThrow = exceptionToThrow;
			}

			public void SaveItems(IEnumerable<IRegistryItem> itemsToSaveToStream, IOverrideLevel levelToSaveFor, Stream stream)
			{
				if (exceptionToThrow != null)
				{
					throw exceptionToThrow;
				}

				AssertEquals("level", expectedLevelToSaveFor, levelToSaveFor);
				AssertContainsExactElementsInAnyOrder(expectedItemsToSave, itemsToSaveToStream);
			}

			public LoadedRegistryItems LoadItems(IEnumerable<IRegistryItem> allRegistryItems, Stream stream)
			{
				throw new NotImplementedException();
			}
		}

		[RequiresSTA]
		public void TestSaveRegistryItems()
		{
			var baseLevel = new DefaultOverrideLevel();
			var overrideLevel = new SystemOverrideLevel();

			var items = new[] { NewItem(), NewItem(), NewItem(), NewItem() };
			overrideLevel.SetValueOf(items[0], "Override value");
			overrideLevel.SetValueOf(items[2], "Override value");

			var diffBizo = new RegistryComparisonBusinessObject(items, baseLevel, overrideLevel);

			using (var form = new RegistryExportForm(diffBizo, new DummySaveHandler(overrideLevel, new[] { items[0], items[2] })))
			{
				form.Show();

				var tempFilePath = Temp.GetTempFileNameWithExtension("xml");

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;

				try
				{
					form.btnExport.PerformClick();
				}
				finally
				{
					DeleteIfExists(tempFilePath);
				}
			}
		}

		void AssertThrowWhenExportingDoesntBubbleExceptionUp(string message, Exception exceptionToThrow)
		{
			var baseLevel = new DefaultOverrideLevel();
			var overrideLevel = new SystemOverrideLevel();

			var items = new[] { NewItem(), NewItem(), NewItem(), NewItem() };
			overrideLevel.SetValueOf(items[0], "Override value");
			overrideLevel.SetValueOf(items[2], "Override value");

			var diffBizo = new RegistryComparisonBusinessObject(items, baseLevel, overrideLevel);

			using (var form = new RegistryExportForm(diffBizo, new DummySaveHandler(exceptionToThrow)))
			{
				form.Show();

				var tempFilePath = Temp.GetTempFileNameWithExtension("xml");

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;

				try
				{
					AssertNoExceptionThrown(message, form.btnExport.PerformClick);
					Assert("File should be cleaned up", !File.Exists(tempFilePath));
				}
				finally
				{
					DeleteIfExists(tempFilePath);
				}
			}
		}

		public void TestSaveRegistryItemsWhenItIsCancelled()
		{
			AssertThrowWhenExportingDoesntBubbleExceptionUp("Since the operation may be cancelled and we need to expect that", new OperationCanceledException());
		}

		public void TestSaveRegistryItemsWhenAnIOExceptionIsThrown()
		{
			AssertThrowWhenExportingDoesntBubbleExceptionUp("If an IOException occurs the user should be told what to do", new IOException());
			AssertEquals("A problem was encountered while trying to write to the file. Please try writing to a different directory.", ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
		}
	}
}
