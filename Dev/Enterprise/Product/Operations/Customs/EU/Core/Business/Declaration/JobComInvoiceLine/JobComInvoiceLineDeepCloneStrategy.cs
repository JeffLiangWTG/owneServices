using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Biz = Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceLineDeepCloneStrategy : Biz.JobComInvoiceLineDeepCloneStrategy
	{
		public JobComInvoiceLineDeepCloneStrategy(Biz.BaseJobComInvoiceLine invoiceLineToClone, Biz.CloneType cloneType, Biz.BaseJobComInvoiceHeader clonedInvoice, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection)
		{
		}

		JobComInvoiceLine EuSourceInvoiceLine => euSourceInvoiceLine ?? (euSourceInvoiceLine = (JobComInvoiceLine)invoiceLineToClone);
		JobComInvoiceLine euSourceInvoiceLine;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = base.CloneInternal(args);
			var euResult = (JobComInvoiceLine)result;

			foreach (SupportingDocument supportingDocument in EuSourceInvoiceLine.SupportingDocuments.ToArray())
			{
				euResult.SupportingDocuments.Add(supportingDocument.Clone(args));
			}
			foreach (AdditionalInfo additionalInfo in EuSourceInvoiceLine.AdditionalInfos.ToArray())
			{
				euResult.AdditionalInfos.Add(additionalInfo.Clone(args));
			}
			foreach (PreviousDocument previousDocument in EuSourceInvoiceLine.PreviousDocuments.ToArray())
			{
				euResult.PreviousDocuments.Add(previousDocument.Clone(args));
			}

			foreach (JobComInvoiceLineTax tax in EuSourceInvoiceLine.Taxes.ToArray())
			{
				euResult.Taxes.Add(tax.Clone(args));
			}

			foreach (AdditionalProcedureCode additionalProcedureCode in EuSourceInvoiceLine.AdditionalProcedureCodes.ToArray())
			{
				euResult.AdditionalProcedureCodes.Add(additionalProcedureCode.Clone(args));
			}

			foreach (CusFiscalReference cusFiscalReference in EuSourceInvoiceLine.FiscalReferences.ToArray())
			{
				euResult.FiscalReferences.Add(cusFiscalReference.Clone(args));
			}

			foreach (CusAuthorizationUsage cusAuthorizationUsage in EuSourceInvoiceLine.CusAuthorizationUsages.ToArray())
			{
				euResult.CusAuthorizationUsages.Add(cusAuthorizationUsage.Clone(args));
			}

			foreach (CusSupplyChainActorReference cusSupplyChainActorReference in EuSourceInvoiceLine.CusSupplyChainActorReferences.ToArray())
			{
				euResult.CusSupplyChainActorReferences.Add(cusSupplyChainActorReference.Clone(args));
			}

			if (EuSourceInvoiceLine.BuyerDocAddress.E2_OA_Address.IsValid)
			{
				euResult.BuyerDocAddress.E2_OA_Address = EuSourceInvoiceLine.BuyerDocAddress.E2_OA_Address;
			}

			if (EuSourceInvoiceLine.SellerDocAddress.E2_OA_Address.IsValid)
			{
				euResult.SellerDocAddress.E2_OA_Address = EuSourceInvoiceLine.SellerDocAddress.E2_OA_Address;
			}

			CloneSupplementaryCodes(args, euResult);

			return euResult;
		}

		void CloneSupplementaryCodes(BusinessObjectCloneArgs cloneArgs, JobComInvoiceLine targetInvoiceLine)
		{
			targetInvoiceLine.JI_SupplementaryCode1 = EuSourceInvoiceLine.JI_SupplementaryCode1;
			targetInvoiceLine.JI_SupplementaryCode2 = EuSourceInvoiceLine.JI_SupplementaryCode2;

			foreach (SupplementaryCode additionalSupplementaryCode in EuSourceInvoiceLine.AdditionalSupplementaryCodes.ToArray())
			{
				targetInvoiceLine.AdditionalSupplementaryCodes.Add(additionalSupplementaryCode.Clone(cloneArgs));
			}
		}
	}
}
