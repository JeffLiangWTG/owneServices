using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.GUI;

sealed class EntryDetailsLayoutBuilder : CommonEntryDetailsLayoutBuilder<JobDeclaration>
{
	protected override int MaxColumns => 3;
}
