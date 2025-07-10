using System;

namespace Enterprise.CommissionManagement.Business
{
	[Serializable]
	public class OrgCommissionAgreementBeingProcessedException : Exception
	{
		public OrgCommissionAgreementBeingProcessedException() : base()
		{
		}

		public OrgCommissionAgreementBeingProcessedException(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected OrgCommissionAgreementBeingProcessedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
