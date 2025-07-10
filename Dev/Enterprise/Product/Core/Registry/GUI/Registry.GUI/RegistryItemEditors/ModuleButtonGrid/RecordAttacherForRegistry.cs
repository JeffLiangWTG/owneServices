using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Registry.GUI
{
	class RecordAttacherForRegistry<T> : ZRecordAttacher
		where T : RegistryProxyBusinessObject, new()
	{
		public RecordAttacherForRegistry(RegistryZUserControl registryControl, IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
			: base(destinationCollection, findBoxList, moduleID)
		{
			RegistryControl = Argument.NotNull(registryControl, "registryControl");
		}

		readonly RegistryZUserControl RegistryControl;

		#region Show

		protected override void ShowCore(IZForm formToShowModalTo)
		{
			base.ShowCore(formToShowModalTo);

			// Since the popup form is Modal, we need to hook onto the close event of the form to focus onto the registry control.
			// This is necessary as Committing of the Registry item occurs on Leaving the Registry Control.
			((IFindBox)this).PopupForm.Closed += PopupForm_Closed;
		}

		void PopupForm_Closed(object sender, EventArgs e)
		{
			RegistryControl.Focus();
			((IFindBox)this).PopupForm.Closed -= PopupForm_Closed;
		}

		#endregion

		#region Attach

		protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			bool result = false;
			var pk = bizO.PK;

			if (!IsBranchAlreadySelected(pk))
			{
				var proxy = new T();
				proxy.ProxyPK = pk;
				listToBulkAdd.Add(proxy);
				result = true;
			}

			return result;
		}

		bool IsBranchAlreadySelected(ZGuid pk)
		{
			foreach (T proxy in DestinationCollection)
			{
				if (proxy.ProxyPK == pk)
				{
					return true;
				}
			}

			return false;
		}

		#endregion
	}
}
