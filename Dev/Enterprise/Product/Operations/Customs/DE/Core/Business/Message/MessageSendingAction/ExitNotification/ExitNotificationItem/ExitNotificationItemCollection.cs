using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public class ExitNotificationItemCollection : NonPersistentBusinessObjectCollection<ExitNotificationItem>
	{
		public ExitNotificationItemCollection(CusExitDetail exitDetail) : base(exitDetail?.Factory)
		{
			Argument.NotNull(exitDetail, nameof(exitDetail));

			AddRange(exitDetail.CusExitItems.Cast<CusExitItem>()
				.Select(p => new ExitNotificationItem(p)));
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => null;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
