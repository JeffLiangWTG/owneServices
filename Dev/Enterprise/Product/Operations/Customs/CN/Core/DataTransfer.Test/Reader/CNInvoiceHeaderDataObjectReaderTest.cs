using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	class CNInvoiceHeaderDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestFillContractNumbers()
		{
			using (Registry.Business.eServices.eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var logger = new TestErrorLogger();
				var helper = new CNDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.China);

				var input = new CommercialInvoiceHeader
				{
					CustomsReferenceCollection = new List<CustomsReference>()
				};
				var @ref = new CustomsReference();
				@ref.Type = new CodeDescriptionPair { Code = "CTR" };
				@ref.Reference = "ContractNo001";
				input.CustomsReferenceCollection.Add(@ref);

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = "BLT";

				var groupHeader = (BaseJobComInvoiceGroupHeader)declaration.JobComInvoiceGroupHeaders.First();
				var output = (JobComInvoiceHeader)new CNInvoiceHeaderDataObjectReader(input, logger, helper, groupHeader).ReadIntoBusinessObject();

				AssertEquals(1, output.ContractNumbers.Count);
				Assert(output.ContractNumbers.Any(num => num.J2_ReferenceNumber == "ContractNo001"));
			}
		}

		public void TestFillInvoiceLineNotesToAddInfo()
		{
			using (Registry.Business.eServices.eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var logger = new TestErrorLogger();
				var helper = new CNDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.China);

				var input = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance).AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()));
				var invoiceLine = new CommercialInvoiceLine();
				input.CommercialInvoiceLineCollection.Add(invoiceLine);
				invoiceLine.AddInfoCollection = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>()
				{
					new UniversalDataBuss.DataObjects.Universal.AddInfo
					{
						Key = Constants.AddInfoKeys.InvoiceLine.CIQIngredient,
						Value = "Ingredient Note 111"
					}
				};

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = "BLT";

				var groupHeader = (BaseJobComInvoiceGroupHeader)declaration.JobComInvoiceGroupHeaders.First();
				var outputInvoice = (JobComInvoiceHeader)new CNInvoiceHeaderDataObjectReader(input, logger, helper, groupHeader).ReadIntoBusinessObject();
				var outputInvoiceLine = (JobComInvoiceLine)outputInvoice.InvoiceLines.First();

				AssertEquals("Ingredient Note 111", outputInvoiceLine.CIQIngredient);
			}
		}
	}
}
