using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class MissingWorksheetException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		public MissingWorksheetException(string message, string worksheetName, string templateName)
			: base(message)
		{
			this.WorksheetName = worksheetName;
			this.TemplateName = templateName;
		}

#if NETFRAMEWORK
		protected MissingWorksheetException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public readonly string WorksheetName;
		public readonly string TemplateName;

		#region Constructor For IJsonSerializable

		internal MissingWorksheetException(MissingWorksheetExceptionJsonData data)
			: base(data.Message)
		{
			this.WorksheetName = data.WorksheetName;
			this.TemplateName = data.TemplateName;
		}

		#endregion

		public object GetJsonData() => new MissingWorksheetExceptionJsonData()
		{
			Message = base.Message,
			WorksheetName = WorksheetName,
			TemplateName = TemplateName,
		};
	}
}
