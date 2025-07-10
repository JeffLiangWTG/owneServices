using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using static Enterprise.Customs.DE.NCTS.Business.Testing.NCTSConditionalFunctionalityTestHelper;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsBillAdditionalDocument))]
	sealed class NctsBillAdditionalDocumentTest : CusSupportingInfoTest<NctsBillAdditionalDocument>
	{
		public void TestCSI_ReferenceNumber_MaxLength()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals(70, additionalDocument.CSI_ReferenceNumberInfo.MaxLength);
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				AssertEquals(70, additionalDocument.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestCSI_ReferenceNumber_MaxLength_DuringTransition()
		{
			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals(35, additionalDocument.CSI_ReferenceNumberInfo.MaxLength);
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				AssertEquals(35, additionalDocument.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestLookups()
		{
			AssertType<NctsBillAdditionalDocumentLookups>(additionalDocument.Lookups);
		}

		public void TestGetNewValidation_Phase5Arrival()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = header.Bills.AddNew();
			var arrivalAdditionalDocument = bill.AdditionalDocuments.AddNew();

			AssertType<CusSupportingInfoDisabledValidation>(arrivalAdditionalDocument.Validation);
		}

		public void TestGetNewValidation_Departure()
		{
			AssertType<NctsBillAdditionalDocumentValidation>(additionalDocument.Validation);
		}

		public void TestReadOnlyAdditionalDocuments_Arrival()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = header.Bills.AddNew();

			var additionalDocumentCollection = bill.AdditionalDocuments;
			var additionalDocument = additionalDocumentCollection.AddNew();

			CombineAssertions("ReadOnly for Arrival", () =>
			{
				AssertEquals(true, additionalDocumentCollection.ReadOnly);
				AssertEquals(true, additionalDocument.ReadOnly);
			});
		}

		public void TestReadOnlyAdditionalDocuments_Departure()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();

			var additionalDocumentCollection = bill.AdditionalDocuments;
			var additionalDocument = additionalDocumentCollection.AddNew();

			CombineAssertions("Not ReadOnly for Departure", () =>
			{
				AssertEquals(false, additionalDocumentCollection.ReadOnly);
				AssertEquals(false, additionalDocument.ReadOnly);
			});
		}

		protected override IEnumerable<NctsBillAdditionalDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			yield return bill.AdditionalDocuments.AddNew();
		}

		protected override void LoadParentIfNeeded(BusinessObjectFactory factory, NctsBillAdditionalDocument bizObj)
		{
			base.LoadParentIfNeeded(factory, bizObj);
			factory.Load<NctsBill>(bizObj.CSI_ParentID);
		}

		protected override BusinessObject GetNewBusinessObject() => additionalDocument;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			additionalDocument = bill.AdditionalDocuments.AddNew();
		}
		NctsHeader nctsHeader;
		NctsBillAdditionalDocument additionalDocument;
	}
}
