using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using Enterprise.Customs.IT.Messaging.MessageStructure;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class NBMessageHeader : IMessageHeader
{
	public NBMessageHeader(INBHeader nBHeader, IEnumerable<IPreviousOperationInfo> nBDataBlocks, ZString annualProgressiveNumber, ZInt progressiveNumber, ISadMessageSendingObject sadMessageSendingObject)
	{
		this.nBHeader = Argument.NotNull(nBHeader, nameof(nBHeader));
		this.nBDataBlocks = Argument.NotNull(nBDataBlocks, nameof(nBDataBlocks));
		this.annualProgressiveNumber = Argument.NotNullOrEmpty(annualProgressiveNumber, nameof(annualProgressiveNumber));
		this.progressiveNumber = Argument.GreaterThanOrEqualToZero(progressiveNumber, nameof(progressiveNumber));
		this.sadMessageSendingObject = Argument.NotNull(sadMessageSendingObject, nameof(sadMessageSendingObject));
	}

	readonly INBHeader nBHeader;
	readonly IEnumerable<IPreviousOperationInfo> nBDataBlocks;
	readonly ZString annualProgressiveNumber;
	readonly ZInt progressiveNumber;
	readonly ISadMessageSendingObject sadMessageSendingObject;

	[MessageLayout(Order = 0)]
	public ISadMessageFixedPart FixedPart => SadMessageFixedPartFactory.GetSadMessageFixedPart
		(isHeader: true
		, sadMessageSendingObject: sadMessageSendingObject
		, messageCode: SadMessageFixedPart.Constants.MessageCode.NB.Header
		, annualProgressiveNumber: annualProgressiveNumber
		, progressiveNumber: progressiveNumber);

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, false)]
	[MessageFieldRules("R")]
	public ZString MessageCodeEntry => nBHeader.MessageCodeEntry;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, false)]
	[MessageFieldRules("R")]
	public ZString ReferenceNumber => nBHeader.ReferenceNumber;

	[MessageLayout(Order = 3)]
	[MessageFieldRules("O")]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	public ZString DeclarationCIN => nBHeader.DeclarationCIN;

	[MessageLayout(Order = 4)]
	[MessageFieldRules("O")]
	[MessageFieldDateDDMMYYRepresentation]
	public ZDate DeclarationDate => nBHeader.DeclarationDate;

	[MessageLayout(Order = 5)]
	[MessageFieldIntegerRepresentation(3, false)]
	[MessageFieldRules("R")]
	public ZInt Number => nBHeader.ItemNumber;

	[MessageLayout(Order = 6)]
	public NBDataBlock HeaderBlock => new NBDataBlock(nBDataBlocks.Take(NBMessage.MaxIterationNumberEachRow), nBDataBlocks.Count() > NBMessage.MaxIterationNumberEachRow);
}
