using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using RefCusCodeListLoader = Enterprise.Customs.Universal.ZZRefCusCodeListCombined.Loader;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUAddInfoLookups : AutoAUAddInfoLookups
	{
		public AUAddInfoLookups(AutoAUAddInfo parent)
			: base(parent)
		{
		}

		public BondedWarehouseCollection BondedWarehouses
		{
			get { return Factory.GetCachedValue("AddInfoLookups.BondedWarehouses", delegate { return new BondedWarehouseCollection(Factory); }); }
		}

		public BusinessObjectCollection GlobalEntryLineKeys
		{
			get
			{
				var invoiceLine = AddInfo.InvoiceLine;
				if (invoiceLine != null)
				{
					return new GlobalCusEntryLineCollection(Factory, invoiceLine);
				}
				else if (AddInfo.JobDeclaration != null)
				{
					return new GlobalCusEntryLineCollection(Factory, AddInfo.JobDeclaration);
				}
				else
				{
					return new GlobalCusEntryLineCollection(Factory);
				}
			}
		}

		public BusinessObjectCollection GlobalDrawbackEntryLineKeys
		{
			get
			{
				var invoiceLine = AddInfo.InvoiceLine;
				if (invoiceLine != null)
				{
					return new GlobalDrawbackCusEntryLineCollection(Factory, invoiceLine);
				}
				else if (AddInfo.JobDeclaration != null)
				{
					return new GlobalDrawbackCusEntryLineCollection(Factory, AddInfo.JobDeclaration);
				}
				else
				{
					return new GlobalDrawbackCusEntryLineCollection(Factory);
				}
			}
		}

		public BusinessObjectCollection EntryKeys
		{
			get
			{
				var invoiceLine = AddInfo.InvoiceLine;
				if (invoiceLine != null)
				{
					IBondedWarehouseLink link = new BondedWarehouseLinkCreator().GetNewBondedWarehouseLink(Factory);
					BondedWarehouseTransactionLine line = (BondedWarehouseTransactionLine)((IBondedWarehouseTransactionLineProvider)invoiceLine).TransactionLine;
					line.UseEntryKeyFromEntry = false;
					return link.GetEntryKeyLookup(line);
				}
				else
				{
					throw new NotSupportedException("Only for the line level");
				}
			}
		}

		public OrgAddressCollection BondedWarehouseAddresses
		{
			get
			{
				var warehouseOrgFK = AddInfo.WarehouseOrgFK;
				return Factory.GetCachedValue("AUAddInfoLookups.BondedWarehouseAddresses" + warehouseOrgFK, () =>
				{
					var bondedWarehouseAddresses = new OrgAddressCollection(Factory, new ZQuery(OrgAddressSchema.OA_OH, warehouseOrgFK));
					bondedWarehouseAddresses.Load();
					return bondedWarehouseAddresses;
				});
			}
		}

		public ICodeDescriptionPairList Gender
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Gender); }
		}

		#region Line Valuation Basis List For Edifice

		public CodeDescriptionPairList ZA_ValuationBasis_Hidden_List
		{
			get
			{
				var invoiceLine = AddInfo.InvoiceLine;
				return invoiceLine == null ? Factory.GetCachedValue<CodeDescriptionPairList>() : invoiceLine.LineValuationBasisList;
			}
		}

		#endregion

		#region Header Valuation Basis List

		public CodeDescriptionPairList HeaderValuationBasisListForEDIFICE
		{
			get
			{
				return Factory.GetCachedValue("AUAddInfoLookups_HeaderValuationBasisListForEDIFICE", () =>
					{
						var result = EDIFICEValuationBasis.Line_List;
						result.AddPair("IG", "Identical goods to previous importation");
						result.AddPair("DV", "Deductive value");
						result.AddPair("SG", "Similar goods");
						result.AddPair("CV", "Computer value with percentage uplift");
						return result;
					});
			}
		}

		#endregion

		#region Valuation Basis List For CMR

		public CodeDescriptionPairList ValuationBasisListForCMR
		{
			get { return Factory.GetCachedValue<CMRValuationBasisList>(); }
		}

		#endregion

		#region ZA_DCX_List
		public CodeDescriptionPairList ZA_DCX_List
		{
			get { return Factory.GetCachedValue<CodeDescriptionPairListCustomsCountryCode>(); }
		}
		#endregion

		#region ZA_DRC_List
		public CodeDescriptionPairList ZA_DRC_List
		{
			get { return Factory.GetCachedValue<CodeDescriptionPairListCustomsDumpingReasonCode>(); }
		}
		#endregion

		#region ZA_GSTE_List

		public CMRCodeListsCollection CMRGSTEList
		{
			get { return Factory.GetCachedValue("AddInfoLookups.CMRGSTEList", delegate { return new CMRCodeListsCollection(Factory, CMRCodeLists.CodeTypes.GSTX); }); }
		}
		#endregion

		#region ZA_InstrumentType_List

		public CodeDescriptionPairList ZA_InstrumentType_List
		{
			get
			{
				var dec = AddInfo.JobDeclaration;
				return dec == null || dec.IsImportCMR
						? Factory.GetCachedValue<CMRInstrumentTypeList>()
						: Factory.GetCachedValue<CustomsInstrumentTypeList>();
			}
		}

		public CMRInstrumentCollection CMRInstrumentNumberList
		{
			get { return new CMRInstrumentCollection(Factory, AddInfo.ZA_InstrumentType_Hidden); }
		}

		#endregion

		#region ZA_LCTE_List

		public CMRCodeListsCollection CMRLCTEList
		{
			get { return Factory.GetCachedValue("AddInfoLookups.CMRLCTEList", delegate { return new CMRCodeListsCollection(Factory, CMRCodeLists.CodeTypes.LCTX); }); }
		}
		#endregion

		#region ZA_ORG_List
		public RefCountryCollection ZA_ORG_List
		{
			get { return Factory.GetCachedValue("AddInfoLookups.RefCountryCollection", delegate { return new RefCountryCollection(Factory); }); }
		}

		#endregion

		#region CountryCode_List
		public RefCountryCollection CountryCode_List
		{
			get { return Factory.GetCachedValue("AddInfoLookups.RefCountryCollection", delegate { return new RefCountryCollection(Factory); }); }
		}
		#endregion

		#region ZA_REL_Lists
		public CodeDescriptionPairList ZA_REL_List
		{
			get { return Factory.GetCachedValue<CMRRelatedTransactionList>(); }
		}

		public CodeDescriptionPairList ZA_HeaderREL_List
		{
			get { return Factory.GetCachedValue<CMRHeaderRelatedTransactionList>(); }
		}
		#endregion

		#region CMRCustQuantityUnits

		public CMRQuantityUnits QuantityUnits
		{
			get { return Factory.GetCachedValue<CMRQuantityUnits>(); }
		}

		#endregion

		#region ZA_WETE_List
		public CodeDescriptionPairList ZA_WETE_List
		{
			get { return Factory.GetCachedValue<CodeDescriptionPairListCustomsWETExemption>(); }
		}

		public CMRCodeListsCollection CMRWETEList
		{
			get { return Factory.GetCachedValue("AddInfoLookups.CMRWETEList", delegate { return new CMRCodeListsCollection(Factory, CMRCodeLists.CodeTypes.WETX); }); }
		}
		#endregion

		#region TreatmentCodeList

		public CodeDescriptionPairList TreatmentCodeList
		{
			get
			{
				return GetTreatmentCodeListForCMR();
			}
		}

		CodeDescriptionPairList GetTreatmentCodeListForCMR()
		{
			var key = new ZStringBuilder(ZString.Format("AddInfoLookups.TreatmentCodeListForCMR-{0}-", AddInfo.EffectiveDutyDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)));
			var invoiceLine = AddInfo.InvoiceLine;
			if (invoiceLine != null)
			{
				key.Append(invoiceLine.IsGeneralRate.ToString());
				key.Append(invoiceLine.AggregatedZA_PST);
			}
			return Factory.GetCachedValue(key.ToString(), delegate
			{
				ZQuery filter = FilterForStartAndEndDate(CMRTreatmentRatePeriodSnapshotSchema.TP_StartDate, CMRTreatmentRatePeriodSnapshotSchema.TP_EndDate);

				if (invoiceLine != null)
				{
					if (invoiceLine.IsGeneralRate)
					{
						filter.AddToFilter(CMRTreatmentRatePeriodSnapshotSchema.TP_PreferenceSchemeType, AUAddInfo.GeneralPreferenceRate);
					}
					else if (!invoiceLine.AggregatedZA_PST.IsEmpty)
					{
						ZQuery preferenceFilter = new ZQuery(CMRTreatmentRatePeriodSnapshotSchema.TP_PreferenceSchemeType, SQLComparisonOperator.Equal, AddInfo.AggregatedPST.ToUpper().ToString());
						ZQuery generalPreferenceFilter = new ZQuery(CMRTreatmentRatePeriodSnapshotSchema.TP_PreferenceSchemeType, SQLComparisonOperator.Equal, AUAddInfo.GeneralPreferenceRate);
						generalPreferenceFilter.AddToFilter(JoinCondition.And, CMRTreatmentRatePeriodSnapshotSchema.TP_CalculationType, SQLComparisonOperator.Equal, Constants.DutyCalcTypes.Info);
						preferenceFilter.AddToFilter(generalPreferenceFilter, JoinCondition.Or);
						filter.AddToFilter(preferenceFilter, JoinCondition.And);
					}
				}

				filter.OrderBy = CMRTreatmentRatePeriodSnapshotSchema.TP_Code.Name;

				var treatments = new CMRTreatmentRatePeriodSnapshotCollection(AddInfo.Factory, filter);
				treatments.Load(filter);
				var treatmentCodeList = new CodeDescriptionPairList();
				treatmentCodeList.AddRange(treatments);

				return treatmentCodeList;
			});
		}

		#endregion

		#region PRF List

		public CodeDescriptionPairList ZA_PRFList
		{
			get
			{
				var isExport = AddInfo.IsExport;
				var aggregatedZA_ORG = AddInfo.AggregatedZA_ORG;
				return Factory.GetCachedValue("AUAddInfoLookups.ZA_PRFList" + isExport + aggregatedZA_ORG, () =>
				{
					var list = new CodeDescriptionPairList();
					if (!isExport)
					{
						switch (aggregatedZA_ORG)
						{
							case Core.Constants.CountryCodes.UnitedStates:
								list = new AUAddInfoLookupsPrfListUS();
								break;
							case Core.Constants.CountryCodes.Thailand:
								list = new AUAddInfoLookupsPrfListThailand();
								break;
						}
					}
					return list;
				});
			}
		}

		#endregion

		#region ZA_DXT_List

		public CodeDescriptionPairList ZA_DXT_List
		{
			get { return Factory.GetCachedValue<CodeDescriptionPairListDumpingExemptionType>(); }
		}

		#endregion

		#region ZA_POC_List

		public RefCountryCollection ZA_POC_List
		{
			get { return Factory.GetCachedValue("AddInfoLookups.RefCountryCollection", delegate { return new RefCountryCollection(Factory); }); }
		}

		#endregion

		#region ZA_PRT_List

		public CodeDescriptionPairList ZA_PRT_List
		{
			get
			{
				CodeDescriptionPairList result = null;
				if (AddInfo.IsGeneralRate)
				{
					result = Factory.GetCachedValue<CodeDescriptionPairList>();
				}
				else if (AddInfo.AggregatedPST.IsEmpty)
				{
					result = RulesListWithoutScheme;
				}
				else
				{
					result = RulesWithScheme;
				}

				return result;
			}
		}

		CodeDescriptionPairList RulesWithScheme
		{
			get
			{
				return Factory.GetCachedValue(ZString.Format("AddInfoLookups.RulesWithScheme-{0}", AddInfo.AggregatedPST), delegate
				{
					ZQuery filter = new ZQuery(CMRPreferenceSchemeRuleSchema.PR_PreferenceSchemePeriodSnapshotSchemeType, AddInfo.AggregatedPST);
					filter.OrderBy = CMRPreferenceSchemeRuleSchema.Constants.PR_RuleType;

					var rulesWithSchemeCollection = new CMRPreferenceSchemeRuleCollection(Factory, filter);
					rulesWithSchemeCollection.Load();

					var rulesWithScheme = new CodeDescriptionPairList();
					for (int i = 0; i < rulesWithSchemeCollection.Count; i++)
					{
						var rule = rulesWithSchemeCollection[i];
						if (!rulesWithScheme.ContainsCode(rule.Code))
						{
							rulesWithScheme.Add(rule);
						}
					}
					return rulesWithScheme;
				});
			}
		}

		CodeDescriptionPairList RulesListWithoutScheme
		{
			get
			{
				return Factory.GetCachedValue(ZString.Format("AddInfoLookups.RulesListWithoutScheme-{0}", AddInfo.EffectiveDutyDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)), delegate
				{
					ZQuery filter = FilterForStartAndEndDate(CMRPreferenceRulePeriodSnapshotSchema.PU_StartDate, CMRPreferenceRulePeriodSnapshotSchema.PU_EndDate);
					filter.OrderBy = CMRPreferenceRulePeriodSnapshotSchema.Constants.PU_RuleType;

					var rulesListWithoutSchemeCollection = new CMRPreferenceRulePeriodSnapshotCollection(Factory, filter);
					rulesListWithoutSchemeCollection.Load();

					var ruleListWithoutScheme = new CodeDescriptionPairList();
					ruleListWithoutScheme.AddRange(rulesListWithoutSchemeCollection);

					return ruleListWithoutScheme;
				});
			}
		}

		#endregion

		#region ZA_PST_List

		public CodeDescriptionPairList ZA_PST_List
		{
			get
			{
				CodeDescriptionPairList result = null;

				if (AddInfo.InvoiceLine != null)
				{
					result = PreferenceSchemeWithOriginAndTariffOrTreatment;
				}
				else if (!AddInfo.AggregatedPOCFallBackToORG.IsEmpty)
				{
					result = PreferenceSchemeWithOrigin;
				}
				else
				{
					result = FlatPreferenceScheme;
				}
				return result;
			}
		}

		CodeDescriptionPairList PreferenceSchemeWithOriginAndTariffOrTreatment
		{
			get
			{
				var invoiceLine = AddInfo.InvoiceLine;
				if (invoiceLine == null)
				{
					return new CodeDescriptionPairList();
				}
				else
				{
					return Factory.GetCachedValue(ZString.Format("AddInfoLookups.PreferenceSchemeWithOriginAndTariffOrTreatment-{0}-{1}-{2}-{3}", invoiceLine.TreatmentCode, invoiceLine.TariffNumber, AddInfo.EffectiveDutyDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture), AddInfo.AggregatedPOCFallBackToORG),
						delegate
						{
							var preferenceSchemeWithTariffOrTreatment = this.PreferenceSchemeWithTariffOrTreatment;
							CodeDescriptionPairList result = null;
							if (AddInfo.AggregatedPOCFallBackToORG.IsEmpty)
							{
								result = preferenceSchemeWithTariffOrTreatment;
							}
							else
							{
								result = new CodeDescriptionPairList();
								for (int i = 0; i < preferenceSchemeWithTariffOrTreatment.Count; i++)
								{
									if (PreferenceSchemeWithOrigin.ContainsCode(preferenceSchemeWithTariffOrTreatment[i].Code) && !result.ContainsCode(preferenceSchemeWithTariffOrTreatment[i].Code))
									{
										result.Add(preferenceSchemeWithTariffOrTreatment[i]);
									}
								}
							}
							return result;
						});
				}
			}
		}

		CodeDescriptionPairList PreferenceSchemeWithTariffOrTreatment
		{
			get
			{
				return AddInfo.InvoiceLine.TreatmentCode.IsEmpty ? PreferenceSchemeWithTariff : PreferenceSchemeWithTreatment;
			}
		}

		ZQuery PreferenceSchemeWithTariffQuery()
		{
			ZQuery result = new ZQuery(CMRTariffRatePeriodSnapshotSchema.TT_TariffClassificationNumber, AddInfo.InvoiceLine.TariffNumber);
			result.AddToFilter(FilterForStartAndEndDate(CMRTariffRatePeriodSnapshotSchema.TT_StartDate, CMRTariffRatePeriodSnapshotSchema.TT_EndDate));
			return result;
		}

		CodeDescriptionPairList PreferenceSchemeWithTariff
		{
			get
			{
				CodeDescriptionPairList result;
				var invoiceLine = AddInfo.InvoiceLine;
				if (invoiceLine != null)
				{
					result = Factory.GetCachedValue(ZString.Format("AddInfoLookups.PreferenceSchemeWithTariff-{0}-{1}", invoiceLine.TariffNumber, AddInfo.EffectiveDutyDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)),
						delegate
						{
							var delegateResult = new CodeDescriptionPairList();
							foreach (CMRTariffRatePeriodSnapshot snapshot in Factory.Load<CMRTariffRatePeriodSnapshot>(PreferenceSchemeWithTariffQuery()))
							{
								delegateResult.Add(snapshot);
							}
							return delegateResult;
						}
					);
				}
				else
				{
					result = Factory.GetCachedValue<CodeDescriptionPairList>();
				}

				return result;
			}
		}

		ZQuery PreferenceSchemeWithTreatmentQuery()
		{
			ZQuery result = new ZQuery(CMRTreatmentRatePeriodSnapshotSchema.TP_Code, AddInfo.InvoiceLine.TreatmentCode);
			result.AddToFilter(FilterForStartAndEndDate(CMRTreatmentRatePeriodSnapshotSchema.TP_StartDate, CMRTreatmentRatePeriodSnapshotSchema.TP_EndDate));
			return result;
		}

		CodeDescriptionPairList PreferenceSchemeWithTreatment
		{
			get
			{
				CodeDescriptionPairList result;
				var invoiceLine = AddInfo.InvoiceLine;
				if (invoiceLine != null)
				{
					result = Factory.GetCachedValue(ZString.Format("AddInfoLookups.PreferenceSchemeWithTreatment-{0}-{1}", invoiceLine.TreatmentCode, AddInfo.EffectiveDutyDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)),
						delegate
						{
							bool onlyGenInfos = false;
							var delegateResult = new CodeDescriptionPairList();
							foreach (CMRTreatmentRatePeriodSnapshot snapshot in Factory.Load<CMRTreatmentRatePeriodSnapshot>(PreferenceSchemeWithTreatmentQuery()))
							{
								if (snapshot.TP_PreferenceSchemeType == AUAddInfo.GeneralPreferenceRate && snapshot.TP_CalculationType == Constants.DutyCalcTypes.Info)
								{
									onlyGenInfos = true;
								}

								delegateResult.AddPair(snapshot.TP_PreferenceSchemeType, ((ICodeDescription)snapshot).Description);
							}
							return onlyGenInfos && delegateResult.Count == 1 ? PreferenceSchemeWithTariff : delegateResult;
						}
					);
				}
				else
				{
					result = Factory.GetCachedValue<CodeDescriptionPairList>();
				}

				return result;
			}
		}

		CodeDescriptionPairList PreferenceSchemeWithOrigin
		{
			get
			{
				return Factory.GetCachedValue(ZString.Format("AddInfoLookups.PreferenceSchemeWithOrigin-{0}", AddInfo.AggregatedPOCFallBackToORG), delegate
				{
					ZQuery filter = new ZQuery(CMRPreferenceSchemePeriodCountrySchema.PC_CountryCode, SQLComparisonOperator.Equal, AddInfo.AggregatedPOCFallBackToORG);

					var preferenceSchemeWithOriginCollection = new CMRPreferenceSchemePeriodCountryCollection(AddInfo.Factory, filter);
					preferenceSchemeWithOriginCollection.Load();

					var preferenceSchemeWithOrigin = new CodeDescriptionPairList();
					preferenceSchemeWithOrigin.AddPair(AUAddInfo.GeneralPreferenceRate, "General");
					for (int i = 0; i < preferenceSchemeWithOriginCollection.Count; i++)
					{
						var scheme = preferenceSchemeWithOriginCollection[i];
						if (!preferenceSchemeWithOrigin.ContainsCode(((ICodeDescription)scheme).Code))
						{
							preferenceSchemeWithOrigin.Add(scheme);
						}
					}

					return preferenceSchemeWithOrigin;
				});
			}
		}

		CodeDescriptionPairList FlatPreferenceScheme
		{
			get
			{
				return Factory.GetCachedValue(ZString.Format("AddInfoLookups.FlatPreferenceScheme-{0}", AddInfo.EffectiveDutyDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)), delegate
				{
					ZQuery filter = FilterForStartAndEndDate(CMRPreferenceSchemePeriodSnapshotSchema.PF_StartDate, CMRPreferenceSchemePeriodSnapshotSchema.PF_EndDate);

					var flatPrefrenceSchemeCollection = new CMRPreferenceSchemePeriodSnapshotCollection(Factory, filter);
					flatPrefrenceSchemeCollection.Load();

					var flatPrefrenceScheme = new CodeDescriptionPairList();
					flatPrefrenceScheme.AddPair(AUAddInfo.GeneralPreferenceRate, "General");
					flatPrefrenceScheme.AddRange(flatPrefrenceSchemeCollection);

					return flatPrefrenceScheme;
				});
			}
		}

		#endregion

		#region ZA_AQISServicePaymentCurrency_List
		public RefCurrencyCollection ZA_AQISServicePaymentCurrency_List
		{
			get { return Factory.GetCachedValue("AddInfoLookups.RefCurrencyCollection", delegate { return new RefCurrencyCollection(Factory); }); }
		}
		#endregion

		#region ZA_RRC_List
		public CodeDescriptionPairList ZA_RRC_List
		{
			get
			{
				var useRefDataRepoRegistryValue = AUCustomsDataRegistry.Instance.UseRefDatabaseData.Value;
				return Factory.GetCachedValue(ZString.Format("AddInfoLookups.ZA_RRC_List-{0}-{1}", AddInfo.EffectiveDutyDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture), useRefDataRepoRegistryValue), delegate
				{
					return useRefDataRepoRegistryValue ? GetRefundReasonCodesFromRefDataRepo() : GetRefundReasonCodesFromCMRRefundReason();
				});
			}
		}
		#endregion

		#region ZA_HART_List
		public CMRCodeListsCollection ZA_HART_List
		{
			get { return Factory.GetCachedValue("AddInfoLookups.ZA_HART_List", delegate { return new CMRCodeListsCollection(Factory, CMRCodeLists.CodeTypes.HEADARSTYP); }); }
		}
		#endregion

		#region ZA_RNO_List

		public const string GeneralRateNo = "001";

		public CodeDescriptionPairList ZA_RNO_List
		{
			get { return GetCachedTariffRateNumberList(); }
		}

		#endregion

		#region ZA_TRN_List

		public CodeDescriptionPairList ZA_TRN_List
		{
			get { return GetCachedTreatmentRateNumberList(); }
		}

		#endregion

		#region ZA_AUStatesList

		public CodeDescriptionPairList ZA_AUStatesList
		{
			get
			{
				return Factory.GetCachedValue("AUAddInfoLookups_AUStateList", () => new AUAddInfoLookupsStatesList());
			}
		}

		#endregion

		#region Rate Number Lists

		CodeDescriptionPairList GetCachedTariffRateNumberList()
		{
			var invoiceLine = AddInfo.InvoiceLine;
			if (invoiceLine != null && invoiceLine.EffectiveDutyDate.IsValid)
			{
				return Factory.GetCachedValue(ZString.Format("AddInfoLookups.ZA_RNO_List-{0}-{1}-{2}", AddInfo.EffectiveDutyDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture), invoiceLine.TariffNumber.Replace(" ", ""), invoiceLine.IsGeneralRate || AddInfo.AggregatedPST.IsEmpty ? AUAddInfo.GeneralPreferenceRate : AddInfo.AggregatedPST.ToUpper().ToString()),
					delegate
					{
						var filter = FilterForStartAndEndDate(CMRTariffRatePeriodSnapshotSchema.TT_StartDate, CMRTariffRatePeriodSnapshotSchema.TT_EndDate);
						filter.AddToFilter(CMRTariffRatePeriodSnapshotSchema.TT_TariffClassificationNumber, invoiceLine.TariffNumber.Replace(" ", ""));
						filter.AddToFilter(CMRTariffRatePeriodSnapshotSchema.TT_PreferenceSchemeType, invoiceLine.IsGeneralRate || AddInfo.AggregatedPST.IsEmpty ? AUAddInfo.GeneralPreferenceRate : AddInfo.AggregatedPST.ToUpper().ToString());

						var rateNumbers = new CMRTariffRatePeriodSnapshotCollection(Factory, filter);
						rateNumbers.Load();

						var result = new CodeDescriptionPairList();
						foreach (CMRTariffRatePeriodSnapshot rateNumber in rateNumbers)
						{
							result.AddPair(rateNumber.TT_RateNumber, rateNumber.GetDutyRateDescription());
						}
						return result;
					});
			}
			else
			{
				return new CodeDescriptionPairList();
			}
		}

		CodeDescriptionPairList GetCachedTreatmentRateNumberList()
		{
			var invoiceLine = AddInfo.InvoiceLine;

			if (invoiceLine != null && invoiceLine.EffectiveDutyDate.IsValid)
			{
				return Factory.GetCachedValue(ZString.Format("AddInfoLookups.ZA_TRN_List-{0}-{1}-{2}", AddInfo.EffectiveDutyDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture), invoiceLine.TreatmentCode, invoiceLine.IsGeneralRate || AddInfo.AggregatedPST.IsEmpty ? AUAddInfo.GeneralPreferenceRate : AddInfo.AggregatedPST.ToUpper().ToString()),
					delegate
					{
						var filter = FilterForStartAndEndDate(CMRTreatmentRatePeriodSnapshotSchema.TP_StartDate, CMRTreatmentRatePeriodSnapshotSchema.TP_EndDate);
						filter.AddToFilter(CMRTreatmentRatePeriodSnapshotSchema.TP_Code, invoiceLine.TreatmentCode);
						filter.AddToFilter(CMRTreatmentRatePeriodSnapshotSchema.TP_PreferenceSchemeType, invoiceLine.IsGeneralRate || AddInfo.AggregatedPST.IsEmpty ? AUAddInfo.GeneralPreferenceRate : AddInfo.AggregatedPST.ToUpper().ToString());

						var rateNumbers = new CMRTreatmentRatePeriodSnapshotCollection(Factory, filter);
						rateNumbers.Load();

						var result = new CodeDescriptionPairList();
						foreach (CMRTreatmentRatePeriodSnapshot rateNumber in rateNumbers)
						{
							result.AddPair(rateNumber.TP_RateNumber, rateNumber.GetDutyRateDescription());
						}
						return result;
					});
			}
			else
			{
				return new CodeDescriptionPairList();
			}
		}

		#endregion

		#region Drawback Lookups and Lists

		public BusinessObjectCollection ImportDeclarations
		{
			get
			{
				if (AddInfo.InvoiceLine != null)
				{
					return new CusEntryHeaderCollection(AddInfo.JobDeclaration, Factory);
				}
				else
				{
					throw new NotSupportedException("Only for the line level");
				}
			}
		}

		public CodeDescriptionPairList DrawbackAmberCodeList
		{
			get
			{
				return Factory.GetCachedValue("AUAddInfoLookups_DrawbackAmberCodeList", () => new DrawbackAmberReasonTypesList());
			}
		}

		public CodeDescriptionPairList ZA_DAM_List
		{
			get
			{
				return Factory.GetCachedValue("AUAddInfoLookups_ZA_DAM_List", () => new DrawbackAssessmentMethodsList());
			}
		}

		#endregion

		public CMREstablishmentCodesCollection EstablishmentCodes
		{
			get { return new CMREstablishmentCodesCollection(Factory); }
		}

		#region Implementation

		ZQuery FilterForStartAndEndDate(SchemaColumn startDate, SchemaColumn endDate)
		{
			ZQuery endDateFilter = new ZQuery(endDate, null);
			endDateFilter.AddToFilter(JoinCondition.Or, endDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, AddInfo.EffectiveDutyDate);

			ZQuery filter = new ZQuery(startDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, AddInfo.EffectiveDutyDate);
			filter.AddToFilter(endDateFilter);

			return filter;
		}

		protected AUAddInfo AddInfo
		{
			get { return (AUAddInfo)Parent; }
		}

		protected CollectionCache CollectionCache
		{
			get { return AddInfo.CollectionCache; }
		}

		CodeDescriptionPairList GetRefundReasonCodesFromCMRRefundReason()
		{
			var fZA_RRC_List = new CodeDescriptionPairList();
			CMRRefundReasonCollection refundReasons = new CMRRefundReasonCollection(Factory);

			ZDateTime effectiveDate = AddInfo.EffectiveDutyDate;
			ZQuery endDateFilter = new ZQuery(CMRRefundReasonSchema.CR_RefundReasonEndDate, null);
			endDateFilter.AddToFilter(JoinCondition.Or, CMRRefundReasonSchema.CR_RefundReasonEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
			ZQuery filter = new ZQuery(CMRRefundReasonSchema.CR_RefundReasonStartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(endDateFilter);
			filter.OrderBy = CMRRefundReasonSchema.Constants.CR_RefundReasonDescription;

			refundReasons.LoadWithMoreFiltering(new ZQuery(filter));

			fZA_RRC_List.AddRange(refundReasons);

			return fZA_RRC_List;
		}

		CodeDescriptionPairList GetRefundReasonCodesFromRefDataRepo()
		{
			var codes = RefCusCodeListLoader.Load(Factory, Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRRR, AddInfo.EffectiveDutyDate).OrderBy(x => x.ZZD_Description);
			var codePairList = new CodeDescriptionPairList();
			codePairList.AddRange(codes.ToList());
			return codePairList;
		}
		#endregion

		public CodeDescriptionPairList AqisCustomsWeightUqList
		{
			get { return Factory.GetCachedValue<EXDOCErrata32List36CustomsWeightUnits>(); }
		}

		public CodeDescriptionPairList SettlementPeriodTypeList => Factory.GetCachedValue<SettlementPeriodTypeList>();
	}
}
