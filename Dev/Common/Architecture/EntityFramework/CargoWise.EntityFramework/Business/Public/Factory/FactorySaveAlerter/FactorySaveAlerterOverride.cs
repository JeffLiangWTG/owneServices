using System;
using System.Linq;

namespace CargoWise.EntityFramework
{
	public static partial class FactorySaveAlerterOverride
	{
		public static IDisposable TemporarilyOverride<T>(T objectThatCanSaveInLWK)
		{
			Validate<T>();
			return FactorySaveAlerter.TemporarilyOverride();
		}

		static partial void Validate<T>();
	}
}

#region Test
#if DEBUG

namespace CargoWise.EntityFramework
{
	using CargoWise.Common;
	using WTG.StaticAnalysis.Annotation;

	public static partial class FactorySaveAlerterOverride
	{
		[ThreadSafe]
		static readonly string[] AllowedTypes =
		{
			"TestType",
			"PerformanceStatisticsPersister",
			"JobConversation",
			"StmPrintServer",
			"CreateDocManagerDatabaseException",
			"CommunicationFailureNotification",
		};

		static partial void Validate<T>()
		{
			var typeName = typeof(T).Name;
			bool isAllowed = AllowedTypes.Any(typeNameToCompare => typeName.Equals(typeNameToCompare));

			if (!isAllowed)
			{
				ErrorReporter.ReportOnce("Need an error report or else catch(...) can hide problems");
				throw new InvalidOperationException($"Type: {typeof(T).FullName} has not been registered as allowed to bypass Factory Save Alerting");
			}
		}
	}
}

#endif
#endregion
