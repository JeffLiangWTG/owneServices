using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class Preference : ITPreference
{
	public Preference(CusEntryLine entryLine)
	{
		Argument.NotNull(entryLine, CusEntryLine.Schema.TableName);
		this.entryLine = entryLine;
	}

	readonly CusEntryLine entryLine;

	public ZString Preference1 { get => entryLine.RandomLine.JI_PrimaryPreference.Left(1); }
	public ZString Preference2 { get => entryLine.RandomLine.JI_PrimaryPreference.Right(2); }
}
