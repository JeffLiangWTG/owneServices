using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using DefaultOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Australian Commercial Invoice Line
	/// </summary>
	[UniversalCopyAddInfo(JobComInvoiceLineSchema.Constants.Prefix, AUAddInfo.Schema.Prefix)]
	public class JobComInvoiceLine : TypeSafeJobComInvoiceLine
		, IAddInfo
		, IAddInfoManager
		, IDutyData
		, ISecondCustomsQuantity
		, IUltimateDistributee
		, ITariffNumberProvider
		, IAQIS
		, ICusAddInfoTypeSupporter
		, ICusCodeDataTypeSupporter
		, Integration.Customs.AU.IJobComInvoiceLine
		, ICusStorageDocPivotParent
	{
		public new class LineComparer : IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				JobComInvoiceLine lineX = (JobComInvoiceLine)x;
				JobComInvoiceLine lineY = (JobComInvoiceLine)y;
				int result = 0;
				if (lineX == null && lineY != null)
				{
					result = -1;
				}
				else if (lineX != null && lineY == null)
				{
					result = 1;
				}
				else if (lineX == null && lineY == null)
				{
					result = 0;
				}
				else
				{
					if (lineX.InvoiceHeader != null && lineY.InvoiceHeader != null)
					{
						result = lineX.InvoiceHeader.JZ_InvoiceNumber.CompareTo(lineY.InvoiceHeader.JZ_InvoiceNumber);
					}
					if (result == 0)
					{
						result = lineX.JI_LineNo.CompareTo(lineY.JI_LineNo);
					}
					if (result == 0)
					{
						result = lineX.JI_LinePrefix.CompareTo(lineY.JI_LinePrefix);
					}
					if (result == 0)
					{
						result = lineX.PK.CompareTo(lineY.PK);
					}
				}
				return result;
			}

			#endregion
		}

		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Light Validation

		internal bool IsMarkAsNeedingValidationCalledForAddInfo
		{
			get { return isMarkAsNeedingValidationCalledForAddInfo; }
			set { isMarkAsNeedingValidationCalledForAddInfo = value; }
		}

		bool isMarkAsNeedingValidationCalledForAddInfo;

		protected override void MarkAsNeedingValidationCore()
		{
			if (!IsMarkAsNeedingValidationCalledForAddInfo && AddInfo.LightValidationIsValid)
			{
				AddInfo.MarkAsNeedingValidation();
			}
		}

		#endregion

		protected override bool GetJI_CustomsUnitQtyInfoReadOnly()
		{
			bool result = !AUCustomsDataRegistry.Instance.AllowOverrideOfCustomsUnitsOnDeclarations.Value;
			if (result && Declaration != null && Declaration.IsExport)
			{
				result = ExportTariff != null || JI_Tariff.IsEmpty;
			}
			return result;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobComInvoiceLineFetchStrategy(this);
		}

		protected override Type CusEntryLineType
		{
			get { return typeof(CusEntryLine); }
		}

		#region drawback members

		public ZDateTime DrawbackLineEffectiveDutyDate
		{
			get { return ZDateTime.Today; }
		}

		public ZString DrawbackAssesmentMethod
		{
			get { return AddInfo.ZA_DAM_Hidden; }
		}

		public ZString DrawbackLineAmberReasonCode
		{
			get { return AddInfo.ZA_DARC_Hidden; }
		}

		public override ZString DrawbackImportDeclarationNumber
		{
			get { return AddInfo.ZA_DDN_Hidden; }
		}

		public override ZInt DrawbackImportDeclarationLine
		{
			get { return AddInfo.ZA_DDL_Hidden; }
		}

		public ZString ExportDeclarationNumber
		{
			get { return AddInfo.ZA_EDN_Hidden; }
		}

		public ZDecimal DrawbackDutyRate
		{
			get { return AddInfo.ZA_DTR_Hidden; }
		}

		public ZDecimal DrawbackDutyAmount
		{
			get { return AddInfo.ZA_DDT_Hidden; }
		}

		protected ZDecimal DrawbackDutyAmountCalculated
		{
			get { return AddInfo.ZA_DDT_Hidden; }
			set
			{
				if (AddInfo.ZA_DDT_Hidden != value)
				{
					ZDecimal oldValue = AddInfo.ZA_DDT_Hidden;
					AddInfo.ZA_DDT_Hidden = value;
					DrawbackDutyAmountOverriden = oldValue;
				}
			}
		}

		public ZDecimal DrawbackDutyAmountOverriden
		{
			get { return drawbackDutyAmountOverriden; }
			set { drawbackDutyAmountOverriden = value; }
		}
		ZDecimal drawbackDutyAmountOverriden;

		public ZDecimal DrawbackCustomsValue
		{
			get { return AddInfo.ZA_DCV_Hidden; }
		}

		protected ZDecimal DrawbackCustomsValueCalculated
		{
			get { return AddInfo.ZA_DCV_Hidden; }
			set
			{
				if (AddInfo.ZA_DCV_Hidden != value)
				{
					ZDecimal oldValue = AddInfo.ZA_DCV_Hidden;
					AddInfo.ZA_DCV_Hidden = value;
					DrawbackCustomsValueOverriden = oldValue;
				}
			}
		}

		public ZDecimal DrawbackCustomsValueOverriden
		{
			get { return drawbackCustomsValueOverriden; }
			set { drawbackCustomsValueOverriden = value; }
		}
		ZDecimal drawbackCustomsValueOverriden;

		public new IDrawbackEntryLine DrawbackImportEntryLine => (IDrawbackEntryLine)base.DrawbackImportEntryLine;

		protected override IBaseDrawbackEntryLine DrawbackImportEntryLineCore
		{
			get
			{
				var candidateLines = FindByConsolidatedDeclarationAndLineNumbers(DrawbackImportDeclarationNumber, DrawbackImportDeclarationLine);
				return candidateLines.FirstOrDefault(c => c.ZA_AggregateEntryLineNumber == DrawbackImportDeclarationLine) ??
					   candidateLines.FirstOrDefault(c => c.CL_LineNumber == DrawbackImportDeclarationLine);
			}
		}

		public CusEntryLine[] FindByConsolidatedDeclarationAndLineNumbers(ZString importDeclarationNumber, ZInt importDeclarationLine)
		{
			if (importDeclarationLine > short.MaxValue)
			{
				return null;
			}

			var query = new ZQuery();
			query.AddToFilter(CusEntryLineSchema.CL_CustomsPostedStatus, SQLComparisonOperator.Equal, Customs.Business.EntryLineStatusList.Codes.Active);

			var lineNumQuery = new ZQuery();
			lineNumQuery.AddToFilter(CusEntryLineSchema.CL_LineNumber, SQLComparisonOperator.Equal, (short)importDeclarationLine);
			lineNumQuery.AddToFilter(JoinCondition.Or, CusEntryLineSchema.CL_AddInfo, SQLComparisonOperator.Contains, AUAddInfoSchema.Constants.ZA_AggregateEntryLineNumber_Hidden.Substring(3) + "=" + importDeclarationLine.ToString());
			query.AddToFilter(lineNumQuery);

			ZDBOnlyQuery entryLineQuery = new ZDBOnlyQuery(typeof(CusEntryLine));
			ZDBOnlySubQuery entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
			ZDBOnlySubQuery cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.Equal, importDeclarationNumber);
			cusEntryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			entryHeaderQuery.AddSubQuery(cusEntryNumQuery, JoinCondition.And);
			entryLineQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			query.AddToFilter(entryLineQuery);

			return Factory.Load<CusEntryLine>(query);
		}

		public void DefaultDrawbackDataOnChangeOfImportDeclarationNumber()
		{
			var drawbackImportEntryLine = DrawbackImportEntryLine;
			if (!IsCopying && drawbackImportEntryLine != null)
			{
				JI_Tariff = drawbackImportEntryLine.CL_AdValoremTariff;
				if (!IsDrawbackLineValueOverriden)
				{
					AddInfo.ZA_DTR_Hidden = drawbackImportEntryLine.CL_DutyPercent;
				}

				if (JI_Description.IsEmpty)
				{
					JI_Description = drawbackImportEntryLine.CL_Description;
				}

				if (JI_CustomsQuantity.IsEmpty)
				{
					if (drawbackImportEntryLine.CustomsUnitQty.IsEmpty)
					{
						JI_CustomsUnitQty = ZString.Empty;
						if (JI_InvoiceQuantity.IsEmpty)
						{
							JI_InvoiceUQ = drawbackImportEntryLine.InvoiceUQ;
							JI_InvoiceQuantity = DrawbackAmountCalculator.InvoiceQuantityFromEntryLineInvoiceQuantity(drawbackImportEntryLine, this);
						}
					}
					else
					{
						JI_CustomsUnitQty = drawbackImportEntryLine.CustomsUnitQty;
						if (!JI_InvoiceQuantity.IsEmpty && !JI_CustomsUnitQty.IsEmpty && UnitConverter.Convertible(JI_InvoiceUQ, drawbackImportEntryLine.InvoiceUQ))
						{
							JI_CustomsQuantity = DrawbackAmountCalculator.CustomsQuantityFromEntryLineCustomsQuantityProRataByInvoiceQuantity(drawbackImportEntryLine, this);
						}
						if (JI_CustomsQuantity.IsEmpty)
						{
							JI_CustomsQuantity = DrawbackAmountCalculator.CustomsQuantityFromEntryLineCustomsQuantity(drawbackImportEntryLine, this);
						}
					}
				}
				CalculateClaimAmount();
			}
		}

		public void CalculateClaimAmount()
		{
			averageCustomsValueCalculated = false;
			if (!IsDrawbackLineValueOverriden)
			{
				DrawbackAmountCalculator drawbackAmountCalculator = new DrawbackAmountCalculator(this);
				drawbackAmountCalculator.Calculate();
				DrawbackCustomsValueCalculated = drawbackAmountCalculator.CalculatedCustomsValue;
				DrawbackDutyAmountCalculated = drawbackAmountCalculator.CalculatedDutyAmount;
				AddInfo.ZA_DTR_Hidden = drawbackAmountCalculator.DutyRate;
				AddInfo.Validation.ValidateZA_DCV_Hidden();
				AddInfo.Validation.ValidateZA_DDT_Hidden();
			}
		}

		public void CalculateClaimAmountIfImputationMethod()
		{
			if (DrawbackAssesmentMethod == JobDeclaration.DrawbackAssessmentMethods.Imputation)
			{
				CalculateClaimAmount();
			}
		}

		public void ReCalculateClaimAmountIfRepresentativeShipment()
		{
			if (DrawbackAssesmentMethod == JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment && !IsDrawbackLineValueOverriden && DrawbackCusEntryLineCollection.Count == 0)
			{
				AddInfo.ZA_DDN_Hidden = "";
				RefreshDrawbackImportEntryLine();
				CalculateClaimAmount();
			}
		}

		[ChildEditable(true)]
		public DrawbackCusEntryLineCollection DrawbackCusEntryLineCollection
		{
			get
			{
				if (drawbackCusEntryLineCollection == null)
				{
					LoadStmNoteFetchHintIfNeeded();
					drawbackCusEntryLineCollection = DrawbackCusEntryLineCollection.CreateDrawbackCusEntryLineCollection(this, Factory);
					RegisterEditableChildObject(drawbackCusEntryLineCollection);
				}
				return drawbackCusEntryLineCollection;
			}
		}
		DrawbackCusEntryLineCollection drawbackCusEntryLineCollection;

		void LoadStmNoteFetchHintIfNeeded()
		{
			var invoice = InvoiceHeader;
			if (invoice != null)
			{
				var declaration = invoice.JobDeclaration;
				if (declaration != null && declaration.IsPersistent)
				{
					declaration.InvoiceLines.LoadStmNoteFetchHintIfNeeded();
				}
				else
				{
					invoice.JobComInvoiceLines.LoadStmNoteFetchHintIfNeeded();
				}
			}
		}

		public
#if DEBUG
 virtual
#endif
 ZString DrawbackCalculationMethodComment
		{
			get
			{
				if (DrawbackAssesmentMethod == JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment && DrawbackCusEntryLineCollection.Count > 0)
				{
					return "Method B Average Calculation used on this line.";
				}
				else
				{
					return "";
				}
			}
		}
		public ZPropertyInfo DrawbackCalculationMethodCommentInfo
		{
			get { return GetZPropertyInfo(nameof(DrawbackCalculationMethodComment)); }
		}

		public
#if DEBUG
 virtual
#endif
 ZBool IsDrawbackClaimAmountUnusual
		{
			get
			{
				ZBool result = false;
				if (AverageCustomsValue > 0)
				{
					if (Math.Abs((AverageCustomsValue - PerUnitCustomsValue) * 100m / AverageCustomsValue) >= CustomsDataRegistry.Instance.VariancePercentageUsedForExceptionReportOfUnusualUnitValue.Value)
					{
						result = true;
					}
				}
				return result;
			}
		}

		ZDecimal PerUnitCustomsValue
		{
			get { return JI_CustomsQuantity <= 0.0m ? 0.0m : decimal.Round(DrawbackCustomsValue / JI_CustomsQuantity, 3); }
		}

		public
#if DEBUG
 virtual
#endif
 ZDecimal AverageCustomsValue
		{
			get
			{
				if (!averageCustomsValueCalculated && Part != null)
				{
					averageCustomsValueCalculated = true;
					averageCustomsValue = 0;
					ZDecimal totalCustomsQuantity = 0;

					BaseJobComInvoiceHeader earliestInvoice = Declaration.Invoices.EarliestInvoice;
					ZDateTime startDate = earliestInvoice != null && earliestInvoice.JZ_InvoiceDate.IsValid ? earliestInvoice.JZ_InvoiceDate : ZDateTime.Now;
					int monthsAgo = CustomsDataRegistry.Instance.HistoryWindowForExceptionReportOfUnusualUnitValue.Value;
					startDate = startDate.AddMonths(monthsAgo > 0 ? -monthsAgo : -18);
					BaseJobComInvoiceHeader latestInvoice = Declaration.Invoices.LatestInvoice;
					ZDateTime endDate = latestInvoice != null && latestInvoice.JZ_InvoiceDate.IsValid ? latestInvoice.JZ_InvoiceDate : ZDateTime.Now;
					endDate = endDate.AddDays(-1);

					ZQuery entryLinesForPartFiler = new ZQuery();
					ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(CusEntryLine));
					ZDBOnlySubQuery invoiceLineQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_CL);
					invoiceLineQuery.AddToFilter(JobComInvoiceLineSchema.JI_OP, Part.PK);
					dBOnlyQuery.AddSubQuery(invoiceLineQuery, JoinCondition.And);
					if (Declaration.Importer != null)
					{
						ZDBOnlySubQuery entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryLineSchema.CL_CH);
						ZDBOnlySubQuery jobDeclarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
						jobDeclarationQuery.AddToFilter(JobDeclarationSchema.JE_OH_Importer, Declaration.Importer.PK);
						entryHeaderQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.And);
						dBOnlyQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);
					}
					entryLinesForPartFiler.AddToFilter(dBOnlyQuery);
					foreach (CusEntryLine entryLine in Factory.Load<CusEntryLine>(entryLinesForPartFiler))
					{
						CusEntryHeader entryHeader = entryLine.Header;
						if (entryHeader != null)
						{
							ZDateTime declarationDate = entryHeader.DeclarationDate;
							if (declarationDate.IsValid && declarationDate >= startDate && declarationDate <= endDate)
							{
								var drawbackAmounts = DrawbackAmountCalculator.DrawbackAmountsForLinePart(entryLine, this);
								averageCustomsValue += drawbackAmounts.CustomsValue;
								totalCustomsQuantity += drawbackAmounts.Quantity;
							}
						}
					}
					if (totalCustomsQuantity > 0)
					{
						averageCustomsValue = decimal.Round(averageCustomsValue / totalCustomsQuantity, 3);
					}
					else
					{
						averageCustomsValue = 0;
					}
				}
				return averageCustomsValue;
			}
		}
		bool averageCustomsValueCalculated;
		ZDecimal averageCustomsValue;

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_DOV_Hidden)]
		public ZBool IsDrawbackLineValueOverriden
		{
			get { return (AddInfo.ZA_DOV_Hidden == "Y"); }
			set
			{
				if (IsDrawbackLineValueOverriden ^ value)
				{
					AddInfo.ZA_DOV_Hidden = value ? "Y" : "";
					if (!IsDrawbackLineValueOverriden)
					{
						CalculateClaimAmount();
					}
				}
			}
		}

		public ZPropertyInfo IsDrawbackLineValueOverridenInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsDrawbackLineValueOverriden), x => AddInfo.ZA_DOV_HiddenInfo); }
		}

		#endregion

		#region IBOMExpander Members

		public override ZBool IsBOMLineExpanded
		{
			get { return AddInfo.ZA_BOMLineExpanded_Hidden; }
			set
			{
				AddInfo.ZA_BOMLineExpanded_Hidden = value;
				CalculateClaimAmount();
			}
		}

		public override ZGuid BOMParentLinePK
		{
			get { return AddInfo.ZA_BOMParentLine_Hidden; }
			set { AddInfo.ZA_BOMParentLine_Hidden = value; }
		}

		#endregion

		#region DefaultOriginAndStateFromExporter

		void DefaultOriginAndStateFromExporter()
		{
			if (!IsCopying && Declaration != null && !Declaration.IsImportingData && Declaration.IsExport && Declaration.Supplier != null)
			{
				JI_CountryOfOrigin = Declaration.Supplier.CountryCode;

				if (IsAUOrigin)
				{
					JI_AUState = Declaration.Supplier.MainAddress.OA_State.SubstringSafe(0, 5);
				}
			}
		}

		#endregion

		#region JI_JZ

		public void MarkAllChargesAsNeedingValidation()
		{
			Charges.MarkAsNeedingValidation();
			ApportionedCharges.MarkAsNeedingValidation();
		}

		public void MarkAllQuarantineAsNeedingValidation()
		{
			if (!IsCopying && IsQuarantine)
			{
				QuarantineExDocLine.Processes.MarkAsNeedingValidation();
				QuarantineExDocLine.MarkAsNeedingValidation();
			}
		}

		public override ZGuid JI_JZ
		{
			get { return base.JI_JZ; }
			set
			{
				bool hasChanged = base.JI_JZ != value;
				if (hasChanged)
				{
					base.JI_JZ = value;

					if (InvoiceHeader != null)
					{
						InvoiceHeader.MarkAsNeedingValidation();
					}

					MarkAllChargesAsNeedingValidation();
					MarkAllQuarantineAsNeedingValidation();
				}

				if (value.IsValid)
				{
					DefaultOriginAndStateFromExporter();
					if (!IsCopying)
					{
						if (hasChanged && Declaration != null && Declaration.IsDrawback)
						{
							UpdateLineDrawbackDefaultValues();
						}
					}
				}
			}
		}

		#endregion

		#region Constants

		public new class Schema : BaseJobComInvoiceLine.Schema
		{
			public const string AddInfo = "AddInfo";
			public const string JI_AUState = "JI_AUState";
			public const string JI_TempImportNum = "JI_TempImportNum";
			public const string JI_TempImportDate = "JI_TempImportDate";
			public const string JI_NoPermitRequired = "JI_NoPermitRequired";
			public const string JI_RelatedExportPermitNumber = "JI_RelatedExportPermitNumber";
			public const string JI_RelatedExportPermitAuthority = "JI_RelatedExportPermitAuthority";
			public const string JI_RelatedExportPermitDate = "JI_RelatedExportPermitDate";
			public const string JI_GoodsOriginCode = "JI_GoodsOriginCode";
			public const string JI_Drawback = "JI_Drawback";
			public const string JI_Texco = "JI_Texco";
			public const string JI_MotorVehiclePlan = "JI_MotorVehiclePlan";
			public const string JI_CustomsUnitQtyForGUI = "JI_CustomsUnitQtyForGUI";
			public const string InstrumentType = "InstrumentType";
			public const string InstrumentCode = "InstrumentCode";
			public const string JI_WarehouseUnitValue = "JI_WarehouseUnitValue";
			public const string JI_PriceAdjustment = "JI_PriceAdjustment";
			public const string JI_LinePrefix = "JI_LinePrefix";
			public const string JI_IsPackToBondForLine = "JI_IsPackToBondForLine";
			public const string JI_RX_LocalCurr = "JI_RX_LocalCurr";
			public const string CurrencyList = "CurrencyList";
			public const string JI_ParentLineCode = "JI_ParentLineCode";

			public const string JI_PRF = "JI_PRF";

			public const string JI_Calc_InterimAntiDumpingDuty = "JI_Calc_InterimAntiDumpingDuty";
			public const string JI_Calc_InterimCountervailingDuty = "JI_Calc_InterimCountervailingDuty";
			public const string JI_Calc_WETAmount = "JI_Calc_WETAmount";
			public const string JI_Calc_LCTAmount = "JI_Calc_LCTAmount";
			public const string JI_Calc_DutyPercent = "JI_Calc_DutyPercent";
			public const string JI_Calc_FlatDutyPortion = "JI_Calc_FlatDutyPortion";
			public const string JI_Calc_AllOtherDuties = "JI_Calc_AllOtherDuties";
		}

		public const string ForeignCountryCode = "YY-FO";

		public static class LinePrefixString
		{
			public const string Parent = "P";
			public const string Trailer = "T";
			public const string Normal = " ";
		}

		public static class CANConstants
		{
			public const string NoPermitRequired = "NO PERMIT";
		}

		#endregion

		#region AQIS Collections

		[ChildEditable(false)]
		public AQISPackageCollection AQISPackages
		{
			get
			{
				if (fAQISPackages == null)
				{
					fAQISPackages = new AQISPackageCollection(Factory, AddInfo);
					fAQISPackages.SplitAndAddAQISElements(AddInfo.ZA_AQISPackageType_Hidden);
					RegisterEditableChildObject(fAQISPackages);
				}

				return fAQISPackages;
			}
		}
		AQISPackageCollection fAQISPackages;

		[ChildEditable(false)]
		public AQISDocumentCollection AQISDocuments
		{
			get
			{
				if (fAQISDocuments == null)
				{
					fAQISDocuments = new AQISDocumentCollection(Factory, AddInfo);
					fAQISDocuments.SplitAndAddAQISElements(AddInfo.ZA_AQISDocuments_Hidden);
					RegisterEditableChildObject(fAQISDocuments);
				}

				return fAQISDocuments;
			}
		}
		AQISDocumentCollection fAQISDocuments;
		public AQISDocumentCollection AggreatedAQISDocuments
		{
			get
			{
				AQISDocumentCollection result = null;

				if (AQISDocuments.Count > 0)
				{
					result = AQISDocuments;
				}
				else if (InvoiceHeader != null && InvoiceHeader.AQISDocuments.Count > 0)
				{
					result = InvoiceHeader.AQISDocuments;
				}
				else if (Declaration != null)
				{
					result = Declaration.AQISDocuments;
				}

				return result;
			}
		}

		[ChildEditable(false)]
		public AQISPremisesIdAndProcessingTypeCollection AQISPremisesIdAndProcessingTypes
		{
			get
			{
				if (fAQISPremisesIdAndProcessingTypes == null)
				{
					fAQISPremisesIdAndProcessingTypes = new AQISPremisesIdAndProcessingTypeCollection(Factory, AddInfo);
					fAQISPremisesIdAndProcessingTypes.SplitAndAddAQISElements(AddInfo.ZA_AQISPremIdProcessType_Hidden);
					RegisterEditableChildObject(fAQISPremisesIdAndProcessingTypes);
				}

				return fAQISPremisesIdAndProcessingTypes;
			}
		}
		AQISPremisesIdAndProcessingTypeCollection fAQISPremisesIdAndProcessingTypes;
		public AQISPremisesIdAndProcessingTypeCollection AggreatedAQISPremisesIdAndProcessingTypes
		{
			get
			{
				AQISPremisesIdAndProcessingTypeCollection result = null;

				if (AQISPremisesIdAndProcessingTypes.Count > 0)
				{
					result = AQISPremisesIdAndProcessingTypes;
				}
				else if (InvoiceHeader != null && InvoiceHeader.AQISPremisesIdAndProcessingTypes.Count > 0)
				{
					result = InvoiceHeader.AQISPremisesIdAndProcessingTypes;
				}
				else if (Declaration != null)
				{
					result = Declaration.AQISPremisesIdAndProcessingTypes;
				}

				return result;
			}
		}

		[ChildEditable(true)]
		public AQISCommodityCodeCollection AQISCommodityCodes
		{
			get
			{
				if (fAQISCommodityCodes == null)
				{
					fAQISCommodityCodes = new AQISCommodityCodeCollection(Factory);
					RegisterEditableChildObject(fAQISCommodityCodes);
				}

				return fAQISCommodityCodes;
			}
		}
		AQISCommodityCodeCollection fAQISCommodityCodes;
		public AQISCommodityCodeCollection AggreatedAQISCommodityCodes
		{
			get
			{
				AQISCommodityCodeCollection result = null;

				if (AQISCommodityCodes.Count > 0)
				{
					result = AQISCommodityCodes;
				}
				else if (InvoiceHeader != null && InvoiceHeader.AQISCommodityCodes.Count > 0)
				{
					result = InvoiceHeader.AQISCommodityCodes;
				}
				else if (Declaration != null)
				{
					result = Declaration.AQISCommodityCodes;
				}

				return result;
			}
		}

		[ChildEditable(true)]
		public AQISEntityIdCollection AQISEntityIds
		{
			get
			{
				if (fAQISEntityIds == null)
				{
					fAQISEntityIds = new AQISEntityIdCollection(Factory);
					RegisterEditableChildObject(fAQISEntityIds);
				}

				return fAQISEntityIds;
			}
		}
		AQISEntityIdCollection fAQISEntityIds;
		public AQISEntityIdCollection AggreatedAQISEntityIds
		{
			get
			{
				AQISEntityIdCollection result = null;

				if (AQISEntityIds.Count > 0)
				{
					result = AQISEntityIds;
				}
				else if (InvoiceHeader != null && InvoiceHeader.AQISEntityIds.Count > 0)
				{
					result = InvoiceHeader.AQISEntityIds;
				}
				else if (Declaration != null)
				{
					result = Declaration.AQISEntityIds;
				}

				return result;
			}
		}

		[ChildEditable(true)]
		public AQISPermitIdCollection AQISPermitIds
		{
			get
			{
				if (fAQISPermitIds == null)
				{
					fAQISPermitIds = new AQISPermitIdCollection(Factory);
					RegisterEditableChildObject(fAQISPermitIds);
				}

				return fAQISPermitIds;
			}
		}
		AQISPermitIdCollection fAQISPermitIds;
		public AQISPermitIdCollection AggreatedAQISPermitIds
		{
			get
			{
				AQISPermitIdCollection result = null;

				if (AQISPermitIds.Count > 0)
				{
					result = AQISPermitIds;
				}
				else if (InvoiceHeader != null && InvoiceHeader.AQISPermitIds.Count > 0)
				{
					result = InvoiceHeader.AQISPermitIds;
				}
				else if (Declaration != null)
				{
					result = Declaration.AQISPermitIds;
				}

				return result;
			}
		}

		[ChildEditable(true)]
		public AQISProducerCodeCollection AQISProducerCodes
		{
			get
			{
				if (fAQISProducerCodes == null)
				{
					fAQISProducerCodes = new AQISProducerCodeCollection(Factory);
					RegisterEditableChildObject(fAQISProducerCodes);
				}

				return fAQISProducerCodes;
			}
		}
		AQISProducerCodeCollection fAQISProducerCodes;
		public AQISProducerCodeCollection AggreatedAQISProducerCodes
		{
			get
			{
				AQISProducerCodeCollection result = null;

				if (AQISProducerCodes.Count > 0)
				{
					result = AQISProducerCodes;
				}
				else if (InvoiceHeader != null && InvoiceHeader.AQISProducerCodes.Count > 0)
				{
					result = InvoiceHeader.AQISProducerCodes;
				}
				else if (Declaration != null)
				{
					result = Declaration.AQISProducerCodes;
				}

				return result;
			}
		}

		#region RFPNumbers

		[ChildEditable(false)]
		public RFPNumberCollection RFPNumbers
		{
			get
			{
				if (fRFPNumbers == null)
				{
					fRFPNumbers = new RFPNumberCollection(this);
					fRFPNumbers.Load();
					fRFPNumbers.CountChanged += RFPNumbers_CountChanged;
					RegisterEditableChildObject(fRFPNumbers);
				}

				return fRFPNumbers;
			}
		}

		void RFPNumbers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (IsQuarantine && fRFPNumbers.Count < 2)
			{
				((QuarantineJobComInvoiceLineValidation)OldValidation).ValidateRFPNumbers();
			}
		}

		RFPNumberCollection fRFPNumbers;

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			AQISPackages.ReBuildAndSaveAQISElements();
			AddInfo.SetAQISFieldsForSave();
			if (Declaration != null && Declaration.IsDrawback && drawbackCusEntryLineCollection != null)
			{
				DrawbackCusEntryLineCollection.SaveToNote();
			}
		}

		#endregion

		#region Related Business Objects

		public ExportClassificationCollection ExportClassificationList
		{
			get { return new ExportClassificationCollection(Factory); }
		}

		public ImportClassificationCollection ImportClassificationList
		{
			get { return new ImportClassificationCollection(Factory); }
		}

		[ChildEditable(true)]
		public new JobComInvApportionedChargeCollection<InvoiceLineApportionedCharge> ApportionedCharges => (JobComInvApportionedChargeCollection<InvoiceLineApportionedCharge>)base.ApportionedCharges;

		protected override IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection()
		{
			return new JobComInvApportionedChargeCollection<InvoiceLineApportionedCharge>(this);
		}

		public ClassificationPeriodSnapshotWrapper StatClassificationWrapper => ClassificationPeriodSnapshotWrapper.Load(Factory, TariffNumber, StatCode, EffectiveDutyDate);

		#endregion

		#region Related Orders Business Objects

		public Order Order
		{
			get
			{
				var filter = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, JI_OrderNumber);
				filter.AddToFilter(JobOrderHeaderSchema.JD_OrderNumberSplit, new ZByte(0));
				return Factory.LoadTop1<Order>(filter);
			}
		}

		#endregion

		#region New Methods and Properties

		public bool IsQuarantine => Declaration?.IsQuarantine ?? InvoiceHeader?.IsQuarantine ?? false;

		public bool IsNEXDOCSActive => InvoiceHeader?.IsNEXDOCSActive ?? false;

		public bool IsAUOrigin => JI_CountryOfOrigin == Enterprise.Core.Constants.CountryCodes.Australia;

		public string DutyControlCaptionPrefix => IsDutyAndTaxEstimatedForWH ? "Est " : "";

		public string GSTControlCaptionPrefix => IsDutyAndTaxEstimatedForWH ? "Est " : (JI_Calc_GSTVATDeferred.IsEmpty ? "" : "Def ");

		#endregion

		#region Overrides

		public override bool IsDutyAndTaxEstimatedForWH
		{
			get { return Declaration.EstimateDutyAndTaxOnWHEntries && JI_IsPackToBondForLine; }
		}

		public override ZDecimal JI_Calc_GSTVATAmountIncludingWHEstimate
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.GSTVATAmountIncludingWHEstimate + CusEntryLine.GSTVATDeferred).Amount; }
		}

		public override ZDecimal JI_Calc_DutyAmountIncludingWHEstimate
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.DutyAmountIncludingWHEstimate).Amount; }
		}

		protected override bool UseBondedWarehouseAutomationCore
		{
			get { return AddInfo.UseBondedWarehouseAutomation; }
			set { AddInfo.UseBondedWarehouseAutomation = value; }
		}

		protected override bool IsGoingIntoBondedWarehouseCore
		{
			get { return JI_IsPackToBondForLine; }
		}

		public override ZGuid JI_BondedWarehouseLineKey
		{
			get { return AddInfo.ZA_BondedWarehouseLineKey_Hidden; }
			set { AddInfo.ZA_BondedWarehouseLineKey_Hidden = value; }
		}

		protected override Customs.Business.BondedWarehouseTransactionLine GetNewBondedWarehouseTransactionLine()
		{
			return new BondedWarehouseTransactionLine(this);
		}

		protected override bool IsBondedWarehousingDisabledCore
		{
			get { return Declaration?.IsBondedWarehousingDisabled ?? false; }
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			AddInfo.OnSaved(saveSucceeded);
		}

		[BusinessObjectTestExclude]
		public override ZString JI_AddInfo
		{
			get { return base.JI_AddInfo; }
			set
			{
				value = value.Trim(AUAddInfo.SeperationCharacter);
#if DEBUG  // The form basher returns add infos that are slightly too long.  This should never happen in release mode.  If it does it should send us the Developer Error
				if (value.Length > JI_AddInfoInfo.MaxLength)
				{
					value = value.Substring(0, JI_AddInfoInfo.MaxLength);
				}
#endif
				if (base.JI_AddInfo != value)
				{
					base.JI_AddInfo = value;
					using (AddInfo.GetValidationSuspender())
					{
						AddInfo.LoadPropertiesFromString(value);
					}

					if (!InvoiceHeader?.IsResetLineValuesFromAddInfoSuspend ?? true)
					{
						// SetJI_CountryOfOrigin must be called after the AddInfoObject is updated
						SetJI_CountryOfOrigin();
						SetJI_ConcessionOrder();
					}
				}
			}
		}

		public override ZString JI_CountryOfOrigin
		{
			get { return base.JI_CountryOfOrigin; }
			set
			{
				base.JI_CountryOfOrigin = value;

				if (!IsCopying)
				{
					SetAddInfoORG();

					if (!IsAUOrigin)
					{
						AddInfo.ZA_AUState_Hidden = "";
					}
				}
			}
		}

		public override ZDecimal JI_LinePrice
		{
			get { return base.JI_LinePrice; }
			set
			{
				ZDecimal newValue = ZArchitecture.Core.Utilities.Round(value, 2);
				var oldValue = JI_LinePrice;
				if (oldValue != newValue)
				{
					base.JI_LinePrice = newValue;
					if (!IsCopying)
					{
						var declaration = Declaration;
						if (declaration != null)
						{
							if (declaration.IsDrawback)
							{
								CalculateClaimAmountIfImputationMethod();
							}
						}
					}
				}
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (Declaration != null && Declaration.IsImport)
			{
				AddInfo.ZA_MotorVehiclePlan_Hidden = ZString.Empty;
				AddInfo.ZA_Texco_Hidden = ZString.Empty;
				AddInfo.ZA_Drawback_Hidden = ZString.Empty;
			}
		}

		//ToDo : ValidateJI_CustomsQuantity should be implemented in Base
		public override ZString JI_PartNo
		{
			get { return base.JI_PartNo; }
			set
			{
				bool hasChanged = base.JI_PartNo != value;
				base.JI_PartNo = value;
				if (!IsCopying)
				{
					OldValidation.ValidateJI_PartNo();
				}
				UpdateDetailsOnPartChange();
				if (!IsCopying && hasChanged && Declaration != null && Declaration.IsDrawback)
				{
					CalculateClaimAmount();
				}
			}
		}

		public override void UpdateDetailsFromPivotOnPartChangeCore()
		{
			if (!IsCopying && !IsDeleted)
			{
				base.UpdateDetailsFromPivotOnPartChangeCore();
				DefaultAddInfoFromPart();
				OldValidation.ValidateJI_CustomsQuantity();
			}
		}

		protected override void ASNRefereshDataCountrySpecific(IEnumerable<ZString> refreshOptions, BaseCusClassPartPivot pivot)
		{
			base.ASNRefereshDataCountrySpecific(refreshOptions, pivot);
			var auPivot = (CusClassPartPivot)pivot;
			if (refreshOptions.Contains(DefaultOptions.Codes.CountryOfOrigin))
			{
				JI_CountryOfOrigin = auPivot.AddInfo.ZA_ORG.Left(JI_CountryOfOriginInfo.MaxLength);
			}
			if (refreshOptions.Contains(DefaultOptions.Codes.Preference))
			{
				AddInfo.ZA_PST = auPivot.AddInfo.ZA_PST;
			}
		}

		public override ZGuid JI_CC
		{
			get { return base.JI_CC; }
			set
			{
				bool hasChanges = base.JI_CC != value;

				if (hasChanges)
				{
					if (!IsCopying && !value.IsEmpty && !IsValidationSuspended)
					{
						RemoveClassificationInfoFromLine();
					}
					base.JI_CC = value;

					if (!IsCopying && Classification != null)
					{
						AddInfo.LoadPropertiesFromString(Classification.AddInfo.ToString(), false);
					}
				}
			}
		}

		void RemoveClassificationInfoFromLine()
		{
			ZString zA_ORG = AddInfo.ZA_ORG;
			ZString zA_PRF = AddInfo.ZA_PRF;
			AddInfo.AddInfoLine = ZString.Empty;
			AddInfo.ZA_ORG = zA_ORG;
			AddInfo.ZA_PRF = zA_PRF;
			AddInfo.ZA_TreatmentCode_Hidden = ZString.Empty;
			AddInfo.ZA_InstrumentCode_Hidden = ZString.Empty;
			AddInfo.ZA_InstrumentType_Hidden = ZString.Empty;
		}

		protected override TariffFormatter TariffFormatter
		{
			get
			{
				if (IsExport)
				{
					return new AUExportTariffUniversalFormatter();
				}
				else
				{
					return new AUImportTariffUniversalFormatter();
				}
			}
		}

		public override void UpdateProductDetailsOnSupplierBuyerChange()
		{
			base.UpdateProductDetailsOnSupplierBuyerChange();
			ForceUQ_ListToReCalculate();
		}

		public override ZDecimal JI_CustomsValue
		{
			get
			{
				ZDecimal result = JI_Calc_FOB_InLocalCurrency;
				if (JI_PriceAdjustment.IsValid)
				{
					result += CurrencyConverter.ConvertExact(JI_PriceAdjustment, JobDeclaration.GetLocalCurrency()).Amount;
				}
				return result;
			}
		}

		protected override ZString ClassificationDetailsForGenericWrapperCore
		{
			get
			{
				var result = base.ClassificationDetailsForGenericWrapperCore;

				if (Declaration != null && Declaration.IsImport)
				{
					if (!TreatmentCode.IsEmpty)
					{
						result += " / " + TreatmentCode;
					}

					var concessionInfo = GetConcessionInfo(InstrumentType, InstrumentCode);
					if (!concessionInfo.IsEmpty)
					{
						result += " / " + concessionInfo;
					}

					concessionInfo = GetConcessionInfo(AddInfo.TCI_InstrumentType, AddInfo.TCI_InstrumentNo);
					if (!concessionInfo.IsEmpty)
					{
						result += " / " + concessionInfo;
					}
				}

				return result;
			}
		}

		ZString GetConcessionInfo(ZString concessionType, ZString concessionNumber)
		{
			var result = concessionType;

			if (!concessionNumber.IsEmpty)
			{
				result += " " + concessionNumber;
			}

			return result;
		}

		#endregion

		#region OldValidation

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			return OldValidation;
		}

		public JobComInvoiceLineValidation OldValidation
		{
			get
			{
				if (!IsCopying)
				{
					JobDeclaration cachedDeclaration = Declaration;
					if (cachedDeclaration != null)
					{
						if (cachedDeclaration.IsExport)
						{
							if (cachedDeclaration.IsQuarantine)
							{
								return new QuarantineJobComInvoiceLineValidation(this);
							}
							else
							{
								return new ExportJobComInvoiceLineValidation(this);
							}
						}
						else if (cachedDeclaration.IsImportCMR)
						{
							if (cachedDeclaration.IsSAC)
							{
								if (cachedDeclaration.IsSACWithLines)
								{
									return new SACJobComInvoiceLineValidation(this);
								}
								else
								{
									return new SACWithoutLinesJobComInvoiceLineValidation(this);
								}
							}
							else
							{
								return new IMDJobComInvoiceLineValidation(this);
							}
						}
						else if (cachedDeclaration.IsImport)
						{
							return new EDIFICEJobComInvoiceLineValidation(this);
						}
						else if (cachedDeclaration.IsDrawback)
						{
							return new DrawbackJobComInvoiceLineValidation(this);
						}
					}
				}
				return new JobComInvoiceLineValidation(this);
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			JobComInvoiceLineValidation oldValidation = this.OldValidation;

			base.RunPreSaveValidationCore();
			oldValidation.ValidateJI_Drawback();
			oldValidation.ValidateJI_MotorVehiclePlan();
			oldValidation.ValidateJI_Texco();
			oldValidation.ValidateInstrumentType();
			oldValidation.ValidateInstrumentCode();
			oldValidation.ValidateJI_Calc_Invoice();
			oldValidation.ValidateJI_LinePrefix();
		}

		#endregion

		#region List Properties

		#region CodeFindBox Lists

		protected override BaseCustomsQuantityConverter GetCustomsQuantityConverter()
		{
			return new CustomsQuantityConverter(this, (ZPropertyInfoDecimal)JI_CustomsQuantityInfo, (ZPropertyInfoString)JI_CustomsUnitQtyInfo);
		}

		protected override BaseCustomsQuantityConverter GetCustomsQuantity2Converter()
		{
			return new CustomsQuantityConverter(this, SecondCustomsQtyInfo, SecondCustomsUQInfo);
		}

		protected CodeDescriptionPairList fJI_UQ_List;
		public CodeDescriptionPairList JI_UQ_List
		{
			get
			{
				if (fJI_UQ_List == null)
				{
					fJI_UQ_List = new CodeDescriptionPairList();
					fJI_UQ_List.AddRange(new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits());
					fJI_UQ_List.AddRange(UnitConverter.ConvertibleUQs);
				}
				return fJI_UQ_List;
			}
		}

		public CodeDescriptionPairList JI_ZA_WRU_List
		{
			get { return UnitConverter.ConvertibleUQs; }
		}

		/// <summary>
		/// Gets called when supplier changes and thus, need to update the UQ list
		/// </summary>
		public void ForceUQ_ListToReCalculate()
		{
			fJI_UQ_List = null;
		}

		#endregion

		#region Code Description List

		public CodeDescriptionPairList JI_WeightUQ_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList JI_AUStatesList
		{
			get
			{
				CodeDescriptionPairList list;
				if (Declaration != null && Declaration.JE_MessageType == JobMessageTypeList.Codes.Quarantine)
				{
					list = Factory.GetCachedValue<EXDOCErrata32List35AustralianStates>();
				}
				else
				{
					list = AddInfo.Lookups.ZA_AUStatesList;
				}
				return list;
			}
		}

		public CodeDescriptionPairList InstrumentTypeList
		{
			get { return OldValidation.InstrumentTypeList; }
		}

		public CodeDescriptionPairList InstrumentCodeList
		{
			get { return OldValidation.InstrumentCodeList; }
		}

		public CodeDescriptionPairList JI_LinePrefix_List
		{
			get { return OldValidation.LinePrefix_List; }
		}

		public CodeDescriptionPairList JI_CustomsUQList
		{
			get { return OldValidation.CustomsUQList; }
		}

		#endregion

		public RefCountryCollection CountryOfOriginList
		{
			get { return OldValidation.CountryOfOriginList; }
		}

		public RefCurrencyCollection CurrencyList
		{
			get { return InvoiceHeader != null ? InvoiceHeader.Lookups.CurrencyList : new RefCurrencyCollection(Factory); }
		}

		public CodeDescriptionPairList LineValuationBasisList
		{
			get { return OldValidation.LineValuationBasisList; }
		}

		#endregion

		#region NonPersistentContainers

		protected override CusContainersInvoiceLinesCollection GetNewContainersPivotCore()
		{
			return new CusContainersInvoiceLinesCollection<CusContainerInvoiceLinePivot, JobComInvoiceLine, CusContainer>(this);
		}

		public void CreateContainerFromAddInfo()
		{
			var containerNumber = AddInfo.ZA_AQISTempContainerNumber_Hidden;
			if (!containerNumber.IsEmpty)
			{
				var declaration = Declaration;
				if (declaration != null)
				{
					var isPersistent = declaration.IsPersistent;

					var container = declaration.AUCusContainers.Find(containerNumber);
					if (container == null)
					{
						container = declaration.AUCusContainers.AddNew();
						container.CO_ContainerNumber = containerNumber;

						var containerSeal = AddInfo.ZA_AQISTempContainerSeal_Hidden;
						if (!containerSeal.IsEmpty)
						{
							container.SealNumberForBinding = containerSeal;
						}
					}

					var pivot = ContainersPivot.Find(p => p.C2_CO == container.PK).FirstOrDefault();
					if (pivot == null)
					{
						using (isPersistent ? null : ContainersPivot.SuspendSettingHasChanges())
						{
							pivot = ContainersPivot.AddNew();

							using (isPersistent ? null : pivot.SuspendSettingHasChanges())
							{
								pivot.C2_CO = container.PK;
							}
						}
					}

					if (isPersistent)
					{
						AddInfo.ZA_AQISTempContainerNumber_Hidden = ZString.Empty;
						AddInfo.ZA_AQISTempContainerSeal_Hidden = ZString.Empty;
					}
				}
			}
		}

		#endregion

		#region New Properties

		public
#if DEBUG
 virtual
#endif
 ZBool DoesTariffRateOrTreatmentCodeDeemGSTExemption => Factory.GetValue(ref doesTariffRateOrTreatmentCodeDeemGSTExemptionCached, GetDoesTariffRateOrTreatmentCodeDeemGSTExemption);

		CachedProperty<ZBool> doesTariffRateOrTreatmentCodeDeemGSTExemptionCached;

		ZBool GetDoesTariffRateOrTreatmentCodeDeemGSTExemption()
		{
			bool result = false;
			if (InvoiceHeader != null)
			{
				ZString tariffRateNumber = AddInfo.ZA_RNO == "" ? new ZString("001") : AddInfo.ZA_RNO;
				ZString prefScheme = IsGeneralRate ? new ZString(AUAddInfo.GeneralPreferenceRate) : AggregatedZA_PST;
				ZString treamentRateNumber = AddInfo.ZA_TRN == "" ? new ZString("001") : AddInfo.ZA_TRN;

				result = InvoiceHeader.GSTExemptTariffRateCharacteristics.IsThisGSTExempt(TariffNumber, tariffRateNumber, prefScheme) ||
					InvoiceHeader.GSTExemptTreatmentRateCharacteristics.IsThisGSTExempt(TreatmentCode, treamentRateNumber, prefScheme);
			}
			return result;
		}

		[DecimalPlaces(4)]
		public ZDecimal WUV
		{
			get { return AddInfo.ZA_WUV > 0 ? AddInfo.ZA_WUV : CalculateWUV(); }
		}

		protected ZDecimal CalculateWUV()
		{
			ZDecimal warehouseUnitValue = 0m;
			if (IsNature20 || (Declaration != null && Declaration.IsExWarehouse))
			{
				if (AddInfo.ZA_WRQ > 0)
				{
					warehouseUnitValue = JI_CustomsValue / AddInfo.ZA_WRQ;
				}
				else if (JI_CustomsQuantity > 0)
				{
					warehouseUnitValue = JI_CustomsValue / JI_CustomsQuantity;
				}
				else if (JI_InvoiceQuantity > 0)
				{
					warehouseUnitValue = JI_CustomsValue / JI_InvoiceQuantity;
				}
			}
			return warehouseUnitValue.Round(4);
		}

		public ZPropertyInfo WUVInfo
		{
			get { return GetZPropertyInfo(nameof(WUV)); }
		}

		public ZString WarehouseCCP
		{
			get
			{
				var warehouseAddress = WarehouseAddress;
				return warehouseAddress == null ? ZString.Empty : warehouseAddress.LocalControlledPremisesID;
			}
		}

		public OrgAddress WarehouseAddress
		{
			get
			{
				OrgAddress result = null;
				JobDeclaration cachedDeclaration = Declaration;
				if (JI_IsPackToBondForLine || (cachedDeclaration != null && cachedDeclaration.IsExWarehouse))
				{
					result = AddInfo.WarehouseAddress;
					if (result == null && cachedDeclaration != null)
					{
						result = cachedDeclaration.WarehouseAddress;
					}
				}
				return result;
			}
		}

		public ZString NatureLegOrCMR
		{
			get
			{
				ZString result = ZString.Empty;

				if (Declaration.IsImport)
				{
					bool isImportCMR = Declaration.IsImportCMR;

					if (Declaration.IsExWarehouse)
					{
						result = isImportCMR ? CusEntryHeader.NatureTypesForImportCMR.Nature30 : CusEntryHeader.NatureTypes.Nature30;
					}
					else
					{
						if (JI_IsPackToBondForLine)
						{
							result = isImportCMR ? CusEntryHeader.NatureTypesForImportCMR.Nature20 : CusEntryHeader.NatureTypes.Nature20;
						}
						else
						{
							result = isImportCMR ? CusEntryHeader.NatureTypesForImportCMR.Nature10 : CusEntryHeader.NatureTypes.Nature10;
						}
					}
				}
				return result;
			}
		}

		public ZString Nature
		{
			get
			{
				ZString result = JobComInvoiceHeader.NatureString.NotDetermined;
				if (InvoiceHeader != null)
				{
					result = InvoiceHeader.Nature;
					if (result == JobComInvoiceHeader.NatureString.NotDetermined)
					{
						if (JI_IsPackToBondForLine)
						{
							result = JobComInvoiceHeader.NatureString.Nature20;
						}
						else
						{
							result = JobComInvoiceHeader.NatureString.Nature10;
						}
					}
				}
				return result;
			}
		}

		public CollectionCache CollectionCache
		{
			get { return Declaration.CollectionCache; }
		}

		public bool NeedsCustomsUQ
		{
			get { return OldValidation.NeedsCustomsUQ; }
		}

		#region DutyCalculator

		public DutyCalculator DutyCalculator // Here for caching purposes (reduces hits to Enterprise_AU reference database)
		{
			get
			{
				if (fDutyCalculator == null)
				{
					fDutyCalculator = new DutyCalculator(this);
				}
				return fDutyCalculator;
			}
		}

		#endregion

		#region JI_ParentLineCode

		public ZString JI_ParentLineCode
		{
			get
			{
				ZString result = ZString.Empty;
				if (ParentLine != null)
				{
					result = ParentLine.JI_LineNo + "/" + ParentLine.JI_Calc_Invoice;
				}
				return result;
			}
		}

		public ZPropertyInfo JI_ParentLineCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JI_ParentLineCode); }
		}

		#endregion

		#region JI_RX_LocalCurr

		public ZGuid JI_RX_LocalCurr
		{
			get
			{
				RefCurrency localCurrency = RefCurrency.LoadFromCurrencyCode(Factory, JobDeclaration.LocalCurrencyConstantCode);
				return localCurrency != null ? localCurrency.PK : ZGuid.Empty;
			}
		}
		public ZPropertyInfo JI_RX_LocalCurrInfo
		{
			get { return GetZPropertyInfo(nameof(JI_RX_LocalCurr)); }
		}

		#endregion

		#region JI_AUState

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_AUState_Hidden)]
		public ZString JI_AUState
		{
			get
			{
				if (AUStateCodeCollection.Count > 1)
				{
					return Res.GetString("47882437-FFF0-47C2-9BA9-CF765DE93F19", "MULT");
				}
				else
				{
					return AddInfo.ZA_AUState_Hidden;
				}
			}
			set
			{
				AddInfo.ZA_AUState_Hidden = value;
				stateCollection = null;
				AUStateCodeCollection.RefreshBinding();
			}
		}

		public ZPropertyInfo JI_AUStateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_AUState, x => AddInfo.ZA_AUState_HiddenInfo); }
		}

		protected internal bool JI_AUState_ReadOnly
		{
			get
			{
				return false;
			}
		}

		public AUStateCodeCollection AUStateCodeCollection
		{
			get
			{
				if (stateCollection == null)
				{
					stateCollection = new AUStateCodeCollection(this, Factory);
				}
				return stateCollection;
			}
		}
		AUStateCodeCollection stateCollection;

		#endregion

		#region JI_WeightedCostByLocation

		public ZDecimal JI_WeightedCostByLocation
		{
			get
			{
				ZDecimal result = 0m;
				if (this.Part != null && this.Declaration.ClientPickupDeliveryAddress != null)
				{
					foreach (OrgPartLocation location in Part.Locations)
					{
						if (location.OR_Warehouse.ToUpper() == this.Declaration.ClientPickupDeliveryAddress.Address.OA_Code.ToUpper())
						{
							result += location.OR_WeightCostThisLocation;
						}
					}
				}
				return result;
			}
		}

		public ZPropertyInfo JI_WeightedCostByLocationInfo
		{
			get { return GetZPropertyInfo(nameof(JI_WeightedCostByLocation)); }
		}

		#endregion

		public IEnumerable<ZString> Permits
		{
			get { return AddInfo.Permits; }
		}

		public IEnumerable<ZString> EncryptionNumbers
		{
			get { return AddInfo.EncryptionNumbers; }
		}

		public List<ZString> PermitsIncludingHeader
		{
			get
			{
				var items = new List<ZString>(Permits);
				if (InvoiceHeader != null)
				{
					items.AddRange(InvoiceHeader.Permits);
				}
				return items;
			}
		}

		public override ZString PermitNumber1
		{
			get { return PermitsIncludingHeader.Count > 0 ? PermitsIncludingHeader[0] : ZString.Empty; }
		}

		public override ZString PermitNumber2
		{
			get { return PermitsIncludingHeader.Count > 1 ? PermitsIncludingHeader[1] : ZString.Empty; }
		}

		public override ZString PermitNumber3
		{
			get { return PermitsIncludingHeader.Count > 2 ? PermitsIncludingHeader[2] : ZString.Empty; }
		}

		public override ZString TempImportNum
		{
			get { return JI_TempImportNum; }
		}

		public List<ZString> EncryptionNumbersIncludingHeader
		{
			get
			{
				var items = new List<ZString>(EncryptionNumbers);
				if (InvoiceHeader != null)
				{
					items.AddRange(InvoiceHeader.EncryptionNumbers);
				}
				return items;
			}
		}

		public ZDecimal DutiableInvoiceSpiritStrengthPercentage
		{
			get { return AddInfo.ZA_ISS > 0 ? AddInfo.ZA_ISS - NonDutiableComponent : 0; }
		}

		public decimal NonDutiableComponent
		{
			get
			{
				string tariffPrefix = JI_Tariff.Left(9);
				return (tariffPrefix == "2203.00.7" || tariffPrefix == "2203.00.6") ? 1.15m : 0m;
			}
		}

		[ChildEditable(true)]
		public new JobComInvChargeCollection<InvoiceLineCharge> Charges
		{
			get { return (JobComInvChargeCollection<InvoiceLineCharge>)base.Charges; }
		}

		protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection()
		{
			return new JobComInvChargeCollection<InvoiceLineCharge>(this);
		}

		#endregion

		#region New Fields Added from AddInfo

		#region JI_TempImportNum
		[ReadOnlyMember(nameof(JI_NoPermitRequired))]
		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_TemporaryImportNumbers_Hidden)]
		public ZString JI_TempImportNum
		{
			get { return AddInfo.ZA_TemporaryImportNumbers_Hidden; }
			set { AddInfo.ZA_TemporaryImportNumbers_Hidden = value; }
		}

		public ZPropertyInfo JI_TempImportNumInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_TempImportNum, x => AddInfo.ZA_TemporaryImportNumbers_HiddenInfo); }
		}

		#endregion

		#region JI_TempImportDate
		[ReadOnlyMember(nameof(JI_NoPermitRequired))]
		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_TemporaryImportDate_Hidden)]
		public ZDateTime JI_TempImportDate
		{
			get { return AddInfo.ZA_TemporaryImportDate_Hidden; }
			set { AddInfo.ZA_TemporaryImportDate_Hidden = value; }
		}

		public ZPropertyInfo JI_TempImportDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_TempImportDate, x => AddInfo.ZA_TemporaryImportDate_HiddenInfo); }
		}
		#endregion

		#region JI_NoPermitRequired
		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_NoPermitRequired_Hidden)]
		public ZBool JI_NoPermitRequired
		{
			get { return InvoiceHeader != null && InvoiceHeader.QuarantineExDocHeader != null && IsNEXDOCSActive && AddInfo.ZA_NoPermitRequired_Hidden; }
			set
			{
				AddInfo.ZA_NoPermitRequired_Hidden = value;
				if (JI_NoPermitRequired)
				{
					JI_TempImportNum = JobComInvoiceLine.CANConstants.NoPermitRequired;
					JI_TempImportDate = ZDate.Empty;
				}
			}
		}

		public ZPropertyInfo JI_NoPermitRequiredInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_NoPermitRequired, x => AddInfo.ZA_NoPermitRequired_HiddenInfo); }
		}

		#endregion

		#region JI_RelatedExportPermitNumber
		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_RelatedExportPermitNumber_Hidden)]
		public ZString JI_RelatedExportPermitNumber
		{
			get { return AddInfo.ZA_RelatedExportPermitNumber_Hidden; }
			set { AddInfo.ZA_RelatedExportPermitNumber_Hidden = value; }
		}
		public ZPropertyInfo JI_RelatedExportPermitNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_RelatedExportPermitNumber, x => AddInfo.ZA_RelatedExportPermitNumber_HiddenInfo); }
		}
		#endregion

		#region JI_RelatedExportPermitAuthority
		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_RelatedExportPermitAuthority_Hidden)]
		public ZString JI_RelatedExportPermitAuthority
		{
			get { return AddInfo.ZA_RelatedExportPermitAuthority_Hidden; }
			set { AddInfo.ZA_RelatedExportPermitAuthority_Hidden = value; }
		}
		public ZPropertyInfo JI_RelatedExportPermitAuthorityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_RelatedExportPermitAuthority, x => AddInfo.ZA_RelatedExportPermitAuthority_HiddenInfo); }
		}
		#endregion

		#region JI_RelatedExportPermitDate
		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_RelatedExportPermitDate_Hidden)]
		public ZDateTime JI_RelatedExportPermitDate
		{
			get { return AddInfo.ZA_RelatedExportPermitDate_Hidden; }
			set { AddInfo.ZA_RelatedExportPermitDate_Hidden = value; }
		}
		public ZPropertyInfo JI_RelatedExportPermitDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_RelatedExportPermitDate, x => AddInfo.ZA_RelatedExportPermitDate_HiddenInfo); }
		}
		#endregion

		#region JI_GoodsOriginCode
		public ZString JI_GoodsOriginCode
		{
			get
			{
				if (IsAUOrigin && !JI_AUState.IsEmpty)
				{
					switch (JI_AUState)
					{
						case "ACT":
							fJI_OriginCode = "AU-CT";
							break;
						case "NSW":
							fJI_OriginCode = "AU-NS";
							break;
						case "QLD":
							fJI_OriginCode = "AU-QL";
							break;
						case "SA":
							fJI_OriginCode = "AU-SA";
							break;
						case "VIC":
							fJI_OriginCode = "AU-VI";
							break;
						case "WA":
							fJI_OriginCode = "AU-WA";
							break;
						case "TAS":
							fJI_OriginCode = "AU-TS";
							break;
						case "NT":
							fJI_OriginCode = "AU-NT";
							break;
						default:
							fJI_OriginCode = "";
							break;
					}
					return fJI_OriginCode;
				}
				else
				{
					return new ZString(ForeignCountryCode);
				}
			}
		}
		#endregion

		#region JI_Drawback

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_Drawback_Hidden)]
		public ZBool JI_Drawback
		{
			get { return new ZBool(AddInfo.ZA_Drawback_Hidden == Enterprise.Core.Constants.BooleanTrueString); }
			set
			{
				if (JI_Drawback != value)
				{
					AddInfo.ZA_Drawback_Hidden = value.ToString();
					if (!IsValidationSuspended)
					{
						OldValidation.ValidateJI_Drawback();
					}
					HasChanges = true;
				}
			}
		}

		public ZPropertyInfo JI_DrawbackInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_Drawback, x => AddInfo.ZA_Drawback_HiddenInfo); }
		}

		#endregion

		#region JI_Texco

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_Texco_Hidden)]
		public ZBool JI_Texco
		{
			get { return new ZBool(AddInfo.ZA_Texco_Hidden == Enterprise.Core.Constants.BooleanTrueString); }
			set
			{
				if (JI_Texco != value)
				{
					AddInfo.ZA_Texco_Hidden = value.ToString();
					if (!IsValidationSuspended)
					{
						OldValidation.ValidateJI_Texco();
					}
					HasChanges = true;
				}
			}
		}

		public ZPropertyInfo JI_TexcoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_Texco, x => AddInfo.ZA_Texco_HiddenInfo); }
		}

		#endregion

		#region JI_MotorVehiclePlan

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_MotorVehiclePlan_Hidden)]
		public ZBool JI_MotorVehiclePlan
		{
			get { return new ZBool(AddInfo.ZA_MotorVehiclePlan_Hidden == Enterprise.Core.Constants.BooleanTrueString); }
			set
			{
				if (JI_MotorVehiclePlan != value)
				{
					AddInfo.ZA_MotorVehiclePlan_Hidden = value.ToString();
					if (!IsValidationSuspended)
					{
						OldValidation.ValidateJI_MotorVehiclePlan();
					}
					HasChanges = true;
				}
			}
		}

		public ZPropertyInfo JI_MotorVehiclePlanInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_MotorVehiclePlan, x => AddInfo.ZA_MotorVehiclePlan_HiddenInfo); }
		}

		#endregion

		#region ZA_ValuationBasis - Only to solve binding problem when tabbing through the field

		public ZString ZA_ValuationBasis_Hidden
		{
			get { return AddInfo.ZA_ValuationBasis_Hidden; }
			set { AddInfo.ZA_ValuationBasis_Hidden = value; }
		}

		public ZPropertyInfo ZA_ValuationBasis_HiddenInfo
		{
			get { return GetWrappedZPropertyInfo(AddInfo.ZA_ValuationBasis_HiddenInfo.Name, x => AddInfo.ZA_ValuationBasis_HiddenInfo); }
		}

		#endregion

		#endregion

		#region InstrumentType

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_InstrumentType_Hidden)]
		[List(nameof(AddInfo) + "." + nameof(AUAddInfo.Lookups) + "." + nameof(AUAddInfoLookups.ZA_InstrumentType_List))]
		public ZString InstrumentType
		{
			get { return AddInfo.ZA_InstrumentType_Hidden; }
			set
			{
				if (InstrumentType != value)
				{
					AddInfo.ZA_InstrumentType_Hidden = value;
					SetInstrumentTypeToConcessionOrder();
					InstrumentTypeInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					ValidateInstrumentType();
				}
			}
		}

		public void ValidateInstrumentType()
		{
			OldValidation.ValidateInstrumentType();
		}

		public ZPropertyInfo InstrumentTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.InstrumentType, x => AddInfo.ZA_InstrumentType_HiddenInfo); }
		}

		public override ZString JI_ConcessionOrder
		{
			get
			{
				return base.JI_ConcessionOrder;
			}
			set
			{
				if (value == "|")
				{
					if (!base.JI_ConcessionOrder.IsEmpty)
					{
						base.JI_ConcessionOrder = "";
					}
				}
				else if (base.JI_ConcessionOrder != value)
				{
					base.JI_ConcessionOrder = value;
				}
			}
		}

		protected void SetInstrumentTypeToConcessionOrder()
		{
			if (JI_ConcessionOrder.IndexOf('|') != -1)
			{
				JI_ConcessionOrder = InstrumentType + "|" + JI_ConcessionOrder.Split('|')[1];
			}
			else
			{
				JI_ConcessionOrder = InstrumentType + "|";
			}
		}

		#endregion

		#region InstrumentCode

		[MaxLength(AUAddInfo.Schema.ZA_InstrumentCode_HiddenMaxLength)]
		public ZString InstrumentCode
		{
			get { return AddInfo.ZA_InstrumentCode_Hidden; }
			set
			{
				if (AddInfo.ZA_InstrumentCode_Hidden != value)
				{
					AddInfo.ZA_InstrumentCode_Hidden = value;
					SetInstrumentCodeToConcessionOrder();
					InstrumentCodeInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					OldValidation.ValidateInstrumentCode();
				}
			}
		}

		public ZPropertyInfo InstrumentCodeInfo
		{
			get { return GetZPropertyInfo(Schema.InstrumentCode); }
		}

		protected void SetInstrumentCodeToConcessionOrder()
		{
			if (JI_ConcessionOrder.IndexOf('|') != -1)
			{
				JI_ConcessionOrder = JI_ConcessionOrder.Split('|')[0] + "|" + InstrumentCode;
			}
			else
			{
				JI_ConcessionOrder = "|" + InstrumentCode;
			}
		}

		#endregion

		#region LinePrefix

		[MaxLength(1)]
		public ZString JI_LinePrefix
		{
			get { return AddInfo.ZA_LinePrefix_Hidden; }
			set
			{
				HasChanges = AddInfo.ZA_LinePrefix_Hidden != value;
				if (value == JobComInvoiceLine.LinePrefixString.Parent)
				{
					JobComInvoiceLine nextLine = GetNextLine();
					if (nextLine != null)
					{
						AddInfo.ZA_RelatedLinePK_Hidden = nextLine.PK.ToString();
						nextLine.JI_ParentLine = JI_LineNo;
						nextLine.AddInfo.ZA_RelatedLinePK_Hidden = PK.ToString();
						nextLine.AddInfo.ZA_LinePrefix_Hidden = JobComInvoiceLine.LinePrefixString.Trailer;
					}
				}
				else if (value == JobComInvoiceLine.LinePrefixString.Trailer)
				{
					JobComInvoiceLine parentLine = GetAboveLine();
					if (parentLine != null)
					{
						AddInfo.ZA_RelatedLinePK_Hidden = parentLine.PK.ToString();
						parentLine.AddInfo.ZA_RelatedLinePK_Hidden = PK.ToString();
						parentLine.AddInfo.ZA_LinePrefix_Hidden = JobComInvoiceLine.LinePrefixString.Parent;
					}
				}
				else if (value == JobComInvoiceLine.LinePrefixString.Normal || value == "")
				{
					FreeUpChildLineIfNecessary();
				}

				AddInfo.ZA_LinePrefix_Hidden = value;
				if (!IsValidationSuspended)
				{
					OldValidation.ValidateJI_LinePrefix();
				}
				JI_LinePrefixInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JI_LinePrefixInfo
		{
			get { return GetZPropertyInfo(Schema.JI_LinePrefix); }
		}

		public bool JI_LinePrefix_ReadOnly
		{
			get
			{
				return JI_LinePrefix == LinePrefixString.Trailer
						&& ParentLine != null
						&& ParentLine.ChildLine == this;
			}
		}

		JobComInvoiceLine GetNextLine()
		{
			int currentIndex = -1;
			for (int index = 0; index < Declaration.InvoiceLines.Count; index++)
			{
				if (Declaration.InvoiceLines[index] == this)
				{
					currentIndex = index;
					break;
				}
			}

			JobComInvoiceLine result = null;
			if (currentIndex != -1 && currentIndex < Declaration.InvoiceLines.Count - 1)
			{
				result = Declaration.InvoiceLines[currentIndex + 1];
			}
			return result;
		}

		JobComInvoiceLine GetAboveLine()
		{
			int currentIndex = -1;
			for (int index = 0; index < Declaration.InvoiceLines.Count; index++)
			{
				if (Declaration.InvoiceLines[index] == this)
				{
					currentIndex = index;
					break;
				}
			}

			JobComInvoiceLine result = null;
			if (currentIndex > 0)
			{
				result = Declaration.InvoiceLines[currentIndex - 1];
			}
			return result;
		}

		#endregion

		#region JI_IsPackToBondForLine_Hidden

		public ZBool JI_IsPackToBondForLine
		{
			get { return AddInfo.ZA_IsPackToBondForLine_Hidden == "Y" || IsWarehousedByExternalAgent; }
			set
			{
				if (JI_IsPackToBondForLine != value)
				{
					AddInfo.ZA_IsPackToBondForLine_Hidden = value ? "Y" : "N";
					HasChanges = true;
					Declaration?.InvalidateIsNature10Cache();
					if (value)
					{
						AddInfo.SynchroniseInvoiceQtyAndWRUIfRequired(AUAddInfo.SyncDirection.FromInvoiceLineToAddInfo);
					}
					else
					{
						AddInfo.ClearWarehouseRelatedProperties();
					}
				}
				JI_IsPackToBondForLineInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					OldValidation.ValidateJI_IsPackToBondForLine();
					AddInfo.Validation.ValidateZA_WRU();
				}
			}
		}

		public ZPropertyInfo JI_IsPackToBondForLineInfo
		{
			get { return GetZPropertyInfo(Schema.JI_IsPackToBondForLine); }
		}

		protected bool JI_IsPackToBondForLine_ReadOnly
		{
			get { return IsWarehousedByExternalAgent; }
		}

		bool IsWarehousedByExternalAgent
		{
			get
			{
				JobDeclaration cachedDeclaration = Declaration;
				return cachedDeclaration != null && cachedDeclaration.IsWarehousedByExternalAgent;
			}
		}

		#endregion

		#region Calculated Properties

		public ZDecimal JI_Calc_TNI
		{
			get { return InvoiceHeader == null || InvoiceHeader.IsDeleted ? ZDecimal.Zero : TransportAndInsurance.Amount; }
		}

		public ZPropertyInfo JI_Calc_TNIInfo
		{
			get { return GetZPropertyInfo(nameof(JI_Calc_TNI)); }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CurrencyList))]
		public ZGuid JI_RX_Calc_TNICurrency
		{
			get
			{
				var result = ZGuid.Empty;

				if (InvoiceHeader != null && !InvoiceHeader.IsDeleted)
				{
					var currency = TransportAndInsurance.Currency;
					result = currency != null ? currency.PK : ZGuid.Empty;
				}

				return result;
			}
		}

		public ZPropertyInfo JI_RX_Calc_TNICurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(JI_RX_Calc_TNICurrency)); }
		}

		#region From dbo.CusEntryLine

		#region JI_Calc_InterimAntiDumpingDuty

		public ZDecimal JI_Calc_InterimAntiDumpingDuty
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(this.CusEntryLine.InterimAntiDumpingDuty).Amount; }
		}
		public ZPropertyInfo JI_Calc_InterimAntiDumpingDutyInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_InterimAntiDumpingDuty); }
		}

		#endregion

		#region JI_Calc_InterimCountervailingDuty

		public ZDecimal JI_Calc_InterimCountervailingDuty
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(this.CusEntryLine.InterimCountervailingDuty).Amount; }
		}
		public ZPropertyInfo JI_Calc_InterimCountervailingDutypingDutyInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_InterimCountervailingDuty); }
		}

		#endregion

		#region JI_Calc_WETAmount

		public ZDecimal JI_Calc_WETAmount
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.WETAmount).Amount; }
		}
		public ZPropertyInfo JI_Calc_WETAmountvailingDutypingDutyInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_WETAmount); }
		}

		public ZDecimal JI_Calc_WETAmountIncludingWHEstimate
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.WETAmountIncludingWHEstimate).Amount; }
		}
		#endregion

		#region JI_Calc_LCTAmount

		public ZDecimal JI_Calc_LCTAmount
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(this.CusEntryLine.LCTAmount).Amount; }
		}
		public ZPropertyInfo JI_Calc_LCTAmountngDutypingDutyInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_LCTAmount); }
		}

		public ZDecimal JI_Calc_LCTAmountIncludingWHEstimate
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.LCTAmountIncludingWHEstimate).Amount; }
		}
		#endregion

		#region JI_Calc_AQISContainerCharges

		public virtual ZDecimal JI_Calc_AQISContainerCharges
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.AQISContainerCharges).Amount; }
		}

		#endregion

		#region JI_Calc_AQISProcessingCharge

		public virtual ZDecimal JI_Calc_AQISProcessingCharge
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.AQISProcessingCharge).Amount; }
		}

		#endregion

		#region JI_Calc_AQISServiceAmount

		public virtual ZDecimal JI_Calc_AQISServiceAmount
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.AQISServiceAmount).Amount; }
		}

		#endregion

		#region JI_Calc_WoodLevy

		public ZDecimal JI_Calc_WoodLevy
		{
			get { return CusEntryLine == null ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.Woodlevy).Amount; }
		}

		public ZDecimal JI_Calc_WoodLevyIncludingWHEstimate
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.WoodLevyIncludingWHEstimate).Amount; }
		}
		#endregion

		#region JI_Calc_DutyPercent

		public ZDecimal JI_Calc_DutyPercent
		{
			get { return (CusEntryLine == null) ? 0 : CusEntryLine.CL_DutyPercent; }
		}
		public ZPropertyInfo JI_Calc_DutyPercentInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_DutyPercent); }
		}

		#endregion

		#region JI_Calc_FlatDutyPortion

		public ZDecimal JI_Calc_FlatDutyPortion
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(this.CusEntryLine.FlatDutyPortion).Amount; }
		}
		public ZPropertyInfo JI_Calc_FlatDutyPortionypingDutyInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_FlatDutyPortion); }
		}

		#endregion

		#region JI_Calc_DumpingDuty

		public
#if DEBUG
 virtual
#endif
 ZDecimal JI_Calc_DumpingDuty
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(this.CusEntryLine.DumpingDuty).Amount; }
		}

		#endregion

		#region JI_Calc_CountervailingDuty

		public
#if DEBUG
 virtual
#endif
 ZDecimal JI_Calc_CountervailingDuty
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(this.CusEntryLine.CountervailingDuty).Amount; }
		}

		#endregion

		#region JI_Calc_OtherDuty

		public ZDecimal JI_Calc_AllOtherDuties
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(this.CusEntryLine.AllOtherDuty).Amount; }
		}

		public ZPropertyInfo JI_Calc_AllOtherDutiesInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_AllOtherDuties); }
		}

		#endregion

		#region JI_Calc_EntryFee

		public ZDecimal JI_Calc_EntryFee
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.EntryFee).Amount; }
		}

		public ZPropertyInfo JI_Calc_EntryFeeInfo
		{
			get { return GetZPropertyInfo(nameof(JI_Calc_EntryFee)); }
		}

		#endregion

		#region JI_Calc_AllEntryFees

		public ZDecimal JI_Calc_AllEntryFees
		{
			get
			{
				return GetAllEntryFeesCore();
			}
		}

		protected virtual ZDecimal GetAllEntryFeesCore()
		{
			return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.AllEntryFees).Amount;
		}

		#endregion

		#region JI_Calc_TradegateGST

		public ZDecimal JI_Calc_TradegateGST
		{
			get
			{
				return GetTradegateGSTCore();
			}
		}

		protected virtual ZDecimal GetTradegateGSTCore()
		{
			return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.TradegateGST).Amount;
		}

		#endregion

		#region JI_Calc_VOTI

		public ZDecimal JI_Calc_VOTI
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.VOTI).Amount; }
		}

		#endregion

		#region JI_Calc_CustomsTransportAndInsuranceInLocalCurrency

		public ZDecimal JI_Calc_CustomsTransportAndInsuranceInLocalCurrency
		{
			get { return (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount).Amount; }
		}

		#endregion

		#region FlatRateDescription

		public ZString FlatRateDescription
		{
			get { return CusEntryLine == null ? ZString.Empty : CusEntryLine.FlatRateDescription; }
		}

		public ZPropertyInfo FlatRateDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(FlatRateDescription)); }
		}

		#endregion

		#endregion

		#region Calculated Charges for Message Builder

		protected ChargeCodeKey fChargeCodeKey;
		protected ChargeCodeKey ChargeCodeKey
		{
			get
			{
				if (fChargeCodeKey == null)
				{
					fChargeCodeKey = new ChargeCodeKey();
				}
				return fChargeCodeKey;
			}
		}

		protected MessageChargeCodeKey fMessageChargeCodeKey;
		protected MessageChargeCodeKey MessageChargeCodeKey
		{
			get
			{
				if (fMessageChargeCodeKey == null)
				{
					fMessageChargeCodeKey = new MessageChargeCodeKey();
				}
				return fMessageChargeCodeKey;
			}
		}

		public Money JI_CustomsTotalValue
		{
			get
			{
				if (InvoiceHeader != null)
				{
					ZDecimal amount = 0m;
					if (InvoiceHeader.Nature == JobComInvoiceHeader.NatureString.Nature30)
					{
						amount = JI_LinePrice;
					}

					return new Money(amount, InvoiceHeader.Invoice_Currency);
				}
				return Money.Empty;
			}
		}

		public Money JI_BuyingCommission
		{
			get
			{
				Money dutiable = GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.DutiableBuyingCommissionExcluded]);
				Money nonDutiable = GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.NonDutiableBuyingCommissionIncluded]);
				Money result = CurrencyConverter.Add(dutiable, new Money(-nonDutiable.Amount, nonDutiable.Currency));
				return GetEmptyMoneyInInvoiceCurrencyIfEmptyAmount(result);
			}
		}

		public Money JI_OtherCommission
		{
			get
			{
				Money dutiable = GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.DutiableOtherCommissionExcluded]);
				Money nonDutiable = GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.NonDutiableOtherCommissionIncluded]);
				Money result = CurrencyConverter.Add(dutiable, new Money(-nonDutiable.Amount, nonDutiable.Currency));
				return GetEmptyMoneyInInvoiceCurrencyIfEmptyAmount(result);
			}
		}

		public Money JI_Commission
		{
			get
			{
				Money dutiable = GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.DutiableCommissionExcluded]);
				Money nonDutiable = GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.NonDutiableCommissionIncluded]);

				Money result = CurrencyConverter.Add(dutiable, new Money(-nonDutiable.Amount, nonDutiable.Currency));
				return GetEmptyMoneyInInvoiceCurrencyIfEmptyAmount(result);
			}
		}

		public override Money TransportAndInsurance
		{
			get
			{
				Money result = AddInfo.TILVMoney;
				if (AddInfo.EffectiveTILVString.IsEmpty && result.Amount == 0.0m)
				{
					result = base.TransportAndInsurance;
					result = CurrencyConverter.Add(result, JI_NonDutiableGSTibleCharges);
				}
				result = result.Round(2);
				return result;
			}
		}

		public ZDecimal TransportAndInsuranceInLocalCurrency
		{
			get
			{
				return CurrencyConverter.ConvertExact(TransportAndInsurance, JobDeclaration.GetLocalCurrency()).Amount;
			}
		}

		public Money JI_PackingCosts
		{
			get
			{
				Money result = GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.PackingCostExcluded]);
				return GetEmptyMoneyInInvoiceCurrencyIfEmptyAmount(result);
			}
		}

		/// <summary>
		/// Added to Dutiable Other Charge1
		/// </summary>
		public Money JI_ExWorksAmount
		{
			get
			{
				Money result = GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.DutyGSTExWorksAmountExcluded]);
				return GetEmptyMoneyInInvoiceCurrencyIfEmptyAmount(result);
			}
		}

		public Money JI_LandingCharges
		{
			get
			{
				Money result = GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.LandingChargesIncluded]);
				return GetEmptyMoneyInInvoiceCurrencyIfEmptyAmount(result);
			}
		}

		public Money JI_Discount
		{
			get
			{
				Money result = GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.NonDutyNonGSTDiscountExcluded]);
				return GetEmptyMoneyInInvoiceCurrencyIfEmptyAmount(result);
			}
		}

		/// <summary>
		/// Dutiable, GSTible
		/// </summary>
		public Money JI_OtherCharges1
		{
			get
			{
				Money result = GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.DutyGSTAdditionChargeExcluded]);
				result = CurrencyConverter.Add(result, GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.DutyGSTOtherChargesExcluded]));
				result = CurrencyConverter.Add(result, JI_ExWorksAmount);
				return GetEmptyMoneyInInvoiceCurrencyIfEmptyAmount(result);
			}
		}

		/// <summary>
		/// Non-Dutiable, Non-GSTible
		/// </summary>
		public Money JI_OtherCharges2
		{
			get
			{
				Money result = GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.NonDutyNonGSTDeductionChargeIncluded]);
				result = CurrencyConverter.Add(result, GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.NonDutyNonGSTOtherChargesIncluded]));
				result = CurrencyConverter.Add(result, GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.NonDutyGSTDeductionChargeIncluded]));
				result = CurrencyConverter.Add(result, GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.NonDutyGSTOtherChargesIncluded]));
				result = CurrencyConverter.Add(result, GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.NonDutyGSTForeignInlandFreightIncluded]));
				result = CurrencyConverter.Add(result, GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.NonDutyGSTExWorksAmountIncluded]));
				return GetEmptyMoneyInInvoiceCurrencyIfEmptyAmount(result);
			}
		}

		/// <summary>
		/// OTH, FIFT and EXW
		/// </summary>
		protected Money JI_NonDutiableGSTibleCharges
		{
			get
			{
				Money result = GetCharge(ChargeCodeKey[ChargeCodeKey.NonDutyGSTDeductionCharge]);

				CurrencyConverter currencyConverter = this.CurrencyConverter;

				if (currencyConverter != null)
				{
					result = currencyConverter.Add(result, GetCharge(ChargeCodeKey[ChargeCodeKey.NonDutyGSTOtherCharges]));
					result = currencyConverter.Add(result, GetCharge(ChargeCodeKey[ChargeCodeKey.NonDutyGSTFIFT]));
					result = currencyConverter.Add(result, GetCharge(ChargeCodeKey[ChargeCodeKey.NonDutyGSTExWorksAmount]));
				}

				return GetEmptyMoneyInInvoiceCurrencyIfEmptyAmount(result);
			}
		}

		/// <summary>
		/// Dutiable, GST-ible
		/// </summary>
		public Money JI_ForeignInlandFreight
		{
			get
			{
				Money result = GetCharge(MessageChargeCodeKey[MessageChargeCodeKey.DutyGSTForeignInlandFreightExcluded]);
				return GetEmptyMoneyInInvoiceCurrencyIfEmptyAmount(result);
			}
		}

		public ZDecimal JI_PriceAdjustmentInLocalCurrency
		{
			get
			{
				return CurrencyConverter.ConvertExact(JI_PriceAdjustment, JobDeclaration.GetLocalCurrency()).Amount;
			}
		}

		public Money JI_PriceAdjustment
		{
			get
			{
				Money result = Money.Empty;
				if (!AddInfo.ZA_ADJ.IsEmpty)
				{
					if (AddInfo.AdjustmentDollarPercentage_Hidden == "%")
					{
						Money linePrice = JI_LinePriceMoney;
						result = new Money(linePrice.Amount / 100 * AddInfo.AdjustmentAmount_Hidden, linePrice.Currency);
					}
					else if (AddInfo.AdjustmentDollarPercentage_Hidden == "$")
					{
						if (AddInfo.AdjustmentCurrency != null && InvoiceHeader != null)
						{
							result = new Money(AddInfo.AdjustmentAmount_Hidden, AdjustmentRefCurrency);
						}
					}
				}
				return result;
			}
		}

		public Money LinePriceIncludingAdjustment
		{
			get { return CurrencyConverter.Add(JI_LinePriceMoney, JI_PriceAdjustment); }
		}

		protected RefCurrency AdjustmentRefCurrency
		{
			get { return (RefCurrency)Factory.Load(typeof(RefCurrency), AddInfo.AdjustmentCurrency.PK); }
		}

		public Money JI_LCT
		{
			get { return new Money(AddInfo.ZA_LCT, LocalCurrency); }
		}

		public Money JI_StandardDuty
		{
			get { return new Money(AddInfo.ZA_STD, LocalCurrency); }
		}

		public Money JI_OtherDutyFactor
		{
			get { return new Money(AddInfo.ZA_ODF, LocalCurrency); }
		}

		public Money JI_DumpingExportPrice
		{
			get { return new Money(AddInfo.DumpingExportAmount, AddInfo.DumpingExportCurrency); }
		}

		public Money JI_WarehouseUnitValue
		{
			get { return new Money(WUV, LocalCurrency); }
		}

		// this is pre CMR legacy
		public Money JI_SecurityConcession
		{
			get { return new Money(AddInfo.ZA_CON, LocalCurrency); }
		}

		public ZString JI_SecondQuantityUQ
		{
			get { return AddInfo.ZA_UQ2; }
		}

		public ZDecimal JI_SecondQuantity
		{
			get { return AddInfo.ZA_QT2; }
		}

		public ZInt JI_WarehouseRelatedLine
		{
			get { return AddInfo.ZA_WRL; }
		}

		public override RefCurrency LocalCurrency
		{
			get { return RefCurrency.LoadFromCurrencyCode(Factory, JobDeclaration.LocalCurrencyConstantCode); }
		}

		#endregion

		#endregion

		#region IAddInfo Members

		bool isLoadingAddInfo;
		public AUAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					if (isLoadingAddInfo)
					{
						ErrorReporter.ReportOnce("JobComInvoiceLine.LoadAddInfo", "AddInfo was recursively loaded, and would have ended in a stack overflow.");
					}

					isLoadingAddInfo = true;
					try
					{
						fAddInfo = new AUAddInfo(this, JI_AddInfoInfo);
					}
					finally
					{
						isLoadingAddInfo = false;
					}

					RegisterEditableChildObject(fAddInfo);
					SetAddInfoDefaults(fAddInfo);
				}

				return fAddInfo;
			}
		}

		void SetAddInfoDefaults(AUAddInfo addInfo)
		{
			var declaration = Declaration;
			if (!IsCopying && !IsInDatabase && declaration != null && declaration.SupportsBondedWarehousing && declaration.IsExWarehouse)
			{
				addInfo.UseBondedWarehouseAutomation = true;
			}

			using (addInfo.SuspendSettingHasChanges())
			{
				((ILightValidationInternals)addInfo).IsValid = ((ILightValidationInternals)this).IsValid;
			}
		}

		public ZDateTime DateOfValuation
		{
			get { return InvoiceHeader != null ? InvoiceHeader.EffectiveValuationDate : ZDateTime.Now; }
		}

		public ZString AggregatedZA_ORG
		{
			get { return AddInfo.ZA_ORG.IsEmpty && AddInfo.ZA_PRF.IsEmpty && InvoiceHeader != null ? InvoiceHeader.AggregatedZA_ORG : AddInfo.ZA_ORG; }
		}

		public ZString AggregatedZA_PRF
		{
			get { return AddInfo.ZA_ORG.IsEmpty && AddInfo.ZA_PRF.IsEmpty && InvoiceHeader != null ? InvoiceHeader.AggregatedZA_PRF : AddInfo.ZA_PRF; }
		}

		public ZString AggregatedZA_PST
		{
			get { return AddInfo.ZA_PST.IsEmpty && AddInfo.ZA_POC.IsEmpty && AddInfo.ZA_PRT.IsEmpty && InvoiceHeader != null ? InvoiceHeader.AddInfo.ZA_PST : AddInfo.ZA_PST; }
		}

		public ZString AggregatedZA_PRT
		{
			get { return AddInfo.ZA_PRT.IsEmpty && AddInfo.ZA_PST.IsEmpty && AddInfo.ZA_POC.IsEmpty && InvoiceHeader != null ? InvoiceHeader.AddInfo.ZA_PRT : AddInfo.ZA_PRT; }
		}

		public ZString AggregatedZA_POC
		{
			get { return AddInfo.ZA_POC.IsEmpty && AddInfo.ZA_PRT.IsEmpty && AddInfo.ZA_PST.IsEmpty && InvoiceHeader != null ? InvoiceHeader.AddInfo.ZA_POC : AddInfo.ZA_POC; }
		}

		public ZString AggregatedZA_GSTE
		{
			get
			{
				return AddInfo.ZA_GSTE.IsEmpty && InvoiceHeader != null ? InvoiceHeader.AddInfo.ZA_GSTE : AddInfo.ZA_GSTE;
			}
		}

		public bool IsGeneralRate
		{
			get { return AggregatedZA_PST == AUAddInfo.GeneralPreferenceRate || HasNoPreferentialRate; }
		}

		public ZString AggregatedAddInfoLine
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				foreach (ZPropertyInfo info in AddInfo.ZPropertyInfoHash)
				{
					if (info.Name.StartsWith(AddInfo.TablePrefix) && !info.Name.EndsWith("_Hidden"))
					{
						IZType aggregatedInfo;
						if (info.Name.EndsWith("ORG"))
						{
							aggregatedInfo = AggregatedZA_ORG;
						}
						else if (info.Name.EndsWith("PRF"))
						{
							aggregatedInfo = AggregatedZA_PRF;
						}
						else
						{
							aggregatedInfo = AggregatedValue(info.Name);
						}

						if (!aggregatedInfo.IsEmpty)
						{
							if (!result.IsEmpty)
							{
								result.Append("*");
							}

							result.Append(info.Name.Substring(3) + "=" + aggregatedInfo.ToString());
						}
					}
				}

				return result.ToString();
			}
		}

		public IZType AggregatedValue(string propertyName)
		{
			IZType result = (IZType)AddInfo[propertyName];
			if (result.IsEmpty && Master != null)
			{
				if (IsPreferenceColumn(propertyName) && HasNoPreferentialRate)
				{
					if (propertyName == AUAddInfoSchema.ZA_PST.Name)
					{
						result = (ZString)AUAddInfo.GeneralPreferenceRate;
					}
				}
				else
				{
					if (propertyName == AUAddInfoSchema.Constants.ZA_RNO)
					{
						CodeDescriptionPairList rNOList = AddInfo.Lookups.ZA_RNO_List;
						if (rNOList.Count == 1 && rNOList[0].Code == AUAddInfoLookups.GeneralRateNo)
						{
							result = (ZString)AUAddInfoLookups.GeneralRateNo;
						}
					}

					if (result.IsEmpty)
					{
						result = ((IAddInfo)Master).AggregatedValue(propertyName);
					}
				}
			}

			return result;
		}

		bool IsPreferenceColumn(string propertyName)
		{
			return propertyName == AUAddInfoSchema.ZA_PRT.Name || propertyName == AUAddInfoSchema.ZA_POC.Name || propertyName == AUAddInfoSchema.ZA_PST.Name;
		}

		internal bool HasNoPreferentialRate => AddInfo.ZA_PST.IsEmpty && WillApplyGstScheme;

		bool IAggregatedAddInfo.IsCopying
		{
			get { return IsCopying; }
		}

		#endregion

		#region Preference Detail Inheritings

		public ZString HeaderPrefOrg
		{
			get { return InvoiceHeader == null ? ZString.Empty : InvoiceHeader.AddInfo.ZA_POC; }
		}

		public ZPropertyInfo HeaderPrefOrgInfo
		{
			get { return GetZPropertyInfo(nameof(HeaderPrefOrg)); }
		}

		public ZString HeaderPrefScheme
		{
			get { return InvoiceHeader == null ? ZString.Empty : InvoiceHeader.AddInfo.ZA_PST; }
		}

		public ZPropertyInfo HeaderPrefSchemeInfo
		{
			get { return GetZPropertyInfo(nameof(HeaderPrefScheme)); }
		}

		public ZString HeaderPrefRule
		{
			get { return InvoiceHeader == null ? ZString.Empty : InvoiceHeader.AddInfo.ZA_PRT; }
		}

		public ZPropertyInfo HeaderPrefRuleInfo
		{
			get { return GetZPropertyInfo(nameof(HeaderPrefRule)); }
		}

		public override ZString HeaderOrigin
		{
			get { return InvoiceHeader == null ? ZString.Empty : InvoiceHeader.AddInfo.ZA_ORG; }
		}

		public ZString HeaderValuationBasis
		{
			get { return InvoiceHeader == null ? ZString.Empty : InvoiceHeader.AddInfo.ZA_VALB_Hidden; }
		}

		public ZPropertyInfo HeaderValuationBasisInfo
		{
			get { return GetZPropertyInfo(nameof(HeaderValuationBasis)); }
		}

		public bool InheritingPreferenceDetailsFromHeader
		{
			get
			{
				bool result;
				var addInfo = AddInfo;
				var headerAddInfo = InvoiceHeader?.AddInfo;

				result =
					headerAddInfo != null
					&& InheritingSchemeFromHeader && !WillApplyGstScheme && headerAddInfo.ZA_PST != AUAddInfo.GeneralPreferenceRate
					&& !headerAddInfo.ZA_POC.IsEmpty && addInfo.ZA_POC.IsEmpty
					&& !headerAddInfo.ZA_PRT.IsEmpty && addInfo.ZA_PRT.IsEmpty;
				return result;
			}
		}

		public bool InheritingSchemeFromHeader => !HeaderPrefScheme.IsEmpty && AddInfo.ZA_PST.IsEmpty;

		public bool WillApplyGstScheme
		{
			get
			{
				var pSTList = AddInfo.Lookups.ZA_PST_List;
				return pSTList.Count == 1 && pSTList[0].Code == AUAddInfo.GeneralPreferenceRate;
			}
		}

		#endregion

		#region Implementation

		protected override bool SupportsBondedWarehousingCore
		{
			get
			{
				return Declaration?.SupportsBondedWarehousing ?? false;
			}
		}

		protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.Australia;
		protected override Type TypeOfPartUsedCore => typeof(AUOrgSupplierPart);

		AUAddInfo fAddInfo;
		DutyCalculator fDutyCalculator;
		ZString fJI_OriginCode;

		public void CalculateLineWeightOrQuantityFromCustomsQty()
		{
			CustomsQuantityConverter.CalculateLineWeightOrQuantityFromCustomsQty();
			CustomsQuantity2Converter.CalculateLineWeightOrQuantityFromCustomsQty();
		}

		public new JobComInvoiceLineLookups Lookups
		{
			get { return (JobComInvoiceLineLookups)base.Lookups; }
		}

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups()
		{
			if (!IsQuarantine)
			{
				return new JobComInvoiceLineLookups(this);
			}
			else if (IsNEXDOCSActive)
			{
				return new NEXDOCSJobComInvoiceLineLookups(this);
			}
			else
			{
				return new EXDOCSJobComInvoiceLineLookups(this);
			}
		}

		protected override bool IsLookupsCachedInBase
		{
			get
			{
				return CalculateIsLookupsCached();
			}
		}

		public void ResetIsLookupsCached()
		{
			isLookupsCached = null;
		}

		bool CalculateIsLookupsCached()
		{
			var result = false;
			if (isLookupsCached.HasValue)
			{
				result = isLookupsCached.Value;
			}
			isLookupsCached = true;
			return result;
		}
		bool? isLookupsCached;

		public override ZDecimal JI_CustomsQuantity
		{
			get { return base.JI_CustomsQuantity; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsQuantity))
				{
					bool hasChanged = base.JI_CustomsQuantity != value;
					base.JI_CustomsQuantity = value;
					if (!IsCopying)
					{
						CalculateLitresFromLitresOfAlchohol();
						var declaration = Declaration;
						if (declaration != null)
						{
							if (declaration.IsQuarantine && hasChanged)
							{
								QuarantineExDocLine.QL_NetQuantity = value;
							}

							if (declaration.IsDrawback && hasChanged)
							{
								CalculateClaimAmount();
							}
						}

						AddInfo.Validation.ValidateZA_WRQ();
					}
				}
			}
		}

		public override ZString JI_CustomsUnitQty
		{
			get { return base.JI_CustomsUnitQty; }
			set
			{
				if (base.JI_CustomsUnitQty != value)
				{
					base.JI_CustomsUnitQty = value;
					if (!IsCopying)
					{
						AddInfo.ZA_UQ2 = OldValidation.SecondUQ;

						if (!AddInfo.ZA_UQ2.IsEmpty)
						{
							if (CanConvertFromNetWeightToCustomsUnit(AddInfo.ZA_UQ2))
							{
								CustomsQuantity2Converter.CalculateFromNetWeightToCustomsQty();
							}
							else
							{
								CustomsQuantity2Converter.CalculateCustomsFactorAndQty();
							}
						}

						if (IsQuarantine)
						{
							var quaratineLineQtyUnit = new CustomsQtyUnitToRFPQtyUnit().GetDescriptionFromCode(value);
							if (String.IsNullOrEmpty(quaratineLineQtyUnit))
							{
								quaratineLineQtyUnit = value;
							}
							QuarantineExDocLine.QL_NetQuantityUnit = quaratineLineQtyUnit;
						}

						if (AddInfo.ShouldSynchroniseInvoiceQtyAndWRU)
						{
							AddInfo.SynchroniseInvoiceQtyAndWRUIfRequired(AUAddInfo.SyncDirection.FromInvoiceLineToAddInfo);
						}
						else if (((ISupportDataImporting)this).IsImportingData)
						{
							AddInfo.ClearWarehouseRelatedProperties();
						}

						AddInfo.Validation.ValidateZA_WRU();
					}
				}
			}
		}

		public override ZDecimal JI_Weight
		{
			get { return base.JI_Weight; }
			set
			{
				var weight = value.Round(3);    // Data import may have > 3 decimals
				if (JI_Weight != weight)
				{
					base.JI_Weight = weight;
					if (!IsCopying)
					{
						if (IsQuarantine && QuarantineExDocLine.QL_GrossMetricWeight != weight)
						{
							QuarantineExDocLine.QL_GrossMetricWeight = weight;
						}
					}
				}
			}
		}

		public override ZString JI_WeightUQ
		{
			get { return base.JI_WeightUQ; }
			set
			{
				if (JI_WeightUQ != value)
				{
					base.JI_WeightUQ = value;
					if (!IsCopying)
					{
						if (IsQuarantine)
						{
							QuarantineExDocLine.QL_GrossMetricWeightUnit = new CustomsWeightUnitToRFPWeightUnit().GetDescriptionFromCode(value);
						}
					}
				}
			}
		}

		public override bool NeedsCustomsQuantity
		{
			get
			{
				bool result = base.NeedsCustomsQuantity;
				if (Declaration != null && Declaration.IsExport)
				{
					result &= JI_CustomsUnitQty != "NR";
				}
				return result;
			}
		}

		public override ZDecimal JI_InvoiceQuantity
		{
			get { return base.JI_InvoiceQuantity; }
			set
			{
				if (JI_InvoiceQuantity != value)
				{
					value = ZArchitecture.Core.Utilities.Round(value, 5);
					base.JI_InvoiceQuantity = value;

					if (!IsCopying)
					{
						if (IsQuarantine)
						{
							QuarantineExDocLine.QL_OuterPackCount = value.Round(0).ToZInt();
						}
						if (JI_CustomsUnitQty.IsEmpty && (Declaration?.IsDrawback ?? false))
						{
							CalculateClaimAmount();
						}

						if (!((ISupportDataImporting)this).IsImportingData)
						{
							AddInfo.SynchroniseInvoiceQtyAndWRUIfRequired(AUAddInfo.SyncDirection.FromInvoiceLineToAddInfo);
						}
					}
				}
			}
		}

		public override ZString JI_InvoiceUQ
		{
			get { return base.JI_InvoiceUQ; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(nameof(JI_InvoiceUQ)))
				{
					var oldValue = JI_InvoiceUQ;
					base.JI_InvoiceUQ = value;

					if (!IsCopying && oldValue != JI_InvoiceUQ)
					{
						if (IsQuarantine)
						{
							QuarantineExDocLine.QL_OuterPackType = CustomsQtyUnitToRFPPackType.GetRFPPackTypeForCustomsQtyUnit(Declaration?.FinalDestination?.Country.Code, value);
							RefreshBinding();
						}

						AddInfo.SynchroniseInvoiceQtyAndWRUIfRequired(AUAddInfo.SyncDirection.FromInvoiceLineToAddInfo);
					}
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZString JI_Tariff
		{
			get { return base.JI_Tariff; }
			set
			{
				var oldValue = JI_Tariff;
				base.JI_Tariff = value;

				if (!IsCopying && JI_Tariff != oldValue)
				{
					AddInfo.CalculateLitresOfAlcoholFromQT2();
					AddInfo.DefaultTariffRateNumber();
					if (Declaration?.IsDrawback ?? false)
					{
						CalculateClaimAmountIfImputationMethod();
					}
				}
			}
		}

		public override ZString FormatTariffForSaving(ZString unformattedTariff) => ((IAUTariffFormatter)TariffFormatter).FormatDotted(unformattedTariff);

		public override bool ShouldWipeNKTaxType => false;

		protected override bool ShouldSetDescriptionWhenTariffChanges
		{
			get { return base.ShouldSetDescriptionWhenTariffChanges && (Declaration == null || !Declaration.IsQuarantine); }
		}

		protected void CalculateLitresFromLitresOfAlchohol()
		{
			if (JI_CustomsUnitQty == "LA" && AddInfo.ZA_UQ2 == "L" && !AddInfo.ZA_ISS.IsEmpty && JI_CustomsQuantity != 0)
			{
				AddInfo.ZA_QT2 = JI_CustomsQuantity / DutiableInvoiceSpiritStrengthPercentage * 100;
			}
		}

		public new Classification Classification
		{
			get { return (Classification)base.Classification; }
		}

		protected internal ITariffView ExportTariff => GetExportTariff(JI_Tariff);

		protected ITariffView GetExportTariff(ZString tariffCode)
		{
			return Factory.GetCachedValue($"Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine_EXP_{tariffCode}_{EffectiveAssessmentDate}",
				() => AUCAHECCWrapper.Load(Factory, tariffCode, EffectiveAssessmentDate));
		}

		protected internal ITariffView ImportTariff => GetImportTariff(JI_Tariff);

		protected ITariffView GetImportTariff(ZString tariffCode)
		{
			return Factory.GetCachedValue($"Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine_IMP_{tariffCode}_{EffectiveAssessmentDate}",
				() => AUCClassWrapper.Load(Factory, tariffCode, EffectiveAssessmentDate));
		}

		#region UniversalTariff

		protected override bool UseUniversalTariffCore => false;

		protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy()
			=> new TariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>(GetTariffForUOMDefaulting, defaultFirstUnitOnly: IsExport);

		ITariff GetTariffForUOMDefaulting(JobComInvoiceLine invoiceLine)
		{
			return IsCopying ? null : new TariffWrapper(invoiceLine);
		}

		public ITariffView Tariff => IsExport ? ExportTariff : ImportTariff;

		public bool UseCustomsRefData => IsExport ? AUCAHECCWrapper.EnableCWRefForAHECC : AUCClassWrapper.UseCustomsReferenceData;

		public override ZString CustomsUQ
		{
			get
			{
				if (UseCustomsRefData)
				{
					var uq = Tariff?.ZZ1_ZZ8_UQ1.ToUpper() ?? ZString.Empty;
					return (uq == AUConstants.AdditionalUQCodes.ERR || uq == AUConstants.AdditionalUQCodes.NR) ? ZString.Empty : uq;
				}
				else
				{
					return OldValidation.CustomsUQ.ToUpper();
				}
			}
		}

		public ZString SecondUQ
		{
			get
			{
				if (UseCustomsRefData)
				{
					return Tariff?.ZZ1_ZZ8_UQ2.ToUpper() ?? ZString.Empty;
				}
				else
				{
					return OldValidation.SecondUQ.ToUpper();
				}
			}
		}

		#endregion

		public new AUOrgSupplierPart Part
		{
			get { return (AUOrgSupplierPart)base.Part; }
		}

		public new CusClassPartPivot Pivot
		{
			get { return (CusClassPartPivot)base.Pivot; }
		}

		protected override bool GetJI_CustomsQuantityReadOnly()
		{
			var declaration = Declaration;
			if (declaration != null && (declaration.IsDrawback || (declaration.IsExport && !JI_Tariff.IsEmpty && ExportTariff == null)))
			{
				return false;
			}
			else
			{
				return JI_CustomsUnitQty == "NR" || JI_CustomsUnitQty.IsEmpty;
			}
		}

		protected override ZString GetTariffDescription(ZString tariffCode)
		{
			if (IsExport)
			{
				var exportTariff = GetExportTariff(tariffCode);
				return exportTariff?.ZZ1_Description ?? ZString.Empty;
			}
			else
			{
				var importTariff = GetImportTariff(tariffCode);
				return importTariff?.ZZ1_Description ?? ZString.Empty;
			}
		}

		protected void DefaultAddInfoFromPart()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				var pivot = Pivot;
				if (pivot != null)
				{
					if (declaration.IsImport || declaration.IsDrawback)
					{
						AddInfo.LoadPropertiesFromString(pivot.EffectiveAddInfo.ToString(), false);
						AQISDocuments.SplitAndAddAQISElements(AddInfo.ZA_AQISDocuments_Hidden);
						AQISPremisesIdAndProcessingTypes.SplitAndAddAQISElements(AddInfo.ZA_AQISPremIdProcessType_Hidden);
						if (declaration.IsImport)
						{
							CopyICSPermitsIfNeeded(pivot);
						}
					}
					else if (declaration.IsQuarantine)
					{
						pivot.ReloadSafe();
						pivot.AddInfo.LoadPropertiesFromString(pivot.CI_AddInfo, true);
						QuarantineExDocLine.QL_ProductType = pivot.AddInfo.ZA_AQISProduct_Hidden;
						QuarantineExDocLine.QL_SupplimentaryCode = pivot.AddInfo.ZA_AQISSupplementaryCode_Hidden;
						QuarantineExDocLine.QL_PackType = pivot.AddInfo.ZA_AQISPackType_Hidden;
						QuarantineExDocLine.QL_PreservationType = pivot.AddInfo.ZA_AQISPreservation_Hidden;
						QuarantineExDocLine.QL_CutCode = pivot.AddInfo.ZA_AQISCutCode_Hidden;
						QuarantineExDocLine.QL_Category = pivot.AddInfo.ZA_AQISCategoryCode_Hidden;
						RefreshBinding();
					}
				}
			}
		}

		void CopyICSPermitsIfNeeded(CusClassPartPivot pivot)
		{
			var permits = ICSPermits;
			if (permits.Count > 0)
			{
				permits.RemoveAndDeleteAll();
			}
			foreach (var permit in pivot.ICSPermits)
			{
				var clonedPermit = (ICSPermit)permit.Clone();
				permits.Add(clonedPermit);
			}
		}

		protected internal void SetJI_CountryOfOrigin()
		{
			if (AddInfo.ZA_ORG.Length > 2)
			{
				RefCountry addInfoCountry = AddInfo.CountryOfOrigin;

				if (addInfoCountry != null)
				{
					base.JI_CountryOfOrigin = addInfoCountry.RN_Code;
				}
			}
			else
			{
				base.JI_CountryOfOrigin = AddInfo.ZA_ORG.Left(2);//check if statement
			}
		}

		protected void SetJI_ConcessionOrder()
		{
			SetInstrumentTypeToConcessionOrder();
			SetInstrumentCodeToConcessionOrder();
		}

		protected void SetAddInfoORG()
		{
			AddInfo.ZA_ORG = JI_CountryOfOrigin;
		}

		protected override ZDecimal ProportionOfCusEntryLine()
		{
			ZDecimal result = 0m;
			if (CusEntryLine != null)
			{
				CusEntryHeader entryHeader = CusEntryLine.Header;
				if (entryHeader != null)
				{
					if (entryHeader.ShouldEntryBeNormalised)
					{
						if (CusEntryLine.CL_CustomsValue != 0)
						{
							Money priceAdjustmentInLocalCurrency = CurrencyConverter.ConvertExact(JI_PriceAdjustment, JobDeclaration.GetLocalCurrency());
							result = (JI_Calc_FOB_InLocalCurrency + priceAdjustmentInLocalCurrency.Amount) / CusEntryLine.CL_CustomsValue;
						}
					}
					else
					{
						Money mergedPriceIncludingAdjustment = CusEntryLine.PriceIncludingAdjustment;
						if (CusEntryLine != null && mergedPriceIncludingAdjustment.Amount != 0m)
						{
							result = (LinePriceIncludingAdjustment / mergedPriceIncludingAdjustment.Amount).Amount;
						}
					}
				}
			}
			return result;
		}

		void UpdateLineDrawbackDefaultValues()
		{
			AddInfo.ZA_DAM_Hidden = InvoiceHeader.AddInfo.ZA_DAM_Hidden;
			AddInfo.ZA_EDN_Hidden = InvoiceHeader.AddInfo.ZA_EDN_Hidden;
		}

		#region Parent/Trailer
		protected void FreeUpChildLineIfNecessary()
		{
			JobComInvoiceLine childLine = this.ChildLine;
			if (childLine != null)
			{
				childLine.JI_ParentLine = (short)0;
				childLine.AddInfo.ZA_RelatedLinePK_Hidden = string.Empty;
				childLine.AddInfo.ZA_LinePrefix_Hidden = string.Empty;
			}
			AddInfo.ZA_LinePrefix_Hidden = string.Empty;
			AddInfo.ZA_RelatedLinePK_Hidden = string.Empty;
		}

		public JobComInvoiceLine ChildLine
		{
			get
			{
				var result = Factory.Load<JobComInvoiceLine>(ChildPK);

				return result != null && result.JI_LinePrefix == JobComInvoiceLine.LinePrefixString.Trailer ? result : null;
			}
		}

		protected ZGuid ChildPK
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (JI_LinePrefix == JobComInvoiceLine.LinePrefixString.Parent
					&& !AddInfo.ZA_RelatedLinePK_Hidden.IsEmpty)
				{
					result = new Guid(AddInfo.ZA_RelatedLinePK_Hidden.ToString());
				}
				return result;
			}
		}

		public JobComInvoiceLine ParentLine
		{
			get
			{
				JobComInvoiceLine result = null;

				if (Declaration != null && !Declaration.IsImportCMR)
				{
					result = (JobComInvoiceLine)Factory.Load(typeof(JobComInvoiceLine), ParentPK);
					result = result != null && result.JI_LinePrefix == JobComInvoiceLine.LinePrefixString.Parent ? result : null;
				}

				return result;
			}
		}

		ZGuid ParentPK
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (JI_LinePrefix == JobComInvoiceLine.LinePrefixString.Trailer
					&& !AddInfo.ZA_RelatedLinePK_Hidden.IsEmpty)
				{
					result = new Guid(AddInfo.ZA_RelatedLinePK_Hidden.ToString());
				}
				return result;
			}
		}

		protected void ChangeChildLineIfNecessary()
		{
			if (ChildLine != null)
			{
				ChildLine.JI_Calc_Invoice = this.JI_Calc_Invoice;
			}
		}

		#endregion

		#endregion

		#region IDutyData Members

		/// <summary>
		/// Approximate calculation ONLY
		/// </summary>
		public ZDecimal CustomsFactor
		{
			get
			{
				return 1;
			}
		}

		public ZDateTime EffectiveDutyDate
		{
			get
			{
				var result = ZDateTime.Invalid;
				var invoiceHeader = InvoiceHeader;
				if (invoiceHeader != null)
				{
					result = InvoiceHeader.EffectiveDutyDate;
				}
				if (!result.IsValid)
				{
					result = ZDateTime.Today;
				}
				return result;
			}
		}

		public ZString TariffNumber
		{
			get { return JI_Tariff.Split(' ')[0].Replace(".", ""); }
		}

		public ZString StatCode
		{
			get { return JI_Tariff.Contains(' ') ? JI_Tariff.Split(' ')[1] : ZString.Empty; }
		}

		public ZString TreatmentCode
		{
			get { return AddInfo.ZA_TreatmentCode_Hidden; }
		}

		public ZDecimal Quantity
		{
			get { return JI_InvoiceQuantity; }
		}

		public ZString UnitOfQuantity
		{
			get { return JI_InvoiceUQ; }
		}

		public ZBool IsNature20
		{
			get { return JI_IsPackToBondForLine; }
		}

		/// <summary>
		/// Approximate calculation ONLY
		/// </summary>
		public Money Price
		{
			get { return InvoiceHeader == null ? Money.Empty : new Money(JI_LinePrice, InvoiceHeader.Invoice_Currency); }
		}

		public Money CustomsValue
		{
			get
			{
				Money result = new Money(JI_Calc_FOB_InLocalCurrency, JobDeclaration.GetLocalCurrency());
				if (JI_PriceAdjustment.IsValid)
				{
					result = CurrencyConverter.Add(result, JI_PriceAdjustment);
				}
				return result;
			}
		}
		#endregion

		#region ISecondCustomsQuantity Members

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_QT2)]
		public ZDecimal SecondCustomsQty
		{
			get { return AddInfo.ZA_QT2; }
			set { AddInfo.ZA_QT2 = value; }
		}

		public ZPropertyInfo SecondCustomsQtyInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SecondCustomsQty), x => AddInfo.ZA_QT2Info); }
		}

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_UQ2)]
		public ZString SecondCustomsUQ
		{
			get { return AddInfo.ZA_UQ2; }
		}

		public ZPropertyInfo SecondCustomsUQInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SecondCustomsUQ), x => AddInfo.ZA_UQ2Info); }
		}

		#endregion

		#region IUltimateDistributee Members

		// TODO: Will be replaced with GenericLandedCostingConfig
		DutyTaxEntryFee IUltimateDistributee.LineDutyTaxEntryFeeItems
		{
			get
			{
				DutyTaxEntryFee result = new DutyTaxEntryFee();
				result[CustomsDisbursementChargeCode.TotalDuty] = JI_Calc_DutyAmountIncludingWHEstimate;

				ZDecimal aQISServiceFee = JI_Calc_AQISServiceAmount;
				result[CustomsDisbursementChargeCode.EntryFees] = JI_Calc_AllEntryFees - aQISServiceFee;
				result[CustomsDisbursementChargeCode.QuarantineFees] = aQISServiceFee;

				result[CustomsDisbursementChargeCode.OtherOrFlatDutyAmount] = JI_Calc_FlatDutyPortion + JI_Calc_AllOtherDuties;
				result[CustomsDisbursementChargeCode.SpecialTax1] = JI_Calc_WETAmountIncludingWHEstimate;
				result[CustomsDisbursementChargeCode.SpecialTax2] = JI_Calc_LCTAmountIncludingWHEstimate;
				result[CustomsDisbursementChargeCode.SpecialTax3] = JI_Calc_WoodLevyIncludingWHEstimate;
				return result;
			}
		}

		ZDecimal IUltimateDistributee.CostInLocalCurrency
		{
			get
			{
				ZDecimal result = 0m;

				if (InvoiceHeader != null)
				{
					if (InvoiceHeader.AddInfo.ZA_IncADJ_Hidden)
					{
						Money added = LCCurrencyConverter.Add(JI_LinePriceMoney, JI_PriceAdjustment);
						result = LCCurrencyConverter.ConvertExact(added, JobDeclaration.GetLocalCurrency()).Amount;
					}
					else
					{
						result = CostInLocalCurrencyForLC;
					}
				}

				return result;
			}
		}

		ZDecimal IUltimateDistributee.LinePriceInInvoiceCurrency
		{
			get
			{
				ZDecimal result = 0m;

				if (InvoiceHeader != null && InvoiceHeader.Invoice_Currency != null && InvoiceHeader.AddInfo.ZA_IncADJ_Hidden)
				{
					Money added = LCCurrencyConverter.Add(JI_LinePriceMoney, JI_PriceAdjustment);
					result = LCCurrencyConverter.ConvertExact(added, InvoiceHeader.Invoice_Currency).Amount;
				}
				else
				{
					result = JI_LinePrice;
				}

				return result;
			}
		}

		#endregion

		#region ITariffNumberProvider

		ZString ITariffNumberProvider.TariffAndStatNumber
		{
			get { return JI_Tariff; }
		}

		#endregion

		[ChildEditable(true)]
		public ICSPermitCollection ICSPermits
		{
			get
			{
				if (icsPermits == null)
				{
					icsPermits = new ICSPermitCollection(this);
					icsPermits.Load();
					RegisterEditableChildObject(icsPermits);
				}
				return icsPermits;
			}
		}
		ICSPermitCollection icsPermits;

		#region Quarantine

		public QuarantineExDocLine QuarantineExDocLine
		{
			get
			{
				if ((fQuarantineExDocLine == null || fQuarantineExDocLine.IsDeleted) && IsQuarantine)
				{
					fQuarantineExDocLine = LoadQuarantineExDocLine();
					if (fQuarantineExDocLine == null)
					{
						fQuarantineExDocLine = Factory.New<QuarantineExDocLine>();
						using (fQuarantineExDocLine.SuspendSettingHasChanges())
						{
							fQuarantineExDocLine.QL_JI = PK;
						}
					}
					RegisterEditableChildObject(fQuarantineExDocLine);
				}
				return fQuarantineExDocLine;
			}
		}
		QuarantineExDocLine fQuarantineExDocLine;

		QuarantineExDocLine LoadQuarantineExDocLine() => Factory.LoadTop1<QuarantineExDocLine>(new ZQuery(QuarantineExDocLineSchema.QL_JI, PK) { FetchOnlyFromLocalCache = !IsInDatabase });

		internal void ResetQuarantineExDocLineCache()
		{
			if (fQuarantineExDocLine != null)
			{
				UnRegisterEditableChildObject(fQuarantineExDocLine);
				fQuarantineExDocLine = null;
			}
		}

		public ZString EXDOCAMLCPerformanceNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (Supplier_Effective != null)
				{
					result = Supplier_Effective.CustomsCodes.GetCustomsRegNo(OrgCusCode.AUQuarantineCodeTypes.EXDOCAMLCPerformanceExporterNumber, Factory.Load<RefCountry>(Core.Constants.CountryGuids.Australia));
				}
				return result;
			}
		}

		#region JI_Description

		public override ZString JI_Description
		{
			get { return base.JI_Description; }
			set
			{
				bool hasChanged = base.JI_Description != value;
				base.JI_Description = value;
				if (hasChanged)
				{
					if (!IsCopying && IsQuarantine)
					{
						QuarantineExDocLine.MarkAsNeedingValidation();
					}
				}
			}
		}

		public void CopyDescriptionToQuarantineLine()
		{
			if (IsQuarantine)
			{
				QuarantineExDocLine.QL_MeatInspectionDescription = JI_Description.Left(QuarantineExDocLine.Schema.QL_MeatInspectionDescriptionMaxLength);
				RefreshBinding();
			}
		}

		#endregion

		public bool JI_LineNo_ReadOnly => IsQuarantine;
		public bool JI_Calc_Invoice_ReadOnly => IsQuarantine;

		public override void Delete()
		{
			EDocPivotCollection.RemoveAndDeleteAll();
			base.Delete();
			ResetQuarantineExDocLineCache();
			this.DeleteChildren<QuarantineExDocLine>(QuarantineExDocLineSchema.QL_JI);
		}

		#endregion

		#region CP Dec Questions
		public CMRCusEntryCPDec[] QuestionsWithEffectiveDefaultAnswers
		{
			get
			{
				List<CMRCusEntryCPDec> result = new List<CMRCusEntryCPDec>();
				if (Classification != null)
				{
					result.AddRange(Classification.Questions.ToArray<CMRCusEntryCPDec>());
				}
				foreach (CMRCusEntryCPDec question in result)
				{
					question.EffectiveDefaultAnswer = question.ON_AnswerCode;
				}
				if (Pivot != null)
				{
					foreach (CMRCusEntryCPDec question in Pivot.Questions)
					{
						if (question.IsAnswered)
						{
							CMRCusEntryCPDec existingQuestion = result.Find(x => x.ON_CPDecNum == question.ON_CPDecNum);
							if (existingQuestion != null)
							{
								if (existingQuestion.EffectiveDefaultAnswer != question.ON_AnswerCode && !existingQuestion.EffectiveDefaultAnswer.IsEmpty && !question.ON_AnswerCode.IsEmpty)
								{
									existingQuestion.EffectiveDefaultAnswer = CMRCusEntryCPDec.Answers.Ambiguous;
									break;
								}
								else
								{
									existingQuestion.EffectiveDefaultAnswer = question.ON_AnswerCode;
								}
							}
							else
							{
								question.EffectiveDefaultAnswer = question.ON_AnswerCode;
								result.Add(question);
							}
						}
					}
				}
				foreach (CMRCusEntryCPDec question in result.ToArray())
				{
					if (question.EffectiveDefaultAnswer.IsEmpty)
					{
						result.Remove(question);
					}
				}
				return result.ToArray();
			}
		}

		#endregion

		#region IAddInfoManager Members

		Customs.Business.IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		#region ICusCodeDataTypeSupporter

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ CusCodeDataTypeList.Codes.ICSPermit, typeof(ICSPermit) }
			};
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.AURFPNumber, typeof(RFPNumber));
			return result;
		}

		#endregion

		#region ICusStorageDocPivotParent

		[ChildEditable(true)]
		public CusStorageDocPivotCollection EDocPivotCollection
		{
			get
			{
				if (eDocPivotCollection == null)
				{
					eDocPivotCollection = new CusStorageDocPivotCollection(this);
					eDocPivotCollection.Load();
					RegisterEditableChildObject(eDocPivotCollection);
				}

				return eDocPivotCollection;
			}
		}
		CusStorageDocPivotCollection eDocPivotCollection;

		bool IsEDocPivotCollectionLoaded => eDocPivotCollection != null && eDocPivotCollection.IsLoaded;

		CusStorageDocPivotCollection ICusStorageDocPivotParent.EDocPivotCollection => EDocPivotCollection;

		IEnumerable<IStorageDocsBaseCollection> ICusStorageDocPivotTypeSupporter.EDocCollections
		{
			get => EDocsHelper.GetEDocCollections(CusEntryLine?.Header, Declaration, Declaration?.Shipment);
		}

		Type ICusStorageDocPivotTypeSupporter.CusStorageDocPivotType => typeof(CusStorageDocPivot);

		void ICusStorageDocPivotTypeSupporter.ReloadCollection()
		{
			if (IsEDocPivotCollectionLoaded)
			{
				EDocPivotCollection.Reload(true);
			}
		}

		ZString ICusStorageDocPivotTypeSupporter.HumanReadableName => InvoiceAndLineReference;

		#endregion
	}
}
