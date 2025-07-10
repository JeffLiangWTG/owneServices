using System;
using Enterprise.DocumentEngine.ReportErrorManagement;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class ExcelLimitationBaseException : DocumentEngineException
	{
		protected ExcelLimitationBaseException(string message, Exception inner = null)
			: base(message, inner)
		{
		}

		internal ExcelLimitationBaseException(ExcelLimitationsHelper.LimitationType limitationType, string message, Exception inner = null)
			: this(message, inner)
		{
			LimitationType = limitationType;
		}

		#region Constructor For IJsonSerializable
		internal ExcelLimitationBaseException(ExcelLimitationBaseExceptionJsonData data)
			: this(data.Message, data.InnerExceptionMessage != null ? new Exception(data.InnerExceptionMessage) : null)
		{
			LimitationType = data.LimitationType;
		}
		#endregion

		public override object GetJsonData() => new FormulaProviderExceptionJsonData()
		{
			Message = base.Message
		};

#if NETFRAMEWORK
		protected ExcelLimitationBaseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
			this.LimitationType = (ExcelLimitationsHelper.LimitationType)Enum.Parse(typeof(ExcelLimitationsHelper.LimitationType), info.GetString("LimitationType"));
		}
#endif

#if NET
		[Obsolete]
#endif
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("LimitationType", LimitationType.ToString());
			base.GetObjectData(info, context);
		}

		internal readonly ExcelLimitationsHelper.LimitationType LimitationType;
	}
}
