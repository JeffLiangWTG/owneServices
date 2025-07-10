using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Enterprise.ZArchitecture.Environment.Registry;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ImportOverridesForm))]
	sealed class ImportOverridesFormBasherTest : ZFormBasherTest
	{
		public void TestShowErrorMessageWhenCompareButtonWithDuplicateItems()
		{
			var saveHandler = new RegistryItemSaveHandler();
			var fileDialog = new ZOpenFileDialog();
			var items = CargoWise.Application.ObjectFactory.Get<IRegistryItemSetLocator>()
				.GetAllRegistryItems()
				.DistinctBy(item => item.Name)
				.Where(item => item.CanBeExported);

			var bizo = new RegistryImportBusinessObject(items, fileDialog, saveHandler);

			using (bizo.openFileDialog)
			using (var fileWithIncorrectXml = TempFile.NewWithExtension("xml"))
			{
				string contents =
					@"<?xml version=""1.0"" encoding=""utf-8""?>
					<RegistryItemOverrides>
						<Items>
							<Item>
								<Name>WebServicePassword</Name>
								<Caption>Web Service User Password</Caption>
								<Data>TESTAAAA</Data>
							</Item>
							<Item>
								<Name>WebServicePassword</Name>
								<Caption>Web Service User Password</Caption>
								<Data>TESTBBBB</Data>
							</Item>
						</Items>
					</RegistryItemOverrides>";

				File.WriteAllText(fileWithIncorrectXml.Filename, contents);
				bizo.OverrideLevel = new DefaultOverrideLevel();
				bizo.FilePath = fileWithIncorrectXml.Filename;

				using (var form = new ImportOverridesForm(bizo))
				{
					form.Show();

					AssertNoExceptionThrown(form.compareButton.PerformClick);
					AssertContains("The file you have imported contains elements which have same name.", ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
				}
			}
		}

		[RequiresSTA]
		public void TestTreeIsPopulated()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "MyCompany";

			var branch = company.Branches.AddNew();
			branch.GB_BranchName = "MyBranch";

			Factory.Save();

			var bizo = RegistryImportBusinessObjectTest.GetBusinessObject();

			using (bizo.openFileDialog)
			using (var form = new ImportOverridesForm(bizo))
			{
				TreeViewAssertion.AssertHasPath("root", form.levelTreeView.Nodes, "System");
				TreeViewAssertion.AssertHasPath("root", form.levelTreeView.Nodes, "Companies/MyCompany/Branches/MyBranch");
			}
		}

		public void TestSelectingAnOverrideSetsTheBusinessObjectValue()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "MyCompany";

			var branch = company.Branches.AddNew();
			branch.GB_BranchName = "MyBranch";

			Factory.Save();

			var bizo = RegistryImportBusinessObjectTest.GetBusinessObject();

			using (bizo.openFileDialog)
			using (var form = new ImportOverridesForm(bizo))
			{
				SelectNode(form.levelTreeView, "Companies/MyCompany/Branches/MyBranch");
				AssertNotNull(form.BusinessEntity.OverrideLevel);
				AssertEquals("MyBranch", form.BusinessEntity.OverrideLevel.Description);

				SelectNode(form.levelTreeView, "System");
				AssertNotNull(form.BusinessEntity.OverrideLevel);
				AssertEquals("System", form.BusinessEntity.OverrideLevel.Description);
			}
		}

		LoadedRegistryItemValue GetLoadedItem(string code, string caption, bool successFullyLoaded = true, int loadedValue = 1)
		{
			return new LoadedRegistryItemValue(code, caption, new IntRegistryItem(code, (NoResString)"", (NoResString)caption, (NoResString)"", RegistryStorageFlags.All, 0), loadedValue, successFullyLoaded);
		}

		public void TestIgnoreFailedItems()
		{
			var failedItems = new[] { GetLoadedItem("Code", "Bad Item", false) };
			var successfulItems = new[] { GetLoadedItem("Code2", "Good Item", true) };

			AssertMessageShown(failedItems.Concat(successfulItems),
@"Some items could not be loaded (1 out of 2 failed). 

Would you like to continue with the import?");
		}

		void AssertMessageShown(IEnumerable<LoadedRegistryItemValue> items, string expectedMessage)
		{
			var saveHandler = new Mock<IRegistryItemSaveHandler>();
			saveHandler
				.Setup(m => m.LoadItems(It.IsAny<IEnumerable<IRegistryItem>>(), It.IsAny<Stream>()))
				.Returns(new LoadedRegistryItems(items));

			using (var tempFile = TempFile.NewWithExtension("xml"))
			{
				var bizo = RegistryImportBusinessObjectTest.GetBusinessObject(saveHandler: saveHandler.Object);
				bizo.OverrideLevel = new DefaultOverrideLevel();
				bizo.FilePath = tempFile.Filename;

				using (bizo.openFileDialog)
				using (var form = new ImportOverridesForm(bizo))
				{
					form.Show();

					var message = (UnitTestUserNotification)Globals.Message;
					message.AddAnswer(DialogResult.No);

					form.compareButton.PerformClick();

					Assert("Since we said we did not want to continue the form should still be open", form.Visible);
					AssertEquals(expectedMessage, message.LastMessage.Text);
				}
			}
		}

		public void TestCompareButtonWithEmptyFilePath()
		{
			var bizo = RegistryImportBusinessObjectTest.GetBusinessObject();

			using (bizo.openFileDialog)
			using (var form = new ImportOverridesForm(bizo))
			{
				form.Show();
				bizo.OverrideLevel = new DefaultOverrideLevel();

				AssertNoExceptionThrown(form.compareButton.PerformClick);
				AssertContains("Please select a file to load", ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
			}
		}

		public void TestCompareButtonWithOutdatedDB()
		{
			var saveHandler = new RegistryItemSaveHandler();
			var fileDialog = new ZOpenFileDialog();
			var items = CargoWise.Application.ObjectFactory.Get<IRegistryItemSetLocator>()
				.GetAllRegistryItems()
				.DistinctBy(item => item.Name)
				.Where(item => item.CanBeExported);

			var bizo = new RegistryImportBusinessObject(items, fileDialog, saveHandler);

			using (bizo.openFileDialog)
			using (var fileWithIncorrectXml = TempFile.NewWithExtension("xml"))
			{
				#region XMLs

				var data = Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(
					@"<?xml version=""1.0"" encoding=""utf-16""?>
					<OrgCodeAlgorithm>
						<RegenerateOrgCodeOnChanges>Y</RegenerateOrgCodeOnChanges>
						<AllowRecalculatedOrgCodeByUser>Y</AllowRecalculatedOrgCodeByUser>
						<FakeBool>N</FakeBool>
						<ArrayOfOrgCodeElement>
							<OrgCodeElement>
								<Description>First Name</Description>
								<Length>0</Length>
								<Order>0</Order>
							</OrgCodeElement>
							<OrgCodeElement>
								<Description>Second Name</Description>
								<Length>5</Length>
								<Order>3</Order>
							</OrgCodeElement>
							<OrgCodeElement>
								<Description>Last Name</Description>
								<Length>0</Length>
								<Order>0</Order>
							</OrgCodeElement>
							<OrgCodeElement>
								<Description>Country Code</Description>
								<Length>2</Length>
								<Order>6</Order>
							</OrgCodeElement>
							<OrgCodeElement>
								<Description>UnlocoCode Code</Description>
								<Length>0</Length>
								<Order>0</Order>
							</OrgCodeElement>
							<OrgCodeElement>
								<Description>IataCode Code</Description>
								<Length>0</Length>
								<Order>0</Order>
							</OrgCodeElement>
							<OrgCodeElement>
								<Description>Globally Unique Number</Description>
								<Length>0</Length>
								<Order>0</Order>
							</OrgCodeElement>
							<OrgCodeElement>
								<Description>Code Specific Unique Number</Description>
								<Length>0</Length>
								<Order>0</Order>
							</OrgCodeElement>
						</ArrayOfOrgCodeElement>
						<ArrayOfOrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Receivables</Description>
								<Selected>N</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Payables</Description>
								<Selected>N</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Consignor</Description>
								<Selected>Y</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Consignee</Description>
								<Selected>N</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Carrier</Description>
								<Selected>Y</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Forwarder</Description>
								<Selected>N</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Broker</Description>
								<Selected>N</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Services</Description>
								<Selected>N</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Competitor</Description>
								<Selected>N</Selected>
							</OrgCodeOrgType>
							<OrgCodeOrgType>
								<Description>Sales</Description>
								<Selected>N</Selected>
							</OrgCodeOrgType>
						</ArrayOfOrgCodeOrgType>
					</OrgCodeAlgorithm>"));
				string contents =
					@"<?xml version=""1.0"" encoding=""utf-8""?>
					<RegistryItemOverrides>
						<Items>
							<Item>
								<Name>OrgCodeAlgorithmDefault</Name>
								<Caption>Organization Code - Default Set</Caption>
								<Data>" + data + @"</Data>
							</Item>
						</Items>
					</RegistryItemOverrides>";

				#endregion

				File.WriteAllText(fileWithIncorrectXml.Filename, contents);
				bizo.OverrideLevel = new DefaultOverrideLevel();
				bizo.FilePath = fileWithIncorrectXml.Filename;

				using (var form = new ImportOverridesForm(bizo))
				{
					form.Show();

					var message = (UnitTestUserNotification)Globals.Message;
					message.AddAnswer(DialogResult.No);

					AssertNoExceptionThrown(form.compareButton.PerformClick);

					Assert("Since we said we did not want to continue the form should still be open", form.Visible);
					AssertContains("The file you have imported contains elements not accepted by the current system.", message.LastMessage.Text);
				}
			}
		}

		public void TestCompareButtonErrors()
		{
			var fileDialog = new Mock<IFileDialog>();
			fileDialog.Setup(m => m.CheckFileExists).Returns(true);
			fileDialog.Setup(m => m.UnmappedFileName).Returns("SomePath.xml");

			var bizo = RegistryImportBusinessObjectTest.GetBusinessObject(fileDialog: fileDialog.Object);
			bizo.OverrideLevel = new DefaultOverrideLevel();

			using (bizo.openFileDialog)
			using (var form = new ImportOverridesForm(bizo))
			{
				form.Show();

				AssertErrorShown(form, new IOException(), "There was a problem loading the file");
				AssertErrorShown(form, new DirectoryNotFoundException(), "The file could not be found");
				AssertErrorShown(form, new XmlException(), "The file is corrupt");
			}
		}

		void AssertErrorShown(ImportOverridesForm form, Exception exceptionToThrow, string errorMessage)
		{
			form.BusinessEntity.exceptionToThrowWhenLoadingItemsForTest = exceptionToThrow;
			AssertNoExceptionThrown(form.compareButton.PerformClick);

			var lastMessage = ((UnitTestUserNotification)Globals.Message).LastMessage.Text;

			AssertContains(exceptionToThrow.GetType().ToString(), errorMessage, lastMessage);
		}

		#region Implementation

#pragma warning disable IDE0001 // Need fully qualified namespace due to conflicts between net48 and netcore frameworks
		readonly CargoWise.Common.DisposableList disposables = new CargoWise.Common.DisposableList(1);
#pragma warning restore IDE0001

		protected override Form GetFormToBashCore()
		{
			var registryImportBusinessObject = RegistryImportBusinessObjectTest.GetBusinessObject();
			disposables.Add(registryImportBusinessObject.openFileDialog);

			return new ImportOverridesForm(registryImportBusinessObject);
		}

		void SelectNode(ZTreeView treeView, string path)
		{
			FindOverridesLevelFormTest.SelectNode(treeView, path);
		}

		protected override void TearDown()
		{
			base.TearDown();

			disposables.Dispose();
		}

		#endregion
	}
}
