using System;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Tools
{
	public interface IDbCommandWrapper
	{
		ZString SqlText { get; }
		ZString SqlTextWithFormat { get; }
		ZDateTime Time { get; }
		ZString ExceptionMessage { get; }
		ZBool IsFailed { get; }
	}

	public class DbCommandWrapper : NonPersistentBusinessObject, IDbCommandWrapper
	{
		public DbCommandWrapper()
		{
		}

		public DbCommandWrapper(SqlCommandExecutedEventArgs sqlCommandExecutedEventArgs)
		{
			if (sqlCommandExecutedEventArgs != null)
			{
				SqlTextWithFormat = sqlCommandExecutedEventArgs.Text.Trim();
				SqlText = Regex.Replace(new CommandTextTrimmer().GetTextWithoutMultiLineComments(SqlTextWithFormat), "\r\n", " ");
				Time = sqlCommandExecutedEventArgs.Time;
				Exception = sqlCommandExecutedEventArgs.Exception;
			}
		}

		public ZString SqlText { get; private set; }

		public ZString SqlTextWithFormat { get; private set; }

		public ZDateTime Time { get; private set; }

		public ZBool IsFailed
		{
			get { return Exception != null; }
		}

		public ZString ExceptionMessage
		{
			get { return IsFailed ? Exception.Message : string.Empty; }
		}

		Exception Exception { get; set; }
	}
}
