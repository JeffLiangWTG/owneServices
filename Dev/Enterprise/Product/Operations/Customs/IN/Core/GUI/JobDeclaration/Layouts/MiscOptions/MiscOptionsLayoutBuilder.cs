using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;

namespace Enterprise.Customs.IN.GUI;

public class MiscOptionsLayoutBuilder : CommonMiscOptionsLayoutBuilder<JobDeclaration>
{
	protected override int MaxColumns => 2;
}
