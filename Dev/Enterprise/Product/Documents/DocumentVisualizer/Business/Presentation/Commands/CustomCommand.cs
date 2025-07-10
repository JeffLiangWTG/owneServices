using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public abstract class CustomCommand : ICommand, INotifiableDocumentInfoCreated
	{
		protected IDocumentInfo documentInfo;

		public abstract string Id { get; }
		public abstract string Caption { get; }
		public virtual object Image { get; }
		public abstract bool IsEnabled { get; }
		public abstract bool IsVisible { get; }

		public abstract bool Invoke();

		public bool Invoke(MacroMap parameters) => Invoke();

		public void NotifyDocumentInfoCreated(IDocumentInfo documentInfo)
		{
			this.documentInfo = documentInfo;
		}
	}
}
