using System;
using CargoWise.Common;
using Enterprise.DocumentEngine.ReportErrorManagement;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	[ExceptionVisibility(ExceptionVisibility.User)]
	public class BoxOutOfSectionBodyException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		public BoxOutOfSectionBodyException(string fieldIdentifier)
			: base(GetMessage(fieldIdentifier))
		{
		}

#if NETFRAMEWORK
		protected BoxOutOfSectionBodyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		static string GetMessage(string fieldIdentifier)
		{
			return FormattableString.Invariant($"The textbox may cross the boundary of #SectionBody, the inner <{fieldIdentifier}> macro cannot be translated.");
		}

		#region Constructor For IJsonSerializable

		internal BoxOutOfSectionBodyException(BoxOutOfSectionBodyExceptionJsonData data)
			: base(data.Message)
		{
		}

		#endregion

		public object GetJsonData() => new BoxOutOfSectionBodyExceptionJsonData()
		{
			Message = base.Message
		};

		internal void AddAsWarningToReport(Report report)
		{
			report.ErrorManager.Add(new ReportProcessingError(Message, ReportProcessingErrorSeverity.WarningWithoutErrorReport, this));
		}
	}
}
