using System;
using System.Windows.Forms;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules
{
	public abstract class ZPopupModule : ZModule, IPopupModuleInternalsForTesting, IZPopupModule
	{
		protected ZPopupModule() { }

		protected abstract ZPopupController GetNewController();

		internal bool IsSingletonModule
		{
			get { return GetNewController() is ZSingletonController; }
		}

		public virtual void Show()
		{
			Show(null, true);
		}

		public void ShowModal(Form parentForm)
		{
			if (parentForm == null)
			{
				throw new ArgumentNullException(nameof(parentForm));
			}
			ShowNew(parentForm);
		}

		public IZForm ShowNew()
		{
			return ShowNew(null);
		}

		public IZForm ShowNew(Form parentForm)
		{
			return Show(parentForm, false);
		}

		public override IZForm ShowPopup()
		{
			return ShowNew();
		}

		public IZForm Show(Form parentForm, bool singleton)
		{
			var controller = GetNewController();
#if DEBUG
			lastController = controller;
#endif
			if (parentForm != null)
			{
				controller.SetFormsModalTo(parentForm);
			}
			return singleton ? controller.ShowSingletonForm() : controller.ShowNewForm();
		}

#if DEBUG
		ZController lastController;

		ZController IPopupModuleInternalsForTesting.LastController
		{
			get { return lastController; }
		}

		public void CloseFormForTestingOnly()
		{
			if (lastController != null && lastController.LastShownForm != null)
			{
				lastController.LastShownForm.Dispose();
			}
		}
#endif
	}

	public interface IPopupModuleInternalsForTesting
	{
#if DEBUG
		ZController LastController { get; }
#endif
	}
}
