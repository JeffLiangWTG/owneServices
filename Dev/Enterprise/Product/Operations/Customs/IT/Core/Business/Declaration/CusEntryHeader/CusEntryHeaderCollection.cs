using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryHeaderCollection : CusEntryHeaderCollection<CusEntryHeader>
{
	public CusEntryHeaderCollection(JobDeclaration parentBO, BusinessObjectFactory factory)
		: base(parentBO, factory)
	{
	}

	public override bool HasAnyEntryWhichMessagesCannotBeChangedCore => this.Cast<CusEntryHeader>().Any(x => x.IsEntryLockedForEditing);
}
