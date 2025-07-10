using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocMiscServ : DocumentWrapper
	{
		DocMiscServ(OrgMiscServ orgMiscServ, BusinessObjectFactory factoryToWrap)
				: base(orgMiscServ, factoryToWrap)
		{
		}

		public static DocMiscServ New(OrgMiscServ orgMiscServ, BusinessObjectFactory factoryToWrap)
		{
			return (orgMiscServ != null) ? new DocMiscServ(orgMiscServ, factoryToWrap) : null;
		}

		OrgMiscServ OrgMiscServ
		{
			get { return (OrgMiscServ)WrappedObject; }
		}

		public override string ToString()
		{
			return (Organisation != null) ? Organisation.ToString() : string.Empty;
		}

		#region Misc Serv Fields

		public Image TrademarkLogo
		{
			get
			{
				Image result = null;
				if (ClientDocumentLogo.Length > 0)
				{
					try
					{
						MemoryStream stream = new MemoryStream(ClientDocumentLogo);
						result = Image.FromStream(stream);
					}
					catch (Exception exception)
					{
						if (exception.IsCriticalException()) { throw; }
						result = null;
					}
				}
				return result;
			}
		}

		public ZBlob ClientDocumentLogo
		{
			get { return OrgMiscServ.ClientDocumentLogo; }
		}

		public ZBool IsImportAirUpliftValid
		{
			get { return OrgMiscServ.IsImportAirUpliftValid; }
		}

		public ZBool IsExportAirUpliftValid
		{
			get { return OrgMiscServ.IsExportAirUpliftValid; }
		}

		public ZBool IsImportSeaUpliftValid
		{
			get { return OrgMiscServ.IsImportSeaUpliftValid; }
		}

		public ZBool IsExportSeaUpliftValid
		{
			get { return OrgMiscServ.IsExportSeaUpliftValid; }
		}

		public DocBankAccount BankAccount
		{
			get { return OrgMiscServ.OM_AB_APDefaultBankAccount.IsValid ? DocBankAccount.New(Factory, OrgMiscServ.OM_AB_APDefaultBankAccount) : null; }
		}

		public DocChargeCode ChargeCode
		{
			get { return OrgMiscServ.OM_AC_APDefaultChargeCode.IsValid ? DocChargeCode.New(Factory, OrgMiscServ.OM_AC_APDefaultChargeCode) : null; }
		}

		public ZString Airline3CharCode
		{
			get { return OrgMiscServ.Airline?.RM_EagleAddedAirlinePrefixOrAccountingCode ?? null; }
		}

		public ZString APCategory
		{
			get { return OrgMiscServ.OM_APCategory; }
		}

		public ZDecimal APCreditLimit
		{
			get { return OrgMiscServ.OM_APCreditLimit; }
		}

		public ZBool APPayInvoiceAfterPostingDefault
		{
			get { return OrgMiscServ.OM_APPayInvoiceAfterPostingDefault; }
		}

		public ZByte APPaymentTermDays
		{
			get { return OrgMiscServ.Header.CompanyData.GetAPTerm().Days; }
		}

		public ZString APPaymentTerms
		{
			get { return OrgMiscServ.Header.CompanyData.GetAPTerm().Term; }
		}

		public ZBool APTaxApplicable
		{
			get { return OrgMiscServ.Header.CompanyData.IsAPTaxApplicable; }
		}

		public ZBool APWHTApplicable
		{
			get { return OrgMiscServ.OM_APWHTApplicable; }
		}

		public ZBool ARAutoUpdateRates
		{
			get { return OrgMiscServ.OM_ARAutoUpdateRates; }
		}

		public ZString ARCategory
		{
			get { return OrgMiscServ.OM_ARCategory; }
		}

		public ZBool ARCombinedStatementInvoice
		{
			get { return OrgMiscServ.OM_ARCombinedStatementInvoice; }
		}

		public ZString ARConsolidatedAccountingCategory
		{
			get { return OrgMiscServ.OM_ARConsolidatedAccountingCategory; }
		}

		public ZDecimal ARCreditLimit
		{
			get { return OrgMiscServ.OM_ARCreditLimit; }
		}

		public ZBool ARDontShowTaxOnDocs
		{
			get { return OrgMiscServ.OM_ARDontShowTaxOnDocs; }
		}

		public ZDecimal ARExportAirCollectUplift
		{
			get { return OrgMiscServ.Header.CompanyData.AccCFXConfigurations.GetRecord("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air)?.JCF_CFXPercentage ?? 0m; }
		}

		public ZDecimal ARExportSeaCollectUplift
		{
			get { return OrgMiscServ.Header.CompanyData.AccCFXConfigurations.GetRecord("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea)?.JCF_CFXPercentage ?? 0m; }
		}

		public ZByte ARGlobalRateBase
		{
			get { return (byte)OrgMiscServ.Header.CompanyData.RateTariffLevels.DefaultLevel; }
		}

		public ZDecimal ARImportAirCollectUplift
		{
			get { return OrgMiscServ.Header.CompanyData.AccCFXConfigurations.GetRecord("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air)?.JCF_CFXPercentage ?? 0m; }
		}

		public ZDecimal ARImportSeaCollectUplift
		{
			get { return OrgMiscServ.Header.CompanyData.AccCFXConfigurations.GetRecord("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea)?.JCF_CFXPercentage ?? 0m; }
		}

		public ZString ARCreditRating
		{
			get { return OrgMiscServ.Header.CompanyData.OB_ARCreditRating; }
		}

		public ZString ARCreditRateWithDescription
		{
			get
			{
				if (!ARCreditRating.IsEmpty)
				{
					return ARCreditRating + " - " + OrgMiscServ.Header.CompanyData.Lookups.OB_ARCreditRating_List.GetDescriptionFromCode(ARCreditRating);
				}
				return ZString.Empty;
			}
		}

		public ZDateTime ARAccountAndCreditReviewDue
		{
			get { return OrgMiscServ.Header.CompanyData.OB_ARAccountAndCreditReviewDue; }
		}

		public ZBool AROnCreditHold
		{
			get { return OrgMiscServ.OM_AROnCreditHold; }
		}

		public ZString ARPreviousChequeDrawer
		{
			get { return OrgMiscServ.OM_ARPreviousChequeDrawer; }
		}

		public ZString ARPreviousChequeDrawerBank
		{
			get { return OrgMiscServ.OM_ARPreviousChequeDrawerBank; }
		}

		public ZString ARPreviousChequeDrawerBankBranch
		{
			get { return OrgMiscServ.OM_ARPreviousChequeDrawerBankBranch; }
		}

		public ZBool ARReceiptInvoiceAfterPostingDefault
		{
			get { return OrgMiscServ.OM_ARReceiptInvoiceAfterPostingDefault; }
		}

		public ZBool ARTaxApplicable
		{
			get { return OrgMiscServ.Header.CompanyData.IsARTaxApplicable; }
		}

		public ZDecimal ARTreatDisbursementsAsStandardValue
		{
			get { return OrgMiscServ.OM_ARTreatDisbursementsAsStandardValue; }
		}

		public ZBool ARWHTApplicable
		{
			get { return OrgMiscServ.OM_ARWHTApplicable; }
		}

		public ZString ARGlobalCreditGroup
		{
			get
			{
				var result = string.Empty;

				if (OrgMiscServ.ARGlobalCreditGroup != null || OrgMiscServ.OM_ARGlobalCreditApproved)
				{
					var header = OrgMiscServ.ARGlobalCreditGroup ?? OrgMiscServ.Header;
					result = string.Format(CultureInfo.InvariantCulture, "{0} {1}", header.OH_Code, header.OH_FullName);
				}

				return result;
			}
		}

		public ZDecimal ARGlobalCreditLimit => OrgMiscServ.ARGlobalCreditLimit;

		public ZString ARGlobalCreditCurrency => OrgMiscServ.ARGlobalCreditCurrencyCode;

		public ZString CreditTerms
		{
			get
			{
				if (OrgMiscServ.Header.CompanyData == null)
				{
					return ZString.Empty;
				}

				if (!OrgMiscServ.Header.CompanyData.TryToGetTheOnlyOrgARTerm(out var standardARTerm, false))
				{
					return MultipleCreditTermText;
				}

				var result = ZString.Empty;
				var termList = new ARInvoiceTermsList();
				if (standardARTerm.Term != string.Empty)
				{
					if (!standardARTerm.IsTermWithoutDays)
					{
						result = standardARTerm.Days.ToString() + (standardARTerm.Term != InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Code ? (standardARTerm.IsTermWithMonths ? " MONTHS " : " DAYS ") : " ");
					}
					result += standardARTerm.TermDescription.Trim().ToUpper();
				}
				else if (termList.GetDescriptionFromCode(Constants.InvoiceTerms.CashOnDelivery) != null)
				{
					result += termList.GetDescriptionFromCode(Constants.InvoiceTerms.CashOnDelivery).Trim().ToUpper();
				}

				return result;
			}
		}

		public ZString DisbursementCreditTerms
		{
			get
			{
				ZString result = ZString.Empty;

				if (OrgMiscServ.Header.CompanyData != null)
				{
					InvoiceTerm disbursementARTerm;
					if (OrgMiscServ.Header.CompanyData.TryToGetTheOnlyOrgARTerm(out disbursementARTerm, true))
					{
						CodeDescriptionPairList termList = new ARInvoiceTermsList();
						if (disbursementARTerm.Term != string.Empty)
						{
							if (!disbursementARTerm.IsTermWithoutDays)
							{
								result = disbursementARTerm.Days.ToString() + (disbursementARTerm.Term != InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Code ? (disbursementARTerm.IsTermWithMonths ? " MONTHS " : " DAYS ") : " ");
							}
							result += disbursementARTerm.TermDescription.Trim().ToUpper();
						}
						else if (termList.GetDescriptionFromCode(Constants.InvoiceTerms.CashOnDelivery) != null)
						{
							result += termList.GetDescriptionFromCode(Constants.InvoiceTerms.CashOnDelivery).Trim().ToUpper();
						}
					}
					else
					{
						result = MultipleCreditTermText;
					}
				}

				return result;
			}
		}

		public MultilingualString ShortenedCreditTerms
		{
			get
			{
				MultilingualString result = (NoResString)ZString.Empty;
				if (OrgMiscServ.Header.CompanyData != null)
				{
					InvoiceTerm standardARTerm;
					if (OrgMiscServ.Header.CompanyData.TryToGetTheOnlyOrgARTerm(out standardARTerm, false))
					{
						CodeDescriptionPairList termList = new InvoiceTermsListWithShortDescription();
						if (!standardARTerm.Term.IsEmpty)
						{
							string invoiceTermDescription = termList.GetMultilingualDescriptionFromCode(standardARTerm.Term);
							if (invoiceTermDescription != null)
							{
								result = (NoResString)string.Format(invoiceTermDescription.Trim(), standardARTerm.Days.ToString());
							}
							else
							{
								result = (NoResString)(standardARTerm.Days.ToString() + (standardARTerm.IsTermWithMonths ? " MONTHS " : " DAYS "));
							}
						}
						else
						{
							string invoiceTermDescription = termList.GetMultilingualDescriptionFromCode(InvoiceTermWithShortDescription.CashOnDelivery.Code);
							if (invoiceTermDescription != null)
							{
								result = (NoResString)invoiceTermDescription.Trim().ToUpper();
							}
						}
					}
					else
					{
						result = (NoResString)MultipleCreditTermText;
					}
				}
				return result;
			}
		}

		public MultilingualString ShortenedDisbursementCreditTerms
		{
			get
			{
				MultilingualString result = (NoResString)ZString.Empty;

				InvoiceTerm disbursementARTerm;
				if (OrgMiscServ.Header.CompanyData != null)
				{
					if (OrgMiscServ.Header.CompanyData.TryToGetTheOnlyOrgARTerm(out disbursementARTerm, true))
					{
						CodeDescriptionPairList termList = new InvoiceTermsListWithShortDescription();
						if (disbursementARTerm.Term != string.Empty)
						{
							string invoiceTermDescription = termList.GetMultilingualDescriptionFromCode(disbursementARTerm.Term);
							if (invoiceTermDescription != null)
							{
								result = (NoResString)string.Format(invoiceTermDescription.Trim(), disbursementARTerm.Days.ToString());
							}
							else
							{
								result = (NoResString)(disbursementARTerm.Days.ToString() + (disbursementARTerm.IsTermWithMonths ? " MONTHS " : " DAYS "));
							}
						}
						else if (termList.GetDescriptionFromCode(Constants.InvoiceTerms.CashOnDelivery) != null)
						{
							string invoiceTermDescription = termList.GetMultilingualDescriptionFromCode(InvoiceTermWithShortDescription.CashOnDelivery.Code);
							if (invoiceTermDescription != null)
							{
								result = (NoResString)invoiceTermDescription.Trim().ToUpper();
							}
						}
					}
					else
					{
						result = (NoResString)MultipleCreditTermText;
					}
				}

				return result;
			}
		}

		ZString MultipleCreditTermText
		{
			get { return Res.GetString("04cbc6c9-e20e-4a3d-804f-6fc9e458c28e", "Multiple – As per Invoice"); }
		}

		public ZDecimal CICapitalEmployed
		{
			get { return OrgMiscServ.OM_CICapitalEmployed; }
		}

		public ZByte CICompetitiveRanking
		{
			get { return OrgMiscServ.OM_CICompetitiveRanking; }
		}

		public ZString CICompetitorCategory
		{
			get { return OrgMiscServ.OM_CICompetitorCategory; }
		}

		public ZShort CIEstimatedStaffThisCountry
		{
			get { return OrgMiscServ.OM_CIEstimatedStaffThisCountry; }
		}

		public ZShort CIEstimatedStaffThisLocation
		{
			get { return OrgMiscServ.OM_CIEstimatedStaffThisLocation; }
		}

		public ZString CIOpportunities
		{
			get { return OrgMiscServ.OM_CIOpportunities; }
		}

		public ZDecimal CIProfit
		{
			get { return OrgMiscServ.OM_CIProfit; }
		}

		public ZString CISellingStyle
		{
			get { return OrgMiscServ.OM_CISellingStyle; }
		}

		public ZString CIStrength
		{
			get { return OrgMiscServ.OM_CIStrength; }
		}

		public ZString CIThreats
		{
			get { return OrgMiscServ.OM_CIThreats; }
		}

		public ZDecimal CITurnover
		{
			get { return OrgMiscServ.OM_CITurnover; }
		}

		public ZString CITypeOfService
		{
			get { return OrgMiscServ.OM_CITypeOfService; }
		}

		public ZString CIWeaknesses
		{
			get { return OrgMiscServ.OM_CIWeaknesses; }
		}

		#region Sales Manager

		public ZDecimal CMAcheivableClientRevenue
		{
			get { return OrgMiscServ.OM_CMAcheivableClientRevenue; }
		}

		public ZDecimal CMConsultingRevenue
		{
			get { return OrgMiscServ.OM_CMConsultingRevenue; }
		}

		public ZDecimal CMPaidUpCapital
		{
			get { return OrgMiscServ.OM_CMPaidUpCapital; }
		}

		public ZByte CMAmountOfBusinessWon
		{
			get { return OrgMiscServ.OM_CMAmountOfBusinessWon; }
		}

		public ZByte CMAmountOfElectronicIntegration
		{
			get { return OrgMiscServ.OM_CMAmountOfElectronicIntegration; }
		}

		public ZDateTime CMClientCommenced
		{
			get { return OrgMiscServ.OM_CMClientCommenced; }
		}

		public ZInt CMNoOfEmployees
		{
			get { return OrgMiscServ.OM_CMNoOfEmployees; }
		}

		public ZDateTime CMEstablishedDate
		{
			get { return OrgMiscServ.OM_CMEstablishedDate; }
		}

		public ZByte CMClientsDesireToRemain
		{
			get { return OrgMiscServ.OM_CMClientsDesireToRemain; }
		}

		public ZString CMClientSize
		{
			get { return OrgMiscServ.OM_CMClientSize; }
		}

		public ZString CMCompetitorActivity
		{
			get { return OrgMiscServ.OM_CMCompetitorActivity; }
		}

		public ZString CMCompetitorActivityWithDescription
		{
			get
			{
				if (!CMCompetitorActivity.IsEmpty)
				{
					return CMCompetitorActivity + " - " + OrgMiscServ.OM_CMCompetitorActivity_List.GetDescriptionFromCode(CMCompetitorActivity);
				}
				return ZString.Empty;
			}
		}

		public ZByte CMEaseClientCanBePoached
		{
			get { return OrgMiscServ.OM_CMEaseClientCanBePoached; }
		}

		public ZDateTime CMEstimatedDateToClose
		{
			get { return OrgMiscServ.OM_CMEstimatedDateToClose; }
		}

		public ZDecimal CMEstimatedProfit
		{
			get { return OrgMiscServ.OM_CMEstimatedProfit; }
		}

		public ZDateTime CMFollowUpDate
		{
			get { return OrgMiscServ.FollowUpDate; }
		}

		public ZDateTime CMFollowUpDateLocal
		{
			get { return OrgMiscServ.FollowUpDateLocal; }
		}

		public ZString CMGrowthOutlook
		{
			get { return OrgMiscServ.OM_CMGrowthOutlook; }
		}

		public ZString CMGrowthOutlookWithDescription
		{
			get
			{
				if (!CMGrowthOutlook.IsEmpty)
				{
					return CMGrowthOutlook + " - " + OrgMiscServ.OM_CMGrowthOutlook_List.GetDescriptionFromCode(CMGrowthOutlook);
				}
				return ZString.Empty;
			}
		}

		public ZDateTime CMLastCallDate
		{
			get { return OrgMiscServ.OM_CMLastCallDate; }
		}

		public ZByte CMOverallClientRelation
		{
			get { return OrgMiscServ.OM_CMOverallClientRelation; }
		}

		public ZString CMOverallEffectOfClientOnAirfreightCosts
		{
			get { return OrgMiscServ.OM_CMOverallEffectOfClientOnAirfreightCosts; }
		}

		public ZString CMOverallEffectOfClientOnLCLCosts
		{
			get { return OrgMiscServ.OM_CMOverallEffectOfClientOnLCLCosts; }
		}

		public ZString CMOverallEffectOfClientOnOtherCosts
		{
			get { return OrgMiscServ.OM_CMOverallEffectOfClientOnOtherCosts; }
		}

		public ZString CMOverallEffectOfClientOnTEUCosts
		{
			get { return OrgMiscServ.OM_CMOverallEffectOfClientOnTEUCosts; }
		}

		public ZString CMOverallEffectOfClientOnWarehousingCosts
		{
			get { return OrgMiscServ.OM_CMOverallEffectOfClientOnWarehousingCosts; }
		}

		public ZDecimal CMPercentage
		{
			get { return OrgMiscServ.OM_CMPercentage; }
		}

		public ZString CMSalesCategory
		{
			get { return OrgMiscServ.OM_CMSalesCategory; }
		}

		public ZString CMSalesCategoryWithDescription
		{
			get
			{
				if (!CMSalesCategory.IsEmpty)
				{
					return CMSalesCategory + " - " + OrgMiscServ.OM_CMSalesCategory_List.GetDescriptionFromCode(CMSalesCategory);
				}
				return ZString.Empty;
			}
		}

		public ZString CMSalesTerritory
		{
			get { return OrgMiscServ.OM_CMSalesTerritory; }
		}

		public ZString CMSalesTerritoryWithDescription
		{
			get
			{
				if (!CMSalesTerritory.IsEmpty)
				{
					return CMSalesTerritory + " - " + OrgMiscServ.OM_CMSalesTerritory_List.GetDescriptionFromCode(CMSalesTerritory);
				}
				return ZString.Empty;
			}
		}

		public ZDecimal CMTotalClientRevenue
		{
			get { return OrgMiscServ.OM_CMTotalClientRevenue; }
		}

		public ZDecimal CMWarehouseRevenue
		{
			get { return OrgMiscServ.OM_CMWarehouseRevenue; }
		}

		public ZBool CMDoesImports
		{
			get { return OrgMiscServ.OM_CMDoesImports; }
		}

		public ZBool CMDoesExports
		{
			get { return OrgMiscServ.OM_CMDoesExports; }
		}

		public ZString CMImportCommodity
		{
			get { return OrgMiscServ.CMMainImportCmdty != null ? OrgMiscServ.CMMainImportCmdty.RH_DescriptionMultilingual : ZString.Empty; }
		}

		public ZString CMExportCommodity
		{
			get { return OrgMiscServ.CMMainExportCmdty != null ? OrgMiscServ.CMMainExportCmdty.RH_DescriptionMultilingual : ZString.Empty; }
		}

		public DocSalesCallsCollection SalesCalls
		{
			get
			{
				DocSalesCallsCollection result;
				if (OrgMiscServ != null && OrgMiscServ.Header != null)
				{
					result = new DocSalesCallsCollection(Factory, OrgMiscServ.Header.SalesCalls);
					result.Sort("CallDate", ListSortDirection.Descending);
				}
				else
				{
					result = new DocSalesCallsCollection(Factory);
				}

				return result;
			}
		}

		public DocSalesTradeLanesCollection SalesTradeLanes
		{
			get
			{
				DocSalesTradeLanesCollection result;
				if (OrgMiscServ?.Header != null)
				{
					result = new DocSalesTradeLanesCollection(Factory, OrgMiscServ.Header);
					result.Load();
					result.Sort("Summary", ListSortDirection.Ascending);
				}
				else
				{
					result = new DocSalesTradeLanesCollection(Factory);
				}

				return result;
			}
		}

		#endregion

		public ZString EXDefaultIncoTerm
		{
			get { return OrgMiscServ.OM_EXDefaultIncoTerm; }
		}

		public ZString EXExporterCategory
		{
			get { return OrgMiscServ.OM_EXExporterCategory; }
		}

		public ZBool FWAgentBelongsToGroup
		{
			get { return OrgMiscServ.OM_FWAgentBelongsToGroup; }
		}

		public ZString FWAgentCategory
		{
			get { return OrgMiscServ.OM_FWAgentCategory; }
		}

		public ZBool FWBillCollectFeesOnSingleInvoice
		{
			get { return OrgMiscServ.OM_FWBillCollectFeesOnSingleInvoice; }
		}

		public ZBool FWDealDirectlyWithUltimates
		{
			get { return OrgMiscServ.OM_FWDealDirectlyWithUltimates; }
		}

		public ZBool FWRequestForCreditAllowed
		{
			get { return OrgMiscServ.OM_FWRequestForCreditAllowed; }
		}

		public DocStaff ExportAirRep
		{
			get { return DocStaff.New(OrgMiscServ.Header.StaffAssignments.ExportAirRepStaff, Factory); }
		}

		public DocStaff ExportSeaRep
		{
			get { return DocStaff.New(OrgMiscServ.Header.StaffAssignments.ExportSeaRepStaff, Factory); }
		}

		public DocStaff ImportAirRep
		{
			get { return DocStaff.New(OrgMiscServ.Header.StaffAssignments.ImportAirRepStaff, Factory); }
		}

		public DocStaff ImportSeaRep
		{
			get { return DocStaff.New(OrgMiscServ.Header.StaffAssignments.ImportSeaRepStaff, Factory); }
		}

		public DocStaff OverallRep
		{
			get { return DocStaff.New(OrgMiscServ.Header.StaffAssignments.OverallSalesRepStaff, Factory); }
		}

		public DocStaff WarehousingRep
		{
			get { return DocStaff.New(OrgMiscServ.Header.StaffAssignments.WarehousingRepStaff, Factory); }
		}

		public ZByte IMCopySeaBills
		{
			get { return OrgMiscServ.OM_IMCopySeaBills; }
		}

		public ZString IMEFTBankAccount
		{
			get { return OrgMiscServ.OM_IMEFTBankAccount; }
		}

		public ZString IMEFTBankBSB
		{
			get { return OrgMiscServ.OM_IMEFTBankBSB; }
		}

		public ZBool IMEftCustomsFromImport
		{
			get { return OrgMiscServ.OM_IMEftCustomsFromImport; }
		}

		public ZBool IMEftHoldUntilPayAuthorised
		{
			get { return OrgMiscServ.OM_IMEftHoldUntilPayAuthorised; }
		}

		public ZInt IMEstDaysDeliveryAir
		{
			get { return OrgMiscServ.OM_IMEstDaysDeliveryAir; }
		}

		public ZInt IMEstDaysDeliveryFCL
		{
			get { return OrgMiscServ.OM_IMEstDaysDeliveryFCL; }
		}

		public ZInt IMEstDaysDeliveryLCL
		{
			get { return OrgMiscServ.OM_IMEstDaysDeliveryLCL; }
		}

		public ZString IMImporterCategory
		{
			get { return OrgMiscServ.OM_IMImporterCategory; }
		}

		public ZBool IMImporterOwnsPartNumbers
		{
			get { return OrgMiscServ.OM_IMImporterOwnsPartNumbers; }
		}

		public ZBool IMIsGSTDeferred
		{
			get { return OrgMiscServ.OM_IMIsGSTDeferred; }
		}

		public ZBool IMJobRequireOrderTrackLink
		{
			get { return OrgMiscServ.OM_IMJobRequireOrderTrackLink; }
		}

		public ZDecimal IMMaxEFTAmount
		{
			get { return OrgMiscServ.OM_IMMaxEFTAmount; }
		}

		public ZString IMMergeCustomsInvoiceLinesBy
		{
			get { return OrgMiscServ.OM_IMMergeCustomsInvoiceLinesBy; }
		}

		public ZDecimal IMMinEFTAmount
		{
			get { return OrgMiscServ.OM_IMMinEFTAmount; }
		}

		public ZString IMOrderLineAttrib1
		{
			get { return OrgMiscServ.OM_IMOrderLineAttrib1; }
		}

		public ZString IMOrderLineAttrib2
		{
			get { return OrgMiscServ.OM_IMOrderLineAttrib2; }
		}

		public ZString IMOrderLineAttrib3
		{
			get { return OrgMiscServ.OM_IMOrderLineAttrib3; }
		}

		public ZBlob IMOrderStatusCodePairList
		{
			get { return OrgMiscServ.OM_IMOrderStatusCodePairList; }
		}

		public ZByte IMOriginalSeaBills
		{
			get { return OrgMiscServ.OM_IMOriginalSeaBills; }
		}

		public ZString IMSendImportDocsTo
		{
			get { return OrgMiscServ.OM_IMSendImportDocsTo; }
		}

		public MultilingualString IMPartAttrib1Name
		{
			get { return OrgMiscServ.OM_IMPartAttrib1NameMultilingual; }
		}

		public MultilingualString IMPartAttrib2Name
		{
			get { return OrgMiscServ.OM_IMPartAttrib2NameMultilingual; }
		}

		public MultilingualString IMPartAttrib3Name
		{
			get { return OrgMiscServ.OM_IMPartAttrib3NameMultilingual; }
		}

		#region Organisations

		public DocOrganisation Organisation
		{
			get { return DocOrganisation.New(OrgMiscServ.Header, Factory); }
		}

		public DocOrganisation APSettlementGroup
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.APSettlementGroup, Factory); }
		}

		public DocOrganisation ARSettlementGroup
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.ARSettlementGroup, Factory); }
		}

		public DocOrganisation CMCompetitor
		{
			get { return null; }
		}

		public DocOrganisation CMControllingAgent
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.ControllingAgent, Factory); }
		}

		public DocOrganisation CMMainBroker
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.All, ZString.Empty), Factory); }
		}

		public DocOrganisation EXAirCartage
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty), Factory); }
		}

		public DocOrganisation EXAirCusBroker
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.PickupAirCustomsBroker, Factory); }
		}

		public DocOrganisation EXAttributeRevenueTo
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.PickupAttributeRevenueTo, Factory); }
		}

		public DocOrganisation EXCustomsBillTo
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.PickupCustomsBillTo, Factory); }
		}

		public DocOrganisation EXFCLCartage
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL), Factory); }
		}

		public DocOrganisation EXFreightBillTo
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.PickupFreightBillTo, Factory); }
		}

		public DocOrganisation EXLCLCartage
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.LCL), Factory); }
		}

		public DocOrganisation EXSeaCusBroker
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.PickupSeaCustomsBroker, Factory); }
		}

		public DocOrganisation FWAPGrouping
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.APGrouping, Factory); }
		}

		public DocOrganisation FWARGrouping
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.ARGrouping, Factory); }
		}

		public DocOrganisation FWDefaultAirDepot
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Air, ZString.Empty), Factory); }
		}

		public DocOrganisation FWDefaultSeaDepot
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, ZString.Empty), Factory); }
		}

		public DocOrganisation FWManagementGrouping
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.ManagementGrouping, Factory); }
		}

		public DocOrganisation IMAirCartage
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty), Factory); }
		}

		public DocOrganisation IMAirCusBroker
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.DeliveryAirCustomsBroker, Factory); }
		}

		public DocOrganisation IMAttributeRevenueTo
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.DeliveryAttributeRevenueTo, Factory); }
		}

		public DocOrganisation IMCustomsBillTo
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.DeliveryCustomsBillTo, Factory); }
		}

		public DocOrganisation IMFCLCartage
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL), Factory); }
		}

		public DocOrganisation IMFreightBillTo
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.DeliveryFreightBillTo, Factory); }
		}

		public DocOrganisation IMLCLCartage
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL), Factory); }
		}

		public DocOrganisation IMSeaCusBroker
		{
			get { return DocOrganisation.New(OrgMiscServ.Header.DeliverySeaCustomsBroker, Factory); }
		}

		#endregion

		public DocCountry EXDefaultCountryOfOrigin
		{
			get { return DocCountry.New(OrgMiscServ.EXDefaultCntryOfOrigin, Factory); }
		}

		public DocServiceLevel ServiceLevel
		{
			get { return DocServiceLevel.New(OrgMiscServ.IMDefaultServiceLevel, Factory); }
		}

		public DocCurrency APDefaultCurrency
		{
			get { return DocCurrency.New(OrgMiscServ.Header.CompanyData.APDefltCurrency, Factory); }
		}

		public ZBool ARQualityAssured
		{
			get { return OrgMiscServ.Header.CompanyData.OB_ARQualityAssured; }
		}

		public ZBool APQualityAssured
		{
			get { return OrgMiscServ.Header.CompanyData.OB_APQualityAssured; }
		}

		public DocCurrency ARDefaultCurrency
		{
			get { return DocCurrency.New(OrgMiscServ.Header.CompanyData.ARDDefltCurrency, Factory); }
		}

		public DocCurrency EXDefaultCurrency
		{
			get { return DocCurrency.New(OrgMiscServ.EXDefCurrency, Factory); }
		}

		public DocCurrency FWDefaultCurrency
		{
			get { return DocCurrency.New(OrgMiscServ.FWDefCurrency, Factory); }
		}

		public ZString CustomAttrib1
		{
			get { return OrgMiscServ.OM_CustomAttrib1; }
		}

		public ZString CustomAttrib2
		{
			get { return OrgMiscServ.OM_CustomAttrib2; }
		}

		public ZString CustomAttrib3
		{
			get { return OrgMiscServ.OM_CustomAttrib3; }
		}

		#endregion
	}
}
