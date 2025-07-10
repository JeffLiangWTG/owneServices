using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Data;

namespace Enterprise.Accounting.Business.Aggregator
{
	public class AggregatorDbCommandFactory
	{
		public AggregatorDbCommandFactory() : this(defaultMaxParametersPerCommand)
		{ }

		public AggregatorDbCommandFactory(int maxParametersPerCommand)
		{
			this.maxParametersPerCommand = maxParametersPerCommand;
		}

		public void AppendQueryLine(int lineNo, string queryLine)
		{
			DbCommand dbCommand = GetCurrentDbCommand(ParametersRegex.Matches(queryLine).Count);
			LinesCommandsDictionary.Add(lineNo, dbCommand);
			dbCommand.CommandText += queryLine;
		}

		public DbCommand GetCommandForLine(int lineNo)
		{
			return LinesCommandsDictionary[lineNo];
		}

		public DbCommand[] GetCommands()
		{
			return Commands.ToArray();
		}

		public void ExecuteNonEmptyCommands()
		{
			foreach (DbCommand command in Commands)
			{
				if (!String.IsNullOrEmpty(command.CommandText))
				{
					command.ExecuteNonQuery();
				}
			}
		}

		#region Implementation

		readonly int maxParametersPerCommand;
		const int defaultMaxParametersPerCommand = 2100;
		int currentCommandParametersCount;

		Dictionary<int, DbCommand> LinesCommandsDictionary
		{
			get { return fLinesCommandsDictionary ?? (fLinesCommandsDictionary = new Dictionary<int, DbCommand>()); }
		}
		Dictionary<int, DbCommand> fLinesCommandsDictionary;

		List<DbCommand> Commands
		{
			get { return fCommands ?? (fCommands = new List<DbCommand>()); }
		}
		List<DbCommand> fCommands;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DbCommand GetCurrentDbCommand(int numberOfParametersToAdd)
		{
			if (currentCommandParametersCount + numberOfParametersToAdd >= maxParametersPerCommand)
			{
				CurrentDbCommand = Db.Connection.Command(String.Empty);
				Commands.Add(CurrentDbCommand);
				currentCommandParametersCount = 0;
			}
			currentCommandParametersCount += numberOfParametersToAdd;
			return CurrentDbCommand;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DbCommand CurrentDbCommand
		{
			get
			{
				if (fCurrentDbCommand == null)
				{
					fCurrentDbCommand = Db.Connection.Command(String.Empty);
					Commands.Add(fCurrentDbCommand);
				}
				return fCurrentDbCommand;
			}
			set
			{
				fCurrentDbCommand = value;
			}
		}
		DbCommand fCurrentDbCommand;

		Regex ParametersRegex
		{
			get { return fParametersRegex ?? (fParametersRegex = new Regex(@"@[\w]*", RegexOptions.Compiled)); }
		}
		Regex fParametersRegex;

		#endregion
	}
}
