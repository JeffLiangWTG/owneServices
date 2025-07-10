using System;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.BR.DataTransfer.Universal.Testing
{
	sealed class BRAdditionalLineTariffDetailDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestReadFullTariffCode()
		{
			var logger = new TestErrorLogger();
			var currentCompanyHelper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Brazil);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_ApplicationCode = "BLT";
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "09022000";

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertReadFullTariffCode();
			}
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertReadFullTariffCode();
			}

			void AssertReadFullTariffCode() => CombineAssertions(() =>
			{
				var input = new AdditionalLineTariffDetail();
				input.Type = new CodeDescriptionPair5Char() { Code = ChildTariffTypeList.Codes.LEBIT };
				input.Tariff = "001";
				var reader = new BRAdditionalLineTariffDetailDataObjectReader(input, logger, Factory, currentCompanyHelper, invoiceLine);
				var tariffDetail = reader.ReadIntoBusinessObject() as CusLineTariffDetail;

				AssertEquals(ChildTariffTypeList.Codes.LEBIT, tariffDetail.BZ_Type);
				AssertEquals("09022000_001", tariffDetail.BZ_Tariff);
				AssertEquals("001", tariffDetail.ExNumber);

				input.Tariff = "";
				reader = new BRAdditionalLineTariffDetailDataObjectReader(input, logger, Factory, currentCompanyHelper, invoiceLine);
				tariffDetail = reader.ReadIntoBusinessObject() as CusLineTariffDetail;

				AssertEquals(ChildTariffTypeList.Codes.LEBIT, tariffDetail.BZ_Type);
				AssertEquals("", tariffDetail.BZ_Tariff);
				AssertEquals("", tariffDetail.ExNumber);
			});
		}
	}
}

