using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn.Internal;

namespace Enterprise.ZArchitecture.PlugIn.Testing
{
	class DummyPlugIn1 : ZPlugInWithDragDropSupport, DummyInterface1
	{
		public DummyPlugIn1(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			TopLevelBizO = new BusinessObjectFactory().New<DummyBusinessObject>();
			this.HostBusinessEntity = hostBusinessEntity;
		}
		public DummyBusinessObject TopLevelBizO;

		public new IBusiness HostBusinessEntity;

		public ZPlugIn[] GetOtherPlugInsWhichImplementExposed(Type @interface)
		{
			return GetOtherPlugInsWhichImplement(@interface);
		}

		public override string Name
		{
			get { return "PlugIn1"; }
		}

		public int OnGUIShownCount;

		public static bool RegisterEditable = true;

		protected override bool RegisterPlugInBusinessEntityAsEditable
		{
			get { return RegisterEditable; }
		}

		public override void OnGUIShown()
		{
			OnGUIShownCount++;
		}

		public new ZBool IsFormEditable
		{
			get { return base.IsFormEditable; }
		}

		public ZLabel CoveringLabel
		{
			get { return ((IPlugInInternals)this).CoveringLabel; }
		}

		ZBool allowPlugInDisplayWithNoLicence;

		public void SetAllowPlugInDisplayWithNoLicence(ZBool value)
		{
			allowPlugInDisplayWithNoLicence = value;
		}

		protected override ZBool AllowPlugInDisplayWithNoLicence
		{
			get
			{
				return allowPlugInDisplayWithNoLicence;
			}
		}

		public int OnMenuShownCount;
		public override void OnMenuShown()
		{
			OnMenuShownCount++;
			base.OnMenuShown();

			if (ShowDataChangedMenusOnMenuShown)
			{
				Menu.MenuItems.Clear();
				if (HostBusinessEntity != null)
				{
					if (HostBusinessEntity.HasChanges)
					{
						Menu.MenuItems.Add("Data Changed");
					}
					else
					{
						Menu.MenuItems.Add("Data Unchanged");
					}
				}
			}
		}

		public bool ShowDataChangedMenusOnMenuShown = true;

		MenuItem Menu;
		protected override MenuItem GetNewTopLevelMenu()
		{
			Menu = new ZMenuItem("PlugIn1");
			Menu.MenuItems.Add("SubMenu");
			return Menu;
		}

		public class TestUserControl : ZUserControl
		{
			public ZTextBox TextBox;

			public TestUserControl(IBusiness topLevelBusinessObject)
			{
				TextBox = new ZTextBox();
				TextBox.BindTo = "Z0_Description";
				ControlDpiScalingHelper.SetWidth(ref TextBox, 200, true);
				ControlDpiScalingHelper.SetHeight(ref TextBox, 10, true);
				Controls.Add(TextBox);
			}
		}

		protected internal override Control GetNewUserControl()
		{
			fUserControl = new TestUserControl(TopLevelBizO);
			return fUserControl;
		}

		public bool HasUserControlExposed = true;
		protected internal override ZBool HasUserControl
		{
			get { return HasUserControlExposed; }
		}

		protected internal override IBusiness GetBusinessEntityForPlugIn()
		{
			AccessedTopLevelObject = true;
			return TopLevelBizO;
		}
		public bool AccessedTopLevelObject;

		public void SelectTabPageExposed()
		{
			SelectTabPage();
		}

		public bool fCanDelete = true; // exposed for testing
		public override bool CanDelete
		{
			get { return fCanDelete; }
		}

		public override void Delete()
		{
		}

		public int OnSavingCount;
		public override void OnSaving()
		{
			OnSavingCount++;
			base.OnSaving();
		}

		public int OnSaveCompleteOrAbortedCount;
		public bool? SaveWasCalled;
		public override void OnSaveCompletedOrAborted(bool saved)
		{
			OnSaveCompleteOrAbortedCount++;
			SaveWasCalled = saved;
			base.OnSaveCompletedOrAborted(saved);
		}

		public int OnUserControlShownCount;
		public override void OnUserControlShown()
		{
			OnUserControlShownCount++;
			base.OnMenuShown();
		}

		public new void ShowCoveringLabel(string text)
		{
			base.ShowCoveringLabel(text);
		}

		public new void HideCoveringLabel()
		{
			base.HideCoveringLabel();
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return fLicenceCheckPoint; }
		}

		public void SetLicenceCheckPoint(LicenceCheckpoint checkPoint)
		{
			fLicenceCheckPoint = checkPoint;
		}

		public int OnParentFormDragDropCount;
		protected override void OnParentFormDragDrop(ZForm form, DragEventArgs args)
		{
			OnParentFormDragDropCount++;
			base.OnParentFormDragDrop(form, args);
		}

		public int OnParentFormDragOverCount;
		protected override void OnParentFormDragOver(ZForm form, DragEventArgs args)
		{
			OnParentFormDragOverCount++;
			base.OnParentFormDragOver(form, args);
		}

		public int OnParentFormDataObjectPastedCount;
		protected override void OnParentFormDataObjectPasted(ZForm form, DataObjectPastedEventArgs e)
		{
			OnParentFormDataObjectPastedCount++;
			base.OnParentFormDataObjectPasted(form, e);
		}

		LicenceCheckpoint fLicenceCheckPoint = (LicenceCheckpoint)EnvProxy.Instance.Licence.AlwaysAllow;

		public override void OnBusinessObjectIsCancelledChanged(ZBool isCancelled)
		{
			fOnBusinessObjectIsCancelledChanged = isCancelled;
		}

		public ZBool IsCancelledCurrentValue
		{
			get
			{
				return fOnBusinessObjectIsCancelledChanged;
			}
		}
		ZBool fOnBusinessObjectIsCancelledChanged;
	}
}
