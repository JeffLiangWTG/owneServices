using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUDSBJobDeclarationIAccIntegrationDataProvider : JobDeclarationIAccIntegrationDataProvider, IAccIntegrationDataProvider
	{
		public AUDSBJobDeclarationIAccIntegrationDataProvider(JobDeclaration declaration, bool useSeperateFactory = true) : base(declaration, useSeperateFactory)
		{
		}

		bool IsQuarantineChargeRatingSeparated => (declarationInBillingFactory as JobDeclaration)?.IsQuarantineChargeRatingSeparated ?? false;

		ZGuid[] IAccIntegrationDataProvider.DisbursementChargeCodes
		{
			get
			{
				if (fDisbursementChargeCodes == null)
				{
					var companyPK = declarationInBillingFactory.CompanyPK.ToGuid();
					fDisbursementChargeCodes = EntryChargeTypeList.GetAllChargeCodePKsOf(companyPK);

					if (IsQuarantineChargeRatingSeparated)
					{
						fDisbursementChargeCodes = fDisbursementChargeCodes.Except(AUASPEntryChargeTypeList.GetChargeCodePKForASP(companyPK)).ToArray();
					}
				}

				return fDisbursementChargeCodes;
			}
		}
		ZGuid[] fDisbursementChargeCodes;

		IAccInvoiceDataProvider[] IAccIntegrationDataProvider.InvDataProviders
		{
			get
			{
				if (fInvoiceDataProvider == null)
				{
					List<IAccInvoiceDataProvider> result = new List<IAccInvoiceDataProvider>();
					var isQuarantineChargeRatingSeparated = IsQuarantineChargeRatingSeparated;

					foreach (ZGuid entryPK in entryHeaderPKs)
					{
						var entryInBillingFactory = (IAccInvoiceDataProvider)declarationInBillingFactory.CustomsEntryHeaders.FindByPK(entryPK) as CusEntryHeader;
						if (entryInBillingFactory != null)
						{
							result.Add(new AUDSBEntryHeaderIAccInvoiceDataProvider(entryInBillingFactory, isQuarantineChargeRatingSeparated));
						}
					}

					fInvoiceDataProvider = result.ToArray();
				}
				return fInvoiceDataProvider;
			}
		}
		IAccInvoiceDataProvider[] fInvoiceDataProvider;
	}
}
