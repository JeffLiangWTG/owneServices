using System.Globalization;
using CargoWise.Common;

namespace CargoWise.Data
{
	public class DbTryCatchWrapper
	{
		public string ExecuteInSqlTryCatch(string commandText)
		{
			Argument.NotNullOrEmpty(commandText, nameof(commandText));

			const string SqlThrow = "THROW;";
			return WrapCommandInTryCatch(commandText, SqlThrow);
		}

		public string WrapCommandToHandleSpecificError(string originalCommand, int errorNumberToHandle, string handlingCommand)
		{
			Argument.NotNullOrEmpty(originalCommand, nameof(originalCommand));
			Argument.NotNullOrEmpty(handlingCommand, nameof(handlingCommand));

			const string RawSqlCatchWithErrorHandling = @"
				IF (ERROR_NUMBER() = {0})
				BEGIN
					{1}
				END
				ELSE THROW;";
			string catchBlock = string.Format(RawSqlCatchWithErrorHandling, errorNumberToHandle, handlingCommand);

			return WrapCommandInTryCatch(originalCommand, catchBlock);
		}

		#region Enable login safe

		public string GetEnableDbLoginCommandSafe(string loginName)
		{
			Argument.NotNullOrEmpty(loginName, nameof(loginName));

			var singleQuotationEscapedLogin = loginName.QuoteEscapedName('\'');
			var bracketEscapedLogin = loginName.QuoteEscapedName();

			var trySql = string.Format(CultureInfo.InvariantCulture, "if (EXISTS (SELECT NULL FROM sys.sql_logins WHERE name = N'{0}' AND is_disabled = 1)) ALTER LOGIN [{1}] ENABLE;", singleQuotationEscapedLogin, bracketEscapedLogin); // sql command format string
			var catchSql = string.Format("ALTER LOGIN [{0}] WITH PASSWORD = '*', CHECK_POLICY = OFF; ALTER LOGIN [{0}] ENABLE; ALTER LOGIN [{0}] WITH PASSWORD = '', CHECK_POLICY = OFF, DEFAULT_LANGUAGE = us_english;", // sql command format string
				bracketEscapedLogin);
			var result = WrapCommandToHandleSpecificError(trySql, CannotEnableLoginWithEmptyPasswordErrorNumber, catchSql);

			return result;
		}

		internal const int CannotEnableLoginWithEmptyPasswordErrorNumber = 15510;

		#endregion

		string WrapCommandInTryCatch(string originalCommand, string catchBlock)
		{
			Argument.NotNullOrEmpty(originalCommand, nameof(originalCommand));
			Argument.NotNullOrEmpty(catchBlock, nameof(catchBlock));

			const string RawSqlTryCatch = @"
				BEGIN TRY
					{0}
				END TRY
				BEGIN CATCH
					{1}
				END CATCH";

			var result = string.Format(RawSqlTryCatch, originalCommand, catchBlock);
			return result;
		}
	}
}
