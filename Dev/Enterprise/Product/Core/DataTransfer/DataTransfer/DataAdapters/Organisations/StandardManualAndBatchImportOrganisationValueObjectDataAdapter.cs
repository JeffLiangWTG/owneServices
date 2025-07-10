using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class StandardManualAndBatchImportOrganisationValueObjectDataAdapter : StandaloneOrganisationValueObjectDataAdapter
	{
		protected override OrgHeader FindBusinessObject(Xsd.Organisation value, IValueObjectImportContext context)
		{
			OrgHeader result = null;

			switch (OrganisationMatchSetting)
			{
				case OrganisationImportMatchingType.OrganisationCodeMatching:
					result = FindOrganisationByEnterpriseCode(value, context);
					break;
				case OrganisationImportMatchingType.LegacyCodeMatching:
					result = FindOrganisationByLegacyCode(value, context);
					break;
				default:
					result = base.FindBusinessObject(value, context);
					break;
			}

			return result;
		}

		OrgHeader FindOrganisationByEnterpriseCode(Xsd.Organisation value, IValueObjectImportContext context)
		{
			OrgHeader result = null;

			if (value.EDICode.IsEmpty)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("0b215ecf-75f6-4153-a4cb-7d0bca983089", "Organization {0} Code is empty for Organization Name : '{1}'. No data has been imported.", Core.Constants.ProductName, value.OrganisationDetails.Name)));
			}
			else
			{
				result = context.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, value.EDICode);
			}

			return result;
		}

		OrgHeader FindOrganisationByLegacyCode(Xsd.Organisation value, IValueObjectImportContext context)
		{
			OrgHeader result = null;

			Xsd.RegistrationNumber legacySystemCode = value.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(Xsd.RegistrationNumberTypes.LSC, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			if (legacySystemCode == null || legacySystemCode.Number.IsEmpty)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("da91a509-dfd2-4c8f-9f07-c254c3ad2cc5", "Organization Legacy Code is empty for Organization Name : '{0}'. No data has been imported.", value.OrganisationDetails.Name)));
			}
			else
			{
				OrgCusCode orgCusCode = (new OrgCusCode.Loader(context.Factory)).LoadFromLegacyCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, legacySystemCode.Number);
				if (orgCusCode != null)
				{
					result = context.Factory.Load<OrgHeader>(orgCusCode.OK_OH);
				}
			}

			return result;
		}

		protected override void ImportFromValueObjectCore(OrgHeader bizObj, Enterprise.DataTransfer.Xml.XsdVersion1.Organisation valueObj, IValueObjectImportContext context)
		{
			((Xsd.XmlInterchange)context.Interchange).ImportEDICode = (OrganisationMatchSetting == OrganisationImportMatchingType.OrganisationCodeMatching);
			base.ImportFromValueObjectCore(bizObj, valueObj, context);

			if (OrganisationMatchSetting == OrganisationImportMatchingType.LegacyCodeMatching && bizObj.OH_Code.IsEmpty)
			{
				context.Notify(new InfoNotification(Res.GetString("b7469631-aa20-4f2b-880b-21f421a358f9", "ERROR: Organization cannot be imported. Organization Code cannot be generated. Organization Name : '{0}'", bizObj.OH_FullNameTruncated)));
			}
		}

		protected OrganisationImportMatchingType OrganisationMatchSetting
		{
			get { return SystemDataRegistry.Instance.OrganisationMatching; }
		}

		protected override void ImportAddresses(Xsd.OrgAddressCollection address, OrgHeader organisation, IValueObjectImportContext context, ZString errorContext)
		{
			StandardManualAndBatchImportAddressValueObjectHelper addressHelper = new StandardManualAndBatchImportAddressValueObjectHelper(errorContext);
			addressHelper.ImportFromValueObjectCollection(address, organisation, context);
		}

		protected override void ImportAccountRecievable(OrgHeader organisation, Xsd.OrganisationDetail organisationDetail, IValueObjectImportContext context, string errorContext)
		{
			foreach (Xsd.AccountsReceivable receivable in organisationDetail.AccountsReceivables)
			{
				if (receivable.IsSpecified)
				{
					ZString companyCode = GlbCompany.CurrentCompany.GC_Code;
					if (receivable.CompanyCodeSpecified && receivable.CompanyCode != companyCode)
					{
						companyCode = receivable.CompanyCode;
					}

					GlbCompany company = context.Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);

					if (company != null)
					{
						OrgCompanyData companyData = organisation.GetCompanyDataForGlbCompany(company);

						if (companyData != null)
						{
							if (!companyData.OB_IsDebtor)
							{
								companyData.OB_IsDebtor = true;
							}

							context.SetPropertyInfoValue(companyData.OB_RX_NKARDDefltCurrencyInfo, receivable.DefaultCurrency, ForeignKeyType.CurrencyNK);
							context.SetPropertyInfoValueIfValueNotEmpty(companyData.OB_ARCreditLimitInfo, receivable.CreditLimit.ToString());
							context.SetPropertyInfoValueIfValueNotEmpty(companyData.OB_ARExternalDebtorCodeInfo, receivable.ExternalDebtorCode.ToString());
							context.SetPropertyInfoValueIfValueNotEmpty(companyData.OB_ARUseSettlementGroupCreditLimitInfo, receivable.UseSettlementGroupCreditLimit.ToString());
							context.SetPropertyInfoValueIfValueNotEmpty(companyData.OB_ARCreditApprovedInfo, receivable.CreditApproved.ToString());
							context.SetPropertyInfoValueIfValueNotEmpty(companyData.OB_AROnCreditHoldInfo, receivable.CreditOnHold.ToString());

							if (receivable.GSTIsApplicableSpecified)
							{
								companyData.SetARTaxApplicable(receivable.GSTIsApplicable);
							}

							context.SetPropertyInfoValueIfValueNotEmpty(companyData.OB_ARWHTApplicableInfo, receivable.WithholdingTaxIsApplicable.ToString());
							context.SetPropertyInfoValueIfValueNotEmpty(companyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTermInfo, receivable.SettlementDetails.StandardInvoiceTerms);
							context.SetPropertyInfoValueIfValueNotEmpty(companyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDaysInfo, receivable.SettlementDetails.StandardInvoiceDays.ToString());

							if (receivable.SettlementDetails.StandardInvoiceTerms != receivable.SettlementDetails.DisbursementInvoiceTerms ||
								receivable.SettlementDetails.StandardInvoiceDays != receivable.SettlementDetails.DisbursementInvoiceDays)
							{
								OrgARTerms disbursementTerm = companyData.CreateOrLoadDisbursementARTerm();
								context.SetPropertyInfoValueIfValueNotEmpty(disbursementTerm.PY_InvoiceTermInfo, receivable.SettlementDetails.DisbursementInvoiceTerms);
								context.SetPropertyInfoValueIfValueNotEmpty(disbursementTerm.PY_InvoiceDaysInfo, receivable.SettlementDetails.DisbursementInvoiceDays.ToString());
							}

							if (receivable.AccountGroupSpecified)
							{
								OrgDebtorGroup debtorGroup = context.Factory.LoadFromNaturalKey<OrgDebtorGroup>(OrgDebtorGroupSchema.OJ_Code, receivable.AccountGroup);
								companyData.OB_OJ_ARDebtorGroup = debtorGroup != null ? debtorGroup.PK : ZGuid.Empty;
							}
						}
					}
				}
			}
		}

		protected override void ImportAccountPayable(OrgHeader organisation, Xsd.OrganisationDetail organisationDetail, IValueObjectImportContext context, string errorContext)
		{
			foreach (Xsd.AccountsPayable payable in organisationDetail.AccountsPayables)
			{
				if (payable.IsSpecified)
				{
					ZString companyCode = GlbCompany.CurrentCompany.GC_Code;
					if (payable.CompanyCodeSpecified && payable.CompanyCode != companyCode)
					{
						companyCode = payable.CompanyCode;
					}

					GlbCompany company = context.Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);

					if (company != null)
					{
						OrgCompanyData companyData = organisation.GetCompanyDataForGlbCompany(company);

						if (companyData != null)
						{
							if (!companyData.OB_IsCreditor)
							{
								string originalValueForARVATConfig = companyData.OB_ARVATConfig;
								companyData.OB_IsCreditor = true;
								companyData.OB_ARVATConfig = originalValueForARVATConfig;
							}

							context.SetPropertyInfoValue(companyData.OB_RX_NKAPDefltCurrencyInfo, payable.DefaultCurrency, ForeignKeyType.CurrencyNK);
							context.SetPropertyInfoValueIfValueNotEmpty(companyData.OB_APCreditLimitInfo, payable.CreditLimit.ToString());
							context.SetPropertyInfoValueIfValueNotEmpty(companyData.OB_APExternalCreditorCodeInfo, payable.ExternalCreditorCode.ToString());

							if (payable.GSTIsApplicableSpecified)
							{
								companyData.SetAPTaxApplicable(payable.GSTIsApplicable);
							}

							context.SetPropertyInfoValueIfValueNotEmpty(companyData.OB_APWHTApplicableInfo, payable.WithholdingTaxIsApplicable.ToString());
							context.SetPropertyInfoValueIfValueNotEmpty(companyData.OB_APPaymentTermsInfo, payable.PaymentTerms);
							context.SetPropertyInfoValueIfValueNotEmpty(companyData.OB_APPaymentTermDaysInfo, payable.PaymentDays.ToString());

							if (payable.AccountGroupSpecified)
							{
								OrgCreditorGroup creditorGroup = context.Factory.LoadFromNaturalKey<OrgCreditorGroup>(OrgCreditorGroupSchema.OG_Code, payable.AccountGroup);
								companyData.OB_OG_APCreditorGroup = creditorGroup != null ? creditorGroup.PK : ZGuid.Empty;
							}
						}
					}
				}
			}
		}

		protected override void ImportOrganisationTypes(OrgHeader organisation, Xsd.OrganisationDetail organisationDetail, IValueObjectImportContext context, string errorContext)
		{
			if (organisationDetail.OrganisationTypes != null)
			{
				foreach (Xsd.OrganisationDetailOrganisationType orgType in organisationDetail.OrganisationTypes)
				{
					switch (orgType.Value)
					{
						case Xsd.OrganisationTypes.A_P:
							SetOrganisationTypePropertyValue(context, organisation.OH_IsCreditorInfo, orgType);
							break;

						case Xsd.OrganisationTypes.A_R:
							SetOrganisationTypePropertyValue(context, organisation.OH_IsDebtorInfo, orgType);
							break;

						case Xsd.OrganisationTypes.ACT:
							SetOrganisationTypePropertyValue(context, organisation.OH_IsActiveInfo, orgType);
							break;

						case Xsd.OrganisationTypes.BRK:
							SetOrganisationTypePropertyValue(context, organisation.OH_IsBrokerInfo, orgType);
							break;

						case Xsd.OrganisationTypes.CAR:
							SetOrganisationTypePropertyValue(context, organisation.OH_IsShippingProviderInfo, orgType);
							break;

						case Xsd.OrganisationTypes.CNE:
							SetOrganisationTypePropertyValue(context, organisation.OH_IsConsigneeInfo, orgType);
							break;

						case Xsd.OrganisationTypes.CNR:
							SetOrganisationTypePropertyValue(context, organisation.OH_IsConsignorInfo, orgType);
							break;

						case Xsd.OrganisationTypes.COM:
							SetOrganisationTypePropertyValue(context, organisation.OH_IsCompetitorInfo, orgType);
							break;

						case Xsd.OrganisationTypes.FWD:
							SetOrganisationTypePropertyValue(context, organisation.OH_IsForwarderInfo, orgType);
							break;

						case Xsd.OrganisationTypes.GLB:
							SetOrganisationTypePropertyValue(context, organisation.OH_IsGlobalAccountInfo, orgType);
							break;

						case Xsd.OrganisationTypes.NAT:
							SetOrganisationTypePropertyValue(context, organisation.OH_IsNationalAccountInfo, orgType);
							break;

						case Xsd.OrganisationTypes.SAL:
							SetOrganisationTypePropertyValue(context, organisation.OH_IsSalesLeadInfo, orgType);
							break;

						case Xsd.OrganisationTypes.SVS:
							SetOrganisationTypePropertyValue(context, organisation.OH_IsMiscFreightServicesInfo, orgType);
							break;

						case Xsd.OrganisationTypes.TMP:
							SetOrganisationTypePropertyValue(context, organisation.OH_IsTempAccountInfo, orgType);
							break;

						case Xsd.OrganisationTypes.TRN:
							SetOrganisationTypePropertyValue(context, organisation.OH_IsTransportClientInfo, orgType);
							break;

						case Xsd.OrganisationTypes.WHS:
							SetOrganisationTypePropertyValue(context, organisation.OH_IsWarehouseClientInfo, orgType);
							break;
					}
				}
			}
		}

		void SetOrganisationTypePropertyValue(IValueObjectImportContext context, ZPropertyInfo propertyInfo, Xsd.OrganisationDetailOrganisationType orgType)
		{
			context.SetPropertyInfoValueIfValueNotEmpty(propertyInfo, (!orgType.StatusSpecified || orgType.Status).ToString());
		}
	}
}
