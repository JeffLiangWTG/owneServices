using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async.AsyncTaskContext.Public;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Module.ScheduleNow;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Module
{
	public class StmServiceTaskModule : ZFilterGridModule
	{
		public StmServiceTaskModule()
		{
			if (!Globals.IsTest)
			{
				ShouldPerformSearchAsync = true;
			}
		}

		public override bool AllowNew => false;
		public override bool AllowDelete => false;

		protected override void AddCopyMenuItem(List<MenuItem> menu)
		{
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			var menu = new List<MenuItem>
			{
				new ZMenuItem(ScheduleNowMenuText, ScheduleNowMenu_Click),
				new ZMenuItem("-"),
				new ZMenuItem(ActivateMenuText, ActivateMenu_Click),
				new ZMenuItem(DeactivateMenuText, DeactivateMenu_Click),
				new ZMenuItem("-")
			};

			menu.AddRange(base.GetNewAdditionalMenuItems());

			return menu.ToArray();
		}

		public override ToolBarButton[] ToolBarButtons
		{
			get
			{
				var buttons = base.ToolBarButtons;

				var activateButton = Array.Find(buttons, (button) => button.Text.Replace("&", "") == ActivateMenuText);
				activateButton.ImageIndex = Icons.GetImageIndex(IconTypes.Tick);

				var deactivateButton = Array.Find(buttons, (button) => button.Text.Replace("&", "") == DeactivateMenuText);
				deactivateButton.ImageIndex = Icons.GetImageIndex(IconTypes.Cross);

				var runNowButton = Array.Find(buttons, (button) => button.Text.Replace("&", "") == ScheduleNowMenuText);
				runNowButton.ImageIndex = Icons.GetImageIndex(IconTypes.Events);

				return Array.FindAll(buttons, (button) => Array.IndexOf(new string[] { "Delete", "Data Transfer" }, button.Text.Replace("&", "")) < 0);
			}
		}

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			// We load the collection twice via OnCustomGridLoad. Once without the web service status, and then with.
			var tasks = StmServiceTaskCollection.Load(factory, query, ObjectFactory.Get<IServiceTaskScheduleStatusProvider>());
			return PerformSearchResult.Success(factory, query, tasks, permitActiveCollectionUpdates: false);
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID => ModuleIDs.StmServiceTask;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.StmServiceTask);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new StmServiceTaskFilterControl(GridCollection, (StmServiceTaskFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new StmServiceTaskCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new StmServiceTaskFilterBusinessObject();
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				if (backgroundTaskContext != null)
				{
					backgroundTaskContext.Dispose();
				}

				if (serviceSubmenu != null)
				{
					serviceSubmenu.Dispose();
				}
			}
			base.Dispose(isDisposing);
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ServiceTask;

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		#endregion

		#region Delete
		protected override IZForm ShowDeleteForm(BusinessObject selectedBusinessObject)
		{
			return null;
		}

		#endregion

		#region Activate / Deactivate

		static MultilingualString ActivateMenuText => ResString.GetMultilingualString("4a0b09d7-bfb3-4103-b2af-0c00bea7143d", "Activate");

		static MultilingualString DeactivateMenuText => ResString.GetMultilingualString("6f3b9162-3441-456f-92e8-c0777649a799", "Deactivate");

		void ActivateMenu_Click(object sender, EventArgs e)
		{
			var controller = (StmServiceTaskController)GetNewController(null);
			controller.ActivateDeactivate(Grid.SelectedElements, true);
		}

		void DeactivateMenu_Click(object sender, EventArgs e)
		{
			var controller = (StmServiceTaskController)GetNewController(null);
			controller.ActivateDeactivate(Grid.SelectedElements, false);
		}

		#endregion

		#region RunNow

		static MultilingualString ScheduleNowMenuText => ResString.GetMultilingualString("4C555DEF-91A0-46F8-AD96-3861B25B7D75", "Schedule Now");

		void ScheduleNowMenu_Click(object sender, EventArgs e)
		{
			var controller = (StmServiceTaskController)GetNewController(null);
			controller.ScheduleNow(Grid.SelectedElements, new ScheduleNowController(), forceRestart: false);
		}

		#endregion

#if DEBUG
		internal
#endif
		bool ShouldShowMaintenanceItems()
		{
			return (EnvProxy.Instance.CurrentUser.IsSupportUser || !EnvProxy.IsHostedWithCargowise);
		}

		#region Menu Helpers

		internal interface IMenuProxy
		{
			IMenuProxy Add(string text, EventHandler onClick = null, string host = null, string dbServer = null, bool enabled = true, bool visible = true);
			void RemoveAll();
			int Count { get; }
			string Text { get; }
			string Host { get; }
			string DbServer { get; }
			bool Checked { get; set; }
			bool Enabled { set; }
			bool Visible { set; }
			event EventHandler Popup;
		}

		internal class MenuProxy : IMenuProxy
		{
			public MenuProxy(MenuItem menuItem)
			{
				menu = menuItem;
			}

			public IMenuProxy Add(string text, EventHandler onClick = null, string host = null, string dbServer = null, bool enabled = true, bool visible = true)
			{
				MenuItem menuItem = new ZMenuItem(text, onClick);
				menuItem.Tag = new[] { host, dbServer };
				menuItem.Enabled = enabled;
				menuItem.Visible = visible;
				menu.MenuItems.Add(menuItem);

				return new MenuProxy(menuItem);
			}

			public void RemoveAll()
			{
				for (var i = menu.MenuItems.Count - 1; i >= 0; i--)
				{
					menu.MenuItems.RemoveAt(i);
				}
			}

			public int Count => menu.MenuItems.Count;

			public string Text => menu.Text;

			public string Host => menu.Tag is string[] { Length: > 0 } data ? data[0] : null;

			public string DbServer => menu.Tag is string[] { Length: > 1 } data ? data[1] : null;

			public bool Checked
			{
				get { return menu.Checked; }
				set { menu.Checked = value; }
			}

			public event EventHandler Popup
			{
				add { menu.Popup += value; }
				remove { menu.Popup -= value; }
			}

			public bool Enabled
			{
				set { menu.Enabled = value; }
			}

			public bool Visible
			{
				set { menu.Visible = value; }
			}

			readonly MenuItem menu;
		}

		internal class ToolStripItemMenuProxy<T> : IMenuProxy where T : ToolStripItem
		{
			internal ToolStripItemMenuProxy(T component)
			{
				item = component;
			}

			public virtual IMenuProxy Add(string text, EventHandler onClick = null, string host = null, string dbServer = null, bool enabled = true, bool visible = true)
			{
				throw new NotSupportedException();
			}

			public virtual void RemoveAll()
			{
				throw new NotSupportedException();
			}

			public virtual int Count => 0;

			public string Text => item.Text;

			public string Host => item.Tag is string[] data && data.Length > 0 ? data[0] : null;

			public string DbServer => item.Tag is string[] data && data.Length > 1 ? data[1] : null;

			public virtual bool Checked
			{
				get => throw new NotSupportedException();
				set => throw new NotSupportedException();
			}

			public virtual event EventHandler Popup
			{
				add => throw new NotSupportedException();
				remove => throw new NotSupportedException();
			}

			public bool Enabled
			{
				set => item.Enabled = value;
			}

			public bool Visible
			{
				set => item.Visible = value;
			}

			protected readonly T item;
		}

		internal class ToolStripMenuProxy : ToolStripItemMenuProxy<ToolStripDropDownItem>
		{
			public ToolStripMenuProxy(ToolStripDropDownItem toolStripDropDownItem) : base(toolStripDropDownItem)
			{
			}

			public override IMenuProxy Add(string text, EventHandler onClick = null, string host = null, string dbServer = null, bool enabled = true, bool visible = true)
			{
				if (text == "-")
				{
					var separator = new ToolStripSeparator
					{
						Tag = new[] { host, dbServer },
						Enabled = enabled,
						Visible = visible,
					};
					item.DropDownItems.Add(separator);

					return new ToolStripItemMenuProxy<ToolStripItem>(separator);
				}

				var menuItem = new ZToolStripMenuItem(text, onClick)
				{
					Tag = new[] { host, dbServer },
					Enabled = enabled,
					Visible = visible,
				};
				item.DropDownItems.Add(menuItem);

				return new ToolStripMenuProxy(menuItem);
			}

			public override void RemoveAll()
			{
				for (var i = item.DropDownItems.Count - 1; i >= 0; i--)
				{
					item.DropDownItems.RemoveAt(i);
				}
			}

			public override int Count => item.DropDownItems.Count;

			public override bool Checked
			{
				get => ((ToolStripMenuItem)item).Checked;
				set => ((ToolStripMenuItem)item).Checked = value;
			}

			public override event EventHandler Popup
			{
				add => item.DropDownOpening += value;
				remove => item.DropDownOpening -= value;
			}
		}

		#endregion

#if DEBUG
		internal
#endif
		MenuItem serviceSubmenu;
		readonly ITaskContext backgroundTaskContext;

		#region Test

#if DEBUG

		public BusinessObjectFactory ExposedFactoryForTest => Factory;

#endif

		#endregion
	}
}
