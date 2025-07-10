using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class GlobalChargeCodeMapPivotOrganization : GlobalChargeCodeMapPivot
	{
		public GlobalChargeCodeMapPivotOrganization(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override AccGlobalChargeCodeMapPivotValidation GetNewValidation()
		{
			return new GlobalChargeCodeMapPivotOrganizationValidation(this);
		}

		#region Properties

		[RelatedBusinessObject("GlobalChargeCodeMap")]
		[List("Lookups.GlobalChargeCodeMapsOrganization")]
		public override ZGuid YP_YG
		{
			get
			{
				return base.YP_YG;
			}
			set
			{
				base.YP_YG = value;
			}
		}

		public GlobalChargeCodeMapOrganization GlobalChargeCodeMap
		{
			get { return Factory.Load<GlobalChargeCodeMapOrganization>(YP_YG); }
		}

		GlobalChargeCodeMapPivotOrganizationCollection fParentCollection;
		public GlobalChargeCodeMapPivotOrganizationCollection ParentCollection
		{
			get
			{
				if (fParentCollection == null)
				{
					foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
					{
						GlobalChargeCodeMapPivotOrganizationCollection invoiceBatchCollection = parentCollection as GlobalChargeCodeMapPivotOrganizationCollection;
						if (invoiceBatchCollection != null)
						{
							fParentCollection = invoiceBatchCollection;
							break;
						}
					}
				}
				return fParentCollection;
			}
		}

		#endregion

		#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			GlobalChargeCodeMapOrganization globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			YP_YG = globalChargeCode.PK;
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
	}
}

