using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using static Enterprise.Customs.IT.Business.SADConstants;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class PackingSynchroniser : EuPackingSynchroniser
{
	public PackingSynchroniser(EU.Business.Declaration.JobDeclarationSynchroniser parentSynchroniser, BaseJobDeclaration declaration) : base(parentSynchroniser, declaration)
	{
	}

	protected override ZString GetConvertedMarksAndNumbers(ZString marksAndNumbers)
	{
		return marksAndNumbers.Left(CustomsFieldMaxLength.EntryLine.MarksAndNumbers);
	}
}
