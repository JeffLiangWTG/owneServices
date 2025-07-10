using System;

namespace Enterprise.ZArchitecture.Environment
{
	[Serializable]
	public class DstRuleParsingException : OdysseyException
	{
		public DstRuleParsingException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected DstRuleParsingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
