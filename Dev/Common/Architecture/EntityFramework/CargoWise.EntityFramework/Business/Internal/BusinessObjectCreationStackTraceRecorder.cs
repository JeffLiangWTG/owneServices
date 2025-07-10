using System.ComponentModel;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.EntityFramework
{
	[ImmutableObject(true)]
	public class BusinessObjectCreationStackTraceRecorder
	{
		static BusinessObjectCreationStackTraceRecorder() { }

		static BusinessObjectCreationStackTraceRecorder Instance => instance ?? (instance = new BusinessObjectCreationStackTraceRecorder());
		[ThreadSafe]
		static BusinessObjectCreationStackTraceRecorder instance;

		public static bool IsEnabled => instance?.Enabled ?? false;

		public static void SetEnable(bool enabled)
		{
			Instance.Enabled = enabled;
		}
		bool Enabled { get; set; }
	}
}
