using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public abstract class TopLevelCommissionLineGroupingCollection<TGrouping, TLine> : CommissionLineGroupingCollection<TGrouping, TLine>
		where TGrouping : CommissionLineGrouping<TGrouping, TLine>
		where TLine : BusinessObject, IViewCommissionLineProvider
	{
		#region Constructor

		protected TopLevelCommissionLineGroupingCollection(IBusinessObjectCollection innerCollection, ViewCommissionLineGrouper<TLine>[] groupers)
			: base(null)
		{
			this.InnerCollection = innerCollection;
			this.groupers = groupers;
		}

		public void Init()
		{
			Refresh();
			InnerCollection.ListChanged += InnerCollection_ListChanged;
		}

		#endregion

		#region Fields

		public readonly IBusinessObjectCollection InnerCollection;

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IEnumerable<ViewCommissionLineGrouper<TLine>> Groupers
		{
			get { return groupers; }
		}
		readonly ViewCommissionLineGrouper<TLine>[] groupers;

		public IEnumerable<TLine> InnerCollectionElements
		{
			get { return InnerCollection.Cast<TLine>(); }
		}

		#endregion

		#region Factory

		public new BusinessObjectFactory Factory
		{
			get { return InnerCollection.Factory; }
		}

		#endregion

		#region Refresh

		void Refresh()
		{
			using (SuspendListChanged())
			{
				RemoveAndDeleteAll();

				AddRefreshFetchHints();

				if (groupers != null && groupers.Any())
				{
					var currentGrouper = groupers.First();
					var groupedElements = currentGrouper.GetGroupings(InnerCollectionElements);
					foreach (var group in groupedElements)
					{
						AddNew(group, groupers.Skip(1).ToArray());
					}
				}
			}

			if (SortInformation != null)
			{
				Sort(SortInformation);
			}
		}

		protected virtual void AddRefreshFetchHints()
		{
		}

		void InnerCollection_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType != ListChangedType.ItemChanged)
			{
				Refresh();
			}
		}

		#endregion
	}
}
