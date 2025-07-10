using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public class FormOpenResult
	{
		public FormOpenResult(ZController controller, IZForm form)
		{
			this.Controller = controller;
			this.Form = form;
		}

		public ZController Controller {	get; }
		public IZForm Form { get; }

		public static FormOpenResult Empty => new FormOpenResult(null, null);
	}
}
