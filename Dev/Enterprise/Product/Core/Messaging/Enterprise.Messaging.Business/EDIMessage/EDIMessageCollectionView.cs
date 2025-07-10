using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Messaging.Business
{
	public class EDIMessageCollectionView : BusinessObjectCollectionView<EDIMessage>, IEnumerable
	{
		public EDIMessageCollectionView(EDIMessageCollection messages, ZQuery viewFilter)
			: base(messages)
		{
			if (viewFilter == null)
			{
				throw new ArgumentNullException(nameof(viewFilter));
			}
			this.ViewFilter = viewFilter;
			Rebuild();
		}

		protected override void RebuildOnConstruction()
		{
			// Do nothing
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
			Sort();
		}

		public override void Load()
		{
			throw new Exception("Call Load on the Collection you are viewing, rather than the view itself");
		}

		protected override BusinessObject AddNewCore()
		{
			var result = base.AddNewCore();
			Sort();
			return result;
		}

		#region Implementation

		protected readonly ZQuery ViewFilter;
		#region Overrides

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return element.MatchesFilter(ViewFilter);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			EDIMessage message = (EDIMessage)child;
			message.EM_ApplicationCode = ApplicationCode;
		}

		#endregion

		protected void Sort()
		{
			Sort(EDIMessage.Schema.EM_SystemCreateTimeUtc, System.ComponentModel.ListSortDirection.Descending);
		}

		protected bool Contains(ZString[] stringArray, ZString valueToSeachFor)
		{
			bool result = false;
			foreach (ZString arrayString in stringArray)
			{
				if (valueToSeachFor == arrayString)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		protected ZString ApplicationCode;

		#endregion
	}
}
