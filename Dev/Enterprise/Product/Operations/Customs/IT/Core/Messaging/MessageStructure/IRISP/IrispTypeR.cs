using System.Collections.Generic;

namespace Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;

public class IrispTypeR : UnifiedDeclarationIrisp
{
	public IEnumerable<SadPositiveResponseMessage> GetSadPositiveResponseMessages()
		=> GetPositiveResponseMessagesOfType<SadPositiveResponseMessage>();

	public IEnumerable<SadNegativeResponseMessage> GetSadNegativeResponseMessages()
		=> GetNegativeResponseMessagesOfType<SadNegativeResponseMessage>();

	public IEnumerable<SadNbPositiveResponseMessage> GetNbPositiveResponseMessages()
		=> GetPositiveResponseMessagesOfType<SadNbPositiveResponseMessage>();

	public IEnumerable<SadNbNegativeResponseMessage> GetNbNegativeResponseMessages()
		=> GetNegativeResponseMessagesOfType<SadNbNegativeResponseMessage>();
}
