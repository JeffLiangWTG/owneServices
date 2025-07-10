using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Provides a Next/Previous control that allows moving forwards and backwards through a given set of business objects.
	/// </summary>
	[SuppressFormDesignerAnalysis]
	public partial class ZPreviousNextControl : ZUserControl, IPostOrCancel
	{
		/// <summary>
		/// Only for the designer - non workable
		/// </summary>
		public ZPreviousNextControl()
		{
			InitializeComponent();
			TabStop = false;
			Visible = false;
		}

		public ZPreviousNextControl(ModuleResultsBusinessObject businessEntity, ZController controller)
		{
			InitializeComponent();
			SetIconsOnButtons();
			this.NextButton.DoNoOverrideMyEditableMode = true;
			this.PreviousButton.DoNoOverrideMyEditableMode = true;
			this.BusinessEntity = businessEntity;
			this.Controller = controller;
			HookEvents();
			UpdateVisible();
			TabStop = false;
		}

		public readonly ZController Controller;
		protected readonly ModuleResultsBusinessObject BusinessEntity;

		#region GUI

		void SetIconsOnButtons()
		{
			SetupNavigationButtonIcons(NextButton, IconTypes.BlackWhite_Forward);
			SetupNavigationButtonIcons(PreviousButton, IconTypes.BlackWhite_Back);
		}

		void SetupNavigationButtonIcons(Button button, IconTypes imageType)
		{
			button.Image = new Bitmap(Icons.GetImage(imageType), CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 15));
		}

		void UpdateVisible()
		{
			Visible = BusinessEntity.IsCurrentPKInResults;
		}

		[DefaultValue(false)]
		public new bool TabStop
		{
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		#endregion

		#region Binding

		public void Bind()
		{
			base.SetDataBinding(BusinessEntity, "");
			NextButton.DataBindings.Add(new KBinding("IsEnabledForBinding", BusinessEntity, "CanMoveNext"));
			PreviousButton.DataBindings.Add(new KBinding("IsEnabledForBinding", BusinessEntity, "CanMovePrevious"));
		}

		/// <summary>
		/// Don't want to do anything with standard binding, we need to bind to our internal objects.
		/// </summary>
		public override void SetDataBinding(object dataSource, string dataMember)
		{
		}

		#endregion

		#region Events

		void UnhookEvents()
		{
			if (HookedEvents)
			{
				BusinessEntity.CurrentRecordNumberChanging -= new ModuleResultsBusinessObject.NumberChangingEvent(BusinessEntity_CurrentRecordNumberChanging);
				BusinessEntity.CurrentRecordNumberChanged -= new EventHandler(BusinessEntity_CurrentRecordNumberChanged);
				BusinessEntity.PKListChanged -= new EventHandler(BusinessEntity_PKListChanged);
				HookedEvents = false;
			}
		}
		bool HookedEvents;

		void HookEvents()
		{
			BusinessEntity.CurrentRecordNumberChanging += new ModuleResultsBusinessObject.NumberChangingEvent(BusinessEntity_CurrentRecordNumberChanging);
			BusinessEntity.CurrentRecordNumberChanged += new EventHandler(BusinessEntity_CurrentRecordNumberChanged);
			BusinessEntity.PKListChanged += new EventHandler(BusinessEntity_PKListChanged);
			HookedEvents = true;
		}

		void NextButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.MoveNext();
		}

		void PreviousButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.MovePrevious();
		}

		void BusinessEntity_PKListChanged(object sender, EventArgs e)
		{
			UpdateVisible();
		}
		#endregion

		#region Switching Forms

		IZForm OpenFormForCurrent()
		{
			IZForm openedForm = null;
			if (Controller.ModuleID != null)
			{
				var bizObj = GetBusinessObjectToEdit();
				if (bizObj != null)
				{
					using (var module = ZModuleFactory.Instance.Create(Controller.ModuleID))
					{
						var filterGridModule = module as ZFilterModule;
						if (filterGridModule != null)
						{
							openedForm = OpenForm(bizObj, filterGridModule);
						}
						else
						{
							Globals.Message.ShowInformation(Res.GetString("08456833-68f0-4182-9109-8071f28f61e8", "This Form does not support moving previous or next."));
						}
					}
				}
			}
			else
			{
				throw new NotSupportedException("ModuleID is null!");
			}
			return openedForm;
		}

		IZForm OpenForm(BusinessObject bizObj, ZFilterModule module)
		{
			module.ModuleResultsPKCollection = BusinessEntity.PKList;

			if (Form.TopLevelTabControl != null && Form.TopLevelTabControl.SelectedTab != null)
			{
				var currentlySelectedTabName = Form.TopLevelTabControl.SelectedTab.Name;
				module.InitialTabPageNameToSelectWhenAFormIsShown = currentlySelectedTabName;
			}

			IZForm openedForm = null;
			switch (Form.DisplayMode)
			{
				case ODisplayMode.Delete:
					openedForm = module.ShowDeleteForm(bizObj);
					break;

				case ODisplayMode.ReadOnly:
					openedForm = module.ShowViewForm(bizObj);
					break;

				default:
					openedForm = module.ShowEditForm(bizObj);
					break;
			}
#if DEBUG
			LastOpenedFormForTesting = openedForm;
#endif
			return openedForm;
		}

#if DEBUG
		internal IZForm LastOpenedFormForTesting;
#endif

		protected virtual BusinessObject GetBusinessObjectToEdit()
		{
			var factory = Controller.Factory;
			var currentPK = BusinessEntity.CurrentPK;
			var loader = Controller as IBusinessObjectLoader;
			return loader?.Load(factory, currentPK) ?? factory.Load(Controller.TypeOfTopLevelBusinessObject, currentPK);
		}

		ZForm Form
		{
			get
			{
				if (fForm == null)
				{
					fForm = (ZForm)FindForm();
				}
				return fForm;
			}
		}
		ZForm fForm;

		enum Continue { Yes, No }

		Continue WarnAndSave()
		{
			var result = Continue.Yes;

			if (Form.BusinessEntity.HasChanges)
			{
				var dlgResult = Globals.Message.Show(Res.GetString("051bc3c7-0c72-457a-8727-d410781745c5", "Changes to this record have not been saved. Do you wish to save before changing records?"), Res.GetString("b0cc96bf-62f3-4846-b345-d67088741029", "Save?"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
				if (dlgResult == DialogResult.Yes)
				{
					if (Form.FireSaveButton() == ContinueWithSave.No)
					{
						result = Continue.No;
					}
				}
				else if (dlgResult == DialogResult.Cancel)
				{
					result = Continue.No;
				}
			}

			return result;
		}

		void BusinessEntity_CurrentRecordNumberChanging(ModuleResultsBusinessObject.CurrentRecordNumberChangingEventArgs args)
		{
			args.Cancel = WarnAndSave() == Continue.No;
		}

		void BusinessEntity_CurrentRecordNumberChanged(object sender, EventArgs e)
		{
			if (!BusinessEntity.HasErrors)
			{
				Form.Invoke(new MethodInvoker(SwitchForms));
			}
		}

		void SwitchForms()
		{
			EnterpriseFormLookStrategy.SavePositionAndSize(Form);
			UnhookEvents();

			IZForm openedForm = null;
			try
			{
				openedForm = OpenFormForCurrent();
			}
			catch
			{
				HookEvents();
				throw;
			}

			if (openedForm == Form)
			{
				HookEvents();
			}
			else
			{
				Form.ForceClose();
				if (Form.Visible)
				{
					//For an unknown reason, the previous form does not close if you directly call form.ForceClose() after typing a new number in and hitting enter. Something to do with just having entered a keystroke, perhaps?
					//Doing it from another thread is the only way I can find to get around this.
					var thread = new Thread(() =>
					{
						Form.BeginInvokeSafe(() => Form.ForceClose());
					}
					);
					thread.Start();
#if DEBUG
					closingThread = thread;
#endif
				}
			}
		}

#if DEBUG
		internal Thread closingThread;
#endif

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			UnhookEvents();
			NextButton.ImageList = null;
			PreviousButton.ImageList = null;

			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Testing
#if DEBUG

		public void FireNextButtonForTesting()
		{
			if (ParentForm != null && !ParentForm.IsDisposed)
			{
				NextButton_Click(this, null);
			}
		}

		public void FirePreviousButtonForTesting()
		{
			if (ParentForm != null && !ParentForm.IsDisposed)
			{
				PreviousButton_Click(this, null);
			}
		}

		public ZButton NextButtonForTesting
		{
			get { return NextButton; }
		}

		public ZButton PreviousButtonForTesting
		{
			get { return PreviousButton; }
		}

#endif
		#endregion
	}
}
