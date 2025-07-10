using System;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.IO;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZProcessStatusFormManager<FormType> : ProcessStatusFormManager<FormType> where FormType : Form, IProcessStatus, new() // Not decalaring a form but rather a restriction on the generic type
	{
		public ZProcessStatusFormManager()
			: base((object sender, ThreadExceptionEventArgs e) => ExceptionReporter.Instance.HandleOrReport(e.Exception))
		{
			threadLanguage = Res.CurrentLanguage;

#if DEBUG
			// This class will pop open a form on a timer, but can get short circuited by fast exeuction.
			// This can subsequently result in intermittently failing tests which are easy to miss and aren't caught by CI.
			if (NUnit.Framework.TestingState.IsRunningTests && typeof(CargoWise.Windows.UI.KForm).IsAssignableFrom(typeof(FormType)))
			{
				NUnit.Framework.TestingState.FailIfRunningTestNotMarkedAsGuiTest();
			}
#endif
		}

		readonly string threadLanguage;

		public bool DisposeOnFormDispose { get; set; }

		protected override sealed FormType CreateForm()
		{
			if (Db.DatabaseNameIsInitialized && Db.ServerNameIsInitialized)
			{
				dbDisposable = Db.DisposableActionForDbConnection();
			}
			if (Res.CurrentLanguage != threadLanguage)
			{
				SetCurrentLanguage(threadLanguage);
			}
			var form = CreateFormCore();
			form.Disposed += (s, e) =>
			{
				dbDisposable?.Dispose();
				if (DisposeOnFormDispose)
				{
					this.Dispose();
				}
			};
			return form;
		}

		static void SetCurrentLanguage(string language)
			=> ObjectFactory.Get<IResourceStrings>().CurrentLanguage = language;

		protected virtual FormType CreateFormCore()
		{
			return base.CreateForm();
		}

		IDisposable dbDisposable;
	}
}
