using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusLineTariffDetailDeepCloneStrategy))]
sealed class CusLineTariffDetailDeepCloneStrategyTest : TestCaseWithFactory
{
	public void TestClone() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var detail = invoiceLine.CusLineTariffDetails.AddNew();
		detail.BZ_Type = "XXX";
		detail.BZ_Tariff = "123";
		detail.BZ_Qty1 = 10m;
		detail.BZ_UQ1 = "Q1";
		detail.BZ_Qty2 = 20m;
		detail.BZ_UQ2 = "Q2";
		Factory.Save();

		var clonedDetail = (Customs.Business.CusLineTariffDetail)new CusLineTariffDetailDeepCloneStrategy(detail, CloneType.TemplateCopy).Clone();
		AssertNotEquals("PK", detail.PK, clonedDetail.PK);
		AssertNotEquals("BZ_ParentID", ZGuid.Empty, clonedDetail.PK);
		AssertEquals("BZ_ParentID", ZGuid.Empty, clonedDetail.BZ_ParentID);
		AssertEquals("BZ_ParentTableCode", ZString.Empty, clonedDetail.BZ_ParentTableCode);
		AssertEquals("BZ_Type", detail.BZ_Type, clonedDetail.BZ_Type);
		AssertEquals("BZ_Tariff", detail.BZ_Tariff, clonedDetail.BZ_Tariff);
		AssertEquals("BZ_Qty1", detail.BZ_Qty1, clonedDetail.BZ_Qty1);
		AssertEquals("BZ_UQ1", detail.BZ_UQ1, clonedDetail.BZ_UQ1);
		AssertEquals("BZ_Qty2", detail.BZ_Qty2, clonedDetail.BZ_Qty2);
		AssertEquals("BZ_UQ2", detail.BZ_UQ2, clonedDetail.BZ_UQ2);
	});
}
