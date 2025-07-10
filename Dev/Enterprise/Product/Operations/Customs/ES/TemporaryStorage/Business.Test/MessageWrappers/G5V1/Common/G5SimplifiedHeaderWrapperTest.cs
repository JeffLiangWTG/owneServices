using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class G5SimplifiedHeaderWrapperTest : WrapperHelperTest<G5SimplifiedHeaderWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if header is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "tempHeader"), () => GetWrapper(null));
		}

		public void TestLRN()
		{
			header.LRN = "2487654321A00000000002";
			AssertEquals("Expected filled LRN", "2487654321A00000000002", wrapper.LRN);
		}

		public void TestNullDeclarant()
		{
			header.Declarant.OA_OH = ZGuid.Empty;
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Declarant.ToString());
		}

		public void TestDeclarant()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.Addresses.AddNew();
				header.Declarant.OA_OH = orgHeader.PK;

				var declarant = wrapper.Declarant;
				AssertNotNull("Expected filled Declarant", declarant);
				AssertSame("Cached Declarant", wrapper.Declarant, declarant);
			});
		}

		public void TestNullRepresentative()
		{
			header.AMA_OA_Representative = ZGuid.Empty;
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Representative.ToString());
		}

		public void TestRepresentative()
		{
			CombineAssertions(() =>
			{
				var orgAddress = Factory.New<OrgAddress>();
				var orgHeader = Factory.New<OrgHeader>();
				orgAddress.OA_OH = orgHeader.PK;
				header.AMA_OA_Representative = orgAddress.PK;

				var representative = wrapper.Representative;
				AssertNotNull("Expected filled Representative", representative);
				AssertSame("Cached Representative", wrapper.Representative, representative);
			});
		}

		public void TestAdditionalInfos()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty AdditionalInfos list", 0, wrapper.AdditionalInfos.Count);

				var addInfo1 = bill.AdditionalInfos.AddNew();
				addInfo1.CSI_Code = "9001";
				addInfo1.CSI_SubType = "INF";
				addInfo1.CSI_ReferenceNumber = "REF1";
				addInfo1.CSI_Description = "DESC1";

				var addInfo2 = bill.AdditionalInfos.AddNew();
				addInfo2.CSI_Code = "9002";
				addInfo2.CSI_SubType = "INF";
				addInfo2.CSI_ReferenceNumber = "REF2";
				addInfo2.CSI_Description = "DESC2";

				var addInfo3 = bill.AdditionalInfos.AddNew();
				addInfo3.CSI_Code = "9003";
				addInfo3.CSI_SubType = "TRA";
				addInfo3.CSI_ReferenceNumber = "REF3";
				addInfo3.CSI_Description = "DESC3";

				var addInfo4 = bill.AdditionalInfos.AddNew();
				addInfo4.CSI_Code = "9004";
				addInfo4.CSI_SubType = "REF";
				addInfo4.CSI_ReferenceNumber = "REF4";
				addInfo4.CSI_Description = "DESC4";

				var item = bill.PackedItems.AddNew();
				var addInfo5 = item.AdditionalInfos.AddNew();
				addInfo5.CSI_Code = "9005";
				addInfo5.CSI_SubType = "INF";
				addInfo5.CSI_ReferenceNumber = "REF5";
				addInfo5.CSI_Description = "DESC5";

				wrapper = GetWrapper(header);
				var documents = wrapper.AdditionalInfos;
				AssertEquals("Expected filled AdditionalInfos (only included those in bill and SubType INF)", 2, documents.Count);
				AssertSame("Cached AdditionalInfos", wrapper.AdditionalInfos, documents);
				AssertContainsExactElementsInAnyOrder("Expected docs with correct Name and Number",
					new (ZString, ZString)[]
					{
						("9001", "DESC1"),
						("9002", "DESC2")
					}, documents.Select(x => (x.Name, x.Number)).ToArray());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<TemporaryStorageHeader>();
			bill = header.Bills.FirstOrDefault() ?? header.Bills.AddNew();
			wrapper = GetWrapper(header);
		}

		TemporaryStorageHeader header;
		TemporaryStorageBill bill;
		G5SimplifiedHeaderWrapper wrapper;

		G5SimplifiedHeaderWrapper GetWrapper(TemporaryStorageHeader header) => new G5SimplifiedHeaderWrapper(header);

		protected override G5SimplifiedHeaderWrapper GetProvider() => wrapper;
	}
}
