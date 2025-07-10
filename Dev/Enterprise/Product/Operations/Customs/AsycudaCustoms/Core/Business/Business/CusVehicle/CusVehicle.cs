using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusVehicle : Customs.Business.CusVehicle, Integration.Customs.AsycudaCustoms.ICusVehicle
	{
		public CusVehicle(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusVehicleValidation Validation => (CusVehicleValidation)base.Validation;

		protected override Customs.Business.CusVehicleValidation GetNewValidation() => new CusVehicleValidation(this);

		protected override void PopulateDataModelIfNeededCore()
		{
			if (!IsInDatabase && CVH_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix)
			{
				// ASY is used the code used for unique index in CusVehicle to ensure AsycudaCustoms countries have single vehicle relationship with JobComInvoiceLine
				CVH_DataModel = "ASY";
			}
		}

		public override ZGuid CVH_ParentID
		{
			get => base.CVH_ParentID;
			set
			{
				var oldValue = CVH_ParentID;
				base.CVH_ParentID = value;
				if (oldValue != CVH_ParentID && !IsCopying)
				{
					Parent?.MarkAsNeedingValidation();
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZString CVH_ParentTableCode
		{
			get => base.CVH_ParentTableCode;
			set
			{
				var oldValue = CVH_ParentTableCode;
				base.CVH_ParentTableCode = value;
				if (oldValue != CVH_ParentTableCode && !IsCopying)
				{
					Parent?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString CVH_VehicleIdentificationNumber
		{
			get => base.CVH_VehicleIdentificationNumber;
			set
			{
				var oldValue = CVH_VehicleIdentificationNumber;
				base.CVH_VehicleIdentificationNumber = value;
				if (oldValue != CVH_VehicleIdentificationNumber && !IsCopying)
				{
					Parent?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZInt CVH_ClusterKey
		{
			get => base.CVH_ClusterKey;
			set
			{
				var oldValue = CVH_ClusterKey;
				base.CVH_ClusterKey = value;
				if (oldValue != CVH_ClusterKey && !IsCopying)
				{
					Parent?.MarkAsNeedingValidation();
				}
			}
		}
	}
}
