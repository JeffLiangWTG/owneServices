using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinzorFramework;

namespace Enterprise.ZArchitecture.GUI
{
	class ReloadFormWinzorDispatcherContext : IWinzorDispatcherContext
	{
		public ReloadFormWinzorDispatcherContext(IWinzorDispatcherContext context)
		{
			inner = context;
		}

		public OpenFormAction OpenForm(Form form)
		{
			if (form.GetType() == Form?.GetType())
			{
				Form.CargoWiseClientServices.RenderService.ReplaceRenderedForm(form);
				return OpenFormAction.BlockUntilShown;
			}
			else
			{
				return inner.OpenForm(form);
			}
		}

		public void InvokeRenderDispatcher(Func<Task> workItem) => inner.InvokeRenderDispatcher(workItem);

		public void NotifyRenderRequired(Control control) => inner.NotifyRenderRequired(control);

		public void OnEnterMessageLoop() => inner.OnEnterMessageLoop();

		public void RegisterRenderTask(Task task) => inner.RegisterRenderTask(task);

		public Form Form => inner.Form;

		readonly IWinzorDispatcherContext inner;
	}
}
