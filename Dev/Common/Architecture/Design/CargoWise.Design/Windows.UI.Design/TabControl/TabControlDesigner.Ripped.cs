using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using CargoWise.Design;
using CargoWise.Windows.UI;

namespace System.Windows.Forms.Design.Ripped
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class TabControlDesigner : ParentControlDesigner
	{
		bool tabControlSelected;
		DesignerVerbCollection verbs;
		DesignerVerb removeVerb;
		bool disableDrawGrid;
		bool addingOnInitialize;
		bool forwardOnDrag;

		protected virtual Type TabPageType
		{
			get { return typeof(TabPage); }
		}

		protected override bool AllowControlLasso
		{
			get
			{
				return false;
			}
		}

		protected override bool DrawGrid
		{
			get
			{
				if (disableDrawGrid)
				{
					return false;
				}
				else
				{
					return base.DrawGrid;
				}
			}
		}

		public override bool ParticipatesWithSnapLines
		{
			get
			{
				if (!forwardOnDrag)
				{
					return false;
				}

				var selectedTabPageDesigner = GetSelectedTabPageDesigner();
				if (selectedTabPageDesigner != null)
				{
					return selectedTabPageDesigner.ParticipatesWithSnapLines;
				}
				else
				{
					return true;
				}
			}
		}

		public override DesignerVerbCollection Verbs
		{
			get
			{
				if (verbs == null)
				{
					removeVerb = new DesignerVerb(System.Design.Ripped.SR.GetString("TabControlRemove"), new EventHandler(OnRemove));
					verbs = new DesignerVerbCollection();
					verbs.Add(new DesignerVerb(System.Design.Ripped.SR.GetString("TabControlAdd"), new EventHandler(OnAdd)));
					verbs.Add(removeVerb);
				}
				if (Control != null)
				{
					removeVerb.Enabled = Control.Controls.Count > 0;
				}

				return verbs;
			}
		}

		public override void InitializeNewComponent(IDictionary defaultValues)
		{
			base.InitializeNewComponent(defaultValues);
			try
			{
				addingOnInitialize = true;
				OnAdd(this, EventArgs.Empty);
				OnAdd(this, EventArgs.Empty);
			}
			finally
			{
				addingOnInitialize = false;
			}
			MemberDescriptor member = TypeDescriptor.GetProperties(Component)["Controls"];
			RaiseComponentChanging(member);
			RaiseComponentChanged(member, null, null);
			TabControl tabControl = (TabControl)Component;
			if (tabControl == null)
			{
				return;
			}

			tabControl.SelectedIndex = 0;
		}

		public override bool CanParent(Control control)
		{
			if (control is TabPage)
			{
				return !Control.Contains(control);
			}
			else
			{
				return false;
			}
		}

		void CheckVerbStatus()
		{
			if (removeVerb == null)
			{
				return;
			}

			removeVerb.Enabled = Control.Controls.Count > 0;
		}

		protected override IComponent[] CreateToolCore(ToolboxItem tool, int x, int y, int width, int height, bool hasLocation, bool hasSize)
		{
			TabControl tabControl = (TabControl)Control;
			if (tabControl.SelectedTab == null)
			{
				throw new ArgumentException(System.Design.Ripped.SR.GetString("TabControlInvalidTabPageType", new object[1]
		{
		  tool.DisplayName
		}));
			}
			else
			{
				IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
				if (designerHost != null)
				{
					ParentControlDesigner.InvokeCreateTool((ParentControlDesigner)designerHost.GetDesigner(tabControl.SelectedTab), tool);
				}

				return null;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
				if (selectionService != null)
				{
					selectionService.SelectionChanged -= new EventHandler(OnSelectionChanged);
				}

				IComponentChangeService componentChangeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
				if (componentChangeService != null)
				{
					componentChangeService.ComponentChanged -= new ComponentChangedEventHandler(OnComponentChanged);
				}

				if (Control is TabControl tabControl)
				{
					tabControl.SelectedIndexChanged -= new EventHandler(OnTabSelectedIndexChanged);
					tabControl.GotFocus -= new EventHandler(OnGotFocus);
					tabControl.RightToLeftLayoutChanged -= new EventHandler(OnRightToLeftLayoutChanged);
					tabControl.ControlAdded -= new ControlEventHandler(OnControlAdded);
				}
			}
			base.Dispose(disposing);
		}

		protected override bool GetHitTest(Drawing.Point point)
		{
			TabControl tabControl = (TabControl)Control;
			if (!tabControlSelected)
			{
				return false;
			}

			Drawing.Point pt = Control.PointToClient(point);
			return !tabControl.DisplayRectangle.Contains(pt);
		}

		internal static TabPage GetTabPageOfComponent(object comp)
		{
			if (!(comp is Control))
			{
				return null;
			}

			Control control = (Control)comp;
			while (control != null && !(control is TabPage))
			{
				control = control.Parent;
			}

			return (TabPage)control;
		}

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			AutoResizeHandles = true;
			ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
			if (selectionService != null)
			{
				selectionService.SelectionChanged += new EventHandler(OnSelectionChanged);
			}

			IComponentChangeService componentChangeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
			if (componentChangeService != null)
			{
				componentChangeService.ComponentChanged += new ComponentChangedEventHandler(OnComponentChanged);
			}

			if (!(component is TabControl tabControl))
			{
				return;
			}

			tabControl.SelectedIndexChanged += new EventHandler(OnTabSelectedIndexChanged);
			tabControl.GotFocus += new EventHandler(OnGotFocus);
			tabControl.RightToLeftLayoutChanged += new EventHandler(OnRightToLeftLayoutChanged);
			tabControl.ControlAdded += new ControlEventHandler(OnControlAdded);
		}

		void OnAdd(object sender, EventArgs eevent)
		{
			TabControl tabControl = (TabControl)Component;
			IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
			if (designerHost == null)
			{
				return;
			}

			DesignerTransaction designerTransaction = null;
			try
			{
				try
				{
					designerTransaction = designerHost.CreateTransaction(System.Design.Ripped.SR.GetString("TabControlAddTab", new object[1] { Component.Site.Name }));
				}
				catch (CheckoutException ex)
				{
					if (ex == CheckoutException.Canceled)
					{
						return;
					}
					else
					{
						throw;
					}
				}
				MemberDescriptor member = TypeDescriptor.GetProperties(tabControl)["Controls"];
				TabPage tabPage = (TabPage)designerHost.CreateComponent(TabPageType);
				if (!addingOnInitialize)
				{
					RaiseComponentChanging(member);
				}

				tabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3);
				string str = null;
				PropertyDescriptor propertyDescriptor1 = TypeDescriptor.GetProperties(tabPage)["Name"];
				if (propertyDescriptor1 != null && propertyDescriptor1.PropertyType == typeof(string))
				{
					str = (string)propertyDescriptor1.GetValue(tabPage);
				}

				if (str != null)
				{
					PropertyDescriptor propertyDescriptor2 = TypeDescriptor.GetProperties(tabPage)["Text"];
					if (propertyDescriptor2 != null)
					{
						propertyDescriptor2.SetValue(tabPage, str);
					}
				}
				PropertyDescriptor propertyDescriptor3 = TypeDescriptor.GetProperties(tabPage)["UseVisualStyleBackColor"];
				if (propertyDescriptor3 != null && propertyDescriptor3.PropertyType == typeof(bool) && (!propertyDescriptor3.IsReadOnly && propertyDescriptor3.IsBrowsable))
				{
					propertyDescriptor3.SetValue(tabPage, true);
				}

				tabControl.Controls.Add(tabPage);
				tabControl.SelectedIndex = tabControl.TabCount - 1;
				if (addingOnInitialize)
				{
					return;
				}

				RaiseComponentChanged(member, null, null);
			}
			finally
			{
				if (designerTransaction != null)
				{
					designerTransaction.Commit();
				}
			}
		}

		void OnComponentChanged(object sender, ComponentChangedEventArgs e)
		{
			CheckVerbStatus();
		}

		void OnGotFocus(object sender, EventArgs e)
		{
			Type eventHandlerServiceType = SystemDesignLoader.SystemDesignAssembly.GetType("System.Windows.Forms.Design.IEventHandlerService", true);

			object service1 = GetService(eventHandlerServiceType);
			if (service1 != null)
			{
				Control control1 = (Control)eventHandlerServiceType.GetProperty("FocusWindow").GetValue(service1, null);
				if (control1 != null)
				{
					control1.Focus();
				}
			}
		}

		void OnRemove(object sender, EventArgs eevent)
		{
			TabControl tabControl = (TabControl)Component;
			if (tabControl == null || tabControl.TabPages.Count == 0)
			{
				return;
			}

			MemberDescriptor member = TypeDescriptor.GetProperties(Component)["Controls"];
			TabPage selectedTab = tabControl.SelectedTab;
			IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
			if (designerHost == null)
			{
				return;
			}

			DesignerTransaction designerTransaction = null;
			try
			{
				try
				{
					designerTransaction = designerHost.CreateTransaction(System.Design.Ripped.SR.GetString("TabControlRemoveTab", selectedTab.Site.Name, Component.Site.Name));
					RaiseComponentChanging(member);
				}
				catch (CheckoutException ex)
				{
					if (ex == CheckoutException.Canceled)
					{
						return;
					}
					else
					{
						throw;
					}
				}
				designerHost.DestroyComponent(selectedTab);
				RaiseComponentChanged(member, null, null);
			}
			finally
			{
				if (designerTransaction != null)
				{
					designerTransaction.Commit();
				}
			}
		}

		protected override void OnPaintAdornments(PaintEventArgs pe)
		{
			try
			{
				disableDrawGrid = true;
				base.OnPaintAdornments(pe);
			}
			finally
			{
				disableDrawGrid = false;
			}
		}

		void OnControlAdded(object sender, ControlEventArgs e)
		{
			if (e.Control == null || e.Control.IsHandleCreated)
			{
				return;
			}

			IntPtr handle = e.Control.Handle;
		}

		void OnRightToLeftLayoutChanged(object sender, EventArgs e)
		{
			if (BehaviorService == null)
			{
				return;
			}

			BehaviorService.SyncSelection();
		}

		void OnSelectionChanged(object sender, EventArgs e)
		{
			ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
			tabControlSelected = false;
			if (selectionService == null)
			{
				return;
			}

			ICollection selectedComponents = selectionService.GetSelectedComponents();
			TabControl tabControl = (TabControl)Component;
			foreach (object comp in (IEnumerable)selectedComponents)
			{
				if (comp == tabControl)
				{
					tabControlSelected = true;
				}

				TabPage tabPageOfComponent = TabControlDesigner.GetTabPageOfComponent(comp);
				if (tabPageOfComponent != null && tabPageOfComponent.Parent == tabControl)
				{
					tabControlSelected = false;
					tabControl.SelectedTab = tabPageOfComponent;

					Type selectionManagerType = SystemDesignLoader.SystemDesignAssembly.GetType("System.Windows.Forms.Design.Behavior.SelectionManager", true);
					object manager1 = GetService(selectionManagerType);
					selectionManagerType.GetMethod("Refresh").Invoke(manager1, null);

					break;
				}
			}
		}

		void OnTabSelectedIndexChanged(object sender, EventArgs e)
		{
			ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
			if (selectionService == null)
			{
				return;
			}

			ICollection selectedComponents = selectionService.GetSelectedComponents();
			TabControl tabControl = (TabControl)Component;
			bool flag = false;
			foreach (object comp in (IEnumerable)selectedComponents)
			{
				TabPage tabPageOfComponent = TabControlDesigner.GetTabPageOfComponent(comp);
				if (tabPageOfComponent != null && tabPageOfComponent.Parent == tabControl && tabPageOfComponent == tabControl.SelectedTab)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				return;
			}

			selectionService.SetSelectedComponents(new object[1]
	  {
		Component
	  });
		}

		protected override void PreFilterProperties(IDictionary properties)
		{
			base.PreFilterProperties(properties);
			string[] strArray = new string[1]
	  {
		"SelectedIndex"
	  };
			Attribute[] attributeArray = Array.Empty<Attribute>();
			for (int index = 0; index < strArray.Length; ++index)
			{
				PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor)properties[strArray[index]];
				if (oldPropertyDescriptor != null)
				{
					properties[strArray[index]] = TypeDescriptor.CreateProperty(typeof(TabControlDesigner), oldPropertyDescriptor, attributeArray);
				}
			}
		}

		ParentControlDesigner GetSelectedTabPageDesigner()
		{
			ParentControlDesigner tabPageDesigner = null;
			TabPage selectedTab = ((TabControl)Component).SelectedTab;
			if (selectedTab != null)
			{
				IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
				if (designerHost != null)
				{
					tabPageDesigner = designerHost.GetDesigner(selectedTab) as ParentControlDesigner;
				}
			}
			return tabPageDesigner;
		}

		protected override void OnDragEnter(DragEventArgs de)
		{
			forwardOnDrag = false;
			object behaviorDataObject = de.Data;
			Type behaviourDataObjectType = SystemDesignLoader.SystemDesignAssembly.GetType("System.Windows.Forms.Design.Behavior.DropSourceBehavior+BehaviorDataObject");
			if (behaviorDataObject != null)
			{
				int primaryControlIndex = -1;
				ArrayList sortedDragControls = (ArrayList)behaviourDataObjectType.GetMethod("GetSortedDragControls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(behaviorDataObject, new object[] { primaryControlIndex });
				if (sortedDragControls != null)
				{
					for (int index = 0; index < sortedDragControls.Count; ++index)
					{
						if (!(sortedDragControls[index] is Control) || sortedDragControls[index] is Control && !(sortedDragControls[index] is TabPage))
						{
							forwardOnDrag = true;
							break;
						}
					}
				}
			}
			else
			{
				forwardOnDrag = true;
			}

			if (forwardOnDrag)
			{
				ParentControlDesigner selectedTabPageDesigner = GetSelectedTabPageDesigner();
				if (selectedTabPageDesigner == null)
				{
					return;
				}

				selectedTabPageDesigner.GetType().GetMethod("OnDragEnterInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(selectedTabPageDesigner, new object[] { de });
			}
			else
			{
				base.OnDragEnter(de);
			}
		}

		protected override void OnDragDrop(DragEventArgs de)
		{
			if (forwardOnDrag)
			{
				ParentControlDesigner selectedTabPageDesigner = GetSelectedTabPageDesigner();
				if (selectedTabPageDesigner != null)
				{
					selectedTabPageDesigner.GetType().GetMethod("OnDragDropInternal", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(selectedTabPageDesigner, new object[] { de });
				}
			}
			else
			{
				base.OnDragDrop(de);
			}

			forwardOnDrag = false;
		}

		protected override void OnDragLeave(EventArgs e)
		{
			if (forwardOnDrag)
			{
				ParentControlDesigner selectedTabPageDesigner = GetSelectedTabPageDesigner();
				if (selectedTabPageDesigner != null)
				{
					selectedTabPageDesigner.GetType().GetMethod("OnDragLeaveInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(selectedTabPageDesigner, new object[] { e });
				}
			}
			else
			{
				base.OnDragLeave(e);
			}

			forwardOnDrag = false;
		}

		protected override void OnDragOver(DragEventArgs de)
		{
			if (forwardOnDrag)
			{
				if (!Control.DisplayRectangle.Contains(Control.PointToClient(ControlDpiScalingHelper.NewScaledPoint(de.X, de.Y, false))))
				{
					de.Effect = DragDropEffects.None;
				}
				else
				{
					ParentControlDesigner selectedTabPageDesigner = GetSelectedTabPageDesigner();
					if (selectedTabPageDesigner == null)
					{
						return;
					}

					selectedTabPageDesigner.GetType().GetMethod("OnDragOverInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(selectedTabPageDesigner, new object[] { de });
				}
			}
			else
			{
				base.OnDragOver(de);
			}
		}

		protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
		{
			if (forwardOnDrag)
			{
				ParentControlDesigner selectedTabPageDesigner = GetSelectedTabPageDesigner();
				if (selectedTabPageDesigner == null)
				{
					return;
				}

				selectedTabPageDesigner.GetType().GetMethod("OnGiveFeedbackInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(selectedTabPageDesigner, new object[] { e });
			}
			else
			{
				base.OnGiveFeedback(e);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "m.Result = (IntPtr)1; is not a redundant cast")]
		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case 123:
					int x = SignedLOWORD((int)(long)m.LParam);
					int y = SignedHIWORD((int)(long)m.LParam);
					if (x == -1 && y == -1)
					{
						Drawing.Point position = Cursor.Position;
						x = position.X;
						y = position.Y;
					}
					OnContextMenu(x, y);
					break;
				case 132:
					base.WndProc(ref m);
					if ((int)(long)m.Result != -1)
					{
						break;
					}

					m.Result = (IntPtr)1;
					break;
				case 276:
				case 277:
					BehaviorService.Invalidate(BehaviorService.ControlRectInAdornerWindow(Control));
					base.WndProc(ref m);
					break;
				default:
					base.WndProc(ref m);
					break;
			}
		}

		static int SignedHIWORD(int n)
		{
			int num1 = (short)((n >> 0x10) & 0xffff);
			num1 <<= 0x10;
			return (num1 >> 0x10);
		}

		static int SignedLOWORD(int n)
		{
			int num1 = (short)(n & 0xffff);
			num1 <<= 0x10;
			return (num1 >> 0x10);
		}
	}
}

