using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageContracts;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE456;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessagePacking;

class IE456MessageUnpackRule : IMessageUnpackRule
{
	ZString IMessageUnpackRule.MessageSubType => DeltaIEResponseMessageSubTypeList.Codes.FunctionalRejection;

	IEnumerable<ZString> IMessageUnpackRule.UnpackMessage(ZString messageJsonText)
	{
		if (JsonHelper.DeserializeMessage<CC456BType>(messageJsonText) is var messageObject456
			&& messageObject456.ImportOperation is var importOperation and { Count: > 1 })
		{
			var importOperationCopiedList = importOperation.ToList();
			foreach (var op in importOperationCopiedList)
			{
				importOperation.Clear();
				importOperation.Add(op);
				var singleLRNMessageText = JsonHelper.SerializeMessage(messageObject456);
				yield return singleLRNMessageText;
			}
		}
		else
		{
			yield return messageJsonText;
		}
	}
}
