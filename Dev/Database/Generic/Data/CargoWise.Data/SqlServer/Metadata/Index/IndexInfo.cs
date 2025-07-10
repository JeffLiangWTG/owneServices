using System;
using CargoWise.Common;
using CargoWise.Database.Shared;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	public static partial class MetaData
	{
		#region IndexOptions

		internal static DataCompression GetDataCompression(int dataCompression)
		{
			switch (dataCompression)
			{
				case 1:
					return DataCompression.ROW;
				case 2:
					return DataCompression.PAGE;
				case 3:
					return DataCompression.COLUMNSTORE;
				case 4:
					return DataCompression.COLUMNSTORE_ARCHIVE;
				default:
					return DataCompression.NONE;
			}
		}

		#endregion // IndexOptions

		#region TestValues

		internal static string DelayForTests { get => delayForTests; private set => delayForTests = value; }
		[ThreadSafe] // private and test only.
		static string delayForTests = string.Empty;

#if DEBUG

		static internal IDisposable AddDelayForTests()
		{
			DelayForTests = "WAITFOR DELAY '00:00:02';";
			return new DisposableAction(() => DelayForTests = null);
		}

#endif

		#endregion
	}
}
