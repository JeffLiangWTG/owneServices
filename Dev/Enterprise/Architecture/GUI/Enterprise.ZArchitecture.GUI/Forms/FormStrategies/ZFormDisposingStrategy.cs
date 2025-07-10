using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	internal class ZFormDisposingStrategy : IDisposable
	{
		internal ZFormDisposingStrategy()
		{
			KForm.FormDisposed += FormDisposed;
		}

		static void FormDisposed(object sender, EventArgs e)
		{
			var form = sender as KForm;
			if (form != null)
			{
				DotNet2HackForMemoryLeak(form);

				if (!(form is ZMessageBox))
				{
					RegistryItemDictionary.Instance.PurgeExpired();
				}
			}
		}

		static void DotNet2HackForMemoryLeak(KForm disposedForm)
		{
			var openForms = ZApplication.GetOpenForms();
			foreach (var form in openForms)
			{
				if (form == disposedForm)
				{
#if NET
#pragma warning disable CW1015 // No Application.OpenForms Rule
					//see: https://github.com/dotnet/winforms/blob/e4efded416f8216935683d9a12e2eb45df78a728/src/System.Windows.Forms/src/System/Windows/Forms/Form.cs#L3894
					Application.OpenForms.GetType().GetMethod("Remove", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(Application.OpenForms, new object[] { disposedForm });
#pragma warning restore CW1015 // No Application.OpenForms Rule
#else
					//see: https://referencesource.microsoft.com/#System.Windows.Forms/winforms/Managed/System/WinForms/Form.cs,4677
					var openFormsInternalRemoveInfo = typeof(Application).GetMethod("OpenFormsInternalRemove", BindingFlags.NonPublic | BindingFlags.Static);
					openFormsInternalRemoveInfo.Invoke(null, new object[] { disposedForm });
#endif
					break;
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				KForm.FormDisposed -= FormDisposed;
			}
		}
	}
}
