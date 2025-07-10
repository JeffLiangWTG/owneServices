using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class NumberFountainUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		protected NumberFountainUniqueIndexFailureHandler(string uniqueIndex, BusinessObject bizObjCausingError)
		{
			if (bizObjCausingError == null)
			{
				throw new ArgumentNullException(nameof(bizObjCausingError));
			}

			fUniqueIndexToHandle = uniqueIndex;
			this.BizObjCausingError = bizObjCausingError;
		}

		readonly string fUniqueIndexToHandle;
		protected readonly BusinessObject BizObjCausingError;

		protected abstract INumberFountainProxy NumberFountainToFix { get; }

		protected virtual DbCommand CommandToFindMaxValueInDatabase(DbConnection connection)
		{
			return null;
		}

		protected virtual string RetrieveAdditionalFountainInformationForErrorReport(IDbConnected connected)
		{
			return "";
		}

		#region IUniqueIndexFailureHandler Members

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get { yield return fUniqueIndexToHandle; }
		}

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			IDbConnected connected = (BizObjCausingError.Factory != null) ? BizObjCausingError.Factory : Db.Connection;
			string numberFountainValueBeforeFix = NumberFountainToFix.PeekPreliminaryFormatted(connected);

			try
			{
				DbCommand command = CommandToFindMaxValueInDatabase(connected.Connection);

				if (command != null)
				{
					NumberFountainToFix.FixFountain(connected, command);
				}
				else
				{
					NumberFountainToFix.FixFountain(connected, HandledUniqueIndexNames.Single());
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string numberFountainValueAfterAttemptedFix = NumberFountainToFix.PeekPreliminaryFormatted(connected);
				string additionalFountainInformation = RetrieveAdditionalFountainInformationForErrorReport(connected);
				ReportAsDeveloperError(numberFountainValueBeforeFix, numberFountainValueAfterAttemptedFix, additionalFountainInformation, ex);
			}

			notifier.ReportInformation(
				Res.GetString("00736e46-cae5-4487-9a9e-c52a7949feca", @"While you were working, the automatically assigned record number was used by another user.
Saving again should automatically resolve this issue.
Number Fountain: {0}
Index: {1}
Value: {2}",
NumberFountainToFix.Name,
indexName,
BizObjCausingError.HumanReadableName),
				Res.GetString("136ec32a-0097-491d-a349-889c4a015a1e", "Reload Required"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Error Exception")]
		void ReportAsDeveloperError(string valueBeforeFix, string valueAfterAttemptedFix, string additionalFountainInformation, Exception ex)
		{
			string reportMask =
				"Number Fountain adjust failed after an unique index violation\r\n" +
				"  Fountain                    : {0}\r\n" +
				"  Value Before                : {1}\r\n" +
				"  Value After Attempted Fix   : {2}\r\n" +
				"  Unique Index Violated       : {3}\r\n" +
				"  BizO causing error          : {4}\r\n";

			string reportMessage = String.Format(reportMask,
				NumberFountainToFix.GetType().FullName, valueBeforeFix, valueAfterAttemptedFix,
				fUniqueIndexToHandle, BizObjCausingError.GetType().FullName) + additionalFountainInformation;

			ErrorReporter.ReportOnce(reportMessage, ex);
		}

		#endregion
	}
}
