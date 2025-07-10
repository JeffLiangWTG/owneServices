using System;

using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class ExportWizardMappingCollection : ImportExportMappingCollection<ExportWizardMapping, ExportWizard>
	{
		public ExportWizardMappingCollection(ExportWizard wizard)
			: base(wizard)
		{
		}
	}

	public class ExportWizardMappingCollectionView : BusinessObjectCollectionView<ExportWizardMapping>
	{
		public ExportWizardMappingCollectionView(ExportWizardMappingCollection collectionToFilter)
			: base(collectionToFilter)
		{ }

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			throw new NotSupportedException("Creating of new elements is not allowed.");
		}

		protected override void RebuildCore()
		{
			base.RebuildCore();
			Sort("Order");
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			ExportWizard wizard = ((ExportWizardMappingCollection)CollectionToFilter).Parent;
			return wizard.IsThisPartOfView((ExportWizardMapping)element);
		}

		public enum Direction { Up, Down }

		public void MoveSelectedElementsUpDown(ExportWizardMapping[] selectedElements, Direction direction)
		{
			ExportWizardMapping[] mappings = new ExportWizardMapping[Count];
			for (int i = 0; i < Count; i++)
			{
				mappings[i] = this[i];
			}
			int start = direction == Direction.Up ? 0 : mappings.Length - 1;
			int end = direction == Direction.Up ? mappings.Length : -1;
			int step = direction == Direction.Up ? 1 : -1;
			for (int i = start; i != end; i += step)
			{
				if (Array.IndexOf(selectedElements, mappings[i]) >= 0)
				{
					int newIndex = i - step;
					if (newIndex >= 0 && newIndex < mappings.Length && Array.IndexOf(selectedElements, mappings[newIndex]) < 0)
					{
						int newOrder = mappings[newIndex].Order;
						mappings[newIndex].Order = mappings[i].Order;
						mappings[i].Order = newOrder;
						ExportWizardMapping temp = mappings[newIndex];
						mappings[newIndex] = mappings[i];
						mappings[i] = temp;
					}
				}
			}

			using (SuspendListChanged())
			{
				Sort("Order");
			}
		}
	}
}
