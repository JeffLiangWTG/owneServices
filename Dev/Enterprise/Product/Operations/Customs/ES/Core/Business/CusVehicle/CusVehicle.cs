using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business;

public class CusVehicle : EU.Business.CusVehicle, Integration.Customs.ES.ICusVehicle
{
	public CusVehicle(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
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

	[ResourceStringData("EF18EDDF-BAC9-427F-8E29-4A5C594488AF", Caption = "Vehicle Identification Number", MediumCaption = "VIN", ShortCaption = "VIN", FullDescription = "The 17-digit number which universally identifies a vehicle")]
	[MaxLength(CusVehicle.Schema.CVH_VehicleIdentificationNumberMaxLength)]
	public override ZString CVH_VehicleIdentificationNumber
	{
		get => base.CVH_VehicleIdentificationNumber;
		set
		{
			var oldValue = CVH_VehicleIdentificationNumber;
			base.CVH_VehicleIdentificationNumber = value;

			if (value != oldValue)
			{
				if (!IsCopying)
				{
					InvoiceLine?.PopulateVINPartAttributeFromUZ_VIN();
					Parent?.MarkAsNeedingValidation();
				}
				CVH_VehicleIdentificationNumberInfo.RefreshBinding(oldValue);
			}
		}
	}

	[ResourceStringData("ES.Business.CVH_ModelName", Caption = "Model")]
	[MaxLength(ModelNameMaxLength)]
	public override ZString CVH_ModelName
	{
		get => base.CVH_ModelName;
		set
		{
			var oldValue = CVH_ModelName;
			base.CVH_ModelName = value;
			if (oldValue != CVH_ModelName && !IsCopying)
			{
				Parent?.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("ES.Business.CVH_BrandName", Caption = "Brand")]
	[MaxLength(BrandNameMaxLength)]
	public override ZString CVH_BrandName
	{
		get => base.CVH_BrandName;
		set
		{
			var oldValue = CVH_BrandName;
			base.CVH_BrandName = value;
			if (oldValue != CVH_BrandName && !IsCopying)
			{
				Parent?.MarkAsNeedingValidation();
			}
		}
	}

	protected override Customs.Business.CusVehicleValidation GetNewValidation() => new CusVehicleValidation(this);

	public JobComInvoiceLine InvoiceLine => Parent as JobComInvoiceLine;

	const int BrandNameMaxLength = 35;
	const int ModelNameMaxLength = 35;
}
