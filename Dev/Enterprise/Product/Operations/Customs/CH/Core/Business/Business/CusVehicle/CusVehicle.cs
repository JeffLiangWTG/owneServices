using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.CH.Business;

public class CusVehicle : Customs.Business.CusVehicle, Integration.Customs.CH.ICusVehicle
{
	public CusVehicle(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override Customs.Business.CusVehicleLookups GetNewLookups() => new CusVehicleLookups(this);

	public new CusVehicleLookups Lookups => (CusVehicleLookups)base.Lookups;

	protected override Customs.Business.CusVehicleValidation GetNewValidation() => new CusVehicleValidation(this);

	public new CusVehicleValidation Validation => (CusVehicleValidation)base.Validation;

	#region CVH_RegistrationNumber

	[MaxLength(9)]
	public override ZString CVH_RegistrationNumber
	{
		get => base.CVH_RegistrationNumber;
		set => base.CVH_RegistrationNumber = value;
	}

	#endregion

	#region CVH_ModelName

	[ResourceStringData("CH.Business.CusVehicle|CVH_ModelName", Caption = "Code")]
	[MaxLength(3)]
	[List(nameof(Lookups) + "." + nameof(CusVehicleLookups.ModelNameCodeList))]
	public override ZString CVH_ModelName
	{
		get => base.CVH_ModelName;
		set => base.CVH_ModelName = value;
	}

	#endregion

	public JobComInvoiceLine InvoiceLine => Parent as JobComInvoiceLine;
}
