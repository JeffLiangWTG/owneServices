using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using static Enterprise.Customs.DE.NCTS.Business.Testing.NCTSConditionalFunctionalityTestHelper;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(CommonPreviousDocument))]
	sealed class CommonPreviousDocumentTest : CusSupportingInfoTest<CommonPreviousDocument>
	{
		CommonPreviousDocument previousDocument;

		public void TestValidation_Phase5Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = nctsHeader.Bills.AddNew();
			var arrivalPreviousDocument = bill.PreviousDocuments.AddNew();
			AssertType<CusSupportingInfoDisabledValidation>(arrivalPreviousDocument.Validation);
		}

		public void TestValidation_Departure()
		{
			AssertType<CommonPreviousDocumentValidation>(previousDocument.Validation);
		}

		public void TestCSI_ReferenceNumber_MaxLength()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				AssertEquals(70, previousDocument.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestCSI_ReferenceNumber_MaxLength_DuringTransition()
		{
			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertEquals(35, previousDocument.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestCSI_ReferenceNumber2_MaxLength()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				AssertEquals(35, previousDocument.CSI_ReferenceNumber2Info.MaxLength);
			});
		}

		public void TestCSI_ReferenceNumber2_MaxLength_DuringTransition()
		{
			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertEquals(26, previousDocument.CSI_ReferenceNumber2Info.MaxLength);
			});
		}

		public void TestIsPhase5Departure()
		{
			AssertEquals(true, previousDocument.IsPhase5Departure);
		}

		public void TestIsPhase5Arrival()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = header.Bills.AddNew();
			var arrivalPreviousDocument = bill.PreviousDocuments.AddNew();
			AssertEquals(true, arrivalPreviousDocument.IsPhase5Arrival);
		}

		public void TestCreateOrRemoveAdditionalDocumentInfForN830()
		{
			var bill = (NctsBill)previousDocument.Parent;
			AssertEquals(0, bill.AdditionalDocuments.Count);

			previousDocument.CSI_Code = "N830";
			AssertEquals(1, bill.AdditionalDocuments.Count);
			var infItem = bill.AdditionalDocuments.Cast<NctsBillAdditionalDocument>().Single();
			AssertEquals("20300", infItem.CSI_Code);
			AssertEquals("INF", infItem.CSI_SubType);

			var previousDocument2 = bill.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = "N830";
			AssertEquals(1, bill.AdditionalDocuments.Count);

			previousDocument2.Delete();
			AssertEquals(1, bill.AdditionalDocuments.Count);

			previousDocument.CSI_Code = "N831";
			AssertEquals(0, bill.AdditionalDocuments.Count);

			previousDocument.CSI_Code = "N830";
			previousDocument.Delete();
			AssertEquals(0, bill.AdditionalDocuments.Count);
		}

		protected override IEnumerable<CommonPreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			yield return bill.PreviousDocuments.AddNew();
			yield return nctsHeader.PreviousDocuments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return previousDocument;
		}

		CommonPreviousDocument GetNewPreviousDocument()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			return bill.PreviousDocuments.AddNew();
		}

		protected override void LoadParentIfNeeded(BusinessObjectFactory factory, CommonPreviousDocument bizObj)
		{
			base.LoadParentIfNeeded(factory, bizObj);
			if (bizObj.Parent is NctsHeader _)
			{
				factory.Load<NctsHeader>(bizObj.CSI_ParentID);
			}
			else if (bizObj.Parent is NctsBill _)
			{
				factory.Load<NctsBill>(bizObj.CSI_ParentID);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			previousDocument = GetNewPreviousDocument();
		}
	}
}
