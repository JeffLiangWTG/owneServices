using System;

namespace CargoWise.EntityFramework
{
	public static class SupportUniversalEventImportingHelper
	{
		public static IDisposable UniversalEventImporting(object obj)
		{
			if (obj is ISupportUniversalEventImporting)
			{
				return new DisposableSupportUniversalEventImporting(obj as ISupportUniversalEventImporting);
			}

			return null;
		}

		class DisposableSupportUniversalEventImporting : IDisposable
		{
			readonly bool previousValue;
			ISupportUniversalEventImporting obj;

			internal DisposableSupportUniversalEventImporting(ISupportUniversalEventImporting obj)
			{
				previousValue = obj.IsSupportUniversalEventImporting;
				this.obj = obj;
				obj.IsSupportUniversalEventImporting = true;
			}

			public void Dispose()
			{
				if (obj != null)
				{
					obj.IsSupportUniversalEventImporting = previousValue;
					obj = null;
				}
			}
		}
	}
}
