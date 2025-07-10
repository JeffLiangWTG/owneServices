using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NctsGuarantee = Enterprise.Customs.EU.NCTS.Business.NctsGuarantee;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class Guarantee : NctsGuarantee
	{
		public Guarantee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Lookups

		public new GuaranteeLookups Lookups => (GuaranteeLookups)base.Lookups;

		protected override MasterFiles.Business.CusBondDetailLookups GetNewLookups() => new GuaranteeLookups(this);

		#endregion

		#region Validation

		public new GuaranteeValidation Validation => (GuaranteeValidation)base.Validation;

		protected override NctsGuaranteePhase5Validation GetNewPhase5Validation() => new GuaranteeValidation(this);

		#endregion

		#region Properties

		public new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

		public override ZString PW_BondType
		{
			get => base.PW_BondType;
			set
			{
				var oldValue = PW_BondType;
				base.PW_BondType = value;

				if (!IsCopying && PW_BondType != oldValue)
				{
					UpdateBondNumber();
					UpdatePassword();

					if (PW_BondNumber2_ReadOnly)
					{
						PW_BondNumber2 = ZString.Empty;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(PW_Password_ReadOnly))]
		[Password]
		public override ZString PW_Password
		{
			get => base.PW_Password;
			set => base.PW_Password = value;
		}

		[List(nameof(Lookups) + "." + nameof(GuaranteeLookups.ReferenceNumbers))]
		[ReadOnlyMember(nameof(PW_BondNumber_ReadOnly))]
		public override ZString PW_BondNumber
		{
			get => base.PW_BondNumber;
			set
			{
				var oldValue = PW_BondNumber;
				base.PW_BondNumber = value;

				if (!IsCopying && PW_BondNumber != oldValue)
				{
					UpdatePassword();
				}
			}
		}

		[ReadOnlyMember(nameof(PW_BondNumber2_ReadOnly))]
		public override ZString PW_BondNumber2
		{
			get => base.PW_BondNumber2;
			set => base.PW_BondNumber2 = value;
		}

		bool PW_BondNumber2_ReadOnly => !PW_BondType.In(new ZString[] { NctsGuaranteeTypeList.Codes._8, NctsGuaranteeTypeList.Codes.R });

		bool PW_BondNumber_ReadOnly => PW_BondNumberAndPW_PasswordShouldBeEmpty || (SingleCusGuaranteeHeaderFilteredByReferenceNumber != null && PW_BondType != NctsGuaranteeTypeList.Codes._0);

		bool PW_Password_ReadOnly => PW_BondNumberAndPW_PasswordShouldBeEmpty || (SingleCusGuaranteeHeaderFilteredByReferenceNumber != null && PW_BondType != NctsGuaranteeTypeList.Codes._0 && Lookups.AccessCodeList.Count <= 1);

		bool PW_BondNumberAndPW_PasswordShouldBeEmpty => !PW_BondType.In(new ZString[] { NctsGuaranteeTypeList.Codes._0, NctsGuaranteeTypeList.Codes._1, NctsGuaranteeTypeList.Codes._2, NctsGuaranteeTypeList.Codes._4 });

		protected override bool CopyCustomsOfficeFromGuaranteeHeader => false;

		protected override ZBool PW_RX_NKCurrencyReadOnly => base.PW_RX_NKCurrencyReadOnly && PW_RX_NKCurrency != ZString.Empty;

		public override ZString AccessCodeFieldType => Lookups.AccessCodeList.Count > 1 ? nameof(FieldType.TextDropEdit) : nameof(FieldType.Text);

		public bool IsDropdownForReferenceNumberAndCode => PW_BondType.In(new ZString[] { NctsGuaranteeTypeList.Codes._0, NctsGuaranteeTypeList.Codes._1, NctsGuaranteeTypeList.Codes._2 });

		BaseCusGuaranteeHeader SingleCusGuaranteeHeader => IsDropdownForReferenceNumberAndCode ? Lookups.SingleReferenceNumber : null;

		BaseCusGuaranteeHeader SingleCusGuaranteeHeaderFilteredByReferenceNumber
		{
			get
			{
				return Factory.GetCachedValue($"DE.NCTS.Business.Guarantee.SingleCusGuaranteeHeaderFilteredByReferenceNumber_{PW_BondType}_{PW_BondNumber}", () =>
				{
					var guarantees = new CusGuaranteeHeaderCollection(Factory, new ZString[] { Core.Constants.CountryCodes.Germany }, new ZString[] { EUGuaranteeTypeList.Codes.TRA });
					guarantees.AdditionalFilter.AddToFilter(CusPermitHeaderSchema.CPH_SubType, PW_BondType);
					guarantees.AdditionalFilter.AddToFilter(CusPermitHeaderSchema.CPH_Number, PW_BondNumber);
					return guarantees.Count == 1 ? guarantees[0] : null;
				});
			}
		}

		void UpdateBondNumber()
		{
			PW_BondNumber = PW_BondNumberAndPW_PasswordShouldBeEmpty || SingleCusGuaranteeHeader is null ||
				SingleCusGuaranteeHeader.CPH_OH_PermitHolder != NctsHeader.Principal.OrganisationPK
				? ZString.Empty
				: SingleCusGuaranteeHeader.CPH_Number;
		}

		void UpdatePassword()
		{
			PW_Password = PW_BondNumberAndPW_PasswordShouldBeEmpty || SingleCusGuaranteeHeaderFilteredByReferenceNumber is null || SingleCusGuaranteeHeaderFilteredByReferenceNumber.AdditionalAccessCodes.Count != 1
				? ZString.Empty
				: SingleCusGuaranteeHeaderFilteredByReferenceNumber.AdditionalAccessCodes[0].CPR_ValueFrom.Left(Schema.PW_PasswordMaxLength);
		}

		#endregion

		#region Apportionment

		protected override GuaranteeApportionmentType GetApportionmentTypeCore()
		{
			switch (PW_BondType)
			{
				case NctsGuaranteeTypeList.Codes._0:
				case NctsGuaranteeTypeList.Codes._1:
					return GuaranteeApportionmentType.EqualShare;
				case NctsGuaranteeTypeList.Codes._4:
					return GuaranteeApportionmentType.Voucher;
				default:
					return GuaranteeApportionmentType.None;
			}
		}

		#endregion
	}
}
