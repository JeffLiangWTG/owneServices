using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Enterprise.ZArchitecture.Environment.Registry;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(RegistryImportBusinessObject))]
	sealed class RegistryImportBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "WTG2008:Do not specify filesystem path separators in path string literals.", Justification = "WHY? this should work...")]
		public void TestFilePathValidation_NotExist()
		{
			var bizo = GetBusinessObject();

			using (bizo.openFileDialog)
			{
				bizo.FilePath = Path.Combine(TempForTest.TempPath, "some\\file\\that\\doesnt\\exist.xml");
				AssertExceptionThrown<DirectoryNotFoundException>(() => ForceLoadItems(bizo));
			}
		}

		void ForceLoadItems(RegistryImportBusinessObject bizo)
		{
			AssertNull("This shouldnt even be called. An exception should be thrown", bizo.LoadedRegistryItems);
			Fail("Should not have gotten this far. Forcing to throw exception");
		}

		public void TestFileHasIncorrectlyFormattedXml()
		{
			var bizo = GetBusinessObject();

			using (bizo.openFileDialog)
			using (var fileWithIncorrectXml = TempFile.NewWithExtension("xml"))
			{
				new XElement("NotTheCorrectRootName").Save(fileWithIncorrectXml.Filename);

				bizo.FilePath = fileWithIncorrectXml.Filename;
				AssertExceptionThrown<XmlException>(() => ForceLoadItems(bizo));
			}
		}

		public void TestFileIsCorrupted()
		{
			var bizo = GetBusinessObject();

			using (bizo.openFileDialog)
			using (var fileWithIncorrectXml = TempFile.NewWithExtension("xml"))
			{
				File.WriteAllText(fileWithIncorrectXml.Filename, "<xml><noEndTag></xml>");

				bizo.FilePath = fileWithIncorrectXml.Filename;
				AssertExceptionThrown<XmlException>(() => ForceLoadItems(bizo));
			}
		}

		class MockSaveHandler : IRegistryItemSaveHandler
		{
			public int LoadItemCallCount { get; private set; }
			public LoadedRegistryItems LoadItems(IEnumerable<IRegistryItem> allRegistryItems, Stream stream)
			{
				LoadItemCallCount++;

				return new LoadedRegistryItems(Enumerable.Empty<LoadedRegistryItemValue>());
			}

			public void SaveItems(IEnumerable<IRegistryItem> itemsToSaveToStream, IOverrideLevel levelToSaveFor, Stream stream)
			{
				throw new NotImplementedException();
			}
		}

		public void TestLoadedRegistryItemsIsChangedWithOtherItems()
		{
			var saveHandler = new MockSaveHandler();
			var bizo = GetBusinessObject(saveHandler: saveHandler);

			using (bizo.openFileDialog)
			{
				using (var tempFile = TempFile.NewWithExtension("xml"))
				{
					bizo.FilePath = tempFile.Filename;

					AssertNotNull(bizo.LoadedRegistryItems);
					AssertEquals(1, saveHandler.LoadItemCallCount);
				}

				using (var differentFile = TempFile.NewWithExtension("xml"))
				{
					bizo.FilePath = differentFile.Filename;

					AssertNotNull(bizo.LoadedRegistryItems);
					AssertEquals(2, saveHandler.LoadItemCallCount);
				}
			}
		}

		public static RegistryImportBusinessObject GetBusinessObject(IEnumerable<IRegistryItem> items = null, IFileDialog fileDialog = null, IRegistryItemSaveHandler saveHandler = null)
		{
			saveHandler = saveHandler ?? new RegistryItemSaveHandler();
			fileDialog = fileDialog ?? new ZOpenFileDialog();
			items = items ?? Enumerable.Empty<IRegistryItem>();

			return new RegistryImportBusinessObject(items, fileDialog, saveHandler);
		}

		IFileDialog fileDialog;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RegistryImportBusinessObject(Enumerable.Empty<IRegistryItem>(), fileDialog, new RegistryItemSaveHandler());
		}

		protected override void SetUp()
		{
			base.SetUp();
			fileDialog = new ZOpenFileDialog();
		}

		protected override void TearDown()
		{
			base.TearDown();
			fileDialog.Dispose();
		}
	}
}
