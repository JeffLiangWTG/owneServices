using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5DepartureDocumentRequestTest : CommonDocumentRequestTest<NCTS5DepartureDocumentRequest, NctsHeader>
	{
		public void TestConstructorMRNCode()
		{
			CreateOrUpdateCusEntryNumber(businessObject, CusEntryNumberTypes.Standard.MovementReferenceNumber, ZString.Empty, "4", new ZDateTime(2021, 8, 1, 11, 0, 0), ZDateTime.Empty);
			AssertExceptionThrown<ArgumentException>("mrnCode null", () => GetDocumentRequestClass(businessObject, "A"));
		}

		public void TestRequestMissingDocuments_DAT_WithBroker()
		{
			businessObject.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;

			CombineAssertions(() =>
			{
				AssertEquals(0, businessObject.Messages.Count);

				var documentRequestMessage = RequestMissingDocument();
				AssertEquals("NCTS Departure Document Capture Request can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

				var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
				var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_NCTS_AEAT_AAA.pdf", "AAA");
				docManagerInfo.Save();

				CreateOrUpdateCusEntryNumber(businessObject, CusEntryNumberTypes.Spain.ClearanceCSV, csvCode, ZString.Empty, new ZDateTime(2021, 8, 1, 11, 0, 5), ZDateTime.Empty);
				businessObject.Factory.Save();

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Message created successfully", 1, documentRequestMessage);

				AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_NCTS_AEAT_DAT.pdf", csvCode, certificate.CertificatePK);

				AssertNoDuplicatedDocumentRequests(mrnCode + "_NCTS_AEAT_DAT.pdf", csvCode, certificate.CertificatePK);
			});
		}

		public void TestRequestMissingDocuments_DAT_WithoutBroker()
		{
			CombineAssertions(() =>
			{
				AssertEquals(0, businessObject.Messages.Count);

				var documentRequestMessage = RequestMissingDocument();
				AssertEquals("NCTS Departure Document Capture Request can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

				var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
				var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_NCTS_AEAT_AAA.pdf", "AAA");
				docManagerInfo.Save();

				CreateOrUpdateCusEntryNumber(businessObject, CusEntryNumberTypes.Spain.ClearanceCSV, csvCode, ZString.Empty, new ZDateTime(2021, 8, 1, 11, 0, 5), ZDateTime.Empty);
				businessObject.Factory.Save();

				documentRequestMessage = RequestMissingDocument();
				AssertEquals("Message created successfully", 1, documentRequestMessage);

				AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_NCTS_AEAT_DAT.pdf", csvCode, ZGuid.Empty);

				AssertNoDuplicatedDocumentRequests(mrnCode + "_NCTS_AEAT_DAT.pdf", csvCode, ZGuid.Empty);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			businessObject = Factory.New<NctsHeader>();
			businessObject.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			businessObject.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			businessObject.TrainingEntry = true;
			CreateOrUpdateCusEntryNumber(businessObject, CusEntryNumberTypes.Standard.MovementReferenceNumber, mrnCode, "4", new ZDateTime(2021, 8, 1, 11, 0, 0), ZDateTime.Empty);
			businessObject.Factory.Save();
		}

		const string csvCode = "ABCDEFGHIJKLMNOP";

		protected override NCTS5DepartureDocumentRequest GetDocumentRequestClass(NctsHeader businessObject, ZString certName) => new NCTS5DepartureDocumentRequest(businessObject, certName);

		protected override EDIMessage GetLastMessage(NctsHeader businessObject)
		{
			businessObject.Messages.Reload(true);
			return businessObject.Messages.LastMessage;
		}

		protected override ZBool IsTrain() => true;
	}
}
