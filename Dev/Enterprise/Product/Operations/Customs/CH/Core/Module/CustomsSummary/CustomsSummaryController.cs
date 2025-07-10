using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.CH.Module;

public sealed class CustomsSummaryController : Customs.Module.StatementController
{
	public CustomsSummaryController()
	{
	}

	public override ControllerID ID => ControllerIDs.Customs.CH.CustomsSummary;

	public override ModuleIdentifier ModuleID => ModuleIDs.Customs.CH.CustomsSummary;

	public override Type TypeOfTopLevelBusinessObject => typeof(CustomsSummaryLine);

	protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => throw new NotSupportedException("No PlugIns");

	protected override IZForm GetForm(IBusiness businessEntity) => businessEntity switch
	{
		JobDeclaration declaration => new GUI.JobDeclarationForm(declaration),
		ForwardingShipment shipment => new ShipmentForm(shipment),
		_ => base.GetForm(businessEntity)
	};

	protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
	{
		if (sourceEntity is not CustomsSummaryLine summaryLine)
		{
			return base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
		}

		var parent = Factory.LoadTop1<CusEntryNumber>(CusEntryNumberHelper.GetEntryNumberQueryForHeaderOrShipment(summaryLine.B3_EntryNum))?.Parent;
		switch (parent)
		{
			case CusEntryHeader header:
				parent = header.Declaration;
				break;
			case ForwardingShipment:
				ChildEditableService.SetState(parent.Factory, ChildEditableServiceStates.Shipment);
				break;
		}
		return parent;
	}

	protected override string AlreadyDeletedOrIrreversiblyChangedMessageCore => Res.GetString("24D94D81-07D7-415C-93B9-47EB14B7E42F", "No declaration or shipment found for the selected entry.");
}
