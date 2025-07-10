using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.Validation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class BaseChargeValidation : JobChargeValidation
	{
		public BaseChargeValidation(BaseCharge parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		protected new BaseCharge Parent;

		public void CheckAllowedToChange(ZPropertyInfo propertyInfo, ZPropertyInfo propertyInfoToAddError = null)
		{
			if (Parent.IsInDatabase && propertyInfo.HasChanges)
			{
				if (!Parent.IsAllowedToModifyThisCharge)
				{
					ZString propertyValue;
					if (propertyInfo.OriginalValue is ZGuid)
					{
						if (propertyInfo.OriginalValue.IsEmpty || !propertyInfo.OriginalValue.IsValid)
						{
							propertyValue = Res.GetString("f6c64814-169a-42cc-a43c-26d15b07892f", "empty");
						}
						else
						{
							BusinessObject relatedObject = BusinessObject.GetRelatedBizO(propertyInfo);
							BusinessObject originalValueRelatedObject = Parent.Factory.Load(relatedObject.GetType(), (ZGuid)propertyInfo.OriginalValue);
							string codeProperty = CodePropertyAttribute.CodePropertyNameFromType(originalValueRelatedObject.GetType());
							propertyValue = (ZString)originalValueRelatedObject[codeProperty];
						}
					}
					else
					{
						propertyValue = propertyInfo.OriginalValue.ToString();
					}
					(propertyInfoToAddError ?? propertyInfo).AddError(string.Format(CultureInfo.InvariantCulture, LoginPermissionErrorMessage(propertyValue)));
				}
				else if (Parent.JobIsReadyForFinancialClosureWithoutModifySecurity && !Parent.Factory.HasContext(BusinessContext.PostManagerCreatingTransaction))
				{
					(propertyInfoToAddError ?? propertyInfo).AddError(JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage);
				}
				else
				{
					var checker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
					if (checker.IsReceivablesCashAdvanceFunctionalityEnabled || checker.IsPayablesCashAdvanceFunctionalityEnabled)
					{
						CheckAllowedToChangeWhenActiveCashAdvanceRequestExist(propertyInfo);
					}
				}
			}
		}

		void CheckAllowedToChangeWhenActiveCashAdvanceRequestExist(ZPropertyInfo propertyInfo)
		{
			var criticalARPropertiesThatCanNotBeChanged = new[] { JobChargeSchema.Constants.JR_OH_SellAccount, JobChargeSchema.Constants.JR_RX_NKSellCurrency };
			var criticalAPPropertiesThatCanNotBeChanged = new[] { JobChargeSchema.Constants.JR_OH_CostAccount, JobChargeSchema.Constants.JR_RX_NKCostCurrency };

			if (criticalARPropertiesThatCanNotBeChanged.Contains(propertyInfo.Name) && Parent.ARCashAdvanceRequirement.HasActiveCashAdvanceRequestLine)
			{
				if (propertyInfo.Name == JobChargeSchema.Constants.JR_OH_SellAccount)
				{
					propertyInfo.AddError(GetActiveCashAdvanceExistErrorMessageForOrganization(LedgerTypes.AccountsReceivable, Parent.ARCashAdvanceRequirement.CashAdvanceRequest.OrganizationCode));
				}
				else
				{
					propertyInfo.AddError(GetActiveCashAdvanceExistErrorMessageForCurrency(LedgerTypes.AccountsReceivable, Parent.ARCashAdvanceRequirement.OSCurrency));
				}
			}
			else if (criticalAPPropertiesThatCanNotBeChanged.Contains(propertyInfo.Name) && Parent.APCashAdvanceRequirement.HasActiveCashAdvanceRequestLine)
			{
				if (propertyInfo.Name == JobChargeSchema.Constants.JR_OH_CostAccount)
				{
					propertyInfo.AddError(GetActiveCashAdvanceExistErrorMessageForOrganization(LedgerTypes.AccountsPayable, Parent.APCashAdvanceRequirement.CashAdvanceRequest.OrganizationCode));
				}
				else
				{
					propertyInfo.AddError(GetActiveCashAdvanceExistErrorMessageForCurrency(LedgerTypes.AccountsPayable, Parent.APCashAdvanceRequirement.OSCurrency));
				}
			}
		}

		static MultilingualString GetActiveCashAdvanceExistErrorMessageForCurrency(ZString ledgerType, ZString currencyCode) =>
			ResString.GetMultilingualString("c142188b-594f-4b3b-8158-606cb7079926", "This charge has an active {0} Advance Payment with Currency {1}. Please change the currency to match the Advance Payment, or cancel the Advance Payment to continue.", ledgerType, currencyCode);
		static MultilingualString GetActiveCashAdvanceExistErrorMessageForOrganization(ZString ledgerType, ZString organizationCode) =>
			ResString.GetMultilingualString("01d5ba47-880e-4c03-818d-3dee180a35b6", "This charge has an active {0} Advance Payment with Organization {1}. Please change the Organization to match the Advance Payment, or cancel the Advance Payment to continue.", ledgerType, organizationCode);

		static string LoginPermissionErrorMessage (ZString propertyValue) =>
			Res.GetString("bdd8c98d-fb46-47be-a418-ac10ab9d9d75", @"You cannot create a new charge / modify / delete charge against a branch for which you do not have login permission. You must reset the value to its previous value {0}", propertyValue);

		internal static string ConsolCostIsInconsistentWithApportionChargeErrorMessage() =>
			Res.GetString("0c7c19cf-1670-4a60-a846-bd54fadc40c1", "Charge invoice date and payment date must be equal to value on parent Consol Cost. Use Job Invoicing > Synchronize Cost Invoice Details menu on Consolidation > Consol Costing to fix data.");

		protected override void CheckJR_A9_SellVATClass()
		{
			base.CheckJR_A9_SellVATClass();

			if (!Parent.IsRevenuePosted)
			{
				CheckAllowedToChange(Parent.JR_A9_SellVATClassInfo);
				CountrySpecificValidationHelper.AddErrorOrWarningIfNoTaxMessage(Parent.JR_A9_SellVATClassInfo, Parent.JR_AT_SellGSTRate, () => Parent.JR_OSSellAmt, () => Parent.JR_OSSellGSTAmt_Calc, () => Parent.JR_Calc_OSSellExtraTaxAmt, isAR: true);
				CheckTaxIdAndTaxMessage(TransactionLineTypes.Revenue, Parent);
			}
		}

		protected override void CheckJR_A9_CostVATClass()
		{
			base.CheckJR_A9_CostVATClass();

			if (!Parent.IsCostPosted)
			{
				CheckAllowedToChange(Parent.JR_A9_CostVATClassInfo);
				CountrySpecificValidationHelper.AddErrorOrWarningIfNoTaxMessage(Parent.JR_A9_CostVATClassInfo, Parent.JR_AT_CostGSTRate, () => Parent.JR_OSCostAmt, () => Parent.JR_OSCostGSTAmt_Calc, () => Parent.JR_Calc_OSCostExtraTaxAmt, isAP: true);
				CheckTaxIdAndTaxMessage(TransactionLineTypes.Cost, Parent);
			}
		}

		protected override void CheckJR_GB_InternalBranch()
		{
			if (Parent.IsOrgProxyAccountPosted)
			{
				return;
			}

			base.CheckJR_GB_InternalBranch();

			if (AutoJRJRegistryStatusHelper.IsAutoJRJEnabled() && Parent.IsConsumerTypeShoudCreateCostOrSellJRJ)
			{
				ListValidation.ErrorIfInvalidPK(Parent.JR_GB_InternalBranchInfo);

				CheckAllowedToChange(Parent.JR_GB_InternalBranchInfo);

				if (Parent.IsNotEligableForAutoJRJ())
				{
					if (!Parent.JR_GB_InternalBranch.IsEmpty)
					{
						Parent.JR_GB_InternalBranchInfo.AddError(Res.GetString("009D08FF-D860-4B42-A28C-DB075D21E7A0", "You cannot set the internal branch when the Cost or Sell charge is not eligible to post auto Job Revenue Journal."));
					}
				}
				else
				{
					if (Parent.JR_GB_InternalBranch.IsValid && !Parent.CostOrSellAccountIsOrgProxy)
					{
						Parent.JR_GB_InternalBranchInfo.AddError(Res.GetString("d4a0062b-29df-42a9-b9ac-b51cf7d3875c", "You can only set the internal branch when the Cost or Sell Account is an organization proxy for the current company."));
					}
					else if (Parent.CostOrSellAccountIsOrgProxy && Parent.CalculatedCompany != null)
					{
						if (Parent.JR_GB_InternalBranch.IsEmpty)
						{
							Parent.JR_GB_InternalBranchInfo.AddWarning(Res.GetString("a074e8d7-ff6f-4488-82b1-1458814575ab", "Job Revenue Journal is posted upon Save only when Internal Branch is specified."));
						}
						else if (Parent.CostAccountIsOrgProxy && !Parent.IsCostPosted
							&& Parent.InternalBranch != null && Parent.InternalBranch.GB_OH_OrgProxy != Parent.JR_OH_CostAccount)
						{
							var branchCodes = GetBranchCodesForOrganisation(Parent.JR_OH_CostAccount);
							Parent.JR_GB_InternalBranchInfo.AddError(Res.GetString("6c5cebba-b879-43fd-961d-c9d5c4a1e0bd", @"The branch you have selected does not match the Cost Account.
Please set the Internal Branch to one of these values in order to create a job revenue journal: {0}", branchCodes));
						}
						else if (Parent.SellAccountIsOrgProxy && !Parent.IsRevenuePosted
							&& Parent.InternalBranch != null && Parent.InternalBranch.GB_OH_OrgProxy != Parent.JR_OH_SellAccount)
						{
							var branchCodes = GetBranchCodesForOrganisation(Parent.JR_OH_SellAccount);
							Parent.JR_GB_InternalBranchInfo.AddError(Res.GetString("c75a9bcb-fa13-43c0-9a59-a5585c621017", @"The branch you have selected does not match the Sell Account.
Please set the Internal Branch to one of these values in order to create a job revenue journal: {0}", branchCodes));
						}
					}
				}
			}
		}

		string GetBranchCodesForOrganisation(ZGuid organisationPK)
		{
			var query = new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, organisationPK);
			query.AddToFilter(GlbBranchSchema.GB_IsActive, true);
			query.AddToFilter(GlbBranchSchema.GB_GC, Parent.CalculatedCompany.PK);
			query.OrderBy = GlbBranchSchema.Constants.GB_Code;
			var branchCodes = string.Join(",", Parent.Factory.Load<GlbBranch>(query).Select(x => x.GB_Code));
			return branchCodes;
		}

		protected override void CheckJR_GE_InternalDept()
		{
			if (Parent.IsOrgProxyAccountPosted)
			{
				return;
			}

			base.CheckJR_GE_InternalDept();
			var internalDeptInfo = Parent.JR_GE_InternalDeptInfo;
			if (AutoJRJRegistryStatusHelper.IsAutoJRJEnabled() && Parent.IsConsumerTypeShoudCreateCostOrSellJRJ)
			{
				ListValidation.ErrorIfInvalidPK(internalDeptInfo);
				CheckAllowedToChange(internalDeptInfo);

				if (Parent.IsNotEligableForAutoJRJ())
				{
					if (!Parent.JR_GE_InternalDept.IsEmpty)
					{
						internalDeptInfo.AddError(Res.GetString("044CB259-6BFB-49CF-B472-CC95C3DC20B8", "You cannot set the internal department when the Cost or Sell charge is not eligible to post auto Job Revenue Journal."));
					}
				}
				else
				{
					CheckDepartmentIsValidForThisChargeCode(Parent.ChargeCode, Parent.InternalDept, internalDeptInfo);
					ValidateMiscDepartment(Parent.InternalDept, internalDeptInfo);

					if (Parent.JR_GE_InternalDept.IsEmpty && Parent.CostOrSellAccountIsOrgProxy)
					{
						internalDeptInfo.AddWarning(Res.GetString("654d7151-c15c-42bc-85d9-860366dd0208", "Job Revenue Journal is posted upon Save only when Internal Department is specified."));
					}

					if (Parent.JR_GE_InternalDept.IsValid && !Parent.CostOrSellAccountIsOrgProxy)
					{
						internalDeptInfo.AddError(Res.GetString("0705ab2c-8fe7-46eb-872d-9e54e0e2c15c", "You can only set the internal department when the Cost or Sell account is an organization proxy for the current company."));
					}
				}
			}

			if (!internalDeptInfo.ReadOnly && !internalDeptInfo.HasErrors())
			{
				GlbBranchCombinationValidation.CheckBranchDepartmentCombination(internalDeptInfo, Parent.InternalBranch, Parent.InternalDept);
			}
		}

		protected void CheckDepartmentIsValidForThisChargeCode(AccChargeCode chargeCode, GlbDepartment department, ZPropertyInfo info)
		{
			if (chargeCode != null
				&& department != null
				&& !chargeCode.AC_DepartmentFilterList.Contains("ALL")
				&& !chargeCode.AC_DepartmentFilterList.Contains(department.GE_Code))
			{
				info.AddError(Res.GetString("603e15af-bf94-40fd-b710-b8d342121fd1", "This department is not valid for the charge code specified on this Job."));
			}
		}

		protected void ValidateMiscDepartment(GlbDepartment department, ZPropertyInfo info)
		{
			if (!info.HasErrors() && ShouldValidateJobForMiscellaneousDepartment)
			{
				if (department != null && department.GE_Misc)
				{
					info.AddError(Res.GetString("555ebc73-b592-40ac-ab1e-9bc191264e34", "Cannot issue job charges for a miscellaneous department."));
				}
			}
		}

		protected bool ShouldValidateJobForMiscellaneousDepartment
		{
			get
			{
				return Parent.InvoicingJob == null
					|| Parent.InvoicingJob.PlugInData == null
					|| Parent.InvoicingJob.PlugInData.InvoicingSupporter.ConsumerType == null
					|| Parent.InvoicingJob.PlugInData.InvoicingSupporter.ConsumerType.ValidateJobForMiscellaneousDepartment;
			}
		}

		protected override void CheckJR_JH_InternalJob()
		{
			if (Parent.IsOrgProxyAccountPosted)
			{
				return;
			}

			base.CheckJR_JH_InternalJob();

			if (AutoJRJRegistryStatusHelper.IsAutoJRJEnabled() && Parent.IsConsumerTypeShoudCreateCostOrSellJRJ)
			{
				ListValidation.ErrorIfInvalidPK(Parent.JR_JH_InternalJobInfo);
				CheckAllowedToChange(Parent.JR_JH_InternalJobInfo);

				if (Parent.IsNotEligableForAutoJRJ())
				{
					if (!Parent.JR_JH_InternalJob.IsEmpty)
					{
						Parent.JR_JH_InternalJobInfo.AddError(Res.GetString("28CA8A38-3E3C-4179-B271-2C3595E9A394", "You cannot set the internal job when the Cost or Sell charge is not eligible to post auto Job Revenue Journal."));
					}
				}
				else
				{
					if (Parent.JR_JH_InternalJob.IsValid)
					{
						if (!Parent.CostOrSellAccountIsOrgProxy)
						{
							Parent.JR_JH_InternalJobInfo.AddError(Res.GetString("d14a8297-cae2-450e-8018-e77a95184b1f", "You can only set the internal job when the Cost or Sell account is an organization proxy for the current company."));
						}
						else if (Parent.JR_GB_InternalBranch.IsEmpty && Parent.JR_GE_InternalDept.IsEmpty)
						{
							Parent.JR_JH_InternalJobInfo.AddError(Res.GetString("3395526c-4e14-46a4-bdad-412bd9166f3e", "You cannot set the internal job without also setting the internal branch or internal department."));
						}

						if (!Parent.JR_JH_InternalJobInfo.HasErrors() && Parent.InternalInvoicingJob.Validation is JobValidation jobValidation)
						{
							string errorMessage = Res.GetString("F4C50F46-1182-48C8-94E3-03EB77C55259", "Job Revenue Journal cannot be posted until the {0:G} for this internal job is recorded. This internal job and charge code combination requires this date for revenue recognition purposes.");

							string error = jobValidation.GetRevenueRecognitionDateValidationError(errorMessage, Parent.ChargeCode);
							if (!string.IsNullOrEmpty(error))
							{
								Parent.JR_JH_InternalJobInfo.AddError(error);
							}
						}

						if (Parent.JR_GB_InternalBranch.IsValid && Parent.JR_GB_InternalBranch == Parent.JR_GB
							&& Parent.JR_GE_InternalDept.IsValid && Parent.JR_GE_InternalDept == Parent.JR_GE
							&& Parent.JR_JH_InternalJob == Parent.JR_JH
							&& !Parent.IsDebtorGatewayAgent())
						{
							Parent.JR_JH_InternalJobInfo.AddWarning(Res.GetString("26ca1576-52e8-48d6-8ea4-a1fd390be606", "Job Revenue Journal is posted upon Save only when Internal Job/-Branch/-Department is different to Charge Job/-Branch/-Department."));
						}
					}
					else if (Parent.JR_GB_InternalBranch.IsValid || Parent.JR_GE_InternalDept.IsValid)
					{
						Parent.JR_JH_InternalJobInfo.AddError(Res.GetString("22f728e2-9c2f-436a-8382-50aa7adceff3", "You must nominate an internal job when the internal branch or department has been set."));
					}

					var apportionmentCharges = Parent.ParentConsolCost?.ApportionmentCharges?.OfType<ApportionSplitCharge>().Where(x => x.JR_IsUsedForApportionment);
					if (Parent is ApportionSplitCharge apportionSplitCharge
						&& Parent.OfType<ApportionSplitCharge>().Any(x => x.JR_IsUsedForApportionment)
						&& !Parent.ShouldCreateJRJ
						&& apportionmentCharges != null
						&& (apportionSplitCharge.IsGatewaySellApportionmentCharge || !AutoJRJRegistryStatusHelper.IsAutoJRJWithTaxRegistrationNumberEnabled() || apportionmentCharges.All(x => !x.IsInternalJobInfoDisabled)))
					{
						var parentConsolCostHasShouldCreateJRJCharge = apportionmentCharges.Any(x => x.ShouldCreateJRJ);

						if (parentConsolCostHasShouldCreateJRJCharge)
						{
							Parent.JR_JH_InternalJobInfo.AddError(Res.GetString("F81E3860-1285-4BC4-9076-389A3CA3841B", "Please ensure the Internal Job/Branch/Department is different to Charge Job/Branch/Department."));
						}
					}
				}
			}
		}

		#region CheckJR_Desc

		protected override void CheckJR_Desc()
		{
			base.CheckJR_Desc();
			CheckAllowedToChange(Parent.JR_DescInfo);
		}

		#endregion

		#region CheckJR_RX_NKCostCurrency

		protected override void CheckJR_RX_NKCostCurrency()
		{
			base.CheckJR_RX_NKCostCurrency();
			CheckAllowedToChange(Parent.JR_RX_NKCostCurrencyInfo);
		}

		#endregion

		#region CheckJR_RX_NKSellCurrency

		protected override void CheckJR_RX_NKSellCurrency()
		{
			base.CheckJR_RX_NKSellCurrency();
			CheckAllowedToChange(Parent.JR_RX_NKSellCurrencyInfo);
		}

		#endregion

		#region CheckJR_RX_NKSellInvoiceCurrency

		protected override void CheckJR_RX_NKSellInvoiceCurrency()
		{
			base.CheckJR_RX_NKSellInvoiceCurrency();
			CheckAllowedToChange(Parent.JR_RX_NKSellInvoiceCurrencyInfo);
		}

		#endregion

		#region CheckJR_CostPlaceOfSupplyType

		protected override void CheckJR_CostPlaceOfSupplyType()
		{
			base.CheckJR_CostPlaceOfSupplyType();
			CheckAllowedToChange(Parent.JR_CostPlaceOfSupplyTypeInfo);
		}

		#endregion

		#region CheckJR_CostPlaceOfSupply

		protected override void CheckJR_CostPlaceOfSupply()
		{
			base.CheckJR_CostPlaceOfSupply();
			CheckAllowedToChange(Parent.JR_CostPlaceOfSupplyInfo);
		}

		#endregion

		#region CheckJR_SellPlaceOfSupplyType

		protected override void CheckJR_SellPlaceOfSupplyType()
		{
			base.CheckJR_SellPlaceOfSupplyType();
			CheckAllowedToChange(Parent.JR_SellPlaceOfSupplyTypeInfo);
		}

		#endregion

		#region CheckJR_SellPlaceOfSupply

		protected override void CheckJR_SellPlaceOfSupply()
		{
			base.CheckJR_SellPlaceOfSupply();
			CheckAllowedToChange(Parent.JR_SellPlaceOfSupplyInfo);
		}

		#endregion

		#region CheckJR_SellGovtChargeCode

		protected override void CheckJR_SellGovtChargeCode()
		{
			base.CheckJR_SellGovtChargeCode();
			CheckAllowedToChange(Parent.JR_SellGovtChargeCodeInfo);
		}

		#endregion

		#region CheckJR_AgentDeclaredCostAmt

		protected override void CheckJR_AgentDeclaredCostAmt()
		{
			base.CheckJR_AgentDeclaredCostAmt();
			CheckAllowedToChange(Parent.JR_AgentDeclaredCostAmtInfo);
		}

		#endregion

		#region CheckJR_AgentDeclaredSellAmt

		protected override void CheckJR_AgentDeclaredSellAmt()
		{
			base.CheckJR_AgentDeclaredSellAmt();
			CheckAllowedToChange(Parent.JR_AgentDeclaredSellAmtInfo);
		}

		#endregion

		#region CheckJR_ChargeType

		protected override void CheckJR_ChargeType()
		{
			base.CheckJR_ChargeType();
			CheckAllowedToChange(Parent.JR_ChargeTypeInfo);
		}

		#endregion

		#region CheckJR_ChequeNo

		protected override void CheckJR_ChequeNo()
		{
			base.CheckJR_ChequeNo();
			CheckAllowedToChange(Parent.JR_ChequeNoInfo);
		}

		#endregion

		#region CheckJR_CostGovtChargeCode

		protected override void CheckJR_CostGovtChargeCode()
		{
			base.CheckJR_CostGovtChargeCode();
			CheckAllowedToChange(Parent.JR_CostGovtChargeCodeInfo);
		}

		#endregion

		#region CheckJR_CostRatingOverride

		protected override void CheckJR_CostRatingOverride()
		{
			base.CheckJR_CostRatingOverride();
			CheckAllowedToChange(Parent.JR_CostRatingOverrideInfo);
		}

		#endregion

		#region CheckJR_MarginPercentage

		protected override void CheckJR_MarginPercentage()
		{
			base.CheckJR_MarginPercentage();
			CheckAllowedToChange(Parent.JR_MarginPercentageInfo);
		}

		#endregion

		#region CheckJR_OSCostAmt

		protected override void CheckJR_OSCostAmt()
		{
			base.CheckJR_OSCostAmt();
			CheckAllowedToChange(Parent.JR_OSCostAmtInfo);
		}

		#endregion

		#region CheckJR_OSSellAmt

		protected override void CheckJR_OSSellAmt()
		{
			if (!Parent.JR_OSSellAmtInfo.HasErrors())
			{
				base.CheckJR_OSSellAmt();
				CheckAllowedToChange(Parent.JR_OSSellAmtInfo);
			}
		}

		#endregion

		#region CheckJR_SellReference

		protected override void CheckJR_SellReference()
		{
			base.CheckJR_SellReference();
			CheckAllowedToChange(Parent.JR_SellReferenceInfo);
			if (!Parent.JR_SellReference.IsEmpty)
			{
				Parent.JR_SellReferenceInfo.AddWarning(Res.GetString("BC28EE80-E0F8-47D5-9D7A-92C87381A53E", "The system will group and post multiple invoices by Sell Reference when a value is entered."));
			}
		}

		#endregion

		#region CheckJR_PaymentDate

		protected override void CheckJR_PaymentDate()
		{
			base.CheckJR_PaymentDate();
			CheckAllowedToChange(Parent.JR_PaymentDateInfo);
		}

		#endregion

		#region CheckJR_EstimatedCost

		protected override void CheckJR_EstimatedCost()
		{
			base.CheckJR_EstimatedCost();
			CheckAllowedToChange(Parent.JR_EstimatedCostInfo);
		}

		#endregion

		#region CheckJR_SellRatingOverride

		protected override void CheckJR_SellRatingOverride()
		{
			base.CheckJR_SellRatingOverride();
			CheckAllowedToChange(Parent.JR_SellRatingOverrideInfo);
		}

		#endregion

		#region CheckJR_APInvoiceDate

		protected override void CheckJR_APInvoiceDate()
		{
			base.CheckJR_APInvoiceDate();
			CheckAllowedToChange(Parent.JR_APInvoiceDateInfo);
		}

		#endregion

		#region CheckJR_APDocumentReceivedDate

		protected override void CheckJR_APDocumentReceivedDate()
		{
			base.CheckJR_APDocumentReceivedDate();
			CheckAllowedToChange(Parent.JR_APDocumentReceivedDateInfo);
		}

		#endregion

		#region CheckJR_Calc_RelatedJobNumber

		internal void ValidateJR_Calc_RelatedJobNumber()
		{
			ValidateCalculatedProperty(Parent.JR_Calc_RelatedJobNumberInfo);
		}

		protected void CheckJR_Calc_RelatedJobNumber()
		{
			if (!Parent.IsRevenuePosted)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JR_Calc_RelatedJobNumberInfo);
			}
		}

		#endregion

		#region CheckJR_Calc_InvoiceTarget

		internal void ValidateJR_Calc_InvoiceTarget()
		{
			ValidateCalculatedProperty(Parent.JR_Calc_InvoiceTargetInfo);
		}

		protected void CheckJR_Calc_InvoiceTarget()
		{
			if (!Parent.IsRevenuePosted)
			{
				var value = Parent.JR_Calc_InvoiceTarget;
				var count = 0;
				foreach (var target in Parent.GetInvoiceTargetsIfEnabled())
				{
					if (target.JobNumber == value)
					{
						count++;
						if (count > 1)
						{
							Parent.JR_Calc_InvoiceTargetInfo.AddError(Res.GetString("4647EA48-13D7-42C1-A608-D01B1FC44FAC", "Unable to select this job. Duplicate jobs with same job number detected."));
							break;
						}
					}
				}

				ListValidation.ErrorIfInvalidCode(Parent.JR_Calc_InvoiceTargetInfo);
			}
		}

		#endregion

		protected override void CheckJR_AT_CostGSTRate()
		{
			base.CheckJR_AT_CostGSTRate();
			CheckAllowedToChange(Parent.JR_AT_CostGSTRateInfo);
		}

		protected override void CheckJR_CostTaxDate()
		{
			base.CheckJR_CostTaxDate();
			CheckAllowedToChange(Parent.JR_CostTaxDateInfo);
		}

		protected override void CheckJR_AT_SellGSTRate()
		{
			base.CheckJR_AT_SellGSTRate();
			CheckAllowedToChange(Parent.JR_AT_SellGSTRateInfo);
		}

		protected override void CheckJR_SellTaxDate()
		{
			base.CheckJR_SellTaxDate();
			CheckAllowedToChange(Parent.JR_SellTaxDateInfo);
		}

		#region CheckJR_AW_CostWHTRate

		protected override void CheckJR_AW_CostWHTRate()
		{
			base.CheckJR_AW_CostWHTRate();
			CheckAllowedToChange(Parent.JR_AW_CostWHTRateInfo);
		}

		#endregion

		#region CheckJR_AW_SellWHTRate

		protected override void CheckJR_AW_SellWHTRate()
		{
			base.CheckJR_AW_SellWHTRate();
			CheckAllowedToChange(Parent.JR_AW_SellWHTRateInfo);
		}

		#endregion

		#region CheckJR_DeclaredOSCostAmt

		protected override void CheckJR_DeclaredOSCostAmt()
		{
			base.CheckJR_DeclaredOSCostAmt();
			CheckAllowedToChange(Parent.JR_DeclaredOSCostAmtInfo);
		}

		#endregion

		#region CheckJR_EstimatedRevenue

		protected override void CheckJR_EstimatedRevenue()
		{
			if (!Parent.JR_EstimatedRevenueInfo.HasErrors())
			{
				base.CheckJR_EstimatedRevenue();
				CheckAllowedToChange(Parent.JR_EstimatedRevenueInfo);
			}
		}

		#endregion

		#region CheckJR_IsIncludedInProfitShare

		protected override void CheckJR_IsIncludedInProfitShare()
		{
			base.CheckJR_IsIncludedInProfitShare();
			CheckAllowedToChange(Parent.JR_IsIncludedInProfitShareInfo);
		}

		#endregion

		#region CheckJR_LineCFX

		protected override void CheckJR_LineCFX()
		{
			base.CheckJR_LineCFX();
			CheckAllowedToChange(Parent.JR_LineCFXInfo);
		}

		#endregion

		#region CheckJR_OP_Product

		protected override void CheckJR_OP_Product()
		{
			base.CheckJR_OP_Product();
			CheckAllowedToChange(Parent.JR_OP_ProductInfo);
		}

		#endregion

		#region CheckJR_OrderReference

		protected override void CheckJR_OrderReference()
		{
			base.CheckJR_OrderReference();
			CheckAllowedToChange(Parent.JR_OrderReferenceInfo);
		}

		#endregion

		#region CheckJR_OSCostWHTAmt

		protected override void CheckJR_OSCostWHTAmt()
		{
			base.CheckJR_OSCostWHTAmt();
			CheckAllowedToChange(Parent.JR_OSCostWHTAmtInfo);
		}

		#endregion

		#region CheckJR_OSSellWHTAmt

		protected override void CheckJR_OSSellWHTAmt()
		{
			if (!Parent.JR_OSSellWHTAmtInfo.HasErrors())
			{
				base.CheckJR_OSSellWHTAmt();
				CheckAllowedToChange(Parent.JR_OSSellWHTAmtInfo);
			}
		}

		#endregion

		#region CheckJR_PreventInvoicePrintGrouping

		protected override void CheckJR_PreventInvoicePrintGrouping()
		{
			base.CheckJR_PreventInvoicePrintGrouping();
			CheckAllowedToChange(Parent.JR_PreventInvoicePrintGroupingInfo);
		}

		#endregion

		#region CheckJR_ProductQuantity

		protected override void CheckJR_ProductQuantity()
		{
			base.CheckJR_ProductQuantity();
			CheckAllowedToChange(Parent.JR_ProductQuantityInfo);
		}

		#endregion

		#region CheckJR_GB

		protected override void CheckJR_GB()
		{
			base.CheckJR_GB();
			ListValidation.ErrorIfInvalidPK(Parent.JR_GBInfo, Parent.Branches);
			CheckAllowedToChange(Parent.JR_GBInfo);

			if (!Parent.IsInDatabase && Parent.ShouldValidateBranchAndDepartment && !Parent.IsAllowedToModifyThisCharge)
			{
				Parent.JR_GBInfo.AddError(Res.GetString("3e1aa202-3ab0-4cdb-bb8c-95206250bb8c", "You cannot create a new charge against a branch for which you do not have login permission."));
			}

			CheckEmptyStates();
		}

		#endregion

		#region CheckJR_GB_CostTaxBranch

		protected override void CheckJR_GB_CostTaxBranch()
		{
			base.CheckJR_GB_CostTaxBranch();

			if (!Parent.JR_GB_CostTaxBranchInfo.ReadOnly)
			{
				CheckAllowedToChange(Parent.JR_GB_CostTaxBranchInfo);

				if (!Parent.JR_GB_CostTaxBranchInfo.HasErrors())
				{
					MandatoryValidation.CheckEntered(Parent.JR_GB_CostTaxBranchInfo);
					ListValidation.ErrorIfInvalidPK(Parent.JR_GB_CostTaxBranchInfo);
				}

				if (!Parent.JR_GB_CostTaxBranchInfo.HasErrors()
					&& !Parent.JR_APInvoiceNum.IsEmpty
					&& Parent.JR_OH_CostAccount.IsValid
					&& Parent.InvoicingJob != null
					&& Parent.InvoicingJob.Charges.Find(x => !x.IsCostPosted
						&& x.JR_APInvoiceNum == Parent.JR_APInvoiceNum
						&& x.JR_OH_CostAccount == Parent.JR_OH_CostAccount
						&& x.JR_GB_CostTaxBranch != Parent.JR_GB_CostTaxBranch).Any())
				{
					Parent.JR_GB_CostTaxBranchInfo.AddError(Res.GetString("A3F9B42C-76FA-47C6-8535-3F8E94E26587", "All cost lines with the same Creditor and AP Invoice Number must have the same Cost Tax Branch."));
				}
			}
		}

		#endregion

		#region CheckJR_GB_SellTaxBranch

		protected override void CheckJR_GB_SellTaxBranch()
		{
			base.CheckJR_GB_SellTaxBranch();

			if (!Parent.JR_GB_SellTaxBranchInfo.ReadOnly)
			{
				CheckAllowedToChange(Parent.JR_GB_SellTaxBranchInfo);

				if (!Parent.JR_GB_SellTaxBranchInfo.HasErrors())
				{
					MandatoryValidation.CheckEntered(Parent.JR_GB_SellTaxBranchInfo);
					ListValidation.ErrorIfInvalidPK(Parent.JR_GB_SellTaxBranchInfo);
				}
			}
		}

		#endregion

		#region CheckEmptyStates

		void CheckEmptyStates()
		{
			if (Parent.HasRowErrors)
			{
				Parent.RemoveRowError(IndiaCompanyEmptyStateErrorMessages.EmptyStateErrorMessageForChargeBranch);
			}
			if (Parent.HasRowWarnings)
			{
				Parent.RemoveRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForOrigin);
				Parent.RemoveRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForDestination);
				Parent.RemoveRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForFixedPlaceOfSupply);
				Parent.RemoveRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForSellAccount);
				Parent.RemoveRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForCostAccount);
			}

			if (CheckEmptyStateForChargeBranch())
			{
				Parent.AddRowError(IndiaCompanyEmptyStateErrorMessages.EmptyStateErrorMessageForChargeBranch);
			}
			if (CheckEmptyStateForOrigin())
			{
				Parent.AddRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForOrigin);
			}
			if (CheckEmptyStateForDestination())
			{
				Parent.AddRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForDestination);
			}
			if (CheckEmptyStateForFixedPlaceOfSupply())
			{
				Parent.AddRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForFixedPlaceOfSupply);
			}
			if (CheckEmptyStateForSellAccount())
			{
				Parent.AddRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForSellAccount);
			}
			if (CheckEmptyStateForCostAccount())
			{
				Parent.AddRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForCostAccount);
			}
		}

		bool CheckEmptyStateForChargeBranch()
		{
			return AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value
				&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.India
				&& !Parent.IsCostPosted && !Parent.IsRevenuePosted
				&& (Parent.IsCostGSTApplicable || Parent.IsSellGSTApplicable)
				&& Parent.Branch != null && string.IsNullOrWhiteSpace(AccountingTaxLocations.GetBranchState(Parent.Branch));
		}

		bool CheckEmptyStateForOrigin()
		{
			if (AllowValidateEmptyState() && Parent.InvoicingJob?.PlugInData?.InvoicingSupporter?.Origin != null)
			{
				var state = (Parent.InvoicingJob.PlugInData.InvoicingSupporter.Origin as ILocation)?.State?.RW_Code;
				if (string.IsNullOrWhiteSpace(state))
				{
					return true;
				}
			}

			return false;
		}

		bool CheckEmptyStateForDestination()
		{
			if (AllowValidateEmptyState() && Parent.InvoicingJob?.PlugInData?.InvoicingSupporter?.Destination != null)
			{
				var state = (Parent.InvoicingJob.PlugInData.InvoicingSupporter.Destination as ILocation)?.State?.RW_Code;
				if (string.IsNullOrWhiteSpace(state))
				{
					return true;
				}
			}

			return false;
		}

		bool CheckEmptyStateForFixedPlaceOfSupply()
		{
			if (AllowValidateEmptyState() && AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.Value && Parent.InvoicingJob?.PlugInData?.InvoicingSupporter?.FixedPlaceOfSupply != null)
			{
				var state = Parent.InvoicingJob.PlugInData.InvoicingSupporter.FixedPlaceOfSupply.State?.RW_Code;
				if (string.IsNullOrWhiteSpace(state))
				{
					return true;
				}
			}

			return false;
		}

		bool CheckEmptyStateForSellAccount()
		{
			return AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value
				&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.India
				&& Parent.IsSellGSTApplicable
				&& Parent.SellAccount != null && string.IsNullOrWhiteSpace(Parent.SellAccount.MainAddress?.OA_State);
		}

		bool CheckEmptyStateForCostAccount()
		{
			return AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value
				&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.India
				&& Parent.IsCostGSTApplicable
				&& Parent.CostAccount != null && string.IsNullOrWhiteSpace(Parent.CostAccount.MainAddress?.OA_State);
		}

		bool AllowValidateEmptyState()
		{
			return AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.India && Parent.InvoicingJob != null;
		}

		#endregion

		#region CheckJR_OSCostExRate

		protected override void CheckJR_OSCostExRate()
		{
			base.CheckJR_OSCostExRate();
			if (!Parent.IsRevenueCharge && !Parent.IsCostPosted && Parent.JR_OSCostExRate <= 0)
			{
				Parent.JR_OSCostExRateInfo.AddError(Res.GetString("6b1ffcdd-cebe-4c7b-b51c-78be336be075", "Cost Exchange Rate for Currency {0} must be greater than 0.", Parent.JR_RX_NKCostCurrency));
			}
		}

		#endregion

		#region CheckJR_OSSellExRate

		protected override void CheckJR_OSSellExRate()
		{
			base.CheckJR_OSSellExRate();
			if (!Parent.IsRevenuePosted && Parent.JR_OSSellExRate <= 0)
			{
				Parent.JR_OSSellExRateInfo.AddError(Res.GetString("962ac558-43f7-4de9-8109-63ec186b0ab2", "Sell Exchange Rate for Currency {0} must be greater than 0.", Parent.JR_RX_NKSellCurrency));
			}
		}

		#endregion

		#region CheckJR_AC

		protected override void CheckJR_AC()
		{
			base.CheckJR_AC();
			if ((Parent.ChargeCode != null) && !(Parent.IsCostPosted || Parent.IsRevenuePosted))
			{
				ListValidation.ErrorIfInvalidPK(Parent.JR_ACInfo, Parent.Lookups.ChargeCodes);
			}

			CheckAllowedToChange(Parent.JR_ACInfo);

			if (Parent.Job != null && Parent.ChargeCode != null && !Parent.IsInDatabase && Parent.JR_E6.IsEmpty)
			{
				if (Parent.Job.IsReadyForCostPosting && Parent.Job.IsReadyForRevenuePosting)
				{
					if (Parent.ChargeCode.AC_ChargeType != Constants.ChargeType.ManualJobAccrual)
					{
						Parent.JR_ACInfo.AddError(Res.GetString("d700757f-bdef-474e-9988-278a2b18d9be", "Only charge codes of type 'MJA' can be used when the job status is 'Job Ready for Revenue and Cost Posting'"));
					}
				}
				else if (Parent.Job.IsReadyForCostPosting)
				{
					if (Parent.ChargeCode.AC_ChargeType != Constants.ChargeType.Revenue &&
						Parent.ChargeCode.AC_ChargeType != Constants.ChargeType.ManualJobAccrual)
					{
						Parent.JR_ACInfo.AddError(Res.GetString("8e62b69e-a22d-4ce1-8146-89eaf803695c", "Only charge codes of type 'MJA' and 'REV' can be used when the job status is 'Job Ready for Cost Posting'"));
					}
				}
				else if (Parent.Job.IsReadyForRevenuePosting)
				{
					if (Parent.ChargeCode.AC_ChargeType != Constants.ChargeType.ManualJobAccrual)
					{
						Parent.JR_ACInfo.AddError(Res.GetString("18e138c1-fa5d-4f1f-916e-bbaefa00f765", "Only charge codes of type 'MJA' can be used when the job status is 'Job Ready for Revenue Posting'"));
					}
				}
			}

			CheckEmptyStates();
			CheckCommentChargeLine();
			CheckIsAllowedToAddNewCharge();
		}

		void CheckCommentChargeLine()
		{
			if (Parent.IsCommentChargeCode && !Parent.IsRevenuePosted)
			{
				var commentChargeLineRegistryValue = AccountingConfigurationRegistry.Instance.CommentChargeLineARInvoiceWarning.Value;
				if (commentChargeLineRegistryValue == CommentChargeLineARInvoiceWarningOptions.WarningValidation)
				{
					Parent.JR_ACInfo.AddWarning(CommentChargeLineValidationMessage);
				}
				else if (commentChargeLineRegistryValue == CommentChargeLineARInvoiceWarningOptions.ErrorValidation)
				{
					Parent.JR_ACInfo.AddError(CommentChargeLineValidationMessage);
				}
			}
		}

		void CheckIsAllowedToAddNewCharge()
		{
			var charge = Parent as ApportionSplitCharge;
			if (!Parent.IsInDatabase && Parent.JobIsReadyForFinancialClosureWithoutModifySecurity
				&& (charge == null || charge.JR_IsUsedForApportionment)
				&& !Parent.Factory.HasAnyOfContexts(BusinessContext.APInvoiceForm, BusinessContext.APCreditNoteForm))
			{
				Parent.JR_ACInfo.AddError(JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage);
			}
		}

		#endregion

		#region CheckJR_AB

		protected override void CheckJR_AB()
		{
			base.CheckJR_AB();
			if (!Parent.IsCostPosted && !Parent.JR_ABInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(Parent.JR_ABInfo, Parent.BankAccounts);
			}

			CheckAllowedToChange(Parent.JR_ABInfo);

			if (Parent.BankAccount != null)
			{
				if (Parent.BankAccount.AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.EPA && Parent.JR_PaymentType != ReceiptTypes.EPayment)
				{
					Parent.JR_ABInfo.AddError(Res.GetString("26e38ea4-d284-4f33-885c-e473956ab25c", "This bank account is an E-Payment Account. Please set Payment Type to EPA - E-Payment."));
				}
				else if (Parent.BankAccount.AB_AccountType != AccountTypeCodeDescriptionPairList.Codes.EPA && Parent.JR_PaymentType == ReceiptTypes.EPayment)
				{
					Parent.JR_ABInfo.AddError(Res.GetString("02794c08-f6b7-4bdf-8bf1-877b495c333e", "Bank Account is not an E-Payment Account."));
				}
			}
		}

		#endregion

		#region CheckJR_AK

		protected override void CheckJR_AK()
		{
			base.CheckJR_AK();
			if (!Parent.IsCostPosted && !Parent.JR_AKInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(Parent.JR_AKInfo, Parent.ChequeBooks);
			}

			CheckAllowedToChange(Parent.JR_AKInfo);
		}

		#endregion

		#region CheckJR_LocalCostAmt

		protected override void CheckJR_LocalCostAmt()
		{
			base.CheckJR_LocalCostAmt();
			if (!Parent.IsCostPosted && !Parent.IsCostRecognized)
			{
				ZString errorMessage = GetRevenueRecongitionDateDontSetErrorMessage();
				if (errorMessage != "")
				{
					Parent.JR_LocalCostAmtInfo.AddError(errorMessage);
				}
			}

			CheckAllowedToChange(Parent.JR_LocalCostAmtInfo);
		}

		#endregion

		#region CheckJR_LocalSellAmt

		protected override void CheckJR_LocalSellAmt()
		{
			if (!Parent.JR_LocalSellAmtInfo.HasErrors())
			{
				base.CheckJR_LocalSellAmt();
				if (!Parent.IsRevenuePosted && !Parent.IsSellRecognized)
				{
					ZString errorMessage = GetRevenueRecongitionDateDontSetErrorMessage();
					if (errorMessage != "")
					{
						Parent.JR_LocalSellAmtInfo.AddError(errorMessage);
					}
				}

				CheckInvoiceMaxValueOfSplittingRules();

				CheckAllowedToChange(Parent.JR_LocalSellAmtInfo);

				if (Parent.LocalSellAmtOverridenAndUserNeedToCheck)
				{
					Parent.JR_LocalSellAmtInfo.AddWarning(PleaseCheckLocalSellAmountWarning);
				}
			}
		}

		void CheckInvoiceMaxValueOfSplittingRules()
		{
			if (!Parent.IsRevenuePosted && Parent.SellAccount != null && !Parent.JR_LocalSellAmtInfo.HasErrors())
			{
				if (ChargeSplitterByChargeCountAndValue.ShouldThisChargesBeSplitted(Parent))
				{
					Parent.JR_LocalSellAmtInfo.AddError(ChargeSplitterByChargeCountAndValue.GetErrorMessageWhenASingleChargeExceedsMaxValue());
				}
			}
		}

		#endregion

		#region CheckJR_OH_SellAccount

		protected override void CheckJR_OH_SellAccount()
		{
			base.CheckJR_OH_SellAccount();
			if (!Parent.JR_IsRevenuePosted)
			{
				if (AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value && Parent.JR_OH_SellAccount.IsEmpty &&
					AmountCheckForWIPMustHaveDebtorCodeValidation(Parent) && Parent.ShouldCreateWIP) //keep these conditions synchronized with IsDebtorValidWhenWIPMustHaveDebtorCode method
				{
					string error = Res.GetString("cb469e41-e259-4b14-8fdb-f8331ae055ba", "You must enter a debtor. Your system has been configured so that the 'debtor' is mandatory when entering an unposted sell of non zero value.\r\n\r\nThe registry setting that governs this rule is Accounting > Job Costing > WIP Must Have Debtor Code");
					Parent.JR_OH_SellAccountInfo.AddError(error);
				}

				CheckAllowedToChange(Parent.JR_OH_SellAccountInfo);
				if (!Parent.IsCostPosted || !Parent.HasContext(BusinessContext.AutoJobRevenueJournal))
				{
					CheckCostAndSellAccountAreNotBothOrgProxies(Parent.JR_OH_SellAccountInfo);
				}
			}
			CheckInvoiceMaxValueOfSplittingRules();

			var (warnings, _) = ExporterExemptionValidationHelper.CheckExporterExemption(Parent.SellAccount, LedgerTypes.AccountsReceivable, ZDateTime.Now);
			if (!warnings.IsEmpty)
			{
				Parent.JR_OH_SellAccountInfo.AddWarning(warnings);
			}
		}

		#endregion

		#region WIP must have Debtor helper methods

		static bool AmountCheckForWIPMustHaveDebtorCodeValidation(BaseCharge chargeForValidation)
		{
			return chargeForValidation.JR_LocalSellAmt != 0m;
		}

		internal static bool IsDebtorValidWhenWIPMustHaveDebtorCode(BaseCharge chargeForValidation)
		{
			var result = true;
			if (chargeForValidation.ShouldCreateWIP)
			{
				var debtorFilter = new ZQuery(chargeForValidation.Debtors.CompleteFilter);
				debtorFilter.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
				result = chargeForValidation.JR_IsRevenuePosted || !AmountCheckForWIPMustHaveDebtorCodeValidation(chargeForValidation) || chargeForValidation.SellAccount != null && chargeForValidation.SellAccount.MatchesFilter(debtorFilter);
			}
			return result;
		}

		#endregion

		#region CheckJR_GE

		protected override void CheckJR_GE()
		{
			base.CheckJR_GE();
			CheckAllowedToChange(Parent.JR_GEInfo);

			if (!Parent.IsInDatabase && Parent.ShouldValidateBranchAndDepartment && !Parent.IsAllowedToModifyThisCharge)
			{
				Parent.JR_GEInfo.AddError(Res.GetString("4fb3770b-7244-4d06-8113-99731b102711", "You cannot create a new charge against a department for which you do not have login permission."));
			}
		}

		#endregion

		#region CheckJR_DisplaySequence

		#endregion

		#region CheckJR_InvoiceType

		protected override void CheckJR_InvoiceType()
		{
			base.CheckJR_InvoiceType();
			CheckAllowedToChange(Parent.JR_InvoiceTypeInfo);
		}

		#endregion

		#region CheckJR_PaymentTYpe

		protected override void CheckJR_PaymentType()
		{
			base.CheckJR_PaymentType();
			CheckAllowedToChange(Parent.JR_PaymentTypeInfo);
			if (Parent.JR_PaymentType == ReceiptTypes.EPayment)
			{
				Parent.JR_PaymentTypeInfo.AddError(Res.GetString("37b112c5-0e47-42f0-8ffb-401fec083a2d", "To process E-Payments, please create a Payment or Payment Batch in the Payables Transactions module or a Payment Approval in the Payment Processing module."));
			}

			if (Parent.IsCashAccount && Parent.JR_PaymentType != ReceiptTypes.Cash)
			{
				Parent.JR_PaymentTypeInfo.AddError(TransactionHeaderValidation.GetCashAccountTypeErrorMessage(Parent.JR_PaymentTypeInfo.HumanReadableName));
			}
		}

		#endregion

		#region CheckJR_OH_CostAccount

		protected override void CheckJR_OH_CostAccount()
		{
			base.CheckJR_OH_CostAccount();

			if (!Globals.IsWeb && !Parent.HasContext(BusinessContext.ApportionRevenueToShipment) && !Parent.IsCostPosted && AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.Value
				&& Parent.JR_OH_CostAccount.IsEmpty && Parent.JR_LocalCostAmt != 0m && Parent.ShouldCreateAccrual)
			{
				string error = Res.GetString("e713ff9c-ed2c-4b92-9aa4-4c8a5eec6b11", "You must enter a creditor. Your system has been configured so that the 'creditor' is mandatory when entering an unposted cost of non zero value.\r\n\r\nThe registry setting that governs this rule is Accounting > Job Costing > Accrual Must Have Creditor Code");
				Parent.JR_OH_CostAccountInfo.AddError(error);
			}

			CheckAllowedToChange(Parent.JR_OH_CostAccountInfo);

			//Run validation only if charge is not cost posted and is either not revenue posted or is not created with AutoJobRevenueJournal
			if (!Parent.IsCostPosted && !(Parent.IsRevenuePosted && Parent.HasContext(BusinessContext.AutoJobRevenueJournal)))
			{
				CheckCostAndSellAccountAreNotBothOrgProxies(Parent.JR_OH_CostAccountInfo);
			}

			var (warnings, _) = ExporterExemptionValidationHelper.CheckExporterExemption(Parent.CostAccount, LedgerTypes.AccountsPayable, ZDateTime.Now);
			if (!warnings.IsEmpty)
			{
				Parent.JR_OH_CostAccountInfo.AddWarning(warnings);
			}
		}

		void CheckCostAndSellAccountAreNotBothOrgProxies(ZPropertyInfo info)
		{
			if (AutoJRJRegistryStatusHelper.IsAutoJRJEnabled() && !Globals.IsWeb && Parent.CostAndSellAccountAreBothOrgProxies)
			{
				info.AddError(Res.GetString("af26226f-a38f-4c8a-967a-51ea99839872", "You cannot set both the Cost Account and Sell Account to be the organization proxies."));
			}
		}

		#endregion

		#region CheckJR_CostRatingOverrideComment

		protected override void CheckJR_CostRatingOverrideComment()
		{
			base.CheckJR_CostRatingOverrideComment();

			if (Parent.JR_CostRatingOverride)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JR_CostRatingOverrideCommentInfo);
			}

			CheckAllowedToChange(Parent.JR_CostRatingOverrideCommentInfo);
		}

		#endregion

		#region CheckJR_SellRatingOverrideComment

		protected override void CheckJR_SellRatingOverrideComment()
		{
			base.CheckJR_SellRatingOverrideComment();

			if (Parent.JR_SellRatingOverride)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JR_SellRatingOverrideCommentInfo);
			}

			CheckAllowedToChange(Parent.JR_SellRatingOverrideCommentInfo);
		}

		#endregion

		#region CheckJR_JH

		protected override void CheckJR_JH()
		{
			base.CheckJR_JH();
			CheckAllowedToChange(Parent.JR_JHInfo);
		}

		#endregion

		#region CheckJR_JR_RevenueLine

		protected override void CheckJR_JR_RevenueLine()
		{
			base.CheckJR_JR_RevenueLine();
			CheckAllowedToChange(Parent.JR_JR_RevenueLineInfo);
		}

		#endregion

		#region CheckJR_LineType

		protected override void CheckJR_LineType()
		{
			base.CheckJR_LineType();
			CheckAllowedToChange(Parent.JR_LineTypeInfo);
		}

		#endregion

		#region CheckJR_OSCostGSTAmt_Calc

		protected void CheckJR_OSCostGSTAmt_Calc()
		{
			if (!Parent.Factory.HasContext(BusinessContext.WarningOnlyValidation))
			{
				CheckJR_OSCostGSTAmt_CalcErrors();
			}
			CheckJR_OSCostGSTAmt_CalcWarning();
		}

		protected virtual void CheckJR_OSCostGSTAmt_CalcErrors()
		{
			TypeValidation.CheckValidMoney(Parent.JR_OSCostGSTAmt_CalcInfo, 19, 4);

			CheckAllowedToChange(Parent.JR_OSCostGSTAmtInfo, Parent.JR_OSCostGSTAmt_CalcInfo);
		}

		protected virtual void CheckJR_OSCostGSTAmt_CalcWarning()
		{
			if (Parent.JR_IsCostTaxAmountOverridden && !Parent.IsCostPosted && !Parent.IsGSTAmountEqualToCalculatedGST)
			{
				Parent.JR_OSCostGSTAmt_CalcInfo.AddWarning(Res.GetString("7d1bfc6e-9db4-4627-8eea-a1423ea89a6a", "This value is different to default tax amount, make sure that this value is correct for invoice posting."));
			}
		}

		#endregion

		protected override void CheckJR_IsCostTaxAmountOverridden()
		{
			base.CheckJR_IsCostTaxAmountOverridden();

			CheckAllowedToChange(Parent.JR_IsCostTaxAmountOverriddenInfo);
		}

		protected override void CheckJR_CostSupplyType()
		{
			base.CheckJR_CostSupplyType();

			if (!Parent.IsCostPosted && AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
			{
				CheckAllowedToChange(Parent.JR_CostSupplyTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.JR_CostSupplyTypeInfo);

				if (!Parent.JR_CostSupplyTypeInfo.HasErrors() && Parent.JR_CostSupplyType.IsEmpty)
				{
					Parent.JR_CostSupplyTypeInfo.AddWarning(Res.GetString("36F7B3C1-ECC2-4C16-82AD-CBCD762156C9", "The Cost Supply Type is not specified. Please check if a supply type is needed before posting."));
				}
			}
		}

		protected override void CheckJR_SellSupplyType()
		{
			base.CheckJR_SellSupplyType();

			if (!Parent.IsRevenuePosted && AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
			{
				CheckAllowedToChange(Parent.JR_SellSupplyTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.JR_SellSupplyTypeInfo);

				if (!Parent.JR_SellSupplyTypeInfo.HasErrors() && Parent.JR_SellSupplyType.IsEmpty)
				{
					Parent.JR_SellSupplyTypeInfo.AddWarning(Res.GetString("3A6C61E3-8EAC-48C4-91E8-985287E13309", "The Sell Supply Type is not specified. Please check if a supply type is needed before posting."));
				}
			}
		}

		internal static string EmptyRevenueRecognitionTypeErrorMessageForCost
		{
			get { return Res.GetString("31ba3e8f-8262-4310-ba8c-37aea0122142", "Cost revenue recognition type can't be empty. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu."); }
		}

		internal static string EmptyRevenueRecognitionTypeErrorMessageForSell
		{
			get { return Res.GetString("FB1D6F18-A367-4FC3-86DE-D416D02D8790", "Sell revenue recognition type can't be empty. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu."); }
		}

		internal string PleaseCheckLocalSellAmountWarning =>
			Res.GetString("6580cdc4-a49a-4437-b1d1-f449e5f7e784", "Local Sell Amount was recalculated based on the current configuration. Please check that all values on this charge are as expected. Previous value was: {0}", Parent.LocalSellAmtPreviousValue);

		internal bool CheckEmptyRevenueRecognitionTypesForCost()
		{
			return Parent.APLine != null && !Parent.ShouldReverseAccrual && Parent.CostRecognition.IsEmpty;
		}

		internal bool CheckEmptyRevenueRecognitionTypesForSell()
		{
			return Parent.ARLine != null && !Parent.ShouldReverseWIP && Parent.SellRecognition.IsEmpty;
		}

		public sealed override void ValidateAll()
		{
			Parent.ClearRowNotifications();

			ValidateAllCore();
		}

		public void ValidateJR_OSSellGSTAmt_Calc()
		{
			ValidateCalculatedProperty(Parent.JR_OSSellGSTAmt_CalcInfo);
		}

		protected void CheckJR_OSSellGSTAmt_Calc()
		{
			if (!Parent.IsRevenuePosted && Parent.JR_OSSellGSTAmt_Calc != 0 && AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.Value)
			{
				Parent.JR_OSSellGSTAmt_CalcInfo.AddWarning(Res.GetString("01549025-37bb-4e58-ad7b-4262c36f5c76", "This value is not precise and is for reference only. It will be recalculated during posting with higher precision due to Calculate Tax at Header Level rules."));
			}
		}

		protected virtual void ValidateAllCore()
		{
			base.ValidateAll();
			ValidateJR_OSCostGSTAmt_Calc();

			Parent.RemoveRowError(EmptyRevenueRecognitionTypeErrorMessageForCost);
			Parent.RemoveRowError(EmptyRevenueRecognitionTypeErrorMessageForSell);
			if (CheckEmptyRevenueRecognitionTypesForCost())
			{
				Parent.AddRowError(EmptyRevenueRecognitionTypeErrorMessageForCost);
			}
			if (CheckEmptyRevenueRecognitionTypesForSell())
			{
				Parent.AddRowError(EmptyRevenueRecognitionTypeErrorMessageForSell);
			}

			ValidateJR_OSSellGSTAmt_Calc();
			ValidateJR_Calc_InvoiceTarget();
			ValidateJR_Calc_RelatedJobNumber();
		}

		#region Helpers

		/// <returns>
		/// The first charge with the same invoice 'key' (cost account and invoice number) but differing given column as this, or null if there are no duplicates.
		/// </returns>
		protected BaseCharge GetChargeForSameInvoiceWithUnequalColumn(SchemaColumn column)
		{
			return GetChargeForSameInvoiceWithUnequalColumn(column, null);
		}

		protected BaseCharge GetChargeForSameInvoiceWithUnequalColumn(SchemaColumn column, ZQuery additionalFilter)
		{
			BaseCharge result = null;
			if (!(Parent.CostAccount != null && Parent.CostAccount.CompanyData.OB_APCostsSelfBilled))
			{
				ZQuery filter = GetChargesForSameAPTransactionFilter();
				if (additionalFilter != null)
				{
					filter.AddToFilter(additionalFilter);
				}
				BusinessObject[] chargesForAPInvoice = Parent.Factory.Load(typeof(BaseCharge), filter);

				List<BaseCharge> unPostedChargeforAPInvoice = new List<BaseCharge>();
				for (int i = 0; i < chargesForAPInvoice.Length; i++)
				{
					BaseCharge invoiceCharge = (BaseCharge)chargesForAPInvoice[i];
					if (!invoiceCharge.IsCostPosted && !invoiceCharge.IsCommentChargeCode)
					{
						unPostedChargeforAPInvoice.Add(invoiceCharge);
					}
				}

				if (column != null)
				{
					foreach (BaseCharge unpostedCharge in unPostedChargeforAPInvoice)
					{
						if (!unpostedCharge[column.Name].Equals(Parent[column.Name]))
						{
							if (!(column is SchemaDateTimeColumn) || new ZDateTime(unpostedCharge[column.Name]).Date != new ZDateTime(Parent[column.Name]).Date)
							{
								result = unpostedCharge;
							}
						}
					}
				}
				else if (unPostedChargeforAPInvoice.Count > 0)
				{
					result = unPostedChargeforAPInvoice[0];
				}
			}
			return result;
		}

		protected ZQuery GetChargesForSameAPTransactionFilter()
		{
			var filter = new ZQuery(JobChargeSchema.JR_APInvoiceNum, Parent.JR_APInvoiceNum);
			filter.AddToFilter(JobChargeSchema.JR_GC, GlbCompany.CurrentCompany.PK);
			var creditorValue = !Parent.JR_OH_CostAccount.IsValid ? null : (object)Parent.JR_OH_CostAccount;
			filter.AddToFilter(JobChargeSchema.JR_OH_CostAccount, SQLComparisonOperator.Equal, creditorValue);

			return filter;
		}

		void CheckTaxIdAndTaxMessage(string lineType, BaseCharge charge)
		{
			var helper = new TaxIdAndTaxMessageMappingHelper();

			AccTaxRate taxRate = null;
			AccInvMsg taxMsg = null;
			ZPropertyInfo propertyInfo = null;
			INotificationType notificationType = null;

			if (lineType == TransactionLineTypes.Revenue)
			{
				taxRate = charge.SellGSTRate;
				taxMsg = charge.SellVATClass;
				propertyInfo = charge.JR_A9_SellVATClassInfo;
				notificationType = !charge.IsInDatabase && charge.JR_IsApportioned ?
					CargoWise.ComponentModel.NotificationType.Warning :
					CargoWise.ComponentModel.NotificationType.Error;
			}
			else if (lineType == TransactionLineTypes.Cost)
			{
				taxRate = charge.CostGSTRate;
				taxMsg = charge.CostVATClass;
				propertyInfo = charge.JR_A9_CostVATClassInfo;
				notificationType = CargoWise.ComponentModel.NotificationType.Error;
			}

			var message = helper.ValidateMapping(lineType, taxRate, taxMsg);
			if (message != null)
			{
				propertyInfo.Add(notificationType, message);
			}
		}

		ZString GetRevenueRecongitionDateDontSetErrorMessage()
		{
			ZString errorMessage = "";
			if (Parent.CanRecognizeProfitOnWIPsAndAccruals || Parent.ShouldCreateJRJ)
			{
				JobValidation jobValidation = Parent.InvoicingJob.Validation as JobValidation;
				if (jobValidation != null)
				{
					var isDeferredRecognitionSupported = Parent.ShouldCreateJRJ;
					errorMessage = jobValidation.GetRevenueRecognitionDateValidationError(Res.GetString("8E519B37-60DB-4516-80FE-CC8E8B07B2A8", "You cannot save non-zero value charges until the {0:G} is set."), Parent.ChargeCode, isDeferredRecognitionSupported);
				}
			}
			return errorMessage;
		}

		#endregion
	}
}
