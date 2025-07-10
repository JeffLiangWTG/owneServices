using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentTransportContractDocumentWrapperTest : DataProviderTestCase<DeclarationConsignmentTransportContractDocumentWrapper>
	{
		public void TestDeconsolidator()
		{
			AssertNotNull("Deconsolidator", Provider.Deconsolidator);
			AssertEquals("Deconsolidator should have 0 entries", 0, Provider.Deconsolidator.Count);
		}

		public void TestId()
		{
			AssertNotNull("ID", Provider.Id);
			AssertEquals("ID should be equal to the expected value", "REF1", Provider.Id.Value);
		}

		public void TestIssueLocation()
		{
			AssertNull("IssueLocation", Provider.IssueLocation);
		}

		public void TestTypeCode()
		{
			AssertNotNull("TypeCode", Provider.TypeCode);
			AssertEquals("TypeCode should be equal to the expected value", "XY1", Provider.TypeCode.Value);
		}

		public void TestConditionCode()
		{
			AssertNotNull("ConditionCode", Provider.ConditionCode);
			AssertEquals("ConditionCode should be equal to the expected value", "27", Provider.ConditionCode.Value);

			var provider2 = DeclarationConsignmentTransportContractDocumentWrapper.NewOrNull(asycudaTransportDocumentInfo2, 2);
			AssertNull("ConditionCode", provider2.ConditionCode);
		}

		public void TestNewOrNull()
		{
			AssertNull("When asycudaTransportDocumentInfo is null", DeclarationConsignmentTransportContractDocumentWrapper.NewOrNull(null));
			AssertNotNull("When asycudaTransportDocumentInfo is not null", DeclarationConsignmentTransportContractDocumentWrapper.NewOrNull(Factory.New<AsycudaTransportDocumentInfo>()));
		}

		protected override DeclarationConsignmentTransportContractDocumentWrapper GetProvider()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_Condition = "27";
			asycudaTransportDocumentInfo1 = bill.TransportDocuments.AddNew();
			asycudaTransportDocumentInfo1.CSI_Code = "XY1";
			asycudaTransportDocumentInfo1.CSI_ReferenceNumber = "REF1";

			asycudaTransportDocumentInfo2 = bill.TransportDocuments.AddNew();
			asycudaTransportDocumentInfo2.CSI_Code = "XYZ2";
			asycudaTransportDocumentInfo2.CSI_ReferenceNumber = "REF2";

			return DeclarationConsignmentTransportContractDocumentWrapper.NewOrNull(asycudaTransportDocumentInfo1);
		}

		AsycudaTransportDocumentInfo asycudaTransportDocumentInfo1;
		AsycudaTransportDocumentInfo asycudaTransportDocumentInfo2;
	}
}
