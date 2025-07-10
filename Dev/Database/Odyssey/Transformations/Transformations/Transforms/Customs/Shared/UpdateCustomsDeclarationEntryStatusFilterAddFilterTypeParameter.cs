using System.Xml.Linq;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;

public class UpdateCustomsDeclarationEntryStatusFilterAddFilterTypeParameter : UpdateFilterAddArgumentTransformation
{
	public override string UserDescription => "Update 'Customs Entry Status' Filters for the Forwarding Shipment module with the 'Any' Argument which has now been added to the filter.";
	protected override string FilterDescription => "Entry Status";
	protected override string ModuleId => "CusDec";
	protected override XElement ParameterToAdd => new XElement("FilterType", "Any");
}
