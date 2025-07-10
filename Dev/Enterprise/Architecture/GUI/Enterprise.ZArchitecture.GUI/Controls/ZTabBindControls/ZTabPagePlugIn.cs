using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressResourceStringContainerControlNameKeyPrefix]
	public class ZTabPagePlugIn : ZBindingTabPage, ICaptionRenderingSupport, IDataGridLayoutIdentifierRoot
	{
		public ZTabPagePlugIn(ZPlugIn plugIn)
		{
			this.PlugIn = plugIn;
			if (plugIn.Name != null)
			{
				Name = plugIn.Name.Replace(" ", "") + "TabPage";
			}
			if (plugIn.Name != null)
			{
				this.AddAncestorChanged(delegate
				{
					if (plugIn.TextOverride != null)
					{
						Text = plugIn.TextOverride;
					}
					else if (FindForm() != null && !CaptionRenderingSupport.IsCaptionRenderingEnabled(this))
					{
						Text = plugIn.Name;
					}
				});
			}
		}

		public readonly ZPlugIn PlugIn;

		protected override void SetDataBindingCore(object dataSource, string dataMember)
		{
			var dataBoundControl = PlugIn.UserControl as IDataBoundControl;
			if (dataBoundControl != null && dataBoundControl.DataSource == null)
			{
				dataBoundControl.SetDataBinding(dataSource, dataMember);
			}
			base.SetDataBindingCore(dataSource, dataMember);
		}

		public static ZPlugIn FindParentPlugIn(Control control)
		{
			var current = control;
			while (current != null) // traverse hierarchy because only the top level tab will have the IZPlugin (if this tab is part of a plugin)
			{
				var tabPagePlugIn = current as ZTabPagePlugIn;
				if (tabPagePlugIn != null && tabPagePlugIn.PlugIn != null)
				{
					return tabPagePlugIn.PlugIn;
				}
				current = current.Parent;
			}
			return null;
		}

		public static ZTabPage FindTopLevelTabWithPlugin(Control control)
		{
			var result = control as ZTabPage;

			var current = control;
			while (current != null)
			{
				if (current is ZTabPagePlugIn && ((ZTabPagePlugIn)current).PlugIn != null)
				{
					result = (ZTabPage)current;
					break;
				}
				current = current.Parent;
			}

			return result;
		}

		protected override object DataSource
		{
			get { return PlugIn.BusinessEntity ?? base.DataSource; }
		}

		#region User Control

		public void AddPlugInUserControl()
		{
			if (PlugIn.UserControl != null && !Controls.Contains(PlugIn.UserControl))
			{
				PlugIn.UserControl.Dock = DockStyle.Fill;
				Controls.Add(PlugIn.UserControl);
			}
		}

		public void RemovePlugInUserControl(Control userControl)
		{
			if (userControl != null && Controls.Contains(userControl))
			{
				userControl.Visible = false;
				Controls.Remove(userControl);
			}
		}

		#endregion

		#region Binding

		internal override bool DelayBinding
		{
			get { return PlugIn.DelayBinding; }
		}

		#endregion

		#region ICaptionRenderingSupport Members

		bool? ICaptionRenderingSupport.CaptionRenderingEnabled
		{
			get { return false; }
		}

		event EventHandler ICaptionRenderingSupport.CaptionRenderingEnabledChanged { add { } remove { } }

		#endregion

		#region IDataGridLayoutIdentifierRoot Members

		/// <summary>
		/// Host Business entity's table name otherwise form name
		/// </summary>
		string IDataGridLayoutIdentifierRoot.ID
		{
			get
			{
				var result = string.Empty;

				var form = FindForm();

				if (form != null)
				{
					var zForm = form as ZForm;

					var businessEntity = zForm != null ? zForm.BusinessEntity : null;

					result = businessEntity != null ? businessEntity.GetType().Name : string.Empty;
				}

				return result;
			}
		}

		#endregion
	}
}
