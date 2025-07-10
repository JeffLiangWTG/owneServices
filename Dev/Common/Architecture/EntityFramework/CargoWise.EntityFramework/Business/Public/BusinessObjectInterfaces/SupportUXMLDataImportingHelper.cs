using System;

namespace CargoWise.EntityFramework
{
	public static class SupportUXMLDataImportingHelper
	{
		/// <summary>
		/// If the argument is an instance of ISupportUXMLDataImporting then it
		/// will create a disposable that will set the ISupportUXMLDataImporting.IsUXMLImportingData to true
		/// and upon being disposed set it to the value it was before.
		/// </summary>
		public static IDisposable UXMLDataImporting(object obj)
		{
			if (obj is ISupportUXMLDataImporting)
			{
				return new DisposableSupportUXMLDataImporting(obj as ISupportUXMLDataImporting);
			}

			return null;
		}

		class DisposableSupportUXMLDataImporting : IDisposable
		{
			readonly bool previousValue;
			ISupportUXMLDataImporting obj;

			internal DisposableSupportUXMLDataImporting(ISupportUXMLDataImporting obj)
			{
				previousValue = obj.IsUXMLImportingData;
				this.obj = obj;
				obj.IsUXMLImportingData = true;
			}

			public void Dispose()
			{
				if (obj != null)
				{
					obj.IsUXMLImportingData = previousValue;
					obj = null;
				}
			}
		}
	}
}
