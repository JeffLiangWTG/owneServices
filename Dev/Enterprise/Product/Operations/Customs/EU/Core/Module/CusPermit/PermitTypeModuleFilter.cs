using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Module
{
	public class PermitTypeModuleFilter : Customs.Module.PermitTypeModuleFilter
	{
		public static class Schema
		{
			public const int Property1MaxLength = CusPermitHeader.Schema.CPH_TypeMaxLength;
			public const int Property2MaxLength = CusPermitHeader.Schema.CPH_SubTypeMaxLength;
			public const int Property3MaxLength = CusPermitHeader.Schema.CPH_FullTypeMaxLength;
		}

		public PermitTypeModuleFilter(ZString description, GetPermitTypeQuery queryDelegate, GetList getCountries) : base(description, queryDelegate, getCountries)
		{
			Property1Validation = null;
			Property2Validation = null;
		}

		[MaxLength(Schema.Property1MaxLength)]
		public override ZString Property1
		{
			get => base.Property1;
			set
			{
				if (base.Property1 != value)
				{
					base.Property1 = value;
					UpdateProperty3();
				}
			}
		}

		[MaxLength(Schema.Property2MaxLength)]
		public override ZString Property2
		{
			get => base.Property2;
			set
			{
				if (base.Property2 != value)
				{
					base.Property2 = value;
					UpdateProperty3();
				}
			}
		}

		[MaxLength(Schema.Property3MaxLength)]
		[List(nameof(PermitFullTypes))]
		public virtual ZString Property3
		{
			get
			{
				if (!property3.HasValue)
				{
					property3 = Property1 + Property2;
				}
				return property3.Value;
			}
			set
			{
				var property3Value = ZString.Empty;
				if (SetNonPersistentPropertyValue(Property3Info, ref property3Value, value))
				{
					Property1 = property3Value.Left(Schema.Property1MaxLength);
					Property2 = property3Value.SubstringSafe(Schema.Property1MaxLength);
					property3 = property3Value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty3();
					}
					Property3Info.RefreshBinding();
				}
			}
		}
		ZString? property3;

		void UpdateProperty3()
		{
			SetNonPersistentPropertyValue(Property3Info, ref property3, Property1.PadRight(Schema.Property1MaxLength) + Property2);
		}

		public ZPropertyInfo Property3Info => GetZPropertyInfo(nameof(Property3));

		public new PermitCountrySpecificInstruction CountrySpecificInstruction => (PermitCountrySpecificInstruction)base.CountrySpecificInstruction;

		public ZZRefCusCodeListCombinedCollection PermitFullTypes => CountrySpecificInstruction?.GetFullTypeList(Property0) ?? Factory.GetSupportingDocumentList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		public virtual bool Property1_ReadOnly => true;

		public override bool Property2_ReadOnly => true;

		public new PermitTypeModuleFilterValidation Validation => (PermitTypeModuleFilterValidation)base.Validation;

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new PermitTypeModuleFilterValidation(this);
		}
	}
}
