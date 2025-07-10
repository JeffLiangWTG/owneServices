using System;
using CargoWise.Common;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class Command : ICommand
	{
		public Command(string id, string caption)
		{
			Argument.NotNullOrEmpty(id, nameof(id));

			Id = id;
			Caption = caption ?? CommandResources.Captions.GetCaptionForCommand(id);
		}

		public Action<MacroMap> Invoker { get; set; }

		public Func<bool> IsEnabled { get; set; }

		public Func<bool> IsVisible { get; set; }

		public string Id { get; }

		public string Caption { get; }

		public object Image { get; }

		public bool Invoke()
		{
			return Invoke(null);
		}

		public bool Invoke(MacroMap parameters)
		{
			Invoker?.Invoke(parameters);
			return true;
		}

		bool ICommand.IsEnabled => IsEnabled == null && Invoker != null || (IsEnabled?.Invoke() ?? false);

		bool ICommand.IsVisible => IsVisible == null && Invoker != null || (IsVisible?.Invoke() ?? false);
	}
}
