using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[Serializable]
	public class SalesTradeLanePartItemCollection : NonPersistentBusinessObjectCollection<SalesTradeLanePartItem>, IJsonSerializable
	{
		public SalesTradeLanePartItemCollection()
		{
		}

		#region Add / Remove

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SalesTradeLanePartItem("XXX", "XXX", "XXX");
		}

		#endregion

		#region IncludedItems

		IEnumerable<SalesTradeLanePartItem> IncludedItems
		{
			get { return this.Cast<SalesTradeLanePartItem>().Where(x => x.Include); }
		}

		public bool HasIncludedItems
		{
			get { return IncludedItems.Any(); }
		}

		public void RecursivelySetIncluded(bool value)
		{
			foreach (SalesTradeLanePartItem item in this)
			{
				item.RecursivelySetIncluded(value);
			}
		}

		public void CopyIncludeItems(SalesTradeLanePartItemCollection sourceCollection)
		{
			if (sourceCollection != null)
			{
				foreach (SalesTradeLanePartItem productItem in this)
				{
					var sourceProductItem = sourceCollection.Cast<SalesTradeLanePartItem>().FirstOrDefault(x => x.Code == productItem.Code);
					if (sourceProductItem != null)
					{
						productItem.Include = sourceProductItem.Include;

						foreach (SalesTradeLanePartItem modeItem in productItem.SubItemsCollection)
						{
							var sourceModeItem = sourceProductItem.SubItemsCollection.Cast<SalesTradeLanePartItem>().FirstOrDefault(x => x.Code == modeItem.Code);
							if (sourceModeItem != null)
							{
								modeItem.Include = sourceModeItem.Include;

								foreach (SalesTradeLanePartItem typeItem in modeItem.SubItemsCollection)
								{
									var sourceTypeItem = sourceModeItem.SubItemsCollection.Cast<SalesTradeLanePartItem>().FirstOrDefault(x => x.Code == typeItem.Code);
									if (sourceTypeItem != null)
									{
										typeItem.Include = sourceTypeItem.Include;
									}
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region WhereClause

		public string BuildFullWhereClause(SqlParameterList parameterListToAppendTo)
		{
			var itemWhereClauses = IncludedItems.Select(x => "(" + x.BuildFullWhereClause(parameterListToAppendTo) + ")");
			return string.Join(" OR ", itemWhereClauses);
		}

		#endregion

		#region Constructor For IJsonSerializable

		internal SalesTradeLanePartItemCollection(SalesTradeLanePartItemCollectionJsonData data)
		{
			AddRange(data.IncludedItems.Select(i => new SalesTradeLanePartItem(i)));
		}

		#endregion

		#region IJsonSerializable Members

		public object GetJsonData() =>
			new SalesTradeLanePartItemCollectionJsonData()
			{
				IncludedItems = IncludedItems
					.Select(i => (SalesTradeLanePartItemJsonData)i.GetJsonData()).ToList()
			};

		#endregion
	}
}
