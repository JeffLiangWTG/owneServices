using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaTransportDocumentInfo))]
	sealed class AsycudaTransportDocumentInfoTest : Customs.Business.Testing.CusSupportingInfoTest<AsycudaTransportDocumentInfo>
	{
		public void TestLookups()
		{
			var transportDocument = Factory.New<AsycudaTransportDocumentInfo>();
			AssertType<AsycudaTransportDocumentInfoLookups>(transportDocument.Lookups);
		}

		public void TestCaptions()
		{
			var transportDocument = Factory.New<AsycudaTransportDocumentInfo>();
			CombineAssertions(() =>
			{
				AssertEquals("CSI_CodeUserInterface caption", "Type", DataBoundResourceStrings.GetDataForProperty(transportDocument.CSI_CodeUserInterfaceInfo).Caption);
				AssertEquals("CSI_ReferenceNumberUserInterface caption", "Reference", DataBoundResourceStrings.GetDataForProperty(transportDocument.CSI_ReferenceNumberUserInterfaceInfo).Caption);
			});
		}

		public void TestDefaults()
		{
			var transportDocument = Factory.New<AsycudaTransportDocumentInfo>();
			AssertEquals("Default CSI_SubType should be TRA", "TRA", transportDocument.CSI_SubType);
		}

		public void TestValidation()
		{
			var additionalInfo = Factory.New<AsycudaTransportDocumentInfo>();
			AssertType<AsycudaTransportDocumentInfoValidation>("Validation must of the expected type", additionalInfo.Validation);
		}

		public void TestListFields()
		{
			var propertyInfo = typeof(AsycudaTransportDocumentInfo)
				.GetProperty(nameof(AsycudaTransportDocumentInfo.CSI_CodeUserInterface));

			var result = propertyInfo
				.GetCustomAttributes(typeof(ListAttribute), false)
				.SingleOrDefault() as ListAttribute;

			AssertNotNull(result);
			AssertEquals("Lookups.CodeList", result.ListDataSourceMember);
		}

		public void TestCSI_Status()
		{
			var transportDoc = (AsycudaTransportDocumentInfo)GetNewBusinessObject();
			transportDoc.CSI_Code = TransportDocsTypeList.Codes.IL1;
			transportDoc.CSI_ReferenceNumber = "I025123A01";
			AssertNullOrEmpty("Status should be empty when CSI_Code is IL1 and ReferenceNumber is not empty, because UserInterface fields are not used", transportDoc.CSI_Status);

			CombineAssertions("Update UserInterface ReferenceNumber to a non-empty value", () =>
			{
				transportDoc.CSI_ReferenceNumberUserInterface = "UI025123A01";
				AssertEquals("CSI_ReferenceNumber should be updated from UserInterface", "UI025123A01", transportDoc.CSI_ReferenceNumber);
				AssertEquals("Status should change to OVR", "OVR", transportDoc.CSI_Status);
			});

			CombineAssertions("Update UserInterface Code to a value different from IL1", () =>
			{
				transportDoc.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL2;
				AssertEquals("CSI_Code should be updated from UserInterface", TransportDocsTypeList.Codes.IL2, transportDoc.CSI_Code);
				AssertNullOrEmpty("Status should be empty because the code is not IL1", transportDoc.CSI_Status);
			});

			CombineAssertions("Update UserInterface Code back to IL1", () =>
			{
				transportDoc.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL1;
				AssertEquals("CSI_Code should be updated from UserInterface", TransportDocsTypeList.Codes.IL1, transportDoc.CSI_Code);
				AssertEquals("Status should be OVR because the code is IL1", "OVR", transportDoc.CSI_Status);
			});

			CombineAssertions("Update UserInterface ReferenceNumber to an empty value", () =>
			{
				transportDoc.CSI_ReferenceNumberUserInterface = "";
				AssertEquals("CSI_ReferenceNumber should be updated from UserInterface", "", transportDoc.CSI_ReferenceNumber);
				AssertNullOrEmpty("Status should be empty because ReferenceNumber is empty", transportDoc.CSI_Status);
			});

			CombineAssertions("Update non-UserInterface ReferenceNumber", () =>
			{
				transportDoc.CSI_ReferenceNumber = "I025123A01";
				AssertEquals("UserInterface ReferenceNumber should be updated from non-UserInterface", "I025123A01", transportDoc.CSI_ReferenceNumberUserInterface);
				AssertNullOrEmpty("Status should not change because non-UserInterface field is used", transportDoc.CSI_Status);
			});

			CombineAssertions("Update non-UserInterface Code", () =>
			{
				transportDoc.CSI_Status = "OVR";
				transportDoc.CSI_Code = "IL2";
				AssertEquals("UserInterface Code should be updated from non-UserInterface", "IL2", transportDoc.CSI_CodeUserInterface);
				AssertEquals("Status should not change because non-UserInterface field is used", "OVR", transportDoc.CSI_Status);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.TransportDocuments.AddNew();
		}

		protected override IEnumerable<AsycudaTransportDocumentInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "785";

			var bill = header.Bills.AddNew();
			var transportDocumentsOnBill = bill.TransportDocuments.AddNew();

			Factory.Save();

			yield return transportDocumentsOnBill;
		}

		protected override void SetUp()
		{
			base.SetUp();
			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableAction?.Dispose();
		}

		IDisposable disposableAction;
	}
}
