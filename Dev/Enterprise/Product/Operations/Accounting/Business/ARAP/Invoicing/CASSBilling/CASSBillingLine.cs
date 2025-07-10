using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class CASSBillingLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		public abstract class Schema
		{
			public const string CASSRejectedClaimValueInLocalCurrencyForDisplay = "CASSRejectedClaimValueInLocalCurrencyForDisplay";
			public const string IssueDate = "IssueDate";
			public const string DateOfArrival = "DateOfArrival";
			public const string DateOfDelivery = "DateOfDelivery";
			public const string BranchCode = "BranchCode";
			public const string MAWBNumber = "MAWBNumber";
			public const string CreditorCode = "CreditorCode";
			public const string LoadPort = "LoadPort";
			public const string DischargePort = "DischargePort";
			public const string SystemWeight = "SystemWeight";
			public const string CASSWeight = "CASSWeight";
			public const string CASSWeightUnit = "CASSWeightUnit";
			public const string WeightDifference = "WeightDifference";
			public const string WeightDifferenceMargin = "WeightDifferenceMargin";
			public const string SystemCostAccrualValue = "SystemCostAccrualValue";
			public const string SystemCostPostedValue = "SystemCostPostedValue";
			public const string CASSCostCurrencyNK = "CASSCostCurrencyNK";
			public const string CASSCostCurrencyExchangeRate = "CASSCostCurrencyExchangeRate";
			public const string CASSCostValueInLocalCurrency = "CASSCostValueInLocalCurrency";
			public const string CASSCostAdjustedValueInLocalCurrency = "CASSCostAdjustedValueInLocalCurrency";
			public const string IsCASSAmendment = "IsCASSAmendment";
			public const string NetCASSCost = "NetCASSCost";
			public const string CostDifference = "CostDifference";
			public const string CostDifferenceMargin = "CostDifferenceMargin";
			public const string StatusForBinding = "StatusForBinding";
			public const string ConsolID = "ConsolID";
			public const string Airline2LetterCode = "Airline2LetterCode";
			public const string InvoiceNumber = "InvoiceNumber";
		}

		public CASSBillingLine(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void AddCostLine(CASSCostLine costLine, ZString lineCurrency)
		{
			ForceRecalculateData();

			if (AggregatedCostLine == null)
			{
				AggregatedCostLine = costLine.CreateNewInstance(CASSCostLineType.Aggregated);
				AggregatedCostLine.CurrencyCode = lineCurrency;
			}

			AggregatedCostLine.Merge(costLine);

			AgentCode = costLine.AgentCode;
			AirlinePrefix = costLine.AirlinePrefix;
			AWBNumber = costLine.AWBSerialNumber;
			IssueDate = costLine.DateAWBExecution;
			DateOfArrival = costLine.DateOfArrival;
			DateOfDelivery = costLine.DateOfDelivery;
			LoadPortIATA = costLine.Origin;
			DischargePortIATA = costLine.Destination;
			CASSWeight = costLine.Weight;
			CASSWeightUnit = costLine.WeightUnit;
			CASSCostCurrencyCode = lineCurrency;
			IsGSTApplicable |= costLine.VATIndicator == "Y";
			IsRejectedClaimLine = (costLine.LineType == CASSCostLineType.Rejected);

			ValidateCASSCostValues();
			ValidateCASSClaimStatus();
			ValidateCASSClaimNotClosedWithSameMAWB();
		}

		#region Properties

		public ZBool IsRejectedClaimLine { get; private set; }

		[ReadOnly(true)]
		public ZBool IsGSTApplicable { get; private set; }

		[ReadOnly(true)]
		public ZString AgentCode
		{
			get { return fAgentCode; }
			private set
			{
				fAgentCode = value;
				IsBranchInitialized = false;
			}
		}
		ZString fAgentCode;

		#region Branch

		public ZString BranchCode
		{
			get { return Branch == null ? ZString.Empty : Branch.GB_Code; }
		}

		public ZPropertyInfo BranchCodeInfo
		{
			get { return GetZPropertyInfo(Schema.BranchCode); }
		}

		public GlbBranch Branch
		{
			get
			{
				if (!IsBranchInitialized)
				{
					fBranch = null;
					var agentCodeTrimmed = TrimIATACode(AgentCode);
					if (!string.IsNullOrWhiteSpace(agentCodeTrimmed))
					{
						foreach (GlbBranch branch in GlbCompany.CurrentCompany.Branches)
						{
							if (agentCodeTrimmed == TrimIATACode(Env.Registry.GetIssuingCarrierAgentIATACode(branch.PK.ToGuid())))
							{
								fBranch = Factory.Load<GlbBranch>(branch.PK);
							}
						}
					}
					IsBranchInitialized = true;
					if (fBranch == null)
					{
						IsConsolBranchSystemCostValuesInitialised = false;
					}
				}
				if (fBranch == null && !IsConsolBranchSystemCostValuesInitialised)
				{
					InitializeConsolBranchSystemCostValues();
				}
				return fBranch;
			}
		}
		GlbBranch fBranch;
		bool IsBranchInitialized;

		#endregion

		#region Consol

		public ZString ConsolID
		{
			get
			{
				if (!IsConsolBranchSystemCostValuesInitialised)
				{
					InitializeConsolBranchSystemCostValues();
				}
				return fConsolID;
			}
		}
		ZString fConsolID;

		public ZPropertyInfo ConsolIDInfo
		{
			get { return GetZPropertyInfo(Schema.ConsolID); }
		}

		public ZGuid ConsolPK
		{
			get
			{
				if (!IsConsolBranchSystemCostValuesInitialised)
				{
					InitializeConsolBranchSystemCostValues();
				}
				return fConsolPK;
			}
		}
		ZGuid fConsolPK;

		public ZString ConsolCreateUser
		{
			get
			{
				if (!IsConsolBranchSystemCostValuesInitialised)
				{
					InitializeConsolBranchSystemCostValues();
				}
				return consolCreateUser;
			}
		}
		ZString consolCreateUser;

		public ZDateTime ConsolCreateTime
		{
			get
			{
				if (!IsConsolBranchSystemCostValuesInitialised)
				{
					InitializeConsolBranchSystemCostValues();
				}
				return consolCreateTime;
			}
		}
		ZDateTime consolCreateTime;

		public ZString ConsolLastEditUser
		{
			get
			{
				if (!IsConsolBranchSystemCostValuesInitialised)
				{
					InitializeConsolBranchSystemCostValues();
				}
				return consolLastEditUser;
			}
		}
		ZString consolLastEditUser;

		public ZDateTime ConsolLastEditTime
		{
			get
			{
				if (!IsConsolBranchSystemCostValuesInitialised)
				{
					InitializeConsolBranchSystemCostValues();
				}
				return consolLastEditTime;
			}
		}
		ZDateTime consolLastEditTime;

		#endregion

		#region AWB Numbers

		[ReadOnly(true)]
		public ZString AWBNumber
		{
			get { return fAWBNumber; }
			private set
			{
				fAWBNumber = value;
				IsConsolBranchSystemCostValuesInitialised = false;
			}
		}
		ZString fAWBNumber;

		public ZString MAWBNumber
		{
			get { return AirlinePrefix + AWBNumber; }
		}

		public ZPropertyInfo MAWBNumberInfo
		{
			get { return GetZPropertyInfo(Schema.MAWBNumber, "Master Bill"); }
		}

		#endregion

		#region IssueDate

		[ReadOnly(true)]
		public ZDateTime IssueDate
		{
			get { return fIssueDate; }
			private set { SetNonPersistentPropertyValue(IssueDateInfo, ref fIssueDate, value); }
		}
		ZDateTime fIssueDate;

		public ZPropertyInfo IssueDateInfo
		{
			get { return GetZPropertyInfo(Schema.IssueDate); }
		}

		#endregion

		#region DateOfArrival

		[ReadOnly(true)]
		public ZDateTime DateOfArrival
		{
			get { return fDateOfArrival; }
			private set { SetNonPersistentPropertyValue(DateOfArrivalInfo, ref fDateOfArrival, value); }
		}
		ZDateTime fDateOfArrival;

		public ZPropertyInfo DateOfArrivalInfo
		{
			get { return GetZPropertyInfo(Schema.DateOfArrival); }
		}

		#endregion

		#region DateOfDelivery

		[ReadOnly(true)]
		public ZDateTime DateOfDelivery
		{
			get { return fDateOfDelivery; }
			private set { SetNonPersistentPropertyValue(DateOfDeliveryInfo, ref fDateOfDelivery, value); }
		}
		ZDateTime fDateOfDelivery;

		public ZPropertyInfo DateOfDeliveryInfo
		{
			get { return GetZPropertyInfo(Schema.DateOfDelivery); }
		}

		#endregion

		#region Airline

		[ReadOnly(true)]
		public ZString AirlinePrefix
		{
			get { return fAirlinePrefix; }
			private set
			{
				fAirlinePrefix = value;
				IsCreditorInitialised = false;
				IsConsolBranchSystemCostValuesInitialised = false;
				if (!IsValidationSuspended)
				{
					ValidateCreditorCode();
				}
			}
		}
		ZString fAirlinePrefix;

		public ZString Airline2LetterCode
		{
			get { return Airline == null ? ZString.Empty : Airline.RM_TwoCharacterCode; }
		}

		public ZPropertyInfo Airline2LetterCodeInfo
		{
			get { return GetZPropertyInfo(Schema.Airline2LetterCode); }
		}

		public RefAirline Airline
		{
			get { return Creditor?.MiscServ?.Airline; }
		}

		#endregion

		#region Creditor

		public ZString CreditorCode
		{
			get { return Creditor == null ? ZString.Empty : Creditor.OH_Code; }
		}

		public ZPropertyInfo CreditorCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CreditorCode); }
		}

		public OrgHeader Creditor
		{
			get
			{
				if (!IsCreditorInitialised)
				{
					creditor = GetCreditor();
					IsCreditorInitialised = true;
				}
				return creditor;
			}
		}
		OrgHeader creditor;
		bool IsCreditorInitialised;

#if DEBUG
		public void ResetCreditor_ForTestOnly() => IsCreditorInitialised = false;
#endif

		OrgHeader GetCreditor()
		{
			if (!AirlinePrefix.IsEmpty)
			{
				var orgsForAgencyCheck = GetListOfOrgsForAgencyCheck(AirlinePrefix, Factory);

				var masterBillHomePort = GetHomePortOfTheBestMatchedMasterBill();

				if (masterBillHomePort != null)
				{
					foreach (var creditorForAgencyCheck in orgsForAgencyCheck)
					{
						var agentPorts = creditorForAgencyCheck.Org.CarrierAppointedAgentPorts_Agency
											.Cast<OrgCarrierAppointedAgentPorts>()
											.Where(p => p.Organisation.OH_IsCreditor &&
												p.O5_PortOrCountry.StartsWith(masterBillHomePort.RL_RN_NKCountryCode, StringComparison.Ordinal));

						//Looking for Agency with same port as the MAWB.
						var matchingAgentPort = agentPorts.FirstOrDefault(p => p.O5_PortOrCountry == masterBillHomePort.Code)
							?? agentPorts.FirstOrDefault(p => p.O5_PortOrCountry == masterBillHomePort.RL_RN_NKCountryCode);

						if (matchingAgentPort != null)
						{
							return matchingAgentPort.Organisation;
						}
					}
				}

				//As there is no agency to set as creditor, let's look for a creditor in the parent candidate orgs.
				return orgsForAgencyCheck
					.Where(c => c.Org.OH_IsCreditor)
					.OrderByDescending(c => c.Priority)
					.FirstOrDefault()
					.Org;
			}

			return null;
		}

		static IEnumerable<(OrgHeader Org, int Priority)> GetListOfOrgsForAgencyCheck(ZString airlinePrefix, BusinessObjectFactory factory)
		{
			Argument.NotNullOrEmpty(airlinePrefix, "airlinePrefix");

			var candidateOrgs = new List<(OrgHeader Org, int Priority)>();
			var orgQuery = GetCarrierQuery(airlinePrefix);
			var carriers = factory.Load<OrgHeader>(orgQuery);
			return carriers.Cast<OrgHeader>()
				.Select<OrgHeader, (OrgHeader org, int Priority)>(c => (c, Convert.ToInt32(c.UNLOCO != null && c.UNLOCO.RL_RN_NKCountryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode) * 10 + Convert.ToInt32(c.OH_IsCreditor)))
				.OrderByDescending(cp => cp.Priority);
		}

		static ZDBOnlyQuery GetCarrierQuery(ZString airlinePrefix)
		{
			/*					 Output SQL
			 * ---------------------------------------------------
			 *	(
					OH_PK IN 
					(
						SELECT OM_OH FROM dbo.OrgMiscServ WHERE OM_RM_Airline IN 
						(
							SELECT RM_PK FROM dbo.RefAirline WHERE RM_EagleAddedAirlinePrefixOrAccountingCode = '<airlinePrefix>' 
							AND
							RM_IsActive = 1
						)
					)
				)
				AND
				OH_PK IN 
				(
					SELECT OB_OH FROM dbo.OrgCompanyData WHERE OB_GC = '<current login company>'
				)
			 */
			Argument.NotNullOrEmpty(airlinePrefix, "airlinePrefix");

			var airlineQuery = new ZDBOnlySubQuery(typeof(RefAirline), RefAirlineSchema.PK);
			airlineQuery.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, airlinePrefix);

			var miscServQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
			miscServQuery.AddSubQuery(OrgMiscServSchema.OM_RM_Airline, airlineQuery, JoinCondition.And);

			var companyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			companyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, Env.CurrentCompany.PK);

			var result = new ZDBOnlyQuery(typeof(OrgHeader));
			result.AddSubQuery(miscServQuery, JoinCondition.And);
			result.AddSubQuery(companyDataQuery, JoinCondition.And);

			return result;
		}

#if DEBUG
		public static ZDBOnlyQuery GetCreditorQuery_ExposedForTestOnly(ZString airlinePrefix) => GetCarrierQuery(airlinePrefix);
#endif

		RefUNLOCO GetHomePortOfTheBestMatchedMasterBill()
		{
			var query = new ZQuery(JobMawbSchema.JM_Airline3DigitPrefix, AirlinePrefix);
			query.AddToFilter(JobMawbSchema.JM_MAWB, AWBNumber);
			query.OrderBy = Invariant($"{JobMawbSchema.JM_SystemCreateTimeUtc.Name} DESC");
			var masterBill = Factory.LoadTop1<JobMawb>(query);
			return masterBill?.Branch?.HomePort;
		}

		#endregion

		#region Load Port

		[ReadOnly(true)]
		public ZString LoadPortIATA
		{
			get { return fLoadPortIATA; }
			private set
			{
				fLoadPortIATA = value;
				IsConsolBranchSystemCostValuesInitialised = false;
				IsLoadPortUNLOCOInitialised = false;
				if (!IsValidationSuspended)
				{
					ValidateLoadPort();
				}
			}
		}
		ZString fLoadPortIATA;

		public RefUNLOCO LoadPortUNLOCO
		{
			get
			{
				if (!IsLoadPortUNLOCOInitialised)
				{
					if (!LoadPortIATA.IsEmpty)
					{
						fLoadPortUNLOCO = RefUNLOCO.LoadFromIATA(Factory, LoadPortIATA);
						if (fLoadPortUNLOCO == null)
						{
							fLoadPortUNLOCO = Factory.LoadTop1<RefUNLOCO>(GetUNLOCOQueryByUNLOCOCode(LoadPortIATA));
						}
						IsLoadPortUNLOCOInitialised = true;
					}
				}
				return fLoadPortUNLOCO;
			}
		}
		RefUNLOCO fLoadPortUNLOCO;
		bool IsLoadPortUNLOCOInitialised;

		public ZString LoadPort
		{
			get { return LoadPortUNLOCO == null ? LoadPortIATA : LoadPortUNLOCO.RL_Code; }
		}

		public ZPropertyInfo LoadPortInfo
		{
			get { return GetZPropertyInfo(Schema.LoadPort); }
		}

		#endregion

		#region Discharge Port

		[ReadOnly(true)]
		public ZString DischargePortIATA
		{
			get { return fDischargePortIATA; }
			private set
			{
				fDischargePortIATA = value;
				IsConsolBranchSystemCostValuesInitialised = false;
				IsDischargePortUNLOCOInitialised = false;
				if (!IsValidationSuspended)
				{
					ValidateDischargePort();
				}
			}
		}
		ZString fDischargePortIATA;

		public RefUNLOCO DischargePortUNLOCO
		{
			get
			{
				if (!IsDischargePortUNLOCOInitialised)
				{
					if (!DischargePortIATA.IsEmpty)
					{
						fDischargePortUNLOCO = RefUNLOCO.LoadFromIATA(Factory, DischargePortIATA);
						if (fDischargePortUNLOCO == null)
						{
							fDischargePortUNLOCO = Factory.LoadTop1<RefUNLOCO>(GetUNLOCOQueryByUNLOCOCode(DischargePortIATA));
						}
						IsDischargePortUNLOCOInitialised = true;
					}
				}
				return fDischargePortUNLOCO;
			}
		}
		RefUNLOCO fDischargePortUNLOCO;
		bool IsDischargePortUNLOCOInitialised;

		public ZString DischargePort
		{
			get { return DischargePortUNLOCO == null ? DischargePortIATA : DischargePortUNLOCO.RL_Code; }
		}

		public ZPropertyInfo DischargePortInfo
		{
			get { return GetZPropertyInfo(Schema.DischargePort); }
		}

		#endregion

		#region Weight

		#region System Weight

		public ZDecimal SystemWeight
		{
			get
			{
				if (fSystemWeight == null)
				{
					InitializeSystemWeight();
				}
				return fSystemWeight.Value;
			}
		}
		ZDecimal? fSystemWeight;

		public ZPropertyInfo SystemWeightInfo
		{
			get { return GetZPropertyInfo(Schema.SystemWeight); }
		}

		#endregion

		#region CASSWeight

		[ReadOnly(true)]
		public ZDecimal CASSWeight
		{
			get;
			private set;
		}

		public ZPropertyInfo CASSWeightInfo
		{
			get { return GetZPropertyInfo(Schema.CASSWeight); }
		}

		[MaxLength(2)]
		[ReadOnly(true)]
		public ZString CASSWeightUnit
		{
			get
			{
				return fCASSWeightUnit;
			}
			private set { SetNonPersistentPropertyValue(CASSWeightUnitInfo, ref fCASSWeightUnit, value); }
		}
		ZString fCASSWeightUnit;

		public ZPropertyInfo CASSWeightUnitInfo
		{
			get { return GetZPropertyInfo(Schema.CASSWeightUnit); }
		}

		#endregion

		#region Weight Calculations

		public ZDecimal WeightDifference
		{
			get { return CASSWeight - SystemWeight; }
		}

		public ZPropertyInfo WeightDifferenceInfo
		{
			get { return GetZPropertyInfo(Schema.WeightDifference); }
		}

		public ZDecimal WeightDifferenceMargin
		{
			get { return SystemWeight == 0M ? 100M : WeightDifference / SystemWeight * 100M; }
		}

		public ZPropertyInfo WeightDifferenceMarginInfo
		{
			get { return GetZPropertyInfo(Schema.WeightDifferenceMargin); }
		}

		#endregion

		#endregion

		#region Cost Values

		#region System Cost Values

		public ZDecimal SystemCostAccrualValue
		{
			get
			{
				if (!IsConsolBranchSystemCostValuesInitialised)
				{
					InitializeConsolBranchSystemCostValues();
				}
				return fSystemCostAccrualValue;
			}
		}
		ZDecimal fSystemCostAccrualValue;

		public ZPropertyInfo SystemCostAccrualValueInfo
		{
			get { return GetZPropertyInfo(Schema.SystemCostAccrualValue); }
		}

		internal Dictionary<ZGuid, Tuple<ZDecimal, ZString>> SystemCostValueByChargeCodes
		{
			get { return systemCostValueByChargeCodes; }
		}
		readonly Dictionary<ZGuid, Tuple<ZDecimal, ZString>> systemCostValueByChargeCodes = new Dictionary<ZGuid, Tuple<ZDecimal, ZString>>();

		internal Dictionary<Tuple<ZGuid, ZGuid>, SystemChargeDataToImport> SystemChargeDataByJobAndChargeCode
		{
			get { return systemChargeDataByJobAndChargeCode; }
		}
		readonly Dictionary<Tuple<ZGuid, ZGuid>, SystemChargeDataToImport> systemChargeDataByJobAndChargeCode = new Dictionary<Tuple<ZGuid, ZGuid>, SystemChargeDataToImport>();

		public ZDecimal SystemCostPostedValue
		{
			get
			{
				if (!IsConsolBranchSystemCostValuesInitialised)
				{
					InitializeConsolBranchSystemCostValues();
				}
				return fSystemCostPostedValue;
			}
		}
		ZDecimal fSystemCostPostedValue;

		public ZPropertyInfo SystemCostPostedValueInfo
		{
			get { return GetZPropertyInfo(Schema.SystemCostPostedValue); }
		}

		#endregion

		#region CASS Cost Currency

		[ReadOnly(true)]
		public ZString CASSCostCurrencyCode
		{
			get { return fCASSCostCurrencyCode; }
			private set
			{
				fCASSCostCurrencyCode = value;
				IsCASSCostCurrencyInitialised = false;
				if (!IsValidationSuspended)
				{
					ValidateCurrencyCode();
				}
			}
		}
		ZString fCASSCostCurrencyCode;

		public RefCurrency CASSCostCurrency
		{
			get
			{
				if (!IsCASSCostCurrencyInitialised)
				{
					fCASSCostCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CASSCostCurrencyCode);
					IsCASSCostCurrencyInitialised = true;
					if (!IsExchangeRateCurrencyBeingSet)
					{
						IsExchangeRateCurrencyBeingSet = true;
						try
						{
							ExchangeRate.Currency = ZString.Empty;
							if (fCASSCostCurrency != null)
							{
								ExchangeRate.Currency = fCASSCostCurrency.RX_Code;
							}
						}
						finally
						{
							IsExchangeRateCurrencyBeingSet = false;
						}
					}
				}
				return fCASSCostCurrency;
			}
		}
		RefCurrency fCASSCostCurrency;
		bool IsCASSCostCurrencyInitialised;

		ZExchangeRate ExchangeRate
		{
			get
			{
				if (fExchangeRate == null)
				{
					fExchangeRate = new ZAccExchangeRate(this, ExchangeRateType.Buy, CASSCostCurrencyExchangeRateInfo, (ZPropertyInfoString)CASSCostCurrencyNKInfo, null);
					fExchangeRate.IsCurrencyRequired = true;
					fExchangeRate.IsRateRequired = true;
					if (!IsExchangeRateCurrencyBeingSet)
					{
						IsExchangeRateCurrencyBeingSet = true;
						try
						{
							fExchangeRate.Currency = ZString.Empty;
							if (CASSCostCurrency != null)
							{
								fExchangeRate.Currency = CASSCostCurrency.RX_Code;
							}
						}
						finally
						{
							IsExchangeRateCurrencyBeingSet = false;
						}
					}
				}
				return fExchangeRate;
			}
		}
		ZExchangeRate fExchangeRate;
		bool IsExchangeRateCurrencyBeingSet;

		void DeleteExchangeRate()
		{
			fExchangeRate?.Delete();
			fExchangeRate = null;
		}

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "CASSCostCurrencyNK is used internally")]
		ZString CASSCostCurrencyNK { get; set; } // for ExchangeRate using

		ZPropertyInfo CASSCostCurrencyNKInfo
		{
			get { return GetZPropertyInfo(Schema.CASSCostCurrencyNK); }
		}

		ZDecimal CASSCostCurrencyExchangeRate { get; set; }

		ZPropertyInfo CASSCostCurrencyExchangeRateInfo
		{
			get { return GetZPropertyInfo(Schema.CASSCostCurrencyExchangeRate); }
		}

		#endregion

		#region CASS Cost Values

		#region Total Value

		public ZDecimal CASSCostValue
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				if (AggregatedCostLine != null)
				{
					result = AggregatedCostLine.GetCASSCost(false);
				}

				return result;
			}
		}

		public ZDecimal CASSCostValueInLocalCurrency
		{
			get { return Env.CurrentCompany.ExchangeRate.ForeignToLocal(CASSCostValue, ExchangeRate.Rate); }
		}

		public ZPropertyInfo CASSCostValueInLocalCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.CASSCostValueInLocalCurrency); }
		}

		public ZDecimal CASSCostValueInLocalCurrencyForDisplay
		{
			get { return IsRejectedClaimLine ? ZDecimal.Zero : CASSCostValueInLocalCurrency; }
		}

		public ZPropertyInfo CASSCostValueInLocalCurrencyForDisplayInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CASSCostValueInLocalCurrencyForDisplay), x => CASSCostValueInLocalCurrencyInfo); }
		}

		public ZDecimal CASSRejectedClaimValueInLocalCurrencyForDisplay
		{
			get { return IsRejectedClaimLine ? CASSCostValueInLocalCurrency : ZDecimal.Zero; }
		}

		public ZPropertyInfo CASSRejectedClaimValueInLocalCurrencyForDisplayInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CASSRejectedClaimValueInLocalCurrencyForDisplay, x => CASSCostValueInLocalCurrencyInfo); }
		}

		#endregion

		#region Tax Value

		public ZDecimal CASSCostTaxValue
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				if (AggregatedCostLine != null)
				{
					result = AggregatedCostLine.GetTax(false);
				}
				return result;
			}
		}

		#endregion

		#region Adjusted Total Value

		public ZDecimal CASSCostAdjustedValue
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				if (AggregatedCostLine != null)
				{
					result = AggregatedCostLine.GetCASSCost(true);
				}
				return -1 * result;
			}
		}

		public ZDecimal CASSCostAdjustedValueInLocalCurrency
		{
			get { return Env.CurrentCompany.ExchangeRate.ForeignToLocal(CASSCostAdjustedValue, ExchangeRate.Rate); }
		}

		public ZPropertyInfo CASSCostAdjustedValueInLocalCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.CASSCostAdjustedValueInLocalCurrency); }
		}

		#endregion

		#region Is CASS Amendment

		public ZBool IsCASSAmendment
		{
			get { return CASSCostAdjustedValue != 0; }
		}

		public ZPropertyInfo IsCASSAmendmentInfo
		{
			get { return GetZPropertyInfo(Schema.IsCASSAmendment); }
		}

		#endregion

		#region Adjusted Tax Value

		public ZDecimal CASSCostTaxAdjustedValue
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				if (AggregatedCostLine != null)
				{
					result = AggregatedCostLine.GetTax(true);
				}
				return -1 * result;
			}
		}

		#endregion

		#endregion

		#region Cost Values Calculations

		public ZDecimal NetCASSCost
		{
			get { return CASSCostAdjustedValueInLocalCurrency + CASSCostValueInLocalCurrency; }
		}

		public ZPropertyInfo NetCASSCostInfo
		{
			get { return GetZPropertyInfo(Schema.NetCASSCost); }
		}

		public ZDecimal CostDifference
		{
			get { return SystemCostAccrualValue - NetCASSCost; }
		}

		public ZPropertyInfo CostDifferenceInfo
		{
			get { return GetZPropertyInfo(Schema.CostDifference); }
		}

		public ZDecimal CostDifferenceMargin
		{
			get { return SystemCostAccrualValue == 0M ? 100M : CostDifference / SystemCostAccrualValue * 100M; }
		}

		public ZPropertyInfo CostDifferenceMarginInfo
		{
			get { return GetZPropertyInfo(Schema.CostDifferenceMargin); }
		}

		#endregion

		#endregion

		#region Status
		public StatusType Status
		{
			get
			{
				StatusType result;
				if (!ConsolPK.IsValid)
				{
					result = StatusType.NotInSystem;
				}
				else if (CostDifference == 0M)
				{
					result = StatusType.Exact;
				}
				else
				{
					result = CostDifference > 0M ? StatusType.Underbilled : StatusType.Overbilled;
				}

				return result;
			}
		}

		public enum StatusType
		{
			Overbilled,
			NotInSystem,
			Underbilled,
			Exact
		}

		public ZString StatusForBinding
		{
			get
			{
				switch (Status)
				{
					case StatusType.Overbilled:
						return Res.GetString("94939414-4E08-49B7-AEDC-41194B4FF44F", "Over-billed");
					case StatusType.NotInSystem:
						return Res.GetString("C5577B7E-2592-4524-AB8F-EF01416FA6F8", "Not In System");
					case StatusType.Underbilled:
						return Res.GetString("287E0C0C-0A5D-4C0D-A387-2A2E7DFD59E5", "Under-billed");
					case StatusType.Exact:
						return Res.GetString("188AE264-8224-47B1-A2C7-548AF95816AB", "Exact");
					default:
						{
							ErrorReporter.ReportOnce(Invariant($"Invalid Status value {Status}"));
							return ZString.Empty;
						}
				}
			}
		}

		public ZPropertyInfo StatusForBindingInfo
		{
			get { return GetZPropertyInfo(Schema.StatusForBinding); }
		}

		#endregion

		#region CASSBilling Claim

		internal CASSBillingClaimCreator ClaimCreator
		{
			get
			{
				if (claimCreator == null)
				{
					claimCreator = new CASSBillingClaimCreator(this);
				}

				return claimCreator;
			}
		}
		CASSBillingClaimCreator claimCreator;

		#endregion

		#region Gateway Billing

		public bool IsForGatewayBilling
		{
			get
			{
				if (!IsConsolBranchSystemCostValuesInitialised)
				{
					InitializeConsolBranchSystemCostValues();
				}
				return isForGatewayBilling;
			}
		}
		bool isForGatewayBilling;

		public Job GatewayBillingJob
		{
			get
			{
				Job result = null;
				if (IsForGatewayBilling)
				{
					var jobQuery = new ZQuery(JobHeaderSchema.JH_ParentID, ConsolPK);
					jobQuery.AddToFilter(JobHeaderSchema.JH_GC, Env.CurrentCompany.PK);
					result = Factory.LoadTop1<Job>(jobQuery);
				}
				return result;
			}
		}

		#endregion

		#region CostLine

		public CASSCostLine AggregatedCostLine
		{
			get;
			private set;
		}

		void DeleteAggregatedCostLine()
		{
			AggregatedCostLine?.Delete();
			AggregatedCostLine = null;
		}

		#endregion

		#region Invoice Number

		[ResourceStringData("APInvoiceNumber", Caption = "AP Invoice Number", ShortCaption = "Invoice Num.")]
		[MaxLength(AccTransactionHeader.Schema.AH_TransactionNumMaxLength)]
		public ZString InvoiceNumber
		{
			get { return invoiceNumber; }
			set
			{
				if (SetNonPersistentPropertyValue(InvoiceNumberInfo, ref invoiceNumber, value))
				{
					IsInvoiceNumberChanged = true;
					ClearCriticalError(Schema.InvoiceNumber);
					ValidateInvoiceNumber();
				}
			}
		}
		ZString invoiceNumber;

		public ZPropertyInfo InvoiceNumberInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceNumber); }
		}

		public bool IsInvoiceNumberChanged { get; set; }

		#endregion

		public Dictionary<ZString, ZString> CriticalErrors
		{
			get
			{
				if (criticalErrors == null)
				{
					criticalErrors = new Dictionary<ZString, ZString>();
				}

				return criticalErrors;
			}
		}
		Dictionary<ZString, ZString> criticalErrors;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCurrencyCode();
			ValidateLoadPort();
			ValidateDischargePort();
			ValidateSystemCostValue();
			ValidateCreditorCode();
			ValidateCASSCostValues();
			ValidateCASSClaimStatus();
			ValidateCASSClaimNotClosedWithSameMAWB();
		}

		protected void ValidateCurrencyCode()
		{
			CASSCostValueInLocalCurrencyInfo.ClearAllNotifications();
			if (CASSCostCurrency == null)
			{
				CASSCostValueInLocalCurrencyInfo.AddError(Res.GetString("991aa5b9-d366-431f-8e5a-3ade4b7ee1d0", "Can't find system currency for CASS currency code '{0}'", CASSCostCurrencyCode));
			}
			else
			{
				if (CASSCostCurrencyExchangeRate == 0M)
				{
					CASSCostValueInLocalCurrencyInfo.AddError(Res.GetString("805cfb21-0985-465c-9928-49e539012669", "Today's BUY rate is not set for currency '{0}'", CASSCostCurrencyCode));
					CASSCostValueInLocalCurrencyInfo.AddAllNotificationsFrom(CASSCostCurrencyNKInfo);
					CASSCostValueInLocalCurrencyInfo.AddAllNotificationsFrom(CASSCostCurrencyExchangeRateInfo);
				}
			}
		}

		protected void ValidateLoadPort()
		{
			LoadPortInfo.ClearAllNotifications();
			if (LoadPortUNLOCO == null)
			{
				LoadPortInfo.AddError(Res.GetString("c515a57e-f598-44cd-a76e-45654f371030", "Can't find UNLOCO for IATA code '{0}'", LoadPortIATA));
			}
		}

		protected void ValidateDischargePort()
		{
			DischargePortInfo.ClearAllNotifications();
			if (DischargePortUNLOCO == null)
			{
				DischargePortInfo.AddError(Res.GetString("c515a57e-f598-44cd-a76e-45654f371030", "Can't find UNLOCO for IATA code '{0}'", DischargePortIATA));
			}
		}

		protected void ValidateSystemCostValue()
		{
			SystemCostAccrualValueInfo.ClearAllNotifications();
			if (AccountingConfigurationRegistry.Instance.CASSChargeCodes.Value.Count == 0)
			{
				SystemCostAccrualValueInfo.AddError(Res.GetString("649eafe4-da79-4bf2-99c2-86e047dde21f", "{0} registry item ({1}) can't be empty.", AccountingConfigurationRegistry.Instance.CASSChargeCodes.Caption, AccountingConfigurationRegistry.Instance.CASSChargeCodes.Category));
			}
			if (AccountingConfigurationRegistry.Instance.CASSGLAccount.Value == Guid.Empty)
			{
				SystemCostAccrualValueInfo.AddError(Res.GetString("649eafe4-da79-4bf2-99c2-86e047dde21f", "{0} registry item ({1}) can't be empty.", AccountingConfigurationRegistry.Instance.CASSGLAccount.Caption, AccountingConfigurationRegistry.Instance.CASSGLAccount.Category));
			}
		}

		protected void ValidateCreditorCode()
		{
			CreditorCodeInfo.ClearAllNotifications();
			if (Creditor == null)
			{
				var errMsg = Res.GetString("293415cc-39ba-4d20-90d4-29baba49511b", "A Creditor for this MAWB could not be identified. Please check that there is a Payables organization in your company that is also flagged as the Carrier for the Airline with code '{0}'.", AirlinePrefix);
				CreditorCodeInfo.AddError(errMsg);
			}
			else if (ClaimCreator.NeedToCreateNewClaim && !(Creditor?.ContactsActive.Any() ?? false))
			{
				var errMsg = Res.GetString("bf3f253e-58d6-4570-a908-f139434dcecd", "Cannot create claim for line(s) with MAWB: {0} as there is no active contact defined for creditor: {1}", MAWBNumber, Creditor.OH_Code);
				CreditorCodeInfo.AddError(errMsg);
			}
		}

		protected void ValidateCASSClaimNotClosedWithSameMAWB()
		{
			MAWBNumberInfo.ClearAllNotifications();
			if (claimCreator.NeedToCreateNewClaim && APAccQueryClaim.IsThereAnyOpenClaimWithMAWB(Factory, MAWBNumber))
			{
				var errMsg = Res.GetString("56c733f4-1eef-47da-adb7-c07056403955", "An open claim with same MAWB exists. Multiple open claims with same MAWB is not allowed.");
				MAWBNumberInfo.AddError(errMsg);
			}
		}

		protected void ValidateCASSClaimStatus()
		{
			StatusForBindingInfo.ClearAllNotifications();

			var warningMsg = ClaimCreator.GetClaimWarningMessage();
			if (!string.IsNullOrWhiteSpace(warningMsg))
			{
				StatusForBindingInfo.AddWarning(warningMsg);
			}
		}

		protected void ValidateCASSCostValues()
		{
			RemoveRowNotification(InformationOnlyLineWarning);

			if (CASSCostValue == 0 && CASSCostAdjustedValue == 0)
			{
				AddRowNotification(InformationOnlyLineWarning);
			}
		}

		public void ValidateInvoiceNumber()
		{
			InvoiceNumberInfo.ClearAllNotifications();

			if (!InvoiceNumber.IsEmpty)
			{
				InvoiceNumberInfo.AddWarning(Res.GetString("5a47c6cc-2708-4290-b731-90efd1357b0d", "Some CASS Costs have an AP Invoice Number entered. The system will calculate and group CASS Cost with AP Invoice Number against respective AP Invoices and CASS Cost without AP Invoice Number into separate AP Invoice with system generated AP Invoice Number."));

				var baseInvoiceNumber = InvoiceLiteralNumberGenerator.GetAutoAPInvoiceNumberForCASS(CASSCostCurrencyCode);
				if (InvoiceNumber.StartsWith(baseInvoiceNumber, StringComparison.OrdinalIgnoreCase))
				{
					var suffix = InvoiceNumber.SubstringSafe(baseInvoiceNumber.Length);
					if (!string.IsNullOrEmpty(suffix))
					{
						var errorMessage = CommonUtils.ValidaeSuffixForGetNumberRepresentation(suffix);
						if (!string.IsNullOrEmpty(errorMessage))
						{
							AddCriticalError(InvoiceNumberInfo.Name, Res.GetString("ab8a3639-5fc0-4e46-81c0-6e1f9588bc05", "Entered invoice number's format matches with the auto generated invoice number. But suffix:'{0}' is not valid.\r\nError: {1}\r\nSuffix must start with \"/\" followed by a text having maximum 2 letters (e.g \"/A\", \"/AB\" etc.)", suffix, errorMessage));
						}
					}
				}
			}

			ZString errorMsg = "";
			if (CriticalErrors.TryGetValue(Schema.InvoiceNumber, out errorMsg))
			{
				InvoiceNumberInfo.AddError(errorMsg);
			}
		}

		public void ValidateInvoiceAndClaimRelatedCriticalProperties()
		{
			ClearCriticalError(MAWBNumberInfo.Name);
			ClearCriticalError(CreditorCodeInfo.Name);

			ValidateCreditorCode();
			ValidateCASSClaimNotClosedWithSameMAWB();

			var propInfos = new ZPropertyInfo[] { MAWBNumberInfo, CreditorCodeInfo };
			foreach (var info in propInfos)
			{
				info.GetErrors().Select(x => x.Message).ForEach((msg) => AddCriticalError(info.Name, msg));
			}
		}

		INotification InformationOnlyLineWarning
		{
			get
			{
				return new Notification(CargoWise.ComponentModel.NotificationType.Warning,
					Res.GetString("D1C8839B-F3A8-48ba-B823-DA5176FDF1F0", "No costs or adjustments on this line. This line is for information only and will not post to an invoice."));
			}
		}

		#endregion

		#region Methods

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<ZGuid, Tuple<ZDecimal, ZString>> GetAccrulaAmountsByChargeCodes()
		{
			if (!IsConsolBranchSystemCostValuesInitialised)
			{
				InitializeConsolBranchSystemCostValues();
			}
			return SystemCostValueByChargeCodes;
		}

		public void ForceRecalculateData()
		{
			IsBranchInitialized = false;
			IsConsolBranchSystemCostValuesInitialised = false;
			IsBranchAndDepartmentForExistingChargesInitialised = false;
			IsCreditorInitialised = false;
			IsLoadPortUNLOCOInitialised = false;
			IsDischargePortUNLOCOInitialised = false;
			IsCASSCostCurrencyInitialised = false;
			fSystemWeight = null;
			fExchangeRate = null;
		}

		public override void Delete()
		{
			DeleteExchangeRate();
			DeleteAggregatedCostLine();
			SystemChargeDataByJobAndChargeCode.Clear();
			SystemCostValueByChargeCodes.Clear();

			base.Delete();
		}

		public void AddCriticalError(string propertyName, string errorMsg)
		{
			if (!CriticalErrors.ContainsKey(propertyName))
			{
				CriticalErrors.Add(propertyName, errorMsg);
			}
		}

		public void ClearCriticalErrors()
		{
			CriticalErrors.Clear();
		}

		public void ClearCriticalError(string propertyName)
		{
			CriticalErrors.Remove(propertyName);
		}

		#endregion

		#region Implementation

		void InitializeSystemWeight()
		{
			if (ConsolPK.IsValid)
			{
				var sqlText = @" 
SELECT 
	JobShipment.JS_UnitOfWeight AS WeightUnit, JobShipment.JS_ActualWeight AS Weight
FROM 
	dbo.JobShipment 
		INNER JOIN
	dbo.JobConShipLink 
		ON JobShipment.JS_PK = JobConShipLink.JN_JS 
WHERE
	JobConShipLink.JN_JK = @Consol
";
				var parameters = new ZSqlParameterCollection();
				parameters.Add("@Consol", ConsolPK, JobConShipLinkSchema.JN_JK);

				var collection = new DynamicBusinessObjectCollection(Factory);
				collection.Load(sqlText, parameters);

				fSystemWeight = 0;
				foreach (DynamicBusinessObject bizo in collection)
				{
					var systemWeightUnit = (ZString)bizo["WeightUnit"];
					fSystemWeight += Constants.Weight.Convert((ZDecimal)bizo["Weight"],
						systemWeightUnit == "" ? CASSWeightUnit : systemWeightUnit, CASSWeightUnit);
				}
			}
			else
			{
				fSystemWeight = 0;
			}
		}

#if DEBUG
		internal
#endif
		void InitializeConsolBranchSystemCostValues()
		{
			if (IsInitializeConsolBranchSystemCostValuesRunningNow)
			{
				return;
			}

			IsInitializeConsolBranchSystemCostValuesRunningNow = true;
			try
			{
				fConsolPK = ZGuid.Empty;
				fConsolID = ZString.Empty;
				fSystemCostAccrualValue = 0M;
				fSystemCostPostedValue = 0M;
				SystemCostValueByChargeCodes.Clear();
				isForGatewayBilling = false;
				consolCreateUser = ZString.Empty;
				consolCreateTime = ZDateTime.Empty;
				consolLastEditUser = ZString.Empty;
				consolLastEditTime = ZDateTime.Empty;

				var chargeCodePKsFromRegistry = AggregatedCostLine?.GetCASSChargeCodePKsFromRegistry();
				var chargeCodes = new HashSet<AccChargeCode>();
				if (chargeCodePKsFromRegistry != null && chargeCodePKsFromRegistry.Length > 0)
				{
					foreach (ZGuid chargeCodePK in chargeCodePKsFromRegistry)
					{
						AccChargeCode chargeCode = Factory.Load<AccChargeCode>(chargeCodePK);
						if (chargeCode != null)
						{
							chargeCodes.Add(chargeCode);
						}
					}
				}

				if (!IsConsolBranchSystemCostValuesInitialised && chargeCodes.Count > 0)
				{
					var collection = GetConsolBranchSystemCostValuesCollection(chargeCodes);

					if (collection.Count > 0)
					{
						DynamicBusinessObject bizo = collection[0];
						fConsolPK = (ZGuid)bizo[JobConsolSchema.Constants.PK];
						var gatewayBillingRate = (ZInt)bizo["IsGatewayBilling"];
						var isCFS = (ZBool)bizo[JobConsolSchema.Constants.JK_IsCFS];

						if (fConsolPK.IsValid && gatewayBillingRate > 0)
						{
							isForGatewayBilling = true;
							//ToDo: everything below this line in this if statement should be redundant now as GetGatewayConsols function has been added. Which means we also don't need the gatewayBillingRate in the SQL.

							isForGatewayBilling = gatewayBillingRate == 5;

							if (gatewayBillingRate == 4 && !isCFS)
							{
								var gatewayBillingAccrualSumValue = chargeCodes.Sum(x => (ZDecimal)bizo["Unposted_LocalCostAmt_" + PKColumnSuffix(x.PK)]);
								isForGatewayBilling = gatewayBillingAccrualSumValue != 0m;
							}

							if (!isForGatewayBilling)
							{
								isForGatewayBilling = Factory.GetCachedValue(fConsolPK.ToStringKey(), delegate
								{
									var factory = new BusinessObjectFactory() { RefreshEnabled = false };
									var consol = factory.Load<ForwardingConsol>(fConsolPK);
									return consol != null && consol.IsGateway();
								}, CacheStalenessPolicy.StaleOnFactorySave);
							}

							if (!isForGatewayBilling)
							{
								if (collection.Count > 1)
								{
									bizo = collection[1];
									fConsolPK = (ZGuid)bizo[JobConsolSchema.Constants.PK];
								}
								else
								{
									fConsolPK = ZGuid.Invalid;
								}
							}
						}

						if (fConsolPK.IsValid)
						{
							fConsolID = (ZString)bizo[JobConsolSchema.Constants.JK_UniqueConsignRef];
							consolCreateUser = (ZString)bizo[JobConsolSchema.Constants.JK_SystemCreateUser];
							consolCreateTime = (ZDateTime)bizo[JobConsolSchema.Constants.JK_SystemCreateTimeUtc];
							consolLastEditUser = (ZString)bizo[JobConsolSchema.Constants.JK_SystemLastEditUser];
							consolLastEditTime = (ZDateTime)bizo[JobConsolSchema.Constants.JK_SystemLastEditTimeUtc];

							fSystemCostPostedValue += (ZDecimal)bizo["Posted_LocalCostAmt"];
							foreach (AccChargeCode chargeCode in chargeCodes)
							{
								var localCstAmt = (ZDecimal)bizo["Unposted_LocalCostAmt_" + PKColumnSuffix(chargeCode.PK)];
								var apportionmentMethod = (ZString)bizo["ApportionmentMethod_" + PKColumnSuffix(chargeCode.PK)];
								SystemCostValueByChargeCodes[chargeCode.PK] = new Tuple<ZDecimal, ZString>(localCstAmt, apportionmentMethod);
								fSystemCostAccrualValue += SystemCostValueByChargeCodes[chargeCode.PK].Item1;
							}
						}
					}

					InitializeBranchAndDepartmentForExistingCharges(chargeCodes);
				}
				IsConsolBranchSystemCostValuesInitialised = true;
				fSystemWeight = null;

				if (!IsValidationSuspended)
				{
					ValidateSystemCostValue();
				}
			}
			finally
			{
				IsInitializeConsolBranchSystemCostValuesRunningNow = false;
			}

			IsBranchAndDepartmentForExistingChargesInitialised = true;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "SQL query doesn't need IFormatting in this case")]
#if DEBUG
		internal
#endif
		void InitializeBranchAndDepartmentForExistingCharges(IEnumerable<AccChargeCode> chargeCodes)
		{
			if (IsBranchAndDepartmentForExistingChargesInitialised)
			{
				return;
			}

			SystemChargeDataByJobAndChargeCode.Clear();

			var parameters = new ZSqlParameterCollection
			{
				{ "@CompanyPK", Env.CurrentCompany.PK, JobHeaderSchema.JH_GC },
				{ "@MasterBillNum", MAWBNumber, JobConsolSchema.JK_MasterBillNum },
				{ "@LineType", ZArchitecture.Core.TransactionLineTypes.Accrual, AccTransactionLinesSchema.AL_LineType },
				{ "@GW", Constants.GatewaySuffixForJobHeaderDeprecated, JobConsolSchema.JK_UniqueConsignRef },
				{ "@Creditor", Creditor?.PK ?? ZGuid.Empty, JobChargeSchema.JR_OH_CostAccount },
				{
					"@ByCreditor",
					AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.GetFallBackValueAtAllLevels(
						EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					CargoWise.Schema.Schema.GenericBitSchemaColumn
				}
			};

			var chargeCodePKs = string.Join(",", chargeCodes.Select(c => $"'{c.PK}'"));

			var sqlText = Invariant($@" 
WITH GttConsols
AS
(
	SELECT JK_PK as GttConsol_PK from GetGatewayConsols(@CompanyPK, 'GTT')
)

SELECT JR_JH JobPK, JR_AC ChargeCodePK, JR_GB BranchPK, JR_GE DepartmentPK, JR_RX_NKCostCurrency CostCurrencyCode, JR_OSCostExRate CostExRate, JR_OSCostAmt OsCostAmount, JR_OH_SellAccount DebtorPK,
		CASE WHEN JR_E6 IS NULL THEN 0 ELSE 1 END AS IsApportioned,
		1 AS IsGatewayBilling
FROM
	dbo.JobConsol
	INNER JOIN dbo.JobHeader ON JH_ParentID = JK_PK
	INNER JOIN dbo.JobCharge ON JH_PK = JR_JH 	
	LEFT JOIN dbo.AccTransactionLines ON JR_AL_APLine = AL_PK
WHERE
	JK_IsForwarding = 1 
	AND EXISTS (SELECT 1 FROM GttConsols WHERE GttConsol_PK = JK_PK)
	AND JK_MasterBillNum = @MasterBillNum
	AND JH_GC = @CompanyPK
	AND ((AL_LineType = @LineType AND AL_ReverseDate IS NULL) OR (AL_LineType IS NULL AND JR_LocalCostAmt != 0))
	AND (JR_OH_CostAccount IS NULL OR JR_OH_CostAccount = @Creditor OR @ByCreditor = 0)
	AND JR_AC in ({chargeCodePKs})

UNION ALL

SELECT JR_JH JobPK, JR_AC ChargeCodePK, JR_GB BranchPK, JR_GE DepartmentPK, JR_RX_NKCostCurrency CostCurrencyCode, JR_OSCostExRate CostExRate, JR_OSCostAmt OsCostAmount, JR_OH_SellAccount DebtorPK,
		CASE WHEN JR_E6 IS NULL THEN 0 ELSE 1 END AS IsApportioned,
		0 AS IsGatewayBilling
FROM
	dbo.JobConsol
	INNER JOIN dbo.JobConShipLink ON JK_PK = JN_JK  
	INNER JOIN dbo.JobHeader ON JH_ParentID = JN_JS
	INNER JOIN dbo.JobCharge ON JH_PK = JR_JH
	LEFT JOIN dbo.AccTransactionLines ON JR_AL_APLine = AL_PK 
WHERE
	JK_MasterBillNum = @MasterBillNum
	AND JH_GC = @CompanyPK
	AND ((AL_LineType = @LineType AND AL_ReverseDate IS NULL) OR (AL_LineType IS NULL AND JR_LocalCostAmt != 0))
	AND (JR_OH_CostAccount IS NULL OR JR_OH_CostAccount = @Creditor OR @ByCreditor = 0)
	AND JR_AC in ({chargeCodePKs})

ORDER BY
	 IsGatewayBilling DESC, JR_JH, JR_AC, IsApportioned DESC
");

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sqlText, parameters);

			foreach (DynamicBusinessObject bo in collection)
			{
				var charge = new SystemChargeDataToImport(
					(ZGuid)bo["JobPK"],
					(ZGuid)bo["ChargeCodePK"],
					(ZGuid)bo["BranchPK"],
					(ZGuid)bo["DepartmentPK"],
					(ZString)bo["CostCurrencyCode"],
					(ZDecimal)bo["CostExRate"],
					(ZDecimal)bo["OsCostAmount"],
					(ZGuid)bo["DebtorPK"],
					(ZInt)bo["IsApportioned"] == 1);

				var key = Tuple.Create(charge.JR_JH, charge.JR_AC);

				if (!SystemChargeDataByJobAndChargeCode.ContainsKey(key))
				{
					SystemChargeDataByJobAndChargeCode.Add(key, charge);
				}
			}
		}

		bool IsBranchAndDepartmentForExistingChargesInitialised;

		static string PKColumnSuffix(ZGuid pk)
		{
			return pk.ToString().Replace('-', '_');
		}

		bool IsConsolBranchSystemCostValuesInitialised;
		bool IsInitializeConsolBranchSystemCostValuesRunningNow;

#if DEBUG
		internal
#endif
		DynamicBusinessObjectCollection GetConsolBranchSystemCostValuesCollection(IEnumerable<AccChargeCode> chargeCodes)
		{
			var chargeCodePks = chargeCodes.Select(cc => cc.PK).ToList();

			var parameters = new ZSqlParameterCollection
			{
				{ "@CompanyPK", Env.CurrentCompany.PK, JobHeaderSchema.JH_GC },
				{ "@MasterBillNum", MAWBNumber, JobConsolSchema.JK_MasterBillNum },
				{ "@Creditor", Creditor?.PK ?? ZGuid.Empty, JobChargeSchema.JR_OH_CostAccount },
				{ "@LineType", TransactionLineTypes.Accrual, AccTransactionLinesSchema.AL_LineType },
				{
					"@ByCreditor", AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.GetFallBackValueAtAllLevels(
						EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					CargoWise.Schema.Schema.GenericBitSchemaColumn
				},
				{ "@GW", Constants.GatewaySuffixForJobHeaderDeprecated, JobConsolSchema.JK_UniqueConsignRef },
				{ "@E6_ParentTableCode", JobConsolSchema.Constants.Prefix, JobConsolCostSchema.E6_ParentTableCode }
			};

			var formattedParametersNames = new StringBuilder();
			var formattedParameters = new StringBuilder();
			foreach (var chargeCodePk in chargeCodePks)
			{
				var chargeCodePKColumnSuffix = PKColumnSuffix(chargeCodePk);
				formattedParametersNames.Append(Invariant($"Unposted_LocalCostAmt_{chargeCodePKColumnSuffix}, ApportionmentMethod_{chargeCodePKColumnSuffix}, "));
				formattedParameters.Append(Invariant($@"
	Unposted_LocalCostAmt_{chargeCodePKColumnSuffix} = SUM(CASE 
			WHEN (AL_LineType = @LineType OR AL_LineType is NULL) AND (JR_OH_CostAccount IS NULL OR JR_OH_CostAccount = @Creditor OR @ByCreditor = 0) 
				THEN (CASE WHEN JR_AC = '{chargeCodePk}' THEN JR_LocalCostAmt END) 
		END),
	ApportionmentMethod_{chargeCodePKColumnSuffix} = 
			(
				SELECT	dbo.CLRCssvAgg(CCAppRow)
				FROM	(SELECT CONCAT(E6_OH_Creditor,'|', E6_RX_NKCurrency,'|', E6_OSCostAmount,'|', E6_ApportionmentMethod,'|', E6_AT_TaxRate,'|', E6_A9_VATClass) as CCAppRow
						 FROM	dbo.JobConsolCost 
						 WHERE	E6_AC_ChargeCode = '{chargeCodePk}' 
								AND E6_GC = @CompanyPK 
								AND E6_ParentID = JK_PK 
								AND E6_ParentTableCode = @E6_ParentTableCode
								AND E6_AH_APInvoice IS NULL) CCAppInfo
			),"));
			}

			var chargeCodePKs = string.Join(",", chargeCodePks.Select(pk => Invariant($"'{pk}'")));

			var sqlText = Invariant($@"
WITH GttConsols
AS
(
	SELECT JK_PK as GttConsol_PK from GetGatewayConsols(@CompanyPK, 'GTT')
),

JobConsolGTW
AS
(
	SELECT
		JK_PK, JK_UniqueConsignRef, JK_IsCFS, JK_SystemCreateUser, JK_SystemCreateTimeUtc, JK_SystemLastEditUser, JK_SystemLastEditTimeUtc
	FROM
		dbo.JobConsol
	WHERE
		JK_IsForwarding = 1 
		AND EXISTS (SELECT 1 FROM GttConsols WHERE GttConsol_PK = JK_PK)
		AND JK_MasterBillNum = @MasterBillNum 
),

LinesSum
AS
(
	SELECT
		JK_PK,
		{formattedParameters}
		Posted_LocalCostAmt = SUM(CASE 
				WHEN JR_OH_CostAccount IS NOT NULL AND JR_OH_CostAccount = @Creditor AND AL_LineType IS NOT NULL AND AL_LineType != @LineType 
					THEN (CASE WHEN JR_AC IN ({chargeCodePKs}) THEN JR_LocalCostAmt ELSE 0 END)
				ELSE 0
			END),
		CASE
			WHEN SUM(CASE WHEN AL_PK IS NOT NULL THEN 1 ELSE 0 END) > 0 THEN 4
			WHEN SUM(CASE WHEN JH_PK IS NOT NULL THEN 1 ELSE 0 END) > 0 THEN 3 
			ELSE 2 
		END AS IsGatewayBilling
	FROM
		JobConsolGTW
		LEFT JOIN dbo.JobHeader ON JH_ParentID = JK_PK AND JH_GC = @CompanyPK
		LEFT JOIN dbo.JobCharge ON JH_PK = JR_JH 
		LEFT JOIN dbo.AccTransactionLines ON JR_AL_APLine = AL_PK 
	GROUP BY JK_PK
),

IsGatewayConsol
AS
(
	SELECT 
		JK_PK,
		CASE
			WHEN SUM(CASE WHEN E6_PK IS NOT NULL AND JR_PK IS NOT NULL THEN 1 ELSE 0 END) > 0 AND SUM(CASE WHEN E6_PK IS NOT NULL AND JR_PK IS NULL THEN 1 ELSE 0 END) = 0 THEN 5 
			WHEN SUM(CASE WHEN E6_PK IS NOT NULL AND JR_PK IS NULL THEN 1 ELSE 0 END) > 0 AND SUM(CASE WHEN E6_PK IS NOT NULL AND JR_PK IS NOT NULL THEN 1 ELSE 0 END) = 0 THEN 1 
			ELSE 0
		END AS IsGatewayBilling
	FROM
		JobConsolGTW
		LEFT JOIN dbo.JobConsolCost ON E6_ParentID = JK_PK AND E6_GC = @CompanyPK
		LEFT JOIN dbo.JobCharge ON JR_E6_GatewaySellHeader = E6_PK
	GROUP BY JK_PK 
)

SELECT
	JobConsolGTW.JK_PK, JK_UniqueConsignRef, JK_IsCFS, JK_SystemCreateUser, JK_SystemCreateTimeUtc, JK_SystemLastEditUser, JK_SystemLastEditTimeUtc,
	{formattedParametersNames}
	Posted_LocalCostAmt,
	CASE
		WHEN IsGatewayConsol.IsGatewayBilling > 0 THEN IsGatewayConsol.IsGatewayBilling ELSE LinesSum.IsGatewayBilling
	END AS IsGatewayBilling
FROM
	JobConsolGTW
	LEFT JOIN LinesSum ON LinesSum.JK_PK = JobConsolGTW.JK_PK
	LEFT JOIN IsGatewayConsol ON IsGatewayConsol.JK_PK = JobConsolGTW.JK_PK

UNION ALL

SELECT
	JK_PK, JK_UniqueConsignRef, JK_IsCFS, JK_SystemCreateUser, JK_SystemCreateTimeUtc, JK_SystemLastEditUser, JK_SystemLastEditTimeUtc,
	{formattedParameters}
	Posted_LocalCostAmt = SUM(CASE 
			WHEN JR_OH_CostAccount IS NOT NULL AND JR_OH_CostAccount = @Creditor AND AL_LineType IS NOT NULL AND AL_LineType != @LineType 
				THEN (CASE WHEN JR_AC IN ({chargeCodePKs}) THEN JR_LocalCostAmt ELSE 0 END)
			ELSE 0
		END),
	0 AS IsGatewayBilling
FROM
	dbo.JobConsol 
	INNER JOIN dbo.JobConShipLink ON JK_PK = JN_JK  
	INNER JOIN dbo.JobHeader ON JH_ParentID = JN_JS
	LEFT JOIN dbo.JobCharge ON JH_PK = JR_JH 
	LEFT JOIN dbo.AccTransactionLines ON JR_AL_APLine = AL_PK
WHERE
	JH_GC = @CompanyPK
	AND JK_MasterBillNum = @MasterBillNum
GROUP BY JK_PK, JK_UniqueConsignRef, JK_IsCFS, JK_SystemCreateUser, JK_SystemCreateTimeUtc, JK_SystemLastEditUser, JK_SystemLastEditTimeUtc

ORDER BY JK_SystemCreateTimeUtc DESC, IsGatewayBilling DESC
");

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sqlText, parameters);

			return collection;
		}

		ZQuery GetUNLOCOQueryByUNLOCOCode(ZString iATACode)
		{
			ZQuery unlocoQuery = new ZQuery(RefUNLOCOSchema.RL_IsActive, true);
			unlocoQuery.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
			unlocoQuery.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.EndsWith, iATACode);
			return unlocoQuery;
		}

		string TrimIATACode(string iATACode)
		{
			return (string.IsNullOrWhiteSpace(iATACode)) ? string.Empty :
				Regex.Replace(iATACode, @"[\s\/-]+", string.Empty);
		}

		#endregion

		#region Critical Error Message

		public static string GetInvoiceNumberDuplicateInCollectionErrorMessage()
		{
			return Res.GetString("BAB14C2D-8066-45C4-AA00-C1136F9BC0D6",
				"This invoice number is already used on another invoice in this batch. Invoice numbers must be unique by creditor and currency.");
		}

		public static string GetInvoiceNumberDuplicateInDatabaseErrorMessage()
		{
			return Res.GetString("510622ed-5b45-40ad-a3f1-980ebda33754",
				"This transaction number already exists for this creditor.");
		}

		#endregion

		internal class SystemChargeDataToImport : IApportionmentChargeToImport
		{
			public SystemChargeDataToImport(ZGuid jr_JH, ZGuid jr_AC, ZGuid jr_GB, ZGuid jr_GE, ZString jr_RX_NKCostCurrency, ZDecimal jr_OSCostExRate, ZDecimal jr_OSCostAmt, ZGuid jr_OH_SellAccount, ZBool isApportioned)
			{
				JR_JH = jr_JH;
				JR_AC = jr_AC;
				JR_GB = jr_GB;
				JR_GE = jr_GE;
				JR_RX_NKCostCurrency = jr_RX_NKCostCurrency;
				JR_OSCostExRate = jr_OSCostExRate;
				JR_OSCostAmt = jr_OSCostAmt;
				JR_OH_SellAccount = jr_OH_SellAccount;
				IsApportioned = isApportioned;
			}

			public ZGuid JR_JH
			{
				get;
				private set;
			}

			public ZGuid JR_AC
			{
				get;
				private set;
			}

			public ZGuid JR_GB
			{
				get;
				private set;
			}

			public ZGuid JR_GE
			{
				get;
				private set;
			}

			public ZString JR_RX_NKCostCurrency
			{
				get;
				private set;
			}

			public ZDecimal JR_OSCostExRate
			{
				get;
				private set;
			}

			public ZDecimal JR_OSCostAmt
			{
				get;
				private set;
			}

			public ZGuid JR_OH_SellAccount
			{
				get;
				private set;
			}

			public ZBool JR_IsRevenuePosted
			{
				get { return false; }
			}

			public ZBool IsApportioned
			{
				get;
				private set;
			}
		}
	}
}

//CASSBillingLineTest class has been moved to Accounting.Business.Testing project.
