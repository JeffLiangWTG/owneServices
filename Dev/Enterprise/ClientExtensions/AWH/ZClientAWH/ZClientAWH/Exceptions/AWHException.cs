#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using CargoWise.Types;

namespace Enterprise.Client.AWH
{
#if NETFRAMEWORK
	[System.Serializable]
#endif
	public class AWHException : System.Exception
	{
		public enum AWHExceptionType
		{
			BranchCodeMappingNotSet,
			DeptCodeMappingNotSet,
			LegacySystemCodeNotSet
		}

		public AWHException(ZString bizObj, AWHExceptionType type)
		{
			this.BizObjName = bizObj;
			this.Type = type;
		}

#if NETFRAMEWORK
		protected AWHException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public readonly ZString BizObjName;
		public readonly AWHExceptionType Type;
	}
}
