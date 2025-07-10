using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class InvalidGroupByColumnException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		internal InvalidGroupByColumnException(string message, string invalidColumnName)
			: base(message)
		{
			this.InvalidColumnName = invalidColumnName;
		}

#if NETFRAMEWORK
		protected InvalidGroupByColumnException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
			this.InvalidColumnName = info.GetString("InvalidColumnName");
		}
#endif

#if NET
		[Obsolete]
#endif
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("InvalidColumnName", InvalidColumnName);
			base.GetObjectData(info, context);
		}

		#region Constructor For IJsonSerializable

		internal InvalidGroupByColumnException(InvalidGroupByColumnExceptionJsonData data)
			: base(data.Message)
		{
			this.InvalidColumnName = data.InvalidColumnName;
		}

		#endregion

		public object GetJsonData() => new InvalidGroupByColumnExceptionJsonData()
		{
			Message = base.Message,
			InvalidColumnName = InvalidColumnName,
		};

		public override string Message
		{
			get
			{
				return Res.GetString("fb5b46dc-ae53-40cb-b3a9-e1652d6ec9e8", "Error processing {0} columns - Could not find column [{1}].{2}", "GroupBy", InvalidColumnName, !string.IsNullOrEmpty(base.Message) ? System.Environment.NewLine + base.Message : string.Empty);
			}
		}

		internal readonly string InvalidColumnName;
	}
}
