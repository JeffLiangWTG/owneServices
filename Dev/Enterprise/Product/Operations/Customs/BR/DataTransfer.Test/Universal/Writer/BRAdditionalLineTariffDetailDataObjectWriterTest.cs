using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.BR.DataTransfer.Universal.Testing
{
	sealed class BRAdditionalLineTariffDetailDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestBRAdditionalLineTariffDetailDataObjectWriter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "09022000";

			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Tariff = "09022000_001";
			tariffDetail.BZ_Type = ChildTariffTypeList.Codes.LEBIT;

			var writer = new BRAdditionalLineTariffDetailDataObjectWriter(new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>())));
			var result = writer.GetDataObject(tariffDetail);

			CombineAssertions(() =>
			{
				AssertEquals(ChildTariffTypeList.Codes.LEBIT, result.Type.Code.Value);
				AssertEquals("001", result.Tariff.Value);
			});

			tariffDetail.BZ_Tariff = "";
			tariffDetail.BZ_Type = ChildTariffTypeList.Codes.LEBIT;

			writer = new BRAdditionalLineTariffDetailDataObjectWriter(new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>())));
			result = writer.GetDataObject(tariffDetail);

			CombineAssertions(() =>
			{
				AssertEquals(ChildTariffTypeList.Codes.LEBIT, result.Type.Code.Value);
				AssertEquals("", result.Tariff.Value);
			});
		}
	}
}
