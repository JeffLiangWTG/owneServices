using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class eDocsUserControlTest : Business.Test.TestCaseWithDocumentFactory
	{
		public void TestAddEdoc()
		{
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(Org))
			using (var file = TempFile.NewWithExtension("txt"))
			{
				System.IO.File.AppendAllText(file.Filename, "hi");
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				form.ExposeAllTabPages();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;

				eDocsUserControl.ShowDialogAndAddValidFiles(true, null, files =>
				{
					AssertEquals(1, files.Length);
					AssertEquals(file.Filename, files[0]);
				});
			}
		}

		public void TestConstructor()
		{
			using (var plugIn = new eDocsPlugInForTesting(Org))
			using (eDocsUserControl control = new eDocsUserControl(plugIn))
			{
				AssertEquals("Plug in is same instance that is passed in", plugIn, control.PlugIn);
				AssertEquals("Grid drag drop target is the same instance that is passed in", plugIn, control.StorageDocsGrid.DragDropTarget);
				AssertEquals("Grid doc manipulation target is the same instance that is passed in", plugIn, control.StorageDocsGrid.DocumentManipulationTarget);
			}
		}

		public void TestOnContextMenuBuilt()
		{
			using var plugIn = new eDocsPlugInForTesting(Org);
			using eDocsUserControl control = new eDocsUserControl(plugIn);
			var testEventTriggered = false;
			control.OnContextMenuBuilt += (s, e) => testEventTriggered = true;
			CombineAssertions(() =>
			{
				control.Visible = false;
				Assert("the variant is not changed as OnContextMenuBuilt is not triggered", !testEventTriggered);
				control.Visible = true;
				Assert("the variant is changed as OnContextMenuBuilt is triggered", testEventTriggered);
			});
		}

		public void TestChangingEDocsPlugin()
		{
			using (var plugIn = new eDocsPlugInForTesting(Org))
			using (var control = new eDocsUserControl(plugIn))
			using (var secondPlugin = new eDocsPlugInForTesting(NewOrgForTest()))
			{
				AssertEquals("Plug in is same instance that is passed in", plugIn, control.PlugIn);
				AssertEquals("Grid drag drop target is the same instance that is passed in", plugIn, control.StorageDocsGrid.DragDropTarget);
				AssertEquals("Grid doc manipulation target is the same instance that is passed in", plugIn, control.StorageDocsGrid.DocumentManipulationTarget);

				control.PlugIn = secondPlugin;

				AssertEquals("Plugin should have updated", secondPlugin, control.PlugIn);
				AssertEquals("Grid drag drop target should have updated", secondPlugin, control.StorageDocsGrid.DragDropTarget);
				AssertEquals("Grid doc manipulation target should have updated", secondPlugin, control.StorageDocsGrid.DocumentManipulationTarget);
			}
		}

		public void TestOnlyShowGrid()
		{
			using (var plugIn = new eDocsPlugInForTesting(Org))
			using (var control = new eDocsUserControl(plugIn))
			using (var secondPlugin = new eDocsPlugInForTesting(NewOrgForTest()))
			{
				AssertEquals("RequiredDocumentsUserControl should be visible", true, control.RequiredDocumentsUserControl.Visible);
				AssertEquals("RelatedParentsGrid should be visible", true, control.RelatedParentsGrid.Visible);
				AssertEquals("DescriptionAndIconPanel should be visible", true, control.DescriptionAndIconPanel.Visible);

				control.OnlyShowGrid = true;

				AssertEquals("RequiredDocumentsUserControl should NOT be visible", false, control.RequiredDocumentsUserControl.Visible);
				AssertEquals("RelatedParentsGrid should NOT be visible", false, control.RelatedParentsGrid.Visible);
				AssertEquals("DescriptionAndIconPanel should NOT be visible", false, control.DescriptionAndIconPanel.Visible);
			}
		}

		public void TestDoubleClickRelatedItems()
		{
			NUnit.Framework.TestCaseHelper.ClearTable(ZArchitecture.Schema.StorageMainSchema.Constants.TableName);
			NUnit.Framework.TestCaseHelper.ClearTable(ZArchitecture.Schema.StorageDocsSchema.Constants.TableName);

			BusinessObject shipment = (BusinessObject)MasterFactory.New<ICommonShipment>();
			using (var form = new ZFormForPlugInTest(shipment))
			{
				OrgHeader consignee = (OrgHeader)MasterFactory.LoadTop1(typeof(OrgHeader), new ZQuery(ZArchitecture.Schema.OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
				shipment["ConsigneePK"] = consignee.PK;
				OrgHeader consignor = (OrgHeader)MasterFactory.LoadTop1(typeof(OrgHeader), new ZQuery(ZArchitecture.Schema.OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
				shipment["ConsignorPK"] = consignor.PK;

				StorageMain main = (StorageMain)MasterFactory.New(typeof(StorageMain));
				main.SM_Type = Core.Constants.DocManagerCodes.Shipment;
				main.SM_ParentFK = shipment.PK;
				main.SM_DB = 1;
				main.Documents.AddNew();

				StorageMain consigneeMain = (StorageMain)MasterFactory.New(typeof(StorageMain));
				consigneeMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
				consigneeMain.SM_ParentFK = ((OrgHeader)shipment["Consignee"]).PK;
				consigneeMain.SM_DB = 1;
				consigneeMain.Documents.AddNew();

				StorageMain consignorMain = (StorageMain)MasterFactory.New(typeof(StorageMain));
				consignorMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
				consignorMain.SM_ParentFK = ((BusinessObject)shipment["Consignor"]).PK;
				consignorMain.SM_DB = 1;
				consignorMain.Documents.AddNew();
				MasterFactory.Save();

				AssertEquals("Precondition: 3 records in StorageMain table", 3, MasterFactory.GetDatabaseCount(typeof(StorageMain)));

				form.Show();
				Assert("Precondition: Plug in should exist", form.PlugIns.Instances.Length > 0);
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				AssertNotNull("Precondition: Tab control should exist", form.TabControl);
				Assert("Precondition: Tab page exists", form.TabControl.TabPages.Count > 0);
				Assert("Precondition: eDocs user control exists", form.TabControl.TabPages[0].Controls.Count > 0);
				eDocsUserControl userControl = (eDocsUserControl)form.TabControl.TabPages[0].Controls[0];

				AssertNotNull("Related parents grid should be bound", userControl.RelatedParentsGrid.ListManager);
				AssertEquals("Related parents Grid should have 3 elements", 3, userControl.RelatedParentsGrid.ListManager.Count);
				AssertEquals("PlugIn's CurrentStorageMainInGrid should be the top level parent", plugIn.CurrentParentMainOnGrid, userControl.RelatedParentsGrid.ListManager.GetCurrent());

				userControl.RelatedParentsGrid.ListManager.Position = 1;
				AssertEquals("PlugIn's CurrentStorageMainInGrid should update to match the current element in the list (Consignee comes first alphabetically)", consigneeMain.PK, ((StorageMain)userControl.RelatedParentsGrid.ListManager.GetCurrent()).PK);

				userControl.RelatedParentsGrid.ListManager.Position = 2;
				AssertEquals("PlugIn's CurrentStorageMainInGrid should update to match the current element in the list (Consignor comes last alphabetically)", consignorMain.PK, ((StorageMain)userControl.RelatedParentsGrid.ListManager.GetCurrent()).PK);

				var mouseEvent = new MouseEventArgs(MouseButtons.Left, 2, userControl.RelatedParentsGrid.GetRowNotificationRectangle(1).X, userControl.RelatedParentsGrid.GetRowNotificationRectangle(1).Y, 0);
				userControl.DoubleClickOn_RelatedParents(userControl.RelatedParentsGrid, mouseEvent);

				AssertEquals("No message has appear (it never gone inside the 'if' statement)", true, eDocsUserControl.testDoubleClick);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("After double click on related eDocs new form with related Organisation has not been openned", true, Application.OpenForms.OfType<MasterFiles.GUI.ZOrganisationsForm>().SingleOrDefault() != null);
				Application.OpenForms.OfType<MasterFiles.GUI.ZOrganisationsForm>().SingleOrDefault().Dispose();
			}
		}

		public void TestDoubleClickRelatedItems_NullReferenceException()
		{
			NUnit.Framework.TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			NUnit.Framework.TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);

			var shipment = (BusinessObject)MasterFactory.New<ICommonShipment>();
			using (var form = new ZFormForPlugInTest(shipment))
			{
				form.Show();

				var userControl = (eDocsUserControl)form.TabControl.TabPages[0].Controls[0];
				userControl.RelatedParentsGrid.ListManager.AddNew();

				var mouseEvent = new MouseEventArgs(MouseButtons.Left, 2, userControl.RelatedParentsGrid.GetRowNotificationRectangle(1).X, userControl.RelatedParentsGrid.GetRowNotificationRectangle(1).Y, 0);
				AssertNoExceptionThrown(() => userControl.DoubleClickOn_RelatedParents(userControl.RelatedParentsGrid, mouseEvent));

				Application.OpenForms.OfType<MasterFiles.GUI.ZOrganisationsForm>().SingleOrDefault()?.Dispose();
			}
		}

		public void TestRelatedItems_DoubleClickNotSupported()
		{
			NUnit.Framework.TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			NUnit.Framework.TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);

			var shipment = (BusinessObject)MasterFactory.New<ICommonShipment>();
			using (var form = new ZFormForPlugInTest(shipment))
			{
				var main = (StorageMain)MasterFactory.New(typeof(StorageMain));
				main.SM_Type = Core.Constants.DocManagerCodes.IncidentRequest;
				main.SM_ParentFK = shipment.PK;
				main.SM_DB = 1;
				main.Documents.AddNew();

				form.ControllerID = ControllerIDs.JobShipment;
				form.Show();

				var userControl = (eDocsUserControl)form.TabControl.TabPages[0].Controls[0];
				userControl.SetControllerNullForTest = true;

				var mouseEvent = new MouseEventArgs(MouseButtons.Left, 2, userControl.RelatedParentsGrid.GetRowNotificationRectangle(0).X, userControl.RelatedParentsGrid.GetRowNotificationRectangle(0).Y, 0);
				userControl.DoubleClickOn_RelatedParents(userControl.RelatedParentsGrid, mouseEvent);

				AssertEquals($"Double click on Related eDocs is not supported in the Shipments module. Please contact {Core.Constants.ProductName} support for more information.", UnitTestUserNotification.Instance.LastMessage.Text);
				Application.OpenForms.OfType<MasterFiles.GUI.ZOrganisationsForm>().SingleOrDefault()?.Dispose();
			}
		}

		public void TestForm()
		{
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(Org))
			{
				form.Show();
				UserIdleWorker.Flush();

				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;

				AssertEquals("UserControl's form reference should be correctly loaded", form, userControl.Form);
			}
		}

		public void TestReadonlyWhenControlIsReadOnly()
		{
			using (var form = new ZFormForPlugInTest(Org))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				form.ExposeAllTabPages();
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;

				var readOnlyToggleControl = userControl as IReadOnlyToggleControl;
				AssertNotNull("eDocsUserControl should implement IReadOnlyToggleControl", readOnlyToggleControl);
				AssertEquals("Precondition: User control is not readonly", false, userControl.ReadOnly);

				readOnlyToggleControl.ReadOnly = true;
				AssertEquals("User control is readonly", true, userControl.ReadOnly);
				AssertEquals("AddFilesButton.ReadOnly", true, userControl.AddEDocsButton.ReadOnly);
				AssertEquals("RefreshButton.ReadOnly", true, userControl.RefreshButton.ReadOnly);
				AssertEquals("StorageDocsGrid.IsEditable", false, userControl.StorageDocsGrid.IsEditable);
			}
		}

		public void TestTickAllDeletedFilesCheckbox()
		{
			using (var form = new ZFormForPlugInTest(Org))
			{
				form.Show();

				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var main = (StorageMain)plugIn.BusinessEntity;
				var doc = main.Documents.AddNew();
				doc.SC_IsDeleted = true;

				var userControl = (eDocsUserControl)plugIn.UserControl;
				userControl.ShowDeletedDocumentsCheckBox.Checked = true;
				AssertEquals("The deleted files MUST be restored if you wish to edit the file content.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				userControl.ShowDeletedDocumentsCheckBox.Checked = false;
				AssertNull("UnTick checkbox should not popup message", UnitTestUserNotification.Instance.LastMessage.Text);

				doc.SC_IsDeleted = false;
				userControl.ShowDeletedDocumentsCheckBox.Checked = true;
				AssertNull("Should not popup message if there's no deleted file", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestReadOnly()
		{
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(Org))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				form.ExposeAllTabPages();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;

				AssertEquals("precondition: User control is not readonly", false, userControl.ReadOnly);
				AssertEquals("DocumentStorageLocationTextBox.ReadOnly", false, userControl.DocumentStorageLocationTextBox.ReadOnly);
				AssertEquals("StorageDocsGrid.IsEditable", true, userControl.StorageDocsGrid.IsEditable);
				AssertEquals("RelatedParentsGrid.ReadOnly", true, userControl.RelatedParentsGrid.ReadOnly);
				AssertEquals("ShowDeletedDocumentsCheckBox.ReadOnly", false, userControl.ShowDeletedDocumentsCheckBox.ReadOnly);
				AssertEquals("RefreshButton.ReadOnly", false, userControl.RefreshButton.ReadOnly);
				AssertEquals("AddFilesButton.ReadOnly", false, userControl.AddEDocsButton.ReadOnly);
			}

			Org.SetReadOnlyIncludingChildren(true);
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(Org))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();
				UserIdleWorker.Flush();
				form.ExposeAllTabPages();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;

				AssertEquals("precondition: User control is readonly", true, userControl.ReadOnly);
				AssertEquals("DocumentStorageLocationTextBox.ReadOnly", true, userControl.DocumentStorageLocationTextBox.ReadOnly);
				AssertEquals("User control should be readonly", true, userControl.ReadOnly);
				AssertEquals("StorageDocsGrid.IsEditable", false, userControl.StorageDocsGrid.IsEditable);
				AssertEquals("ShowDeletedDocumentsCheckBox.ReadOnly", true, userControl.ShowDeletedDocumentsCheckBox.ReadOnly);
				AssertEquals("RefreshButton.ReadOnly", true, userControl.RefreshButton.ReadOnly);
				AssertEquals("AddFilesButton.ReadOnly", true, userControl.AddEDocsButton.ReadOnly);
			}
		}

		#region readonly2 test

		internal class OrgHeader2 : OrgHeader
		{
			public OrgHeader2(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override DocumentSupporter DocumentSupporter
			{
				get
				{
					return new OrgHeaderDocumentSupporter2(this);
				}
			}
		}

		class OrgHeaderDocumentSupporter2 : OrgHeaderDocumentSupporter
		{
			public OrgHeaderDocumentSupporter2(OrgHeader orgHeader)
				: base(orgHeader)
			{
			}

			public override bool StorageDocsAreEditableIfInRelated
			{
				get
				{
					return true;
				}
			}
		}

		public void TestReadOnly2()
		{
			OrgHeader2 org1 = MasterFactory.New<OrgHeader2>();
			OrgHeader2 org2 = MasterFactory.New<OrgHeader2>();
			org1.OH_Code = "11111";
			org2.OH_Code = "22222";
			StorageMain main1 = MasterFactory.NewWithValidTestData<StorageMain>();
			main1.SM_ParentFK = org1.PK;
			main1.SM_Type = "ORG";
			StorageMain main2 = MasterFactory.NewWithValidTestData<StorageMain>();
			main2.SM_ParentFK = org2.PK;
			main2.SM_Type = "ORG";

			MasterFactory.Save();

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org1))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				form.ExposeAllTabPages();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;

				StorageMain sm = form.TopLevelParentMain;
				typeof(StorageMain).GetField("documentOwner", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(sm, org1);
				typeof(StorageMain).GetField("documentOwner", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(main2, org2);

				sm.RelatedParentMains.Add(main2);

				userControl.RelatedParentsGrid.CurrentCell = new DataGridCell(1, 0);
				AssertEquals("StorageDocsGrid.IsEditable", true, userControl.StorageDocsGrid.IsEditable);
			}
		}

		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIconAndDescription_SelectDifferentDocument()
		{
			var pdfDocBytes = DocumentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF");
			var jpgDocBytes = DocumentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\compressed.jpg");
			var fileAssociationRetriever = new FileAssociationRetriever();
			using (var form = new ZFormForPlugInTest(Org))
			{
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var currentIcon = userControl.IconPictureBox.Image;

				form.Show();
				var storageMain = (StorageMain)plugIn.BusinessEntity;
				var addedPDFDoc = storageMain.AddFileOrDocument(pdfDocBytes, new AddFileOrDocumentDto { FileName = "Sample.PDF", DocumentType = "PDF" });

				AssertEquals("Description text should be PDF", fileAssociationRetriever.GetFriendlyDocumentName(addedPDFDoc.SC_DataType), userControl.ProgramNameLabel.Text);
				AssertNotNull("Icon image exists", userControl.IconPictureBox.Image);
				AssertImageEquals("Icon image should be PDF image", fileAssociationRetriever.GetIconForExtension(addedPDFDoc.SC_DataType), userControl.IconPictureBox.Image);

				var addedJPGDoc = storageMain.AddFileOrDocument(jpgDocBytes, new AddFileOrDocumentDto { FileName = "compressed.jpg" });

				userControl.StorageDocsGrid.CurrentRowIndex = 1;

				AssertEquals("Description text should be JPG", fileAssociationRetriever.GetFriendlyDocumentName(addedJPGDoc.SC_DataType), userControl.ProgramNameLabel.Text);
				AssertImageEquals("Icon image should be JPG image", fileAssociationRetriever.GetIconForExtension(addedJPGDoc.SC_DataType), userControl.IconPictureBox.Image);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIconAndDescription_FirstDocumentConfiguredBeforeDisplayingIcon()
		{
			var testDocBytes = DocumentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF");
			var fileAssociationRetriever = new FileAssociationRetriever();
			using (var form = new ZFormForPlugInTest(Org))
			{
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var currentIcon = userControl.IconPictureBox.Image;

				form.Show();
				var storageMain = (StorageMain)plugIn.BusinessEntity;
				var doc = storageMain.AddFileOrDocument(testDocBytes, new AddFileOrDocumentDto { FileName = "Sample.PDF", DocumentType = "PDF" });

				AssertEquals("Description text should be PDF", fileAssociationRetriever.GetFriendlyDocumentName(doc.SC_DataType), userControl.ProgramNameLabel.Text);
				AssertNotNull("Icon image exists", userControl.IconPictureBox.Image);
				AssertImageEquals("Icon image should be PDF image", fileAssociationRetriever.GetIconForExtension(doc.SC_DataType), userControl.IconPictureBox.Image);
			}
		}

		public void TestChangingTopLevelParentMeansMainGridIsReadOnly()
		{
			StorageMain main = (StorageMain)MasterFactory.NewWithValidTestData(typeof(StorageMain));
			main.SM_ParentFK = Org.PK;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			main.Documents.AddNew();

			BusinessObject shipment = (BusinessObject)MasterFactory.New<ICommonShipment>();
			shipment["ConsigneePK"] = Org.PK;
			StorageMain main2 = (StorageMain)MasterFactory.NewWithValidTestData(typeof(StorageMain));
			main2.SM_ParentFK = shipment.PK;
			main2.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			main2.Documents.AddNew();

			MasterFactory.Save();

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			{
				form.Show();
				UserIdleWorker.Flush();

				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;
				StorageMain topLevelParentMain = (StorageMain)plugIn.BusinessEntity;

				AssertNotNull("Precondition: List manager shouldn't be null", userControl.RelatedParentsGrid.ListManager);
				userControl.RelatedParentsGrid.ListManager.Position = 0;
				AssertEquals("User Control's top level parent should be the shipment's pk", shipment.PK, topLevelParentMain.SM_ParentFK);
				AssertEquals("Precondition: storage main at the top of the list should be the TopLevelParentMain", topLevelParentMain.PK, ((StorageMain)userControl.RelatedParentsGrid.ListManager.List[0]).PK);
				AssertEquals("The main grid should be editable when we are on the StorageMain object for the BizO behind the form", true, userControl.StorageDocsGrid.IsEditable);
				AssertEquals("Editable label should not be visible", false, userControl.NotEditableLabel.Visible);
				AssertEquals("StorageDocsGrid should not be read only", false, userControl.StorageDocsGrid.ReadOnly);
				AssertEquals("AddEDocsButton should not be read only", false, userControl.AddEDocsButton.ReadOnly);

				userControl.RelatedParentsGrid.ListManager.Position = 1;
				AssertEquals("On changing index, the main grid should not be editable", false, userControl.StorageDocsGrid.IsEditable);
				AssertEquals("Editable label should be visible", true, userControl.NotEditableLabel.Visible);
				AssertEquals("StorageDocsGrid should be read only", true, userControl.StorageDocsGrid.ReadOnly);
				AssertEquals("AddEDocsButton should be read only", true, userControl.AddEDocsButton.ReadOnly);
			}
		}

		public void TestShowDocumentRelatedCheckBoxesSameYCoordinateAndHeight()
		{
			using (var form = new ZFormForPlugInTest(Org))
			{
				form.Show();
				var userControl = (eDocsUserControl)form.TabControl.TabPages[0].Controls[0];
				var requiredYCoordinate = userControl.SpecificDocumentLabel.Location.Y;
				var requiredHeight = userControl.SpecificDocumentLabel.Size.Height;
				var checkBoxes = new List<ZCheckBox>() 
				{ 
					userControl.BranchSpecificCheckBox,
					userControl.CompanySpecificCheckBox,
					userControl.DepartmentSpecificCheckBox,
					userControl.ShowDeletedDocumentsCheckBox,
					userControl.PreviewDocumentsCheckBox
				};
					
				Assert("All related checkboxes have the same Y-coordinate", checkBoxes.All(checkbox => checkbox.Location.Y == requiredYCoordinate));
				Assert("All related checkboxes have the same checkbox height", checkBoxes.All(checkbox => checkbox.Size.Height == requiredHeight));
			}
		}

		public void TestChangingTopLevelParent_NotEditableLabelIsInvisibleIfTopLevelParentIsSelected()
		{
			var main = (StorageMain)MasterFactory.NewWithValidTestData(typeof(StorageMain));
			main.SM_ParentFK = Org.PK;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			main.Documents.AddNew();

			var shipment = (BusinessObject)MasterFactory.New<ICommonShipment>();
			shipment["ConsigneePK"] = Org.PK;
			var main2 = (StorageMain)MasterFactory.NewWithValidTestData(typeof(StorageMain));
			main2.SM_ParentFK = shipment.PK;
			main2.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			main2.Documents.AddNew();

			MasterFactory.Save();

			using (var form = new ZFormForPlugInTest(shipment))
			{
				form.Show();
				UserIdleWorker.Flush();

				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var topLevelParentMain = (StorageMain)plugIn.BusinessEntity;

				AssertNotNull("Precondition: List manager shouldn't be null", userControl.RelatedParentsGrid.ListManager);
				userControl.RelatedParentsGrid.ListManager.Position = 0;
				AssertEquals("User Control's top level parent should be the shipment's pk", shipment.PK, topLevelParentMain.SM_ParentFK);
				AssertEquals("Precondition: storage main at the top of the list should be the TopLevelParentMain", topLevelParentMain.PK, ((StorageMain)userControl.RelatedParentsGrid.ListManager.List[0]).PK);
				AssertEquals("The main grid should be editable when we are on the StorageMain object for the BizO behind the form", true, userControl.StorageDocsGrid.IsEditable);
				AssertEquals("Editable label should not be visible", false, userControl.NotEditableLabel.Visible);
				AssertEquals("StorageDocsGrid should not be read only", false, userControl.StorageDocsGrid.ReadOnly);
				AssertEquals("AddEDocsButton should not be read only", false, userControl.AddEDocsButton.ReadOnly);

				userControl.RelatedParentsGrid.ListManager.Position = 1;
				AssertEquals("On changing index, the main grid should not be editable", false, userControl.StorageDocsGrid.IsEditable);
				AssertEquals("NotEditableLabel should be visible", true, userControl.NotEditableLabel.Visible);
				AssertEquals("StorageDocsGrid should be read only", true, userControl.StorageDocsGrid.ReadOnly);
				AssertEquals("AddEDocsButton should be read only", true, userControl.AddEDocsButton.ReadOnly);

				Env.Security.eDocsModify.IsAllowed = false;
				userControl.RelatedParentsGrid.ListManager.Position = 0;
				AssertEquals("User Control's top level parent should be the shipment's pk", shipment.PK, topLevelParentMain.SM_ParentFK);
				AssertEquals("NotEditableLabel should not be visible", false, userControl.NotEditableLabel.Visible);
				AssertEquals("The grid is not be editable", false, userControl.StorageDocsGrid.IsEditable);
			}
		}

		public void TestIcon()
		{
			StorageMain main = (StorageMain)MasterFactory.NewWithValidTestData(typeof(StorageMain));
			main.SM_ParentFK = Org.PK;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			StorageFile file = main.Files.AddNew();
			file.SC_DataType = "PDF";
			main.Documents.AddNew();
			MasterFactory.Save();

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(Org))
			{
				form.Show();
				UserIdleWorker.Flush();

				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;
				StorageMain topLevelParentMain = (StorageMain)plugIn.BusinessEntity;

				AssertNotNull("Precondition: List manager shouldn't be null", userControl.StorageDocsGrid.ListManager);
				userControl.StorageDocsGrid.ListManager.Position = 0;
				Image currentIcon = userControl.IconPictureBox.Image;
				AssertNotNull("icon should exist", currentIcon);

				userControl.StorageDocsGrid.ListManager.Position = 1;
				Image changedIcon = userControl.IconPictureBox.Image;
				AssertNotNull("icon should exist", changedIcon);
				Assert("Icons are different when the list manager position changes", currentIcon != changedIcon);
			}
		}

		public void TestIsStorageDocGridWorkingWithAllDepartmentCheckBox()
		{
			var shipment = MasterFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(ICommonShipment)));
			var consol = MasterFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(ICommonConsol)));

			var pivot = MasterFactory.New(ObjectFactory.GetType(typeof(IJobConShipLink)));
			pivot[JobConShipLinkSchema.JN_JK] = consol.PK;
			pivot[JobConShipLinkSchema.JN_JS] = shipment.PK;

			var storageMain = (StorageMain)MasterFactory.NewWithValidTestData(typeof(StorageMain));
			storageMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			storageMain.SM_ParentFK = shipment.PK;

			var departmentKey = ZGuid.NewZGuid();

			var file = storageMain.Files.AddNew();
			file.SC_DataType = "PDF";

			var departmentSpecificFile = storageMain.Files.AddNew();
			departmentSpecificFile.SC_DataType = "PDF";
			departmentSpecificFile.SC_GE_Department = departmentKey;
			MasterFactory.Save();

			using (var consolForm = new ZFormForPlugInTest(consol))
			{
				consolForm.Show();
				consolForm.ExposeAllTabPages();

				var plugIn = (eDocsPlugIn)consolForm.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;

				AssertEquals("2 items should be in RelatedParentGrid", 2, userControl.RelatedParentsGrid.ListManager.List.Count);
				AssertEquals("NO items should be in StorageDocsGrid for consolidation", 0, userControl.StorageDocsGrid.ListManager.List.Count);

				userControl.RelatedParentsGrid.ListManager.Position = 1;
				AssertEquals("1 file should be shown for shipment in consolisation.", 1, userControl.StorageDocsGrid.ListManager.List.Count);

				userControl.DepartmentSpecificCheckBox.Checked = true;
				AssertEquals("2 file should be shown for shipment in consolisation.", 2, userControl.StorageDocsGrid.ListManager.List.Count);
			}
		}

		public void TestIsStorageDocGridWorkingWithAllBranchCheckBox()
		{
			var shipment = MasterFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(ICommonShipment)));
			var consol = MasterFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(ICommonConsol)));

			var pivot = MasterFactory.New(ObjectFactory.GetType(typeof(IJobConShipLink)));
			pivot[JobConShipLinkSchema.JN_JK] = consol.PK;
			pivot[JobConShipLinkSchema.JN_JS] = shipment.PK;

			var storageMain = MasterFactory.NewWithValidTestData<StorageMain>();
			storageMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			storageMain.SM_ParentFK = shipment.PK;

			var branchKey = ZGuid.NewZGuid();

			var file = storageMain.Files.AddNew();
			file.SC_DataType = "PDF";

			var branchSpecificFile = storageMain.Files.AddNew();
			branchSpecificFile.SC_DataType = "PDF";
			branchSpecificFile.SC_GB_Branch = branchKey;
			MasterFactory.Save();

			using (var consolForm = new ZFormForPlugInTest(consol))
			{
				consolForm.Show();
				consolForm.ExposeAllTabPages();

				var plugIn = (eDocsPlugIn)consolForm.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;

				AssertEquals("2 items should be in RelatedParentGrid", 2, userControl.RelatedParentsGrid.ListManager.List.Count);
				AssertEquals("NO items should be in StorageDocsGrid for consolidation", 0, userControl.StorageDocsGrid.ListManager.List.Count);

				userControl.RelatedParentsGrid.ListManager.Position = 1;
				AssertEquals("1 file should be shown for shipment in consolisation.", 1, userControl.StorageDocsGrid.ListManager.List.Count);

				userControl.BranchSpecificCheckBox.Checked = true;
				AssertEquals("2 file should be shown for shipment in consolisation.", 2, userControl.StorageDocsGrid.ListManager.List.Count);
			}
		}

		public void TestIsStorageDocGridWorkingWithAllCompanyCheckBox()
		{
			var shipment = MasterFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(ICommonShipment)));
			var consol = MasterFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(ICommonConsol)));

			var pivot = MasterFactory.New(ObjectFactory.GetType(typeof(IJobConShipLink)));
			pivot[JobConShipLinkSchema.JN_JK] = consol.PK;
			pivot[JobConShipLinkSchema.JN_JS] = shipment.PK;

			var storageMain = MasterFactory.NewWithValidTestData<StorageMain>();
			storageMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			storageMain.SM_ParentFK = shipment.PK;

			var companyKey = ZGuid.NewZGuid();

			var file = storageMain.Files.AddNew();
			file.SC_DataType = "PDF";

			var branchSpecificFile = storageMain.Files.AddNew();
			branchSpecificFile.SC_DataType = "PDF";
			branchSpecificFile.SC_GC_Company = companyKey;
			MasterFactory.Save();

			using (var consolForm = new ZFormForPlugInTest(consol))
			{
				consolForm.Show();
				consolForm.ExposeAllTabPages();

				var plugIn = (eDocsPlugIn)consolForm.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;

				AssertEquals("2 items should be in RelatedParentGrid", 2, userControl.RelatedParentsGrid.ListManager.List.Count);
				AssertEquals("NO items should be in StorageDocsGrid for consolidation", 0, userControl.StorageDocsGrid.ListManager.List.Count);
				userControl.RelatedParentsGrid.ListManager.Position = 1;
				AssertEquals("1 file should be shown for shipment in consolisation.", 1, userControl.StorageDocsGrid.ListManager.List.Count);

				userControl.CompanySpecificCheckBox.Checked = true;
				AssertEquals("2 file should be shown for shipment in consolisation.", 2, userControl.StorageDocsGrid.ListManager.List.Count);
			}
		}

		public void TestIsStorageDocGridWorkingWithDeletedCheckBox()
		{
			var shipment = MasterFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(ICommonShipment)));
			var consol = MasterFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(ICommonConsol)));

			var pivot = MasterFactory.New(ObjectFactory.GetType(typeof(IJobConShipLink)));
			pivot[JobConShipLinkSchema.JN_JK] = consol.PK;
			pivot[JobConShipLinkSchema.JN_JS] = shipment.PK;

			var storageMain = MasterFactory.NewWithValidTestData<StorageMain>();
			storageMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			storageMain.SM_ParentFK = shipment.PK;

			var file = storageMain.Files.AddNew();
			file.SC_DataType = "PDF";

			var deletedFile = storageMain.Files.AddNew();
			deletedFile.SC_DataType = "PDF";
			MasterFactory.Save();

			deletedFile.DeleteQuietly();
			MasterFactory.Save();

			using (var consolForm = new ZFormForPlugInTest(consol))
			{
				consolForm.Show();
				consolForm.ExposeAllTabPages();

				var plugIn = (eDocsPlugIn)consolForm.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;

				AssertEquals("2 items should be in RelatedParentGrid", 2, userControl.RelatedParentsGrid.ListManager.List.Count);
				AssertEquals("NO items should be in StorageDocsGrid for consolidation", 0, userControl.StorageDocsGrid.ListManager.List.Count);

				userControl.RelatedParentsGrid.ListManager.Position = 1;
				AssertEquals("1 file should be shown for shipment in consolisation.", 1, userControl.StorageDocsGrid.ListManager.List.Count);

				userControl.ShowDeletedDocumentsCheckBox.Checked = true;
				AssertEquals("2 file should be shown for shipment in consolisation.", 2, userControl.StorageDocsGrid.ListManager.List.Count);
			}
		}

		public void TestRelatedParentsGridColourDeciding()
		{
			StorageMain main = (StorageMain)MasterFactory.NewWithValidTestData(typeof(StorageMain));
			main.SM_ParentFK = Org.PK;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			main.Documents.AddNew();

			BusinessObject shipment = (BusinessObject)MasterFactory.New<ICommonShipment>();
			shipment["ConsigneePK"] = Org.PK;
			StorageMain main2 = (StorageMain)MasterFactory.NewWithValidTestData(typeof(StorageMain));
			main2.SM_ParentFK = shipment.PK;
			main2.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			main2.Documents.AddNew();

			MasterFactory.Save();

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(shipment))
			{
				form.Show();
				UserIdleWorker.Flush();
				form.ExposeAllTabPages();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;

				AssertEquals("Top level parent should have Info colour", SystemColors.Info, GetGridRowColour(userControl.RelatedParentsGrid, userControl.RelatedParentsGrid.ListManager.List[0]));
				AssertEquals("not top level parent should have ordinary colour", Color.Empty, GetGridRowColour(userControl.RelatedParentsGrid, userControl.RelatedParentsGrid.ListManager.List[1]));
			}
		}

		public void TestStorageDocsGridColourDeciding_ForDeletedDocument()
		{
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(Org))
			{
				form.Show();
				form.ExposeAllTabPages();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;
				StorageMain topLevelParentMain = (StorageMain)plugIn.BusinessEntity;

				StorageDocs document = topLevelParentMain.Documents.AddNew();
				document.DeleteQuietly();
				MasterFactory.Save();

				topLevelParentMain.ViewIncludesDeletedDocuments = true;
				AssertEquals("Deleted rows should be Crimson", Color.Crimson, GetGridRowColour(userControl.StorageDocsGrid, document));
			}
		}

		public void TestStorageDocsGridColourDeciding_ForUnreadDocument()
		{
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(Org))
			{
				form.Show();
				form.ExposeAllTabPages();
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;
				StorageMain topLevelParentMain = (StorageMain)plugIn.BusinessEntity;

				StorageDocs document = topLevelParentMain.Documents.AddNew();
				document.SC_DocType = DocTypeWithForceUserToRead.RT_DocType;
				DocTypeWithForceUserToRead.Factory.Save();
				topLevelParentMain.Factory.Save();

				using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("XXX"))
				{
					AssertEquals("Document should be unread for the test", true, document.RequiresUserToRead);
					AssertEquals("An 'unread related document' should be Blue", Color.SkyBlue, GetGridRowColour(userControl.StorageDocsGrid, document));

					document.NotifyReadByUser();
					AssertEquals("Document should be unread for the test", false, document.RequiresUserToRead);
					AssertEquals("After the document has been read it should be the normal grid colour again", Color.Empty, GetGridRowColour(userControl.StorageDocsGrid, document));
				}
			}
		}

		public void TestStorageDocsGrid_HideWhenRelatedSecurityDenied()
		{
			var staff = MasterFactory.NewWithValidTestData<GlbStaff>();
			var person = MasterFactory.NewWithValidTestData<GlbPerson>();

			staff.GS_PER = person.PK;

			var main = MasterFactory.NewWithValidTestData<StorageMain>();
			main.SM_ParentFK = person.PK;
			main.SM_Type = Core.Constants.DocManagerCodes.Person;
			main.Documents.AddNew();
			var main2 = MasterFactory.NewWithValidTestData<StorageMain>();
			main2.SM_ParentFK = staff.PK;
			main2.SM_Type = Core.Constants.DocManagerCodes.Staff;
			main2.Documents.AddNew();

			MasterFactory.Save();

			Env.Security.ViewStaffeDocs.IsAllowed = true;

			using (var form = new ZFormForPlugInTest(person))
			{
				form.Show();

				form.ExposeAllTabPages();
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;

				userControl.RelatedParentsGrid.SelectSingleElement(main2);
				AssertEquals(true, userControl.StorageDocsGrid.Visible);
				userControl.RelatedParentsGrid.SelectSingleElement(main);
				AssertEquals(true, userControl.StorageDocsGrid.Visible);
			}

			Env.Security.ViewStaffeDocs.IsAllowed = false;

			using (var form = new ZFormForPlugInTest(person))
			{
				form.Show();

				form.ExposeAllTabPages();
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;

				userControl.RelatedParentsGrid.SelectSingleElement(main2);
				AssertEquals(false, userControl.StorageDocsGrid.Visible);
				userControl.RelatedParentsGrid.SelectSingleElement(main);
				AssertEquals(true, userControl.StorageDocsGrid.Visible);
			}
		}

		public void TestVisibilityOfRequiredDocumentsWithIDocsAndCartageParent()
		{
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(Org))
			{
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;

				DocumentFactory docFactory = new DocumentFactoryProvider().GetFactory(Factory);
				BusinessObject test1 = docFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));

				Assert("Order is an IDocsAndCartageParent", test1 is IDocsAndCartageParent);

				StorageMain testMain = docFactory.NewWithValidTestData<StorageMain>();
				docFactory.Save();

				testMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
				testMain.SM_ParentFK = test1.PK;
				userControl.SetDataBinding(testMain, "");
				Assert("Required documents should be visible for an IDocsAndCartageParent.", userControl.RequiredDocumentsUserControl.Visible);
			}
		}

		public void TestVisibilityOfGenerateNumberButton()
		{
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(Org))
			{
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;

				DocumentFactory docFactory = new DocumentFactoryProvider().GetFactory(Factory);
				BusinessObject test1 = docFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));

				Assert("Order is an IDocsAndCartageParent", test1 is IDocsAndCartageParent);

				StorageMain testMain = docFactory.NewWithValidTestData<StorageMain>();
				docFactory.Save();

				testMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
				testMain.SM_ParentFK = test1.PK;

				Registry.Business.FreightDataRegistry.Instance.StorageNumberButtonActivation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				userControl.SetDataBinding(testMain, "");

				Assert("Required documents should be visible for an IDocsAndCartageParent.", userControl.GenerateNumberButton.Visible);
			}
		}

		public void TestVisibilityOfRequiredDocumentsWithIHaveRequiredDocuments()
		{
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(Org))
			{
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;

				DocumentFactory docFactory = new DocumentFactoryProvider().GetFactory(Factory);
				BusinessObject test2 = docFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IOrder)));

				Assert("Order is an IHaveRequiredDocuments", test2 is IHaveRequiredDocuments);

				StorageMain testMain = docFactory.NewWithValidTestData<StorageMain>();
				docFactory.Save();

				testMain.SM_Type = Core.Constants.DocManagerCodes.Order;
				testMain.SM_ParentFK = test2.PK;
				userControl.SetDataBinding(testMain, "");
				Assert("Required documents should be visible for an IHaveRequiredDocuments.", userControl.RequiredDocumentsUserControl.Visible);
			}
		}

		public void TestGridContainsDocTypeDescriptionColumn()
		{
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(Org))
			{
				form.Show();

				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;
				DocumentFactory docFactory = new DocumentFactoryProvider().GetFactory(Factory);
				BusinessObject test1 = docFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));

				StorageMain testMain = docFactory.NewWithValidTestData<StorageMain>();
				docFactory.Save();

				testMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
				testMain.SM_ParentFK = test1.PK;
				userControl.SetDataBinding(testMain, "");
				Assert(userControl.StorageDocsGrid.Columns.Contains("SC_DocType_Description"));
				Assert(!userControl.StorageDocsGrid.Columns["SC_DocType_Description"].IsVisible);
			}
		}
		public void TestGridContainsAuditColumnsGroup()
		{
			var expectedColumnsOfAuditGroup = new List<string> 
			{
				"SC_SystemLastEditTimeUtc",
				"SC_SystemCreateUser",
				"SC_SystemCreateUserFullName",
				"SC_SystemCreateTimeUtc",
				"SC_LastEditingUserFullName",
				"SC_LastEditingUser"
			};
			using (var form = new ZFormForPlugInTest(Org))
			{
				form.Show();
				var userControl = (eDocsUserControl)form.TabControl.TabPages[0].Controls[0];
				var auditColumns = userControl.StorageDocsGrid.Columns.Where(col => col.GroupName.Caption == "Audit Details");
				Assert("All columns names are expected to be part of Audit Details", auditColumns.All(col => expectedColumnsOfAuditGroup.Contains(col.ColumnName)));
				Assert("Audit Details group has six columns", auditColumns.Count() == 6);
				Assert("Audit Details columns are not visible by default", auditColumns.Count(col => !col.IsVisible) == 6);
			}
		}

			public void TestVisibilityOfRequiredDocumentsWithOtherBizO()
		{
			using (ZFormForPlugInTest form = new ZFormForPlugInTest(Org))
			{
				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;

				DocumentFactory docFactory = new DocumentFactoryProvider().GetFactory(Factory);
				BusinessObject test3 = docFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingConsol)));

				Assert("Consol is an IDocsAndCartageParent or an IHaveRequiredDocuments", (test3 is IDocsAndCartageParent || test3 is IHaveRequiredDocuments));

				StorageMain testMain = docFactory.NewWithValidTestData<StorageMain>();
				docFactory.Save();

				testMain.SM_Type = Core.Constants.DocManagerCodes.Vessel;
				testMain.SM_ParentFK = test3.PK;
				userControl.SetDataBinding(testMain, "");
				Assert("Required documents should not be visible for a Voyage (doesn't implement either interface).", !userControl.RequiredDocumentsUserControl.Visible);
			}
		}

		public void TestRefreshButton_Click()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (ZFormForPlugInTest form = new ZFormForPlugInTest(org))
			{
				form.Show();

				eDocsPlugIn plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				eDocsUserControl userControl = (eDocsUserControl)plugIn.UserControl;

				var newDoc = org.DocManagerInfo().AddFileOrDocument(new byte[] { 1, 1, 1, 1 }, "Sample.txt", "PUB");
				newDoc.Description = "Sample file";

				org.OH_Code = "TSTORG";
				org.Validation.ValidateAll();
				Assert("Org should have validation errors", org.HasErrors);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				userControl.RefreshButton.PerformClick();
				Assert("Org change should not be saved", org.HasChanges);
				Assert("New document should not be saved", !((BusinessObject)newDoc).IsInDatabase);

				org.OH_FullName = "Test Org";
				org.OH_RL_NKClosestPort = "AUSYD";
				org.Addresses[0].OA_Address1 = "abcd";
				org.Addresses[0].OA_PostCode = "2000";
				org.Validation.ValidateAll();
				Assert("Org should have no validation errors", !org.HasErrors);
				Assert("New document should have no validation errors", !((BusinessObject)newDoc).HasErrors);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				userControl.RefreshButton.PerformClick();
				Assert("Org change should be saved", !org.HasChanges);
				Assert("New document should be saved", ((BusinessObject)newDoc).IsInDatabase);
			}
		}

		public void TestRefreshAfterHostBusinessObjectDeleted()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var form = new ZFormForPlugInTest(org))
			{
				form.Show();

				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;

				var otherFactory = new BusinessObjectFactory();
				otherFactory.RefreshEnabled = false;
				var orgInOtherFactory = otherFactory.Load<OrgHeader>(org.PK);
				orgInOtherFactory.Delete();
				otherFactory.Save();

				userControl.RefreshButton.PerformClick();

				AssertEquals("This Organization has been deleted by another user or process. This form will be closed now.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, form.IsDisposed);
			}
		}

		public void TestPreviewDocumentsCheckBox_Click()
		{
			using (var form = new ZFormForPlugInTest(Org))
			{
				form.Show();
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;

				AssertEquals("documentPreviewPanel visibility is initially off", false, userControl.DocumentPreviewPanel.Visible);
				userControl.PreviewDocumentsCheckBox.Checked = true;
				AssertEquals("documentPreviewPanel is visible", true, userControl.DocumentPreviewPanel.Visible);
				userControl.PreviewDocumentsCheckBox.Checked = false;
				AssertEquals("documentPreviewPanel is not visible", false, userControl.DocumentPreviewPanel.Visible);
			}
		}

		public void TestPreviewDocumentsCheckBoxPreviewEnabled()
		{
			var user1 = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "E"));
			var user2 = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "~BP"));
			DocManagerRegistry.Instance.EDocsPreviewEnabled.SetValue(user1.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			DocManagerRegistry.Instance.EDocsPreviewEnabled.SetValue(user2.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var getPreviewEnabled = () =>
			{
				return DocManagerRegistry.Instance.EDocsPreviewEnabled.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentUser?.PK ?? Guid.Empty, Guid.Empty, Guid.Empty);
			};

			using (Env.SetTemporaryUserContext(user1.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				using var form1 = new ZFormForPlugInTest(Org);
				form1.Show();
				var previewCheckBox1 = (form1.PlugIns.Instances[0].UserControl as eDocsUserControl).PreviewDocumentsCheckBox;
				AssertEquals("Checkbox corresponds to registry value, not enabled as set initially", false, previewCheckBox1.Checked);

				previewCheckBox1.Checked = true;
				AssertEquals("Preview should be enabled as user1 ticked the checkbox.", true, getPreviewEnabled());

				using (Env.SetTemporaryUserContext(user2.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					AssertEquals("Preview should be disabled for user2 even if user1 ticked the checkbox.", false, getPreviewEnabled());
				}

				// restart form
				form1.ForceClose();
				form1.Show();

				AssertEquals("Registry remembered checkbox was checked", true, getPreviewEnabled());
				AssertEquals("Checkbox uses registry checked value", true, previewCheckBox1.Checked);

				using (Env.SetTemporaryUserContext(user2.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var orgInAnotherFactory = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Org.PK));
					using var form2 = new ZFormForPlugInTest(orgInAnotherFactory);
					form2.Show();
					var previewCheckBox2 = (form2.PlugIns.Instances[0].UserControl as eDocsUserControl).PreviewDocumentsCheckBox;
					AssertEquals("Checkbox corresponds to registry value, not enabled as set initially.", false, getPreviewEnabled());

					previewCheckBox2.Checked = true;
					AssertEquals("Preview should be enabled as user2 ticked the checkbox.", true, getPreviewEnabled());

					// Set preview enabled to false for user2, then we can check user1 is not affected by this change.
					previewCheckBox2.Checked = false;
				}

				AssertEquals("Preview should be disabled for user1 even if user2 ticked the checkbox.", true, getPreviewEnabled());
			}
		}

		public void TestPreviewDocumentsCheckBox_EditableInViewMode()
		{
			using (var form = new ZFormForPlugInTest(Org))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();
				form.ExposeAllTabPages();
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;

				AssertEquals("User control should be readonly", true, userControl.ReadOnly);
				AssertEquals("PreviewDocumentCheckBox should be editable in view mode", false, userControl.PreviewDocumentsCheckBox.ReadOnly);
			}
		}

		public void TestResizingParentControlMaintainsDocumentPreviewPanelProportions()
		{
			using (var form = new ZFormForPlugInTest(Org))
			{
				form.Show();
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var previewPanel = userControl.DocumentPreviewPanel;

				AssertEquals("preview panel initial height is same as the edocs user control", userControl.Size.Height, previewPanel.Size.Height);
				AssertEquals("preview panel initial width is 0.8 * the width of the edocs user control", (int)(userControl.Size.Height * 0.8), previewPanel.Size.Width);
				userControl.Size = new Size(4000, 8000);
				AssertEquals("preview panel height is same as the edocs user control", userControl.Size.Height, previewPanel.Size.Height);
				AssertEquals("preview panel width is 0.8 * the width of the edocs user control", (int)(userControl.Size.Height * 0.8), previewPanel.Size.Width);
				userControl.Size = new Size(1000, 2000);
				AssertEquals("preview panel height is same as the edocs user control", userControl.Size.Height, previewPanel.Size.Height);
				AssertEquals("preview panel width is 0.8 * the width of the edocs user control", (int)(userControl.Size.Height * 0.8), previewPanel.Size.Width);
			}
		}

		public void TestDocumentPreview_NoEdocs()
		{
			using (var form = new ZFormForPlugInTest(Org))
			{
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var documentPreview = userControl.DocumentPreview;

				form.Show();
				userControl.PreviewDocumentsCheckBox.Checked = true;

				AssertNull("The graphical display is cleared", documentPreview.PageSelector);
				AssertEquals("The graphical diplay is readonly", true, documentPreview.ReadOnly);
			}
		}

		public void TestPreventReEntrancy_DocumentPreview()
		{
			using (var form = new ZFormForPlugInTest(Org))
			{
				form.BindingContext = new BindingContext();
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var documentPreview = userControl.DocumentPreview;

				form.Show();
				userControl.PreviewDocumentsCheckBox.Checked = true;

				AssertNull("The graphical display is cleared", documentPreview.PageSelector);
				AssertEquals("The graphical diplay is readonly", true, documentPreview.ReadOnly);
			}
		}

		public void TestPreventReEntrancy_AllCompanies()
		{
			AssertCheckBox("CompanySpecificCheckBox");
		}

		public void TestPreventReEntrancy_AllBranches()
		{
			AssertCheckBox("BranchSpecificCheckBox");
		}

		public void TestPreventReEntrancy_AllDepartments()
		{
			AssertCheckBox("DepartmentSpecificCheckBox");
		}

		public void TestPreventReEntrancy_AllDeleted()
		{
			AssertCheckBox("ShowDeletedDocumentsCheckBox");
		}

		void AssertCheckBox(string checkboxName)
		{
			var consol = MasterFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(ICommonConsol)));
			MasterFactory.Save();

			using (var consolForm = new ZFormForPlugInTest(consol))
			{
				consolForm.BindingContext = new BindingContext();
				consolForm.Show();
				consolForm.ExposeAllTabPages();

				var plugIn = (eDocsPlugIn)consolForm.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;

				var checkbox = (ZCheckBox)consolForm.Controls.Find(checkboxName, true).Single();
				checkbox.Checked = true;
				AssertEquals("0 file should be shown for shipment in consolisation.", 0, userControl.StorageDocsGrid.ListManager.List.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentPreview_AddOneEdoc()
		{
			var testDocBytes = DocumentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF");
			using (var form = new ZFormForPlugInTest(Org))
			{
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var documentPreview = userControl.DocumentPreview;
				var storageMain = (StorageMain)plugIn.BusinessEntity;

				form.Show();
				userControl.PreviewDocumentsCheckBox.Checked = true;

				var doc = storageMain.AddFileOrDocument(testDocBytes, new AddFileOrDocumentDto { FileName = "Sample.PDF", DocumentType = "PDF" });

				var previewedDocument = documentPreview.Document;
				var image = documentPreview.imageProvider.Document;

				AssertEquals("The storage document is the current document", doc.GetFileNameOnlyWithExtension(), previewedDocument.GetFileNameOnlyWithExtension());
				AssertEquals("The graphical display is not readonly", false, documentPreview.ReadOnly);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentPreviewEdoc_CurrentEdocChanged()
		{
			var testDocBytes1 = DocumentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF");
			var testDocBytes2 = DocumentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\colored.tif");
			using (var form = new ZFormForPlugInTest(Org))
			{
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var documentPreview = userControl.DocumentPreview;

				var storageMain = (StorageMain)plugIn.BusinessEntity;
				var doc1 = storageMain.AddFileOrDocument(testDocBytes1, new AddFileOrDocumentDto { FileName = "Sample.PDF", DocumentType = "PDF" });
				var doc2 = storageMain.AddFileOrDocument(testDocBytes2, new AddFileOrDocumentDto { FileName = "colored.tif" });

				form.Show();
				userControl.PreviewDocumentsCheckBox.Checked = true;

				var currentEdoc = userControl.StorageDocsGrid.CurrentElement;
				AssertEquals("Precondition: The current document is the first document", doc1.GetFileNameOnlyWithExtension(), currentEdoc.GetFileNameOnlyWithExtension());

				userControl.StorageDocsGrid.ListManager.Position = 1;

				var previewedDocument = documentPreview.Document;

				AssertEquals("The previewed document is the current document", doc2.GetFileNameOnlyWithExtension(), previewedDocument.GetFileNameOnlyWithExtension());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentPreviewEdoc_CurrentEdocChanged_PreviewDocumentsCheckBoxWasNotChecked()
		{
			var testDocBytes1 = DocumentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF");
			var testDocBytes2 = DocumentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\colored.tif");
			using (var form = new ZFormForPlugInTest(Org))
			{
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var documentPreview = userControl.DocumentPreview;

				var storageMain = (StorageMain)plugIn.BusinessEntity;
				var doc1 = storageMain.AddFileOrDocument(testDocBytes1, new AddFileOrDocumentDto { FileName = "Sample.PDF", DocumentType = "PDF" });
				var doc2 = storageMain.AddFileOrDocument(testDocBytes2, new AddFileOrDocumentDto { FileName = "colored.tif" });

				form.Show();
				AssertNull("The previewed document is null because PreviewDocumentsCheckBox was not checked", documentPreview.Document);

				userControl.StorageDocsGrid.ListManager.Position = 1;
				AssertNull("The previewed document is null because PreviewDocumentsCheckBox was not checked", documentPreview.Document);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentPreview_EdocsDeletedClosesPreview()
		{
			var testDocBytes = DocumentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF");
			using (var form = new ZFormForPlugInTest(Org))
			{
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var documentPreview = userControl.DocumentPreview;

				var storageMain = (StorageMain)plugIn.BusinessEntity;
				storageMain.AddFileOrDocument(testDocBytes, new AddFileOrDocumentDto { FileName = "Sample.PDF", DocumentType = "PDF" });

				form.Show();
				userControl.PreviewDocumentsCheckBox.Checked = true;

				((UnitTestUserNotification)Globals.Message).AddAnswer(DialogResult.Yes);

				AssertNotNull("The graphical display is active", documentPreview.PageSelector);

				userControl.StorageDocsGrid.Select(0);
				userControl.StorageDocsGrid.ContextMenu.MenuItems.Cast<MenuItem>().First(x => x.Text == "De&lete").PerformClick();

				AssertNull("The graphical display is cleared", documentPreview.PageSelector);
				AssertEquals("The graphical diplay is readonly", true, documentPreview.ReadOnly);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentPreview_UnsupportedDocumentFormat()
		{
			var testDocBytes = DocumentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\UnsupportedFormat.docx");
			using (var form = new ZFormForPlugInTest(Org))
			{
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var documentPreview = userControl.DocumentPreview;

				var storageMain = (StorageMain)plugIn.BusinessEntity;
				storageMain.AddFileOrDocument(testDocBytes, new AddFileOrDocumentDto { FileName = "UnsupportedFormat.docx" });

				form.Show();
				userControl.PreviewDocumentsCheckBox.Checked = true;

				var image = documentPreview.imageProvider.Document;

				AssertType("The previewed document is the dummy document", typeof(DummyPreviewable), image);
				AssertEquals("The error message is correct", "This document format is not supported for previewing.", ((DummyPreviewable)image).ErrorMessage);
				AssertEquals("The graphical diplay is readonly", true, documentPreview.ReadOnly);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentPreview_DocumentViewDisabled()
		{
			var testDocBytes = DocumentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.pdf");
			using (var form = new ZFormForPlugInTest(Org))
			{
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var documentPreview = userControl.DocumentPreview;

				var storageMain = (StorageMain)plugIn.BusinessEntity;
				storageMain.AddFileOrDocument(testDocBytes, new AddFileOrDocumentDto { FileName = "Sample.pdf", DocumentType = Core.Constants.RefDocTypes.QuarantineRemotePrint });

				form.Show();

				AssemblyDataLookup.AddAssemblyDataForTesting(Testing.DeliveryAuthorizingDocumentOwner.DocManagerCode, new DeliveryAuthorizingDocumentOwnerAssemblyData());
				var owner = storageMain.Factory.New<DeliveryAuthorizingDocumentOwner>();
				owner.DocumentViewEnabled = false;
				storageMain.SM_ParentFK = owner.PK;
				storageMain.SM_Type = Testing.DeliveryAuthorizingDocumentOwner.DocManagerCode;

				userControl.PreviewDocumentsCheckBox.Checked = true;
				var image = documentPreview.imageProvider.Document;

				AssertType("The previewed document is the dummy document", typeof(DummyPreviewable), image);
				AssertEquals("The error message is correct", "This document format is not supported for previewing.", ((DummyPreviewable)image).ErrorMessage);
				AssertEquals("The graphical diplay is readonly", true, documentPreview.ReadOnly);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentPreview_DisplayDocumentException()
		{
			var testDocBytes = DocumentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test Corrupted.pdf");
			using (var form = new ZFormForPlugInTest(Org))
			{
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var documentPreview = userControl.DocumentPreview;

				var storageMain = (StorageMain)plugIn.BusinessEntity;
				storageMain.AddFileOrDocument(testDocBytes, new AddFileOrDocumentDto { FileName = "Test Corrupted.pdf", DocumentType = "PDF" });

				form.Show();
				userControl.PreviewDocumentsCheckBox.Checked = true;

				var image = documentPreview.imageProvider.Document;

				AssertType("The previewed document is the dummy document", typeof(DummyPreviewable), image);
				AssertEquals("The error message is correct", "The preview cannot be displayed.", ((DummyPreviewable)image).ErrorMessage);
				AssertEquals("The graphical diplay is readonly", true, documentPreview.ReadOnly);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStorageDocsGridAndRelatedParentsGridBindingContextRefreshed()
		{
			var testDocBytes = DocumentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF");
			using (var form = new ZFormForPlugInTest(Org))
			{
				var plugIn = (eDocsPlugIn)form.PlugIns.Instances[0];
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var bindingContext1 = new BindingContext();
				var bindingContext2 = new BindingContext();

				userControl.BindingContext = bindingContext1;

				AssertEquals("StorageDocsGrid.BindingContext should be bindingContext1", bindingContext1, userControl.StorageDocsGrid.BindingContext);
				AssertEquals("RelatedParentsGrid.BindingContext should be bindingContext1", bindingContext1, userControl.RelatedParentsGrid.BindingContext);

				userControl.BindingContext = bindingContext2;

				AssertEquals("StorageDocsGrid.BindingContextInitialized should be false", true, userControl.StorageDocsGrid.BindingContextInitialized);
				AssertEquals("RelatedParentsGrid.BindingContextInitialized should be false", true, userControl.RelatedParentsGrid.BindingContextInitialized);
				AssertEquals("StorageDocsGrid.BindingContext should be bindingContext2", bindingContext2, userControl.StorageDocsGrid.BindingContext);
				AssertEquals("RelatedParentsGrid.BindingContext should be bindingContext2", bindingContext2, userControl.RelatedParentsGrid.BindingContext);
			}
		}

		#region Implementation

		OrgHeader Org;

		OrgHeader NewOrgForTest()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TESTORG";
			org.OH_FullName = "Test Organization";

			return org;
		}

		Color GetGridRowColour(ZGrid grid, object objectAtRow)
		{
			EventHandler<ColourDecidingEventArgs> handler = (EventHandler<ColourDecidingEventArgs>)typeof(ZGrid).GetField("ColourDeciding", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(grid);
			ColourDecidingEventArgs e = new ColourDecidingEventArgs(objectAtRow);
			handler(null, e);
			return e.Colour;
		}

		RefDocType DocTypeWithForceUserToRead
		{
			get
			{
				if (fDocTypeWithForceUserToRead == null)
				{
					fDocTypeWithForceUserToRead = MasterFactory.New<RefDocType>();
					fDocTypeWithForceUserToRead.RT_ForceUserToRead = true;
					fDocTypeWithForceUserToRead.RT_ReferenceType = Core.Constants.ReferenceTypes.All;
					fDocTypeWithForceUserToRead.RT_DocType = "FUR";
				}
				return fDocTypeWithForceUserToRead;
			}
		}
		RefDocType fDocTypeWithForceUserToRead;

		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.LoadTop1<OrgHeader>(new ZQuery());
		}

		#endregion
	}
}
