using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.AccountingVoucherPrint
{
	public class OptionalFilterCriteriaList : CollectionBase
	{
		public OptionalFilterCriteria this[int index]
		{
			get
			{
				return (OptionalFilterCriteria)this.List[index];
			}
		}

		public void Add(OptionalFilterCriteria newCriteria)
		{
			if (newCriteria != null)
			{
				if (FindItemByDescription(newCriteria.Description) == null)
				{
					this.List.Add(newCriteria);
				}
			}
		}

		public void Add(OptionalFilterCriteria newCriteria, bool selected)
		{
			newCriteria.Enabled = selected;
			if (newCriteria != null)
			{
				if (FindItemByDescription(newCriteria.Description) == null)
				{
					this.List.Add(newCriteria);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public ZQuery GetFilterFromSelectedItems()
		{
			ZQuery result = new ZQuery();
			foreach (OptionalFilterCriteria item in List)
			{
				result.AddToFilter(item.Filter, JoinCondition.Or);
			}

			return result;
		}

		public OptionalFilterCriteria FindItemByDescription(ZString descr)
		{
			foreach (OptionalFilterCriteria item in List)
			{
				if (item.Description == descr)
				{
					return item;
				}
			}

			return null;
		}

		public void ClearAllSelection()
		{
			SetAllEnabled(false);
		}

		public void SetItemByDescription(ZString descr, bool selected)
		{
			OptionalFilterCriteria result = FindItemByDescription(descr);
			if (result != null)
			{
				result.Enabled = selected;
			}
		}

		public bool FindSelectedValueByDescription(ZString descr)
		{
			OptionalFilterCriteria result = FindItemByDescription(descr);
			if (result != null)
			{
				return result.Enabled;
			}
			else
			{
				return false;
			}
		}

		public int SelectedItemCount
		{
			get
			{
				int count = 0;
				foreach (OptionalFilterCriteria item in List)
				{
					if (item.Enabled)
					{
						count++;
					}
				}
				return count;
			}
		}

		void SetAllEnabled(bool value)
		{
			foreach (OptionalFilterCriteria item in List)
			{
				item.Enabled = value;
			}
		}
	}
}
