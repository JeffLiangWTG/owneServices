using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Common.GUI
{
	[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "IDisposable is implemented on IZForm interface, not here")]
	public class WaitingForResponseFromBorderWiseWebMessageBox : WaitingForResponseFromBorderWiseMessageBox, IZForm
	{
		internal WaitingForResponseFromBorderWiseWebMessageBox(string okButtonText, string caption, string message) : base(okButtonText, caption, message)
		{
			OpenedFormCache.GetInstance().Add(FormBizo.PK.ToGuid(), this, ControllerIDs.BorderWiseWebReturnHook.ToString());
		}

		public BorderWiseReturnHookNonPersistentBizo FormBizo { get; } = new BorderWiseReturnHookNonPersistentBizo();

		#region IZForm Members

		ControllerID IZForm.ControllerID { get; set; } = ControllerIDs.BorderWiseWebReturnHook;

		ODisplayMode IZForm.DisplayMode { get; set; } = ODisplayMode.Browse;

		ModuleResultsBusinessObject IZForm.ModuleResultsBusinessObject { set { } }

		IBusiness IZForm.BusinessEntityForPersistingForm
		{
			get => FormBizo;
			set { }
		}

		Guid IZForm.IdentifierForPersistingForm
		{
			get => FormBizo.PK.ToGuid();
			set { }
		}

		bool IZForm.IsActivityLogFinished { get; set; }

		IDisposable ILicensedComponent.LicensedComponentManager => null;

		public event EventHandler OnClosedEvent;
		protected override void OnClosing(CancelEventArgs e)
		{
			if (OnClosedEvent != null)
			{
				OnClosedEvent(null, e);
			}
			base.OnClosing(e);
		}

		void IZForm.FormInitialSize()
		{
		}

		#endregion
	}
}
