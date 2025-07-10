using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Business
{
	public class StmTemplateBaseCollection : StmTemplateCollection
	{
		public StmTemplateBaseCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
			EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			IsManagedForDataRefresh = true;
		}

		public StmTemplateBaseCollection(BusinessObjectFactory factory) : base(factory)
		{
			fEditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			IsManagedForDataRefresh = true;
		}

		public new StmTemplateBase this[int index]
		{
			get { return (StmTemplateBase)Elements[index]; }
		}

		public new StmTemplateBase AddNew()
		{
			return (StmTemplateBase)base.AddNew();
		}

		public new StmTemplateBase AddNew(Type bizObjType)
		{
			return (StmTemplateBase)base.AddNew(bizObjType);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public MenuEditingMode EditingMode
		{
			get { return fEditingMode; }
			set
			{
				fEditingMode = value;
				foreach (StmTemplateBase template in this)
				{
					template.EditingMode = value;
				}
			}
		}

		#region Implementation

		MenuEditingMode fEditingMode;

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			StmTemplateBase template = (StmTemplateBase)bizOAdded;

			template.EditingMode = EditingMode;
		}

		#endregion
	}
}
