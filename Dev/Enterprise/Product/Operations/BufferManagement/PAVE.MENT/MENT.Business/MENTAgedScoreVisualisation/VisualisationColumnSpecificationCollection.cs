using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.PAVE.MENT.Business
{
	public class VisualisationColumnSpecificationCollection : NonPersistentBusinessObjectCollection<VisualisationColumnSpecification>
	{
		public VisualisationColumnSpecificationCollection(MENTAgedScoreVisualisation parent)
			: base(parent.Factory)
		{
			this.parent = parent;
		}

		readonly MENTAgedScoreVisualisation parent;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new VisualisationColumnSpecification(parent);
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			if (property.Name == (NoResString)"Column" || property.Name == "ColumnDisplay")
			{
				return new AlphanumericStringPropertyComparer(typeof(VisualisationColumnSpecification), property.Name, direction);
			}
			else
			{
				return base.GetComparerForSort(property, direction);
			}
		}
	}
}
