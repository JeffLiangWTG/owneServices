using System;

namespace Enterprise.Accounting.Business
{
	[Serializable]
	public class MissingGLHeaderException : Exception
	{
		public MissingGLHeaderException(string missingGLHeaderName)
			: base(GetMissingControlAccountMessage(missingGLHeaderName))
		{
		}

#if NETFRAMEWORK
		protected MissingGLHeaderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		static string GetMissingControlAccountMessage(string controlAccountName)
		{
			return Res.GetString("82524a35-0035-4185-b637-882d7ca88844", "{0} is missing. Please set an appropriate value for the Registry item at Accounting > General Ledger Defaults > Control Account > {0}", controlAccountName);
		}
	}

	[Serializable]
	public class InvalidAccountingJournalOperationException : InvalidOperationException
	{
		public InvalidAccountingJournalOperationException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected InvalidAccountingJournalOperationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
