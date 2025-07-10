using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageHeaderGuarantee : CommonGuarantee
	{
		public TemporaryStorageHeaderGuarantee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override TypeLoaderCollection ParentLoaders
		{
			get { return new TypeLoaderCollection(typeof(TemporaryStorageHeader)); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PW_RX_NKCurrency = "EUR";
			PW_ApplicationCode = ApplicationCodeList.Codes.Pentant;
		}

		[ReadOnlyMember(nameof(PW_BondAmount_ReadOnly))]
		public override ZDecimal PW_BondAmount { get => base.PW_BondAmount; set => base.PW_BondAmount = value; }

		[ReadOnly(true)]
		[MaxLength(3)]
		public override ZString PW_RX_NKCurrency { get => base.PW_RX_NKCurrency; set => base.PW_RX_NKCurrency = value; }

		public override ZBool PW_Override
		{
			get => base.PW_Override;
			set
			{
				var oldValue = PW_Override;
				base.PW_Override = value;
				if (!IsCopying && oldValue != value)
				{
					SetBondAmountWithLiabilityAmount();
				}
			}
		}

		protected bool PW_BondAmount_ReadOnly => !PW_Override;

		public ZDecimal LiabilityAmount => TemporaryStorageHeader.Bills?.SelectMany(b => b.PackedItems)?.Sum(x => x.LiabilityAmount) ?? ZDecimal.Zero;

		public void SetBondAmountWithLiabilityAmount()
		{
			if (!PW_Override)
			{
				PW_BondAmount = LiabilityAmount;
			}
		}

		public TemporaryStorageHeader TemporaryStorageHeader => Factory.Load<TemporaryStorageHeader>(PW_ParentID);

		public new TemporaryStorageHeaderGuaranteeLookups Lookups => (TemporaryStorageHeaderGuaranteeLookups)base.Lookups;

		protected override CusBondDetailLookups GetNewLookups() => new TemporaryStorageHeaderGuaranteeLookups(this);

		public override CusGuaranteeHeader CusGuarantee
		{
			get
			{
				CusGuaranteeHeader result = null;
				if (TemporaryStorageHeader != null)
				{
					var source = Factory.Load<CusGuaranteeHeader>(CusGuaranteeFilter);
					result = source.FirstOrDefault(CusGuaranteeTypeFilter);
				}

				return result;
			}
		}

		public CusGuaranteeHeader CusGuaranteeWithoutPermitHolder => cusGuaranteeWithoutPermitHolder ?? (cusGuaranteeWithoutPermitHolder = GetGuaranteeHeader());
		CusGuaranteeHeader cusGuaranteeWithoutPermitHolder;

		CusGuaranteeHeader GetGuaranteeHeader()
		{
			return Factory.Load<CusGuaranteeHeader>(LatestPermitHeaderMatchingBondNumberAndC00009CountryFilter).FirstOrDefault(CusGuaranteeTypeFilter);
		}

		internal ZQuery LatestPermitHeaderMatchingBondNumberAndC00009CountryFilter
		{
			get
			{
				var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
				query.AddToFilter(CusPermitHeaderSchema.CPH_Number, PW_BondNumber);
				var countryCodes = TemporaryStorageHeader?.GetC0009CountryCodes() ?? Array.Empty<ZString>();
				query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, countryCodes);
				query.OrderBy = CusPermitHeaderSchema.Constants.CPH_StartDate + OrderByClause.Descending + "," + CusPermitHeaderSchema.Constants.CPH_SystemCreateTimeUtc + OrderByClause.Descending;
				return query;
			}
		}

		public ZQuery CusGuaranteeFilter
		{
			get
			{
				var header = TemporaryStorageHeader;
				return header != null ? LatestPermitHeaderMatchingBondNumberAndC00009CountryFilter : LatestPermitHeaderMatchingBondNumberAndC00009CountryFilter.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, TemporaryStorageHeader?.Declarant?.OA_OH ?? ZGuid.Empty);
			}
		}

		Func<CusGuaranteeHeader, bool> CusGuaranteeTypeFilter => cusGuarantee => cusGuarantee.CPH_Type == EUGuaranteeTypeList.Codes.TST;
	}
}
