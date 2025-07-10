using System.ComponentModel;
using System.Drawing;
using CargoWise.Common.Testing;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	public class ZStatusBar : KStatusBar, ICaptionedComponents
	{
		public ZStatusBar()
		{
			UserEventTracker.Instance.AddUserEventToControl(this);
			DisposableLeakListener.Instance.RegisterDisposable(this);
			translationFeedbackManager = new TranslationFeedbackManager(this);
			devInfoPopupManager = new DevInfoPopupManager(this);
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (translationFeedbackManager != null)
				{
					translationFeedbackManager.Dispose();
				}
				if (devInfoPopupManager != null)
				{
					devInfoPopupManager.Dispose();
				}

				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}

			base.Dispose(isNotFinalizing);
		}

		#region ICaptionedComponents

		public object GetCaptionedComponentAt(Point p)
		{
			int? panelIndex = null;
			for (var i = 0; i < Panels.Count; i++)
			{
				if (GetCaptionedComponentRect(i).Contains(p))
				{
					panelIndex = i;
					break;
				}
			}
			return panelIndex;
		}

		public Rectangle GetCaptionedComponentRect(object component)
		{
			var x = 0;
			for (var i = 0; i < (int)component; i++)
			{
				x += Panels[i].Width;
			}
			return ControlDpiScalingHelper.NewScaledRectangle(x, 0, Panels[(int)component].Width, this.Height, false);
		}

		public object GetCaptionedComponentData(object component)
		{
			return Panels[(int)component].Text;
		}

		#endregion

		readonly internal TranslationFeedbackManager translationFeedbackManager;
		readonly internal DevInfoPopupManager devInfoPopupManager;
	}
}
