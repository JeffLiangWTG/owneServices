//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCADutyAndTaxAddInfoLookups
//
//    This class should be used for overriding collections in AutoCADutyAndTaxAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CADutyAndTaxAddInfoLookups : AutoCADutyAndTaxAddInfoLookups
	{
		public CADutyAndTaxAddInfoLookups(AutoCADutyAndTaxAddInfo parent)
			: base(parent)
		{
		}

		new DutyAndTax Parent
		{
			get { return (DutyAndTax)((CADutyAndTaxAddInfo)base.Parent).Parent; }
		}

		public CodeDescriptionPairList Types
		{
			get
			{
				return Factory.GetCachedValue("CADutyAndTaxTypes" + Parent.IsDutyOnProductOrCusClassification, delegate()
				{
					if (Parent.IsDutyOnProductOrCusClassification)
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(DutyAndTaxTypes.Codes.ADD, DutyAndTaxTypes.Descriptions.ADD);
						result.AddPair(DutyAndTaxTypes.Codes.CVD, DutyAndTaxTypes.Descriptions.CVD);
						result.AddPair(DutyAndTaxTypes.Codes.SUR, DutyAndTaxTypes.Descriptions.SUR);
						return result;
					}
					else
					{
						var result = new DutyAndTaxTypes();
						result.RemoveCode(DutyAndTaxTypes.Codes.SIMADuty);
						return result;
					}
				});
			}
		}

		public CodeDescriptionPairList Rates
		{
			get
			{
				var parent = Parent;
				var classificationNumber = parent.Parent?.ClassificationNumber;
				var effectiveDate = parent.Parent?.EffectiveDutyDate ?? ZDateTime.Today;
				if (parent.IsGST)
				{
					return Factory.GetCachedValue(string.Format("RefCusCodeList_CAGST_{0}_{1}", effectiveDate.ToISO8601ShortDateString(), classificationNumber), () =>
					{
						var result = new CodeDescriptionPairList();
						var rates = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, effectiveDate);
						result.AddRange(rates);
						result.Sort();
						return result;
					});
				}
				else if (parent.IsExciseTax || parent.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty)
				{
					return UniversalReferenceHelper.GetRefCusRateCodePairList(Factory, DutyAndTaxTypes.Codes.ExciseTax, effectiveDate, classificationNumber);
				}
				else if (parent.C1_TaxType == DutyAndTaxTypes.Codes.SUR)
				{
					if (classificationNumber.HasValue)
					{
						var countryOfOrigin = parent.Parent?.CountryOfOrigin ?? ZString.Empty;
						return Factory.GetCachedValue(string.Format("SurtaxCodeList_{0}_{1}_{2}", classificationNumber.Value, countryOfOrigin, effectiveDate.ToISO8601ShortDateString()), () =>
						{
							var result = new CodeDescriptionPairList();
							var tariffView = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Canada, Constants.TariffTypes.HarmonizedSystem, classificationNumber.Value, effectiveDate);
							if (tariffView != null)
							{
								var rateViews = tariffView.Rates.Where(d => d.ZZ2_StartDate <= effectiveDate && d.ZZ2_EndDate >= effectiveDate && d.RateCode == DutyAndTaxTypes.Codes.SUR);
								rateViews.ForEach(x =>
								{
									var applicabilities = x.FilteredRateApplicabilities.Where(x => x.IsApplicable(countryOfOrigin, effectiveDate)).Select(y => y.ZZT_AdditionalCode);
									applicabilities.ForEach(y => result.AddPairIfNotExist(y, ZString.Empty));
								});
							}
							return result;
						});
					}
				}
				return new CodeDescriptionPairList();
			}
		}

		public CodeDescriptionPairList ExemptCodes
		{
			get
			{
				return Factory.GetCachedValue("ExemptCodes" + Parent.C1_TaxType, delegate()
				{
					var result = new CodeDescriptionPairList();
					if (DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(Parent.C1_TaxType))
					{
						result = new SIMACodes();
					}
					else if (Parent.IsExciseTax)
					{
						result = new ExciseTaxExemptionCodes();
					}
					else if (Parent.IsGST)
					{
						result = new GSTStatusCodes();
					}
					else if (Parent.C1_TaxType == DutyAndTaxTypes.Codes.CPT)
					{
						result = new CPTExcemptionCodes();
					}

					return result;
				});
			}
		}

		public RateTypes RateTypes
		{
			get { return new RateTypes(); }
		}

		public CodeDescriptionPairList CustomsUQList
		{
			get { return new CustomsUnitOfMeasureList(); }
		}

		public RefCurrencyCollection CurrencyList
		{
			get { return new RefCurrencyCollection(Factory); }
		}
	}
}
