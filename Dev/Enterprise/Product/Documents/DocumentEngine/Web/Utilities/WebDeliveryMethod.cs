using System;
using System.IO;

using Enterprise.DocumentEngine.DeliveryMethods;

namespace Enterprise.DocumentEngine.Web
{
	public class WebDeliveryMethod : DeliveryMethod, IDisposable
	{
		public MemoryStream MergeFilesIntoSingleExcelFile()
		{
			return MergeFilesIntoOneXLS() as MemoryStream;
		}

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			foreach (DeliveryInfo info in DeliveryInfos)
			{
				if (info.FileContents != null)
				{
					info.FileContents.Close();
				}
			}
		}

		#endregion
	}
}
