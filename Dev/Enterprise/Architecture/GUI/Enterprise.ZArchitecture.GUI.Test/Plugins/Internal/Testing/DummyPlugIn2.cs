using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.PlugIn.Testing
{
	sealed class DummyPlugIn2 : ZAlwaysLoadPlugIn, DummyInterface1
	{
		public DummyPlugIn2(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			TopLevelBizO = new BusinessObjectFactory().New<DummyBusinessObject>();
			this.HostBusinessEntity = hostBusinessEntity;
		}
		public DummyBusinessObject TopLevelBizO;
		public new IBusiness HostBusinessEntity;

		public new ZBool IsFormEditable
		{
			get { return base.IsFormEditable; }
		}

		ZBool allowPlugInDisplayWithNoLicence;

		public void SetAllowPlugInDisplayWithNoLicence(ZBool value)
		{
			allowPlugInDisplayWithNoLicence = value;
		}

		protected override ZBool AllowPlugInDisplayWithNoLicence
		{
			get { return allowPlugInDisplayWithNoLicence; }
		}

		public ZBool PublicAllowPlugInDisplayWithNoLicence
		{
			get { return base.AllowPlugInDisplayWithNoLicence; }
		}

		internal override bool DelayBinding
		{
			get
			{
				if (OverrideDelayBinding)
				{
					return DelayBindingExposed;
				}

				return base.DelayBinding;
			}
		}

		public bool DelayBindingExposed;
		public bool OverrideDelayBinding;

		public bool ShouldDisplayPlugIn = true;
		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return ShouldDisplayPlugIn;
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
			base.OnGUIShown();
		}

		public bool ShouldBeAbleToGetUserControl = true;
		protected internal override Control GetNewUserControl()
		{
			fUserControl = ShouldBeAbleToGetUserControl ? new TestUserControl(TopLevelBizO) : null;
			return fUserControl;
		}

		protected internal override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return fLicenceCheckPoint; }
		}

		public void SetLicenceCheckPoint(LicenceCheckpoint checkPoint)
		{
			fLicenceCheckPoint = checkPoint;
		}

		LicenceCheckpoint fLicenceCheckPoint;

		public bool HasCreatedBusinessEntity;
		public bool ReturnNullForBusinessEntity;

		protected internal override IBusiness GetBusinessEntityForPlugIn()
		{
			HasCreatedBusinessEntity = true;
			return (ReturnNullForBusinessEntity) ? null : TopLevelBizO;
		}

		public void SelectTabPageExposed()
		{
			SelectTabPage();
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

		public int OnMenuShownCount;
		public override void OnMenuShown()
		{
			OnMenuShownCount++;
			base.OnMenuShown();
		}

		public int OnUserControlShownCount;
		public override void OnUserControlShown()
		{
			OnUserControlShownCount++;
			base.OnMenuShown();
		}

		[ToolboxItem(false)]
		public class TestUserControl : ZUserControl
		{
			public ZTextBox TextBox;

			public TestUserControl(IBusiness topLevelBusinessObject)
			{
				Name = "TestUserControl";
				TextBox = new ZTextBox();
				TextBox.BindTo = "Z0_Description";
				ControlDpiScalingHelper.SetWidth(ref TextBox, 200, true);
				ControlDpiScalingHelper.SetHeight(ref TextBox, 10, true);
				Controls.Add(TextBox);
			}
		}

		readonly MenuItem Menu = new ZMenuItem("PlugIn2");
		protected override MenuItem GetNewTopLevelMenu()
		{
			return Menu;
		}

		public override string Name
		{
			get { return fName; }
		}

		public void SetName(string name)
		{
			fName = name;
		}

		string fName = "PlugIn2";

		public override bool CanDelete
		{
			get { return (BusinessEntity as DummyBusinessObject).Z0_Description == "AllowDelete"; }
		}

		public override void Delete()
		{
			BusinessEntity.Delete();
		}

		public bool UserClickedCancelOnPreSaveDialog;
		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			if (UserClickedCancelOnPreSaveDialog)
			{
				return ContinueWithSave.No;
			}

			return base.ShowPreSaveDialogsCore();
		}

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
