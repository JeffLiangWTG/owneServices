using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	#region Overriding Interfaces

	#region Button Text Override

	public interface IButtonPostTextOverride
	{
		string PostButtonText { get; }
	}

	public interface IButtonDeleteTextOverride
	{
		string DeleteButtonText { get; }
	}

	public interface IButtonNewTextOverride
	{
		string NewButtonText { get; }
	}

	public interface IButtonApplyTextOverride
	{
		string ApplyButtonText { get; }
	}

	public interface IButtonCancelTextOverride
	{
		string CancelButtonText { get; }
	}

	public interface IButtonCloseTextOverride
	{
		string CloseButtonText { get; }
	}

	#endregion

	#region PreviousNextControl

	public interface IPreviousNextControlProvider
	{
		bool AutoAddPreviousNextButtons { get; }
#if DEBUG
		ZPreviousNextControl PreviousNextControlForTesting { get; set; }
#endif
	}

	public interface IPreviousNextControlOverrideProvider
	{
		bool OverridesSetPreviousNextControlParentAndPosition { get; }
		bool ShouldDoBaseSetPreviousNextControlParentAndPosition { get; }
		bool OverridesGetPreviousNextControl { get; }

		void SetPreviousNextControlParentAndPosition(ZPreviousNextControl control);

		ZPreviousNextControl GetPreviousNextControl(ModuleResultsBusinessObject bizObj, ZController controller);
	}

	#endregion

	public interface IPostingButtonsProvider
	{
		IButton CommandButtonApply { get; }
		IButton CommandButtonPost { get; }
		IButton CommandButtonCancel { get; }
		bool IsPostOnly { get; set; }
		bool AllowNew { get; }
		bool SetupPostingCalled { get; set; }
		void AssignButtonsInternal(IButton saveAndCloseButtonControl, IButton cancelButtonControl, IButton saveButtonControl);
	}

	#endregion

	public class FormPostingButtonVisualInfo
	{
		public string Text { get; set; }
		public Image Image { get; set; }
		public bool IsNew { get; set; }
	}

	public static class ZFormPostingButtonsStrategy
	{
		#region Buttons Text

		public static FormPostingButtonVisualInfo PostButtonText(IPostingButtonsProvider form)
		{
			var buttonTextOverride = form as IButtonPostTextOverride;

			var text = (buttonTextOverride != null && !string.IsNullOrEmpty(buttonTextOverride.PostButtonText))
							? buttonTextOverride.PostButtonText
							: form.IsPostOnly ? Res.GetString("Posting.Buttons.Save", "&Save") : Res.GetString("Posting.Buttons.SaveClose", "S&ave && Close");

			var image = form.IsPostOnly ? Icons.GetImage(IconTypes.BlackWhite_Save) : Icons.GetImage(IconTypes.BlackWhite_SaveClose);
			return new FormPostingButtonVisualInfo { Text = text, Image = image };
		}

		public static FormPostingButtonVisualInfo DeleteButtonText(IPostingButtonsProvider form)
		{
			var buttonTextOverride = form as IButtonDeleteTextOverride;
			var text = (buttonTextOverride != null && !string.IsNullOrEmpty(buttonTextOverride.DeleteButtonText))
					? buttonTextOverride.DeleteButtonText
					: Res.GetString("Posting.Buttons.Delete", "&Delete");

			var image = Icons.GetImage(IconTypes.BlackWhite_Cross);

			var zform = form as ZForm;
			if (zform != null && zform.BusinessEntityForValidation != null)
			{
				var cancellable = zform.GetICancellable(zform.BusinessEntityForValidation);
				if (cancellable != null && PreventDeleteAttribute.IsTrue(cancellable.GetType()))
				{
					text = cancellable.IsCancelled ? Res.GetString("Posting.Buttons.Deactivate", "Deactivate") : Res.GetString("Posting.Buttons.Activate", "Activate");
					image = cancellable.IsCancelled ? Icons.GetImage(IconTypes.BlackWhite_Cross) : Icons.GetImage(IconTypes.BlackWhite_Tick);
				}
			}

			return new FormPostingButtonVisualInfo { Text = text, Image = image };
		}

		public static FormPostingButtonVisualInfo NewButtonText(IPostingButtonsProvider form)
		{
			var buttonTextOverride = form as IButtonNewTextOverride;
			var text = (buttonTextOverride != null && !string.IsNullOrEmpty(buttonTextOverride.NewButtonText))
					? buttonTextOverride.NewButtonText
					: Res.GetString("Posting.Buttons.New", "&New");

			return new FormPostingButtonVisualInfo { Text = text, Image = Icons.GetImage(IconTypes.NewButtonRest), IsNew = true };
		}

		public static FormPostingButtonVisualInfo ApplyButtonText(IPostingButtonsProvider form)
		{
			var buttonTextOverride = form as IButtonApplyTextOverride;
			var text = (buttonTextOverride != null && !string.IsNullOrEmpty(buttonTextOverride.ApplyButtonText))
					? buttonTextOverride.ApplyButtonText
					: Res.GetString("Posting.Buttons.Save", "&Save");

			return new FormPostingButtonVisualInfo { Text = text, Image = Icons.GetImage(IconTypes.BlackWhite_Save) };
		}

		public static FormPostingButtonVisualInfo CancelButtonText(IPostingButtonsProvider form)
		{
			var buttonTextOverride = form as IButtonCancelTextOverride;
			var text = (buttonTextOverride != null && !string.IsNullOrEmpty(buttonTextOverride.CancelButtonText))
					? buttonTextOverride.CancelButtonText
					: Res.GetString("Posting.Buttons.Cancel", "&Cancel");

			return new FormPostingButtonVisualInfo { Text = text, Image = Icons.GetImage(IconTypes.BlackWhite_Cancel) };
		}

		public static FormPostingButtonVisualInfo CloseButtonText(IPostingButtonsProvider form)
		{
			var buttonTextOverride = form as IButtonCloseTextOverride;
			var text = (buttonTextOverride != null && !string.IsNullOrEmpty(buttonTextOverride.CloseButtonText))
					? buttonTextOverride.CloseButtonText
					: DefaultCloseButtonText;

			return new FormPostingButtonVisualInfo { Text = text, Image = Icons.GetImage(IconTypes.BlackWhite_Close) };
		}		

		public static string DefaultCloseButtonText
		{
			get { return Res.GetString("Posting.Buttons.Close", "&Close"); }
		}

		#endregion

		public static void AddAdornments(KForm form)
		{
			if (!form.IsDesignMode())
			{
				if (form.DataSource != null)
				{
					SetPreviousNextControl(form);
				}
				else
				{
					form.DataSourceChanged += new EventHandler(SetPreviousNextControl);
				}
			}
		}

		static void SetPreviousNextControl(object sender, EventArgs e)
		{
			var form = (KForm)sender;
			if (form.DataSource != null)
			{
				form.DataSourceChanged -= new EventHandler(SetPreviousNextControl);
				SetPreviousNextControl(form);
			}
		}

		#region Set PreviousNext Control

		static void SetPreviousNextControl(Form form)
		{
			var zform = form as IZForm;
			var bform = form as IBusinessForm;
			var previousNextControlProvider = form as IPreviousNextControlProvider;
			if (previousNextControlProvider != null && zform != null && bform != null)
			{
				if (previousNextControlProvider.AutoAddPreviousNextButtons && bform.ModuleResultsBusinessObject != null)
				{
					var previousNextControl = GetPreviousNextControl(
						form, bform.ModuleResultsBusinessObject, ZControllerFactory.Create(zform.ControllerID));
					SetPreviousNextControlParentAndPosition(form, previousNextControl);
					previousNextControl.Bind();
					previousNextControl.BringToFront();
				}
			}
		}

		static void SetPreviousNextControlParentAndPosition(Form form, ZPreviousNextControl control)
		{
			var provider = form as IPreviousNextControlOverrideProvider;

			if (provider == null ||
				!provider.OverridesSetPreviousNextControlParentAndPosition || provider.ShouldDoBaseSetPreviousNextControlParentAndPosition)
			{
				var statusBarProvider = form as IStatusBarProvider;
				var bottom = (statusBarProvider != null) ? statusBarProvider.MainStatusBar.Top : form.Height;

				ControlDpiScalingHelper.SetTop(ref control, bottom - control.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				ControlDpiScalingHelper.SetLeft(ref control, 1, true);
				control.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
				form.Controls.Add(control);
			}

			if (provider != null && provider.OverridesSetPreviousNextControlParentAndPosition)
			{
				provider.SetPreviousNextControlParentAndPosition(control);
			}
		}

		static ZPreviousNextControl GetPreviousNextControl(Form form, ModuleResultsBusinessObject bizObj, ZController controller)
		{
			var provider = form as IPreviousNextControlOverrideProvider;

			var control =
				(provider != null && provider.OverridesGetPreviousNextControl)
					? provider.GetPreviousNextControl(bizObj, controller)
					: new ZPreviousNextControl(bizObj, controller);

#if DEBUG
			var previousNextControlProvider = form as IPreviousNextControlProvider;
			if (previousNextControlProvider != null)
			{
				previousNextControlProvider.PreviousNextControlForTesting = control;
			}
#endif

			return control;
		}

		#endregion

		#region Setup Posting Buttons

		public static void SetupPosting(IPostingButtonsProvider form, ZPostingButtonsUserControl buttonsControl)
		{
			SetupPosting(form, buttonsControl.SaveAndCloseButton, buttonsControl.CloseButton, buttonsControl.SaveButton);
		}

		public static void SetupPosting(IPostingButtonsProvider form, ZPostingButtonsUserControl buttonsControl, bool isSaveButtonHidden)
		{
			SetupPosting(form, buttonsControl.SaveAndCloseButton, buttonsControl.CloseButton, isSaveButtonHidden ? null : buttonsControl.SaveButton);
		}

		internal static void SetupPosting(IPostingButtonsProvider form, ZToolStripButton saveAndCloseButtonControl, ZToolStripButton cancelButtonControl)
		{
			SetupPosting(form, saveAndCloseButtonControl, cancelButtonControl, null);
		}

		public static void SetupPosting(IPostingButtonsProvider form, ZButton saveAndCloseButtonControl, ZButton cancelButtonControl)
		{
			SetupPosting(form, saveAndCloseButtonControl, cancelButtonControl, null);
		}

		public static void SetupPosting(IPostingButtonsProvider form, ZButton saveAndCloseButtonControl, ZButton cancelButtonControl, ZButton saveButtonControl)
		{
			form.AssignButtonsInternal(saveAndCloseButtonControl, cancelButtonControl, saveButtonControl);
			SetupPostingCore(form);
		}

#if DEBUG
		public
#else
		internal
#endif
		static void SetupPosting(IPostingButtonsProvider form, ZToolStripButton saveAndCloseButtonControl, ZToolStripButton cancelButtonControl, ZToolStripButton saveButtonControl)
		{
			form.AssignButtonsInternal(saveAndCloseButtonControl, cancelButtonControl, saveButtonControl);
			SetupPostingCore(form);
		}

		static void SetupPostingCore(IPostingButtonsProvider form)
		{
			var zform = form as ZForm;
			if (zform != null)
			{
				EventHandler<HasChangesChangedEventArgs> hasChangesChangedHandler = delegate
				{
					if ((zform.DisplayMode == ODisplayMode.Edit && BusinessObjectFactory.GlobalSaveCount == zform.LastSaveCount))
					{
						return;
					}

					// Whilst in a transaction that buttons do not need updating
					// The buttons will be refreshed upon the saved event
					if (zform.BusinessEntityForHasChanges != null
						&& zform.BusinessEntityForHasChanges.Factory != null
						&& zform.BusinessEntityForHasChanges.Factory.IsInTransaction)
					{
						return;
					}

					UpdateSaveButtonsBasedOnHasChanges(zform);
				};

				RunWhenDataSourceSet(zform, delegate
				{
					var businessEntityForHasChanges = zform.BusinessEntityForHasChanges;
					if (businessEntityForHasChanges != null)
					{
						businessEntityForHasChanges.HasChangesChanged += hasChangesChangedHandler;
						var factory = businessEntityForHasChanges.Factory;
						BusinessObjectFactory.SavedEventHandler savedHandler = delegate { UpdateSaveButtonsBasedOnHasChanges(zform); };
						if (factory != null)
						{
							factory.Saved += savedHandler;
						}

						zform.Disposed += delegate
						{
							if (businessEntityForHasChanges != null)
							{
								businessEntityForHasChanges.HasChangesChanged -= hasChangesChangedHandler;
								if (factory != null)
								{
									factory.Saved -= savedHandler;
								}
							}
						};

						zform.FactoryChanged += delegate
						{
							if (factory != null)
							{
								factory.Saved -= savedHandler;
							}

							factory = zform.BusinessEntityForHasChanges != null ? zform.BusinessEntityForHasChanges.Factory : null;

							if (factory != null)
							{
								factory.Saved += savedHandler;
							}
						};
					}
				});

				UpdateSaveButtonsBasedOnHasChanges(zform);
			}

			form.SetupPostingCalled = true;
		}

		static void RunWhenDataSourceSet(ZForm form, MethodInvoker invoker)
		{
			if (form.DataSource != null)
			{
				invoker();
			}
			else
			{
				var handler = new EventHandlerReference();
				handler.Value = delegate
				{
					invoker();
					form.DataSourceChanged -= handler.Value;
				};
				form.DataSourceChanged += handler.Value;
			}
		}

		class EventHandlerReference
		{
			public EventHandler Value;
		}

		public static IDisposable DeferredUpdateSaveButtonsBasedOnHasChanges(ZForm form, bool excludeChildrenHasChanges = false)
		{
			return new UpdateSaveButtonsBasedOnHasChangesDeferrer(form, false, excludeChildrenHasChanges);
		}

		class UpdateSaveButtonsBasedOnHasChangesDeferrer : IDisposable
		{
			public UpdateSaveButtonsBasedOnHasChangesDeferrer(ZForm form, bool shouldUpdate, bool excludeChildrenHasChanges = false)
			{
				this.form = form;
				this.excludeChildrenHasChanges = excludeChildrenHasChanges;
				UpdateTracker value;
				if (FormsInProcess.TryGetValue(form, out value))
				{
					value.count++;
					value.shouldUpdate |= shouldUpdate;
				}
				else
				{
					FormsInProcess.Add(form, new UpdateTracker() { shouldUpdate = shouldUpdate });
				}
			}

			readonly ZForm form;
			readonly bool excludeChildrenHasChanges;

			public void Dispose()
			{
				UpdateTracker value;
				if (FormsInProcess.TryGetValue(form, out value))
				{
					if (value.count == 0)
					{
						try
						{
							if (value.shouldUpdate)
							{
								UpdateSaveButtonsBasedOnHasChanges(excludeChildrenHasChanges);
							}
						}
						finally
						{
							FormsInProcess.Remove(form);
						}
					}
					else
					{
						value.count--;
					}
				}
			}

			void UpdateSaveButtonsBasedOnHasChanges(bool excludeChildrenHasChanges = false)
			{
				if (form.DisplayMode != ODisplayMode.Delete && form.DisplayMode != ODisplayMode.ReadOnly && form.BusinessEntityForHasChanges != null)
				{
					var hasChanges = excludeChildrenHasChanges ? form.BusinessEntityForHasChanges.HasChangesNotIncludingChildren : form.BusinessEntityForHasChanges.HasChanges;
					if (hasChanges)
					{
						if (form.OriginalDisplayMode == ODisplayMode.Undefined)
						{
							form.OriginalDisplayMode = form.DisplayMode;
						}

						if (form.DisplayMode != ODisplayMode.Edit)
						{
							form.DisplayMode = ODisplayMode.Edit;
							form.LastSaveCount = BusinessObjectFactory.GlobalSaveCount;
						}
					}
					else
					{
						if (form.OriginalDisplayMode != ODisplayMode.Undefined && form.DisplayMode != form.OriginalDisplayMode)
						{
							form.DisplayMode = form.OriginalDisplayMode;
						}
					}
				}
			}
		}

		public static void UpdateSaveButtonsBasedOnHasChanges(ZForm form)
		{
			UpdateTracker value;
			if (FormsInProcess.TryGetValue(form, out value))
			{
				value.shouldUpdate = true;
			}
			else
			{
				using (new UpdateSaveButtonsBasedOnHasChangesDeferrer(form, true))
				{
				}
			}
		}

		static Dictionary<ZForm, UpdateTracker> FormsInProcess
		{
			get { return formsInProcess ?? (formsInProcess = new Dictionary<ZForm, UpdateTracker>()); }
		}
		[ThreadStatic]
		static Dictionary<ZForm, UpdateTracker> formsInProcess;

		class UpdateTracker
		{
			public int count;
			public bool shouldUpdate;
		}

		#endregion

		#region Do Display Mode

		internal static void DoDisplayModeNew(Form form)
		{
			var postingButtonsProvider = form as IPostingButtonsProvider;
			if (postingButtonsProvider != null)
			{
				if (postingButtonsProvider.CommandButtonPost != null)
				{
					postingButtonsProvider.CommandButtonPost.Visible = true;
					postingButtonsProvider.CommandButtonPost.Enabled = true;
					SetButtonDetails(postingButtonsProvider.CommandButtonPost, PostButtonText(postingButtonsProvider));
				}

				if (postingButtonsProvider.CommandButtonCancel != null)
				{
					SetButtonDetails(postingButtonsProvider.CommandButtonCancel, CancelButtonText(postingButtonsProvider));
				}

				if (postingButtonsProvider.CommandButtonApply != null)
				{
					postingButtonsProvider.CommandButtonApply.Visible = !postingButtonsProvider.IsPostOnly;
					postingButtonsProvider.CommandButtonApply.Enabled = !postingButtonsProvider.IsPostOnly;
					SetButtonDetails(postingButtonsProvider.CommandButtonApply, ApplyButtonText(postingButtonsProvider));
				}
			}
		}

		internal static void DoDisplayModeNewSaved(Form form)
		{
			DoDisplayModeNew(form);

			var postingButtonsProvider = form as IPostingButtonsProvider;
			if (postingButtonsProvider != null)
			{
				if (postingButtonsProvider.CommandButtonPost != null)
				{
					postingButtonsProvider.CommandButtonPost.Enabled = false;
				}

				if (postingButtonsProvider.CommandButtonCancel != null)
				{
					SetButtonDetails(postingButtonsProvider.CommandButtonCancel, CloseButtonText(postingButtonsProvider));
				}

				if (postingButtonsProvider.CommandButtonApply != null)
				{
					postingButtonsProvider.CommandButtonApply.Enabled = false;
				}
			}
		}

		internal static void DoDisplayModeBrowse(Form form)
		{
			DoDisplayModeNew(form);

			var postingButtonsProvider = form as IPostingButtonsProvider;
			if (postingButtonsProvider != null)
			{
				if (postingButtonsProvider.CommandButtonPost != null)
				{
					postingButtonsProvider.CommandButtonPost.Enabled = false;
				}

				if (postingButtonsProvider.CommandButtonApply != null)
				{
					if (postingButtonsProvider.AllowNew)
					{
						postingButtonsProvider.CommandButtonApply.Enabled = true;
						SetButtonDetails(postingButtonsProvider.CommandButtonApply, NewButtonText(postingButtonsProvider));
					}
					else
					{
						postingButtonsProvider.CommandButtonApply.Enabled = false;
					}
				}

				if (postingButtonsProvider.CommandButtonCancel != null)
				{
					SetButtonDetails(postingButtonsProvider.CommandButtonCancel, CloseButtonText(postingButtonsProvider));
				}
			}
		}

		internal static void DoDisplayModeEdit(Form form)
		{
			DoDisplayModeNew(form);

			var postingButtonsProvider = form as IPostingButtonsProvider;
			if (postingButtonsProvider != null)
			{
				if (postingButtonsProvider.CommandButtonPost != null)
				{
					postingButtonsProvider.CommandButtonPost.Enabled = true;

					// if we switched to Edit mode from Delete mode - we need to fix colour of CommandButtonPost
					// coz "Delete" was red and "Save & Close" should be the same colour as "Cancel"
					if (postingButtonsProvider.CommandButtonCancel != null &&
						postingButtonsProvider.CommandButtonPost.BackColor != postingButtonsProvider.CommandButtonCancel.BackColor)
					{
						postingButtonsProvider.CommandButtonPost.ForeColor = postingButtonsProvider.CommandButtonCancel.ForeColor;
						postingButtonsProvider.CommandButtonPost.BackColor = postingButtonsProvider.CommandButtonCancel.BackColor;
					}
				}

				if (postingButtonsProvider.CommandButtonCancel != null)
				{
					SetButtonDetails(postingButtonsProvider.CommandButtonCancel, CancelButtonText(postingButtonsProvider));
				}

				if (postingButtonsProvider.CommandButtonApply != null)
				{
					postingButtonsProvider.CommandButtonApply.Enabled = !postingButtonsProvider.IsPostOnly;
				}
			}
		}

		internal static void DoDisplayModeDelete(Form form)
		{
			var postingButtonsProvider = form as IPostingButtonsProvider;
			if (postingButtonsProvider != null)
			{
				if (postingButtonsProvider.CommandButtonPost != null)
				{
					postingButtonsProvider.CommandButtonPost.Visible = true;
					postingButtonsProvider.CommandButtonPost.Enabled = true;
					SetButtonDetails(postingButtonsProvider.CommandButtonPost, DeleteButtonText(postingButtonsProvider));
					postingButtonsProvider.CommandButtonPost.ForeColor = Color.White;
					postingButtonsProvider.CommandButtonPost.BackColor = Color.Crimson;
				}

				if (postingButtonsProvider.CommandButtonCancel != null)
				{
					postingButtonsProvider.CommandButtonCancel.Enabled = true;
					SetButtonDetails(postingButtonsProvider.CommandButtonCancel, CancelButtonText(postingButtonsProvider));
				}

				if (postingButtonsProvider.CommandButtonApply != null)
				{
					postingButtonsProvider.CommandButtonApply.Visible = false;
				}
			}
		}

		internal static void DoDisplayModeReadOnly(Form form)
		{
			var postingButtonsProvider = form as IPostingButtonsProvider;
			if (postingButtonsProvider != null)
			{
				if (postingButtonsProvider.CommandButtonPost != null)
				{
					if (postingButtonsProvider.IsPostOnly)
					{
						postingButtonsProvider.CommandButtonPost.Visible = true;
						postingButtonsProvider.CommandButtonPost.Enabled = false;
					}
					else
					{
						postingButtonsProvider.CommandButtonPost.Visible = false;
					}
				}

				if (postingButtonsProvider.CommandButtonCancel != null)
				{
					postingButtonsProvider.CommandButtonCancel.Enabled = true;
					SetButtonDetails(postingButtonsProvider.CommandButtonCancel, CloseButtonText(postingButtonsProvider));
				}

				if (postingButtonsProvider.CommandButtonApply != null)
				{
					postingButtonsProvider.CommandButtonApply.Visible = false;
				}
			}
		}

		static void SetButtonDetails(IButton button, FormPostingButtonVisualInfo info)
		{
			button.Text = info.Text;
			if (button.ShouldSetImage)
			{
				button.Image = info.Image;
			}
			if (button is ZToolStripButton ztsb)
			{
				ztsb.IgnoreMnemonic = info.IsNew;
			}
		}

		#endregion
	}
}
