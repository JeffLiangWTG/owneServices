using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ExportAddInfoJobComInvoiceLineValidationTest : AddInfoJobComInvoiceLineValidationTest
	{
		public void TestCheckCa_ConveyanceIdentificationNumber()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory, false);
			invoiceLine.JI_Tariff = helper.ExportTariff84289020.ZZ1_TariffCode;
			invoiceLine.CA_ConveyanceIdentificationNumber = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.CA_ConveyanceIdentificationNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Tariff = helper.ExportTariff87032330.ZZ1_TariffCode;
			invoiceLine.CA_ConveyanceIdentificationNumber = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.CA_ConveyanceIdentificationNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}
}
