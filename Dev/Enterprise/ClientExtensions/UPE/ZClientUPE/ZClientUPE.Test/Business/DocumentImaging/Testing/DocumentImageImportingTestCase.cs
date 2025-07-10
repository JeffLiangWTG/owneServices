using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.CommercialInvoice;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Client.UPE.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.DocumentImaging.Testing
{
	abstract class DocumentImageImportingTestCase : TestCaseWithFactory
	{
		protected string TestFilesPath
		{
			get
			{
				return UPETestHelper.TestFiles.DocumentImaging.Folder;
			}
		}

		protected void CopyTestFileToTempRepository(string searchPattern)
		{
			foreach (FileInfo file in new DirectoryInfo(TestFilesPath).GetFiles(searchPattern))
			{
				CopyTestFileToTempRepository(file.Name, file.Name);
			}
		}

		protected void CopyTestFileToTempRepository(string sourceFileName, string targetFileName)
		{
			string testFile = Path.Combine(Env.TempPath, targetFileName);
			File.Copy(Path.Combine(TestFilesPath, sourceFileName), testFile);
			File.SetAttributes(testFile, FileAttributes.Normal);
			File.SetCreationTime(testFile, ZDateTime.Now.AddSeconds(-6).ToDateTime());
		}

		protected void ReplaceInFile(string fileName, string searchFor, string replaceWith)
		{
			string content = File.ReadAllText(fileName);
			content = content.Replace(searchFor, replaceWith);
			File.WriteAllText(fileName, content);
		}

		protected DocumentImageTypeCollection DocumentTypes
		{
			get
			{
				return fDocumentTypes ?? (fDocumentTypes = UPEDataRegistry.Instance.DocumentImagingImageTypes.Value);
			}
		}

		DocumentImageTypeCollection fDocumentTypes;
		protected DocumentImageType DocumentType;
		protected void UpdateDocumentTypesRegistryValue()
		{
			UPEDataRegistry.Instance.DocumentImagingImageTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DocumentTypes);
		}

		protected GlbGroup EmailNotificationGroup
		{
			get
			{
				if (fEmailNotificationGroup == null)
				{
					fEmailNotificationGroup = Factory.New<GlbGroup>();
					GlbStaff recipient = fEmailNotificationGroup.Staff.AddNew();
					recipient.GS_EmailAddress = "bob@edi.com.au";
					recipient.GS_Code = "ZAC";
				}

				return fEmailNotificationGroup;
			}
		}

		GlbGroup fEmailNotificationGroup;
		#region CusHAWB and related StorageMain
		protected StorageMain CusHAWBStorageMain
		{
			get
			{
				if (fCusHAWBStorageMain == null)
				{
					fCusHAWBStorageMain = Factory.New<StorageMain>();
					fCusHAWBStorageMain.SM_ParentFK = CusHAWB.PK;
					fCusHAWBStorageMain.SM_DB = 1;
				}

				return fCusHAWBStorageMain;
			}
		}

		StorageMain fCusHAWBStorageMain;
		protected StorageDocs CusHAWBStorageDoc
		{
			get
			{
				if (fCusHAWBStorageDoc == null)
				{
					fCusHAWBStorageDoc = CusHAWBStorageMain.Documents.AddNew();
					fCusHAWBStorageDoc.SC_Date = ZDateTime.Now;
					fCusHAWBStorageDoc.SC_ImageData = UPETestHelper.TestFiles.CommercialInvoiceBmpBytes;
					fCusHAWBStorageDoc.SC_DocType = CommercialInvoiceDocManager.commercialInvoiceDocType;
				}

				return fCusHAWBStorageDoc;
			}
		}

		StorageDocs fCusHAWBStorageDoc;
		protected UPECusHAWB CusHAWB
		{
			get
			{
				if (fCusHAWB == null)
				{
					fCusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
					fCusHAWB.CS_CM = Factory.NewWithValidTestData<CusMAWB>().PK;
					fCusHAWB.WayBillShort = "M1302370459";
				}

				return fCusHAWB;
			}
		}

		UPECusHAWB fCusHAWB;
		#endregion
		#region JobDeclaration and related StorageMain
		protected StorageMain JobDeclarationStorageMain
		{
			get
			{
				if (fJobDeclarationStorageMain == null)
				{
					fJobDeclarationStorageMain = Factory.New<StorageMain>();
					fJobDeclarationStorageMain.SM_ParentFK = JobDeclaration.PK;
					fJobDeclarationStorageMain.SM_DB = 1;
				}

				return fJobDeclarationStorageMain;
			}
		}

		StorageMain fJobDeclarationStorageMain;
		protected StorageDocs JobDeclarationStorageDoc
		{
			get
			{
				if (fJobDeclarationStorageDoc == null)
				{
					fJobDeclarationStorageDoc = JobDeclarationStorageMain.Documents.AddNew();
					fJobDeclarationStorageDoc.SC_Date = ZDateTime.Now;
					fJobDeclarationStorageDoc.SC_ImageData = UPETestHelper.TestFiles.CommercialInvoiceBmpBytes;
					fJobDeclarationStorageDoc.SC_DocType = CommercialInvoiceDocManager.commercialInvoiceDocType;
				}

				return fJobDeclarationStorageDoc;
			}
		}

		StorageDocs fJobDeclarationStorageDoc;
		protected UPEJobDeclaration JobDeclaration
		{
			get
			{
				if (fJobDeclaration == null)
				{
					fJobDeclaration = Factory.NewWithValidTestData<UPEJobDeclaration>();
					fJobDeclaration.JE_AgentsReference = "M1302370459"; // short HAWB
				}

				return fJobDeclaration;
			}
		}

		UPEJobDeclaration fJobDeclaration;
		protected void RecreateJobDeclaration()
		{
			fJobDeclaration.Delete();
			fJobDeclaration = null;
		}

		#endregion
		#region OrgHeader and related StorageMain
		protected StorageMain OrgStorageMain
		{
			get
			{
				if (fOrgStorageMain == null)
				{
					fOrgStorageMain = Factory.New<StorageMain>();
					fOrgStorageMain.SM_ParentFK = Organisation.PK;
				}

				return fOrgStorageMain;
			}
		}

		StorageMain fOrgStorageMain;
		protected StorageDocs OrgStorageDoc
		{
			get
			{
				if (fOrgStorageDoc == null)
				{
					fOrgStorageDoc = OrgStorageMain.Documents.AddNew();
					fOrgStorageDoc.SC_Date = ZDateTime.Now;
					fOrgStorageDoc.SC_ImageData = UPETestHelper.TestFiles.CommercialInvoiceBmpBytes;
					fOrgStorageDoc.SC_DocType = CommercialInvoiceDocManager.commercialInvoiceDocType;
				}

				return fOrgStorageDoc;
			}
		}

		StorageDocs fOrgStorageDoc;
		protected OrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.NewWithValidTestData<OrgHeader>();
				}

				return fOrganisation;
			}
		}

		OrgHeader fOrganisation;
		#endregion
		#region Implementation
		void CreateDocumentImageTypes()
		{
			DocumentType = DocumentTypes.AddNew("03", "MSC", "UPS Test Document Type", false, false);
			UpdateDocumentTypesRegistryValue();
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			EmailNotificationGroup.Factory.Save();
			UPEDataRegistry.Instance.DocumentImagingNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationGroup.PK.ToGuid());
			UPEDataRegistry.Instance.DocumentImagingRepository = new DirectoryInfo(Env.TempPath);
			CreateDocumentImageTypes();
		}

		protected override void TearDown()
		{
			base.TearDown();
			foreach (FileInfo file in new DirectoryInfo(Env.TempPath).GetFiles())
			{
				file.Attributes = FileAttributes.Normal;
				file.Delete();
			}

			foreach (string folderName in Directory.GetDirectories(Env.TempPath))
			{
				new DirectoryInfo(folderName).Delete(true);
			}
		}
		#endregion
	}
}
