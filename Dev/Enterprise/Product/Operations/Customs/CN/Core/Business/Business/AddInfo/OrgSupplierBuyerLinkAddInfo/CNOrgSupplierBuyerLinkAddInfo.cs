using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CNOrgSupplierBuyerLinkAddInfo : NonPersistentBusinessObject
	{
		public CNOrgSupplierBuyerLinkAddInfo(OrgSupplierBuyerLinkAddInfo addInfoBO)
			: base(addInfoBO.Factory)
		{
			AddInfoBO = Argument.NotNull(addInfoBO, nameof(addInfoBO));
		}

		OrgSupplierBuyerLinkAddInfo AddInfoBO { get; }

		public override bool IsDeleted =>
			AddInfoBO?.Parent is not BusinessObject parentBizObj
			|| parentBizObj.IsDeleted
			|| base.IsDeleted;

		public ZGuid ParentPK => AddInfoBO.Parent.PK;

		OrgSupplierBuyerLink LinkBO => (OrgSupplierBuyerLink)AddInfoBO.Parent;

		public ZString ImporterCountry => LinkBO.OL_RN_NKImporterCountry;

		#region Properties

		[List(nameof(Lookups) + "." + nameof(CNOrgSupplierBuyerLinkAddInfoLookups.ProcedureCodeList))]
		[MaxLength(4)]
		public ZString ZO_ProcedureCode
		{
			get => AddInfoBO.ZO_ProcedureCode;
			set
			{
				AddInfoBO.ZO_ProcedureCode = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateZO_ProcedureCode();
				}
				ZO_ProcedureCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ZO_ProcedureCodeInfo => GetZPropertyInfo(OrgSupplierBuyerLinkAddInfo.Schema.ZO_ProcedureCode);

		[List(nameof(Lookups) + "." + nameof(CNOrgSupplierBuyerLinkAddInfoLookups.LevyTypeList))]
		[MaxLength(3)]
		public ZString ZO_LevyType
		{
			get => AddInfoBO.ZO_LevyType;
			set
			{
				AddInfoBO.ZO_LevyType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateZO_LevyType();
				}
				ZO_LevyTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ZO_LevyTypeInfo => GetZPropertyInfo(OrgSupplierBuyerLinkAddInfo.Schema.ZO_LevyType);

		[MaxLength(12)]
		public ZString ZO_ManualNo
		{
			get => AddInfoBO.ZO_ManualNo;
			set
			{
				AddInfoBO.ZO_ManualNo = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateZO_ManualNo();
				}
				ZO_ManualNoInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ZO_ManualNoInfo => GetZPropertyInfo(OrgSupplierBuyerLinkAddInfo.Schema.ZO_ManualNo);

		#endregion

		#region Lookups

		public CNOrgSupplierBuyerLinkAddInfoLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = new CNOrgSupplierBuyerLinkAddInfoLookups(this);
				}

				return fLookups;
			}
		}

		CNOrgSupplierBuyerLinkAddInfoLookups fLookups;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public CNOrgSupplierBuyerLinkAddInfoValidation Validation => new CNOrgSupplierBuyerLinkAddInfoValidation(this);

		#endregion
	}
}
