using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	public static class ProcessHeaderCompletionStatementSequenceHandler
	{
		public static ZString GetUniqueCompletionStatementForReappliedWorkflowTemplate(IProcessHeaderCollection processHeaders, ZString templateCompletionStatement)
		{
			var newSequence = 0;
			var regex = new Regex($@"^{templateCompletionStatement}(\s[(](?<Number>\d+)[)])?$");
			foreach (var header in processHeaders.Cast<ProcessHeader>())
			{
				var match = regex.Match(header.FH_CompletionStatement);
				if (match.Success)
				{
					if (int.TryParse(match.Groups["Number"].Value, out var sequence) && sequence >= newSequence)
					{
						newSequence = sequence + 1;
					}
					else if (newSequence == 0)
					{
						newSequence = 1;
					}
				}
			}

			if (newSequence == 0)
			{
				return templateCompletionStatement;
			}

			return $"{templateCompletionStatement} ({newSequence})";
		}

		public static void SetClonedWorkflowSequence(ProcessHeader clone)
		{
			var nameSegments = clone.FH_CompletionStatement.ToString().Split();
			var regex = new Regex(@"^\((\d+)\)$"); // The $ ensures only 1 match because it must be at the end of the string
			var match = regex.Match(nameSegments.Last());
			var newCompletionStatement = clone.FH_CompletionStatement;
			var sequenceNumber = 1;

			if (match.Success)
			{
				var bracketedSequenceNumber = match.Groups[0].Value;

				if (bracketedSequenceNumber.Length != newCompletionStatement.Length)
				{
					var index = newCompletionStatement.LastIndexOf(bracketedSequenceNumber, StringComparison.Ordinal);
					newCompletionStatement = newCompletionStatement.Substring(0, index).TrimEnd();
				}
				else // if the whole completion statement is just a number in brackets
				{
					newCompletionStatement = string.Empty;
				}

				sequenceNumber = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) + 1;
			}

			clone.FH_CompletionStatement = string.Format(CultureInfo.InvariantCulture, string.IsNullOrEmpty(newCompletionStatement) ? "{0}({1})" : "{0} ({1})", newCompletionStatement, sequenceNumber);
		}
	}
}
