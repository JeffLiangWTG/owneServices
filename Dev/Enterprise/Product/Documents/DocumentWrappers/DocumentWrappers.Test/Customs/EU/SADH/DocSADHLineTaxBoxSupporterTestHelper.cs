using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Moq;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	public sealed class DocSADHLineTaxBoxSupporterTestHelper
	{
		public static IDocSADHLineTaxBoxSupporter CreateSupporter(ZString type, ZString taxBase, ZString rate, ZString rateDuty, ZString rateOverride, ZString amount, ZString methodOfPayment, string nationalFeeTypeCode = "", string declarationMethodOfPayment = "")
		{
			var mockSupporter = new Mock<IDocSADHLineTaxBoxSupporter>();
			mockSupporter.Setup(m => m.Type).Returns(type);
			mockSupporter.Setup(m => m.TaxBase).Returns(taxBase);
			mockSupporter.Setup(m => m.Rate).Returns(rate);
			mockSupporter.Setup(m => m.RateDuty).Returns(rateDuty);
			mockSupporter.Setup(m => m.RateOverride).Returns(rateOverride);
			mockSupporter.Setup(m => m.AmountInDeclarationCurrency).Returns(amount);
			mockSupporter.Setup(m => m.MethodOfPayment).Returns(methodOfPayment);
			mockSupporter.Setup(m => m.NationalFeeTypeCode).Returns(nationalFeeTypeCode);
			mockSupporter.Setup(m => m.DeclarationMethodOfPayment).Returns(declarationMethodOfPayment);
			return mockSupporter.Object;
		}
	}
}
