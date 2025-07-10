using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class IOROrgPKJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckIOROrgPK()
		{
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			var declarationForIOR = Factory.New<JobDeclaration>();
			declarationForIOR.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationForIOR.JE_CustomsOffice = "2701";

			using (declarationForIOR.SuspendValidationTesting())
			{
				var importer = Factory.New<OrgHeader>();
				var poaDocument1 = MakePoaDoc(importer);
				MakePoaDocAttr(poaDocument1, "DIRECTION", "EXP");

				var importer2 = Factory.New<OrgHeader>();
				var poaDocument2 = MakePoaDoc(importer2);
				MakePoaDocAttr(poaDocument2, "DIRECTION", "IMP");
				 MakePoaDocAttr(poaDocument2, "PORT OF ENTRY", "1011");

				var importer3 = Factory.New<OrgHeader>();
				var poaDocument3 = MakePoaDoc(importer3);
				MakePoaDocAttr(poaDocument3, "DIRECTION", "IMP");
				MakePoaDocAttr(poaDocument3, "PORT OF ENTRY", "2701");

				var usCompany = Factory.NewWithValidTestData<GlbCompany>();
				usCompany.GC_Code = "~US";
				var importer4 = Factory.New<OrgHeader>();
				var poaDocument4 = MakePoaDoc(importer4);
				MakePoaDocAttr(poaDocument4, "COMPANY CODE", usCompany.GC_Code);

				var messageError = "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for 'IMP' Direction.";
				declarationForIOR.ImporterOfRecordAddress.OrganisationPK = importer.PK;
				AssertHasMessageErrorContaining(declarationForIOR.ImporterOfRecordAddress.OrganisationPKInfo, messageError);

				messageError = "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for '2701' Port of Entry.";
				declarationForIOR.ClearAllNotifications();
				declarationForIOR.ImporterOfRecordAddress.OrganisationPK = importer2.PK;
				AssertHasMessageErrorContaining(declarationForIOR.ImporterOfRecordAddress.OrganisationPKInfo, messageError);

				declarationForIOR.ClearAllNotifications();
				declarationForIOR.ImporterOfRecordAddress.OrganisationPK = importer3.PK;
				AssertNoMessageErrorContaining(declarationForIOR.ImporterOfRecordAddress.OrganisationPKInfo, messageError);

				messageError = "There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for 'EDI' Company.";
				declarationForIOR.ClearAllNotifications();
				declarationForIOR.ImporterOfRecordAddress.OrganisationPK = importer4.PK;
				AssertHasMessageErrorContaining(declarationForIOR.ImporterOfRecordAddress.OrganisationPKInfo, messageError);
			}
		}

		static JobRequiredDocument MakePoaDoc(OrgHeader importer)
		{
			var poaDocument = importer.RequiredDocuments.AddNew("POA");
			poaDocument.EQ_DocType = "POA";
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddYears(-1);
			poaDocument.EQ_ValidToDate = ZDateTime.Today.AddYears(1);
			return poaDocument;
		}

		static JobRequiredDocAttrib MakePoaDocAttr(JobRequiredDocument document, ZString attribName, ZString attribValue)
		{
			var poaDocumentAttrib = document.Attributes.AddNew();
			poaDocumentAttrib.D0_AttribName = attribName;
			poaDocumentAttrib.D0_AttribValue = attribValue;
			return poaDocumentAttrib;
		}
	}
}
