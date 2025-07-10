using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public abstract class CusEntryHeaderMessageSendingActionLookups : ZLookups
	{
		protected CusEntryHeaderMessageSendingActionLookups(CusEntryHeaderMessageSendingAction parent) : base(parent) { }

		public abstract CodeDescriptionPairList SendingActionTypeList { get; }

		public virtual CodeDescriptionPairList SendingActionTypeListForDisplay => SendingActionTypeList;
	}
}
