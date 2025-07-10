using System;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ShowStorageDocUrlHandlerTest : TestCaseWithFactory
	{
		#region Create

		public void TestCreate()
		{
			using var resetRegistry = RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var serverNameAndDatabaseName = $"&ServerName={InstanceDetails.Current.ServerName}&DatabaseName={InstanceDetails.Current.DatabaseName}";
			AssertEquals(
				"If the result of Create changes, the shortcuts for existing clients will break",
				$"edient:Command=ShowStorageDoc&BusinessEntityPK=ac7c630a-de0f-4b50-8a57-afa2aa38f2ce&StorageDocPK=e0ac8f0e-d079-4be7-a49a-994b4b23032d{serverNameAndDatabaseName}&Hash=%2b2MNj7aovWBxJJu2hGePrgnreE%2f26rLD%2b",
				UrlHandler.Create(new ZGuid("ac7c630a-de0f-4b50-8a57-afa2aa38f2ce"), new ZGuid("e0ac8f0e-d079-4be7-a49a-994b4b23032d")));
		}

		public void TestCreate_Default()
		{
			using var resetRegistry = RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.DefaultValue);

			AssertEquals(
				"If the result of Create changes, the shortcuts for existing clients will break",
				$"edient:Command=ShowStorageDoc&BusinessEntityPK=ac7c630a-de0f-4b50-8a57-afa2aa38f2ce&StorageDocPK=e0ac8f0e-d079-4be7-a49a-994b4b23032d&Hash=%2b2MNj7aovWBxJJu2hGePrgnreE%2f26rLD%2b",
				UrlHandler.Create(new ZGuid("ac7c630a-de0f-4b50-8a57-afa2aa38f2ce"), new ZGuid("e0ac8f0e-d079-4be7-a49a-994b4b23032d")));
		}

		#endregion

		#region StorageDoc Tests

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStorageDocIsShown()
		{
			var docPkg = CreateTestStorageDoc(true, true);

			ErrorReporter.Clear();
			ViewStorageDocWithAssertions(docPkg, () =>
			{
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertNotNull(ZApplication.GetOpenForms().OfType<ZForm>().FirstOrDefault(form => form.FormCaption.Contains("eDoc Viewer")));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStorageDocNotProvided()
		{
			var docPkg = CreateTestStorageDoc(true, false);

			ErrorReporter.Clear();
			ViewStorageDocWithAssertions(docPkg, () =>
			{
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertEquals($"This is not a valid {Enterprise.Core.Constants.ProductName} shortcut or hyperlink.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStorageMainNotSaved()
		{
			var docPkg = CreateTestStorageDoc(createStorageMain: true, createStorageDoc: true, save: false);

			ErrorReporter.Clear();
			ViewStorageDocWithAssertions(docPkg, () =>
			{
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertEquals("The requested document could not be found. Please ensure the document has been saved and has not been deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStorageDocNotFound()
		{
			var docPkg = CreateTestStorageDoc(true, false);
			docPkg.StorageDoc = Factory.NewWithValidTestData<DummyBusinessObject>();

			ErrorReporter.Clear();
			ViewStorageDocWithAssertions(docPkg, () =>
			{
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertEquals("The requested document could not be found. Please ensure the document has been saved and has not been deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStorageDocShowErrorWhenFormIsNull()
		{
			var docPkg = CreateTestStorageDoc(true, false);
			docPkg.StorageDoc = Factory.NewWithValidTestData<DummyBusinessObject>();

			ErrorReporter.Clear();
			var url = UrlHandler.Create(docPkg.BusinessObjectPK, docPkg.StorageDocPK);
			using (UrlHandler.SetupStorageDocUrlHandlerForTest())
			{
				try
				{
					EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				}
				catch (EnterpriseUrlHandlerException)
				{
				}

				AssertNull(ErrorReporter.LastExceptionReported);
				AssertEquals($"An active form could not be found to use for displaying the requested document. Please ensure {Enterprise.Core.Constants.ProductName} is open.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoStorageMain()
		{
			var docPkg = CreateTestStorageDoc(false, false);

			ErrorReporter.Clear();
			ViewStorageDocWithAssertions(docPkg, () =>
			{
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertNull(ZApplication.GetOpenForms().OfType<ZForm>().FirstOrDefault(form => form.FormCaption.Contains("eDoc Viewer")));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestModuleAccessDenied()
		{
			var docPkg = CreateTestStorageDoc(true, true);
			EnvProxy.Instance.Security.Organisation.IsAllowed = false;

			try
			{
				ErrorReporter.Clear();
				ViewStorageDocWithAssertions(docPkg, () =>
				{
					AssertNull(ErrorReporter.LastExceptionReported);
					AssertEquals("You do not have sufficient security rights to access this document", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
			finally
			{
				EnvProxy.Instance.Security.Organisation.ClearIsAllowedCache();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentAccessDenied_CanViewAllDocuments()
		{
			var docPkg = CreateTestStorageDoc(true, true);

			var notCurrentCompanyQuery = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, EnvProxy.Instance.CurrentCompany.PK);
			var company = Factory.LoadTop1(ObjectFactory.GetType<IGlbCompany>(), notCurrentCompanyQuery);
			docPkg.StorageDoc[StorageDocsSchema.SC_GC_Company] = company[GlbCompanySchema.PK];
			docPkg.Save();

			ErrorReporter.Clear();
			ViewStorageDocWithAssertions(docPkg, () =>
			{
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertNotNull(ZApplication.GetOpenForms().OfType<ZForm>().FirstOrDefault(form => form.FormCaption.Contains("eDoc Viewer")));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentAccessDenied_CantViewAllDocuments()
		{
			Env.Security.ViewAllBranchSpecificDocuments.IsAllowed = false;
			Env.Security.ViewAllCompanySpecificDocuments.IsAllowed = false;
			Env.Security.ViewAllDepartmentSpecificDocuments.IsAllowed = false;

			var docPkg = CreateTestStorageDoc(true, true);

			var notCurrentCompanyQuery = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, EnvProxy.Instance.CurrentCompany.PK);
			var company = Factory.LoadTop1(ObjectFactory.GetType<IGlbCompany>(), notCurrentCompanyQuery);
			docPkg.StorageDoc[StorageDocsSchema.SC_GC_Company] = company[GlbCompanySchema.PK];
			docPkg.Save();

			ErrorReporter.Clear();
			ViewStorageDocWithAssertions(docPkg, () =>
			{
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertEquals("You do not have sufficient security rights to access this document", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOepnEdocWhenOnlyMainFormIsOpen()
		{
			var docPkg = CreateTestStorageDoc(true, true);
			ErrorReporter.Clear();

			var url = UrlHandler.Create(docPkg.BusinessObjectPK, docPkg.StorageDocPK);
			using (UrlHandler.SetupStorageDocUrlHandlerForTest())
			using (var form = new KForm())
			{
				try
				{
					form.Show();
					EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				}
				catch (EnterpriseUrlHandlerException ex)
				{
					Globals.Message.Show(ex.Message);
				}
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertNotNull(ZApplication.GetOpenForms().OfType<ZForm>().FirstOrDefault(f => f.FormCaption.Contains("eDoc Viewer")));
			}
		}

		#endregion

		#region Implementation

		void ViewStorageDocWithAssertions(TestingStorageDocPackage pkg, Action assertions)
		{
			var url = UrlHandler.Create(pkg.BusinessObjectPK, pkg.StorageDocPK);
			using (UrlHandler.SetupStorageDocUrlHandlerForTest())
			using (var form = new ZForm())
			{
				try
				{
					form.Show();
					EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				}
				catch (EnterpriseUrlHandlerException ex)
				{
					Globals.Message.Show(ex.Message);
				}
				assertions();
			}
		}

		TestingStorageDocPackage CreateTestStorageDoc(bool createStorageMain, bool createStorageDoc, bool save = true)
		{
			if (!createStorageDoc && !createStorageMain)
			{
				return new TestingStorageDocPackage();
			}

			var storageMainPK = ZGuid.Empty;

			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = documentFactoryProvider.GetFactory(Factory);
			var documentFactoryForBO = (BusinessObjectFactory)documentFactory;
			var storageDocsFactory = documentFactory.GetFactory(1);
			var dummyOrgHeader = documentFactoryForBO.NewWithValidTestData(ObjectFactory.GetType<IOrgHeader>());
			dummyOrgHeader[OrgHeaderSchema.OH_Code] = "UnitTest";

			var pkg = new TestingStorageDocPackage()
			{
				BusinessObject = dummyOrgHeader,
				Factory = documentFactoryForBO
			};

			if (createStorageMain)
			{
				var storageMain = (BusinessObject)documentFactoryForBO.New<IStorageMain>();
				storageMain[StorageMainSchema.SM_Type] = Enterprise.Core.Constants.DocManagerCodes.Organisation;
				storageMain[StorageMainSchema.SM_ParentFK.Name] = dummyOrgHeader.PK;
				storageMain[StorageMainSchema.SM_DB.Name] = 1;

				storageMainPK = storageMain.PK;
			}

			if (createStorageDoc)
			{
				var documentDate = new ZDateTime(2005, 10, 11);
				var storageDocs = (BusinessObject)storageDocsFactory.New<IStorageDocs>();
				storageDocs[StorageDocsSchema.SC_SM.Name] = storageMainPK;
				storageDocs[StorageDocsSchema.SC_DataType.Name] = "TIF";
				storageDocs[StorageDocsSchema.SC_Date.Name] = documentDate;
				storageDocs[StorageDocsSchema.SC_DocType.Name] = "MBL";
				storageDocs[StorageDocsSchema.SC_ImageData.Name] = File.ReadAllBytes(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs", "Small.tif"));
				storageDocs[StorageDocsSchema.SC_Desc.Name] = "Testing Dummy";
				storageDocs[StorageDocsSchema.SC_IsPublished.Name] = true;

				var storageFile = (BusinessObject)storageDocsFactory.New<IStorageFile>();
				storageFile[StorageDocsSchema.SC_SM.Name] = storageMainPK;
				storageFile[StorageDocsSchema.SC_Date.Name] = documentDate;
				storageFile[StorageDocsSchema.SC_DocType.Name] = "QUO";

				pkg.StorageDoc = storageDocs;
			}
			if (save)
			{
				documentFactory.Save();
			}

			return pkg;
		}

		class TestingStorageDocPackage
		{
			public BusinessObject BusinessObject { get; set; }
			public ZGuid BusinessObjectPK { get { return BusinessObject != null ? BusinessObject.PK : ZGuid.Empty; } }

			public BusinessObject StorageDoc { get; set; }
			public ZGuid StorageDocPK { get { return StorageDoc != null ? StorageDoc.PK : ZGuid.Empty; } }

			public BusinessObjectFactory Factory { get; set; }
			public void Save()
			{
				if (Factory != null)
				{
					Factory.Save();
				}
			}
		}

		#endregion

		ShowStorageDocUrlHandler UrlHandler
		{
			get { return ShowStorageDocUrlHandler.Instance; }
		}
	}
}
