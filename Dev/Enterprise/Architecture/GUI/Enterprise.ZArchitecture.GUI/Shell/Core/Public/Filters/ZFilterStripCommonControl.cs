using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using Enterprise.Core.Modules;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	[Flags]
	public enum AutoRefreshWarningType
	{
		None = 0,
		SlowQuery = 1,
		ErrorQuery = 2,
	}

	[ToolboxItem(false)]
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public partial class ZFilterStripCommonControl : ZFilterStripBaseControl, IFilterControl, IFilterStripCommonControlInternalsForTesting, IDisposeStackProvider
	{
		readonly StackTrace stripControlCreationStack;
		StackTrace recentItemsControlCreationStack;

		public ZFilterStripCommonControl()
		{
			InitializeComponent();
			InitialiseGrid();

			ShowSearchResultsMessageBox = true;
		}

		public ZFilterStripCommonControl(FilterStripBusinessObject filterBusinessObject)
			: base(filterBusinessObject)
		{
			InitializeComponent();
			InitialiseGrid();

			ShowSearchResultsMessageBox = true;

			stripControlCreationStack = new StackTrace();

			ToolStripPermissionsLabel.AllowOutsideOfParent();

			Grid.AllowOverlap(CoveringLabel);
			Grid.AllowOverlap(ToolStrip);
			Grid.AllowOverlap(ToolStripHelp);
			Grid.AllowOverlap(ToolStripRecordsFoundLabel);
			FilterStripsPanel.AllowOverlap(ToolStripPermissionsLabel);
		}

		#region Dispose()

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEventsOnRecentItems();
				DisposeOfRecentItems();
				UnhookFormEvents();
				components?.Dispose();
			}

			if (TrackDisposedAccess)
			{
				disposeStack = new StackTrace();
				disposeControlPath = ControlDescription.GetControlPath(this);
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Grid

		public
#if DEBUG
 virtual
#endif
 ZFilterGrid Grid
		{
			get { return grid ?? (grid = GetNewFilteredGrid()); }
		}

		protected ZFilterGrid grid;

		public sealed override ZGrid RelatedGrid
		{
			get { return Grid; }
		}

		protected virtual ZFilterGrid GetNewFilteredGrid()
		{
			return new ZDisplayGrid();
		}

		void InitialiseGrid()
		{
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			((ISupportInitialize)(this.Grid)).BeginInit();
			this.SuspendLayout();
			//
			// FilteredGrid
			//
			Grid.AllowNavigation = false;
			Grid.CaptionVisible = false;
			Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			Grid.LayoutKey = "FilteredGrid";
			Grid.Name = "FilteredGrid";
			Grid.ReadOnly = true;
			Grid.ShouldSetErrorsOnTabPage = false;
			Grid.BorderStyle = System.Windows.Forms.BorderStyle.None;

			//
			// ControlForLayout
			//
			ControlForLayout.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			ControlForLayout.Location = ControlDpiScalingHelper.NewScaledPoint(0, 152);
			ControlForLayout.Size = ControlDpiScalingHelper.NewScaledSize(758, 22);
			ControlForLayout.TabIndex = 6;

			InitialiseGridCore();

			this.Controls.Add(this.ControlForLayout);
			this.Controls.SetChildIndex(this.ControlForLayout, 0);
			((ISupportInitialize)(this.Grid)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected virtual void InitialiseGridCore()
		{
		}

		#endregion

		#region Search

		protected sealed override bool ShouldClearCollectionResultsOnFindButtonDropDownItemClicked { get { return true; } }

		protected override bool GetDefaultShouldRunSearchOnStripsInitialized()
			=> !this.IsDesignMode() && EnvProxy.Instance.Registry.RunSearchOnEnteringAModule;

		public bool RunSearchOnEnteringAModuleOverride
		{
			get => ShouldRunSearchOnStripsInitialized;
			set => ShouldRunSearchOnStripsInitialized = value;
		}

		#endregion

		public string GetToolStripRecordsFoundLabelText()
		{
			return ToolStripRecordsFoundLabel.Text;
		}

		public void SetToolStripPermissionsLabel(bool hasPermissions)
		{
			if (!hasPermissions)
			{
				this.ToolStripPermissionsLabel.Text = Res.GetString("cc18adc3-0edb-48cb-9b4e-cd9cefb97bc3", "You do not have the appropriate security rights to view this module.");
			}
			else
			{
				this.ToolStripPermissionsLabel.Text = "";
			}
		}

		#region Layout

		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);
			HandleGridSizing();
		}

		protected virtual void HandleGridSizing()
		{
			if (ControlForLayout.Left != 0 || ControlForLayout.Right != ClientRectangle.Right || ControlForLayout.Bottom != ClientRectangle.Bottom)
			{
				ControlForLayout.Location = ControlDpiScalingHelper.NewScaledPoint(0, ControlForLayout.Top, false);
				ControlDpiScalingHelper.SetHeight(ControlForLayout, ClientSize.Height - ControlForLayout.Top, false);
				ControlDpiScalingHelper.SetWidth(ControlForLayout, ClientSize.Width, false);
			}
		}

		#endregion

		#region Recent Items

		public void LoadRecentItems()
		{
			if (Grid != null && FilterModule != null && FilterModule.LimitedColumns == null)
			{
				recentItemsControlCreationStack = new StackTrace();

				SetupRecentItemsControl();
				var removeAllLinksMenuItem = new ZMenuItem(ResString.GetMultilingualString("7BE7A502-CEB5-4d3f-B906-80992C3A31F9", "Remove All Links"),
					(sender, args) =>
					{
						var message = ResString.GetMultilingualString("0B281567-8ED7-4e35-90FC-CBFD0EFE2A2E", "Are you sure you want to remove all links?");
						var caption = ResString.GetMultilingualString("9F7A70DE-62FD-4812-A5EB-A6AD22133FDC", "Remove All Links");
						var result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

						if (result == DialogResult.Yes)
						{
							RecentItemManager.Instance.RemoveAllRecentItems(FilterModule.RecentItemsModuleID.Name);
						}
					});
				RecentItemsLabel.ContextMenu.MenuItems.Add(removeAllLinksMenuItem);
				RefreshRecentItems();
				HookEventsOnRecentItems();
			}
		}

		public virtual void HookFormEvents()
		{
		}

		public virtual void UnhookFormEvents()
		{
		}

		void RecentItemsChangedHandler(object sender, RecentItemsChangedEventArgs args)
		{
			if (FilterModule?.RecentItemsModuleID != null)
			{
				if (args.Module == FilterModule.RecentItemsModuleID.Name && IsHandleCreated && !IsDisposed)
				{
					if (InvokeRequired)
					{
						BeginInvoke(RefreshRecentItems);
					}
					else
					{
						RefreshRecentItems();
					}
				}
			}
		}

		void HookEventsOnRecentItems()
		{
			if (!this.IsDesignMode())
			{
				RecentItemManager.RecentItemsChanged += RecentItemsChangedHandler;
			}
		}

		void UnhookEventsOnRecentItems()
		{
			if (!this.IsDesignMode())
			{
				RecentItemManager.RecentItemsChanged -= RecentItemsChangedHandler;
			}
		}

		void DisposeOfRecentItems()
		{
			try
			{
				if (RecentItemsControl != null)
				{
					var viewModel = RecentItemsControl.DataContext as CargoWise.Main.Navigation.MenuSection;
					viewModel?.Dispose();

					RecentItemsControl.DataContext = null;
				}
			}
			catch (InvalidOperationException ex)
			{
				var message = FormattableString.Invariant($@"{ex.Message}
IsHandleCreated: {this.IsHandleCreated}
InvokeRequired:{this.InvokeRequired}

ZFilterStripCommonControl Creation Stack:
{stripControlCreationStack}

RecentItemsControl Creation Stack:
{recentItemsControlCreationStack}"); // Error message

				throw new InvalidOperationException(message, ex);
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Reduction of complexity of 'if'-blocks here may lead to less readable code.")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "User Control Constant, exception message")]
		void RefreshRecentItems()
		{
			if (FilterModule?.RecentItemsModuleID != null)
			{
				DisposeOfRecentItems();

				var recent = ResString.GetMultilingualString("e145b73c-a868-4aad-8026-d6928e7d33d0", "Recent");
				var recentItemsViewModel = new CargoWise.Main.Navigation.MenuSection(recent.ToString(Res.CurrentLanguage), "Recent", recent, CargoWise.Main.Navigation.SectionType.RecentItem, RecentItemManager.Instance.MaximumNumberOfRecentItems);
				var recentItems = RecentItemManager.Instance.GetRecentItems(FilterModule.RecentItemsModuleID.Name);
				foreach (var recentItem in recentItems)
				{
					var gridCollection = FilterModule.GridCollection;
					var elementType = gridCollection.TypeOfElements;
					if (recentItem.STL_ItemPK.IsValid &&
						((IZFilterGridModule)FilterModule).AllowTemplateRecords && !FilterModule.AllowLoadTemplateRecords &&
							FilterModule.FilterBusinessObject.Factory != null && gridCollection != null)
					{
						var tableSchema = BusinessObjectFactory.GetTableSchemaFromType(elementType);
						if (tableSchema != null)
						{
							var query = new ZQuery(tableSchema.PK, recentItem.STL_ItemPK);
							if (!FilterModule.FilterBusinessObject.Factory.ExistsInDatabase(tableSchema.TableName, query)) // Do not use Exists() - it loads bizos into memory, and in wrong factory
							{
								continue;
							}
						}
					}

					var shortcut = new LinkWrapper(recentItem);
					var recordKey = shortcut.RecordKey;
					var favoriteProvider = ObjectFactory.Get<IFavoriteProvider>();
					var menuItem = new CargoWise.Main.Navigation.MenuItem(
						shortcut.UniqueKey,
						(NoResString)shortcut.RecordDescription,
						() =>
						{
							var openWithWindowPersister = true;

							var factory = gridCollection != null && FilterModule.ModuleDecisionProvider != null ? FilterModule.FilterBusinessObject?.Factory?.CreateNewFactory() : null; // Do not load bizos into filter's factory - create new one
							if (factory != null)
							{
								BusinessObject bizO = null;

								try
								{
									var loader = gridCollection as IBusinessObjectLoader;
									bizO = loader?.Load(factory, recordKey) ?? factory.Load(elementType, recordKey);
								}
								catch (ApplicationException e)
								{
									if (e.GetInnermostException().Message.Contains("StorageMain can only be loaded in a DocumentFactory"))
									{
										var provider = ObjectFactory.Get<IDocumentFactoryProvider>();
										var docFactory = (BusinessObjectFactory)provider.GetFactory(factory);
										bizO = docFactory.Load(elementType, recordKey);
									}
									else
									{
										throw;
									}
								}

								if (bizO == null && ((IZFilterGridModule)FilterModule).AllowTemplateRecords)
								{
									bizO = FilterModule.LoadFromTemplateRecordPk(factory, recordKey);
								}

								if (bizO != null)
								{
									openWithWindowPersister = false;
									using ((FilterModule.ModuleDecisionProvider as IHaveSpecialDefaultAction)?.DefineDefaultActionTriggeredByRecentItems())
									using (FilterModule.SetRecentItemsSelection())
									{
										FilterModule.ModuleDecisionProvider.HandleDefaultAction(new[] { bizO });
									}
								}
							}

							if (openWithWindowPersister)
							{
								WindowPersister.OpenFormsFromUrls(shortcut.RecordUrl);
							}
						},
						() =>
						{
							var menu = new ContextMenu();

							if (components == null)
							{
								components = new Container();
							}

							components.Add(menu);
							var removeLinkMenuItem = new ZMenuItem(
								ResString.GetMultilingualString("5A73435B-429F-4f59-9160-B3692E53F5C8", "Remove Link"),
								(senderObject, args) =>
								{
									using (((ZMenuItem)senderObject).Parent)
									{
										var message = ResString.GetMultilingualString("EAF53BEF-BE58-474f-8505-4DA52FFE58FF", "Are you sure you want to remove this link?");
										var caption = ResString.GetMultilingualString("51EC31F1-9B20-4b7d-A704-0576558B4DF6", "Remove Link");
										var context = new DialogDefaultContext(new ZGuid("393AD517-C0D2-45D8-AFA4-0F24DE2F7F87"), caption, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question, showCheckboxOnly: true);
										var result = Globals.Message.ShowOrDefault(context, message);

										if (result == ZDialogResult.Yes)
										{
											RecentItemManager.Instance.RemoveFromRecentItems(FilterModule.RecentItemsModuleID.Name, shortcut);
										}
									}
								});

							menu.MenuItems.Add(removeLinkMenuItem);
#if WINZOR
							var position = MousePosition;
#else
							var position = this.PointToClient(MousePosition);
#endif
							menu.Show(this, position);
						},
						null);

					menuItem.SetFavoriteAction(() =>
					{
						if (favoriteProvider != null)
						{
							if (RecentItemManager.Instance.IsInFavoriteModules(shortcut))
							{
								favoriteProvider.DeleteFromFavorites(shortcut);
								menuItem.IsInFavorites = false;
							}
							else
							{
								if (favoriteProvider.AddToFavorites(shortcut))
								{
									menuItem.IsInFavorites = true;
								}
							}
						}
					});

					menuItem.IsInFavorites = RecentItemManager.Instance.IsInFavoriteModules(shortcut);
					recentItemsViewModel.Items.Add(menuItem);
				}

				RecentItemsControl.DataContext = recentItemsViewModel;
			}
		}

		protected ZFilterGridModule FilterModule => filterModule ?? (filterModule = Grid.ParentModule as ZFilterGridModule);

		ZFilterGridModule filterModule;

		#endregion

		#region IFilterControl Members

		ZFilterGrid IFilterControl.FilteredGrid
		{
			get { return Grid; }
		}

		Control IFilterControl.OuterFilterControl
		{
			get { return this; }
		}

		void IFilterControl.CommitAllFilters()
		{
			if (Controls != null && Controls[0] != null)
			{
				Controls[0].Focus();
			}
		}

		#endregion

#if DEBUG
		#region IFilterStripCommonControlInternalsForTesting Members

		void IFilterStripCommonControlInternalsForTesting.Bind()
		{
			Bind();
		}

		#endregion
#endif

		public void HideManageLayoutsButton()
		{
			ToolStripManageDropButton.Visible = false;
		}

		public void HideSaveLayoutsButton()
		{
			ToolStripSaveLayoutButton.Visible = false;
		}

		#region Disposed access tracking

		[DefaultValue(false)]
		[Browsable(false)]
		public bool TrackDisposedAccess { get; set; } = true;

		public StackTrace DisposeStack => disposeStack;
		StackTrace disposeStack;

		public string DisposeControlPath => disposeControlPath;
		string disposeControlPath;

		#endregion
	}
}
