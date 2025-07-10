using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.PlugIn
{
	public class PlugIns : IDisposable
	{
		public PlugIns(IBusiness businessEntity, ZTabControl topLevelTabControl)
		{
			fTopLevelTabControl = topLevelTabControl;
			fBusinessEntity = businessEntity;
		}

		public PlugIns(IBusiness businessEntity, ZForm form)
		{
			fBusinessEntity = businessEntity;
			fForm = form;
		}

		/// <summary>
		/// Adds a PlugIn to the form, based on Controller ID.
		/// </summary>
		/// <param name="plugInControllerID">The controller ID of the plugin.</param>
		public void Add(ControllerID plugInControllerID)
		{
			Add(new PlugInInfo(plugInControllerID, null, null, -1));
		}

		public void Add(ControllerID plugInControllerID, SecurityCheckpoint overrideSecurityCheckpointForPlugIn)
		{
			Add(new PlugInInfo(plugInControllerID, overrideSecurityCheckpointForPlugIn, null, -1));
		}

		public void Add(ControllerID plugInControllerID, SecurityCheckpoint overrideSecurityCheckpointForPlugIn, GetTopLevelTabControlDelegate getTopLevelTabControlDelegate)
		{
			Add(new PlugInInfo(plugInControllerID, overrideSecurityCheckpointForPlugIn, null, () => -1, () => null, getTopLevelTabControlDelegate));
		}

		public void AddCurrentDependentPlugIn(ControllerID plugInControllerID, ZGrid gridWithCurrent, int requestedIndex = -1)
		{
			if (gridWithCurrent == null)
			{
				throw new ArgumentNullException(nameof(gridWithCurrent), "GridWithCurrent cannot be null.");
			}

			Add(new PlugInInfo(plugInControllerID, null, gridWithCurrent, requestedIndex));
		}

		public void AddPlugInAtTabPageIndex(ControllerID plugInControllerID, SecurityCheckpoint overrideSecurityCheckpointForPlugIn, int requestedIndex)
		{
			AddPlugInAtTabPageIndex(plugInControllerID, overrideSecurityCheckpointForPlugIn, () => requestedIndex);
		}

		public void AddPlugInAtTabPageIndex(ControllerID plugInControllerID, SecurityCheckpoint overrideSecurityCheckpointForPlugIn, ZPlugIn.GetRequestedTabPageIndexdDelegate requestedTabPageIndexDeterminer)
		{
			Add(new PlugInInfo(plugInControllerID, overrideSecurityCheckpointForPlugIn, null, requestedTabPageIndexDeterminer, () => null));
		}

		public void AddPlugInAtTabPageIndex(ControllerID plugInControllerID, ZPlugIn.GetRequestedTabPageIndexdDelegate requestedTabPageIndexDeterminer)
		{
			AddPlugInAtTabPageIndex(plugInControllerID, null, requestedTabPageIndexDeterminer);
		}

		public void AddPlugInAtTabPageIndex(ControllerID plugInControllerID, int requestedIndex)
		{
			AddPlugInAtTabPageIndex(plugInControllerID, null, requestedIndex);
		}

		public void AddPlugInAtTabPageIndex(ControllerID plugInControllerID, int requestedIndex, GetBusinessEntityOverrideDelegate getBusinessEntityOverride)
		{
			AddPlugInAtTabPageIndex(plugInControllerID, null, () => requestedIndex, getBusinessEntityOverride);
		}

		public void AddPlugInAtTabPageIndex(ControllerID plugInControllerID, SecurityCheckpoint overrideSecurityCheckpointForPlugIn, ZPlugIn.GetRequestedTabPageIndexdDelegate requestedTabPageIndexDeterminer, GetBusinessEntityOverrideDelegate getBusinessEntityOverride)
		{
			Add(new PlugInInfo(plugInControllerID, overrideSecurityCheckpointForPlugIn, null, requestedTabPageIndexDeterminer, getBusinessEntityOverride));
		}

		public void AddPlugInAtTabPageIndex(ControllerID plugInControllerID, SecurityCheckpoint overrideSecurityCheckpointForPlugIn, ZPlugIn.GetRequestedTabPageIndexdDelegate requestedTabPageIndexDeterminer, GetBusinessEntityOverrideDelegate getBusinessEntityOverride, GetTopLevelTabControlDelegate getTopLevelTabControlDelegate)
		{
			Add(new PlugInInfo(plugInControllerID, overrideSecurityCheckpointForPlugIn, null, requestedTabPageIndexDeterminer, getBusinessEntityOverride, getTopLevelTabControlDelegate));
		}

		public void SelectPlugInTabPage(ControllerID plugInControllerID)
		{
			if (plugInControllerID == null)
			{
				throw new ArgumentNullException(nameof(plugInControllerID), "PlugInControllerID cannot be null");
			}

			var plugIn = GetInstance(plugInControllerID);
			if (plugIn != null)
			{
				plugIn.SelectTabPage();
			}
		}

		void Add(PlugInInfo info)
		{
			if (plugInsHaveBeenAddedToTabPages)
			{
				ErrorReporter.ReportOnce($"AttemptedToAddPlugInAfterPlugInsAddedToTabPages:{info.ID}:{fTopLevelTabControl?.GetTopLevelNonParentedControl().Name}", $"Cannot add a plug in after plug ins have been added to tab pages. Consider adding the plug in at an earlier time. AddPlugInTabPages CallStack: {addPlugInTabPagesCallStack}");
			}

			foreach (PlugInInfo pair in PlugInInfos)
			{
				if (pair.ID == info.ID)
				{
					throw new ZException("Cannot add the same plug in more than once. Controller ID: " + info.ID.ToString());
				}
			}

			PlugInInfos.Add(info);

			if (fTopLevelTabControl != null)
			{
				fTopLevelTabControl.PlugInMenusSetup = false;
			}
		}

		/// <summary>
		/// Created PlugIns.
		/// </summary>
		public ZPlugIn[] Instances
		{
			get
			{
				AddInstancesToHashByID();
				var result = new ZPlugIn[ZPlugIns.Count];

				// manual copy so we get the correct order
				var i = 0;
				foreach (PlugInInfo pair in PlugInInfos)
				{
					if (ZPlugIns[pair.ID] != null)
					{
						result[i] = (ZPlugIn)ZPlugIns[pair.ID];
						i++;
					}
				}

				return result;
			}
		}

		internal void AddPlugInTabPages(ZTabControl tabControl)
		{
			plugInsHaveBeenAddedToTabPages = true;
			addPlugInTabPagesCallStack = System.Environment.StackTrace;
			foreach (var plugIn in Instances)
			{
				var hostTabControl = (plugIn.IsInSubTabControl && plugIn.TopLevelTabControl != null) ? plugIn.TopLevelTabControl : tabControl;

				if (plugIn.Enabled && plugIn.HasUserControlInternal && !hostTabControl.TabPages.Contains(plugIn.TabPage))
				{
					var tabPage = plugIn.TabPage;
					var parentForm = hostTabControl.FindForm() as ZForm;

					parentForm?.CustomisePluginTab(tabPage, plugIn.ControllerID);

					var requestedTabPageIndex = plugIn.RequestedTabPageIndex;
					if (requestedTabPageIndex >= 0 && requestedTabPageIndex < hostTabControl.TabPages.Count)
					{
						hostTabControl.TabPages.Insert(tabPage, requestedTabPageIndex);
					}
					else
					{
						hostTabControl.TabPages.Add(tabPage);
					}
					ControlDpiScalingHelper.SetWidth(ref tabPage, hostTabControl.Width, false);
				}
			}
		}
		string addPlugInTabPagesCallStack;

		public ZPlugIn[] GetPlugInsWhichImplement(Type interfaceType)
		{
			return GetOtherPlugInsWhichImplement(interfaceType, null);
		}

		public ZPlugIn[] GetOtherPlugInsWhichImplement(Type interfaceType, ZPlugIn plugInToExclude)
		{
			var result = new List<ZPlugIn>();

			foreach (var plugIn in Instances)
			{
				if (plugIn != plugInToExclude && plugIn.Enabled && plugIn.GetType().GetInterface(interfaceType.Name, true) != null)
				{
					result.Add(plugIn);
				}
			}

			return result.ToArray();
		}

		public ZPlugIn GetPlugIn(ControllerID plugInControllerID)
		{
			return GetInstance(plugInControllerID);
		}

		/// <summary>
		/// Find a plugin in this collection by ControllerID, without forcing all the other plugins to load/create.
		/// </summary>
#if DEBUG
		public
#else
		internal
#endif
		bool IsPlugInAvailable(ControllerID controllerID)
		{
			foreach (PlugInInfo info in PlugInInfos)
			{
				if (info.ID == controllerID)
				{
					return true;
				}
			}
			return false;
		}

		public void OnBusinessObjectIsCancelledChanged(ZBool isCancelled)
		{
			foreach (var plugIn in Instances)
			{
				plugIn.OnBusinessObjectIsCancelledChanged(isCancelled);
			}
		}

		#region Implementation

		readonly ArrayList PlugInInfos = new ArrayList();
		readonly Hashtable ZPlugIns = new Hashtable();
		readonly IBusiness fBusinessEntity;
		readonly ZTabControl fTopLevelTabControl;
		readonly ZForm fForm;

		bool plugInsHaveBeenAddedToTabPages;

		ZPlugIn GetInstance(ControllerID plugInControllerID)
		{
			AddInstancesToHashByID();
			return ZPlugIns[plugInControllerID] as ZPlugIn;
		}

		IBusiness GetBusinessEntity(PlugInInfo info)
		{
			return info.GetBusinessEntityOverride() ?? fBusinessEntity;
		}

		void AddInstancesToHashByID()
		{
			lock (addingPlugInLock)
			{
				foreach (PlugInInfo info in PlugInInfos)
				{
					if (ZPlugIns[info.ID] == null)
					{
						var controller = ZControllerFactory.Create(info.ID);
						if (controller != null)
						{
							var plugIn = controller.GetPlugInInternal(GetBusinessEntity(info), info.CheckPoint);
							if (plugIn != null)
							{
								plugIn.RequestedTabPageIndexDeterminer = info.RequestedTabPageIndexDeterminer;
								plugIn.ParentCollection = this;

								var tabControl = info.GetTopLevelTabControl();
								if (tabControl == null)
								{
									tabControl = fTopLevelTabControl;
								}
								else
								{
									plugIn.IsInSubTabControl = true;
								}

								plugIn.InitializePlugin(tabControl, fForm);
								plugIn.DisplayMode = fParentFormDisplayMode;
								plugIn.HookFormEvents();
								if (info.CurrentChangedGrid != null)
								{
									plugIn.CurrentGrid = info.CurrentChangedGrid;
								}

								if (ZPlugIns.ContainsKey(info.ID))
								{
									ZPlugIns[info.ID] = plugIn;
								}
								else
								{
									ZPlugIns.Add(info.ID, plugIn);
								}
							}
						}
					}
				}
			}
		}

		static readonly object addingPlugInLock = new object();
		public delegate IBusiness GetBusinessEntityOverrideDelegate();
		public delegate ZTabControl GetTopLevelTabControlDelegate();

		#region PlugIn Info Structure

		class PlugInInfo
		{
			public PlugInInfo(ControllerID iD, SecurityCheckpoint checkPoint, ZGrid currentChangedGrid, int requestedTabPageIndex)
				: this(iD, checkPoint, currentChangedGrid, () => requestedTabPageIndex, () => null, () => null)
			{
			}

			public PlugInInfo(ControllerID iD, SecurityCheckpoint checkPoint, ZGrid currentChangedGrid, ZPlugIn.GetRequestedTabPageIndexdDelegate requestedTabPageIndexDeterminer, GetBusinessEntityOverrideDelegate getBusinessEntityOverride)
				: this(iD, checkPoint, currentChangedGrid, requestedTabPageIndexDeterminer, getBusinessEntityOverride, () => null)
			{
			}

			public PlugInInfo(ControllerID iD, SecurityCheckpoint checkPoint, ZGrid currentChangedGrid, ZPlugIn.GetRequestedTabPageIndexdDelegate requestedTabPageIndexDeterminer, GetBusinessEntityOverrideDelegate getBusinessEntityOverride, GetTopLevelTabControlDelegate getTopLevelTabControl)
			{
				this.ID = iD;
				this.CheckPoint = checkPoint;
				this.CurrentChangedGrid = currentChangedGrid;
				this.RequestedTabPageIndexDeterminer = requestedTabPageIndexDeterminer;
				this.GetBusinessEntityOverride = getBusinessEntityOverride;
				this.GetTopLevelTabControl = getTopLevelTabControl;
			}

			public readonly GetBusinessEntityOverrideDelegate GetBusinessEntityOverride;
			public readonly GetTopLevelTabControlDelegate GetTopLevelTabControl;
			public readonly ZPlugIn.GetRequestedTabPageIndexdDelegate RequestedTabPageIndexDeterminer;
			public readonly ControllerID ID;
			public readonly SecurityCheckpoint CheckPoint;
			public readonly ZGrid CurrentChangedGrid;
		}

		#endregion

		#region Display Mode

		internal ODisplayMode ParentFormDisplayMode
		{
			set
			{
				fParentFormDisplayMode = value;
				foreach (PlugInInfo pair in PlugInInfos)
				{
					if (ZPlugIns[pair.ID] != null)
					{
						((ZPlugIn)ZPlugIns[pair.ID]).DisplayMode = value;
					}
				}
			}
		}

		ODisplayMode fParentFormDisplayMode;

		#endregion

		#region Internal functions for aggregating PlugIns

		internal ITransactionParticipant[] FactoriesToBeSaved
		{
			get
			{
				var result = new List<ITransactionParticipant>();
				foreach (var plugIn in Instances)
				{
					if (plugIn.IsActive)
					{
						foreach (var transactionParticipant in plugIn.FactoriesToBeSaved)
						{
							if (transactionParticipant == null)
							{
								ErrorReporter.ReportOnce("NullInPlugInFactoriesToBeSaved", $"Plugin type: {plugIn.GetType().FullName}");
							}
							else if (!result.Contains(transactionParticipant))
							{
								MarkParticipantDebuggingName(transactionParticipant, plugIn.Name);
								result.Add(transactionParticipant);
							}
						}
					}
				}
				return result.ToArray();
			}
		}

		void MarkParticipantDebuggingName(ITransactionParticipant transactionParticipant, string name)
		{
			if (transactionParticipant is BusinessObjectFactory factory && string.IsNullOrEmpty(factory.NameForDebugging))
			{
				factory.NameForDebugging = "PlugIn: " + name;
			}
		}
		internal void SynchronisePlugInsOnSave()
		{
			foreach (var plugIn in Instances)
			{
				plugIn.OnSavingInternal();
			}
		}

		internal void SynchronisePlugInsOnSaveCompletedOrAborted(bool saved)
		{
			foreach (var plugIn in Instances)
			{
				plugIn.OnSaveCompletedOrAbortedInternal(saved);
			}
		}

		internal void Delete()
		{
			foreach (var plugIn in Instances)
			{
				plugIn.DeleteInternal();
			}
		}

		internal ContinueWithSave ShowPreSaveDialogs()
		{
			var result = ContinueWithSave.Yes;

			foreach (var plugIn in Instances)
			{
				if (plugIn.ShowPreSaveDialogs() == ContinueWithSave.No)
				{
					result = ContinueWithSave.No;
					break;
				}
			}

			return result;
		}

		internal ZForm.ContinueWithDelete ShowPreDeleteDialogs()
		{
			var result = ZForm.ContinueWithDelete.Yes;

			foreach (var plugIn in Instances)
			{
				if (!plugIn.CanDelete)
				{
					Globals.Message.Show(plugIn.CannotDeleteMessage, Res.GetString("8ef2a7ca-0cf8-4f54-b69a-bc8dcf7aef60", "Cannot Delete..."), MessageBoxButtons.OK, MessageBoxIcon.Error);
					result = ZForm.ContinueWithDelete.No;
					break;
				}
			}

			return result;
		}

		#region PlugInMenu Info Structure

		internal struct PlugInMenuInfo
		{
			public PlugInMenuInfo(ControllerID iD, MenuItem[] menuItems)
			{
				this.ID = iD;
				this.MenuItems = menuItems;
			}

			public readonly ControllerID ID;
			public readonly MenuItem[] MenuItems;
		}

		#endregion

		internal PlugInMenuInfo[] TopLevelMenus
		{
			get
			{
				var result = new List<PlugInMenuInfo>();
				foreach (PlugInInfo plugInInfo in PlugInInfos)
				{
					var plugIn = GetInstance(plugInInfo.ID);
					if (plugIn != null)
					{
						result.Add(new PlugInMenuInfo(plugInInfo.ID, plugIn.TopLevelMenusInternal));
					}
				}
				return result.ToArray();
			}
		}

		internal void SetMenusVisible(bool visible)
		{
			foreach (var pluginMenuInfo in TopLevelMenus)
			{
				foreach (var menu in pluginMenuInfo.MenuItems)
				{
					if (visible != menu.Visible)
					{
						menu.Visible = visible;
					}
				}
			}
		}

		internal void SynchronisePlugInWithVisibleTabPage()
		{
			foreach (var plugIn in Instances)
			{
				plugIn.SynchroniseIfTabPageVisible();
			}
		}

		#endregion

		#endregion

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			foreach (ZPlugIn plugIn in ZPlugIns.Values)
			{
				plugIn.Dispose();
			}
		}

		#endregion
	}
}
