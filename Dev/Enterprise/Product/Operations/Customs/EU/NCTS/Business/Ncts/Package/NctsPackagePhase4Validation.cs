using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPackagePhase4Validation : NctsPackageCommonValidation
	{
		public NctsPackagePhase4Validation(NctsPackage parent)
			: base(parent)
		{
		}

		protected override void CheckB5_UnitCount()
		{
			base.CheckB5_UnitCount();
			var parent = Parent;
			var nctsHeader = parent.Parent?.Header;
			if (!parent.IsBulk && !parent.IsUnpacked && nctsHeader?.MovementHeader != null)
			{
				var unitType = parent.B5_UnitType;
				var marksAndNumbers = parent.B5_MarksAndNumbers;
				var goodsItems = nctsHeader.MovementHeader.GoodsItems;
				var packages = goodsItems.SelectMany(item => item.Packages).Cast<NctsPackage>();
				var hasRelatedNonZeroUnitItem = packages.Any(x => x.B5_UnitType == unitType && x.B5_MarksAndNumbers == marksAndNumbers && x.B5_UnitCount > 0);
				if (!hasRelatedNonZeroUnitItem)
				{
					MandatoryValidation.MessageErrorIfIsZero(parent.B5_UnitCountInfo);
				}
			}
		}
	}
}
