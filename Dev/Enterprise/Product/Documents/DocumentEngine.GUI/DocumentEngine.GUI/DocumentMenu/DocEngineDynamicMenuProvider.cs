using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	public abstract class DocEngineDynamicMenuProvider<T, U> : IDynamicMenuProvider<T>
		where T : IDynamicMenu
		where U : class, IZDocumentMenuItem, IDisposable, new()
	{
		protected const string PlaceHolder = "<PlaceHolder>";

		protected DocEngineDynamicMenuProvider()
		{
		}

		#region IDynamicMenuProvider

		public T GetMenuItem(Form parentForm, BusinessObject businessObject)
		{
			T result = default(T);
			var businessObjectAsIDocumentSupportable = businessObject as IDocumentSupportable;
			if (businessObjectAsIDocumentSupportable != null
				&& businessObjectAsIDocumentSupportable.DocumentSupporter != null
				&& businessObjectAsIDocumentSupportable.DocumentSupporter.ShowDocumentsInDynamicMenu)
			{
				IDocumentEventsForMenu businessObjectAsIDocumentEventsForMenu;

				if (CanShowDocumentMenu(parentForm, out businessObjectAsIDocumentEventsForMenu))
				{
					result = GetNewMenuItem(Res.GetString("b8a0b88a-d9cf-4536-a996-835b1621d8ad", "Documents"));
					AddPlaceHolder(result);
					result.Opening += delegate
					{
						var itemsToDispose = new IDisposable[result.MenuItems.Count];
						result.MenuItems.CopyTo(itemsToDispose, 0);
						result.MenuItems.Clear();

						foreach (var item in itemsToDispose)
						{
							item.Dispose();
						}

						using (var documentsMenu = new U())
						{
							var businessObjectAsIStmNoteParent = businessObjectAsIDocumentSupportable as IStmNoteParent;
							var note = businessObjectAsIStmNoteParent == null ? null : DocumentNote.RetrieveNote(businessObjectAsIStmNoteParent);
							if (note != null)
							{
								documentsMenu.Setup(businessObjectAsIDocumentSupportable, businessObjectAsIDocumentEventsForMenu, note.UserDefinedFieldList, note.GetSystemDefinedFieldList());
							}
							else
							{
								documentsMenu.Setup(businessObjectAsIDocumentSupportable, businessObjectAsIDocumentEventsForMenu, null, null);
							}

							documentsMenu.LoadMenus(parentForm);

							while (documentsMenu.Items.Count > 0)
							{
								result.MenuItems.Add(documentsMenu.Items[0]);
							}

#if DEBUG
							if (Globals.IsTest)
							{
								LastDocumentMenuForTesting = documentsMenu;
							}
#endif
						}

						//hack: if the program becomes unresponsive during opening the documents submenu,
						//a graphical glitch occurs where the menu is transposed a few px left and up,
						//then a window that may be a different window takes focus (deterministic & consistent).
						parentForm.BeginInvoke(new Action(() =>
						{
							if (!parentForm.ContainsFocus && parentForm.CanFocus)
							{
								parentForm.Focus();
							}
						}
						));
					};

					if (businessObjectAsIDocumentEventsForMenu != null)
					{
						businessObjectAsIDocumentSupportable.DocumentSupporter.Initialise(businessObjectAsIDocumentEventsForMenu);
					}
				}
				else
				{
					result = GetNewMenuItem(ZDocumentsMenuItemConstants.PleaseSaveYourRecord);
				}
			}

			return result;
		}

		bool CanShowDocumentMenu(Form parentForm, out IDocumentEventsForMenu businessObjectAsIDocumentEventsForMenu)
		{
			businessObjectAsIDocumentEventsForMenu = null;
			var canShowDocumentMenu = true;
			var parentZForm = parentForm as ZForm;
			if (parentZForm != null)
			{
				var topLevelBusinessEntity = parentZForm.BusinessEntity;
				if (topLevelBusinessEntity != null) // We're on a bound editable form.
				{
					canShowDocumentMenu = !topLevelBusinessEntity.HasChanges;
					businessObjectAsIDocumentEventsForMenu = topLevelBusinessEntity as IDocumentEventsForMenu;
				}
			}

			return canShowDocumentMenu;
		}

		#endregion

		protected abstract void AddPlaceHolder(T result);
		protected abstract T GetNewMenuItem(string name);

		#region LastDocumentMenuForTesting
#if DEBUG
		WeakReference<U> LastDocumentMenuForTestingReference;

		internal U LastDocumentMenuForTesting
		{
			get => LastDocumentMenuForTestingReference.TryGetTarget(out var value) ? value : null;
			set => LastDocumentMenuForTestingReference = new WeakReference<U>(value);
		}
#endif
		#endregion
	}
}
