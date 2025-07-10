using System;

namespace CargoWise.EntityFramework
{
	[Flags]
	public enum TableHints
	{
		None = 0,
		FORCESEEK = 1,
		NOLOCK = 2,
		READPAST = 4,
		UPDLOCK = 8,
		ROWLOCK = 16
	}

	static class TableHintNames
	{
		public static string Name(TableHints tableHint)
		{
			switch (tableHint)
			{
				case TableHints.FORCESEEK:
					return "FORCESEEK";
				case TableHints.NOLOCK:
					return "NOLOCK";
				case TableHints.READPAST:
					return "READPAST";
				case TableHints.UPDLOCK:
					return "UPDLOCK";
				case TableHints.ROWLOCK:
					return "ROWLOCK";
			}

			throw new InvalidOperationException("Invalid TableHint");
		}
	}
}
