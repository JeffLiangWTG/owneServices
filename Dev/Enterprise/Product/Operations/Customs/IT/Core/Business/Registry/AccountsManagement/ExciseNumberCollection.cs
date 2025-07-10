using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Registry;

public class ExciseNumberCollection : NonPersistentBusinessObjectCollection<ExciseNumber>
{
	public ExciseNumberCollection(Account account, BusinessObjectFactory factory) : base(factory)
	{
		Account = Argument.NotNull(account, nameof(account));
		Argument.NotNull(factory, nameof(factory));
	}

	public Account Account { get; }

	public ZBool ContainsExciseNumber(ZString exciseNumber) => this.Cast<ExciseNumber>().Any(x => x.Number == exciseNumber);

	protected override BusinessObject CreateNonPersistentBusinessObject() => new ExciseNumber(Account, Factory);
}
