using System;
using System.Data;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;

#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif

namespace CargoWise.EntityFramework
{
	[Serializable]
	public class ZSaveException : Exception
	{
		public ZSaveException(ZDataException dataLayerException, BusinessObjectFactory factory)
			: this(dataLayerException, factory, "")
		{
		}

		protected ZSaveException(ZDataException dataLayerException, BusinessObjectFactory factory, string errorType)
			: this(dataLayerException, factory, errorType, "")
		{
		}

		protected ZSaveException(ZDataException dataLayerException, BusinessObjectFactory factory, string errorType, string additionalInfo)
			: base(CreateMessage(dataLayerException.Row, factory, dataLayerException, errorType, additionalInfo), dataLayerException)
		{
			this.Factory = factory;
			this.fRow = dataLayerException.Row;
		}

#if NETFRAMEWORK
		protected ZSaveException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
			if (InnerException == null)
			{
				debugInfo = (NoResString)"ZSaveException Serialization No InnerException\r\nStackTrace:\r\n" + Environment.StackTrace;
			}
		}
#endif

		public BusinessObject[] BusinessObjects
		{
			get
			{
				if (fBusinessObjects == null)
				{
					fBusinessObjects = GetBusinessObjectsForRow(Row, Factory);
				}
				return fBusinessObjects;
			}
		}

		public readonly BusinessObjectFactory Factory;

		public DataRow Row
		{
			get { return fRow; }
		}

		public string FriendlyMessage
		{
			get { return InnerException.FriendlyMessage; }
		}

		public string ExtraDebugInfo
		{
			get { return InnerException.ExtraDebugInfo; }
		}

		public bool CanRecover
		{
			get { return InnerException.CanRecover; }
		}

		public bool ShouldBeReportedToEDI
		{
			get { return InnerException.ShouldBeReportedToEDI; }
		}

		public new ZDataException InnerException
		{
			get { return (ZDataException)base.InnerException; }
		}

		public ZString IndexNameIfUniqueIndexViolation
		{
			get
			{
				if (InnerException == null)
				{
					ErrorReporter.ReportOnce("ZSaveException.InnerException is null", debugInfo);
				}
				if (InnerException.CoreErrorHandler == null)
				{
					ErrorReporter.ReportOnce("ZSaveException.InnerException.CoreErrorHandler is null", debugInfo);
				}
				return InnerException?.CoreErrorHandler?.IndexNameIfUniqueIndexViolation ?? string.Empty;
			}
		}

		protected readonly DataRow fRow;
		BusinessObject[] fBusinessObjects;
		readonly string debugInfo;

		#region SuppressResourceStringsCheckRegion
		protected static string CreateMessage(DataRow row, BusinessObjectFactory factory, Exception innerException, string errorType, string additionalInfo)
		{
			var builder = new StringBuilder();
			builder.Append("\r\n**");
			builder.Append(errorType);
			builder.Append(" Error Saving Record **\r\n");

			if (factory != null)
			{
				builder.Append("\r\nServerName: ");
				builder.Append(factory.RowFactory.DbConnection.ServerName);
				builder.Append("\r\nDatabaseName: ");
				builder.Append(factory.RowFactory.DbConnection.CurrentDatabase);
			}

			if (row != null)
			{
				ZGuid pk = ZDataUtils.GetPK(row);

				builder.Append("\r\nTablename: ");
				builder.Append(row.Table.TableName);
				builder.Append("\r\nPK: ");
				builder.Append(pk);
				builder.Append("\r\nRowState: ");
				builder.Append(row.RowState);
				builder.Append("\r\nFactory validation suspended: ");
				builder.Append(factory.IsValidationSuspended);
				builder.Append("\r\nFactory name for debugging: ");
				builder.Append(factory.NameForDebugging);
			}
			else
			{
				builder.Append("\r\nUnknown record");
			}

			foreach (BusinessObject bO in GetBusinessObjectsForRow(row, factory))
			{
				builder.Append("\r\nBusiness object around row = ");
				builder.Append(bO.GetType().FullName);
				builder.Append("\r\nBusiness object validation suspended: ");
				builder.Append(bO.ValidationIndex);
				builder.Append("\r\nBusiness object is marking as needing validation suspended: ");
				builder.Append(bO.IsMarkingAsNeedingValidationSuspended);
				try
				{
					builder.Append("\r\nBusiness object light validation is enabled: ");
					builder.Append(bO.LightValidationEnabled);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
				}
				builder.Append("\r\nBusiness object additional info: ");
				builder.Append(bO.GetAdditionalInfoForZSaveException());
			}

			if (innerException != null && innerException.InnerException != null)
			{
				builder.Append("\r\n\r\nInner Message = ");
				builder.Append(innerException.InnerException.Message);
			}

			if (!string.IsNullOrEmpty(additionalInfo))
			{
				builder.Append("\r\n\r\nAdditional Information = ");
				builder.Append(additionalInfo);
			}

			builder.Append("\r\n\r\n");
			return builder.ToString();
		}
		#endregion

		protected static BusinessObject[] GetBusinessObjectsForRow(DataRow row, BusinessObjectFactory factory)
		{
			BusinessObject[] result = Array.Empty<BusinessObject>();
			if (row != null && factory != null)
			{
				result = factory.GetBizOsForPK(ZDataUtils.GetPK(row));
			}
			return result;
		}
	}

	[Serializable]
	public class ZSaveConcurrencyException : ZSaveException, IConcurrencyException
	{
		public ZSaveConcurrencyException(ZSaveConcurrencyException ex, bool notifyUserWithoutErrorReport = false)
			: this((ZDataConcurrencyException)ex.InnerException, ex.Factory)
		{
			Data.Add("SOURCE", ex);
			NotifyUserWithoutErrorReport = notifyUserWithoutErrorReport;
		}

		public ZSaveConcurrencyException(ZDataConcurrencyException exception, BusinessObjectFactory factory)
			: base(exception, factory, "CONCURRENCY", GetConcurrencyExceptionInfo(exception))
		{
		}

#if NETFRAMEWORK
		protected ZSaveConcurrencyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		static string GetConcurrencyExceptionInfo(ZDataConcurrencyException exception)
		{
			try
			{
				Type handlerType = exception.Row != null ? ObjectFactory.GetType<IConcurrencyExceptionHandler>() : null;
				IConcurrencyExceptionHandler handler = handlerType != null ? (IConcurrencyExceptionHandler)Activator.CreateInstance(handlerType, exception) : null;
				var info = handler?.Info;
				exception.ColumnsDBChanged = handler?.ColumnsDBChanged;
				return info;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				// We are handling other exception so ignore this one -- Unless it's critical
			}
			return null;
		}

		public bool NotifyUserWithoutErrorReport { get; }
	}

	[Serializable]
	public class ZSaveErrorAfterCommitInDbException : ZSaveException
	{
		public ZSaveErrorAfterCommitInDbException(ZDataException dataLayerException, BusinessObjectFactory factory) : base(dataLayerException, factory) { }

#if NETFRAMEWORK
		protected ZSaveErrorAfterCommitInDbException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}
