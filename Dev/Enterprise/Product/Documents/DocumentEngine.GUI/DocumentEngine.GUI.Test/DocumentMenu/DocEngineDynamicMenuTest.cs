using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	public abstract class DocEngineDynamicMenuTest<W, T, U> : TestCaseWithFactory
			where W : DocEngineDynamicMenuProvider<T, U>, new()
			where T : IDynamicMenu, IDisposable
			where U : class, IZDocumentMenuItem, IDisposable, new()
	{
		[GuiTest]
		public void TestMenuGetsUDFFieldsWhereAvailable()
		{
			var udfFieldName = "UDField1";
			var udfFieldValue = "Got ONE!!!";

			var templateContents = @"
{A}-[#Config]
{A}-[Name=TestStuff]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#SectionBody]
{B}-[Real Field - Z0_VarCharMax] {C}-[<Z0_VarCharMax>]
{B}-[UDF Field - {0}] {C}-[<{0}>]
{B}-[End of Test]
{A}-[#EndOfReport]
".Replace("{0}", udfFieldName).Trim();

			var parentBO = Factory.New<DummyBODocSupportable>();
			var stmMenuItem = Factory.New<DocumentCommand>();
			stmMenuItem.SU_MenuName = "TestMenuGetsUDFFieldsWhereAvailable";
			stmMenuItem.SU_BusinessContext = parentBO.DocumentSupporter.BusinessContext.ToString();

			var stmTemplate = Factory.New<StmTemplate>();
			var stmMenuTemplateLink = Factory.New<StmMenuTemplatePivot>();
			stmMenuTemplateLink.SI_SU = stmMenuItem.PK;
			stmMenuTemplateLink.SI_SO = stmTemplate.PK;

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);
				DocumentEngineTestHelper.GenerateExcelWorkSheetFromString(excelInterface.WorkSheets[0], templateContents);
				using (var stream = new MemoryStream())
				{
					excelInterface.SaveToStream(stream);
					stmTemplate.SO_Template = stream.CopyToByteArray();
				}
			}
			stmTemplate.SO_DataContext = ".DummyBODocSupportable";

			using (var form = new ZForm(parentBO))
			{
				form.Show();
				var documentNote = DocumentNote.LoadNote(parentBO);

				var userDefinedField = new TextField(Factory);
				userDefinedField.DisplayName = udfFieldName;
				userDefinedField.FieldName = udfFieldName;
				userDefinedField.Value = udfFieldValue;
				documentNote.UserDefinedFieldList.Add(userDefinedField);
				documentNote.ST_IsCustomDescription = false;

				Application.DoEvents();

				Factory.Save();
				documentNote.Factory.Save();

				using (var item = Menu.GetMenuItem(form, parentBO))
				{
					AddEmptyEventArgs(item);

					var menuItem = item.MenuItems[0];
					AssertContains("menuItem.Text", stmMenuItem.SU_MenuName, GetText(menuItem));
					AssertNotNull("menuItem", menuItem);
					PeformMenuClick(menuItem);

					var documentsMenu = Menu.LastDocumentMenuForTesting;
					var runReports = GetReportInfos(documentsMenu);
					AssertEquals("runReports.Count", 1, runReports.Count);
					var runReport = runReports[0];
					AssertEquals("runReport.UserDefinedFieldValueList.ContainsName(udfFieldName)", true, runReport.UserDefinedFieldValueList.ContainsName(udfFieldName));
					var udfField = runReport.UserDefinedFieldValueList[udfFieldName];
					AssertEquals("udfField.FieldName", udfFieldName, udfField.FieldName);
					AssertEquals("udfField.ValueAsObject", udfFieldValue, udfField.ValueAsObject);
				}
			}
		}

		[GuiTest]
		public void TestGetMenu()
		{
			var saveMessage = "Please save your record before running documents.";
			var organisation = Factory.New<OrgHeader>();
			using (var form = new ZForm(organisation))
			{
				form.Show();
				organisation.OH_FullName = "abc";
				organisation.OH_Code = "abc";

				Application.DoEvents();
				AssertEquals(true, organisation.HasChanges);
				using (var item = Menu.GetMenuItem(form, organisation))
				{
					AssertEquals("no menu items on the menu", 0, item.MenuItems.Count);
					AssertEquals(saveMessage, item.Text);
				}

				Factory.Save();
				using (var item = Menu.GetMenuItem(form, organisation))
				{
					AssertEquals("Documents", item.Text);
					AssertEquals("place holder", 1, item.MenuItems.Count);
					AssertEquals("place holder", "<PlaceHolder>", GetText(item.MenuItems[0]));

					AddEmptyEventArgs(item);
					AssertEquals("menu items are added", true, item.MenuItems.Count > 0);
					AssertNotEquals("content", "<PlaceHolder>", GetText(item.MenuItems[0]));
				}
			}
		}

		[GuiTest]
		public void TestGetMenu_ShowDocumentsInDynamicMenu()
		{
			var bizObj = Factory.New<DummyBusinessObjectWithDocumentSupporter>();
			var documentSupporter = ((DummyDocumentSupporter)bizObj.DocumentSupporter);

			using (var form = new ZForm(bizObj))
			{
				form.Show();
				Application.DoEvents();

				documentSupporter.showDocumentsInDynamicMenu = false;
				Factory.Save();

				using (var item = Menu.GetMenuItem(form, bizObj))
				{
					AssertNull(item);
				}

				documentSupporter.showDocumentsInDynamicMenu = true;
				Factory.Save();

				using (var item = Menu.GetMenuItem(form, bizObj))
				{
					AssertEquals("Documents", item.Text);
					AssertEquals("place holder", 1, item.MenuItems.Count);
					AssertEquals("place holder", "<PlaceHolder>", GetText(item.MenuItems[0]));

					AddEmptyEventArgs(item);
					AssertEquals("menu items are added", true, item.MenuItems.Count > 0);
					AssertNotEquals("content", "<PlaceHolder>", GetText(item.MenuItems[0]));
				}
			}
		}

		#region Implementation

		class DummyBusinessObjectWithDocumentSupporter : DummyEnterpriseBusinessObject, IDocumentSupportable
		{
			public DummyBusinessObjectWithDocumentSupporter(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public DocumentSupporter DocumentSupporter
			{
				get { return documentSupporter ?? (documentSupporter = new DummyDocumentSupporter(this)); }
			}
			DummyDocumentSupporter documentSupporter;
		}

		class DummyDocumentSupporter : DocumentSupporter
		{
			public DummyDocumentSupporter(BusinessObject parentBusinessObject)
				: base(parentBusinessObject)
			{ }

			public override bool ShowDocumentsInDynamicMenu
			{
				get { return showDocumentsInDynamicMenu; }
			}
			public bool showDocumentsInDynamicMenu;

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.Test; }
			}

			public override ZArchitecture.Modules.ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { return null; }
			}

			protected override DocumentEngineCore.DocWrappers.DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, MasterFiles.Integration.IStmMenuItem commandBeingRun)
			{
				return null;
			}

			protected override Core.Constants.DataContext[] GetSupportedDataContexts()
			{
				return new Core.Constants.DataContext[] { Core.Constants.DataContext.UnitTest };
			}
		}

		W Menu
		{
			get { return menu ?? (menu = new W()); }
		}
		W menu;

		protected abstract string GetText(object menuItem);
		protected abstract List<ReportRunInfoForTesting> GetReportInfos(U documentsMenu);
		protected abstract void PeformMenuClick(object menuItem);
		protected abstract void AddEmptyEventArgs(T item);

		#endregion
	}
}
