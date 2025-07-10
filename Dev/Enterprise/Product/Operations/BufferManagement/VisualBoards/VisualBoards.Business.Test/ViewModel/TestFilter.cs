using System;
using System.Collections.Generic;
using System.ComponentModel;
using Enterprise.BufferManagement.Business;

namespace Enterprise.VisualBoards.Business.Test
{
	public class TestFilter : IBoardFilter, IFilterApplicator
	{
		public bool IsApplicable(ICardContent cardContent, CellContent cell, BMBoardSectionViewModel boardSection = null)
		{
			return IsApplicableFunc == null || IsApplicableFunc(cardContent);
		}

		public Func<ICardContent, bool> IsApplicableFunc { get; set; }

		public void Apply(IComponent control, bool isApplicable, IEnumerable<AppliedFilter> applicableApplicators = null)
		{
			if (isApplicable && ApplyAction != null)
			{
				ApplyAction();
			}
		}

		public Action ApplyAction { get; set; }

		public virtual bool AllowMultiple
		{
			get { return true; }
		}

		public string FilterName
		{
			get { return "TestFilter"; }
		}

		public bool RequiresRedraw
		{
			get { return true; }
		}

		public bool RequiresRemoval { get; set; }

		public Action RestoreVisualStateAfterFilterRemovedAction { get; set; }
	}

	public class TestFilterForToggling : TestFilter
	{
		internal TestFilterForToggling(string name)
		{
			this.name = name;
		}

		readonly string name;

		public override bool Equals(object obj)
		{
			return ((TestFilterForToggling)obj).name == name;
		}

		public override int GetHashCode()
		{
			return name.GetHashCode();
		}
	}

	public class TestFilterWhichDoesNotAllowMultiples : TestFilter
	{
		public override bool AllowMultiple
		{
			get { return false; }
		}
	}

	[DescendantRefreshableFilter]
	public class TestFilterRequiringChildUpdates : TestFilter
	{
	}
}
