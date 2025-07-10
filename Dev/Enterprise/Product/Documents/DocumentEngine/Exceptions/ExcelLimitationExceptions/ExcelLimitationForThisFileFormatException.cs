using System;
using Enterprise.DocumentEngine.ReportErrorManagement;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class ExcelLimitationForThisFileFormatException : ExcelLimitationBaseException
	{
		internal ExcelLimitationForThisFileFormatException(ExcelLimitationsHelper.LimitationType limitationType, string message, Exception inner = null)
			: base(limitationType, message, inner)
		{
		}

#if NETFRAMEWORK
		protected ExcelLimitationForThisFileFormatException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		internal static bool IsSupportedExcelLimitationException(FlexCelException flexCelException, out ExcelLimitationsHelper.LimitationType? limitationType)
		{
			if (flexCelException is FlexCelXlsAdapterException flexCelXlsAdapterException)
			{
				switch (flexCelXlsAdapterException.ErrorCode)
				{
					case XlsErr.ErrTooManyRows:
					case XlsErr.ErrTooManyEntries:
						limitationType = ExcelLimitationsHelper.LimitationType.Row;
						return true;

					case XlsErr.ErrTooManyColumns:
						limitationType = ExcelLimitationsHelper.LimitationType.Column;
						return true;
				}
			}
			else if (flexCelException is FlexCelCoreException flexCelCoreException)
			{
				if (flexCelCoreException.ErrorCode == FlxErr.ErrFormulaTooLong)
				{
					limitationType = ExcelLimitationsHelper.LimitationType.Formula;
					return true;
				}
			}
			limitationType = null;
			return false;
		}
	}
}
