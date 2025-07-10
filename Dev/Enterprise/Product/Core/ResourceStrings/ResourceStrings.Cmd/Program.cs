using System;
using System.Globalization;
using System.Text;
using Enterprise.Initialisation;
using Enterprise.ResourceStrings.Business;

namespace ResourceStrings.Cmd
{
	internal class Program
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "This is not a debug message, we are using StdIO for IPC")]
		static void Main(string[] args)
		{
			Initialiser.InitialiseConsoleApp();
			DatabaseResourceStringSource.Disable();

			while (true)
			{
				var line = Console.ReadLine();
				if (!string.IsNullOrEmpty(line))
				{
					string[] parsed = line.Split('\t');
					if (parsed.Length == 2 && parsed[0].Equals("C"))
					{
						Console.WriteLine(Enterprise.ZArchitecture.Core.Culture.GetLanguageForCulture(new CultureInfo(parsed[1])));
					}
					else if (parsed.Length == 4 && parsed[0].Equals("L"))
					{
						string result = string.Empty;
						var match = ResourceStringsFactory.Lookup(CargoWiseOne.ResourceStrings.ResourceStrings.Normalize(parsed[1]), parsed[2]);
						if (match != null)
						{
							switch (parsed[3])
							{
								case "CAP":
									result = match.HD_Caption;
									break;
								case "FUL":
									result = match.HD_FullDescription;
									break;
								case "MED":
									result = match.HD_MidCaption;
									break;
								case "SHO":
									result = match.HD_ShortCaption;
									break;
							}
						}
						Console.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(result)));
					}
					else
					{
						throw new InvalidOperationException("Unknown command");
					}
				}
			}
		}
	}
}
