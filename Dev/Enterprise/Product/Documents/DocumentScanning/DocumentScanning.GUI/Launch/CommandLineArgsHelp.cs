using System.Collections;
using System.Collections.Specialized;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.Launch
{
	public class CommandLineArgsHelp
	{
		public CommandLineArgsHelp()
		{
		}

		CommandLineArguments fCmdLineArguments;

		public void ReadInArguments(CommandLineArguments aCmdLineArguments)
		{
			fCmdLineArguments = aCmdLineArguments;
		}

		public CommandLineArguments CmdLineArguments
		{
			get { return fCmdLineArguments; }
		}

		// eg. IsOption("abc") -> returns true if abc is an option.
		public bool IsOption(string paramType)
		{
			string rawKey = "-" + paramType;

			object valueObj = fCmdLineArguments.OptionalArgs[rawKey];

			if (valueObj is bool)
			{
				if ((bool)valueObj)
				{
					return true;
				}
			}

			return false;
		}

		// e.g. GetOptionValue("optionA") returns "123" from an option such as "-optionA:123" 
		public string GetOptionValue(string paramType)
		{
			string rawKey = "-" + paramType + ":";
			object val = fCmdLineArguments.OptionalArgs[rawKey];
			if (val is string)
			{
				return (string)val;
			}
			else
			{
				return "";
			}
		}

		public StringCollection MandatoryArgs()
		{
			StringCollection sc = new StringCollection();
			sc.Add(fCmdLineArguments.ServerName);
			sc.Add(fCmdLineArguments.DatabaseName);

			if (!string.IsNullOrEmpty(fCmdLineArguments.ModuleName))
			{
				sc.Add(fCmdLineArguments.ModuleName);
			}
			return sc;
		}

		public string[] CmdLineArgsAsLine()
		{
			StringCollection sc = MandatoryArgs();

			IDictionaryEnumerator curEnumerator = fCmdLineArguments.OptionalArgs.GetEnumerator();
			while (curEnumerator.MoveNext())
			{
				string curKey = curEnumerator.Key.ToString();
				object curValueObj = curEnumerator.Value;

				string curEntry = "";

				if (curKey.EndsWith(":"))
				{
					if (curValueObj is string)
					{
						curEntry = curKey + (string)curValueObj;
					}
				}
				else
				{
					if (curValueObj is bool)
					{
						if ((bool)curValueObj)
						{
							curEntry = curKey;
						}
					}
				}

				if (!string.IsNullOrEmpty(curEntry))
				{
					sc.Add(curEntry);
				}
			}

			string[] result = new string[sc.Count];
			if (sc.Count > 0)
			{
				sc.CopyTo(result, 0);
			}

			return result;
		}
	}
}
