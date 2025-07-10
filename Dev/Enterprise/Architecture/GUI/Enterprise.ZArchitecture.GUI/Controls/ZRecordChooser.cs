using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZRecordChooser<T> : IFindBox
		where T : BusinessObject
	{
		public ZRecordChooser(ModuleIdentifier moduleId)
			: this(moduleId, null)
		{
		}

		public ZRecordChooser(ModuleIdentifier moduleId, IBusinessObjectCollection collection)
		{
			this.moduleId = moduleId;
			this.collection = collection;
		}

		public delegate void HandleSelectedBusinessObjects(T[] selectedBusinessObjects);

		public void ShowModal(Form parentForm, HandleSelectedBusinessObjects handler)
			=> ShowModalCore(parentForm, handler);

		protected virtual void ShowModalCore(Form parentForm, HandleSelectedBusinessObjects handler)
		{
			var module = ZModuleFactory.Instance.Create(moduleId) as ZFilterModule ?? throw new InvalidOperationException(string.Format("{0} is not a ZFilterModule", moduleId.Name));

			module.OverrideModuleDecisionProvider(GetDefaultModuleDecisionProvider());
			popupForm = new EmbeddedModulePopup(module);
			ModulePopupSelectionHandler.Hook(this, handler);
			popupForm.ShowModal(this, parentForm);
		}

		protected virtual ZRecordChooserModuleDecisionProvider GetDefaultModuleDecisionProvider()
		{
			return new ZRecordChooserModuleDecisionProvider(this);
		}

		class ModulePopupSelectionHandler
		{
			ModulePopupSelectionHandler(ZRecordChooser<T> recordChooser, HandleSelectedBusinessObjects selectedBizOHandler)
			{
				this.evHandler = delegate(object sender, EmbeddedModulePopup.SelectedEventArgs e)
				{
					var result = new T[e.SelectedBusinessObjects.Length];
					Array.Copy(e.SelectedBusinessObjects, result, result.Length);
					selectedBizOHandler(result);

					recordChooser.popupForm.Selected -= evHandler;
					recordChooser.popupForm = null;
				};
				recordChooser.popupForm.Selected += evHandler;
			}

			public static ModulePopupSelectionHandler Hook(ZRecordChooser<T> recordChooser, HandleSelectedBusinessObjects selectedBizOHandler)
			{
				return new ModulePopupSelectionHandler(recordChooser, selectedBizOHandler);
			}

			readonly EmbeddedModulePopup.SelectedEventHandler evHandler;
		}

		readonly ModuleIdentifier moduleId;
		readonly IBusinessObjectCollection collection;

		#region IFindBox Members

		string IFindBox.Code { get; set; }
		string IFindBox.Description { get; set; }

		IFindBoxListProvider IFindBox.ListProvider
		{
			get { return collection as IFindBoxListProvider; }
		}

		IFindBoxPopup IFindBox.PopupForm
		{
			get { return popupForm; }
		}
		EmbeddedModulePopup popupForm;

		#endregion

		#region ZRecordChooserModuleDecisionProvider

		public class ZRecordChooserModuleDecisionProvider : ButtonGridModuleDecisionProvider
		{
			public ZRecordChooserModuleDecisionProvider(ZRecordChooser<T> recordChooser)
				: base(recordChooser)
			{
			}

			public override IBusinessObjectCollection List
			{
				get { return ((ZRecordChooser<T>)FindBox).collection; }
			}
		}

		#endregion
	}
}
