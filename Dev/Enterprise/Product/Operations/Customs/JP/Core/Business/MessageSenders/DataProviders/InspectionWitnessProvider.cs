using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;

namespace Enterprise.Customs.JP.Business;

public class InspectionWitnessProvider : IJapaneseAddress
{
	public InspectionWitnessProvider(JobDeclaration declaration)
	{
		Argument.NotNull(declaration, nameof(declaration));
		this.declaration = declaration;
	}

	readonly JobDeclaration declaration;

	public string Code => null;

	public string NACCSUserCode => declaration.InspectionWitnessCode;

	public string Name => null;

	public string PostCode => null;

	public string Street => null;

	public string AdditionalInformation => null;

	public string City => null;

	public string Prefecture => null;

	public string Phone => null;
}

