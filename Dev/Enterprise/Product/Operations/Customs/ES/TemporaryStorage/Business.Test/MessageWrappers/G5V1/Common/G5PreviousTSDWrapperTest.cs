using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class G5PreviousTSDWrapperTest : WrapperHelperTest<G5PreviousTSDWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if doc is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "doc"), () => GetWrapper(null));
		}

		public void TestMRN()
		{
			CombineAssertions(() =>
			{
				doc.CSI_ReferenceNumber = "Reference";
				doc.CSI_Code = "337";
				AssertEquals("If CSI_Code is 337 expected filled MRN", "Reference", wrapper.MRN);

				doc.CSI_Code = "SUM";
				AssertEquals("If CSI_Code is not 337 expected empty MRN even is CSI_ReferenceNumber is declared", ZString.Empty, wrapper.MRN);
			});
		}

		public void TestTransportMeans()
		{
			CombineAssertions(() =>
			{
				doc.CSI_ReferenceNumber2 = "Reference2";

				doc.CSI_Code = "SUM";
				AssertNull("If CSI_Code is not in the lists of transport documents expected null TransportMeans", wrapper.TransportMeans);

				doc.CSI_Code = "C624";
				wrapper = GetWrapper(doc);
				var transportMeans = wrapper.TransportMeans;
				AssertNotNull("Expected filled TransportMeans", transportMeans);
				AssertSame("Cached TransportMeans", wrapper.TransportMeans, transportMeans);
				AssertEquals("Expected filled Type", "40", transportMeans.Type);
				AssertEquals("Expected filled Id", "Reference2", transportMeans.Id);
			});
		}

		public void TestTransportDocument()
		{
			CombineAssertions(() =>
			{
				doc.CSI_ReferenceNumber = "Reference";

				doc.CSI_Code = "SUM";
				AssertNull("If CSI_Code is not in the lists of transport documents expected null TransportDocument", wrapper.TransportDocument);

				doc.CSI_Code = "C624";
				wrapper = GetWrapper(doc);
				var transportDocument = wrapper.TransportDocument;
				AssertNotNull("Expected filled TransportDocument", transportDocument);
				AssertSame("Cached TransportDocument", wrapper.TransportDocument, transportDocument);
				AssertEquals("Expected filled Name", "C624", transportDocument.Name);
				AssertEquals("Expected filled Number", "Reference", transportDocument.Number);
			});
		}

		public void TestGoodsItemId()
		{
			CombineAssertions(() =>
			{
				doc.CSI_LineNo = 5;
				doc.CSI_Code = "337";
				AssertEquals("If CSI_Code is 337 expected filled GoodsItemId", "5", wrapper.GoodsItemId);

				doc.CSI_LineNo = 0;
				AssertEquals("If CSI_Code is 337 expected empty GoodsItemId when 0", ZString.Empty, wrapper.GoodsItemId);

				doc.CSI_LineNo = 5;
				doc.CSI_Code = "SUM";
				AssertEquals("If CSI_Code is not 337 expected empty GoodsItemId even is CSI_LineNo is not 0", ZString.Empty, wrapper.GoodsItemId);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			doc = Factory.New<TemporaryStoragePreviousDocument>();
			wrapper = GetWrapper(doc);
		}

		TemporaryStoragePreviousDocument doc;
		G5PreviousTSDWrapper wrapper;

		G5PreviousTSDWrapper GetWrapper(TemporaryStoragePreviousDocument doc) => new G5PreviousTSDWrapper(doc);

		protected override G5PreviousTSDWrapper GetProvider() => wrapper;
	}
}
