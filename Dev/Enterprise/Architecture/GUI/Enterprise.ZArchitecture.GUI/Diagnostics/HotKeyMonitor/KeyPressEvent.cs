using System.Text;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.GUI
{
	public class KeyPressEvent
	{
		public KeyPressEvent(string currentControlName, string keyDataName, string processorName, bool processed)
		{
			CurrentDateTime = HotKeyMonitor.GetCurrentDateTime();
			ControlName = currentControlName;
			KeyDataName = keyDataName;
			ProcessorName = processorName;
			KeyPressProcessed = processed;
		}

		public KeyPressEvent(string currentControlName, string keyDataName, string callStacks)
		{
			CurrentDateTime = HotKeyMonitor.GetCurrentDateTime();
			ControlName = currentControlName;
			KeyDataName = keyDataName;
			CallStacks = callStacks;
		}

		internal string CurrentDateTime { get; }
		string ControlName { get; }
		string KeyDataName { get; }
		string ProcessorName { get; }
		internal bool KeyPressProcessed { get; set; }
		string CallStacks { get; }

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.Append($"\t[{CurrentDateTime}] [{ControlName}] [{KeyDataName}] [processor: '{ProcessorName}'] [processed: {KeyPressProcessed}]");

			if (!CallStacks.IsNullOrEmpty())
			{
				builder.AppendLine(System.Environment.NewLine);
				builder.AppendLine($"\t{CallStacks.Replace("   ", "\t")}");
			}

			return builder.ToString();
		}
	}
}
