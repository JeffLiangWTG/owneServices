#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using CargoWise.Types;

namespace Enterprise.Client.DP2
{
#if NETFRAMEWORK
	[System.Serializable]
#endif
	public class DP2Exception : System.Exception
	{
		public enum DP2ExceptionType
		{
			BranchCodeMappingNotSet,
			DeptCodeMappingNotSet,
			LegacySystemCodeNotSet
		}

		public DP2Exception(ZString bizObj, DP2ExceptionType type)
		{
			this.BizObjName = bizObj;
			this.Type = type;
		}

#if NETFRAMEWORK
		protected DP2Exception(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public readonly ZString BizObjName;
		public readonly DP2ExceptionType Type;
	}
}
