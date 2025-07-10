using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.MX.Business
{
	public class CusVehicle : Customs.Business.CusVehicle
	{
		public CusVehicle(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Customs.Business.CusVehicleValidation GetNewValidation() => new CusVehicleValidation(this);

		public new CusVehicleValidation Validation => (CusVehicleValidation)base.Validation;

		[ResourceStringData("Enterprise.Customs.MX.Business.CusVehicle|VehicleVIN", ShortCaption = "VIN", Caption = "VIN", FullDescription = "The Vehicle Identification Number or Serial Number.")]
		public override ZString CVH_SerialNumber
		{
			get => base.CVH_SerialNumber;
			set
			{
				var oldValue = CVH_SerialNumber;
				base.CVH_SerialNumber = value;

				if (!IsCopying && oldValue != CVH_SerialNumber)
				{
					Parent?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.MX.Business.CusVehicle|VehicleMileage", ShortCaption = "Mileage", Caption = "Mileage", FullDescription = "The vehicle Mileage.")]
		public override ZInt CVH_Mileage
		{
			get => base.CVH_Mileage;
			set
			{
				var oldValue = CVH_Mileage;
				base.CVH_Mileage = value;
				if (!IsCopying && oldValue != CVH_Mileage)
				{
					Parent?.MarkAsNeedingValidation();

					CVH_MileageUQ = CVH_Mileage > 0 ? (ZString)Core.Constants.Distance.Kilometres : ZString.Empty;
				}
			}
		}

		public override ZGuid CVH_ParentID
		{
			get => base.CVH_ParentID;
			set
			{
				var oldValue = CVH_ParentID;
				base.CVH_ParentID = value;
				if (!IsCopying && oldValue != CVH_ParentID)
				{
					Parent?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.MX.Business.CusVehicle|VehicleMileageUQ", ShortCaption = "Mileage UQ", Caption = "Mileage UQ", FullDescription = "The vehicle Mileage UQ.")]
		public override ZString CVH_MileageUQ
		{
			get => base.CVH_MileageUQ;
			set
			{
				var oldValue = CVH_MileageUQ;
				base.CVH_MileageUQ = value;
				if (!IsCopying && oldValue != CVH_MileageUQ)
				{
					Parent?.MarkAsNeedingValidation();
				}
			}
		}
	}
}
