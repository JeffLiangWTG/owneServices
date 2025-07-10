using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class ABLEntryNumLookups : CusEntryNumLookups
	{
		public ABLEntryNumLookups(ABLEntryNum parent) : base(parent)
		{
		}

		public new ABLEntryNum Parent => (ABLEntryNum)base.Parent;

		public virtual CodeDescriptionPairList CustomsEntryNumberTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var bill = Parent?.Parent as AsycudaBill;
				var messagingProvider = bill?.Header.MessagingProvider;
				if (messagingProvider != null)
				{
					result = messagingProvider.GetCustomsEntryNumberTypeList(Factory, bill.CountryCode);
				}
				return result;
			}
		}
	}
}
