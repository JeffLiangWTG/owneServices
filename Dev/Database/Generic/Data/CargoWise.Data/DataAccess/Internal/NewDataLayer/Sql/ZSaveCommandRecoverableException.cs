using System;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	[Serializable]
	public class ZSaveCommandRecoverableException : ZSaveCommandException
	{
		public ZSaveCommandRecoverableException(Exception innerException, Guid errorPk, bool isConcurrencyError, Action<List<Guid>> recoverAction)
			: base(innerException, errorPk, isConcurrencyError)
		{
			RecoverAction = recoverAction;
		}

#if NETFRAMEWORK
		protected ZSaveCommandRecoverableException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public Action<List<Guid>> RecoverAction { get; }
	}
}
