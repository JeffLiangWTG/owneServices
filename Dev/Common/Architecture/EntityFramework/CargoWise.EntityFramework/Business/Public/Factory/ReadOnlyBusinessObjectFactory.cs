using System.Diagnostics;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;

namespace CargoWise.EntityFramework
{
	public class ReadOnlyBusinessObjectFactory : BusinessObjectFactory
	{
		public ReadOnlyBusinessObjectFactory(bool crossThreadErrorCheckingEnabled = true, bool allowChangingThreadOwnership = true)
			: base(crossThreadErrorCheckingEnabled, allowChangingThreadOwnership)
		{
		}

		public ReadOnlyBusinessObjectFactory(string databaseName)
			: base(databaseName)
		{
		}

		public ReadOnlyBusinessObjectFactory(DbConnection connection)
			: base(connection)
		{
		}

		protected override void SaveCore()
		{
			if (ReportErrorOnSaveAttempt)
			{
				ErrorReporter.ReportOnce("SavingReadOnlyFactory", string.Format(CultureInfo.CurrentCulture,
					"Error: {0} named [{1}] should not be saved. Call stack trace: {2}",
					nameof(ReadOnlyBusinessObjectFactory),
					NameForDebugging,
					new StackTrace()
				));
			}
		}

		public bool ReportErrorOnSaveAttempt { get; set; } = true;
	}
}
