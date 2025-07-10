using System.Xml.Linq;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.Shared;

[TestedType(typeof(UpdateCustomsDeclarationEntryStatusFilterAddFilterTypeParameter))]
public class UpdateCustomsDeclarationEntryStatusFilterAddFilterTypeParameterTest : UpdateFilterAddArgumentTransformationAbstractTest
{
	protected override DataTransformation GetNewTestTransformationInstance() => new UpdateCustomsDeclarationEntryStatusFilterAddFilterTypeParameter();

	protected override string ModuleToUpdate => "CusDec";
	protected override string OtherModule => "JobConsol";
	protected override string FilterDescriptionToUpdate => "Entry Status";
	protected override string OtherFilterDescription => "Created Time";
	protected override XElement ParameterToBeAdded => new XElement("FilterType", "Any");
}
