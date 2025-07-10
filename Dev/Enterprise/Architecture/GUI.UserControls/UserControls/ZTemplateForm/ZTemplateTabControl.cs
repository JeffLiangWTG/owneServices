using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel.Design;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZTemplateTabControl : ZTabControl, IDesignerActionItemSource
	{
		#region Automatic Tab Page Ordering

		protected override Control.ControlCollection CreateControlsInstance()
		{
			return new ControlCollection(this);
		}

		public new ControlCollection Controls
		{
			get { return (ControlCollection)base.Controls; }
		}

		public new class ControlCollection : ZTabControl.ControlCollection
		{
			public ControlCollection(ZTemplateTabControl owner)
				: base(owner)
			{
			}

			public override void Add(Control value)
			{
				Owner.SuspendLayout();
				try
				{
					Stack removedTabPages = new Stack();
					for (int i = TabPagesInOrder.Length - 1; i >= 0; i--)
					{
						Type tabPageType = TabPagesInOrder[i];
						RemoveIfExists(FindTabPage(tabPageType), removedTabPages);

						if (tabPageType.IsInstanceOfType(value))
						{
							break;
						}
					}
					base.Add(value);
					RestoreRemovedTabPages(removedTabPages);
				}
				finally
				{
					Owner.ResumeLayout();
				}
			}

			internal T FindTabPage<T>() where T : ZTabPage
			{
				return (T)FindTabPage(typeof(T));
			}

			internal ZTabPage FindTabPage(Type tabPageType)
			{
				foreach (Control control in this)
				{
					if (tabPageType.IsInstanceOfType(control))
					{
						return (ZTabPage)control;
					}
				}
				return null;
			}

			#region Implementation

			protected new ZTemplateTabControl Owner
			{
				get { return base.Owner as ZTemplateTabControl; }
			}

			void RemoveIfExists(TabPage tabPage, Stack tabPageBackup)
			{
				if (tabPage != null && Owner.TabPages.Contains(tabPage))
				{
					tabPageBackup.Push(tabPage);
					int index = Owner.TabPages.IndexOf(tabPage);
					if (index > -1)
					{
						RemoveInternal(index);
					}
				}
			}

			void RestoreRemovedTabPages(Stack removedTabPages)
			{
				while (removedTabPages.Count > 0)
				{
					base.Add((TabPage)removedTabPages.Pop());
				}
			}

			void RemoveInternal(int index)
			{
				if (index != -1 && index < Owner.TabPages.Count)
				{
					typeof(TabControl).GetMethod("RemoveTabPage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Owner, new object[] { index });
				}
			}

			Type[] TabPagesInOrder
			{
				get
				{
					return new Type[]
					{
						typeof(ZAutoSizedTabPagePlugIn),
						typeof(ZStmNoteTabPage),
						typeof(ZLogsTabPage),
					};
				}
			}

			#endregion
		}

		#endregion

		#region IDesignerActionItemSource Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Design Time Only")]
		DesignerActionItem[] IDesignerActionItemSource.GetSortedActionItems(DesignerActionList actionList)
		{
			List<DesignerActionItem> result = new List<DesignerActionItem>();
			result.Add(new KDesignerActionMethodItem(actionList, delegate
			{ AddTabPage<ZStmNoteTabPage>(); }, "Add StmNote TabPage", true));
			result.Add(new KDesignerActionMethodItem(actionList, delegate
			{ AddTabPage<ZLogsTabPage>(); }, "Add Logs TabPage", true));
			result.Add(new KDesignerActionMethodItem(actionList, delegate
			{ AddTabPage(ObjectFactory.GetType<IWorkflowTabPage>()); }, "Add Tracking TabPage", true));
			result.Add(new KDesignerActionMethodItem(actionList, delegate
			{ AddTabPage<RelatedJobsTabPage>(); }, "Add Related Jobs TabPage", true));

			return result.ToArray();
		}

		void AddTabPage<T>() where T : ZTabPage, new()
		{
			AddTabPage(typeof(T));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "ZArchitecture Design Tool.")]
		void AddTabPage(Type tabType)
		{
			if (Controls.FindTabPage(tabType) != null)
			{
				MessageBox.Show("Only one " + tabType.Name + " can be added per tab control!", "(_|_)", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				TabPages.Add((ZTabPage)Activator.CreateInstance(tabType));
			}
		}

		#endregion

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);

			if (isNotFinalizing)
			{
				try
				{
					SelectedIndex = -1;
					TabPages.Clear();
				}
				catch (Win32Exception) { } //swallow exceptions during dispose, especially ones related to CreateHandle
				Controls.Clear();
			}
		}
	}
}
