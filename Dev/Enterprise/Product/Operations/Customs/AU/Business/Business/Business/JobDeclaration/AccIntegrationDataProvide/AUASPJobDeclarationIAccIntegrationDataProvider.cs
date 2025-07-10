using System;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUASPJobDeclarationIAccIntegrationDataProvider : JobDeclarationIAccIntegrationDataProvider, IAccIntegrationDataProvider
	{
		public AUASPJobDeclarationIAccIntegrationDataProvider(JobDeclaration declaration, ZGuid cusEntryHeaderPK, bool useSeperateFactory = true) : base(declaration, useSeperateFactory)
		{
			entryHeaderPK = cusEntryHeaderPK;
		}

		readonly ZGuid entryHeaderPK;

		ZGuid[] IAccIntegrationDataProvider.DisbursementChargeCodes => new[] { AUASPEntryChargeTypeList.GetChargeCodePKForASP(declarationInBillingFactory.CompanyPK.ToGuid()) };

		IAccInvoiceDataProvider[] IAccIntegrationDataProvider.InvDataProviders
		{
			get
			{
				if (fInvoiceDataProvider == null)
				{
					var entryInBillingFactory = declarationInBillingFactory.CustomsEntryHeaders.FindByPK(entryHeaderPK) as CusEntryHeader;

					fInvoiceDataProvider = entryInBillingFactory != null
						? new[] { new AUASPEntryHeaderIAccInvoiceDataProvider(entryInBillingFactory) }
						: Array.Empty<IAccInvoiceDataProvider>();
				}
				return fInvoiceDataProvider;
			}
		}
		IAccInvoiceDataProvider[] fInvoiceDataProvider;
	}
}
