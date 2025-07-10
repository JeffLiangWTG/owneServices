using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.GB.Chief
{
	class GbMessageManagerCreditCheckWithSecurityHelper : Integration.Customs.GB.GBChief.IGbMessageManagerCreditCheckWithSecurityHelper
	{
		public GbMessageManagerCreditCheckWithSecurityHelper(JobDeclaration declaration, Customs.Business.ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			this.declaration = declaration;
			this.sendMessagesToCustoms = sendMessagesToCustoms;
		}

		public bool ShouldCheckCreditForThisDeclarationAndMessage()
		{
			var companyPK = declaration.RegistryCompanyPK;
			var branchPk = declaration.RegistryBranchPK;
			var shouldCheck = false;
			var shouldAlwaysCheck = GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_AlwaysCheck.GetFallBackValueAtAllLevels(companyPK, branchPk, Guid.Empty);
			if (!shouldAlwaysCheck)
			{
				var checkEstimateThreshold = GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_EstimateDuty.GetFallBackValueAtAllLevels(companyPK, branchPk, Guid.Empty);
				var checkNotLodgedArrived = GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_NotLodgedArrived.GetFallBackValueAtAllLevels(companyPK, branchPk, Guid.Empty);
				var checkPreLodgedArriving = GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_PreLodgedArriving.GetFallBackValueAtAllLevels(companyPK, branchPk, Guid.Empty);

				if (!shouldCheck && checkNotLodgedArrived && IsNotLodgedButArrived)
				{
					shouldCheck = true;
				}

				if (!shouldCheck && checkPreLodgedArriving && IsPreLodgedAndArriving)
				{
					shouldCheck = true;
				}

				if (!shouldCheck && checkEstimateThreshold >= 0 && IsLodgedOrPrelodged)
				{
					if (HasEntryWhoseDutyIsGreaterThanBeforeByThisMuch(checkEstimateThreshold))
					{
						shouldCheck = true;
					}
				}
			}
			else
			{
				shouldCheck = true;
			}

			return shouldCheck;
		}

		public bool WarnAboutCreditChecks()
		{
			var helper = new Customs.Business.MessageManagerCreditCheckWithSecurityHelper(declaration);  // this will look at the registry for us
			var isAllowed = ShouldCheckCreditForThisDeclarationAndMessage() ? helper.IsCreditCheckOKToSend : helper.IsDeniedPartyOKToSend;
			if (!isAllowed)
			{
				if (!helper.IsCreditCheckDoneOutsideCW1)
				{
					sendMessagesToCustoms.NotifyUserOfAnInvalidOperation(helper.ReasonForNotAllowed);
				}
			}
			return isAllowed;
		}

		bool IsPreLodgedAndArriving
		{
			get { return declaration.IsPrelodgedAndNotCancelled && declaration.IsGoodsArrivedSubStyle; }
		}

		bool IsNotLodgedButArrived
		{
			get { return !IsLodgedOrPrelodged && declaration.IsGoodsArrivedSubStyle; }
		}

		bool IsLodgedOrPrelodged
		{
			get { return !declaration.DeclarationNumber.IsEmpty; }
		}

		bool HasEntryWhoseDutyIsGreaterThanBeforeByThisMuch(decimal checkEstimateThreshold)
		{
			var anyEntrysLiabilityIsGreater = false;
			foreach (CusEntryHeader entryHeader in declaration.ActiveEntryHeaders)
			{
				var estimatorDocumentWrapper = DocumentWrapperFactory.CreateWrapperWithoutException("Enterprise.Customs.GB.DocumentWrappers.DocTaxEstimatorWrapper, Enterprise.Customs.GB.DocumentWrappers", entryHeader);
				if (estimatorDocumentWrapper != null)
				{
					var iTotalDutyAndTotalVatProvider = estimatorDocumentWrapper as ITotalDutyAndTotalVatProvider;
					if (iTotalDutyAndTotalVatProvider != null)
					{
						// This relies upon CusEntryHeaderCharges being in GBP
						var aussieChargeTypesToIgnore = new List<string> { Constants.Customs.CusEntryFeeTypes.DutyAmount, Constants.Customs.CusEntryFeeTypes.GSTVATAmount, Constants.Customs.CusEntryFeeTypes.GSTVATDeferred };  // Ignore data for old records
						var oldLiability = (from CusEntryLine line in entryHeader.MergedLines from EU.Business.Declaration.CusEntryLineFee fee in line.Fees where !aussieChargeTypesToIgnore.Contains(fee.CF_ChargeType) select (decimal)fee.CF_ChargeAmount).Sum();
						var newLiability = iTotalDutyAndTotalVatProvider.TotalDuty + iTotalDutyAndTotalVatProvider.TotalVAT;
						if (newLiability == 0)
						{
							continue;
						}
						else if (oldLiability == 0 || (100 * newLiability / oldLiability) - 100 > checkEstimateThreshold)
						{
							anyEntrysLiabilityIsGreater = true;
							break;
						}
					}
				}
			}

			return anyEntrysLiabilityIsGreater;
		}

		readonly JobDeclaration declaration;
		readonly Customs.Business.ISendsMessagesToCustoms sendMessagesToCustoms;
	}
}
