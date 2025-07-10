using System;
using System.Reactive.Disposables;
using CargoWise.Application;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentVisualizer.Presentation
{
	/// <summary>
	/// Enhances any ICommand by adding Progress form to Invoke
	/// </summary>
	public sealed class ProgressCommand : ICommand, INotifiableDocumentInfoCreated
	{
		public ProgressCommand(ICommand command)
		{
			this.command = command ?? throw new ArgumentNullException(nameof(command));
		}

		readonly ICommand command;

		public string Id => command.Id;
		public string Caption => command.Caption;
		public object Image => command.Image;
		public bool IsEnabled => command.IsEnabled;
		public bool IsVisible => command.IsVisible;

		public bool Invoke()
		{
			using (CreateProgressManager())
			{
				return command.Invoke();
			}
		}

		public bool Invoke(MacroMap parameters)
		{
			using (CreateProgressManager())
			{
				return command.Invoke(parameters);
			}
		}

		IDisposable CreateProgressManager()
		{
			if (Globals.IsUserInteractive)
			{
				var manager = ObjectFactory.Get<IProgressManager>();
				manager.Start();
				return manager;
			}

			return Disposable.Empty;
		}

		#region INotifiableDocumentInfoCreated members

		public void NotifyDocumentInfoCreated(IDocumentInfo documentInfo)
		{
			if (documentInfo == null)
			{
				return;
			}

			if (command is INotifiableDocumentInfoCreated notifiable)
			{
				notifiable.NotifyDocumentInfoCreated(documentInfo);
			}
		}

		#endregion
	}
}
