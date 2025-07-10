using System;

namespace CargoWise.EntityFramework
{
	public static class SupportDataImportingHelper
	{
		/// <summary>
		/// If the argument is an instance of ISupportDataImporting then it
		/// will create a disposable that will set the ISupportDataImporting.IsImportingData to true
		/// and upon being disposed set it to the value it was before.
		/// </summary>
		public static IDisposable DataImporting(object obj)
		{
			if (obj is ISupportDataImporting)
			{
				return new DisposableSupportDataImporting(obj as ISupportDataImporting);
			}

			return null;
		}

		internal class DisposableSupportDataImporting : IDisposable
		{
			readonly bool previousValue;
			ISupportDataImporting obj;

			internal DisposableSupportDataImporting(ISupportDataImporting obj)
			{
				previousValue = obj.IsImportingData;
				this.obj = obj;
				obj.IsImportingData = true;
			}

			public void Dispose()
			{
				if (obj != null)
				{
					obj.IsImportingData = previousValue;
					obj = null;
				}
			}
		}
	}
}
