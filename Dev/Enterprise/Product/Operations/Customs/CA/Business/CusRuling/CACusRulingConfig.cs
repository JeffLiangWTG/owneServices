using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using ConfigTypes = Enterprise.Customs.Universal.RefCusRulingConfigTypes.Codes;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CACusRulingConfig : CusRulingConfigCombined, Integration.Customs.CA.ICACusRulingConfig
	{
		public CACusRulingConfig(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Override Properties
		public override ZString ZZY_Value
		{
			get => base.ZZY_Value;
			set
			{
				var oldValue = base.ZZY_Value;
				if (oldValue != value)
				{
					base.ZZY_Value = value;
					if (ZZY_Type == ConfigTypes.Maximum || ZZY_Type == ConfigTypes.Minimum)
					{
						Validation.ValidateZZY_Rate();
					}
				}
			}
		}

		[ResourceStringData("EA7DD204-5C90-4D5A-A32E-2D92F1063371", Caption = "Rate(%)", ShortCaption = "Rate", FullDescription = "Enter the Rate of percentage here, i.e. 10 for 10 % (not .10)")]
		public override ZDecimal ZZY_Rate
		{
			get { return base.ZZY_Rate; }
			set
			{
				var oldValue = base.ZZY_Rate;
				if (oldValue != value)
				{
					base.ZZY_Rate = value;
					if (ZZY_Type == ConfigTypes.Maximum || ZZY_Type == ConfigTypes.Minimum)
					{
						Validation.ValidateZZY_Value();
					}
				}
			}
		}

		public override ZString ZZY_Type
		{
			get => base.ZZY_Type;
			set
			{
				var oldValue = base.ZZY_Type;
				if (oldValue != value)
				{
					base.ZZY_Type = value;
					Validation.ValidateZZY_Category();
				}
			}
		}

		public override ZString ZZY_Category
		{
			get => base.ZZY_Category;
			set
			{
				var oldValue = base.ZZY_Category;
				if (oldValue != value)
				{
					base.ZZY_Category = value;
					Validation.ValidateZZY_Type();
				}
			}
		}
		#endregion

		#region Override Methods
		#region ZZY_RateReadOnly
		protected override ZString GetRateReadOnlyCacheKey()
		{
			var result = base.GetRateReadOnlyCacheKey();
			return result += ZString.Format("CACusRulingConfig|Type:{0}-Category:{1}-Value:{2}", ZZY_Type, ZZY_Category, ZZY_Value);
		}

		protected override bool GetIsRateReadOnlyCore()
		{
			var result = base.GetIsRateReadOnlyCore();
			if (!result && IsCategoryValid)
			{
				if (IsDAT)
				{
					result = true;
				}
				else if (ZZY_Type == ConfigTypes.NoneFree || ZZY_Type == ConfigTypes.AcceptAmount || ZZY_Type == ConfigTypes.AcceptRate)
				{
					result = true;
				}
				else if (ZZY_Type == ConfigTypes.Maximum || ZZY_Type == ConfigTypes.Minimum)
				{
					if (!ZZY_Value.IsEmpty)
					{
						var decimalValue = ZDecimal.Zero;
						if (ZDecimal.TryParse(ZZY_Value, out decimalValue) && !decimalValue.IsEmpty)
						{
							result = true;
						}
					}
				}
				else if (((IsDTY || IsEXD) && ZZY_Type == ConfigTypes.TreatmentCode) || ((IsGST || IsSIM || IsEXC) && ZZY_Type == ConfigTypes.ExemptCode))
				{
					result = true;
				}
			}
			return result;
		}
		#endregion

		#region ZZY_ValueReadOnly
		protected override ZString GetValueReadOnlyCacheKey()
		{
			var result = base.GetValueReadOnlyCacheKey();
			return result += ZString.Format("CACusRulingConfig|Type:{0}-Category:{1}-Rate:{2}", ZZY_Type, ZZY_Category, ZZY_Rate.IsEmpty);
		}

		protected override bool GetIsValueReadOnlyCore()
		{
			var result = base.GetIsValueReadOnlyCore();
			if (!result && IsCategoryValid)
			{
				if (IsDAT || ZZY_Type == ConfigTypes.NoneFree || ZZY_Type == ConfigTypes.AcceptRate || ZZY_Type == ConfigTypes.AdValorem || ZZY_Type == ConfigTypes.Specific
					|| ((ZZY_Type == ConfigTypes.Maximum || ZZY_Type == ConfigTypes.Minimum) && !ZZY_Rate.IsEmpty))
				{
					result = true;
				}
			}
			return result;
		}
		#endregion

		protected override ZString GetValueFieldTypeCore()
		{
			var result = base.GetValueFieldTypeCore();
			if (IsCategoryValid)
			{
				if (ZZY_Type == ConfigTypes.AcceptAmount || ZZY_Type == ConfigTypes.Maximum || ZZY_Type == ConfigTypes.Minimum)
				{
					result = nameof(FieldType.Decimal);
				}
				else if (((IsDTY || IsEXD) && ZZY_Type == ConfigTypes.TreatmentCode) || ((IsGST || IsSIM || IsEXC) && ZZY_Type == ConfigTypes.ExemptCode))
				{
					result = nameof(FieldType.TextDropEdit);
				}
			}
			return result;
		}

		public override ZInt ZZY_ValueDecimalPlaces => 2;
		#endregion

		#region New Properties

		public new CACusRuling Ruling => (CACusRuling)base.Ruling;
		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		public IEnumerable<CACusRulingConfig> ParentConfigs
		{
			get
			{
				IEnumerable<CACusRulingConfig> result = null;
				var ruling = Ruling;
				if (ruling != null)
				{
					result = ruling.Configurations.Cast<CACusRulingConfig>();
				}
				else
				{
					var invoiceLine = InvoiceLine;
					if (invoiceLine != null)
					{
						result = invoiceLine.RulingConfigurations.Cast<CACusRulingConfig>();
					}
				}
				return result ?? Enumerable.Empty<CACusRulingConfig>();
			}
		}
		#endregion

		#region lookups
		public new CACusRulingConfigLookups Lookups => (CACusRulingConfigLookups)base.Lookups;

		protected override CusRulingConfigCombinedLookups GetNewLookups()
		{
			return new CACusRulingConfigLookups(this);
		}
		#endregion

		#region Validation
		public new CACusRulingConfigValidation Validation => (CACusRulingConfigValidation)base.Validation;

		protected override CusRulingConfigCombinedValidation GetNewValidation()
		{
			return new CACusRulingConfigValidation(this);
		}
		#endregion
	}
}
