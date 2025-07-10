using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Business;

namespace Enterprise.DocumentEngine
{
	public class ReportCommandCollection : StmMenuItemBaseCollection
	{
		/// <summary>
		/// Creates a report menu item collection
		/// </summary>
		public ReportCommandCollection(BusinessObjectFactory factory, string businessContext)
			: base(factory)
		{
			this.fBusinessContext = businessContext;
		}
		readonly string fBusinessContext;

		public string BusinessContext
		{
			get { return fBusinessContext; }
		}

		public new ReportCommand this[int index]
		{
			get { return (ReportCommand)Elements[index]; }
		}

		public new ReportCommand AddNew()
		{
			return (ReportCommand)base.AddNew();
		}

		public new ReportCommand AddNew(Type bizObjType)
		{
			return (ReportCommand)base.AddNew(bizObjType);
		}

		public override void Load()
		{
			var filter = GetApplicableMenusFilter(BusinessContext, true);
			LoadMenus(filter);
		}

		public void LoadApplicableReports()
		{
			Load();

			for (int index = 0; index < Count; index++)
			{
				if (!this[index].IsApplicable)
				{
					Remove(this[index]);
					index--;
				}
			}
		}

		public void SetAllowNew(bool value)
		{
			fAllowNew = value;
		}

		protected override bool AllowNewCore
		{
			get { return fAllowNew; }
		}
		bool fAllowNew = true;

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (bizOAdded != null)
			{
				ReportCommand command = (ReportCommand)bizOAdded;
				command.SU_BusinessContext = BusinessContext;
				if (ReadOnly)
				{
					command.ReadOnly = true; //TODO: Is this required?
				}
			}
		}
	}
}
