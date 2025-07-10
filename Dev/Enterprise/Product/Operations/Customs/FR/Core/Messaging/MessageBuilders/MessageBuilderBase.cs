using CargoWise.Customs.FR.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders
{
	public enum TransactionTypes { Unknow = 0, Original = 1 }
	public abstract class MessageBuilderBase<T> : IMessageBuilderBase
	{
		protected MessageBuilderBase()
		{
		}

		protected abstract T GenerateDeclarationMessage();

		public string GetMessage()
		{
			T itemMessage = GenerateDeclarationMessage();

			return itemMessage == null ? new ZString("") : Extensions.Serialize(itemMessage);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Not translatable")]
		public const string CorrelationidPlaceholder = "~CORRELATIONID~";
	}

	public static class MessageBuilderBaseExtension
	{
		public readonly static int NumberOfCharactersLongText = 260;
		public readonly static int NumberOfCharactersShortText = 34;
		public readonly static int NumberOfCharactersAddCode = 5;
		public readonly static string StringNull;
	}
}
