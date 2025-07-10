using System;
using System.Collections.Generic;
using System.Text;

namespace CargoWise.Main.Navigation
{
	public class ShortcutKeyConverter
	{
		public IEnumerable<string> Convert(string shortCutKey)
		{
			if (string.IsNullOrWhiteSpace(shortCutKey))
			{
				throw new ArgumentNullException($"ShortCutKey should not be null .{shortCutKey}");
			}
			var charQueue = new Queue<char>();
			var strings = new HashSet<string>();
			foreach (var c in shortCutKey)
			{
				if (c == '+' && charQueue.Count > 0)
				{
					yield return AddKey(shortCutKey);
				}
				else
				{
					charQueue.Enqueue(c);
				}
			}
			if (charQueue.Count > 0)
			{
				yield return AddKey(shortCutKey);
			}
			string AddKey(string shortCutKey)
			{
				var sb = new StringBuilder();
				while (charQueue.Count > 0)
				{
					sb.Append(charQueue.Dequeue());
				}
				var keyToAdd = sb.ToString();
				if (strings.Contains(keyToAdd))
				{
					throw new ArgumentException($"Duplicated Key {keyToAdd} in {shortCutKey}");
				}
				strings.Add(keyToAdd);
				return keyToAdd;
			}
		}
	}
}
