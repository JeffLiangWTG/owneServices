using System.Xml.Linq;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.Shared;

[TestedType(typeof(UpdateShipmentCustomsEntryStatusFilterAddFilterTypeParameter))]
public class UpdateShipmentCustomsEntryStatusFilterAddFilterTypeParameterTest : UpdateFilterAddArgumentTransformationAbstractTest
{
	protected override DataTransformation GetNewTestTransformationInstance() => new UpdateShipmentCustomsEntryStatusFilterAddFilterTypeParameter(2);

	protected override string ModuleToUpdate => "JobShipment";
	protected override string OtherModule => "CusDec";
	protected override string FilterDescriptionToUpdate => "Customs Entry Status";
	protected override string OtherFilterDescription => "Created Time";
	protected override XElement ParameterToBeAdded => new XElement("FilterType", "Any");
}
