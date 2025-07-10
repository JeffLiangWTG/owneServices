using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.PlugIn.Testing
{
	sealed class DummyPlugIn3 : ZPlugIn, DummyInterface1, DummyInterface3
	{
		public DummyPlugIn3(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			TopLevelBizO = new BusinessObjectFactory().New(typeof(DummyBusinessObject));
		}

		protected internal override ZBool HasUserControl
		{
			get { return true; }
		}

		public bool DisableUserControl;

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return (LicenceCheckpoint)EnvProxy.Instance.Licence.AlwaysAllow; }
		}

		public int CurrentChangedCount;
		protected override void OnCurrentChanged()
		{
			CurrentChangedCount++;
			base.OnCurrentChanged();
		}

		public BusinessObject GetCurrent()
		{
			return Current;
		}

		public ZPlugIn[] GetOtherPlugInsWhichImplementExposed(Type @interface)
		{
			return GetOtherPlugInsWhichImplement(@interface);
		}

		[ToolboxItem(false)]
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

		internal Control Container;

		protected internal override Control GetNewUserControl()
		{
			if (!DisableUserControl)
			{
				Container = new TestUserControl(TopLevelBizO);
			}
			return Container;
		}

		readonly IBusiness TopLevelBizO;

		protected internal override IBusiness GetBusinessEntityForPlugIn()
		{
			return TopLevelBizO;
		}

		readonly MenuItem Menu1 = new ZMenuItem("PlugIn3");
		readonly MenuItem Menu2 = new ZMenuItem("PlugIn3-Menu2");

		protected override MenuItem[] AllTopLevelMenus
		{
			get
			{
				var result = new List<MenuItem>();
				result.Add(Menu1);
				result.Add(Menu2);
				return result.ToArray();
			}
		}

		public override string Name
		{
			get { return "PlugIn3"; }
		}

		public override bool CanDelete
		{
			get
			{
				return true;
			}
		}

		public override void Delete()
		{
			BusinessEntity.Delete();
		}

		public bool ThrowExceptionInShowPreSaveDialogs;

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			if (ThrowExceptionInShowPreSaveDialogs)
			{
				throw new ZException("Shouldn't show pre save dialogs");
			}

			return ContinueWithSave.Yes;
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

		public int OnIsFormEditableChangedCount;
		protected override void OnIsFormEditableChanged()
		{
			OnIsFormEditableChangedCount++;
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
