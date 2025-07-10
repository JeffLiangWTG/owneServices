using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class OuterPackLineCollectionView : BusinessObjectCollectionView<PackLine>, IPackLineCollection
	{
		readonly CommonContainer container;

		public OuterPackLineCollectionView(OuterPackLineCollection packLines, CommonContainer container)
			: base(packLines)
		{
			this.container = container;
			this.Rebuild();
		}

		#region Overrides

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			if (container != null)
			{
				PackLine packLine = (PackLine)element;
				return packLine.Containers.Contains(container.PK);
			}
			else
			{
				return true;
			}
		}

		#endregion

		#region IPackLineCollection Members

		PackLineCollectionCalculator IPackLineCollection.Totals
		{
			get { return totals ?? (totals = new PackLineCollectionCalculator(this, PackLineCollectionCalculator.FilterMode.NoFilter)); }
		}
		PackLineCollectionCalculator totals;

		BusinessObjectFactory IPackLineCollection.Factory
		{
			get { return Factory; }
		}

		int IPackLineCollection.Count
		{
			get { return Count; }
		}

		ZString IPackLineCollection.MasterPackagesUnit
		{
			get { return PackLines.MasterPackagesUnit; }
		}

		ZString IPackLineCollection.MasterWeightUnit
		{
			get { return PackLines.MasterWeightUnit; }
		}

		ZString IPackLineCollection.MasterVolumeUnit
		{
			get { return PackLines.MasterVolumeUnit; }
		}

		#endregion

		#region Implementation

		IPackLineCollection PackLines
		{
			get { return (IPackLineCollection)collectionToFilter; }
		}

		#endregion
	}
}
