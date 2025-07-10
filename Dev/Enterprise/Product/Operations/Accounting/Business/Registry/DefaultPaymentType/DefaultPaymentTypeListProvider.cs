using System;
using CargoWise.Application;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Registry
{
	class DefaultPaymentTypeListProvider : ICodeDescriptionPairListProvider
	{
		public DefaultPaymentTypeListProvider()
		{
		}

		Guid CompanyPK;

		public CodeDescriptionPairList CodeDescriptionPairList
		{
			get
			{
				var defaultValuesShouldContainEPAPaymentType = CompanyPK != Guid.Empty && ObjectFactory.Get<IAccounting>().IsEPaymentFunctionalityEnabledForAnyProvider(CompanyPK);
				var list = new CodeDescriptionPairList(OLookUpEditType.PaymentMethod);
				if (list.ContainsCode(ReceiptTypes.EPayment) && !defaultValuesShouldContainEPAPaymentType)
				{
					list.RemoveCode(ReceiptTypes.EPayment);
				}
				else if (!list.ContainsCode(ReceiptTypes.EPayment) && defaultValuesShouldContainEPAPaymentType)
				{
					list.AddPair(ReceiptTypes.EPayment, ResString.GetMultilingualString("e013cebe-ee88-49c0-93d1-da5e06a1d51a", "E-Payment"));
				}
				return list;
			}
		}

		public void SetCompanyPk(Guid companyPK) => CompanyPK = companyPK;
	}
}
