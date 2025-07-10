using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.Licensing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.PRA.GUI
{
	public class AUContainerMessagingPlugIn : ZPlugIn, IAddColumnsToGrid
	{
		public AUContainerMessagingPlugIn(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
		{
		}

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		#endregion

		#region Current Changed

		protected override void OnCurrentChanged()
		{
			base.OnCurrentChanged();
			UpdateCollectionWithCurrent();

			if (fTopLevelMenu != null)
			{
				if (CurrentContainer != null)
				{
					fTopLevelMenu.PRAContainer = CurrentContainer;
				}
			}
		}

		void UpdateCollectionWithCurrent()
		{
			if (ContainerCollection.Count > 0)
			{
				ContainerCollection.RemoveAll();
			}

			if (CurrentContainer != null)
			{
				ContainerCollection.Add((BusinessObject)CurrentContainer);
			}
		}

		#endregion

		#region GUI

		protected override Control GetNewUserControl()
		{
			return new AUContainerMessagingUserControl();
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			if (fTopLevelMenu == null)
			{
				fTopLevelMenu = new AUContainerMessagingMenu();
				fTopLevelMenu.PRAContainer = CurrentContainer;
				fTopLevelMenu.MenuItems.Add("<PlaceHolderForPopup>");
			}
			return fTopLevelMenu;
		}

		public override void OnMenuShown()
		{
			base.OnMenuShown();
			fTopLevelMenu.ReloadMenuItems(HostBusinessEntity as BusinessObject);
		}

		AUContainerMessagingMenu fTopLevelMenu;

		#endregion

		#region Delete

		public override bool CanDelete
		{
			get { return false; }
		}

		public override string CannotDeleteMessage
		{
			get { return "Cannot Delete PRA Messages. These are maintained automatically."; }
		}

		#endregion

		#region IAddColumnsToGrid

		void IAddColumnsToGrid.AddColumnsToContainerGrid(ZGrid grid)
		{
			ZTextBoxColumnStyleInfo pRAStatusColumnInfo = new ZTextBoxColumnStyleInfo();
#if DEBUG
			TypeDescriptor.AddAttributes(pRAStatusColumnInfo, new SuppressFormsLocalizedTestAttribute());
#endif
			pRAStatusColumnInfo.Caption = "PRA Status";
			pRAStatusColumnInfo.ColumnName = "CurrentPRAStatus";
			pRAStatusColumnInfo.IsVisible = true;
			pRAStatusColumnInfo.ToolTip = "The current PRA Status.";
			ControlDpiScalingHelper.SetWidth(ref pRAStatusColumnInfo, 125, true);

			grid.ColumnStyles.Add(pRAStatusColumnInfo);
		}

		#endregion

		#region Implementation

		protected PRAContainerCollection ContainerCollection
		{
			get
			{
				if (fContainerCollection == null)
				{
					fContainerCollection = new PRAContainerCollection(Factory);
					UpdateCollectionWithCurrent();
				}
				return fContainerCollection;
			}
		}

		PRAContainerCollection fContainerCollection;

		public override string Name
		{
			get { return "PRA Messaging"; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return ContainerCollection;
		}

		protected override bool RegisterPlugInBusinessEntityAsEditable
		{
			get { return false; }
		}

		protected IPRAContainerMessaging CurrentContainer
		{
			get
			{
				if (IsCurrentDependent)
				{
					return (IPRAContainerMessaging)Current;
				}
				else if (HostBusinessEntity is IPRAContainerMessaging)
				{
					return (IPRAContainerMessaging)HostBusinessEntity;
				}
				return null;
			}
		}

		#endregion
	}
}
