using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;

public abstract class UnifiedDeclarationIrisp : Irisp
{
	public IdocElaborationInfoServiceRecord IdocElaborationInfo { get; protected set; }
	public UnifiedDeclarationControlServiceRecord ControlResult { get; protected set; }
	public IList<UnifiedDeclarationResponseMessage> ResponseMessages { get; protected set; }

	protected IEnumerable<T> GetPositiveResponseMessagesOfType<T>() where T : UnifiedDeclarationPositiveResponseMessage
		=> ResponseMessages.OfType<T>();

	protected IEnumerable<T> GetNegativeResponseMessagesOfType<T>() where T : UnifiedDeclarationNegativeResponseMessage
		=> ResponseMessages.OfType<T>();

	public ZBool IsPositive => ResponseMessages.All(x => x is UnifiedDeclarationPositiveResponseMessage);
	public ZBool IsPartialPositive => !IsPositive && ResponseMessages.Any(x => x is UnifiedDeclarationPositiveResponseMessage);
	public ZBool IsNegative => ResponseMessages.All(x => x is UnifiedDeclarationNegativeResponseMessage);

	public UnifiedDeclarationIrispPartsAggregator Aggregator => aggregator ?? (aggregator = new UnifiedDeclarationIrispPartsAggregator(this));
	UnifiedDeclarationIrispPartsAggregator aggregator;

	protected override void Load(ZString content)
	{
		base.Load(content);

		IdocElaborationInfo = new IdocElaborationInfoServiceRecord();
		IdocElaborationInfo.Load(Lines.ElementAt(1));

		ControlResult = new UnifiedDeclarationControlServiceRecord();
		ControlResult.Load(Lines.ElementAt(2));

		ResponseMessages = new List<UnifiedDeclarationResponseMessage>();
		var responseMessageBlocks = Regex.Split(content, @"(?=ESEGUITO)").Skip(1);
		for (int i = 0; i < responseMessageBlocks.Count(); i++)
		{
			var responseMessageBlock = responseMessageBlocks.ElementAt(i);
			var responseMessage = GetAndLoadResponseMessage(responseMessageBlock);
			ResponseMessages.Add(responseMessage);
		}
	}

	UnifiedDeclarationResponseMessage GetAndLoadResponseMessage(ZString responseMessageBlock)
	{
		var responseMessage = UnifiedDeclarationResponseMessage.New(responseMessageBlock.SplitByNewLine().ElementAt(1));
		responseMessage.Load(responseMessageBlock);
		return responseMessage;
	}
}
