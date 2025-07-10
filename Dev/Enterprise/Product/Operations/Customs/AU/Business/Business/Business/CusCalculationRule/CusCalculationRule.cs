using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[DescriptionProperty(nameof(Description))]
	public class CusCalculationRule : Customs.Business.CusCalculationRule, ICusCalculationRule
	{
		public CusCalculationRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString Description
		{
			get
			{
				var sb = new ZStringBuilder();
				sb.AppendIfNotEmpty(Importer?.OH_Code);
				sb.AppendIfNotEmpty(sb.Length > 0 ? " " : "", CCR_TransportMode);
				sb.AppendIfNotEmpty(sb.Length > 0 ? " " : "", CCR_StartDate.ToShortDateString());
				sb.Prepend($"{CCR_RuleType} Rule - ");
				return sb.ToString();
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[ResourceStringData("CusCalculationRule|CCR_RuleType", Caption = "Rule Type")]
		public override ZString CCR_RuleType { get => base.CCR_RuleType; set => base.CCR_RuleType = value; }

		[ResourceStringData("CusCalculationRule|CCR_OH_Importer", Caption = "Importer", FullDescription = "If blank, this will be used as the fallback rate.")]
		public override ZGuid CCR_OH_Importer { get => base.CCR_OH_Importer; set => base.CCR_OH_Importer = value; }

		[ResourceStringData("CusCalculationRule|CCR_TransportMode", Caption = "Transport Mode")]
		public override ZString CCR_TransportMode { get => base.CCR_TransportMode; set => base.CCR_TransportMode = value; }

		[ResourceStringData("CusCalculationRule|CCR_BasedOn", Caption = "Based On", FullDescription = "Subject to the Rule Type of INS - ‘The Incoterm structure used to evaluate the value’.")]
		[List(nameof(Lookups) + "." + nameof(CusCalculationRuleLookups.BasedOnList))]
		public override ZString CCR_BasedOn { get => base.CCR_BasedOn; set => base.CCR_BasedOn = value; }

		[ResourceStringData("CusCalculationRule|CCR_RX_NKCurrency", Caption = "Currency")]
		public override ZString CCR_RX_NKCurrency { get => base.CCR_RX_NKCurrency; set => base.CCR_RX_NKCurrency = value; }

		[ResourceStringData("CusCalculationRule|CCR_StartDate", Caption = "Start Date")]
		public override ZDateTimeOffset CCR_StartDate { get => base.CCR_StartDate; set => base.CCR_StartDate = value; }

		[ResourceStringData("CusCalculationRule|CCR_EndDate", Caption = "End Date")]
		public override ZDateTimeOffset CCR_EndDate
		{
			get => base.CCR_EndDate;
			set
			{
				var newValue = value;
				if (newValue.IsEmpty)
				{
					newValue = DefaultEndDate;
				}
				base.CCR_EndDate = newValue;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CCR_RuleType = CusCalculationRuleTypeList.Codes.INS;
			CCR_GC_Company = GlbCompany.CurrentCompany.PK;
			CCR_EndDate = DefaultEndDate;
		}

		protected override Customs.Business.CusCalculationRuleLookups GetNewLookups()
		{
			return new CusCalculationRuleLookups(this);
		}

		protected override Customs.Business.CusCalculationRuleValidation GetNewValidation()
		{
			return new CusCalculationRuleValidation(this);
		}

		[ChildEditable]
		public CusCalculationRuleRateCollection CalculationRuleRateCollection
		{
			get
			{
				if (calculationRuleRateCollection == null)
				{
					if (CCR_RuleType == CusCalculationRuleTypeList.Codes.INS)
					{
						calculationRuleRateCollection = InsuranceRuleCalculation.LoadRatesFromFormula(this, Factory);
					}
					else
					{
						calculationRuleRateCollection = new CusCalculationRuleRateCollection(Factory, this);
					}
					RegisterEditableChildObject(calculationRuleRateCollection);
				}
				return calculationRuleRateCollection;
			}
		}
		CusCalculationRuleRateCollection calculationRuleRateCollection;

		public override void OnSaving()
		{
			base.OnSaving();
			if (CCR_RuleType == CusCalculationRuleTypeList.Codes.INS && CalculationRuleRateCollection.Count > 0 && CalculationRuleRateCollection.HasChanges)
			{
				var formula = InsuranceRuleCalculation.GetFormulaFromRates(CalculationRuleRateCollection);
				CCR_Formula = formula;
			}
		}

		public new CusCalculationRuleLookups Lookups => (CusCalculationRuleLookups)base.Lookups;

		internal ZDateTimeOffset DefaultEndDate => ZDateTime.MaxSmallDateTime.ToOffset();

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public CusCalculationRule LoadApplicableInsuranceRule(ZGuid importerPK, ZString transportMode, ZDateTimeOffset effectiveDate)
			{
				var query = new ZDBOnlyQuery(typeof(CusCalculationRule));
				query.AddToFilter(JoinCondition.Or, CusCalculationRuleSchema.CCR_OH_Importer, DBNull.Value);
				if (importerPK.IsValid)
				{
					query.AddToFilter(JoinCondition.Or, CusCalculationRuleSchema.CCR_OH_Importer, importerPK);
				}
				query.AddToFilter(CusCalculationRuleSchema.CCR_RuleType, CusCalculationRuleTypeList.Codes.INS);
				query.AddToFilter(CusCalculationRuleSchema.CCR_GC_Company, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(CusCalculationRuleSchema.CCR_StartDate, SQLComparisonOperator.LessThanOrEqualTo, effectiveDate);
				query.AddToFilter(CusCalculationRuleSchema.CCR_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, effectiveDate);
				query.AddToFilter(CusCalculationRuleSchema.CCR_TransportMode, new[] { ZString.Empty, transportMode });
				query.OrderBy = "CCR_OH_Importer DESC, CCR_TransportMode DESC";
				return Factory.LoadTop1<CusCalculationRule>(query);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusCalculationRule);
			}
		}
	}
}
