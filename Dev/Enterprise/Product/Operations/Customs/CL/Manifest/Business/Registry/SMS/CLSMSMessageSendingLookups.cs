using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class CLSMSMessageSendingLookups : ZLookups
	{
		public CLSMSMessageSendingLookups(CLSMSMessageSending parent) : base(parent)
		{
		}

		public new CLSMSMessageSending Parent => (CLSMSMessageSending)base.Parent;

		public new BusinessObjectFactory Factory => Parent.Factory;

		public CodeDescriptionPairList XtCredentialStatusList => Factory.GetCachedValue<CLSMSMessageXtCredentialStatusList>();
	}
}
