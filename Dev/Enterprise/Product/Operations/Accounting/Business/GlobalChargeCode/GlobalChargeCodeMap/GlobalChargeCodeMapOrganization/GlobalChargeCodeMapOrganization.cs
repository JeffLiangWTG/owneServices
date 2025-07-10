using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class GlobalChargeCodeMapOrganization : GlobalChargeCodeMap
	{
		public GlobalChargeCodeMapOrganization(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override AccGlobalChargeCodeMapValidation GetNewValidation()
		{
			return new GlobalChargeCodeMapOrganizationValidation(this);
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new GlobalChargeCodeMapFetchStrategy(this);
		}

		#region Properties

		#region YG_APChargeCode

		[MaxLength(10)]
		public ZString YG_APChargeCode
		{
			get
			{
				if (fYG_APChargeCode == ZString.Empty)
				{
					fYG_APChargeCode = PivotCollection.GetCodeOfAPChargeCode();
				}
				return fYG_APChargeCode;
			}
		}

		ZString fYG_APChargeCode;

		public ZPropertyInfo YG_APChargeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.YG_APChargeCode); }
		}

		#endregion

		#region YG_ARChargeCodes

		[MaxLength(255)]
		public ZString YG_ARChargeCodes
		{
			get
			{
				if (fYG_ARChargeCodes == ZString.Empty)
				{
					fYG_ARChargeCodes = PivotCollection.GetCodesOfARChargeCodes();
				}
				return fYG_ARChargeCodes;
			}
		}

		ZString fYG_ARChargeCodes;

		public ZPropertyInfo YG_ARChargeCodesInfo
		{
			get { return GetZPropertyInfo(Schema.YG_ARChargeCodes); }
		}

		#endregion

		[ChildEditable]
		public GlobalChargeCodeMapPivotOrganizationCollection PivotCollection
		{
			get
			{
				if (fPivotCollection == null)
				{
					fPivotCollection = new GlobalChargeCodeMapPivotOrganizationCollection(Factory, PK);
					fPivotCollection.Load();
					RegisterEditableChildObject(fPivotCollection);
				}
				return fPivotCollection;
			}
		}

		GlobalChargeCodeMapPivotOrganizationCollection fPivotCollection;

		#endregion

		#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			YG_OH = org1.PK;
			if ((kind & TestBusinessObjectKind.PopulateDependentCollections) != 0)
			{
				org1.OH_Code = "CWG";
			}
		}

#endif
	}
}

