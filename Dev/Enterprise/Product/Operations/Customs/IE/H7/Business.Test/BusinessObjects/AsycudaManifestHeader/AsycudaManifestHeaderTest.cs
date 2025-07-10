using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : EU.H7.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestAMA_AgentTypeCaption()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var resourceStringDataAttribute = header.AMA_AgentTypeInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Rep. Status", resourceStringDataAttribute.Caption);
		}

		public void TestAMA_PaymentMethodCaption()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var resourceStringDataAttribute = header.AMA_PaymentMethodInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Payment Method", resourceStringDataAttribute.Caption);
		}

		public void TestAMA_PaymentAccountNumberCaption()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var resourceStringDataAttribute = header.AMA_PaymentAccountNumberInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Account Number", resourceStringDataAttribute.Caption);
		}

		public void TestAMA_ApplicationCode()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var propertyInfo = header.AMA_ApplicationCodeInfo;
			var resourceStringDataAttribute = propertyInfo.GetAttribute<ResourceStringDataAttribute>();

			CombineAssertions(() =>
			{
				AssertEquals("Submit Type", resourceStringDataAttribute.Caption);
				AssertEquals("Default value", "LV1", header.AMA_ApplicationCode);
			});

			header.AMA_ApplicationCode = "aa";
			AssertEquals("When an invalid submitType is entered, display the last invalid submitType value.", "LV1", header.AMA_ApplicationCode);

			header.AMA_ApplicationCode = "LV2";
			AssertEquals("When a valid submitType is entered, set the submitType to the new value.", "LV2", header.AMA_ApplicationCode);
		}

		public void TestMarkBillsAsNeedingValidation_WhenAMA_ApplicationCodeChange()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.Bills.AddNew();
			header.Bills.AddNew();

			var markedAsNeedingValidationCount = 0;
			Factory.MarkedAsNeedingValidation += (obj) =>
			{
				if (obj is AsycudaBill)
				{
					markedAsNeedingValidationCount++;
				}
			};

			header.AMA_ApplicationCode = "LV2";

			CombineAssertions(() =>
			{
				AssertEquals($"Setting AMA_ApplicationCode should mark bills as needing Validation(including master bill)", header.Bills.Count + 1, markedAsNeedingValidationCount);
			});
		}

		public void TestBills()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertType<EU.H7.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>>(header.Bills);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			CombineAssertions(() =>
			{
				AssertEquals(typeof(PreviousDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)header).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.PreviousDocument]);
				AssertEquals(typeof(SupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)header).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.SupportingDocument]);
			});
		}

		public void TestSupportingDocuments()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertType<EU.H7.Business.SupportingDocumentCollection<SupportingDocument>>(header.SupportingDocuments);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<AsycudaManifestHeader>();

		public void TestPreviousDocuments()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertType<EU.H7.Business.PreviousDocumentCollection<PreviousDocument>>(header.PreviousDocuments);
		}

		public void TestIControllerIDProviderMemers()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var controllerIdProvider = header as IControllerIDProvider;

			CombineAssertions("IControllerIDProvider Memers", () =>
			{
				AssertEquals("ControllerId", ControllerIDs.Customs.EU.EUH7, controllerIdProvider.ControllerID);
				AssertEquals("BusinessObjectPK", header.PK, controllerIdProvider.BusinessObjectPK);
			});
		}

		public void TestDataGrouping()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_ApplicationCode = "LV2";
			AssertEquals("IE", header.DataGrouping);

			header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_ApplicationCode = "LV1";
			AssertEquals("IE5", header.DataGrouping);
		}
	}
}
