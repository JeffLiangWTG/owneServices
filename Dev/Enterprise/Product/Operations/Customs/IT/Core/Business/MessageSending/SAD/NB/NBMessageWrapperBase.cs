using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public abstract class NBMessageWrapperBase : INBMessageSendingObject
{
	protected NBMessageWrapperBase(INBWrappableBusinessObject nbObject)
	{
		this.nbObject = Argument.NotNull(nbObject, nameof(nbObject));
	}

	protected readonly INBWrappableBusinessObject nbObject;

	public abstract ZString AnnualProgressiveNumber { get; }

	public abstract INBHeader Header { get; }

	public IEnumerable<IPreviousOperationInfo> DataBlocks
	{
		get
		{
			foreach (GroupedPreviousDocument item in nbObject.NBGroupedPreviousDocuments)
			{
				yield return new NBPreviousOperationInfo(item);
			}
		}
	}
}
