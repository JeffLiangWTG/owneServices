namespace Enterprise.DocumentEngine.Business
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.DocumentEngine.DocBuilder;
	using Enterprise.ZArchitecture.Schema;

	public class StmMenuDocumentConfigItemCollection : DependentBusinessObjectCollection<StmMenuDocumentConfigItem, StmMenuDocumentConfig>, IDocumentConfigItemCollection
	{
		public StmMenuDocumentConfigItemCollection(StmMenuDocumentConfig parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		public StmMenuDocumentConfigItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowSort
		{
			get { return false; }
		}

		public StmMenuDocumentConfigItem AddFromTemplateSection(TemplateSection section)
		{
			StmMenuDocumentConfigItem result = AddNew();
			result.S4_PrintOrder = Count;
			result.S4_SectionItemName = section.SectionName;
			result.S4_SectionType = section.IsGenericSectionType ? GenericSectionUsageList.Codes.BodySection : section.TypeCode.ToString();
			RebuildPrintOrdersBySectionTypes();
			return result;
		}

		public bool Contains(ZString sectionName)
		{
			foreach (StmMenuDocumentConfigItem configItem in this)
			{
				if (configItem.S4_SectionItemName == sectionName)
				{
					return true;
				}
			}

			return false;
		}

		public void DeleteAndReorder(params StmMenuDocumentConfigItem[] configItems)
		{
			foreach (StmMenuDocumentConfigItem configItemToDelete in configItems)
			{
				foreach (StmMenuDocumentConfigItem existingConfigItem in this)
				{
					if (existingConfigItem.S4_PrintOrder > configItemToDelete.S4_PrintOrder)
					{
						existingConfigItem.S4_PrintOrder--;
					}
				}

				configItemToDelete.Delete();
			}

			((IBusinessObjectCollectionInternals)this).HasChangesFromDelete = true;
		}

		public void MoveDown(IEnumerable<StmMenuDocumentConfigItem> configItems)
		{
			for (var index = Count - 1; index > 0; index--)
			{
				var configItemToMove = this[index - 1];
				if (configItems.Contains(configItemToMove))
				{
					var destination = this[index];
					if (!configItems.Contains(destination))
					{
						var comparer = new StmMenuDocumentConfigItemComparer();
						if (comparer.CompareByType(destination, configItemToMove) == 0)
						{
							destination.S4_PrintOrder -= 1;
							configItemToMove.S4_PrintOrder += 1;
							SortByPrintOrder();
						}
					}
				}
			}
		}

		public void MoveUp(IEnumerable<StmMenuDocumentConfigItem> configItems)
		{
			for (var index = 0; index < Count - 1; index++)
			{
				var configItemToMove = this[index + 1];
				if (configItems.Contains(configItemToMove))
				{
					var destination = this[index];
					if (!configItems.Contains(destination))
					{
						var comparer = new StmMenuDocumentConfigItemComparer();
						if (comparer.CompareByType(destination, configItemToMove) == 0)
						{
							destination.S4_PrintOrder += 1;
							configItemToMove.S4_PrintOrder -= 1;
							SortByPrintOrder();
						}
					}
				}
			}
		}

		void Move(int index, int adjacentIndexDifference)
		{
			StmMenuDocumentConfigItem configItemToMove = this[index];
			StmMenuDocumentConfigItem adjacentConfigItem = this[index + adjacentIndexDifference];

			var comparer = new StmMenuDocumentConfigItemComparer();
			if (comparer.CompareByType(configItemToMove, adjacentConfigItem) == 0)
			{
				configItemToMove.S4_PrintOrder += adjacentIndexDifference;
				adjacentConfigItem.S4_PrintOrder -= adjacentIndexDifference;
				SortByPrintOrder();
			}
		}

		public void MoveDown(int index)
		{
			if ((index > -1) && (index < Count - 1))
			{
				Move(index, 1);
			}
		}

		public void MoveUp(int index)
		{
			if (index > 0)
			{
				Move(index, -1);
			}
		}

		public void SortByPrintOrder()
		{
			Sort(StmMenuDocumentConfigItemSchema.Constants.S4_PrintOrder);
		}

		internal void RebuildPrintOrdersBySectionTypes()
		{
			var comparer = new StmMenuDocumentConfigItemComparer();
			Sort(comparer);

			ZInt printOrder = 1;
			foreach (StmMenuDocumentConfigItem configItem in this)
			{
				configItem.S4_PrintOrder = printOrder;
				printOrder++;
			}

			SortByPrintOrder();
		}

		public MenuEditingMode EditingMode
		{
			get { return fEditingMode; }
			set
			{
				fEditingMode = value;
				foreach (StmMenuDocumentConfigItem item in this)
				{
					item.EditingMode = value;
				}
			}
		}

		MenuEditingMode fEditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
			AddSectionTypeChangedHandler(businessObject as StmMenuDocumentConfigItem);
		}

		protected override BusinessObject AddNewCore()
		{
			var result = base.AddNewCore();
			AddSectionTypeChangedHandler(result as StmMenuDocumentConfigItem);
			return result;
		}

		void AddSectionTypeChangedHandler(StmMenuDocumentConfigItem documentConfigItem)
		{
			if (documentConfigItem != null)
			{
				documentConfigItem.S4_SectionTypeInfo.ValueChanged += new EventHandler(S4_SectionTypeInfo_ValueChanged);
			}
		}

		public override void Remove(BusinessObject elementToRemove)
		{
			var configItem = elementToRemove as StmMenuDocumentConfigItem;
			if (configItem != null)
			{
				configItem.S4_SectionTypeInfo.ValueChanged -= new EventHandler(S4_SectionTypeInfo_ValueChanged);
			}

			base.Remove(elementToRemove);

			RebuildPrintOrdersBySectionTypes();
		}

		void S4_SectionTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			RebuildPrintOrdersBySectionTypes();
		}
	}
}
