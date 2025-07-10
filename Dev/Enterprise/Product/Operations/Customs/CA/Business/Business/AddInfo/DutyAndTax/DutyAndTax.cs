//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Data;
	using System.Linq;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Common.CA;
	using Enterprise.Customs.Universal;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;

	public class DutyAndTax : AutoDutyAndTax, ISynchroniserReadOnlyMembersProvider, IDisposable, IDutyAndTaxDataForCalculation, Integration.Customs.CA.IDutyAndTax
	{
		#region Schema

		public new class Schema : AutoDutyAndTax.Schema
		{
			public const string AmountDescription = "AmountDescription";
			public const int AmountDescriptionMaxLength = 10000;
			public const string Description = "Description";
			public const int DescriptionMaxLength = 256;
			public const string Quantity = "Quantity";
			public const string ForeignCurrencyExchangeRate = "ForeignCurrencyExchangeRate";
			public const string NormalValueCurrencyExchangeRate = "NormalValueCurrencyExchangeRate";
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public DutyAndTax(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new IDutyAndTaxData Parent { get; internal set; }
		public ZBool IsInRefFiles { get; set; }
		public bool IsOldRow { get; set; }
		public bool IsEXDDuty => C1_DutyType == DutyAndTaxManager.CombinedDuty.Excise;

		#region New Flags

		public ZBool IsDutyOnProductOrCusClassification
		{
			get { return Parent is CusClassPartPivot || Parent is CusClassification; }
		}

		#endregion

		#region C1_Code

		[ReadOnlyMember(nameof(C1_Code_ReadOnly))]
		[ResourceStringData("CADutyAndTaxAddInfo|C1_Code", Caption = "Code")]
		public override ZString C1_Code
		{
			get { return base.C1_Code; }
			set
			{
				var oldValue = base.C1_Code;
				base.C1_Code = value;
				if (oldValue != C1_Code && !IsCopying)
				{
					if (C1_TaxType == DutyAndTaxTypes.Codes.SUR)
					{
						var classificationNumber = Parent?.ClassificationNumber ?? ZString.Empty;
						var effectiveDutyDate = Parent?.EffectiveDutyDate ?? ZDateTime.Today;
						var countryOfOrigin = Parent?.CountryOfOrigin ?? ZString.Empty;
						if (!classificationNumber.IsEmpty && !countryOfOrigin.IsEmpty)
						{
							var tariffView = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Canada, Constants.TariffTypes.HarmonizedSystem, classificationNumber, effectiveDutyDate);
							var rateViews = tariffView.Rates.Where(d => d.ZZ2_StartDate <= effectiveDutyDate && d.ZZ2_EndDate >= effectiveDutyDate && d.RateCode == DutyAndTaxTypes.Codes.SUR);
							var rateView = rateViews.FirstOrDefault(x => x.FilteredRateApplicabilities.Where(x => x.IsApplicable(countryOfOrigin, effectiveDutyDate)).Select(y => y.ZZT_AdditionalCode).Contains(C1_Code));
							if (rateView != null)
							{
								var rateAndUnit = rateView.ZZ2_RateFormula.KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.*").Split('*').OrderBy(x => x).ToArray();
								if (rateAndUnit.Length == 2)
								{
									var rate = ZDecimal.ParseSafe(rateAndUnit[0], 0);
									var rateType = ZString.Empty;
									if (rateAndUnit[1] == DutyAndTaxManager.VFDInFormula)
									{
										rate = rate * 100;
										rateType = RateTypes.Codes.AdValorem;
									}
									else
									{
										rateType = RateTypes.Codes.Specific;
									}

									C1_Rate = rate;
									C1_RateType = rateType;
								}
								else if (rateAndUnit.Length == 1)
								{
									C1_Rate = ZDecimal.ParseSafe(rateAndUnit[0], 0);

									if (C1_Rate == 0m)
									{
										C1_Override = true;
										C1_RateType = RateTypes.Codes.AcceptT;
										C1_Amount = ZDecimal.Zero;
									}
								}
							}
						}
					}
					UpdateRateTypeForCIGARS();
				}
				if (InvoiceLine != null)
				{
					switch (C1_TaxType)
					{
						case DutyAndTaxTypes.Codes.ADD:
							InvoiceLine.CA_ADD_CodeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CPT:
							InvoiceLine.CA_CPT_CodeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CTA:
							InvoiceLine.CA_CTA_CodeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CustomsDuty:
							switch (C1_DutyType)
							{
								case DutyAndTaxManager.CombinedDuty.Excise:
									InvoiceLine.CA_EXCDTY_CodeInfo.RefreshBinding();
									break;
								case DutyAndTaxManager.CombinedDuty.Classification:
									InvoiceLine.CA_CLSDTY_CodeInfo.RefreshBinding();
									break;
							}
							break;
						case DutyAndTaxTypes.Codes.CVD:
							InvoiceLine.CA_CVD_CodeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.ExciseTax:
							InvoiceLine.CA_EXS_CodeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.GST:
							InvoiceLine.CA_GST_CodeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.SAF:
							InvoiceLine.CA_SAF_CodeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.SUR:
							InvoiceLine.CA_SUR_CodeInfo.RefreshBinding();
							break;
					}
				}
			}
		}

		public bool C1_Code_ReadOnly
		{
			get
			{
				return (!(IsTax || C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty) || !C1_Override)
					&& (C1_TaxType != DutyAndTaxTypes.Codes.ADD
					&& C1_TaxType != DutyAndTaxTypes.Codes.CVD
					&& C1_TaxType != DutyAndTaxTypes.Codes.SUR
					&& C1_TaxType != DutyAndTaxTypes.Codes.ExciseTax
					&& C1_TaxType != DutyAndTaxTypes.Codes.SAF);
			}
		}

		#endregion

		#region C1_ExemptCode

		[ReadOnlyMember(nameof(C1_ExemptCode_ReadOnly))]
		[ResourceStringData("CADutyAndTaxAddInfo|C1_ExemptCode", Caption = "Exempt/Code", ShortCaption = "Exempt")]
		public override ZString C1_ExemptCode
		{
			get { return base.C1_ExemptCode; }
			set
			{
				var oldValue = C1_ExemptCode;
				base.C1_ExemptCode = value;
				if (!IsCopying && oldValue != C1_ExemptCode)
				{
					ZBool? isAmountNotRequiredForSIMA = null;
					if (!C1_ExemptCode.IsEmpty)
					{
						var clearAmount = !C1_Override && IsTax;
						if (!clearAmount)
						{
							isAmountNotRequiredForSIMA = IsAmountNotRequiredForSIMA;
							clearAmount = isAmountNotRequiredForSIMA.Value;
						}

						if (clearAmount)
						{
							C1_Amount = ZDecimal.Zero;
						}
					}

					if (Parent != null && Parent is IDutyAndTaxData parent && DutyAndTaxTypes.IsSIMATaxCode(C1_TaxType))
					{
						if (!parent.IsSettingSIMAExemptCodeInProgress && C1_TaxType != DutyAndTaxTypes.Codes.SUR)
						{
							try
							{
								parent.IsSettingSIMAExemptCodeInProgress = true;
								foreach (var simaDuty in parent.SIMADuties)
								{
									if (simaDuty != this && simaDuty.C1_ExemptCode != C1_ExemptCode && simaDuty.C1_TaxType != DutyAndTaxTypes.Codes.SUR)
									{
										simaDuty.C1_ExemptCode = C1_ExemptCode;
									}
								}
							}
							finally
							{
								parent.IsSettingSIMAExemptCodeInProgress = false;
							}
						}

						if (!(isAmountNotRequiredForSIMA ?? IsAmountNotRequiredForSIMA))
						{
							DutyAndTaxAmountCalculator.CalculateDutyAndTaxAmount(this, parent.CustomsValue);
						}
					}
				}
				if (InvoiceLine != null)
				{
					switch (C1_TaxType)
					{
						case DutyAndTaxTypes.Codes.ADD:
							InvoiceLine.CA_ADD_ExemptCodeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CPT:
							InvoiceLine.CA_CPT_ExemptCodeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CTA:
							InvoiceLine.CA_CTA_ExemptCodeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CustomsDuty:
							switch (C1_DutyType)
							{
								case DutyAndTaxManager.CombinedDuty.Excise:
									InvoiceLine.CA_EXCDTY_ExemptCodeInfo.RefreshBinding();
									break;
								case DutyAndTaxManager.CombinedDuty.Classification:
									InvoiceLine.CA_CLSDTY_ExemptCodeInfo.RefreshBinding();
									break;
							}
							break;
						case DutyAndTaxTypes.Codes.CVD:
							InvoiceLine.CA_CVD_ExemptCodeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.ExciseTax:
							InvoiceLine.CA_EXS_ExemptCodeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.GST:
							InvoiceLine.CA_GST_ExemptCodeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.SAF:
							InvoiceLine.CA_SAF_ExemptCodeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.SUR:
							InvoiceLine.CA_SUR_ExemptCodeInfo.RefreshBinding();
							break;
					}
				}
			}
		}

		public bool C1_ExemptCode_ReadOnly
		{
			get { return C1_TaxType.IsEmpty || C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty || C1_TaxType == DutyAndTaxTypes.Codes.CTA; }
		}

		public bool IsTax
		{
			get { return IsExciseTax || IsGST; }
		}

		public bool IsGST => C1_TaxType == DutyAndTaxTypes.Codes.GST;

		public bool IsExciseTax => C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax;

		public bool IsSurtax => C1_TaxType == DutyAndTaxTypes.Codes.SUR;

		#endregion

		#region C1_Override

		[ResourceStringData("CADutyAndTaxAddInfo|C1_Override", Caption = "Override", ShortCaption = "Ovr.")]
		public override ZBool C1_Override
		{
			get { return base.C1_Override; }
			set
			{
				var oldValue = C1_Override;
				base.C1_Override = value;
				if (C1_Override != oldValue && !IsCopying)
				{
					if (oldValue && !C1_Override && DutyAndTaxTypes.IsSIMATaxCode(C1_TaxType))
					{
						DutyAndTaxManager.ResetSIMADutiesWhenNotOverridden(Factory, this, Parent);
					}
					else if (C1_Override)
					{
						C1_OriginalAmount = C1_Amount;
						EnableAndSynchronise();
						UpdateRateTypeForCIGARS();
					}
				}
				if (InvoiceLine != null)
				{
					switch (C1_TaxType)
					{
						case DutyAndTaxTypes.Codes.ADD:
							InvoiceLine.CA_ADD_OverrideInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CPT:
							InvoiceLine.CA_CPT_OverrideInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CTA:
							InvoiceLine.CA_CTA_OverrideInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CustomsDuty:
							switch (C1_DutyType)
							{
								case DutyAndTaxManager.CombinedDuty.Excise:
									InvoiceLine.CA_EXCDTY_OverrideInfo.RefreshBinding();
									break;
								case DutyAndTaxManager.CombinedDuty.Classification:
									InvoiceLine.CA_CLSDTY_OverrideInfo.RefreshBinding();
									break;
							}
							break;
						case DutyAndTaxTypes.Codes.CVD:
							InvoiceLine.CA_CVD_OverrideInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.ExciseTax:
							InvoiceLine.CA_EXS_OverrideInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.GST:
							InvoiceLine.CA_GST_OverrideInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.SAF:
							InvoiceLine.CA_SAF_OverrideInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.SUR:
							InvoiceLine.CA_SUR_OverrideInfo.RefreshBinding();
							break;
					}
				}
			}
		}

		#endregion

		#region C1_RateType

		[ReadOnlyMember(nameof(SIMADutyReadOnly))]
		[ResourceStringData("CADutyAndTaxAddInfo|C1_RateType", Caption = "Rate Type", ShortCaption = "Type")]
		public override ZString C1_RateType
		{
			get { return base.C1_RateType; }
			set
			{
				var oldValue = C1_RateType;
				base.C1_RateType = value;
				if (!IsCopying && oldValue != C1_RateType)
				{
					if (C1_RateType == RateTypes.Codes.Exempt || C1_RateType == RateTypes.Codes.Free)
					{
						C1_Amount = ZDecimal.Zero;
					}
				}
				if (InvoiceLine != null)
				{
					switch (C1_TaxType)
					{
						case DutyAndTaxTypes.Codes.ADD:
							InvoiceLine.CA_ADD_RateTypeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CPT:
							InvoiceLine.CA_CPT_RateTypeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CTA:
							InvoiceLine.CA_CTA_RateTypeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CustomsDuty:
							switch (C1_DutyType)
							{
								case DutyAndTaxManager.CombinedDuty.Excise:
									InvoiceLine.CA_EXCDTY_RateTypeInfo.RefreshBinding();
									break;
								case DutyAndTaxManager.CombinedDuty.Classification:
									InvoiceLine.CA_CLSDTY_RateTypeInfo.RefreshBinding();
									break;
							}
							break;
						case DutyAndTaxTypes.Codes.CVD:
							InvoiceLine.CA_CVD_RateTypeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.ExciseTax:
							InvoiceLine.CA_EXS_RateTypeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.GST:
							InvoiceLine.CA_GST_RateTypeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.SAF:
							InvoiceLine.CA_SAF_RateTypeInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.SUR:
							InvoiceLine.CA_SUR_RateTypeInfo.RefreshBinding();
							break;
					}
				}
			}
		}

		#endregion

		#region C1_Amount

		[ReadOnlyMember(nameof(NotOverridenReadOnly))]
		[ResourceStringData("CADutyAndTaxAddInfo|C1_Amount", Caption = "Amount")]
		public override ZDecimal C1_Amount
		{
			get { return base.C1_Amount; }
			set
			{
				base.C1_Amount = value;
				if (C1_Override)
				{
					C1_OriginalAmount = C1_Amount;
				}
				if (InvoiceLine != null)
				{
					switch (C1_TaxType)
					{
						case DutyAndTaxTypes.Codes.ADD:
							InvoiceLine.CA_ADD_AmountInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CPT:
							InvoiceLine.CA_CPT_AmountInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CTA:
							InvoiceLine.CA_CTA_AmountInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CustomsDuty:
							switch (C1_DutyType)
							{
								case DutyAndTaxManager.CombinedDuty.Excise:
									InvoiceLine.CA_EXCDTY_AmountInfo.RefreshBinding();
									break;
								case DutyAndTaxManager.CombinedDuty.Classification:
									InvoiceLine.CA_CLSDTY_AmountInfo.RefreshBinding();
									break;
							}
							break;
						case DutyAndTaxTypes.Codes.CVD:
							InvoiceLine.CA_CVD_AmountInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.ExciseTax:
							InvoiceLine.CA_EXS_AmountInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.GST:
							InvoiceLine.CA_GST_AmountInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.SAF:
							InvoiceLine.CA_SAF_AmountInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.SUR:
							InvoiceLine.CA_SUR_AmountInfo.RefreshBinding();
							break;
					}
				}
			}
		}

		public ZBool IsAmountNotRequiredForSIMA
		{
			get { return DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(C1_TaxType) && C1_ExemptCode.EndsWith("0", StringComparison.OrdinalIgnoreCase); }
		}

		#endregion

		#region C1_OriginalAmount

		[ResourceStringData("CADutyAndTaxAddInfo|C1_OriginalAmount", Caption = "Original Amount")]
		[ReadOnly(true)]
		public override ZDecimal C1_OriginalAmount
		{
			get { return base.C1_OriginalAmount; }
			set { base.C1_OriginalAmount = value; }
		}

		#endregion

		#region C1_PreviousTranLine

		[ReadOnlyMember(nameof(CustomsDutyReadOnly))]
		[ResourceStringData("CADutyAndTaxAddInfo|C1_PreviousTranLine", Caption = "Previous Transaction Line", ShortCaption = "PTLN", MediumCaption = "Prev. Tran. Line", FullDescription = "Previous transaction line number.")]
		public override ZInt C1_PreviousTranLine
		{
			get { return base.C1_PreviousTranLine; }
			set { base.C1_PreviousTranLine = value; }
		}

		#endregion

		#region C1_PreviousTranNumber

		[ReadOnlyMember(nameof(CustomsDutyReadOnly))]
		[ResourceStringData("CADutyAndTaxAddInfo|C1_PreviousTranNumber", Caption = "Previous Transaction Number", ShortCaption = "Prev. Tran. #", MediumCaption = "Previous Tran. #")]
		public override ZString C1_PreviousTranNumber
		{
			get { return base.C1_PreviousTranNumber; }
			set { base.C1_PreviousTranNumber = value; }
		}

		#endregion

		#region C1_Rate

		[ReadOnlyMember(nameof(SIMADutyReadOnly))]
		[ResourceStringData("CADutyAndTaxAddInfo|C1_Rate", Caption = "Rate")]
		public override ZDecimal C1_Rate
		{
			get { return base.C1_Rate; }
			set
			{
				var oldValue = C1_Rate;
				base.C1_Rate = value;
				if (oldValue != C1_Rate && !IsCopying && C1_Override && Parent != null)
				{
					DutyAndTaxAmountCalculator.CalculateDutyAndTaxAmount(this, Parent.CustomsValue);
				}
				if (InvoiceLine != null)
				{
					switch (C1_TaxType)
					{
						case DutyAndTaxTypes.Codes.ADD:
							InvoiceLine.CA_ADD_RateInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CPT:
							InvoiceLine.CA_CPT_RateInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CTA:
							InvoiceLine.CA_CTA_RateInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.CustomsDuty:
							switch (C1_DutyType)
							{
								case DutyAndTaxManager.CombinedDuty.Excise:
									InvoiceLine.CA_EXCDTY_RateInfo.RefreshBinding();
									break;
								case DutyAndTaxManager.CombinedDuty.Classification:
									InvoiceLine.CA_CLSDTY_RateInfo.RefreshBinding();
									break;
							}
							break;
						case DutyAndTaxTypes.Codes.CVD:
							InvoiceLine.CA_CVD_RateInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.ExciseTax:
							InvoiceLine.CA_EXS_RateInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.GST:
							InvoiceLine.CA_GST_RateInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.SAF:
							InvoiceLine.CA_SAF_RateInfo.RefreshBinding();
							break;
						case DutyAndTaxTypes.Codes.SUR:
							InvoiceLine.CA_SUR_RateInfo.RefreshBinding();
							break;
					}
				}
			}
		}

		#endregion

		#region C1_TaxType

		[ReadOnlyMember(nameof(NotOverridenReadOnly))]
		[ResourceStringData("CADutyAndTaxAddInfo|C1_TaxType", Caption = "Type")]
		public override ZString C1_TaxType
		{
			get { return base.C1_TaxType; }
			set
			{
				var hasChanges = base.C1_TaxType != value;
				base.C1_TaxType = value;
				if (hasChanges)
				{
					if (DutyAndTaxTypes.IsSIMATaxCode(C1_TaxType) && C1_ExemptCode.IsEmpty && Parent != null)
					{
						var simaDutyHasExemptCode = Parent.SIMADuties.FirstOrDefault(x => x != this && !x.C1_ExemptCode.IsEmpty);
						if (simaDutyHasExemptCode != null)
						{
							C1_ExemptCode = simaDutyHasExemptCode.C1_ExemptCode;
						}
					}

					if (C1_TaxType != DutyAndTaxTypes.Codes.CustomsDuty)
					{
						C1_PreviousTranLine = ZInt.Zero;
						C1_PreviousTranNumber = ZString.Empty;
					}
				}
			}
		}

		#endregion

		#region C1_UnitOfMeasure

		[ReadOnlyMember(nameof(SIMADutyReadOnly))]
		[ResourceStringData("CADutyAndTaxAddInfo|C1_UnitOfMeasure", Caption = "Unit Of Measure", ShortCaption = "UOM")]
		public override ZString C1_UnitOfMeasure
		{
			get { return base.C1_UnitOfMeasure; }
			set
			{
				var oldValue = C1_UnitOfMeasure;
				base.C1_UnitOfMeasure = value;
				if (oldValue != C1_UnitOfMeasure && !IsCopying && C1_Override && Parent != null)
				{
					DutyAndTaxAmountCalculator.CalculateDutyAndTaxAmount(this, Parent.CustomsValue);
				}
			}
		}

		#endregion

		#region Description

		[ReadOnly(true)]
		[ResourceStringData("DutyAndTax|Description", ShortCaption = "Desc.", Caption = "Description")]
		[MaxLength(Schema.DescriptionMaxLength)]
		public ZString Description
		{
			get
			{
				return Factory.GetValue(ref cachedDescription, () =>
				{
					var result = ZString.Empty;
					switch (C1_TaxType)
					{
						case DutyAndTaxTypes.Codes.GST:
						case DutyAndTaxTypes.Codes.ExciseTax:
							result = AddInfoLookups.Rates.GetDescriptionFromCode(C1_Code);
							break;
						default:
							result = AddInfoLookups.Types.GetDescriptionFromCode(C1_TaxType);
							break;
					}
					return result;
				});
			}
		}
		CachedProperty<ZString> cachedDescription;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		#endregion

		#region Quantity

		[ReadOnly(true)]
		[DecimalPlaces(5)]
		[ResourceStringData("DutyAndTax|Quantity", ShortCaption = "Qty", Caption = "Quantity")]
		public ZDecimal Quantity
		{
			get { return GetQuantity(C1_UnitOfMeasure, C1_TaxType, C1_RateType); }
			set
			{
				quantity = value;
				QuantityInfo.RefreshBinding();
			}
		}
		ZDecimal quantity;

		ZDecimal GetQuantity(ZString unitOfMeasure, ZString taxType, ZString rateType)
		{
			var result = ZDecimal.Zero;
			if (quantity != ZDecimal.Zero)
			{
				result = quantity;
			}
			else
			{
				result = DutyAndTaxAmountCalculator.GetQuantity(Parent, unitOfMeasure, taxType, rateType, C1_NormalValuePerUnit);
			}
			return result;
		}

		public ZPropertyInfo QuantityInfo
		{
			get { return GetZPropertyInfo(Schema.Quantity); }
		}

		#endregion

		#region Amount Description

		[ReadOnly(true)]
		[ResourceStringData("DutyAndTax|AmountDescription", ShortCaption = "Desc.", Caption = "Amount Calculation Description")]
		public ZString AmountDescription
		{
			get
			{
				if (cachedAmountDescription == null)
				{
					cachedAmountDescription = new CachedProperty<ZString>(Factory, () =>
					{
						if (!C1_Override)
						{
							Parent.DutyAndTaxManager?.UpdateDutiesAmountDescriptions();
							if (amountDescription.IsEmpty)
							{
								amountDescription = DutyAndTaxAmountDescriptor.GetDescription(this);
							}
						}
						else
						{
							amountDescription = ZString.Empty;
						}
						return amountDescription;
					});
				}
				return cachedAmountDescription.Value;
			}
			internal set { SetNonPersistentPropertyValue(AmountDescriptionInfo, ref amountDescription, value); }
		}
		CachedProperty<ZString> cachedAmountDescription;
		ZString amountDescription;

		public ZPropertyInfo AmountDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.AmountDescription); }
		}

		#endregion

		#region C1_NormalValuePerUnit

		[ReadOnlyMember(nameof(IsNormalValueReadOnly))]
		[ResourceStringData("CADutyAndTaxAddInfo|C1_NormalValuePerUnit", Caption = "Normal Value per Unit")]
		public override ZDecimal C1_NormalValuePerUnit
		{
			get { return base.C1_NormalValuePerUnit; }
			set
			{
				var oldValue = C1_NormalValuePerUnit;
				base.C1_NormalValuePerUnit = value;
				if (C1_NormalValuePerUnit != oldValue && !IsCopying && C1_Override)
				{
					if (C1_NormalValuePerUnit.IsEmpty)
					{
						C1_NormalValueCurrency = ZString.Empty;
					}
					else if (C1_NormalValueCurrency.IsEmpty)
					{
						C1_NormalValueCurrency = Core.Constants.CurrencyCodes.Canada;
					}

					if (Parent != null)
					{
						var customsValue = Parent.CustomsValue;
						if (Parent.ExchangeRate != NormalValueCurrencyExchangeRate)
						{
							customsValue = Utilities.Round(Parent.ValueForCurrencyConversion * NormalValueCurrencyExchangeRate, 2);
						}
						DutyAndTaxAmountCalculator.CalculateDutyAndTaxAmount(this, customsValue);
					}
				}
			}
		}

		#endregion

		#region C1_NormalValueCurrency

		[ReadOnlyMember(nameof(IsNormalValueReadOnly))]
		[ResourceStringData("CADutyAndTaxAddInfo|C1_NormalValueCurrency", Caption = "Normal Value Currency", MediumCaption = "Normal Value Curr.")]
		public override ZString C1_NormalValueCurrency
		{
			get { return base.C1_NormalValueCurrency; }
			set
			{
				var oldValue = C1_NormalValueCurrency;
				base.C1_NormalValueCurrency = value;
				if (oldValue != C1_NormalValueCurrency && !IsCopying && C1_Override)
				{
					if (Parent != null)
					{
						DutyAndTaxAmountCalculator.CalculateDutyAndTaxAmount(this, Parent.CustomsValue);
					}
				}
			}
		}

		ZBool IsNormalValueReadOnly
		{
			get { return C1_TaxType != DutyAndTaxTypes.Codes.ADD || !C1_Override; }
		}

		#endregion

		#region C1_ForeignRate

		[ReadOnlyMember(nameof(IsForeignCurrencyReadOnly))]
		[ResourceStringData("CADutyAndTaxAddInfo|C1_ForeignRate", Caption = "Foreign Rate")]
		public override ZDecimal C1_ForeignRate
		{
			get { return base.C1_ForeignRate; }
			set
			{
				var oldValue = C1_ForeignRate;
				base.C1_ForeignRate = value;
				if (oldValue != C1_ForeignRate && !IsCopying)
				{
					CalculateRateFromForeighRate();
				}
			}
		}

		ZBool IsForeignCurrencyReadOnly
		{
			get { return !DutyAndTaxTypes.IsSIMATaxCode(C1_TaxType) || !C1_Override; }
		}

		ZBool IsCountervailingTaxType => C1_TaxType == DutyAndTaxTypes.Codes.CVD;

		internal void CalculateRateFromForeighRate()
		{
			if (!IsDutyOnProductOrCusClassification)
			{
				var result = ZDecimal.Zero;
				if (C1_ForeignCurrency == Core.Constants.CurrencyCodes.Canada)
				{
					result = C1_ForeignRate;
				}
				else if (!C1_ForeignRate.IsEmpty && !C1_ForeignCurrency.IsEmpty)
				{
					var fromCurrency = RefCurrency.LoadFromCurrencyCode(Factory, C1_ForeignCurrency);
					var toCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Canada);
					if (CurrencyConverter != null)
					{
						var rateInCAD = CurrencyConverter.ConvertExact(new Money(C1_ForeignRate, fromCurrency), toCurrency, roundToDestinationCurrencyDecimals: !IsCountervailingTaxType);
						if (rateInCAD != null)
						{
							result = rateInCAD.Amount;
						}
					}
				}

				C1_Rate = result;
			}
		}

		#endregion

		#region C1_ForeignCurrency

		[ReadOnlyMember(nameof(IsForeignCurrencyReadOnly))]
		[ResourceStringData("CADutyAndTaxAddInfo|C1_ForeignCurrency", ShortCaption = "Curr.", Caption = "Foreign Currency", MediumCaption = "Foreign Curr.")]
		public override ZString C1_ForeignCurrency
		{
			get { return base.C1_ForeignCurrency; }
			set
			{
				var oldValue = C1_ForeignCurrency;
				base.C1_ForeignCurrency = value;
				if (oldValue != value && !IsCopying)
				{
					CalculateRateFromForeighRate();
				}
			}
		}

		#endregion

		#region ForeignCurrencyExchangeRate

		[DecimalPlaces(6)]
		[ResourceStringData("DutyAndTax|ForeignCurrencyExchangeRate", ShortCaption = "Exchange Rate", Caption = "Foreign Curr. Exchange Rate", FullDescription = "Foreign Currency Exchange Rate")]
		public ZDecimal ForeignCurrencyExchangeRate
		{
			get
			{
				return Factory.GetValue(ref foreignCurrencyExchangeRateCached, () =>
				{
					var result = ZDecimal.Zero;
					if (!C1_ForeignCurrency.IsEmpty && CurrencyConverter != null)
					{
						var fromCurrency = RefCurrency.LoadFromCurrencyCode(Factory, C1_ForeignCurrency);
						result = CurrencyConverter.GetExchangeRate(fromCurrency);
					}
					return result;
				});
			}
		}
		CachedProperty<ZDecimal> foreignCurrencyExchangeRateCached;

		public ZPropertyInfo ForeignCurrencyExchangeRateInfo
		{
			get { return GetZPropertyInfo(Schema.ForeignCurrencyExchangeRate); }
		}

		#endregion

		#region NormalValueCurrencyExchangeRate

		[DecimalPlaces(6)]
		[ResourceStringData("DutyAndTax|NormalValueCurrencyExchangeRate", ShortCaption = "Exchange Rate", Caption = "Normal Val. Curr. Exchange Rate", FullDescription = "Normal Value Currency Exchange Rate")]
		public ZDecimal NormalValueCurrencyExchangeRate
		{
			get
			{
				return Factory.GetValue(ref normalValueCurrencyExchangeRateCached, () =>
				{
					var result = ZDecimal.Zero;
					if (!C1_NormalValueCurrency.IsEmpty && CurrencyConverter != null)
					{
						var fromCurrency = RefCurrency.LoadFromCurrencyCode(Factory, C1_NormalValueCurrency);
						result = CurrencyConverter.GetExchangeRate(fromCurrency);
					}
					return result;
				});
			}
		}
		CachedProperty<ZDecimal> normalValueCurrencyExchangeRateCached;

		public ZPropertyInfo NormalValueCurrencyExchangeRateInfo
		{
			get { return GetZPropertyInfo(Schema.NormalValueCurrencyExchangeRate); }
		}

		#endregion

		#region CAGSTRateCode
		public ZZRefCusCodeListCombined CAGSTRateCode
		{
			get
			{
				if (IsGST)
				{
					return Factory.GetCachedValue(string.Format("CAGSTRateCode_{0}_{1}", C1_Code, (Parent?.EffectiveDutyDate ?? ZDateTime.Today).ToISO8601ShortDateString()), delegate ()
					{
						return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, C1_Code, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, Parent.EffectiveDutyDate);
					});
				}
				else
				{
					return null;
				}
			}
		}
		#endregion

		#region CIGARS

		readonly List<ZString> cigarsRateCodes = new List<ZString> { "E01", "E07", "E37" };

		void UpdateRateTypeForCIGARS()
		{
			if (IsExciseTax && C1_Override && cigarsRateCodes.Contains(C1_Code) && C1_RateType != RateTypes.Codes.AcceptT && C1_RateType != RateTypes.Codes.AcceptX)
			{
				C1_RateType = RateTypes.Codes.AcceptT;
			}
		}

		#endregion

		CurrencyConverter CurrencyConverter
		{
			get { return currencyConverter ?? (currencyConverter = Parent?.CurrencyConverter); }
		}
		CurrencyConverter currencyConverter;

		#region Common ReadOnly

		public bool NotOverridenReadOnly
		{
			get { return !C1_Override; }
		}

		public bool SIMADutyReadOnly
		{
			get { return C1_TaxType == DutyAndTaxTypes.Codes.SIMADuty || !C1_Override; }
		}

		protected bool CustomsDutyReadOnly
		{
			get
			{
				return (C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty && !(Parent?.IsWarehouseOrSupplementaryEntry ?? ZBool.False) && !C1_Override) || (C1_TaxType != DutyAndTaxTypes.Codes.CustomsDuty);
			}
		}

		#endregion

		public bool IsWarehouseOrSupplementaryEntryHasValues => (Parent?.IsWarehouseOrSupplementaryEntry ?? ZBool.False) && (!C1_PreviousTranNumber.IsEmpty || !C1_PreviousTranLine.IsEmpty);

		public static ZString GetExciseTaxCodeYieldHighestTax(BusinessObjectFactory factory, IDutyAndTaxData parent, IEnumerable<ZString> refNums)
		{
			var result = ZString.Empty;
			if (parent != null)
			{
				var taxRates = CACTaxRate.Load(factory, refNums, DutyAndTaxTypes.Codes.ExciseTax, parent.EffectiveDutyDate);
				var highestTaxAmount = 0m;
				foreach (var taxRate in taxRates)
				{
					var taxAmount = 0m;
					switch (taxRate.ZH_RateType)
					{
						case RateTypes.Codes.AdValorem:
							taxAmount = Utilities.Round(parent.NormalDutyPaidValue * taxRate.ZH_Rate / 100m, 2);
							break;
						case RateTypes.Codes.Specific:
							taxAmount = Utilities.Round(DutyAndTaxAmountCalculator.GetQuantity(parent, taxRate.ZH_UnitOfMeasure, DutyAndTaxTypes.Codes.ExciseTax, taxRate.ZH_RateType, 0m) * taxRate.ZH_Rate, 2);
							break;
					}
					if (taxAmount > highestTaxAmount)
					{
						highestTaxAmount = taxAmount;
						result = taxRate.ZH_TaxRefNumber;
					}
				}
			}
			return result;
		}

		protected override Customs.Business.MultiLineAddInfos.CusAddInfoValidation GetNewValidation()
		{
			return new DutyAndTaxValidation(this);
		}

		internal void ClearFieldsForRecycle()
		{
			this.amountDescription = ZString.Empty;
			this.quantity = ZDecimal.Zero;
			this.IsInRefFiles = false;

			if (DutyAndTaxTypes.IsSIMATaxCode(C1_TaxType))
			{
				this.C1_ExemptCode = ZString.Empty;
				this.C1_RateType = ZString.Empty;
				this.C1_Rate = ZDecimal.Zero;
				this.C1_UnitOfMeasure = ZString.Empty;
				this.C1_NormalValuePerUnit = ZDecimal.Zero;
				this.C1_NormalValueCurrency = ZString.Empty;
				this.C1_ForeignRate = ZDecimal.Zero;
				this.C1_ForeignCurrency = ZString.Empty;
			}
		}

		#region Synchroniser
		public void EnableAndSynchronise()
		{
			if (ShouldSynchronise && !B2DutyAndTaxSynchroniser.IsEnabled)
			{
				using (GetValidationSuspender())
				{
					B2DutyAndTaxSynchroniser.SetEnabled(true, B2DutyAndTaxSynchroniser.DetectEnabled);
					B2DutyAndTaxSynchroniser.Synchronise();
				}
			}
		}

		internal B2DutyAndTaxSynchroniser B2DutyAndTaxSynchroniser
		{
			get
			{
				if (fB2DutyAndTaxSynchroniser == null && InvoiceLine?.CorrespondingAsClaimedForInvoiceLine.DutiesAndTaxes is DutyAndTaxCollection dutiesAndTaxes)
				{
					var taxType = C1_TaxType;
					var destination = dutiesAndTaxes.FirstOrDefault(x => !x.C1_Override && x.C1_TaxType.EqualsIgnoringCase(taxType));
					if (destination == null)
					{
						destination = dutiesAndTaxes.AddNew();
						destination.C1_TaxType = taxType;
					}
					fB2DutyAndTaxSynchroniser = new B2DutyAndTaxSynchroniser(destination, this);
				}

				return fB2DutyAndTaxSynchroniser;
			}
		}
		B2DutyAndTaxSynchroniser fB2DutyAndTaxSynchroniser;

		internal bool ShouldSynchronise => !IsInDatabase
			&& C1_Override
			&& InvoiceLine is JobComInvoiceLine invoiceLine
			&& invoiceLine.CA_IsAccountForLine
			&& invoiceLine.Declaration is JobDeclaration declaration
			&& (declaration.IsB2Adjustments || declaration.IsB3X)
			&& invoiceLine.CorrespondingAsClaimedForInvoiceLine != null;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				return Parent as JobComInvoiceLine;
			}
		}

		#endregion

		#region ISynchroniserReadOnlyMembersProvider

		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		#endregion

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (fB2DutyAndTaxSynchroniser != null)
				{
					fB2DutyAndTaxSynchroniser.SetEnabled(false, fB2DutyAndTaxSynchroniser.DetectEnabled);
					fB2DutyAndTaxSynchroniser.Dispose();
					fB2DutyAndTaxSynchroniser = null;
				}
			}
		}

		#endregion

		#region IDutyAndTaxDataForCalculation
		ZString IDutyAndTaxDataForCalculation.RateType => C1_RateType;

		ZDecimal IDutyAndTaxDataForCalculation.Rate => C1_Rate;

		ZString IDutyAndTaxDataForCalculation.UnitOfMeasure => C1_UnitOfMeasure;

		ZDecimal IDutyAndTaxDataForCalculation.ValueForCalculation { get => C1_ValueForCalculation; set => C1_ValueForCalculation = value; }

		ZDecimal IDutyAndTaxDataForCalculation.NormalValuePerUnit => C1_NormalValuePerUnit;

		ZString IDutyAndTaxDataForCalculation.NormalValueCurrency => C1_NormalValueCurrency;

		ZDecimal IDutyAndTaxDataForCalculation.Amount { get => C1_Amount; set => C1_Amount = value; }

		ZString IDutyAndTaxDataForCalculation.TaxType => C1_TaxType;

		ZString IDutyAndTaxDataForCalculation.ExemptCode => C1_ExemptCode;

		ZBool IDutyAndTaxDataForCalculation.Override => C1_Override;

		ZString IDutyAndTaxDataForCalculation.DutyType => C1_DutyType;

		#endregion
	}
}
