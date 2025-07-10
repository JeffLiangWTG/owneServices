using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class MessageKeySet
	{
		public static MessageKeySet Unknown => new MessageKeySet(new[] { "UNKNOWN" });

		public MessageKeySet(IEnumerable<string> keys)
		{
			DependsOn = new List<string>(keys.Select(FormatKey));
			Affects = DependsOn;
		}

		public IReadOnlyList<string> DependsOn { get; }

		public IReadOnlyList<string> Affects { get; }

#if DEBUG

		/*
		 * LockingMessageQueue was designed to support scenarios where keys that message depends on does not match keys that message affects.
		 *
		 * It can be useful if we want unknown messages to block all other messages, like this:
		 * Unknown Message
		 * - Depends On: UNKNOWN
		 * - Affects: UNKNOWN
		 * Normal Message
		 * - Depends On: UNKNOWN, MAWB123
		 * - Affects: MAWB123
		 *
		 * However, we decided not to use this feature, so this constructor is hidden under '#if DEBUG'.
		 * If we will need to redesign LockingMessageQueue, the knowledge that there is no production code relying on this feature might be useful.
		 * On the other hand, if we choose to use this feature, LockingMessageQueue already has appropriate code and tests.
		 */

		internal MessageKeySet(IEnumerable<string> dependsOn, IEnumerable<string> affects)
		{
			DependsOn = new List<string>(dependsOn.Select(FormatKey));
			Affects = new List<string>(affects.Select(FormatKey));
		}

#endif

		string FormatKey(string key)
		{
			key = key ?? string.Empty;

			if (key.Length > LockMechanismConstants.MaxKeyLength)
			{
				key = key.Substring(0, LockMechanismConstants.MaxKeyLength);
			}

			return key;
		}
	}
}
