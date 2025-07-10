using System;
using System.Windows.Forms;
using System.Windows.Input;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class TranslationFeedbackManager : Disposable
	{
		public static bool InTranslationFeedbackMode()
		{
			return (InTranslationFeedbackModeForTest || Keyboard.IsKeyDown(Key.F2)) && IsTranslationFeedbackAccessible();
		}

		internal static bool IsTranslationFeedbackAccessible()
		{
			using (Db.DisposableActionForDbConnection())
			{
				return
					(!Res.IsEnglish(Res.CurrentLanguage) || Globals.IsDebugMode) &&
					EnvProxy.Instance.Security != null && EnvProxy.Instance.Security.TranslationFeedback.IsAllowed;
			}
		}

		public TranslationFeedbackManager(Control control, ClickMode clickMode = ClickMode.OnClick)
		{
			this.control = control;
			this.clickMode = clickMode;
			switch (clickMode)
			{
				case ClickMode.OnClick:
					control.Click += new EventHandler(control_Click);
					break;
				case ClickMode.OnMouseUp:
					control.MouseUp += new MouseEventHandler(control_MouseUp);
					break;
			}
		}

		public bool InMode => InTranslationFeedbackModeForTest || Keyboard.IsKeyDown(Key.F2);

		protected virtual void OpenFeedbackForm()
		{
			OpenFeedbackForm(control);
		}

		protected override void Dispose(bool isDisposing)
		{
			switch (clickMode)
			{
				case ClickMode.OnClick:
					control.Click -= new EventHandler(control_Click);
					break;
				case ClickMode.OnMouseUp:
					control.MouseUp -= new MouseEventHandler(control_MouseUp);
					break;
			}
		}

		readonly protected internal Control control;
		readonly ClickMode clickMode;
	}
}
