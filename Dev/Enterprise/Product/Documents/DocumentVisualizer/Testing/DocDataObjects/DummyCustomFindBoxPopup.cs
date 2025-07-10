using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyCustomFindBoxPopup : IFindBoxPopup
	{
		public event EventHandler Closed
		{
			add
			{
				closed += value;
			}
			remove
			{
				closed -= value;
			}
		}

		event EventHandler closed;

		public void Dispose()
		{
		}

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup)
		{
			return SilentSelectResult.None;
		}

		public void SelectRowByPK(ZGuid pK)
		{
		}

		public void ShowModal(IFindBox findBox, Form parentForm)
		{
			ShownModalFormInvoker?.Invoke(findBox, parentForm);
			closed?.Invoke(this, EventArgs.Empty);
		}

		public Action<IFindBox, Form> ShownModalFormInvoker { get; set; }
	}
}
