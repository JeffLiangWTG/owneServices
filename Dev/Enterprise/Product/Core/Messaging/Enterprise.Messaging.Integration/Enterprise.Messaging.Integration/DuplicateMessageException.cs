using System;

namespace Enterprise.Messaging.Integration
{
	[Serializable]
	public class DuplicateMessageException : Exception
	{
		public DuplicateMessageException()
		{
		}

		public DuplicateMessageException(string applicationReference) : base(applicationReference)
		{
			ApplicationReference = applicationReference.Trim();
		}

		public DuplicateMessageException(string applicationReference, Exception innerException) : base(applicationReference, innerException)
		{
			ApplicationReference = applicationReference.Trim();
		}

#if NETFRAMEWORK
		protected DuplicateMessageException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
			if (info != null)
			{
				ApplicationReference = info.GetString(Field);
			}
		}
#endif

		public string ApplicationReference { get; private set; }

#if NET
		[Obsolete]
#endif
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);

			if (info != null)
			{
				info.AddValue(Field, ApplicationReference);
			}
		}

		const string Field = "ApplicationReference";
	}
}
