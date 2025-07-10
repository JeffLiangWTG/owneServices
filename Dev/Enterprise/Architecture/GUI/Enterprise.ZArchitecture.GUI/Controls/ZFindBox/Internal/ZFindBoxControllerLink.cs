using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	internal class PopupControllerLink : ZFindBoxControllerLink
	{
		public PopupControllerLink(IFindBox findBox)
			: base(findBox)
		{
		}

		protected override void PerformCloseAction(ZGuid savedPK, IFindBoxPopup popupForm)
		{
			if (popupForm != null)
			{
				popupForm.SelectRowByPK(savedPK);
			}
		}
	}

	internal class DirectToFormControllerLink : ZFindBoxControllerLink
	{
		public DirectToFormControllerLink(IFindBox findBox)
			: base(findBox)
		{
		}

		protected override void PerformCloseAction(ZGuid savedPK, IFindBoxPopup popupForm)
		{
			if (FindBox is Control findBoxControl)
			{
				findBoxControl.BeginInvokeSafe(() => UpdateFindBox(savedPK));
			}
			else
			{
				UpdateFindBox(savedPK);
				ErrorReporter.ReportOnce(nameof(PerformCloseAction), "FindBox should always be Control.");
			}
		}

		void UpdateFindBox(ZGuid savedPK)
		{
			FindBox.Code = FindBox.ListProvider.CodeFromPrimaryKey(savedPK);
			FindBox.Description = FindBox.ListProvider.DescriptionFromCode(FindBox.Code);
			if (FindBox is ZCodeFindBox codeFindBox)
			{
				if (FindBox is ZGuidFindBox guidFindBox)
				{
					guidFindBox.ClearCodePairCache();
				}

				var binding = codeFindBox.CodeBox.DataBindings["Text"];
				if (binding != null)
				{
					binding.WriteValue();
					binding.ReadValue();
				}
			}
		}
	}

	public abstract class ZFindBoxControllerLink
	{
		protected ZFindBoxControllerLink(IFindBox findBox)
		{
			FindBox = findBox;
		}

		public void HookController(ZController controller)
		{
			var form = controller.LastShownForm;
			if (form != null)
			{
				form.Closed -= LastShownForm_Closed;
				form.Closed += LastShownForm_Closed;

				var zForm = form as ZForm;
				var factory = zForm != null && zForm.BusinessEntity != null && zForm.BusinessEntity.Factory != null ? zForm.BusinessEntity.Factory : controller.Factory;
				if (factory != null)
				{
					factory.Saved -= Factory_Saved;
					factory.Saved += Factory_Saved;
				}

				if (formAndControllers == null)
				{
					formAndControllers = new Dictionary<IZForm, Tuple<ZController, BusinessObjectFactory>>(1);
				}
				if (!formAndControllers.ContainsKey(form))
				{
					formAndControllers.Add(form, new Tuple<ZController, BusinessObjectFactory>(controller, factory));
				}
			}
		}

		protected abstract void PerformCloseAction(ZGuid savedPK, IFindBoxPopup popupForm);

		#region Implementation

		protected IFindBox FindBox { get; private set; }

		Dictionary<IZForm, Tuple<ZController, BusinessObjectFactory>> formAndControllers;
		List<BusinessObjectFactory> savedFactories;

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				if (savedFactories == null)
				{
					savedFactories = new List<BusinessObjectFactory>(1);
				}
				if (!savedFactories.Contains(factory))
				{
					savedFactories.Add(factory);
				}
			}
		}

		void LastShownForm_Closed(object sender, EventArgs e)
		{
			var form = sender as IZForm;
			if (form != null)
			{
				form.Closed -= LastShownForm_Closed;

				Tuple<ZController, BusinessObjectFactory> controllerAndFactory;
				if (formAndControllers.TryGetValue(form, out controllerAndFactory))
				{
					formAndControllers.Remove(form);

					var controller = controllerAndFactory.Item1;
					var factory = controllerAndFactory.Item2;
					if (factory != null)
					{
						factory.Saved -= Factory_Saved;
						if (savedFactories != null && savedFactories.Contains(factory))
						{
							savedFactories.Remove(factory);
							if (controller != null)
							{
								PerformCloseAction(controller.LastSavedPK, form as IFindBoxPopup);
							}
						}
					}

					if (controller != null && controller.LastShownForm != null && controller.LastShownForm != form)
					{
						ErrorReporter.ReportOnce("ZFindBoxControllerLink.LastShownForm_Closed.Mismatched_Form",
							"ZController.LastShownForm seems to have been changed since ZFindBoxControllerLink was created." +
							"\r\nLink: " + GetType().Name +
							"\r\nController: " + controller.ID.Name + " (" + controller.GetType().Name + ")" +
							"\r\nController last shown form: " + controller.LastShownForm.ToString() +
							"\r\nSender form: " + form.ToString());
					}
				}
				else
				{
					ErrorReporter.ReportOnce("ZFindBoxControllerLink.LastShownForm_Closed.Unregistered_Form",
						"LastShownForm_Closed() seems to have been called twice for same form." +
						"\r\nLink: " + GetType().Name +
						"\r\nSender form: " + form.ToString());
				}
			}
		}

		#endregion
	}
}
