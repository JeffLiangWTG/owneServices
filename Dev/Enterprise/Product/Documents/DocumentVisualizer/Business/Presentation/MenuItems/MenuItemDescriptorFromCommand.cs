using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	[DebuggerDisplay("{" + nameof(Caption) + "}")]
	sealed class MenuItemDescriptorFromCommand : IMenuItemDescriptor
	{
		public MenuItemDescriptorFromCommand(IResourceAccessor accessor, ICommand command)
		{
			this.accessor = accessor ?? throw new MacroRuntimeException(nameof(accessor));
			this.command = command ?? throw new MacroRuntimeException(nameof(command));

			this.imageUris = CreateImageUriDictionary();
		}

		readonly IResourceAccessor accessor;
		readonly ICommand command;

		readonly IReadOnlyDictionary<string, Uri> imageUris;

		public string Caption => command.Caption;

		public void Invoke()
		{
			command.Invoke();
		}

		public bool IsEnabled()
		{
			return command.IsEnabled;
		}

		public bool IsVisible()
		{
			return command.IsVisible;
		}

		public object Image
		{
			get
			{
				if (command.Image != null)
				{
					return command.Image;
				}

				if (image == null
					&& imageUris.TryGetValue(command.Id, out var uri))
				{
					image = accessor.Get(uri);
				}

				return image;
			}
		}

		object image;

		public IEnumerable<IMenuItemDescriptor> MenuItems => Enumerable.Empty<IMenuItemDescriptor>();

		#region Implementation

		IReadOnlyDictionary<string, Uri> CreateImageUriDictionary()
		{
			return new Dictionary<string, Uri>
			{
				[CommandIds.DeliverDocument] = new Uri(ImageUri.DeliverDocument),
				[CommandIds.SendMessage] = new Uri(ImageUri.SendMessage),
				[CommandIds.SendWithdrawal] = new Uri(ImageUri.WithdrawDocument),
				[CommandIds.ResetToOriginal] = new Uri(ImageUri.Reset),
				[CommandIds.ShowMacroEvaluator] = new Uri(ImageUri.MacroEvaluator),
				[CommandIds.ShowDocumentData] = new Uri(ImageUri.Data),
				[CommandIds.ShowMessagingData] = new Uri(ImageUri.Data),
				[CommandIds.ShowOverriddenData] = new Uri(ImageUri.Data),
				[CommandIds.ResetOverriddenData] = new Uri(ImageUri.Reset),
				[CommandIds.SaveOverriddenData] = new Uri(ImageUri.Save),
				[CommandIds.Exit] = new Uri(ImageUri.Cancel)
			};
		}

		#endregion
	}
}
