using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Business.Message.MessagesWrappers.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentConsignmentTransportContractDocumentWrapperTest : DataProviderTestCase<DeclarationGoodsShipmentConsignmentTransportContractDocumentWrapper>
	{
		public void TestIssueDateTime()
		{
			AssertNull("IssueDateTime", Provider.IssueDateTime);
			jobDeclaration.JE_MasterBillIssuedDate = new DateTime(2024, 06, 30, 10, 12, 0, DateTimeKind.Utc);
			AssertEquals("IssueDateTime should be equal to the expected value", "2024-06-30T10:12:00", Provider.IssueDateTime);
		}

		public void TestTypeCode()
		{
			AssertNull("TypeCode", Provider.TypeCode);

			jobDeclaration.JE_TransportMode = TransportModes.Air;
			jobDeclaration.JE_TransportMeans = "1"; // jobDeclaration.JE_TransportMeans Should be automatically initiated according to transport modes in uncommited WI00544602 
			AssertEquals("TypeCode should be equal to '1'", "1", Provider.TypeCode.Value);

			jobDeclaration.JE_TransportMode = TransportModes.Sea;
			jobDeclaration.JE_TransportMeans = "11";
			AssertEquals("TypeCode should be equal to '11'", "11", Provider.TypeCode.Value);

			jobDeclaration.JE_TransportMode = TransportModes.Road;
			jobDeclaration.JE_TransportMeans = "20";
			AssertEquals("TypeCode should be equal to '20'", "20", Provider.TypeCode.Value);
		}

		public void TestID()
		{
			AssertNull("ID", Provider.ID);

			jobDeclaration.JE_TransportMode = TransportModes.Air;
			jobDeclaration.JE_DateAtFinalDestination = new DateTime(2024, 06, 30, 10, 12, 0, DateTimeKind.Utc);
			AssertEquals("ID should be equal to JE_DateAtFinalDestination year part", "2024", Provider.ID.Value);

			jobDeclaration.JE_ExportDate = new DateTime(2023, 06, 30, 10, 12, 0, DateTimeKind.Utc);
			AssertEquals("ID should be equal to JE_ExportDate year part", "2023", Provider.ID.Value);

			jobDeclaration.JE_DateAtOrigin = new DateTime(2022, 06, 30, 10, 12, 0, DateTimeKind.Utc);
			AssertEquals("ID should be equal to JE_DateAtOrigin year part", "2022", Provider.ID.Value);

			jobDeclaration.JE_TransportMode = TransportModes.Sea;
			jobDeclaration.JE_ManifestNumber = "1234567890";
			AssertEquals("ID should be equal to JE_ManifestNumber", "1234567890", Provider.ID.Value);

			jobDeclaration.JE_TransportMode = TransportModes.Road;
			AssertEquals("ID should be equal to JE_ManifestNumber", "1234567890", Provider.ID.Value);
		}

		public void TestDmExtensions()
		{
			AssertNotNull("DmExtensions", Provider.DmExtensions);
			AssertType<DeclarationGoodsShipmentConsignmentTransportContractDocumentDMExtensionsWrapper>(Provider.DmExtensions);
		}

		public void TestNewOrNull()
		{
			AssertNotNull("Provider", Provider);
			AssertNull("Provider", DeclarationGoodsShipmentConsignmentTransportContractDocumentWrapper.NewOrNull(null));
		}

		protected override DeclarationGoodsShipmentConsignmentTransportContractDocumentWrapper GetProvider()
		{
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_GoodsOrigin = "IL";

			return DeclarationGoodsShipmentConsignmentTransportContractDocumentWrapper.NewOrNull(jobDeclaration);
		}

		JobDeclaration jobDeclaration;
	}
}
