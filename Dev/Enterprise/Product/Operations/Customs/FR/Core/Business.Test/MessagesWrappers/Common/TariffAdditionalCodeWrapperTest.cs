using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class TariffAdditionalCodeWrapperTest : TestCaseWithFactory
	{
		public void TestNationalTariffAdditionalCodeWrapperThrowsArgumentException()
		{
			EU.Business.SupplementaryCode supplementaryCode = null;
			AssertExceptionThrown(typeof(ArgumentNullException), () => { new TariffAdditionalCodeWrapper(supplementaryCode); });

			SupportingDocument dispoPart = null;
			AssertExceptionThrown(typeof(ArgumentNullException), () => { new TariffAdditionalCodeWrapper(dispoPart); });

			AdditionalInfo specMen = null;
			AssertExceptionThrown(typeof(ArgumentNullException), () => { new TariffAdditionalCodeWrapper(specMen); });
		}

		public void TestGenericCodeWrapper()
		{
			var wrapper = new TariffAdditionalCodeWrapper("Z001");

			AssertEquals("Z001", wrapper.Code);
			AssertEquals(ZString.Empty, wrapper.Description);
			AssertEquals(false, wrapper.IsCompleted);
		}

		public void TestEuropeanTariffAdditionalCodeWrapper()
		{
			var additionalInfo = Factory.New<EU.Business.SupplementaryCode>();
			additionalInfo.CY_Code = "B149";
			additionalInfo.CY_Data = "Foshan Nanhai Shengdige Decoration Material Co. Ltd";

			var wrapper = new TariffAdditionalCodeWrapper(additionalInfo);

			AssertEquals(additionalInfo.CY_Code, wrapper.Code);
			AssertEquals(additionalInfo.CY_Data, wrapper.Description);
		}

		public void TestSupportingDocumentAdditionalCodeWrapper()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			var supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "S035";
			supportingDocument.CSI_Description = "Chablis et Petit Chablis";

			var wrapper = new TariffAdditionalCodeWrapper(supportingDocument);

			AssertEquals(supportingDocument.CSI_Code, wrapper.Code);
			AssertEquals(ZString.Empty, wrapper.Description);

			wrapper = new TariffAdditionalCodeWrapper(supportingDocument);
			AssertEquals(supportingDocument.CSI_Code, wrapper.Code);
			AssertEquals(ZString.Empty, wrapper.Description);
		}
	}
}
