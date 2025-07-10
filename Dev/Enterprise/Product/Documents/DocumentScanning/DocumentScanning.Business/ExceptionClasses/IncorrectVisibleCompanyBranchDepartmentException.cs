using System;

namespace Enterprise.DocumentScanning.Business
{
	[Serializable]
	public class IncorrectVisibleCompanyBranchDepartmentException : Exception
	{
		public IncorrectVisibleCompanyBranchDepartmentException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected IncorrectVisibleCompanyBranchDepartmentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
