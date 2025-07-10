using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class GlobalChargeCodeMapPivotIntercompany : GlobalChargeCodeMapPivot, IGlobalChargeCodeMapPivotIntercompany
	{
		public GlobalChargeCodeMapPivotIntercompany(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override AccGlobalChargeCodeMapPivotValidation GetNewValidation()
		{
			return new GlobalChargeCodeMapPivotIntercompanyValidation(this);
		}

		#region Properties

		[RelatedBusinessObject("GlobalChargeCodeMap")]
		[List("Lookups.GlobalChargeCodeMapsIntercompany")]
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

		#endregion

		public GlobalChargeCodeMapIntercompany GlobalChargeCodeMap
		{
			get { return Factory.Load<GlobalChargeCodeMapIntercompany>(YP_YG); }
		}

		GlobalChargeCodeMapPivotIntercompanyCollection fParentCollection;
		public GlobalChargeCodeMapPivotIntercompanyCollection ParentCollection
		{
			get
			{
				if (fParentCollection == null)
				{
					foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
					{
						GlobalChargeCodeMapPivotIntercompanyCollection invoiceBatchCollection = parentCollection as GlobalChargeCodeMapPivotIntercompanyCollection;
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

		public ZBool SupportLocalClientOverride
		{
			get;
			set;
		}

		public OrgHeader JobLocalClient
		{
			get
			{
				return Factory.Load<OrgHeader>(YP_OH_LocalClientOverride);
			}
		}

		#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			YP_YG = globalChargeCode.PK;
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
	}
}

