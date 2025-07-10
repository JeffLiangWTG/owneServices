using System;

using CargoWise.EntityFramework;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class DataImportFlagChanger : IDisposable
	{
		public DataImportFlagChanger(IBusiness bizO)
		{
			BizOForDataImport = bizO as ISupportDataImporting;
			if (BizOForDataImport != null)
			{
				WasImportingData = BizOForDataImport.IsImportingData;
				BizOForDataImport.IsImportingData = true;
			}

			LastBizO = bizO;
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (BizOForDataImport != null && !WasImportingData)
			{
				BizOForDataImport.IsImportingData = false;
			}
		}

		#endregion

		readonly bool WasImportingData;
		readonly ISupportDataImporting BizOForDataImport;

		#region For Test
		public static IBusiness LastBizO
		{
			get { return (IBusiness)BizOWeakReference.Target; }
			set { BizOWeakReference.Target = value; }
		}

		static WeakReference BizOWeakReference
		{
			get { return bizOWeakReference ?? (bizOWeakReference = new WeakReference(null)); }
		}
		[ThreadStatic]
		static WeakReference bizOWeakReference;

		#endregion
	}
}
