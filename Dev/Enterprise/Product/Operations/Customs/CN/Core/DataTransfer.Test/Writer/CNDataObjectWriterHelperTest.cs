using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	class CNDataObjectWriterHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetAdditionalCustomsReferenceDataForCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var invoiceHeader = declaration.Invoices.AddNew();
			var contractNum = invoiceHeader.ContractNumbers.AddNew();
			contractNum.J2_ReferenceNumber = "CONTRACT_NO_01";

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new CNDataObjectWriterHelper(Factory.BOFactory));
			var result = writer.GetDataObject(invoiceHeader);
			AssertEquals(1, result.CustomsReferenceCollection.Count);
			Assert(result.CustomsReferenceCollection.Any(@ref => @ref.Type.Code == new ZString?("CTR") && @ref.Reference == new ZString?("CONTRACT_NO_01")));
		}
	}
}
