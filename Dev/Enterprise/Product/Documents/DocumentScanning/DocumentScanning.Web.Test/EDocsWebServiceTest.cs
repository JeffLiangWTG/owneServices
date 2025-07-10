using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineIntegration;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Services.Testing
{
	sealed class EDocsWebServiceTest : TestCaseWithFactory
	{
		public void TestAddOrUpdateEDocWithEmptyFile()
		{
			var eDocDetail = new EDocDetail()
			{
				DocumentTypePK = mscDocType.PK.ToGuid(),
				Description = "HELLO WORLD",
				FileName = "Testing.txt",
				IsPublished = true,
			};

			var service = new EDocsService();
			var result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, Array.Empty<byte>(), true);
			AssertEquals(EDocUpdateStatus.InvalidDocument, result.Status);
		}

		public void TestGetEDocDetailWithoutDocType()
		{
			var eDoc = storageMain.AddFileOrDocument(
				new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto { FileName = "Testing", DocumentType = "TTT" });

			eDoc.SC_Desc = "Jerry Testing";
			eDoc.SC_IsPublished = true;

			documentFactory.Save();

			var service = new EDocsService();

			AssertNoExceptionThrown(() => service.GetEDocDetails(businessObjectPK, true, true));
		}

		[TestDate(2024, 1, 1, 12, 15, 00)]
		public void TestAddOrUpdateEDoc()
		{
			var eDocDetail = new EDocDetail()
			{
				DocumentTypePK = mscDocType.PK.ToGuid(),
				Description = "HELLO WORLD",
				FileName = "Testing.txt",
				IsPublished = true,
			};

			var service = new EDocsService();
			var result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, new byte[] { 1, 2, 3 }, true);
			AssertEquals(EDocUpdateStatus.Succeeded, result.Status);

			var parentInNewFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<OrgHeader>(businessObjectPK);

			var eDocs = ((IDocManagerSupport)parentInNewFactory).DocManagerInfo.AllEDocs;
			AssertEquals(1, eDocs.Count);

			var eDoc = (StorageDocsBase)eDocs[0];
			AssertEquals(Core.Constants.RefDocTypes.MiscellaneousDocument, eDoc.SC_DocType);
			AssertEquals("HELLO WORLD", eDoc.SC_DescMultilingual);
			AssertEquals("Testing", eDoc.SC_FileName);
			AssertEquals("TXT", eDoc.SC_DataType);
			AssertEquals(true, eDoc.SC_IsPublished);
			AssertEquals(new byte[] { 1, 2, 3 }, eDoc.SC_ImageData);

			AssertEquals(eDoc.PK.ToGuid(), result.EDocDetail.Id);
			AssertEquals(eDoc.SC_AddingUser, result.EDocDetail.CreatingUser);
			AssertEquals(eDoc.ParentMain.SM_DB, result.EDocDetail.DatabaseNumber);
			AssertEquals("TXT", result.EDocDetail.DataType);
			AssertEquals(eDoc.SC_Date.ToDateTime(), result.EDocDetail.DateAdded);
			AssertEquals(DateTimeKind.Utc, result.EDocDetail.DateAdded.Kind);
			AssertEquals("HELLO WORLD", result.EDocDetail.Description);
			AssertEquals(eDoc.DocType.PK.ToGuid(), result.EDocDetail.DocumentTypePK);
			AssertEquals(eDoc.DocType.RT_DocType, result.EDocDetail.DocumentTypeCode);
			AssertEquals(eDoc.DocType.RT_DescMultilingual, result.EDocDetail.DocumentTypeDescription);
			AssertEquals("Testing", result.EDocDetail.FileName);
			AssertEquals(false, result.EDocDetail.IsDeleted);
			AssertEquals(true, result.EDocDetail.IsPublished);
			AssertEquals(false, result.EDocDetail.IsSystemGenerated);
			AssertEquals(eDoc.SC_SystemCreateTimeUtc, result.EDocDetail.LastEditDate);
			AssertEquals(DateTimeKind.Utc, result.EDocDetail.LastEditDate.Kind);
			AssertEquals(eDoc.SC_LastEditingUser, result.EDocDetail.LastEditUser);

			eDocDetail.Id = eDoc.PK.ToGuid();
			result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, new byte[] { 1, 2, 4 }, true);
			AssertEquals(EDocUpdateStatus.Succeeded, result.Status);

			parentInNewFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<OrgHeader>(businessObjectPK);
			eDocs = ((IDocManagerSupport)parentInNewFactory).DocManagerInfo.AllEDocs;
			AssertEquals(1, eDocs.Count);

			eDoc = (StorageDocsBase)eDocs[0];
			AssertEquals(Core.Constants.RefDocTypes.MiscellaneousDocument, eDoc.SC_DocType);
			AssertEquals("HELLO WORLD", eDoc.SC_DescMultilingual);
			AssertEquals("Testing", eDoc.SC_FileName);
			AssertEquals("TXT", eDoc.SC_DataType);
			AssertEquals(true, eDoc.SC_IsPublished);
			AssertEquals(new byte[] { 1, 2, 4 }, eDoc.SC_ImageData);

			AssertEquals(eDoc.PK.ToGuid(), result.EDocDetail.Id);
			AssertEquals(eDoc.SC_AddingUser, result.EDocDetail.CreatingUser);
			AssertEquals(eDoc.ParentMain.SM_DB, result.EDocDetail.DatabaseNumber);
			AssertEquals("TXT", result.EDocDetail.DataType);
			AssertEquals(eDoc.SC_Date.ToDateTime(), result.EDocDetail.DateAdded);
			AssertEquals(DateTimeKind.Utc, result.EDocDetail.DateAdded.Kind);
			AssertEquals("HELLO WORLD", result.EDocDetail.Description);
			AssertEquals(eDoc.DocType.PK.ToGuid(), result.EDocDetail.DocumentTypePK);
			AssertEquals(eDoc.DocType.RT_DocType, result.EDocDetail.DocumentTypeCode);
			AssertEquals(eDoc.DocType.RT_DescMultilingual, result.EDocDetail.DocumentTypeDescription);
			AssertEquals("Testing", result.EDocDetail.FileName);
			AssertEquals(false, result.EDocDetail.IsDeleted);
			AssertEquals(true, result.EDocDetail.IsPublished);
			AssertEquals(false, result.EDocDetail.IsSystemGenerated);
			AssertEquals(eDoc.SC_SystemLastEditTimeUtc, result.EDocDetail.LastEditDate);
			AssertEquals(DateTimeKind.Utc, result.EDocDetail.LastEditDate.Kind);
			AssertEquals(eDoc.SC_LastEditingUser, result.EDocDetail.LastEditUser);

			eDocDetail.IsPublished = false;
			result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, null, true);
			AssertEquals(EDocUpdateStatus.Succeeded, result.Status);

			parentInNewFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<OrgHeader>(businessObjectPK);
			eDocs = ((IDocManagerSupport)parentInNewFactory).DocManagerInfo.AllEDocs;
			eDoc = (StorageDocsBase)eDocs[0];
			AssertEquals(false, eDoc.SC_IsPublished);
			AssertEquals(new byte[] { 1, 2, 4 }, eDoc.SC_ImageData);
			AssertEquals(false, result.EDocDetail.IsPublished);

			parentInNewFactory.Logs.Find(x => x.SL_SE_NKEvent == Events.DocumentImported.Code).Single();
		}

		public void TestAddOrUpdateEDoc_DoesNotConvertImageToTIFF()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var imageName = "test-image.png";
				var imagePath = resourceRetriever.SaveResourceToFile($"Enterprise.DocumentScanning.Web.Test.{imageName}");
				var imageData = File.ReadAllBytes(imagePath);

				var eDocDetail = new EDocDetail()
				{
					DocumentTypePK = mscDocType.PK.ToGuid(),
					Description = "MY IMAGE",
					FileName = imageName,
					IsPublished = true,
				};

				var service = new EDocsService();
				var result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, imageData, true);
				AssertEquals("PNG", result.EDocDetail.DataType);
			}
		}

		public void TestAddOrUpdateEDoc_BusinessObjectNotFound()
		{
			var eDocDetail = new EDocDetail()
			{
				DocumentTypePK = mscDocType.PK.ToGuid(),
				Description = "HELLO WORLD",
				FileName = "Testing.txt",
				IsPublished = true,
			};

			var service = new EDocsService();
			var result = service.AddOrUpdateEDoc(tablePrefix, Guid.NewGuid(), eDocDetail, new byte[] { 1, 2, 3 }, true);
			AssertEquals(EDocUpdateStatus.BusinessObjectNotFound, result.Status);
		}

		public void TestAddOrUpdateEDoc_BusinessObjectNotSupported()
		{
			var eDocDetail = new EDocDetail()
			{
				DocumentTypePK = mscDocType.PK.ToGuid(),
				Description = "HELLO WORLD",
				FileName = "Testing.txt",
				IsPublished = true,
			};

			var service = new EDocsService();
			var result = service.AddOrUpdateEDoc("SM", storageMain.PK.ToGuid(), eDocDetail, new byte[] { 1, 2, 3 }, true);
			AssertEquals(EDocUpdateStatus.BusinessObjectNotSupported, result.Status);
		}

		public void TestAddOrUpdateEDoc_InvalidDocumentType()
		{
			var eDocDetail = new EDocDetail()
			{
				DocumentTypePK = Guid.NewGuid(),
				Description = "HELLO WORLD",
				FileName = "Testing.txt",
				IsPublished = true,
			};

			var service = new EDocsService();
			var result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, new byte[] { 1, 2, 3 }, true);
			AssertEquals(EDocUpdateStatus.InvalidDocumentType, result.Status);
		}

		public void TestAddOrUpdateEDoc_UnpublishedFileForContact()
		{
			var contact = documentFactory.NewWithValidTestData<OrgContact>();
			documentFactory.Save();

			var eDocDetail = new EDocDetail()
			{
				DocumentTypePK = mscDocType.PK.ToGuid(),
				Description = "HELLO WORLD",
				FileName = "Testing.txt",
				IsPublished = false,
			};

			var service = new EDocsService();
			var result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, new byte[] { 1, 2, 3 }, false, contact.PK);
			AssertEquals(EDocUpdateStatus.UnpublishedForContact, result.Status);
		}

		public void TestAddOrUpdateEDoc_DocumentTypeForContact()
		{
			var dummyDocType = documentFactory.NewWithValidTestData<RefDocType>();
			dummyDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ComplianceReport;
			dummyDocType.RT_DocType = "AAA";
			dummyDocType.RT_IsActive = true;
			dummyDocType.RT_IsPublished = true;

			var webSecurity = new DocumentWebSecurityRights(documentFactory);
			var docTypeSecurity = webSecurity.GetSecurityRight(dummyDocType);
			var contact = documentFactory.NewWithValidTestData<OrgContact>();
			documentFactory.Save();

			GrantSecurityRightToContact(contact, dummyDocType, false);
			var eDocDetail = new EDocDetail()
			{
				DocumentTypePK = dummyDocType.PK.ToGuid(),
				Description = "HELLO WORLD",
				FileName = "Testing.txt",
				IsPublished = true,
			};

			var service = new EDocsService();
			var result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, new byte[] { 1, 2, 3 }, false, contact.PK);
			AssertEquals(EDocUpdateStatus.InvalidDocumentType, result.Status);

			GrantSecurityRightToContact(contact, dummyDocType, true);

			documentFactory.Save();
			result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, new byte[] { 1, 2, 3 }, false, contact.PK);
			AssertEquals(EDocUpdateStatus.Succeeded, result.Status);
		}

		public void TestAddOrUpdateEDoc_EDocNotFound()
		{
			var eDocDetail = new EDocDetail()
			{
				Id = Guid.NewGuid(),
				DocumentTypePK = mscDocType.PK.ToGuid(),
				Description = "HELLO WORLD",
				FileName = "Testing.txt",
				IsPublished = true,
			};

			var service = new EDocsService();
			var result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, new byte[] { 1, 2, 3 }, true);
			AssertEquals(EDocUpdateStatus.EDocNotFound, result.Status);
		}

		public void TestAddOrUpdateEDoc_ContentNotSpecified()
		{
			var eDocDetail = new EDocDetail()
			{
				DocumentTypePK = mscDocType.PK.ToGuid(),
				Description = "HELLO WORLD",
				FileName = "Testing.txt",
				IsPublished = true,
			};

			var service = new EDocsService();
			var result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, null, true);
			AssertEquals(EDocUpdateStatus.ContentNotSpecified, result.Status);
		}

		public void TestAddOrUpdateEDoc_WithVirusDetected()
		{
			using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var eDocDetail = new EDocDetail()
				{
					DocumentTypePK = mscDocType.PK.ToGuid(),
					Description = "HELLO WORLD",
					FileName = "TestingVirus.txt",
					IsPublished = true,
				};

				var moqAmsiContext = new Mock<IAmsiContext>();
				var moqAmsiSession = new Mock<IAmsiSession>();
				moqAmsiSession.Setup(session => session.IsMalware(It.IsAny<byte[]>(), It.IsAny<string>())).Returns(() => true);
				moqAmsiContext.Setup(context => context.CreateSession()).Returns(moqAmsiSession.Object);
				using (ObjectFactory.Substitute(moqAmsiContext.Object))
				{
					var service = new EDocsService();
					var result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, new byte[] { 1, 2, 3 }, true);
					AssertEquals(EDocUpdateStatus.VirusDetected, result.Status);
				}
			}
		}

		public void TestGetEdocDetails_AppliesModuleSecurity()
		{
			var edocsHost = documentFactory.NewWithValidTestData<GlbStaff>();
			var storageMain = documentFactory.New<StorageMain>();
			storageMain.SM_DB = 1;
			storageMain.SM_Type = Core.Constants.DocManagerCodes.Staff;
			storageMain.SM_ParentFK = edocsHost.PK;

			documentFactory.Save();

			var edoc = storageMain.AddFileOrDocument(
				new byte[] { 1, 2, 3 },
				new AddFileOrDocumentDto { FileName = "Testing", DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument });

			edoc.SC_IsPublished = true;

			documentFactory.Save();

			var service = new EDocsService();
			((IEDocsSecurity)edocsHost).EdocsSecurityCheckpoint.IsAllowed = false;
			var edocs = service.GetEDocDetails(edocsHost.PK.ToGuid(), true, true, null);
			AssertEquals(0, edocs.Length);

			((IEDocsSecurity)edocsHost).EdocsSecurityCheckpoint.IsAllowed = true;
			edocs = service.GetEDocDetails(edocsHost.PK.ToGuid(), true, true, null);
			AssertEquals(1, edocs.Length);
		}

		public void TestGetEDocDetails()
		{
			var webSecurity = new DocumentWebSecurityRights(documentFactory);
			var invoiceDocSecurity = webSecurity.GetSecurityRight(invoiceDocType);

			var contact = documentFactory.NewWithValidTestData<OrgContact>();
			GrantSecurityRightToContact(contact, mscDocType, true);
			GrantSecurityRightToContact(contact, invoiceDocType, true);

			var eDoc1 = storageMain.AddFileOrDocument(
				new byte[] { 1, 2, 3 },
				new AddFileOrDocumentDto { FileName = "Testing", DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument });

			eDoc1.SC_Desc = "HELLO WORLD";
			eDoc1.SC_IsPublished = true;

			var eDoc2 = storageMain.AddFileOrDocument(
				new byte[] { 4, 5, 6 },
				new AddFileOrDocumentDto { FileName = "Testing 2", DocumentType = Core.Constants.RefDocTypes.Invoice });

			eDoc2.SC_Desc = "HELLO WORLD 2";
			eDoc2.SC_IsPublished = false;

			var eDoc3 = storageMain.AddFileOrDocument(
				new byte[] { 7, 8, 9 },
				new AddFileOrDocumentDto { FileName = "Testing 3", DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument });

			eDoc3.SC_Desc = "HELLO WORLD 3";
			eDoc3.SC_IsPublished = true;

			var eDoc4 = storageMain.AddFileOrDocument(
				new byte[] { 10, 11, 12 },
				new AddFileOrDocumentDto { FileName = "Testing 4", DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument });

			eDoc4.SC_Desc = "HELLO WORLD 4";
			eDoc4.SC_IsPublished = false;

			documentFactory.Save();

			eDoc3.SC_IsDeleted = true;
			eDoc4.SC_IsDeleted = true;

			documentFactory.Save();

			var service = new EDocsService();
			var includeDeleted = true;
			var includeUnpublished = true;
			var eDocDetails = service.GetEDocDetails(businessObjectPK, !includeDeleted, includeUnpublished);
			AssertEquals(2, eDocDetails.Length);

			AssertEquals(eDoc1.PK.ToGuid(), eDocDetails[0].Id);
			AssertEquals(mscDocType.PK, eDocDetails[0].DocumentTypePK);
			AssertEquals(mscDocType.RT_DocType, eDocDetails[0].DocumentTypeCode);
			AssertEquals(mscDocType.RT_DescMultilingual, eDocDetails[0].DocumentTypeDescription);
			AssertEquals("HELLO WORLD", eDocDetails[0].Description);
			AssertEquals("Testing", eDocDetails[0].FileName);
			AssertEquals(false, eDocDetails[0].IsSystemGenerated);
			AssertEquals(true, eDocDetails[0].IsPublished);
			AssertEquals(false, eDocDetails[0].IsDeleted);
			AssertEquals("Organization (UnitTest)", eDocDetails[0].OwnerReadableName);
			AssertEquals(eDoc1.SC_Date.ToDateTime().ToString("YYYYMMddHHmm"), eDocDetails[0].DateAdded.ToString("YYYYMMddHHmm"));

			AssertEquals(eDoc2.PK.ToGuid(), eDocDetails[1].Id);
			AssertEquals(invoiceDocType.PK, eDocDetails[1].DocumentTypePK);
			AssertEquals(invoiceDocType.RT_DocType, eDocDetails[1].DocumentTypeCode);
			AssertEquals(invoiceDocType.RT_DescMultilingual, eDocDetails[1].DocumentTypeDescription);
			AssertEquals("HELLO WORLD 2", eDocDetails[1].Description);
			AssertEquals("Testing 2", eDocDetails[1].FileName);
			AssertEquals(false, eDocDetails[1].IsSystemGenerated);
			AssertEquals(false, eDocDetails[1].IsPublished);
			AssertEquals(false, eDocDetails[1].IsDeleted);
			AssertEquals("Organization (UnitTest)", eDocDetails[1].OwnerReadableName);
			AssertEquals(eDoc2.SC_Date.ToDateTime().ToString("YYYYMMddHHmm"), eDocDetails[1].DateAdded.ToString("YYYYMMddHHmm"));

			eDocDetails = service.GetEDocDetails(businessObjectPK, !includeDeleted, includeUnpublished, contact.PK);
			AssertEquals(2, eDocDetails.Length);
			AssertEquals(eDoc1.PK.ToGuid(), eDocDetails[0].Id);
			AssertEquals(eDoc2.PK.ToGuid(), eDocDetails[1].Id);

			GrantSecurityRightToContact(contact, invoiceDocType, false);
			documentFactory.Save();
			eDocDetails = service.GetEDocDetails(businessObjectPK, !includeDeleted, includeUnpublished, contact.PK);
			AssertEquals(1, eDocDetails.Length);
			AssertEquals(eDoc1.PK.ToGuid(), eDocDetails[0].Id);

			eDocDetails = service.GetEDocDetails(businessObjectPK, !includeDeleted, !includeUnpublished);
			AssertEquals(1, eDocDetails.Length);

			AssertEquals(eDoc1.PK.ToGuid(), eDocDetails[0].Id);

			eDocDetails = service.GetEDocDetails(businessObjectPK, includeDeleted, includeUnpublished);
			AssertEquals(4, eDocDetails.Length);
			AssertEquals(eDoc1.PK.ToGuid(), eDocDetails[0].Id);
			AssertEquals(eDoc2.PK.ToGuid(), eDocDetails[1].Id);
			AssertEquals(eDoc3.PK.ToGuid(), eDocDetails[2].Id);
			AssertEquals(eDoc4.PK.ToGuid(), eDocDetails[3].Id);

			eDocDetails = service.GetEDocDetails(businessObjectPK, includeDeleted, !includeUnpublished);
			AssertEquals(2, eDocDetails.Length);

			AssertEquals(eDoc1.PK.ToGuid(), eDocDetails[0].Id);
			AssertEquals(eDoc3.PK.ToGuid(), eDocDetails[1].Id);
		}

		public void TestAddOrUpdateEDoc_ConcurrencyHandling()
		{
			var newParent = documentFactory.New<OrgHeader>();
			newParent.OH_Code = "Concurrency";
			documentFactory.Save();
			var service = new EDocsService();

			var newParentPK = newParent.PK.ToGuid();
			var first = true;
			EDocUpdateResult firstResult = null;
			EDocUpdateResult secondResult = null;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(_ =>
			{
				if (first)
				{
					first = false;
					firstResult = service.AddOrUpdateEDoc(tablePrefix, newParentPK, new EDocDetail
					{
						FileName = "Testing",
						DocumentTypePK = mscDocType.PK.ToGuid(),
						Description = "HELLO WORLD",
						IsPublished = true
					}, new byte[] { 1, 2, 3 }, true);
				}
			});

			AssertNoExceptionThrown(() =>
			{
				secondResult = service.AddOrUpdateEDoc(tablePrefix, newParentPK, new EDocDetail
				{
					FileName = "Testing",
					DocumentTypePK = mscDocType.PK.ToGuid(),
					Description = "HELLO WORLD",
					IsPublished = true
				}, new byte[] { 1, 2, 3 }, true);
			});

			AssertEquals(EDocUpdateStatus.Succeeded, firstResult.Status);
			AssertEquals(EDocUpdateStatus.Succeeded, secondResult.Status);
		}

		public void TestAddOrUpdateEDoc_WhenContactUpadtesAnAvailableEDoc_ThenReturnSucceeded()
		{
			var service = new EDocsService();
			var (eDocPK, contactPK) = SetUp_UserIsContactAndEDocIsAvailable();
			var eDocDetail = new EDocDetail()
			{
				Id = eDocPK,
				DocumentTypePK = contactAvailableDocType.PK.ToGuid(),
				Description = "HELLO WORLD",
				FileName = "Testing.txt",
				IsPublished = true,
			};

			var result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, new byte[] { 1, 2, 3 }, false, contactPK);

			AssertEquals(EDocUpdateStatus.Succeeded, result.Status);
		}

		public void TestAddOrUpdateEDoc_WhenUpdatingUnpublishedEDoc_ThenReturnEDocNotFound()
		{
			var service = new EDocsService();
			var eDocPK = SetUp_EDocIsNotPublished();
			var eDocDetail = new EDocDetail()
			{
				Id = eDocPK,
				DocumentTypePK = mscDocType.PK.ToGuid(),
				Description = "HELLO WORLD",
				FileName = "Testing.txt",
				IsPublished = true,
			};

			var result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, new byte[] { 1, 2, 3 }, false);

			AssertEquals(EDocUpdateStatus.EDocNotFound, result.Status);
		}

		public void TestAddOrUpdateEDoc_WhenUpdatingEDocThatNotAvailableForCurrentEnvContext_ThenReturnEDocNotFound()
		{
			var service = new EDocsService();
			var eDocPK = SetUp_EDocIsNotAvailableForCurrentEnvContext();
			var eDocDetail = new EDocDetail()
			{
				Id = eDocPK,
				DocumentTypePK = mscDocType.PK.ToGuid(),
				Description = "HELLO WORLD",
				FileName = "Testing.txt",
				IsPublished = true,
			};

			var result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, new byte[] { 1, 2, 3 }, false);

			AssertEquals(EDocUpdateStatus.EDocNotFound, result.Status);
		}

		public void TestAddOrUpdateEDoc_WhenContactUpdatesAnEDocsButCanNotViewIt_ThenReturnEDocNotFound()
		{
			var service = new EDocsService();
			var (eDocPK, contactPK) = SetUp_UserIsContactAndCanNotViewDocument();
			var eDocDetail = new EDocDetail()
			{
				Id = eDocPK,
				DocumentTypePK = contactAvailableDocType.PK.ToGuid(),
				Description = "HELLO WORLD",
				FileName = "Testing.txt",
				IsPublished = true,
			};

			var result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, new byte[] { 1, 2, 3 }, false, contactPK);

			AssertEquals(EDocUpdateStatus.EDocNotFound, result.Status);
		}

		public void TestAddOrUpdateEDoc_UnpublishOlderVersionDocs()
		{
			SystemDataRegistry.Instance.UnpublishOlderVersionDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var service = new EDocsService();

			// Set up an eDoc with file name Testing.TIF
			var (eDocPK, contactPK) = SetUp_UserIsContactAndEDocIsAvailable();

			// Add another eDoc with same file name via service to supersede the previous one
			var eDocDetail = new EDocDetail()
			{
				DocumentTypePK = dummyDocType.PK.ToGuid(),
				FileName = "Testing.tif",
				IsPublished = true,
			};

			var result = service.AddOrUpdateEDoc(tablePrefix, businessObjectPK, eDocDetail, new byte[] { 1, 2, 3 }, false, contactPK);
			AssertEquals("The new eDoc should be published", true, result.EDocDetail.IsPublished);

			var eDoc = storageMain.eDocs.First(x => x.PK == eDocPK) as IeDoc;
			AssertEquals("The original eDoc should be unpublished", false, eDoc.IsPublished);
		}

		#region TestGetEDocImageData

		public void TestGetEDocImageDataPublished()
		{
			var storageDoc = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto { FileName = "Testing.TIF", DocumentType = "MSC" });
			storageDoc.SC_IsPublished = true;
			documentFactory.Save();

			var service = new EDocsService();
			var includeUnpublished = true;
			var imageData = service.GetEDocImageData(storageDoc.PK.ToGuid(), storageMain.SM_DB, includeUnpublished);
			AssertEquals(new byte[] { 1, 2, 3 }, imageData.Data);
			AssertEquals("Testing.TIF", imageData.FullFileName);

			imageData = service.GetEDocImageData(storageDoc.PK.ToGuid(), storageMain.SM_DB, !includeUnpublished);
			AssertEquals(new byte[] { 1, 2, 3 }, imageData.Data);
			AssertEquals("Testing.TIF", imageData.FullFileName);
		}

		public void TestGetEDocImageDataUnpublished()
		{
			var storageDoc = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto { FileName = "Testing.TIF", DocumentType = "MSC" });
			storageDoc.SC_IsPublished = false;
			documentFactory.Save();

			var service = new EDocsService();
			var includeUnpublished = true;
			var imageData = service.GetEDocImageData(storageDoc.PK.ToGuid(), storageMain.SM_DB, includeUnpublished);
			AssertEquals(new byte[] { 1, 2, 3 }, imageData.Data);
			AssertEquals("Testing.TIF", imageData.FullFileName);

			imageData = service.GetEDocImageData(storageDoc.PK.ToGuid(), storageMain.SM_DB, !includeUnpublished);
			AssertNull(imageData);
		}

		public void TestGetEDocImageData_WhenUserIsContactAndEDocIsAvailable_ThenReturnData()
		{
			var service = new EDocsService();
			var (eDocPK, contactPK) = SetUp_UserIsContactAndEDocIsAvailable();

			var imageData = service.GetEDocImageData(eDocPK, storageMain.SM_DB, false, contactPK);

			AssertEquals(new byte[] { 1, 2, 3 }, imageData.Data);
			AssertEquals("Testing.TIF", imageData.FullFileName);
		}

		public void TestGetEDocImageData_WhenEDocIsNotAvailableForCurrentEnvContext_ThenReturnNull()
		{
			var service = new EDocsService();
			var eDocPK = SetUp_EDocIsNotAvailableForCurrentEnvContext();

			var imageData = service.GetEDocImageData(eDocPK, storageMain.SM_DB, false);

			AssertNull(imageData);
		}

		public void TestGetEDocImageData_WhenUserIsContactAndCanNotViewDocument_ThenReturnNull()
		{
			var service = new EDocsService();
			var (eDocPK, contactPK) = SetUp_UserIsContactAndCanNotViewDocument();

			var imageData = service.GetEDocImageData(eDocPK, storageMain.SM_DB, false, contactPK);

			AssertNull(imageData);
		}

		#endregion

		#region TestDeliverEDoc

		public void TestDeliverEDoc_WhenUserIsStaffAndEDocIsAvailable_ThenDeliverSuccess()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var storageMain = documentFactory.NewWithValidTestData<StorageMain>();
			var storageDoc = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto { FileName = "Testing.TIF", DocumentType = "MSC" });
			Factory.Save();
			documentFactory.Save();

			var service = new EDocsService();
			var deliveryInstructions = CreateDeliveryInstructions();

			service.DeliverEDoc(storageDoc.PK.ToGuid(), storageDoc.ParentMain.SM_DB, deliveryInstructions, true);

			AssertEDocIsDelivered();
		}

		public void TestDeliverEDoc_WhenUserIsContactAndEDocIsAvailable_ThenDeliverSuccess()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var service = new EDocsService();
			var (eDocPK, contactPK) = SetUp_UserIsContactAndEDocIsAvailable();
			var deliveryInstructions = CreateDeliveryInstructions();

			service.DeliverEDoc(eDocPK, storageMain.SM_DB, deliveryInstructions, false, contactPK);

			AssertEDocIsDelivered();
		}

		void AssertEDocIsDelivered()
		{
			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals(1, printJobs.Length);

			var printJob = printJobs[0];
			AssertEquals("unit.test@cargowise.com", printJob.SP_Destination);
			AssertEquals(new byte[] { 1, 2, 3 }, printJob.SP_CustomProperties);
		}

		public void TestDeliverEDoc_WhenEDocIsNotPublished_ThenDoNothing()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var service = new EDocsService();
			var eDocPK = SetUp_EDocIsNotPublished();
			var deliveryInstructions = CreateDeliveryInstructions();

			service.DeliverEDoc(eDocPK, storageMain.SM_DB, deliveryInstructions, false);

			AssertEDocIsNotDelivered();
		}

		public void TestDeliverEDoc_WhenEDocIsNotAvailableForCurrentEnvContext_ThenDoNothing()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var service = new EDocsService();
			var eDocPK = SetUp_EDocIsNotAvailableForCurrentEnvContext();
			var deliveryInstructions = CreateDeliveryInstructions();

			service.DeliverEDoc(eDocPK, storageMain.SM_DB, deliveryInstructions, false);

			AssertEDocIsNotDelivered();
		}

		public void TestDeliverEDoc_WhenUserIsContactAndCanNotViewDocument_ThenDoNothing()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var service = new EDocsService();
			var (eDocPK, contactPK) = SetUp_UserIsContactAndCanNotViewDocument();
			var deliveryInstructions = CreateDeliveryInstructions();

			service.DeliverEDoc(eDocPK, storageMain.SM_DB, deliveryInstructions, false, contactPK);

			AssertEDocIsNotDelivered();
		}

		DeliveryInstructionsBase CreateDeliveryInstructions()
		{
			var recipient = new DeliveryRecipientBase();
			recipient.Name = "Steve Jobs";
			recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.Email = "unit.test@cargowise.com";
			recipient.EmailAttachmentType = OrgConstants.AttachmentType.TIF;

			var deliveryInstructions = new DeliveryInstructionsBase();
			deliveryInstructions.Recipients = new[] { recipient };

			return deliveryInstructions;
		}

		void AssertEDocIsNotDelivered()
		{
			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals(0, printJobs.Length);
		}

		#endregion

		#region TestDeleteEDoc

		public void TestDeleteEDoc_WhenUserIsStaffAndEDocIsAvailable_ThenDeleteSuccess()
		{
			var eDoc1 = storageMain.AddFileOrDocument(
				new byte[] { 1, 2, 3 },
				new AddFileOrDocumentDto { FileName = "eDoc 1", DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument });

			storageMain.AddFileOrDocument(
				new byte[] { 4, 5, 6 },
				new AddFileOrDocumentDto { FileName = "eDoc 2", DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument });

			documentFactory.Save();

			var service = new EDocsService();
			var eDocs = service.GetEDocDetails(parent.PK.ToGuid(), false, true);
			AssertEquals(2, eDocs.Length);

			service.DeleteEDoc(eDoc1.PK.ToGuid(), storageMain.SM_DB, true);
			eDocs = service.GetEDocDetails(parent.PK.ToGuid(), true, true);
			AssertEquals(2, eDocs.Length);

			eDocs = service.GetEDocDetails(parent.PK.ToGuid(), false, true);
			AssertEquals(1, eDocs.Length);
			AssertEquals("eDoc 2", eDocs[0].FileName);
		}

		public void TestDeleteEDoc_WhenUserIsContactAndEDocIsAvailable_ThenDeleteSuccess()
		{
			var service = new EDocsService();
			var (eDocPK, contactPK) = SetUp_UserIsContactAndEDocIsAvailable();

			service.DeleteEDoc(eDocPK, storageMain.SM_DB, false, contactPK);

			AssertEDocIsDeleted(eDocPK);
		}

		public void TestDeleteEDoc_WhenEDocIsNotPublished_ThenDeleteNothing()
		{
			var service = new EDocsService();
			var eDocPK = SetUp_EDocIsNotPublished();

			service.DeleteEDoc(eDocPK, storageMain.SM_DB, false);

			AssertEDocIsNotDeleted(eDocPK);
		}

		public void TestDeleteEDoc_WhenEDocIsNotAvailableForCurrentEnvContext_ThenDeleteNothing()
		{
			var service = new EDocsService();
			var eDocPK = SetUp_EDocIsNotPublished();

			service.DeleteEDoc(eDocPK, storageMain.SM_DB, false);

			AssertEDocIsNotDeleted(eDocPK);
		}

		public void TestDeleteEDoc_WhenUserIsContactAndCanNotViewDocument_ThenDeleteNothing()
		{
			var service = new EDocsService();
			var (eDocPK, contactPK) = SetUp_UserIsContactAndCanNotViewDocument();

			service.DeleteEDoc(eDocPK, storageMain.SM_DB, false, contactPK);

			AssertEDocIsNotDeleted(eDocPK);
		}

		public void TestDeleteEDoc_WhenUserIsContactAndEDocIsNotCreatedByWebUser_ThenDeleteNothing()
		{
			var service = new EDocsService();
			var (eDocPK, contactPK) = SetUp_UserIsContactAndEDocIsAvailable(systemCreateUserOfEDoc: User.UnKnownUserCode);

			service.DeleteEDoc(eDocPK, storageMain.SM_DB, false, contactPK);

			AssertEDocIsNotDeleted(eDocPK);
		}

		void AssertEDocIsDeleted(Guid eDocPK) => Assert(GetEDoc(eDocPK).SC_IsDeleted);

		void AssertEDocIsNotDeleted(Guid eDocPK) => Assert(!GetEDoc(eDocPK).SC_IsDeleted);

		StorageDocs GetEDoc(Guid eDocPK)
		{
			var numberedFactory = documentFactory.GetFactory(storageMain.SM_DB);
			return numberedFactory.Load<StorageDocs>(new ZGuid(eDocPK));
		}

		#endregion

		public void TestGetEDocCount()
		{
			var doc = storageMain.AddFileOrDocument(
				new byte[] { 1, 2, 3 },
				new AddFileOrDocumentDto { FileName = "publishedDoc", DocumentType = Core.Constants.RefDocTypes.Invoice });
			doc.SC_IsPublished = true;

			doc = storageMain.AddFileOrDocument(
				new byte[] { 4, 5, 6 },
				new AddFileOrDocumentDto { FileName = "unpublishedDoc", DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument });
			doc.SC_IsPublished = false;

			doc = storageMain.AddFileOrDocument(
				new byte[] { 4, 5, 6 },
				new AddFileOrDocumentDto { FileName = "unpublishedDoc2", DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument });
			doc.SC_IsPublished = false;

			doc = storageMain.AddFileOrDocument(
				new byte[] { 7, 8, 9 },
				new AddFileOrDocumentDto { FileName = "deletedDoc", DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument });
			doc.SC_IsPublished = true;

			doc = storageMain.AddFileOrDocument(
				new byte[] { 7, 8, 9 },
				new AddFileOrDocumentDto { FileName = "deletedDoc", DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument });
			doc.SC_IsPublished = true;
			doc.SC_IsDeleted = true;

			documentFactory.Save();

			var service = new EDocsService();
			var allEDocCount = service.GetEDocCount(parent.PK.ToGuid());
			AssertEquals(2, allEDocCount);

			var webSecurity = new DocumentWebSecurityRights(documentFactory);
			var contact = documentFactory.NewWithValidTestData<OrgContact>();

			GrantSecurityRightToContact(contact, mscDocType, true);
			GrantSecurityRightToContact(contact, invoiceDocType, true);
			documentFactory.Save();

			allEDocCount = service.GetEDocCount(parent.PK.ToGuid(), contact.PK);
			AssertEquals(2, allEDocCount);

			GrantSecurityRightToContact(contact, invoiceDocType, false);
			documentFactory.Save();

			allEDocCount = service.GetEDocCount(parent.PK.ToGuid(), contact.PK);
			AssertEquals(1, allEDocCount);
		}

		public void TestEDocCount_EntityNotFound()
		{
			var service = new EDocsService();

			var count = service.GetEDocCount(new Guid("00000000-0000-0000-0000-000000000000"));
			AssertEquals(0, count);
		}

		public void TestGetRefDocTypes_FilterByActive()
		{
			var activeDocType = documentFactory.NewWithValidTestData<RefDocType>();
			activeDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			activeDocType.RT_IsActive = true;
			activeDocType.RT_IsPublished = true;

			var inactiveDocType = documentFactory.NewWithValidTestData<RefDocType>();
			inactiveDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			inactiveDocType.RT_IsActive = false;
			inactiveDocType.RT_IsPublished = true;

			documentFactory.Save();

			var service = new EDocsService();
			var docTypes = service.GetRefDocTypes(Core.Constants.ReferenceTypes.SupplyChainLogistics, includeUnpublished: false);

			var result = docTypes.Select(d => d.PK).ToArray();
			AssertCollectionContains("Should contain active doc types", activeDocType.PK, result);
			AssertCollectionNotContains("Should not contain inactive doc types", inactiveDocType.PK, result);
		}

		public void TestGetRefDocTypes_ShouldIgnoreCachedTable()
		{
			var activeDocType = documentFactory.NewWithValidTestData<RefDocType>();
			activeDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			activeDocType.RT_IsActive = true;
			activeDocType.RT_IsPublished = true;

			documentFactory.Save();

			var service = new EDocsService();
			var docTypes = service.GetRefDocTypes(Core.Constants.ReferenceTypes.SupplyChainLogistics, includeUnpublished: true);

			var result = docTypes.FirstOrDefault(d => d.PK == activeDocType.PK);
			AssertNotNull("Should contain doc types", result);
			AssertEquals("Should be Published", result.RT_IsPublished, true);

			using (RowFactory.SetCachedTables(RefDocType.Schema.TableName))
			{
				// Update RefDocType directly into DB without using factory
				var updateSql = string.Format(@"UPDATE {0} SET {1} = @published, RT_SystemLastEditTimeUtc = GetUtcDate(), RT_SystemLastEditUser = 'E' where {2} = @pk",
					RefDocType.Schema.TableName,
					RefDocType.Schema.RT_IsPublished,
					RefDocType.Schema.PK);
				using (var command = TestConnection.Command(updateSql))
				{
					command.AddParameter("@published", SqlDbType.Bit, 0);
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, activeDocType.PK.ToGuid());
					command.ExecuteNonQuery();
				}

				service = new EDocsService();
				docTypes = service.GetRefDocTypes(Core.Constants.ReferenceTypes.SupplyChainLogistics, includeUnpublished: true);

				result = docTypes.FirstOrDefault(d => d.PK == activeDocType.PK);
				AssertNotNull("Should contain doc types", result);
				AssertEquals("Should be Unpublished", result.RT_IsPublished, false);
			}
		}

		public void TestGetRefDocTypes_FilterByDocManagerCode()
		{
			var service = new EDocsService();
			var docTypes = service.GetRefDocTypes(Core.Constants.ReferenceTypes.SupplyChainLogistics, includeUnpublished: false);

			AssertGreaterThan("Should return doc types with selected reference type", GetReferenceTypeCount(Core.Constants.ReferenceTypes.SupplyChainLogistics), 0);
			AssertGreaterThan("Should return doc types with 'All' reference type", GetReferenceTypeCount(Core.Constants.ReferenceTypes.All), 0);
			AssertEquals("Should not return doc types with other reference types", GetReferenceTypeCountOtherThan(Core.Constants.ReferenceTypes.SupplyChainLogistics, Core.Constants.ReferenceTypes.All), 0);

			int GetReferenceTypeCount(string referenceType)
			{
				return docTypes.Count(d => d.RT_ReferenceType == referenceType);
			}

			int GetReferenceTypeCountOtherThan(params ZString[] referenceTypes)
			{
				return docTypes.Count(d => !referenceTypes.Contains(d.RT_ReferenceType));
			}
		}

		public void TestGetRefDocTypes_IncludeUnpublished()
		{
			var publishedDocType = documentFactory.NewWithValidTestData<RefDocType>();
			publishedDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			publishedDocType.RT_IsPublished = true;

			var unpublishedDocType = documentFactory.NewWithValidTestData<RefDocType>();
			unpublishedDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			unpublishedDocType.RT_IsPublished = false;

			documentFactory.Save();

			var service = new EDocsService();

			var unfilteredResult = GetRefDocTypes(true).Select(d => d.PK).ToArray();
			AssertCollectionContains("Should return published doc types", publishedDocType.PK, unfilteredResult);
			AssertCollectionContains("Should return unpublished doc types", unpublishedDocType.PK, unfilteredResult);

			Assert("Should return only published doc types", GetRefDocTypes(false).All(d => d.RT_IsPublished));

			IEnumerable<RefDocType> GetRefDocTypes(bool includeUnpublished) =>
				service.GetRefDocTypes(Core.Constants.ReferenceTypes.SupplyChainLogistics, includeUnpublished);
		}

		public void TestGetRefDocTypes_FilterByContactSecurityRight()
		{
			var availableDocType = documentFactory.NewWithValidTestData<RefDocType>();
			availableDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ComplianceReport;
			availableDocType.RT_IsActive = true;
			availableDocType.RT_IsPublished = true;

			var unavailableDocType = documentFactory.NewWithValidTestData<RefDocType>();
			unavailableDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ComplianceReport;
			unavailableDocType.RT_IsActive = true;
			unavailableDocType.RT_IsPublished = true;

			var webSecurity = new DocumentWebSecurityRights(documentFactory);
			var availableDocSecurity = webSecurity.GetSecurityRight(availableDocType);
			var unavailableDocSecurity = webSecurity.GetSecurityRight(unavailableDocType);

			var contact = documentFactory.NewWithValidTestData<OrgContact>();
			GrantSecurityRightToContact(contact, availableDocType, true);
			documentFactory.Save();

			GrantSecurityRightToContact(contact, unavailableDocType, false);
			documentFactory.Save();

			var service = new EDocsService();
			var docTypes = service.GetRefDocTypes(Core.Constants.ReferenceTypes.ComplianceReport, includeUnpublished: false, contact.PK);

			var result = docTypes.Select(d => d.PK).ToArray();
			AssertCollectionContains("Should contain available doc types", availableDocType.PK, result);
			AssertCollectionNotContains("Should not contain inactive doc types", unavailableDocType.PK, result);
		}

		#region Implementation

		(Guid eDocPK, Guid contactPK) SetUp_UserIsContactAndEDocIsAvailable(string systemCreateUserOfEDoc = User.WebUserCode)
		{
			GlbStaff.CurrentUser.GS_Code = systemCreateUserOfEDoc;

			var storageDoc = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto { FileName = "Testing.TIF", DocumentType = "MSC" });
			storageDoc.SC_IsPublished = true;
			storageDoc.SC_DocType = DummyDocTypeCode;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			GrantSecurityRightToContact(contact, contactAvailableDocType, true);
			GrantSecurityRightToContact(contact, dummyDocType, true);

			Factory.Save();
			documentFactory.Save();

			return (storageDoc.PK.ToGuid(), contact.PK.ToGuid());
		}

		(Guid eDocPK, Guid contactPK) SetUp_UserIsContactAndCanNotViewDocument()
		{
			var storageDoc = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto { FileName = "Testing.TIF", DocumentType = "MSC" });
			storageDoc.SC_IsPublished = true;
			storageDoc.SC_DocType = DummyDocTypeCode;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			GrantSecurityRightToContact(contact, contactAvailableDocType, true);
			GrantSecurityRightToContact(contact, dummyDocType, false);

			Factory.Save();
			documentFactory.Save();

			return (storageDoc.PK.ToGuid(), contact.PK.ToGuid());
		}

		void GrantSecurityRightToContact(OrgContact contact, RefDocType docType, bool granted)
		{
			if (GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.Value)
			{
				var webSecurity = new DocumentWebSecurityRights(documentFactory);
				var docSecurityRight = webSecurity.GetSecurityRight(docType);
				var groupCode = $"DOC_{docType.RT_ReferenceType}_{docType.RT_DocType}";
				var group = documentFactory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, groupCode));

				if (group == null)
				{
					group = documentFactory.New<GlbGroup>();
					group.GG_Type = "ORG";
					group.GG_Code = groupCode;
					group.GG_Desc = group.GG_Code;
				}

				var guQuery = new ZQuery(GlbSecuritySchema.GU_GG, group.PK);
				guQuery.AddToFilter(GlbSecuritySchema.GU_SecurityRight, docSecurityRight.SecurityItemName);
				var security = documentFactory.LoadTop1<GlbSecurity>(guQuery);
				if (security == null)
				{
					security = documentFactory.New<GlbSecurity>();
					security.GU_GG = group.PK;
					security.GU_SecurityRight = docSecurityRight.SecurityItemName;
					security.GU_SecurityItemIsAllowed = true;
				}

				var contactQuery = new ZQuery(GlbGroupOrgContactLinkSchema.GCK_OC_Contact, contact.PK);
				contactQuery.AddToFilter(GlbGroupOrgContactLinkSchema.GCK_GG_Group, group.PK);
				var contactLink = documentFactory.LoadTop1<GlbGroupOrgContactLink>(contactQuery);

				if (granted && contactLink == null)
				{
					var newContactLink = documentFactory.New<GlbGroupOrgContactLink>();
					newContactLink.GCK_GG_Group = group.PK;
					newContactLink.GCK_OC_Contact = contact.PK;
				}
				else if (!granted && contactLink != null)
				{
					contactLink.Delete();
				}
			}
			else
			{
				var webSecurity = new DocumentWebSecurityRights(documentFactory);
				var docSecurityRight = webSecurity.GetSecurityRight(docType);
				var orgSecurityRight = contact.Header.SecurityRights.AddNew();
				orgSecurityRight.OX_SecurityItemName = docSecurityRight.SecurityItemName;
				orgSecurityRight.OX_Granted = granted;
			}
		}

		Guid SetUp_EDocIsNotPublished()
		{
			var storageMain = documentFactory.NewWithValidTestData<StorageMain>();
			var storageDoc = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto { FileName = "Testing.TIF", DocumentType = "MSC" });
			storageDoc.SC_IsPublished = false;

			documentFactory.Save();

			return storageDoc.PK.ToGuid();
		}

		Guid SetUp_EDocIsNotAvailableForCurrentEnvContext()
		{
			var storageDoc = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto { FileName = "Testing.TIF", DocumentType = "MSC" });
			storageDoc.SC_IsPublished = true;
			storageDoc.SC_GC_Company = Guid.NewGuid();

			documentFactory.Save();

			return storageDoc.PK.ToGuid();
		}

		protected override void SetUp()
		{
			base.SetUp();
			(new DocManagerDBHelper()).LastWritableDatabaseWithFreeSpace(); //to ensure SD001 exists
			documentFactory = new DocumentFactoryProvider().GetFactory(Factory);

			parent = documentFactory.New<OrgHeader>();
			parent.OH_Code = "UnitTest";

			storageMain = documentFactory.New<StorageMain>();
			storageMain.SM_DB = 1;
			storageMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			storageMain.SM_ParentFK = parent.PK;

			documentFactory.Save();

			tablePrefix = parent.TablePrefix;
			businessObjectPK = parent.PK.ToGuid();

			mscDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.MiscellaneousDocument));
			invoiceDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.Invoice));

			dummyDocType = Factory.NewWithValidTestData<RefDocType>();
			dummyDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.All;
			dummyDocType.RT_DocType = DummyDocTypeCode;
			dummyDocType.RT_IsActive = true;
			dummyDocType.RT_IsPublished = true;

			contactAvailableDocType = Factory.NewWithValidTestData<RefDocType>();
			contactAvailableDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.All;
			contactAvailableDocType.RT_DocType = ContactAvailableDocTypeCode;
			contactAvailableDocType.RT_IsActive = true;
			contactAvailableDocType.RT_IsPublished = true;

			Factory.Save();
		}

		DocumentFactory documentFactory;
		OrgHeader parent;
		StorageMain storageMain;
		Guid businessObjectPK;
		string tablePrefix;
		RefDocType mscDocType;
		RefDocType invoiceDocType;

		RefDocType dummyDocType;
		RefDocType contactAvailableDocType;

		const string DummyDocTypeCode = "DMY";
		const string ContactAvailableDocTypeCode = "CAV";

		#endregion
	}
}
