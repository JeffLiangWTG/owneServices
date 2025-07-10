using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class JobComInvoiceLineDeepCloneStrategy : Customs.Business.JobComInvoiceLineDeepCloneStrategy
	{
		public JobComInvoiceLineDeepCloneStrategy(JobComInvoiceLine invoiceLineToClone, CloneType cloneType, BaseJobComInvoiceHeader clonedInvoice, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection)
		{
		}

		new JobComInvoiceLine bizObjToClone
		{
			get { return (JobComInvoiceLine)base.bizObjToClone; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (JobComInvoiceLine)base.CloneInternal(args);

			if (IsTemplateCopy)
			{
				CloneCountrySpecificData(result, bizObjToClone);
			}
			return result;
		}

		public static void CloneCountrySpecificData(JobComInvoiceLine clonedInvoiceLine, JobComInvoiceLine invoiceLineToClone)
		{
			using (clonedInvoiceLine.GetValidationSuspender())
			using (clonedInvoiceLine.SuspendSettingHasChanges())
			{
				clonedInvoiceLine.FullGoodsDescription = invoiceLineToClone.FullGoodsDescription;

				if (clonedInvoiceLine.IsExport)
				{
					clonedInvoiceLine.ComplementaryDescription = invoiceLineToClone.ComplementaryDescription;
					clonedInvoiceLine.Attributes.CopyDataFrom(invoiceLineToClone.Attributes);
				}

				if (clonedInvoiceLine.IsImport)
				{
					clonedInvoiceLine.NVECusCodeDataCollection.CopyDataFrom(invoiceLineToClone.NVECusCodeDataCollection);
					clonedInvoiceLine.TariffDetachs.CloneFrom(invoiceLineToClone.TariffDetachs);
					clonedInvoiceLine.CusLineTariffDetails.CloneFrom(invoiceLineToClone.CusLineTariffDetails);
					clonedInvoiceLine.TaxRegimeCollection.CloneFrom(invoiceLineToClone.TaxRegimeCollection);

					if (clonedInvoiceLine.IsImportOnly)
					{
						clonedInvoiceLine.PreviousDocuments.CloneFrom(invoiceLineToClone.PreviousDocuments);
						clonedInvoiceLine.MercosulForeignDeclarations.CloneFrom(invoiceLineToClone.MercosulForeignDeclarations);
						clonedInvoiceLine.Permits.CloneFrom(invoiceLineToClone.Permits);
						clonedInvoiceLine.Taxes.CloneFrom(invoiceLineToClone.Taxes);
					}

					if (clonedInvoiceLine.IsImportLicense)
					{
						clonedInvoiceLine.DrawbackImportLicense.CloneFrom(invoiceLineToClone.DrawbackImportLicense);
						clonedInvoiceLine.ConsentingProcessCollection.CloneFrom(invoiceLineToClone.ConsentingProcessCollection);
						clonedInvoiceLine.DocAddresses.CloneFrom(invoiceLineToClone.DocAddresses);
					}

					if (clonedInvoiceLine.IsImportSiscomex)
					{
						clonedInvoiceLine.ImportLicenseSupportingInfo.CloneFrom(invoiceLineToClone.ImportLicenseSupportingInfo);
						clonedInvoiceLine.PreviousDocuments.CloneFrom(invoiceLineToClone.PreviousDocuments);
						clonedInvoiceLine.MercosulForeignDeclarations.CloneFrom(invoiceLineToClone.MercosulForeignDeclarations);
						clonedInvoiceLine.QuantityPerUnitInfos.CloneFrom(invoiceLineToClone.QuantityPerUnitInfos);
						clonedInvoiceLine.LegalActInfos.CloneFrom(invoiceLineToClone.LegalActInfos);
						clonedInvoiceLine.Taxes.CloneFrom(invoiceLineToClone.Taxes);

						if (clonedInvoiceLine.Declaration.PK == invoiceLineToClone.Declaration.PK)
						{
							clonedInvoiceLine.JI_CEI = invoiceLineToClone.JI_CEI;
						}
					}

					if (clonedInvoiceLine.IsImportExcludingLicense)
					{
						if (clonedInvoiceLine.AdditionalTariffsIsLoaded)
						{
							clonedInvoiceLine.AdditionalTariffs.Rebuild();
						}
						if (clonedInvoiceLine.SpecialCaseTaxesIsLoaded)
						{
							clonedInvoiceLine.SpecialCaseTaxes.Rebuild();
						}
					}
				}
			}
		}
	}
}
