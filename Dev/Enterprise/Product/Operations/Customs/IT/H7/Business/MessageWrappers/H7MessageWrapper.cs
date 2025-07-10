using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;

namespace Enterprise.Customs.IT.H7.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
public sealed class H7MessageWrapper : IH7Message
{
	public H7MessageWrapper(AsycudaBill bill)
	{
		this.bill = Argument.NotNull(bill, nameof(bill));
	}

	readonly AsycudaBill bill;

	#region IH7Message

	IH7Header IH7Message.Header => header ?? (header = new H7HeaderWrapper(bill));

	IH7Header header;

	IReadOnlyCollection<IH7Item> IH7Message.Items => items ?? (items = GetItems());

	IReadOnlyCollection<IH7Item> items;

	#endregion

	IReadOnlyCollection<IH7Item> GetItems()
	{
		return bill.PackedItems
			.Cast<AsycudaPackedItem>()
			.Select(pi => new H7ItemWrapper(pi))
			.Cast<IH7Item>()
			.ToList()
			.AsReadOnly();
	}
}
