using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public class CopyAndSendToCustomsFormDataLookups : ZLookups
	{
		public CopyAndSendToCustomsFormDataLookups(CopyAndSendToCustomsFormData parent) : base(parent) { }

		public CodeDescriptionPairList SendingActionTypeList => new AESOutgoingMessageTypeList();

		protected new CopyAndSendToCustomsFormData Parent => (CopyAndSendToCustomsFormData)base.Parent;
	}
}
