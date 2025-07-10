using System.Xml.Linq;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;

public class UpdateShipmentCustomsEntryStatusFilterAddFilterTypeParameter : UpdateFilterAddArgumentTransformation
{
	public UpdateShipmentCustomsEntryStatusFilterAddFilterTypeParameter()
	{
	}

#if DEBUG
	public UpdateShipmentCustomsEntryStatusFilterAddFilterTypeParameter(short maxRecordsPerBatch) : base(maxRecordsPerBatch)
	{
	}
#endif

	public override string UserDescription => "Update 'Customs Entry Status' Filters for the Forwarding Shipment module with the 'Any' Argument which has now been added to the filter.";
	protected override string FilterDescription => "Customs Entry Status";
	protected override string ModuleId => "JobShipment";
	protected override XElement ParameterToAdd => new XElement("FilterType", "Any");
}
