using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.EConversation.Business
{
	public enum MessageType { LocalInternal, LocalPublished, Remote }
	public enum MessageSubType { UserMessage, SystemLog }

	public interface IConversationMessage : IBusiness
	{
		MessageType MessageType { get; }
		MessageSubType MessageSubType { get; }

		ZString Body { get; }
		ZString AdditionalNoteForDisplay { get; }
		ZDateTime SystemCreateTimeInUtc { get; }
		ZDateTime SendLocalDateTime { get; }

		ZString SenderDisplayName { get; }
		ZString SenderCode { get; }

		bool IsSystemMessage { get; }

		ZGuid Id { get; }

		bool IsRatingEnabled { get; }
		int Rating { get; }
		bool HasRatingChanged { get; }

		void Like();
		void Dislike();

		bool IsDeleted { get; }
	}
}
