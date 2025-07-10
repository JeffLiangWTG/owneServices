using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class CusPermitHeader : BaseCusPermitHeader, Integration.Customs.EU.ICusPermitHeader
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : BaseCusPermitHeader.Schema
		{
			public const int CPH_FullTypeMaxLength = 7;
			public new const int CPH_TypeMaxLength = 4;
		}

		protected override ZBool AllowNewLineTransactions => IsCUM && IsTransactionsApplicable();

		public CusPermitHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new static readonly CusPermitHeaderTypeDecider TypeDecider = new CusPermitHeaderTypeDecider();

		[MaxLength(Schema.CPH_TypeMaxLength)]
		public override ZString CPH_Type
		{
			get => base.CPH_Type;
			set
			{
				var safeValue = value.Left(Schema.CPH_TypeMaxLength);
				if (base.CPH_Type != safeValue)
				{
					base.CPH_Type = safeValue;
					UpdateCPH_FullType();
				}
			}
		}

		[MaxLength(Schema.CPH_SubTypeMaxLength)]
		public override ZString CPH_SubType
		{
			get => base.CPH_SubType;
			set
			{
				if (base.CPH_SubType != value)
				{
					base.CPH_SubType = value;
					UpdateCPH_FullType();
				}
			}
		}

		[MaxLength(Schema.CPH_FullTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusPermitHeaderLookups.PermitFullTypes))]
		public virtual ZString CPH_FullType
		{
			get
			{
				if (!fullType.HasValue)
				{
					fullType = CPH_Type + CPH_SubType;
				}
				return fullType.Value;
			}
			set
			{
				var fullTypeValue = ZString.Empty;
				if (SetNonPersistentPropertyValue(CPH_FullTypeInfo, ref fullTypeValue, value))
				{
					CPH_Type = fullTypeValue.Left(Schema.CPH_TypeMaxLength);
					CPH_SubType = fullTypeValue.SubstringSafe(Schema.CPH_TypeMaxLength);
					fullType = fullTypeValue;

					if (!IsValidationSuspended)
					{
						Validation.ValidateCPH_FullType();
					}
					CPH_FullTypeInfo.RefreshBinding();
				}
			}
		}
		ZString? fullType;

		public ZPropertyInfo CPH_FullTypeInfo => GetZPropertyInfo(nameof(CPH_FullType));

		void UpdateCPH_FullType()
		{
			SetNonPersistentPropertyValue(CPH_FullTypeInfo, ref fullType, CPH_Type.PadRight(Schema.CPH_TypeMaxLength) + CPH_SubType);
		}

		protected virtual bool CPH_FullTYpe_ReadOnly => false;

		protected override bool CPH_Type_ReadOnly => true;

		public override bool CPH_SubType_ReadOnly => true;

		[List(nameof(Lookups) + "." + nameof(CusPermitHeaderLookups.UnitOfQuantityList))]
		public override ZString CPH_UnitOfMeasure { get => base.CPH_UnitOfMeasure; set => base.CPH_UnitOfMeasure = value; }

		public new CusPermitHeaderValidation Validation => (CusPermitHeaderValidation)base.Validation;

		protected override Customs.Business.CusPermitHeaderValidation GetNewValidation()
		{
			return new CusPermitHeaderValidation(this);
		}

		public new CusPermitHeaderLookups Lookups => (CusPermitHeaderLookups)base.Lookups;

		protected override Customs.Business.CusPermitHeaderLookups GetNewLookups()
		{
			return new CusPermitHeaderLookups(this);
		}

		public new PermitCountrySpecificInstruction CountrySpecificInstruction => (PermitCountrySpecificInstruction)base.CountrySpecificInstruction;

		public ZString GetDefaultDataGroupingCode() => Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CPH_RN_NKCountryCode);
	}
}
