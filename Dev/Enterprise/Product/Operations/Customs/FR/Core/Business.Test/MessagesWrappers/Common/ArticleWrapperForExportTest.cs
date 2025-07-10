using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class ArticleWrapperForExportTest : TestCaseWithFactory
	{
		public void TestExportPropertiesExist()
		{
			AssertEquals((sbyte)1, articleFrWrapper.Seals);
			AssertEquals("SCELLE1", articleFrWrapper.SealIds.FirstOrDefault());
			AssertEquals(2, articleFrWrapper.DangerousGoodsDeltas.Count());
			AssertEquals("Dangerous Goods Delta should be equal to UNNO number without the Variant", "0004", articleFrWrapper.DangerousGoodsDeltas.FirstOrDefault());
			AssertEquals("Dangerous Goods Delta should be equal to UNNO number without the Variant", "0014", articleFrWrapper.DangerousGoodsDeltas.ToList()[1]);

			AssertEquals(Customs.Business.TransportChargesModeOfPayment.Codes.CreditCard, articleFrWrapper.TransportMethodPayment);

			AssertEquals(ZString.Empty, articleFrWrapper.PACCode);
			AssertEquals(ZDecimal.Zero, articleFrWrapper.Restitution);
			AssertEquals(ZString.Empty, articleFrWrapper.ExportCertification1);
			AssertEquals(ZString.Empty, articleFrWrapper.ExportCertification2);
			AssertEquals(ZString.Empty, articleFrWrapper.RestitutionMention);
			AssertEquals(ZString.Empty, articleFrWrapper.Recipient);
			AssertEquals(ZString.Empty, articleFrWrapper.InvariantMention);
			AssertEquals(ZString.Empty, articleFrWrapper.VariantMention);
			AssertEquals(ZDecimal.Zero, articleFrWrapper.VariantRate);
			AssertEquals(ZDecimal.Zero, articleFrWrapper.LoadingStartDate);
			AssertEquals(ZDecimal.Zero, articleFrWrapper.LoadingStartHour);
			AssertEquals(ZDecimal.Zero, articleFrWrapper.LoadingEndDate);
			AssertEquals(ZDecimal.Zero, articleFrWrapper.LoadingEndHour);
		}

		protected override void SetUp()
		{
			var declaration = ArticleWrapperTest.CreateDeclaration(false);
			var cusEntryHeader = declaration.CustomsEntryHeaders[0];
			var errorCollector = new EU.Business.ErrorCollector();
			var entriesLine = cusEntryHeader.MergedLines.Cast<Declaration.CusEntryLine>();
			var cusFrEntryLine = entriesLine.FirstOrDefault(a => a.CountryOfOriginCode == Core.Constants.CountryCodes.France);
			articleFrWrapper = new ArticleWrapper(cusEntryHeader, cusFrEntryLine);
		}
		ArticleWrapper articleFrWrapper;
	}
}
