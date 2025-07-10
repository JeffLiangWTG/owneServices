using System;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	[Serializable]
	public class EntityMatchingException : ZException
	{
		public EntityMatchingException(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected EntityMatchingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
