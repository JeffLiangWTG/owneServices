using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.LandedCosting.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocLandedCostHeader : DocBaseWrapper
	{
		protected DocLandedCostHeader(LandedCostHeader lCHeader, BusinessObjectFactory factoryToWrap)
			: base(lCHeader, factoryToWrap)
		{
		}

		public static DocLandedCostHeader New(LandedCostHeader lCHeader, BusinessObjectFactory factoryForWrapper)
		{
			DocLandedCostHeader result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(lCHeader, factoryForWrapper);
			}
			else if (lCHeader != null)
			{
				return new DocLandedCostHeader(lCHeader, factoryForWrapper);
			}

			return result;
		}

		protected delegate DocLandedCostHeader NewDelegate(LandedCostHeader lCHeader, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#region ZDateTime Fields

		public ZDateTime DateOfProcessing
		{
			get { return LCHeader.LT_DateOfProcessing; }
		}

		#endregion

		#region ZString Fields

		public ZString ConsigneeName
		{
			get { return LCHeader.Consignee == null ? ZString.Empty : LCHeader.Consignee.OH_FullName; }
		}

		public ZString UniqueReferenceNumber
		{
			get
			{
				return LCHeader.UniqueReferenceNumber;
			}
		}

		public ZDecimal TotalLinePrice
		{
			get { return RoundingHelper.Round(LCHeader.TotalLinePrice, LCHeader.LocalCurrencyDecimals); }
		}

		public ZString MultipleInvoices
		{
			get { return LCHeader.MultipleInvoices; }
		}

		public ZDecimal TotalInvoiceCost
		{
			get { return RoundingHelper.Round(LCHeader.TotalInvoiceCost, LCHeader.LocalCurrencyDecimals); }
		}

		public ZDecimal TotalCustomsDisbursementCharges
		{
			get { return RoundingHelper.Round(LCHeader.TotalCustomsDisbursementCharges, LCHeader.LocalCurrencyDecimals); }
		}

		public ZDecimal TotalLandingCost
		{
			get { return RoundingHelper.Round(LCHeader.TotalLandingCost, LCHeader.LocalCurrencyDecimals); }
		}

		public ZDecimal TotalGroup1
		{
			get { return RoundingHelper.Round(LCHeader.TotalGroup1, LCHeader.LocalCurrencyDecimals); }
		}

		public ZDecimal TotalGroup2
		{
			get { return RoundingHelper.Round(LCHeader.TotalGroup2, LCHeader.LocalCurrencyDecimals); }
		}

		public ZDecimal TotalGroup3
		{
			get { return RoundingHelper.Round(LCHeader.TotalGroup3, LCHeader.LocalCurrencyDecimals); }
		}

		public ZDecimal TotalGroup4
		{
			get { return RoundingHelper.Round(LCHeader.TotalGroup4, LCHeader.LocalCurrencyDecimals); }
		}

		public ZDecimal TotalGroup5
		{
			get { return RoundingHelper.Round(LCHeader.TotalGroup5, LCHeader.LocalCurrencyDecimals); }
		}

		public ZDecimal TotalGroup6
		{
			get { return RoundingHelper.Round(LCHeader.TotalGroup6, LCHeader.LocalCurrencyDecimals); }
		}

		public ZDecimal TotalGroupMisc
		{
			get { return RoundingHelper.Round(LCHeader.TotalGroupMisc, LCHeader.LocalCurrencyDecimals); }
		}

		public ZDecimal TotalCost
		{
			get { return RoundingHelper.Round(LCHeader.TotalCost, LCHeader.LocalCurrencyDecimals); }
		}

		public ZDecimal TotalCostWithMarkup1Applied
		{
			get { return RoundingHelper.Round(LCHeader.TotalCostWithMarkup1Applied, LCHeader.LocalCurrencyDecimals); }
		}

		public ZDecimal TotalAmountPayable
		{
			get
			{
				ZDecimal sum = 0;
				foreach (DocBaseCusEntryHeader cusEntryHeader in BaseEntryHeaders)
				{
					sum += cusEntryHeader.TotalAmountPayable;
				}

				return sum;
			}
		}

		public ZDecimal TotalCIFAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				foreach (DocBaseCusEntryHeader baseCusEntryHeader in BaseEntryHeaders)
				{
					result += baseCusEntryHeader.CIF.Amount;
				}
				return result;
			}
		}

		public ZDecimal TotalCIFLocalAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				foreach (DocBaseCusEntryHeader baseCusEntryHeader in BaseEntryHeaders)
				{
					result += baseCusEntryHeader.CIFInLocalCurrency.Amount;
				}

				return result;
			}
		}

		public DocCustomsDisbursementChargeCollection DocCustomsDisbursementCharges
		{
			get
			{
				if (fCustomsDisbursementCharges == null)
				{
					fCustomsDisbursementCharges = new DocCustomsDisbursementChargeCollection(Factory);
					var customsChargeLCItemSettings = LCHeader.CustomsChargeLCItemSettings;
					if (customsChargeLCItemSettings != null)
					{
						foreach (var setting in customsChargeLCItemSettings)
						{
							fCustomsDisbursementCharges.Add(DocCustomsDisbursementCharge.New(
								new CustomsDisbursementCharge(setting, RoundingHelper.Round(LCHeader.GetTotalCustomsDisbursementChargeAmount(setting.CostType), LCHeader.LocalCurrencyDecimals)), Factory));
						}
					}
				}
				return fCustomsDisbursementCharges;
			}
		}
		DocCustomsDisbursementChargeCollection fCustomsDisbursementCharges;

		#endregion

		#region ZString Label Fields

		public ZString LandedCostGroup1Label
		{
			get { return LCHeader.LandedCostGroup1Label; }
		}

		public ZString LandedCostGroup2Label
		{
			get { return LCHeader.LandedCostGroup2Label; }
		}

		public ZString LandedCostGroup3Label
		{
			get { return LCHeader.LandedCostGroup3Label; }
		}

		public ZString LandedCostGroup4Label
		{
			get { return LCHeader.LandedCostGroup4Label; }
		}

		public ZString LandedCostGroup5Label
		{
			get { return LCHeader.LandedCostGroup5Label; }
		}

		public ZString LandedCostGroup6Label
		{
			get { return LCHeader.LandedCostGroup6Label; }
		}

		public ZString LandedCostGroupMiscLabel
		{
			get { return LCHeader.LandedCostGroupMiscLabel; }
		}

		public ZString Disclaimer
		{
			get { return LCHeader.Disclaimer; }
		}

		public ZString LandedCostPercentageLabel
		{
			get { return LCHeader.LandedCostPercentageLabel; }
		}

		public ZString LandedTotalCostPercentageLabel
		{
			get { return LCHeader.LandedTotalCostPercentageLabel; }
		}

		#endregion

		#region Related Objects

		public DocLandedCostHistoryCollection Histories
		{
			get
			{
				if (fHistories == null)
				{
					fHistories = new DocLandedCostHistoryCollection(LCHeader.Histories, Factory);
					fHistories.Sort(new DocLandedCostHistoryComparer());
				}
				return fHistories;
			}
		}
		DocLandedCostHistoryCollection fHistories;

		public DocLandedCostInputCollection CostInputs
		{
			get
			{
				if (fCostInputs == null)
				{
					fCostInputs = new DocLandedCostInputCollection(LCHeader.CostInputs, Factory);
				}
				return fCostInputs;
			}
		}
		DocLandedCostInputCollection fCostInputs;

		public LandedCostDutyRateSummaryCollection DutyRates
		{
			get { return fDutyRates ?? (fDutyRates = new LandedCostDutyRateSummaryCollection(Histories, Factory)); }
		}
		LandedCostDutyRateSummaryCollection fDutyRates;

		#region BaseEntryHeaders
		public DocBaseCusEntryHeaderCollection BaseEntryHeaders
		{
			get { return fBaseEntryHeaders ?? (fBaseEntryHeaders = GetNewEntryHeadersCollection()); }
		}
		DocBaseCusEntryHeaderCollection fBaseEntryHeaders;

		DocBaseCusEntryHeaderCollection GetNewEntryHeadersCollection()
		{
			if (DocDeclaration != null)
			{
				return DocDeclaration.RateEntryHeaders;
			}
			return new EmptyDocCusEntryHeaderCollection(Factory);
		}

		public DocBaseJobDeclaration DocDeclaration
		{
			get
			{
				if (fDocDeclaration == null)
				{
					fDocDeclaration = GetNewDocDeclaration();
				}
				return fDocDeclaration;
			}
		}
		DocBaseJobDeclaration fDocDeclaration;
		DocBaseJobDeclaration GetNewDocDeclaration()
		{
			DocBaseJobDeclaration docDeclaration = null;
			BaseJobDeclaration declaration = LCHeader.Parent as BaseJobDeclaration;
			if (declaration != null)
			{
				docDeclaration = DocBaseJobDeclaration.New(declaration, Factory);
			}
			return docDeclaration;
		}

		public class EmptyDocCusEntryHeaderCollection : DocBaseCusEntryHeaderCollection
		{
			public EmptyDocCusEntryHeaderCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}
		#endregion

		public DocBaseJobDeclaration JobDeclaration
		{
			get
			{
				DocBaseJobDeclaration result = null;
				BaseJobDeclaration jobDeclaration = LCHeader.Parent as BaseJobDeclaration;
				if (jobDeclaration != null)
				{
					result = DocBaseJobDeclaration.New(jobDeclaration, Factory);
				}
				return result;
			}
		}
		#endregion

		#region Implementation

		RoundingHelper RoundingHelper
		{
			get
			{
				if (fRoundingHelper == null)
				{
					fRoundingHelper = CreateRoundingHelper();
				}
				return fRoundingHelper;
			}
		}
		RoundingHelper fRoundingHelper;

		protected virtual RoundingHelper CreateRoundingHelper()
		{
			return new RoundingHelper();
		}

		LandedCostHeader LCHeader
		{
			get { return (LandedCostHeader)WrappedObject; }
		}

		protected override BusinessObject BusinessObjectToLogAgainst
		{
			get
			{
				BusinessObject result = (BusinessObject)LCHeader.Parent;
				return result ?? base.BusinessObjectToLogAgainst;
			}
		}

		public override string ToString()
		{
			return UniqueReferenceNumber;
		}
		#endregion
	}
}
