using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.Shared
{
	public partial class MessageStatusList
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MessageStatusList(MultilingualString multilingualDescription)
			: this()
		{
			AddDefaultPairs(multilingualDescription);
		}

		public static bool IsError(string code)
		{
			return code == Codes.ErrorChange ||
				code == Codes.ErrorDelete ||
				code == Codes.ErrorOriginal ||
				code == Codes.ErrorReplace;
		}

		public static bool IsAwaiting(string code)
		{
			return code == Codes.AwaitingChange ||
				code == Codes.AwaitingDelete ||
				code == Codes.AwaitingOriginal ||
				code == Codes.AwaitingReplace;
		}

		public static bool IsAcknowledged(string code)
		{
			return code == Codes.AcknowledgedChange ||
				code == Codes.AcknowledgedDelete ||
				code == Codes.AcknowledgedOriginal ||
				code == Codes.AcknowledgedReplace;
		}

		public static ZString GetCategoryForStatus(ZString messageStatus, CodeDescriptionPairList categories)
		{
			var result = MessageStatusCategoryList.Codes.Unknown;
			if (messageStatus.Length == 0)
			{
				result = MessageStatusCategoryList.Codes.NotSent;
			}
			else
			{
				var code = messageStatus.SubstringSafe(0, 2);
				if (categories.ContainsCode(code))
				{
					result = code;
				}
			}
			return result;
		}

		protected virtual void AddDefaultPairs(MultilingualString multilingualDescription)
		{
			if (multilingualDescription == null || multilingualDescription.IsEmpty)
			{
				return;
			}

			AddOverwriteIfExists(new CodeDescriptionPair(Codes.AcknowledgedChange, ResString.GetMultilingualString("MessageStatusList|AcknowledgedChange1", "Acknowledged {0} Change", multilingualDescription)));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.AcknowledgedDelete, ResString.GetMultilingualString("MessageStatusList|AcknowledgedDelete1", "Acknowledged {0} Cancel", multilingualDescription)));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.AcknowledgedOriginal, ResString.GetMultilingualString("MessageStatusList|AcknowledgedOriginal1", "Acknowledged {0} Original", multilingualDescription)));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.AcknowledgedReplace, ResString.GetMultilingualString("MessageStatusList|AcknowledgedReplace1", "Acknowledged {0} Replace", multilingualDescription)));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.AwaitingChange, ResString.GetMultilingualString("MessageStatusList|AwaitingChange1", "Awaiting {0} Change", multilingualDescription)));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.AwaitingDelete, ResString.GetMultilingualString("MessageStatusList|AwaitingDelete1", "Awaiting {0} Cancel", multilingualDescription)));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.AwaitingOriginal, ResString.GetMultilingualString("MessageStatusList|AwaitingOriginal1", "Awaiting {0} Original", multilingualDescription)));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.AwaitingReplace, ResString.GetMultilingualString("MessageStatusList|AwaitingReplace1", "Awaiting {0} Replace", multilingualDescription)));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.ClearChange, ResString.GetMultilingualString("MessageStatusList|ClearChange1", "Accepted {0} Change", multilingualDescription)));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.ClearDelete, ResString.GetMultilingualString("MessageStatusList|ClearDelete1", "Accepted {0} Cancel", multilingualDescription)));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.ClearOriginal, ResString.GetMultilingualString("MessageStatusList|ClearOriginal1", "Accepted {0} Original", multilingualDescription)));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.ClearReplace, ResString.GetMultilingualString("MessageStatusList|ClearReplace1", "Accepted {0} Replace", multilingualDescription)));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.ErrorChange, ResString.GetMultilingualString("MessageStatusList|ErrorChange1", "Error {0} Change", multilingualDescription)));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.ErrorDelete, ResString.GetMultilingualString("MessageStatusList|ErrorDelete1", "Error {0} Cancel", multilingualDescription)));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.ErrorOriginal, ResString.GetMultilingualString("MessageStatusList|ErrorOriginal1", "Error {0} Original", multilingualDescription)));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.ErrorReplace, ResString.GetMultilingualString("MessageStatusList|ErrorReplace1", "Error {0} Replace", multilingualDescription)));

			Sort();
		}
	}
}
