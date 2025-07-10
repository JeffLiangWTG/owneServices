using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class NBDataBlock
{
	public NBDataBlock(IEnumerable<IPreviousOperationInfo> nBDataBlocks, ZBool continuation)
	{
		this.nBDataBlocks = Argument.NotNull(nBDataBlocks, nameof(nBDataBlocks));
		Continuation = continuation;
	}

	readonly IEnumerable<IPreviousOperationInfo> nBDataBlocks;

	[MessageLayout(Order = 0)]
	public NBPreviousOperationInfo Operation1 => GetPreviousOperationInfo(0);

	[MessageLayout(Order = 1)]
	public NBPreviousOperationInfo Operation2 => GetPreviousOperationInfo(1);

	[MessageLayout(Order = 2)]
	public NBPreviousOperationInfo Operation3 => GetPreviousOperationInfo(2);

	[MessageLayout(Order = 3)]
	public NBPreviousOperationInfo Operation4 => GetPreviousOperationInfo(3);

	[MessageLayout(Order = 4)]
	[MessageFieldRules("O")]
	[MessageFieldBoolRepresentation()]
	public ZBool Continuation { get; }

	NBPreviousOperationInfo GetPreviousOperationInfo(int index) => index < nBDataBlocks.Count() ? new NBPreviousOperationInfo(nBDataBlocks.ElementAt(index)) : new NBPreviousOperationInfo();
}
