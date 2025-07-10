using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class AnnexNCTS5SendMessageWrapperTest : WrapperHelperTest<AnnexNCTS5SendMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if docPivots is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "docPivots"), () => new AnnexNCTS5SendMessageWrapper(nctsHeader, Certificate, null, "Y"));

				AssertExceptionThrown("Constructor Throws Exception if docPivots is empty", typeof(ArgumentOutOfRangeException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value '0' cannot be less than or equal to 0.", "docPivots"), () => new AnnexNCTS5SendMessageWrapper(nctsHeader, Certificate, new List<NctsCusStorageDocPivot>(), "Y"));

				AssertExceptionThrown("Constructor Throws Exception if requestDispatch is empty", typeof(ArgumentException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be empty string (\"\").", "requestDispatch"), () => new AnnexNCTS5SendMessageWrapper(nctsHeader, Certificate, docPivotList, ZString.Empty));
			});
		}

		public void TestTransitOperation()
		{
			var transitOperation = wrapper.TransitOperation;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled TransitOperation", transitOperation);
				AssertSame("Cached TransitOperation", wrapper.TransitOperation, transitOperation);
			});
		}

		public void TestDispatchRequestCode()
		{
			AssertEquals("Expected filled DispatchRequestCode", "S", wrapper.DispatchRequestCode);
		}

		public void TestDocuments()
		{
			var documents = wrapper.Documents;
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled Documents", 1, documents.Count);
				AssertSame("Cached Documents", wrapper.Documents, documents);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var eDoc = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var pivot = nctsHeader.EDocPivotCollection.AddNew();
			pivot.CSD_StorageDocReference = eDoc.UniqueKey;

			docPivotList = new List<NctsCusStorageDocPivot>() { pivot };

			wrapper = GetWrapper(nctsHeader);
		}

		NctsHeader nctsHeader;
		List<NctsCusStorageDocPivot> docPivotList;
		AnnexNCTS5SendMessageWrapper wrapper;

		AnnexNCTS5SendMessageWrapper GetWrapper(NctsHeader nctsHeader) => new AnnexNCTS5SendMessageWrapper(nctsHeader, Certificate, docPivotList, "Y");

		protected override AnnexNCTS5SendMessageWrapper GetProvider() => wrapper;
	}
}
