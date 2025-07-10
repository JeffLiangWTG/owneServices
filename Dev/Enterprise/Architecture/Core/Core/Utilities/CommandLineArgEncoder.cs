using System.Text;

namespace Enterprise.ZArchitecture.Core
{
	public static class CommandLineArgEncoder
	{
		public static string EnquoteArgumentIfNeeded(string argument)
		{
			if (argument == null)
			{
				return null;
			}

			bool shouldBeQuoted = argument.IndexOf(' ') != -1;
			StringBuilder sb = new StringBuilder(argument.Length + (shouldBeQuoted ? 2 : 0));

			if (shouldBeQuoted)
			{
				sb.Append('"');
			}

			int backslashes = 0;

			for (int i = 0; i < argument.Length; i++)
			{
				if (argument[i] == '\\')
				{
					backslashes++;
				}
				else if (argument[i] == '"')
				{
					if (backslashes > 0)
					{
						sb.Append('\\', backslashes * 2); // Add twice amount of backslashes
						backslashes = 0;
					}
					sb.Append('\\').Append('"');
				}
				else
				{
					if (backslashes > 0)
					{
						sb.Append('\\', backslashes);
						backslashes = 0;
					}
					sb.Append(argument[i]);
				}
			}

			if (backslashes > 0)
			{
				sb.Append('\\', backslashes);
			}

			if (shouldBeQuoted)
			{
				if (backslashes > 0)
				{
					sb.Append('\\', backslashes); // Double backslashes
				}
				sb.Append('"');
			}

			return sb.ToString();
		}
	}
}
