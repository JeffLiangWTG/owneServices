using System;
using CargoWise.Application;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	internal class APPaymentMethodListProvider : ICodeDescriptionPairListProvider
	{
		public APPaymentMethodListProvider()
		{
		}

		Guid CompanyPK;

		public CodeDescriptionPairList CodeDescriptionPairList
		{
			get
			{
				var paymentMethodsToUse = GetAPPaymentMethods();
				if (CompanyPK == Guid.Empty || !ObjectFactory.Get<IAccounting>().IsEPaymentFunctionalityEnabledForOFX(CompanyPK))
				{
					paymentMethodsToUse.RemoveCode(EPaymentMethods.EPaymentViaOFX);
				}
				return paymentMethodsToUse;
			}
		}

		public void SetCompanyPk(Guid companyPK) => CompanyPK = companyPK;

		internal static CodeDescriptionPairList GetAPPaymentMethods()
		{
			var apPaymentMethods = new CodeDescriptionPairList();
			apPaymentMethods.AddPair(ReceiptTypes.Cheque, ResString.GetMultilingualString("c5e10f9e-f906-4144-88a7-16824acd96b5", "Check"));
			apPaymentMethods.AddPair(ReceiptTypes.DirectDebit, ResString.GetMultilingualString("c2fff59f-829c-40c5-99d9-28bf419fc41a", "Direct Debit"));
			apPaymentMethods.AddPair(ReceiptTypes.eNettDirectDebit, ResString.GetMultilingualString("a25682e2-614f-4ffe-94e6-7f15b024ad22", "ComPay Direct Debit"));
			apPaymentMethods.AddPair(EPaymentMethods.EPaymentViaOFX, ResString.GetMultilingualString("4d07bd49-bc92-4034-bb5d-29b9171b0f3c", "E-Payment via OFX"));
			return apPaymentMethods;
		}
	}
}
