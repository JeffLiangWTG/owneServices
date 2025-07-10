using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business
{
	public class ExitNotificationPackageCollection : NonPersistentBusinessObjectCollection<ExitNotificationPackage>
	{
		public ExitNotificationPackageCollection(CusExitDetail exitDetail) : base(exitDetail?.Factory)
		{
			Argument.NotNull(exitDetail, nameof(exitDetail));

			AddRange(exitDetail.CusExitItems.Cast<CusExitItem>()
				.SelectMany(i => i.Packages)
				.Cast<CusExitItemPackage>()
				.Select(p => new ExitNotificationPackage(p)));
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => null;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
