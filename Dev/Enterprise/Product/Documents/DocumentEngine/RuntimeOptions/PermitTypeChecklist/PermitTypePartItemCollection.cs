using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[Serializable]
	public class PermitTypePartItemCollection : NonPersistentBusinessObjectCollection<PermitTypePartItem>, IJsonSerializable
	{
		public PermitTypePartItemCollection()
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
			return new PermitTypePartItem("XXX", "XXX", "XXX");
		}

		#endregion

		#region IncludedItems

		IEnumerable<PermitTypePartItem> IncludedItems
		{
			get { return this.Cast<PermitTypePartItem>().Where(x => x.Include); }
		}

		public bool HasIncludedItems
		{
			get { return IncludedItems.Any(); }
		}

		public void RecursivelySetIncluded(bool value)
		{
			foreach (PermitTypePartItem item in this)
			{
				item.RecursivelySetIncluded(value);
			}
		}

		public void CopyIncludeItems(PermitTypePartItemCollection sourceCollection)
		{
			if (sourceCollection != null)
			{
				foreach (PermitTypePartItem productItem in this)
				{
					var sourceProductItem = sourceCollection.Cast<PermitTypePartItem>().FirstOrDefault(x => x.Code == productItem.Code);
					if (sourceProductItem != null)
					{
						productItem.Include = sourceProductItem.Include;

						foreach (PermitTypePartItem modeItem in productItem.SubItemsCollection)
						{
							var sourceModeItem = sourceProductItem.SubItemsCollection.Cast<PermitTypePartItem>().FirstOrDefault(x => x.Code == modeItem.Code);
							if (sourceModeItem != null)
							{
								modeItem.Include = sourceModeItem.Include;

								foreach (PermitTypePartItem typeItem in modeItem.SubItemsCollection)
								{
									var sourceTypeItem = sourceModeItem.SubItemsCollection.Cast<PermitTypePartItem>().FirstOrDefault(x => x.Code == typeItem.Code);
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

		internal PermitTypePartItemCollection(PermitTypePartItemCollectionJsonData data)
		{
			AddRange(data.IncludedItems.Select(i => new PermitTypePartItem(i)));
		}

		#endregion

		#region IJsonSerializable Members

		public object GetJsonData() =>
			new PermitTypePartItemCollectionJsonData()
			{
				IncludedItems = IncludedItems
					.Select(i => (PermitTypePartItemJsonData)i.GetJsonData()).ToList()
			};

		#endregion
	}
}
