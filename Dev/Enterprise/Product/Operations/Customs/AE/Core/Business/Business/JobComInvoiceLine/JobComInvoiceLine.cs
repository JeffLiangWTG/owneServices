using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Business;

public class JobComInvoiceLine : TypeSafeJobComInvoiceLine, Integration.Customs.AE.IJobComInvoiceLine
{
	public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override bool IsGoingIntoBondedWarehouseCore => false;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		JI_InvoiceUQ = InvoiceUQList.Codes.Boxes;
		JI_NewUsed = AEGoodsConditionList.Codes.New;
	}

	[ResourceStringData("Enterprise.Customs.AE.Business.JobComInvoiceLine|JI_NewUsed", Caption = "Goods Condition")]
	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.GoodsConditionList))]
	public override ZString JI_NewUsed { get => base.JI_NewUsed; set => base.JI_NewUsed = value; }

	protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups() => new JobComInvoiceLineLookups(this);

	protected override ZString GetTariffDescription(ZString tariffCode) => ZString.Empty;

	protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.UnitedArabEmirates;

	protected override ZString DefaultDataGroupingForTariffsCore => InvoiceHeader?.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) ?? AEConstants.DefaultDataGroupingForTariffs;

	protected override System.Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

	#region Vehicles

	public new ICusVehicleCollection<CusVehicle, JobComInvoiceLine> Vehicles => (ICusVehicleCollection<CusVehicle, JobComInvoiceLine>)base.Vehicles;

	protected override ICusVehicleCollection<Customs.Business.CusVehicle, BaseJobComInvoiceLine> GetNewCusVehicleCollection() => new CusVehicleCollection<CusVehicle, JobComInvoiceLine>(this);

	public override VehicleRelationshipType VehicleRelationship => VehicleRelationshipType.Many;

	#endregion
}
