using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public sealed class DisabledCommand : ICommand
	{
		DisabledCommand(string commandId)
		{
			Id = commandId;
			Caption = CommandResources.Captions.GetCaptionForCommand(commandId);
		}

		public static DisabledCommand SendMessage => new DisabledCommand(CommandIds.SendMessage);
		public static DisabledCommand SendWithdrawal => new DisabledCommand(CommandIds.SendWithdrawal);
		public static DisabledCommand ResetToOriginal => new DisabledCommand(CommandIds.ResetToOriginal);

		public string Id { get; }
		public string Caption { get; }
		public object Image { get; }
		public bool IsEnabled => false;
		public bool IsVisible => true;
		public bool Invoke() => Invoke(null);
		public bool Invoke(MacroMap parameters) => false;

		public void Initialize(object documentInfo)
		{
			// ignore
		}
	}
}
