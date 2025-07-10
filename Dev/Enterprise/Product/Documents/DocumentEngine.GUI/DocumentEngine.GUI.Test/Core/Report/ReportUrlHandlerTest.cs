using System;
using System.Collections.Generic;
using System.IO;
using AppDomainWrappers.Net;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	class ReportUrlHandlerTest : TestCaseWithDummy
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateReportUrl()
		{
			using var resetRegistry = RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ReportCommand.Factory.Save();
			var serverNameAndDatabaseName = $"&ServerName={InstanceDetails.Current.ServerName}&DatabaseName={InstanceDetails.Current.DatabaseName}&";

			TemplateName = "ThreeFilters.xls";
			string url = RemoveSecurityHashFromUrl(UrlHandler.Create(ReportCommand));
			AssertEquals("edient:Command=" + UrlHandler.GetExpectedCommandText() + "&LicenceCode=" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "&ReportPK=" + ReportCommand.PK + "&Date+Filter=&Date+Range+Filter+From=&Date+Range+Filter+To=&Text+Filter=" + serverNameAndDatabaseName, url);

			TemplateName = "ThreeFilters.xls";
			url = RemoveSecurityHashFromUrl(UrlHandler.Create(Report));
			AssertEquals("edient:Command=" + UrlHandler.GetExpectedCommandText() + "&LicenceCode=" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "&ReportPK=" + ReportCommand.PK + "&Date+Filter=&Date+Range+Filter+From=&Date+Range+Filter+To=&Text+Filter=" + serverNameAndDatabaseName, url);

			TemplateName = "DateRangeFilter.xls";
			url = RemoveSecurityHashFromUrl(UrlHandler.Create(Report));
			AssertEquals("edient:Command=" + UrlHandler.GetExpectedCommandText() + "&LicenceCode=" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "&ReportPK=" + ReportCommand.PK + "&Some+date+From=&Some+date+To=" + serverNameAndDatabaseName, url);

			TemplateName = "ThreeSortOrders.xls";
			url = RemoveSecurityHashFromUrl(UrlHandler.Create(Report));
			AssertEquals("edient:Command=" + UrlHandler.GetExpectedCommandText() + "&LicenceCode=" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "&ReportPK=" + ReportCommand.PK + "&Sort=Alpha" + serverNameAndDatabaseName, url);

			TemplateName = "ThreeGroupBys.xls";
			url = RemoveSecurityHashFromUrl(UrlHandler.Create(Report));
			AssertEquals("edient:Command=" + UrlHandler.GetExpectedCommandText() + "&LicenceCode=" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "&ReportPK=" + ReportCommand.PK + "&GroupBy=Alpha" + serverNameAndDatabaseName, url);

			TemplateName = "ThreeOptionalTemplates.xls";
			Report.PrepareForRender();
			Report.OptionalTemplateSheetCollection["Beta"].Selected = true;
			Report.OptionalTemplateSheetCollection["Gamma"].Selected = true;
			url = RemoveSecurityHashFromUrl(UrlHandler.Create(Report));
			if (Report.OptionalTemplateSheetCollection.IndexOf(i => (OptionalTemplateSheet)i == Report.OptionalTemplateSheetCollection["Beta"]) < Report.OptionalTemplateSheetCollection.IndexOf(i => (OptionalTemplateSheet)i == Report.OptionalTemplateSheetCollection["Gamma"]))
			{
				AssertEquals("edient:Command=" + UrlHandler.GetExpectedCommandText() + "&LicenceCode=" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "&ReportPK=" + ReportCommand.PK + "&OptionalTemplate1=Beta&OptionalTemplate2=Gamma" + serverNameAndDatabaseName, url);
			}
			else
			{
				AssertEquals("edient:Command=" + UrlHandler.GetExpectedCommandText() + "&LicenceCode=" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "&ReportPK=" + ReportCommand.PK + "&OptionalTemplate1=Gamma&OptionalTemplate2=Beta" + serverNameAndDatabaseName, url);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateReportUrl_Default()
		{
			using var resetRegistry = RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.DefaultValue);
			ReportCommand.Factory.Save();

			TemplateName = "ThreeFilters.xls";
			string url = RemoveSecurityHashFromUrl(UrlHandler.Create(ReportCommand));
			AssertEquals("edient:Command=" + UrlHandler.GetExpectedCommandText() + "&LicenceCode=" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "&ReportPK=" + ReportCommand.PK + "&Date+Filter=&Date+Range+Filter+From=&Date+Range+Filter+To=&Text+Filter=&", url);

			TemplateName = "ThreeFilters.xls";
			url = RemoveSecurityHashFromUrl(UrlHandler.Create(Report));
			AssertEquals("edient:Command=" + UrlHandler.GetExpectedCommandText() + "&LicenceCode=" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "&ReportPK=" + ReportCommand.PK + "&Date+Filter=&Date+Range+Filter+From=&Date+Range+Filter+To=&Text+Filter=&", url);

			TemplateName = "DateRangeFilter.xls";
			url = RemoveSecurityHashFromUrl(UrlHandler.Create(Report));
			AssertEquals("edient:Command=" + UrlHandler.GetExpectedCommandText() + "&LicenceCode=" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "&ReportPK=" + ReportCommand.PK + "&Some+date+From=&Some+date+To=&", url);

			TemplateName = "ThreeSortOrders.xls";
			url = RemoveSecurityHashFromUrl(UrlHandler.Create(Report));
			AssertEquals("edient:Command=" + UrlHandler.GetExpectedCommandText() + "&LicenceCode=" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "&ReportPK=" + ReportCommand.PK + "&Sort=Alpha&", url);

			TemplateName = "ThreeGroupBys.xls";
			url = RemoveSecurityHashFromUrl(UrlHandler.Create(Report));
			AssertEquals("edient:Command=" + UrlHandler.GetExpectedCommandText() + "&LicenceCode=" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "&ReportPK=" + ReportCommand.PK + "&GroupBy=Alpha&", url);

			TemplateName = "ThreeOptionalTemplates.xls";
			Report.PrepareForRender();
			Report.OptionalTemplateSheetCollection["Beta"].Selected = true;
			Report.OptionalTemplateSheetCollection["Gamma"].Selected = true;
			url = RemoveSecurityHashFromUrl(UrlHandler.Create(Report));
			if (Report.OptionalTemplateSheetCollection.IndexOf(i => (OptionalTemplateSheet)i == Report.OptionalTemplateSheetCollection["Beta"]) < Report.OptionalTemplateSheetCollection.IndexOf(i => (OptionalTemplateSheet)i == Report.OptionalTemplateSheetCollection["Gamma"]))
			{
				AssertEquals("edient:Command=" + UrlHandler.GetExpectedCommandText() + "&LicenceCode=" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "&ReportPK=" + ReportCommand.PK + "&OptionalTemplate1=Beta&OptionalTemplate2=Gamma&", url);
			}
			else
			{
				AssertEquals("edient:Command=" + UrlHandler.GetExpectedCommandText() + "&LicenceCode=" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "&ReportPK=" + ReportCommand.PK + "&OptionalTemplate1=Gamma&OptionalTemplate2=Beta&", url);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateReportUrl_ListPopulatedFiltersFirst()
		{
			using var resetRegistry = RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			this.TemplateName = "ThreeFilters.xls";
			ReportCommand.Factory.Save();
			var serverNameAndDatabaseName = $"&ServerName={InstanceDetails.Current.ServerName}&DatabaseName={InstanceDetails.Current.DatabaseName}&";

			Report.PrepareForRender();
			TextField field = (TextField)Report.FilterCollection["Text Filter"];
			field.Value = "Filter Value";

			string url = RemoveSecurityHashFromUrl(UrlHandler.Create(Report));
			AssertEquals("edient:Command=" + UrlHandler.GetExpectedCommandText() + "&LicenceCode=" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "&ReportPK=" + ReportCommand.PK + "&Text+Filter=Filter+Value&Date+Filter=&Date+Range+Filter+From=&Date+Range+Filter+To=" + serverNameAndDatabaseName, url);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateReportUrl_ListPopulatedFiltersFirst_Default()
		{
			using var resetRegistry = RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.DefaultValue);
			this.TemplateName = "ThreeFilters.xls";
			ReportCommand.Factory.Save();

			Report.PrepareForRender();
			TextField field = (TextField)Report.FilterCollection["Text Filter"];
			field.Value = "Filter Value";

			string url = RemoveSecurityHashFromUrl(UrlHandler.Create(Report));
			AssertEquals("edient:Command=" + UrlHandler.GetExpectedCommandText() + "&LicenceCode=" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "&ReportPK=" + ReportCommand.PK + "&Text+Filter=Filter+Value&Date+Filter=&Date+Range+Filter+From=&Date+Range+Filter+To=&", url);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvalidUrlSecurityHash()
		{
			string url = ReportUrlHandler.Instance.Create(Report);
			url = url.Replace("ReportPK=", "ReportPK=Invalid");

			EnterpriseUrlHandlerService.UnregisterUrlHandler(ReportUrlHandler.Instance);
			EnterpriseUrlHandlerService.RegisterUrlHandler(ReportUrlHandler.Instance);
			try
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				Fail("Expected an exception");
			}
			catch (EnterpriseUrlHandlerException ex)
			{
				AssertEquals("Report should not be run due to an invalid url", null, UrlHandler.GetHelper().LastReportPrintSet);
				AssertEquals($"This is not a valid {BrandingFactory.Instance.ProductName} shortcut or hyperlink.", ex.Message);
			}
			finally
			{
				EnterpriseUrlHandlerService.UnregisterUrlHandler(ReportUrlHandler.Instance);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCanHandle()
		{
			QueryString queryString = BuildQueryString("ThreeFilters.xls");
			AssertEquals(true, UrlHandler.CanHandle(queryString));

			queryString["Command"] = "SomeOtherCommand";
			AssertEquals(false, UrlHandler.CanHandle(queryString));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestHandle_WhenLoggedWithDifferentEnterpriseOrServerCode()
		{
			Dummy.Factory.Save();
			string url = UrlHandler.Create(Report);

			UrlHandler.SetCurrentCompany(Factory.NewWithValidTestData<GlbCompany>());
			AssertExceptionThrown<EnterpriseUrlHandlerException>($"{BrandingFactory.Instance.ProductName} is not running in the with correct product key or from the correct licensed server installation directory.",
				() => EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectExceptionMessage(typeof(EnterpriseUrlHandlerException), "The report this link was created from isn't available for this version of CargoWise")]
		public void TestHandle_WhenReportCommandDoesntExist()
		{
			Dummy.Factory.Save();
			string url = UrlHandler.Create(Report);

			ReportCommand.Delete();
			Factory.Save();
			EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectExceptionMessage(typeof(EnterpriseUrlHandlerException), "The template of this report has been removed, check it first.")]
		public void TestHandleWhenPivotIsNull()
		{
			var queryString = BuildQueryString("ThreeFilters.xls");
			Pivot.Delete();
			Factory.Save();
			UrlHandler.Handle(queryString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestHandle_FiltersParsed()
		{
			QueryString queryString = BuildQueryString("ThreeFilters.xls");
			queryString["Date Filter"] = new ZDateTime(2005, 1, 1).ToISO8601String();
			queryString["Date Range Filter From"] = new ZDateTime(2005, 2, 2).ToISO8601String();
			queryString["Date Range Filter To"] = new ZDateTime(2005, 3, 3).ToISO8601String();
			queryString["Text Filter"] = "Text Filter Value";
			AssertEquals("Report options form should be shown", true, UrlHandler.Handle(queryString));
			AssertEquals("Report options form should be shown", true, UrlHandler.GetHelper().LastReportPrintSet.IsRun);
			AssertEquals("Handle does not dispose print set.", 1, UrlHandler.GetHelper().LastReportPrintSet[0].Count);

			Report report = UrlHandler.GetHelper().LastReportPrintSet[0].GetFirstReport();
			AssertEquals("Date Filter", new ZDateTime(2005, 1, 1), report.FilterCollection["Date Filter"].ValueAsObject);
			AssertEquals("Date Range Filter (from)", new ZDateTime(2005, 2, 2), ((DateRangeField)report.FilterCollection["Date Range Filter"]).ValueLow);
			AssertEquals("Date Range Filter (to)", new ZDateTime(2005, 3, 3), ((DateRangeField)report.FilterCollection["Date Range Filter"]).ValueHigh);
			AssertEquals("Text Filter", "Text Filter Value", report.FilterCollection["Text Filter"].ValueAsObject);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestHandle_SortOrderParsed()
		{
			QueryString queryString = BuildQueryString("ThreeSortOrders.xls");
			queryString["Sort"] = "Gamma";
			AssertEquals("Report options form should be shown", true, UrlHandler.Handle(queryString));
			AssertEquals("Report options form should be shown", true, UrlHandler.GetHelper().LastReportPrintSet.IsRun);
			AssertEquals("Handle does not dispose print set.", 1, UrlHandler.GetHelper().LastReportPrintSet[0].Count);

			Report report = UrlHandler.GetHelper().LastReportPrintSet[0].GetFirstReport();
			AssertEquals("Gamma should be selected", "Gamma", report.SortOrderCollection[2].DisplayName);
			AssertEquals("Gamma should be selected", true, report.SortOrderCollection[2].Selected);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestHandle_GroupByParsed()
		{
			QueryString queryString = BuildQueryString("ThreeGroupBys.xls");
			queryString["GroupBy"] = "Gamma";
			AssertEquals("Report options form should be shown", true, UrlHandler.Handle(queryString));
			AssertEquals("Report options form should be shown", true, UrlHandler.GetHelper().LastReportPrintSet.IsRun);
			AssertEquals("Handle does not dispose print set.", 1, UrlHandler.GetHelper().LastReportPrintSet[0].Count);

			Report report = UrlHandler.GetHelper().LastReportPrintSet[0].GetFirstReport();
			AssertEquals("Gamma should be selected", "Gamma", report.GroupByCollection[2].DisplayName);
			AssertEquals("Gamma should be selected", true, report.GroupByCollection[2].Selected);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestHandle_GroupByParsed_WhenGroupByInvalid()
		{
			QueryString queryString = BuildQueryString("ThreeGroupBys.xls");
			queryString["GroupBy"] = "DoesntExist";
			AssertEquals("Report options form should be shown", true, UrlHandler.Handle(queryString));
			AssertEquals("Report options form should be shown", true, UrlHandler.GetHelper().LastReportPrintSet.IsRun);
			AssertEquals("Handle does not dispose print set.", 1, UrlHandler.GetHelper().LastReportPrintSet[0].Count);

			Report report = UrlHandler.GetHelper().LastReportPrintSet[0].GetFirstReport();
			AssertEquals("Default should be selected", true, report.GroupByCollection[0].Selected);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestHandle_ConfigurationParsed()
		{
			QueryString queryString = BuildQueryString("ThreeGroupBys.xls");
			queryString["Configuration"] = "Abcd";
			AssertEquals("Report options form should be shown", true, UrlHandler.Handle(queryString));
			AssertEquals("Report options form should be shown", true, UrlHandler.GetHelper().LastReportPrintSet.IsRun);
			AssertEquals("Handle does not dispose print set.", 1, UrlHandler.GetHelper().LastReportPrintSet[0].Count);

			Report report = UrlHandler.GetHelper().LastReportPrintSet[0].GetFirstReport();
			AssertEquals("Configuration selected", "Abcd", report.ColumnHeadingManager.CurrentColumnConfigurationManager.Description);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestHandle_OptionalTemplatesParsed()
		{
			QueryString queryString = BuildQueryString("ThreeOptionalTemplates.xls");
			queryString["OptionalTemplate1"] = "Decoy";
			queryString["OptionalTemplate2"] = "Gamma";
			queryString["OptionalTemplate3"] = "Beta";
			AssertEquals("Report options form should be shown", true, UrlHandler.Handle(queryString));
			AssertEquals("Report options form should be shown", true, UrlHandler.GetHelper().LastReportPrintSet.IsRun);
			AssertEquals("Handle does not dispose print set.", 1, UrlHandler.GetHelper().LastReportPrintSet[0].Count);

			Report report = UrlHandler.GetHelper().LastReportPrintSet[0].GetFirstReport();
			AssertEquals("Alpha not selected", false, report.OptionalTemplateSheetCollection["Alpha"].Selected);
			AssertEquals("Beta selected", true, report.OptionalTemplateSheetCollection["Beta"].Selected);
			AssertEquals("Gamma selected", true, report.OptionalTemplateSheetCollection["Gamma"].Selected);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestHandle_InsufficientUserRights()
		{
			QueryString queryString = BuildQueryString("ThreeGroupBys.xls");

			SecurityCheckpoint parent = Env.Security.OrderReports;
			var checkpoint = EnvProxy.Instance.Security.FindOrCreateReportCheckpoint(ReportCommand.PK.ToGuid(), ReportCommand.SU_MenuNameMultilingual, ModuleIDs.OrdersReport, parent);
			checkpoint.IsAllowed = false;

			UrlHandler.Handle(queryString);
			AssertContains("Should be insufficient security rights", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestHandle_InsufficientUserRights_2()
		{
			QueryString queryString = BuildQueryString("ThreeGroupBys.xls");

			SecurityCheckpoint parent = Env.Security.OrderReports;
			var checkpoint = EnvProxy.Instance.Security.FindOrCreateReportRunCheckpoint(ModuleIDs.OrdersReport, parent);

			var testGroup = Factory.New<GlbGroup>();
			testGroup.GG_Code = "BAM";
			testGroup.GG_IsActive = true;

			var testStaff = Factory.NewWithValidTestData<GlbStaff>();
			testStaff.GS_LoginName = "Bob.test2";

			var testLink = Factory.New<GlbGroupLink>();
			testLink.GK_GS = testStaff.PK;
			testLink.GK_GG = testGroup.PK;

			var reportSecurityItemRR = Factory.New<GlbSecurity>();
			reportSecurityItemRR.GU_GG = testGroup.PK;
			reportSecurityItemRR.GU_SecurityRight = EnvProxy.Instance.Security.FindOrCreateReportRunCheckpoint(ModuleIDs.OrdersReport, parent).Code;
			reportSecurityItemRR.GU_SecurityItemIsAllowed = false;
			Factory.Save();

			using (Env.SetTemporaryUserContext("Bob.test2", Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				UrlHandler.Handle(queryString);
				AssertContains("Should be insufficient security rights", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestHandle_ForClientModule()
		{
			ReportCommand.SU_BusinessContext = "RepClientModuleID";
			Factory.Save();

			var moduleId = new ClientModuleIdentifier(TestClientModuleId.ClientModuleID, "Reports", (NoResString)"Test Client Reports");
			var info = new ModuleInfo(moduleId, "Enterprise.DocumentEngine.Module.Test", "Enterprise.DocumentEngine.Module.Testing.ZReportModuleTest+DummyClientReportModule");
			var clientModuleInfo = new NewClientModuleInfo("CategoryName1", "SectionName1", info);
			var clientModuleInfos = new[] { clientModuleInfo };
			TestClientHook.Instance.NewClientModulesForTest = clientModuleInfos;

			using (ClientHookLoader.Instance.OverrideClientHookForTest(TestClientHook.Instance))
			{
				string url = UrlHandler.Create(Report);
				QueryString queryString = new QueryString(url.Replace(ReportUrlHandlerForTest.EdiUrlPrefix, ""));

				UrlHandler.Handle(queryString);

				AssertEquals("Should be handled", "None", UnitTestUserNotification.Instance.LastMessage.ToString().Trim());
			}
		}

		[ExpectExceptionMessage(typeof(EnterpriseUrlHandlerException), "The report NotAModule could not be found in client modules")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestHandle_ForBusinessContextNotInClientModule()
		{
			ReportCommand.SU_BusinessContext = "RepNotAModule";
			Factory.Save();

			var moduleId = new ClientModuleIdentifier(TestClientModuleId.ClientModuleID, "Reports", (NoResString)"Test Client Reports");
			var info = new ModuleInfo(moduleId, "Enterprise.DocumentEngine.Module", "Enterprise.DocumentEngine.Module.Testing.ZReportModuleTest+DummyClientReportModule");
			var clientModuleInfo = new NewClientModuleInfo("CategoryName1", "SectionName1", info);
			var clientModuleInfos = new[] { clientModuleInfo };
			TestClientHook.Instance.NewClientModulesForTest = clientModuleInfos;

			using (ClientHookLoader.Instance.OverrideClientHookForTest(TestClientHook.Instance))
			{
				string url = UrlHandler.Create(Report);
				QueryString queryString = new QueryString(url.Replace(ReportUrlHandlerForTest.EdiUrlPrefix, ""));

				UrlHandler.Handle(queryString);
			}
		}

		#region Test Classes

		internal class ReportUrlHandlerHelper : IDisposable
		{
			public TestReportPrintSet LastReportPrintSet
			{
				get
				{
					TestReportPrintSet result = null;

					if (CreatedTestReportPrintSets.Count > 0)
					{
						result = CreatedTestReportPrintSets[CreatedTestReportPrintSets.Count - 1];
					}

					return result;
				}
			}

			internal readonly List<TestReportPrintSet> CreatedTestReportPrintSets = new List<TestReportPrintSet>();

			#region IDisposable Members

			public void Dispose()
			{
				foreach (TestReportPrintSet reportPrintSet in CreatedTestReportPrintSets)
				{
					reportPrintSet.Dispose();
				}

				CreatedTestReportPrintSets.Clear();
			}

			#endregion
		}

		internal interface IReportUrlHandlerForTest : IReportUrlHandler
		{
			void SetCurrentCompany(IGlbCompany company);
			ReportUrlHandlerHelper GetHelper();
			string GetExpectedCommandText();
		}

		class ReportUrlHandlerForTest : ReportUrlHandler, IReportUrlHandlerForTest
		{
			protected override ReportPrintSet NewReportPrintSetToRun(ReportCommand reportCommand)
			{
				var result = new TestReportPrintSet(reportCommand);
				GetHelper().CreatedTestReportPrintSets.Add(result);
				return result;
			}

			protected override IGlbCompany CurrentCompany
			{
				get { return currentCompany ?? base.CurrentCompany; }
			}
			IGlbCompany currentCompany;

			public void SetCurrentCompany(IGlbCompany company)
			{
				currentCompany = company;
			}

			public string GetExpectedCommandText()
			{
				return ExpectedCommandText;
			}

			public ReportUrlHandlerHelper GetHelper()
			{
				return helper ?? (helper = new ReportUrlHandlerHelper());
			}
			ReportUrlHandlerHelper helper;
		}

		internal class TestReportPrintSet : ReportPrintSet
		{
			internal TestReportPrintSet(ReportCommand reportCommand)
				: base(reportCommand)
			{
			}

			internal bool IsRun;

			public override DeliveryInstructionDestination RunWithPartialInstructions(AllowedDeliveryOptions deliveryOptions, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
			{
				this.IsRun = true;
				return DeliveryInstructionDestination.TakenFromContact;
			}
		}

		#endregion

		#region Implementation

		protected QueryString BuildQueryString(string templateName)
		{
			this.TemplateName = templateName;

			var newConfigName = new NewConfiguration(Report.ColumnHeadingManager);
			newConfigName.NewName = "Abcd";
			CombinedConfigurationManager manager = newConfigName.NewManager;
			manager.Save(Report.FilterCollection, Report.GroupByCollection.SelectedGroupBy.DisplayName, Report.SortOrderCollection.SelectedOrder.DisplayName, Report.Orientation, Report.Language);

			ReportCommand.Factory.Save();

			string url = UrlHandler.Create(Report);
			QueryString queryString = new QueryString(url.Replace(ReportUrlHandlerForTest.EdiUrlPrefix, ""));
			return queryString;
		}

		bool wasReportUrlHandlerRegistered;

		string TemplateName
		{
			get { return templateName; }
			set
			{
				if (report != null)
				{
					report.Dispose();
				}
				report = null;
				fExcelTemplate = null;
				reportCommand = null;
				documentPack = null;

				templateName = value;
			}
		}
		string templateName = "ThreeFilters.xls";

		Report Report
		{
			get
			{
				if (report == null)
				{
					report = new Report(DocumentPack, ExcelTemplate, System.Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest);
				}
				return report;
			}
		}
		Report report;

		ExcelTemplateForUnitTesting ExcelTemplate
		{
			get { return fExcelTemplate ?? (fExcelTemplate = new ExcelTemplateForUnitTesting(TemplateName, TestFilesSubFolder.ReportTestFiles)); }
		}
		ExcelTemplateForUnitTesting fExcelTemplate;

		DocumentPack DocumentPack
		{
			get
			{
				if (documentPack == null)
				{
					documentPack = new DocumentPack(ReportCommand);
				}
				return documentPack;
			}
		}
		DocumentPack documentPack;

		StmTemplate Template
		{
			get
			{
				if (template == null)
				{
					template = Factory.New<StmTemplate>();
					template.SO_Template = ExcelTemplate.GetAsByteArray();
					template.SO_Name = TemplateName;
				}
				return template;
			}
		}
		StmTemplate template;

		ReportCommand ReportCommand
		{
			get
			{
				if (reportCommand == null)
				{
					reportCommand = Factory.New<ReportCommand>();
					reportCommand.SU_MenuName = TemplateName;
					reportCommand.SU_BusinessContext = "RepOrdersReport";

					Pivot.SI_SU = reportCommand.PK;
					Pivot.SI_SO = Template.PK;
				}
				return reportCommand;
			}
		}
		ReportCommand reportCommand;

		StmMenuTemplatePivot Pivot => pivot ?? (pivot = Factory.New<StmMenuTemplatePivot>());
		StmMenuTemplatePivot pivot;

		protected IReportUrlHandlerForTest UrlHandler
		{
			get
			{
				if (urlHandler == null)
				{
					urlHandler = GetNewReportUrlHandlerForTest();
				}
				return urlHandler;
			}
		}
		IReportUrlHandlerForTest urlHandler;

		protected virtual IReportUrlHandlerForTest GetNewReportUrlHandlerForTest()
		{
			return new ReportUrlHandlerForTest();
		}

		string RemoveSecurityHashFromUrl(string url)
		{
			int startIndex = url.IndexOf("Hash=");
			int endIndex = url.IndexOf("&", startIndex);
			return url.Substring(0, startIndex) + (endIndex == -1 ? "" : url.Substring(endIndex + 1));
		}

		protected override void SetUp()
		{
			base.SetUp();
			System.Windows.Forms.Application.DoEvents();

			wasReportUrlHandlerRegistered = ((IList<UrlHandler>)EnterpriseUrlHandlerService.UrlHandlers).Contains(ReportUrlHandler.Instance);
			EnterpriseUrlHandlerService.RegisterUrlHandler((UrlHandler)UrlHandler);
			EnterpriseUrlHandlerService.UnregisterUrlHandler(ReportUrlHandler.Instance);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (report != null)
			{
				report.Dispose();
				report = null;
			}
			fExcelTemplate = null;
			reportCommand = null;
			documentPack = null;

			if (urlHandler != null)
			{
				EnterpriseUrlHandlerService.UnregisterUrlHandler((UrlHandler)urlHandler);
				urlHandler.GetHelper().Dispose();
			}

			if (wasReportUrlHandlerRegistered)
			{
				EnterpriseUrlHandlerService.RegisterUrlHandler(ReportUrlHandler.Instance);
			}
		}

		#endregion
	}

#if !WINZOR

	[UseSnapshotProtection]
	sealed class ReportUrlHandlerTestForDomainAndInstance : TestCase
	{
		[ExpectNoExceptions] // Assertions in second appDomain
		public void TestRegisteredDomainInstanceContainsDomainAndInstanceName()
		{
			using var resourceRetriever =
				new EmbeddedResourceRetriever(typeof(DocumentEngine.Testing.PrintTaskTest).Assembly);
			using var domain = new AppDomainWrapper("TestRegisteredDomainInstanceContainsDomainAndInstanceName");
			var factory = new BusinessObjectFactory();
			var reportCommand = factory.New<ReportCommand>();
			reportCommand.SU_MenuName = Guid.NewGuid().ToString();
			reportCommand.SU_IsPublished = false;
			factory.Save();
			var tempFileName = resourceRetriever.SaveResourceToFile(
				"Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.ThreeFilters.xls", "ThreeFilters.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("ThreeFilters.xls", Path.GetFullPath(tempFileName));

			var config = new ProcessConfig()
			{
				NamespacePath = "",
				ClassName = nameof(ReportUrlHandlerTest),
				MethodName = nameof(DoTestRegisteredDomainInstanceContainsDomainAndInstanceName),
				MethodParameters = new string[]
				{
					Db.ServerName,
					Db.DatabaseName,
					typeof(ReportUrlHandler).AssemblyQualifiedName,
					reportCommand.PK.ToGuid().ToString(),
					Convert.ToBase64String(excelTemplate.GetAsByteArray()),
					excelTemplate.TemplateSourceLocation.ToString(),
					excelTemplate.TemplateName.ToString()
				}
			};

			config.AssemblyFile = Path.Combine(config.BinFolder, "Enterprise.DocumentEngineCore.GUI.Test.dll");

#if NET48_OR_GREATER
			domain.RunMethodInProcess48(config);
#elif NET6_0_OR_GREATER
			domain.RunMethodInProcess(config);
#else
#error Unrecognized .NET version
#endif
		}

		static void DoTestRegisteredDomainInstanceContainsDomainAndInstanceName(
			string serverName,
			string databaseName,
			string urlHandlerTypeName,
			string reportCommandPKasString,
			string excelTemplateContentAsString,
			string excelTemplateLocation,
			string excelTemplateName
			)
		{
			var urlHandler = (ReportUrlHandler)Activator.CreateInstance(Type.GetType(urlHandlerTypeName, true), true);

			Db.InitializeDatabaseDetails(serverName, databaseName);

			var reportCommandPK = Guid.Parse(reportCommandPKasString);
			var excelTemplateContent = Convert.FromBase64String(excelTemplateContentAsString);
			var excelTemplate = new ExcelTemplateReadFromByteArray(excelTemplateName, excelTemplateLocation, excelTemplateContent);

			EnterpriseApplicationConfiguration.ConfigureObjectFactory();
			var factory = new BusinessObjectFactory();

			var template = factory.New<StmTemplate>();
			template.SO_Template = excelTemplateContent;
			var pivot = factory.New<StmMenuTemplatePivot>();
			var reportCommand = factory.Load<ReportCommand>(reportCommandPK);
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			var documentPack = new DocumentPack(reportCommand);
			var report = new Report(documentPack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest);

			string name;
			string url;
			using (new WindowsIdentityImpersonator(TestConstants.ADTestAdminAccount.NameWithDomain, TestConstants.ADTestAdminAccount.Password, () => { }))
			{
				var cargoWiseOneInstanceClass = new CargoWiseOneInstanceClass();
				name = Guid.NewGuid().ToString();
				var ou = (DirectoryEntryWrapper)new DirectorySearcherWrapper(TestConstants.ADTestAdminAccount.NameWithDomain, TestConstants.ADTestAdminAccount.Password, TestConstants.Domain).FindOrganisationalUnit(TestConstants.TestApplicationCreationOU);
				var instance = cargoWiseOneInstanceClass.AddNewInstance(name, Db.ServerName, Db.DatabaseName, ou.DirectoryEntry);
				try
				{
					url = urlHandler.Create(report);
				}
				finally
				{
					instance.Delete();
				}
			}

			AssertContains("Domain=sand.wtg.zone", url);
			AssertContains("Instance=" + name, url);
		}
	}

#endif
}
