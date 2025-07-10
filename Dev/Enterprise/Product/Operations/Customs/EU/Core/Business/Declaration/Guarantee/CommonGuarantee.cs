using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CommonGuarantee : CusBondDetail, ICommonGuarantee
	{
		public CommonGuarantee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override CusBondDetailLookups GetNewLookups() => new CommonGuaranteeLookups(this);
		public new CommonGuaranteeLookups Lookups => (CommonGuaranteeLookups)base.Lookups;

		protected override CusBondDetailValidation GetNewValidation() => new CommonGuaranteeValidation(this);
		public new CommonGuaranteeValidation Validation => (CommonGuaranteeValidation)base.Validation;

		[List(nameof(Lookups) + "." + nameof(CommonGuaranteeLookups.BondTypeList))]
		public override ZString PW_BondType
		{
			get => base.PW_BondType;
			set => base.PW_BondType = value;
		}

		[List(nameof(Lookups) + "." + nameof(CommonGuaranteeLookups.OfficeCodeList))]
		public override ZString PW_BondFiledPort
		{
			get => base.PW_BondFiledPort;
			set => base.PW_BondFiledPort = value;
		}

		[List(nameof(Lookups) + "." + nameof(CommonGuaranteeLookups.List71NonEcContractingCountriesList))]
		public override ZString PW_ValidityLimitation
		{
			get => base.PW_ValidityLimitation;
			set => base.PW_ValidityLimitation = value;
		}

		protected override bool SupportsCloneCore() => true;

		[List(nameof(Lookups) + "." + nameof(CommonGuaranteeLookups.ReferenceNumbers))]
		public override ZString PW_BondNumber
		{
			get => base.PW_BondNumber;
			set
			{
				ZString oldValue = PW_BondNumber;
				base.PW_BondNumber = value;

				if (oldValue != PW_BondNumber && ShouldSetupHolderIdentificationOnBondNumberChange)
				{
					UpdateHolderIdentificationNumberAndSynchronizeGuaranteeHeader(PW_BondNumber);
				}
			}
		}

		protected virtual ZBool ShouldSetupHolderIdentificationOnBondNumberChange => true;

		[List(nameof(Lookups) + "." + nameof(CommonGuaranteeLookups.ReferenceNumbers))]
		public override ZString PW_BondNumber2
		{
			get => base.PW_BondNumber2;
			set
			{
				var oldValue = PW_BondNumber2;
				base.PW_BondNumber2 = value;

				if (oldValue != PW_BondNumber2 && ShouldSetupHolderIdentificationOnBondNumber2Change)
				{
					UpdateHolderIdentificationNumberAndSynchronizeGuaranteeHeader(PW_BondNumber2);
				}
			}
		}

		protected virtual ZBool ShouldSetupHolderIdentificationOnBondNumber2Change => false;

		protected virtual void SyncroniseWithGuaranteeHeader(Customs.Business.BaseCusGuaranteeHeader guaranteeHeader)
		{
			PW_BondType = !PW_BondType.IsEmpty && guaranteeHeader.CPH_SubType.IsEmpty ? PW_BondType : guaranteeHeader.CPH_SubType;
			PW_Password = guaranteeHeader.MainAccessCode.Left(CusBondDetail.Schema.PW_PasswordMaxLength);
		}

		[Password]
		[List(nameof(Lookups) + "." + nameof(CommonGuaranteeLookups.AccessCodeList))]
		public override ZString PW_Password { get => base.PW_Password; set => base.PW_Password = value; }

		[List(nameof(Lookups) + "." + nameof(CommonGuaranteeLookups.HolderIdentificationList))]
		public override ZString PW_HolderIdentification { get => base.PW_HolderIdentification; set => base.PW_HolderIdentification = value; }

		protected virtual Func<OrgHeader, ZString> AlternativeDelegateToFindHolderIdentification => null;

		public virtual ZString ReferenceNumberFieldType => nameof(FieldType.TextCodeFindBox);

		public virtual ZString AccessCodeFieldType => nameof(FieldType.Text);

		public virtual CusGuaranteeHeader CusGuarantee
		{
			get
			{
				if (cusGuarantee == null || cusGuarantee.IsDeleted || cusGuarantee.PK != PW_CPH_Guarantee)
				{
					cusGuarantee = Factory.Load<CusGuaranteeHeader>(PW_CPH_Guarantee);
				}
				return cusGuarantee;
			}
		}
		CusGuaranteeHeader cusGuarantee;

		void UpdateHolderIdentificationNumberAndSynchronizeGuaranteeHeader(ZString bondNumber)
		{
			var guarantee = Lookups.ReferenceNumbers
				.FirstOrDefault(g => g.CPH_Number == bondNumber && g.CPH_IsActive == true && (g.CPH_EndDate >= ZDateTime.Today || g.CPH_EndDate == ZDateTime.Empty));

			if (guarantee == null || guarantee.CPH_OH_PermitHolder.IsEmpty)
			{
				return;
			}

			var org = Factory.Load<OrgHeader>(OrgHeaderSchema.Constants.Prefix, guarantee.CPH_OH_PermitHolder);
			var code = org.GetEuIdentificationNumber();
			if (code.IsEmpty && AlternativeDelegateToFindHolderIdentification != null)
			{
				code = AlternativeDelegateToFindHolderIdentification(org);
			}

			PW_HolderIdentification = code;
			SyncroniseWithGuaranteeHeader(guarantee);
		}

		public ZString GetWarningErrorIfRemainingBalanceIsNotEnough()
		{
			var warningMessage = ZString.Empty;
			var totalBalanceIncludingPendingDecimal = CusGuarantee?.CPH_Calc_TotalBalanceIncludingPendingDecimal;
			var hasNotEnoughGuaranteeRemainingBalance = totalBalanceIncludingPendingDecimal < PW_BondAmount;
			if (hasNotEnoughGuaranteeRemainingBalance)
			{
				warningMessage = ResString.GetMultilingualString("BCD0CC31-B85A-4A4A-B52F-80FC18E6CBF0",
															"Guarantee Nº ({0}) has not enough remaining balance ({1:0.00}) to create the guarantee transaction ({2:0.00}). It will be created anyway.",
															PW_BondNumber, totalBalanceIncludingPendingDecimal, PW_BondAmount);
			}
			return warningMessage;
		}
	}
}
