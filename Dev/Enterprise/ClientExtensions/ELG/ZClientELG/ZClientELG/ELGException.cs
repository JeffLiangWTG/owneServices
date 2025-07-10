using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Enterprise.Client.ELG
{
#if NETFRAMEWORK
	[Serializable]
#endif
	public class ELGException : Exception
	{
		public ELGException(ELGExceptionType type)
		{
			this.Type = type;
		}

#if NETFRAMEWORK
		protected ELGException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public ELGExceptionType Type
		{
			get { return type; }
			private set { type = value; }
		}
		ELGExceptionType type;
	}

	public enum ELGExceptionType
	{
		Unknown,
		NoBranchDepartmentMappingsSetOrFound,
		NoTransportModeAndChargeCodeMappingsSetOrFound,
		NoSageAccountCodeMappingsSetOrFound
	}
}
