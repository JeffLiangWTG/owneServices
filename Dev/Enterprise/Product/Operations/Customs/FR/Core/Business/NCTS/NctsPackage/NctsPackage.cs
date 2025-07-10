using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsPackage : EU.NCTS.Business.NctsPackage, Integration.Customs.FR.INctsPackage
	{
		public NctsPackage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

		protected override CusInvPackValidation GetNewPhase4Validation() => new NctsPackagePhase4Validation(this);

		protected override CusInvPackValidation GetNewPhase5Validation() => new NctsPackagePhase5Validation(this);

		protected override CusInvPackLookups GetNewLookups() => new NctsPackageLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			DataRow row = ((IBusinessObjectInternals)this).Row;
			row[CusInvPackSchema.Constants.B5_UnitCount] = (long)0;
		}

		public bool HasContainer => ContainersPivotsForBindingOnly.Count > 0;

		public bool HasSelectedContainer => ContainersPivotsForBindingOnly.Any(x => ((NonPersistentContainerPivotPhase5)x).ContainerSelected);

		protected override EU.NCTS.Business.NonPersistentContainerPivotPhase5Collection GetContainersPivotsCore()
		{
			return new NonPersistentContainerPivotPhase5Collection(this);
		}

		public bool ItemsPackagesAreCustomsCompliant
		{
			get
			{
				var goodsItems = Parent.Bill.GoodsItems;
				var firstGoodsItemHasPackage = goodsItems.Cast<NctsDepartureCargoDesc>().FirstOrDefault(x => x.BY_LineNo == 1)?.Packages?.Any(p => p.B5_UnitCount > 0) ?? false;
				var allOtherGoodsItemsHaveNoPackages = goodsItems.Cast<NctsDepartureCargoDesc>().Where(item => item.BY_LineNo != 1).All(item => item.Packages?.All(p => p.B5_UnitCount == 0) ?? false);
				var allGoodsItemsHavePackages = goodsItems.Cast<NctsDepartureCargoDesc>().All(x => x.Packages?.All(p => p.B5_UnitCount > 0) ?? false);

				return allGoodsItemsHavePackages || (firstGoodsItemHasPackage && allOtherGoodsItemsHaveNoPackages);
			}
		}
	}
}
