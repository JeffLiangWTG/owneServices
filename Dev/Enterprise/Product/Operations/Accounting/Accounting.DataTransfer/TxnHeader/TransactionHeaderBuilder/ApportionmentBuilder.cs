using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.Accounting.Business.GenericConsol;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.Accounting.DataTransfer.XmlMapping;
using Enterprise.Accounting.Integration;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class ApportionmentBuilder
	{
		public ApportionmentBuilder(INotificationManager notifier, TransactionBuilderConfig config)
		{
			this.Notifier = notifier;
			this.Config = config;
		}

		readonly TransactionBuilderConfig Config;

		public void AddApportionmentToInvoiceBusinessObject(InvoicingBase invoice, Xsd.TxnLine xmlInvoiceLine, IValueObjectImportContext context, ZString errorContext)
		{
			InvoicingBase aPInvoice = invoice;

			ZGuid consolPK = GetConsolPK(invoice.Factory, xmlInvoiceLine.ConsolOrJobNo, xmlInvoiceLine.MasterBillNo);
			IJobCostingPlugIn consol = GenericConsol.GetIJobCostingPlugInByPK(invoice.Factory, consolPK);

			if (consol != null)
			{
				JobConsolCost cost = invoice.ConsolCosting.ConsolCosts.AddNew();
				cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.CostSupporter.PK, consol.CostSupporter.Type);

				if (cost.ShipmentsToApportion.Length > 0)
				{
					SetChargeCode(invoice, cost, xmlInvoiceLine, errorContext);
					SetTaxRate(cost, xmlInvoiceLine, errorContext);

					cost.E6_OSCostAmount = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(cost.Factory, xmlInvoiceLine.OsInvoiceAmtExclTax, invoice.GetType());
					if (xmlInvoiceLine.OverrideSystemExchangeRate &&
						xmlInvoiceLine.OsInvoiceAmtExclTax.IsSpecified &&
						xmlInvoiceLine.OsInvoiceAmtExclTax.CurrencyCode != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						cost.E6_LocalCostAmount = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(cost.Factory, xmlInvoiceLine.LocalInvoiceAmtExclTax, invoice.GetType());
					}

					SetTaxAmount(cost, xmlInvoiceLine, errorContext, invoice);
					if (xmlInvoiceLine.ConsolApportionmentMethodSpecified)
					{
						cost.E6_ApportionmentMethod = ApportionmentMethodXmlMapping.Instance.GetEnterpriseCode(xmlInvoiceLine.ConsolApportionmentMethod, "", null);
					}
					cost.SetIsUsedForApportionment();
					invoice.ImportSingleCost(cost, (InvoicingLineBase)invoice.Lines.AddNew());
				}
				else
				{
					invoice.ConsolCosting.ConsolCosts.RemoveAndDelete(cost);
					Notifier.AddErrorToNotifications(Res.GetString("C5BAF574-631B-4113-BA98-510CBB116F48", "Consol '{0}' does not have any shipments to apportion.", consol.JK_UniqueConsignRef));
				}
			}
			else
			{
				Notifier.ReportNoBizObjsFoundWarning(consol as BusinessObject, Res.GetString("ff0da234-9c37-4493-bc36-7e47d7ece5ea", "Consol"), xmlInvoiceLine.ConsolOrJobNo, errorContext);
				TransactionLineBuilder builder = new TransactionLineBuilder(Notifier, Config);
				builder.AddTransactionLineToInvoiceBusinessObject(invoice, xmlInvoiceLine, context, errorContext);
			}
		}

		protected void SetTaxAmount(JobConsolCost cost, Xsd.TxnLine xmlInvoiceLine, string errorContext, InvoicingBase invoice)
		{
			ZDecimal oSTaxAmount = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(cost.Factory, xmlInvoiceLine.OsTaxAmount, invoice.GetType());

			if (oSTaxAmount != 0)
			{
				if (cost.TaxRate == null)
				{
					Notifier.AddErrorToNotifications(Res.GetString("0D8A9859-5AC2-4717-850B-F4BFEF3A7441", "Consol Cost Tax Amount cannot be set if there is no Tax Code"));
				}
				else if (cost.TaxRate.GetRate(cost.E6_TaxDate) == 0)
				{
					Notifier.AddErrorToNotifications(Res.GetString("C8E1B234-FB5E-4808-A8D8-BD32A9BAF33B", "Consol Cost Tax Amount cannot be set if Tax Rate is zero"));
				}
				else
				{
					cost.E6_OSGSTAmount_Calc = oSTaxAmount;
				}
			}
		}

		protected virtual OrgHeader GetOrganisationForChargeCodeMapping(InvoicingBase invoice)
		{
			return invoice.Header;
		}

		protected virtual ZString GetXMLChargeCodeValue(Xsd.TxnLine xmlInvoiceLine)
		{
			return xmlInvoiceLine.ChargeCode;
		}

		protected void SetChargeCode(InvoicingBase invoice, JobConsolCost cost, Xsd.TxnLine xmlInvoiceLine, ZString errorContext)
		{
			if (!xmlInvoiceLine.ChargeCode.IsEmpty)
			{
				OrgHeader org = GetOrganisationForChargeCodeMapping(invoice);
				if (org != null)
				{
					ZDBOnlySubQuery globalChargeCodeQuery;

					ZDBOnlySubQuery branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_GC);
					branchSubQuery.AddToFilter(GlbBranchSchema.GB_OH_OrgProxy, org.PK);

					ZDBOnlyQuery aRCompanyQuery = new ZDBOnlyQuery(typeof(GlbCompany));
					aRCompanyQuery.AddToFilter(GlbCompanySchema.GC_OH_OrgProxy, org.PK);
					aRCompanyQuery.AddSubQuery(GlbCompanySchema.PK, branchSubQuery, JoinCondition.Or);

					GlbCompany aRCompany = invoice.Factory.LoadTop1<GlbCompany>(aRCompanyQuery);
					if (aRCompany != null)
					{
						ZDBOnlySubQuery chargeCodeSubQuery = new ZDBOnlySubQuery(typeof(AccChargeCode), AccChargeCodeSchema.PK);
						chargeCodeSubQuery.AddToFilter(AccChargeCodeSchema.AC_Code, GetXMLChargeCodeValue(xmlInvoiceLine));
						chargeCodeSubQuery.AddToFilter(AccChargeCodeSchema.AC_GC, aRCompany.PK);

						ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(GlobalChargeCodeMapPivot), AccGlobalChargeCodeMapPivotSchema.YP_YG);
						pivotSubQuery.AddToFilter(AccGlobalChargeCodeMapPivotSchema.YP_TYPE, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
						pivotSubQuery.AddSubQuery(AccGlobalChargeCodeMapPivotSchema.YP_AC, chargeCodeSubQuery, JoinCondition.And);

						globalChargeCodeQuery = new ZDBOnlySubQuery(typeof(GlobalChargeCodeMap), AccGlobalChargeCodeMapSchema.PK);
						globalChargeCodeQuery.AddToFilter(AccGlobalChargeCodeMapSchema.YG_OH, null);
						globalChargeCodeQuery.AddToFilter(AccGlobalChargeCodeMapSchema.YG_IsActive, true);
						globalChargeCodeQuery.AddSubQuery(AccGlobalChargeCodeMapSchema.PK, pivotSubQuery, JoinCondition.And);
					}
					else
					{
						globalChargeCodeQuery = new ZDBOnlySubQuery(typeof(GlobalChargeCodeMap), AccGlobalChargeCodeMapSchema.PK);
						globalChargeCodeQuery.AddToFilter(AccGlobalChargeCodeMapSchema.YG_OH, org.PK);
						globalChargeCodeQuery.AddToFilter(AccGlobalChargeCodeMapSchema.YG_Code, GetXMLChargeCodeValue(xmlInvoiceLine));
						globalChargeCodeQuery.AddToFilter(AccGlobalChargeCodeMapSchema.YG_IsActive, true);
					}

					ZDBOnlySubQuery pivotQuery = new ZDBOnlySubQuery(typeof(GlobalChargeCodeMapPivot), AccGlobalChargeCodeMapPivotSchema.YP_AC);
					pivotQuery.AddToFilter(AccGlobalChargeCodeMapPivotSchema.YP_TYPE, ZArchitecture.Core.LedgerTypes.AccountsPayable);
					pivotQuery.AddSubQuery(AccGlobalChargeCodeMapPivotSchema.YP_YG, globalChargeCodeQuery, JoinCondition.And);

					ZDBOnlySubQuery chargeCodeQuery = new ZDBOnlySubQuery(typeof(AccChargeCode), AccChargeCodeSchema.AC_Code);
					chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
					chargeCodeQuery.AddSubQuery(AccChargeCodeSchema.PK, pivotQuery, JoinCondition.And);

					ZDBOnlyQuery genericChargeCodeQuery = new ZDBOnlyQuery(typeof(GenericCharge));
					genericChargeCodeQuery.AddToFilter(ViewGenericChargeSchema.VC_GC, GlbCompany.CurrentCompany.PK);
					genericChargeCodeQuery.AddToFilter(ViewGenericChargeSchema.VC_IsGLAccount, false);
					genericChargeCodeQuery.AddSubQuery(ViewGenericChargeSchema.VC_Code, chargeCodeQuery, JoinCondition.And);

					GenericCharge genericChargeCode = invoice.Factory.LoadTop1<GenericCharge>(genericChargeCodeQuery);
					if (genericChargeCode != null)
					{
						cost.E6_AC_ChargeCode = genericChargeCode.PK;
					}
				}
				if (cost.ChargeCode == null)
				{
					AccChargeCode patternMatchCode = null;
					ZQuery patternMatchFilter = new ZQuery();
					if (org != null)
					{
						patternMatchFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, org.PK);
						patternMatchFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.ChargeCodes);
						patternMatchFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, GetXMLChargeCodeValue(xmlInvoiceLine));
					}
					else
					{
						patternMatchFilter.IsNoResultQuery = true;
					}

					OrgPatternMatchOverride[] matches = invoice.Factory.Load<OrgPatternMatchOverride>(patternMatchFilter);
					List<ZString> localCodes = new List<ZString>();
					foreach (OrgPatternMatchOverride match in matches)
					{
						if (!localCodes.Contains(match.OO_LocalCode))
						{
							localCodes.Add(match.OO_LocalCode);
						}
					}

					ZQuery chargecodeFilter = new ZQuery(AccChargeCodeSchema.AC_Code, localCodes.ToArray());
					chargecodeFilter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
					patternMatchCode = invoice.Factory.LoadTop1<AccChargeCode>(chargecodeFilter);

					if (patternMatchCode != null)
					{
						cost.E6_AC_ChargeCode = patternMatchCode.PK;
					}
					else
					{
						ZQuery chargeCodeFilter = new ZQuery(ViewGenericChargeSchema.VC_Code, xmlInvoiceLine.ChargeCode);
						chargeCodeFilter.AddToFilter(ViewGenericChargeSchema.VC_GC, GlbCompany.CurrentCompany.PK);
						chargeCodeFilter.AddToFilter(ViewGenericChargeSchema.VC_IsGLAccount, ZBool.False);

						GenericCharge chargeCode = invoice.Factory.LoadTop1<GenericCharge>(chargeCodeFilter);

						if (chargeCode != null)
						{
							cost.E6_AC_ChargeCode = chargeCode.PK;
						}
						else
						{
							Notifier.ReportNoBizObjsFoundError(Res.GetString("5047fbf1-72f1-48f1-9c56-fcf7cf53a398", "Charge Code"), xmlInvoiceLine.ChargeCode, 0, errorContext);
						}
					}
				}
			}
		}

		protected void SetTaxRate(JobConsolCost cost, Xsd.TxnLine xmlInvoiceLine, string errorContext)
		{
			if (!xmlInvoiceLine.TaxCode.IsEmpty)
			{
				ZQuery taxRateFilter = new ZQuery(AccTaxRateSchema.AT_Code, xmlInvoiceLine.TaxCode);
				taxRateFilter.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AccTaxRate taxRate = cost.Factory.LoadTop1<AccTaxRate>(taxRateFilter);

				int numberOfTaxRates = cost.Factory.GetDatabaseCount(typeof(AccTaxRate), taxRateFilter);
				Notifier.ReportNoBizObjsFoundError(Res.GetString("341f51cf-98ec-4181-9c80-f070e3db658e", "Tax Rate"), xmlInvoiceLine.TaxCode, numberOfTaxRates, errorContext);

				if (taxRate != null)
				{
					cost.E6_AT_TaxRate = taxRate.PK;
				}
			}
		}

		public ZGuid GetConsolPK(BusinessObjectFactory factory, ZString consolNumber, ZString masterBillNumber)
		{
			IJobCostingPlugIn consol = null;

			if (!consolNumber.IsEmpty)
			{
				int count = GenericConsol.GetCountofIJobCostingPlugInByPrimaryCode(factory, consolNumber);
				if (count == 1)
				{
					consol = GenericConsol.GetIJobCostingPlugInByPrimaryCode(factory, consolNumber);
				}
				else if (count > 1)
				{
					ZString errorMessage = Res.GetString("4a6111ce-32e5-40a3-8832-89113c8c646c", "More than one Consol was found with the following Consol Number: {0}.", consolNumber) + "\r\n";
					Notifier.AddWarningToNotifications(errorMessage);
				}
			}

			if (consol == null && !masterBillNumber.IsEmpty)
			{
				int count = GenericConsol.GetCountofIJobCostingPlugInBySecondaryCode(factory, masterBillNumber);
				if (count == 1)
				{
					consol = GenericConsol.GetIJobCostingPlugInBySecondaryCode(factory, masterBillNumber);
				}
				else if (count > 1)
				{
					ZString errorMessage = Res.GetString("f4a8560b-1b81-4425-9981-e06936c85cec", "More than one Consol was found with the following Master Bill Number: {0}.", masterBillNumber) + "\r\n";
					Notifier.AddWarningToNotifications(errorMessage);
				}
			}

			return (consol != null) ? consol.CostSupporter.PK : ZGuid.Empty;
		}

		readonly INotificationManager Notifier;
	}
}
