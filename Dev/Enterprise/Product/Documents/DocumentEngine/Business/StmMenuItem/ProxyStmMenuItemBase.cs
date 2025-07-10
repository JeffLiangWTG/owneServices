using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Business
{
	public class ProxyStmMenuItemBase : StmMenuItemBase
	{
		public ProxyStmMenuItemBase(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static ProxyStmMenuItemBase New(StmMenuItemBase parentMenuItemBase, ZString context)
		{
			var proxyStmMenuItem = parentMenuItemBase.Factory.New<ProxyStmMenuItemBase>();
			proxyStmMenuItem.ParentMenuItem = parentMenuItemBase;
			proxyStmMenuItem.VirtualBusinessContext = context;

			return proxyStmMenuItem;
		}

		#region Properties

		StmMenuItemBase parentMenuItem;
		internal StmMenuItemBase ParentMenuItem
		{
			get
			{
				return parentMenuItem;
			}
			set
			{
				if (parentMenuItem == value)
				{
					return;
				}

				if (parentMenuItem != null)
				{
					parentMenuItem.OnDeleted -= ParentMenuItem_OnDeleted;
				}

				if (value != null)
				{
					value.OnDeleted += ParentMenuItem_OnDeleted;
				}

				parentMenuItem = value;
			}
		}

		ZString VirtualBusinessContext { get; set; }

		public void ParentMenuItem_OnDeleted(object sender, EventArgs e)
		{
			Delete();
		}

		public override ZString SU_MenuName
		{
			get { return ParentMenuItem != null ? ParentMenuItem.SU_MenuName : ZString.Empty; }
		}

		public override ZString SU_BusinessContext
		{
			get { return VirtualBusinessContext; }
		}

		public override ZString SU_DocumentDirection
		{
			get { return ParentMenuItem != null ? ParentMenuItem.SU_DocumentDirection : ZString.Empty; }
		}

		public override ZString SU_FilterList
		{
			get { return ParentMenuItem != null ? ParentMenuItem.SU_FilterList : ZString.Empty; }
		}

		public override ZString SU_MenuPath
		{
			get { return ParentMenuItem != null ? ParentMenuItem.SU_MenuPath : ZString.Empty; }
		}

		public override ZBool SU_IsSystemDefined
		{
			get { return ParentMenuItem != null ? ParentMenuItem.SU_IsSystemDefined : ZBool.False; }
		}

		public override ZBool SU_IsClientSpecific
		{
			get { return ParentMenuItem != null ? ParentMenuItem.SU_IsClientSpecific : ZBool.False; }
		}

		#endregion

		public override bool IsSavedByFactory
		{
			get { return false; }
		}
	}
}
