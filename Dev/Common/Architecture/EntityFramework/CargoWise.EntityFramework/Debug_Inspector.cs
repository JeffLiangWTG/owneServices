#region For Debugging Purposes:
#if DEBUG

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.EntityFramework
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This class is used to gather data for debugging purposes")]
	public static class Debug_Inspector
	{
		public static void Add(string label, string info)
		{
			if (!Ht.ContainsKey(label))
			{
				Ht.Add(label, new List<string>());
			}

			var list = Ht[label];

			if (!list.Contains(info))
			{
				list.Add(info);
			}

			Counter++;
			Calls.Add(new Call(Counter, label, info));
		}

		public static void Alert()
		{
			AlertValue++;
		}

		public static void Clear()
		{
			Ht.Clear();
			Calls.Clear();

			Counter = 0;
			AlertValue = 0;
		}

		public static string DumpData()
		{
			var sb = new StringBuilder();

			sb.Append($"<Debug_Info>\r\n");

			Ht.Keys.ToList().ForEach(x =>
			{
				sb.Append($"\r\n<{x}>\r\n");
				var list = Ht[x];

				list.ForEach(y =>
				{
					sb.Append($"{y}\r\n");
				});

				sb.Append($"</{x}>\r\n");
			});

			//sb.Append("-------------------------------------------------\r\n");

			sb.Append($"\r\n<Call_History>\r\n");

			Calls.ForEach(x => sb.Append($"{x.Id},  {x.Label},  {x.Value}\r\n"));

			sb.Append($"</Call_History>\r\n");

			//sb.Append("-------------------------------------------------\r\n");

			sb.Append($"\r\n<Alerts>{AlertValue}</Alerts>\r\n");

			sb.Append($"\r\n</Debug_Info>\r\n");

			var retVal = sb.ToString();
			return retVal;
		}

		#region Instance Variables:

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
		public readonly static Dictionary<string, List<string>> Ht = new Dictionary<string, List<string>>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
		public readonly static List<Call> Calls = new List<Call>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "This whole class is used only for debugging purposes")]
		public static int Counter = 0;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "This whole class is used only for debugging purposes")]
		public static int AlertValue = 0;

		#endregion Instance Variables.
	}

	public class Call
	{
		public Call(int id, string label, object value)
		{
			Id = id;
			Label = label;
			Value = value;
		}

		public readonly int Id;
		public readonly string Label;
		public readonly object Value;
	}
}

#endif
#endregion
