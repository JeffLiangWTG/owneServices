using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;

public abstract class UnifiedDeclarationResponseMessage
{
	public ZString InnerText { get; protected set; }
	public ZDateTime IdocElaborationDateTime { get; protected set; }
	public ZString RecordType { get; protected set; }
	public ZString MessageCode { get; protected set; }
	public ZString DeclarationNumber { get; protected set; }
	public ZInt MessageProgressiveNumber { get; protected set; }
	public ZString CustomsOffice { get; protected set; }
	public ZString OperationResult { get; protected set; }

	protected IEnumerable<ZString> Lines { get; private set; }

	internal virtual void Load(ZString content)
	{
		InnerText = content;
		Lines = InnerText.SplitByNewLine();
		IdocElaborationDateTime = Lines.ElementAt(0).SubStringAndTrim(11, 15).ParseToDateTimeWithFormat((NoResString)"dd/MM/yy  HH:mm");
		var line1 = Lines.ElementAt(1);
		RecordType = line1.SubStringAndTrim(0, 1);
		MessageCode = line1.SubStringAndTrim(MessageCodeFieldStartIndex, MessageCodeFieldLength);
		DeclarationNumber = line1.SubStringAndTrim(13, 6);
		MessageProgressiveNumber = ZInt.Parse(line1.SubStringAndTrim(19, 2));
		CustomsOffice = line1.SubStringAndTrim(21, 6);
		OperationResult = line1.SubStringAndTrim(OperationResultFieldStartIndex, OperationResultFieldLength);
	}

	public static UnifiedDeclarationResponseMessage New(ZString responseMessageBlockLine)
	{
		Argument.NotNullOrEmpty(responseMessageBlockLine, nameof(responseMessageBlockLine));
		CheckResponseMessageBlockLineLength(responseMessageBlockLine);

		var messageCode = responseMessageBlockLine.SubStringAndTrim(MessageCodeFieldStartIndex, MessageCodeFieldLength);
		var operationResult = responseMessageBlockLine.SubStringAndTrim(OperationResultFieldStartIndex, OperationResultFieldLength);
		var isCorrelationFound = responseCorrelationsDictionary.TryGetValue((messageCode, operationResult), out var getResponseMessageDelegate);
		return isCorrelationFound ? getResponseMessageDelegate() : null;
	}

	#region Implementation

	const int MessageCodeFieldStartIndex = 1;
	const int MessageCodeFieldLength = 8;
	const int OperationResultFieldStartIndex = 27;
	const int OperationResultFieldLength = 1;

	static readonly ImmutableDictionary<(ZString MessageCode, ZString OperationResult), Func<UnifiedDeclarationResponseMessage>> responseCorrelationsDictionary = LoadResponseCorrelationsDictionary();

	static ImmutableDictionary<(ZString MessageCode, ZString OperationResult), Func<UnifiedDeclarationResponseMessage>> LoadResponseCorrelationsDictionary()
	{
		var correlations = new Dictionary<(ZString MessageCode, ZString OperationResult), Func<UnifiedDeclarationResponseMessage>>();
		correlations.Add((IrispConstants.MessageTypes.IM, IrispConstants.OperationResults.P), () => new SadPositiveResponseMessage());
		correlations.Add((IrispConstants.MessageTypes.IM, IrispConstants.OperationResults.N), () => new SadNegativeResponseMessage());
		correlations.Add((IrispConstants.MessageTypes.ET, IrispConstants.OperationResults.P), () => new SadPositiveResponseMessage());
		correlations.Add((IrispConstants.MessageTypes.ET, IrispConstants.OperationResults.N), () => new SadNegativeResponseMessage());
		correlations.Add((IrispConstants.MessageTypes.NB, IrispConstants.OperationResults.P), () => new SadNbPositiveResponseMessage());
		correlations.Add((IrispConstants.MessageTypes.NB, IrispConstants.OperationResults.N), () => new SadNbNegativeResponseMessage());
		return correlations.ToImmutableDictionary();
	}

	static void CheckResponseMessageBlockLineLength(ZString responseMessageBlockLine)
	{
		const int minimumLength = OperationResultFieldStartIndex + OperationResultFieldLength;
		if (responseMessageBlockLine.Length < minimumLength)
		{
			throw new ArgumentException($"{nameof(responseMessageBlockLine)} must be at least {minimumLength} characters length");
		}
	}

	#endregion
}
