using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public class KeyPressSpan
	{
		public KeyPressSpan()
		{
		}

		public KeyPressSpan(string controlName, HashSet<string> registeredKeys)
		{
			ControlName = controlName;
			RegisteredHotKeys = registeredKeys;
			KeyPressEvents = new List<KeyPressEvent>();
		}

		public KeyPressSpan FilteredClone()
		{
			var keyPressSpan = new KeyPressSpan();
			keyPressSpan.ControlName = ControlName;
			keyPressSpan.RegisteredHotKeys = RegisteredHotKeys;
			keyPressSpan.KeyPressEvents = KeyPressEvents.Where(e => e.KeyPressProcessed).ToList();
			return keyPressSpan;
		}

		public void AddKeyPressEvent(KeyPressEvent keyPressEvent)
		{
			KeyPressEvents.Add(keyPressEvent);
		}

		public void AddHotKeys(HashSet<string> registeredKeys)
		{
			RegisteredHotKeys.UnionWith(registeredKeys);
		}

		internal string ControlName { get; set; }
		HashSet<string> RegisteredHotKeys { get; set; }
		internal List<KeyPressEvent> KeyPressEvents { get; set; }

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendLine($"*** Records In [{ControlName}] ***");
			builder.AppendLine();
			builder.AppendLine((NoResString)"Registered Hot Keys");
			RegisteredHotKeys.ForEach(e => builder.AppendLine($"\t- {e}"));
			builder.AppendLine();
			KeyPressEvents.ForEach(e => builder.AppendLine(e.ToString()));
			return builder.ToString();
		}
	}
}
