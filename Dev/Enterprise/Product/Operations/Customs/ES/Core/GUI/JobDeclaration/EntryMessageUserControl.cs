using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.ES.GUI;
public partial class EntryMessageUserControl : EU.GUI.EntryMessageUserControl
{
	public EntryMessageUserControl()
	{
	}

	protected override BaseCustomsEntryUserControl GetMessageUserControl()
	{
		BaseCustomsEntryUserControl result;
		if (JobDeclaration.IsExport)
		{
			result = new ExportMessageUserControl();
		}
		else
		{
			result = new ImportMessageUserControl((JobDeclaration)JobDeclaration);
		}
		return result;
	}
	protected override void RefreshControlVisibiltyOnShown()
	{
		(fMessageUserControl as MessageUserControl)?.RefreshTabVisibilty();
	}
}
