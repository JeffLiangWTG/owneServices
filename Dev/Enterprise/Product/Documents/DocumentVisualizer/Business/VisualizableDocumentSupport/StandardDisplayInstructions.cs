using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class StandardDisplayInstructions : IDisplayInstructions
	{
		public StandardDisplayInstructions(IStandardTemplate template, IMacroScope scope, IMacroEvaluationContext context)
		{
			Argument.NotNull(template, nameof(template));
			this.template = template;
			this.scope = scope;
			this.context = context;
		}

		readonly IStandardTemplate template;
		readonly IMacroScope scope;
		readonly IMacroEvaluationContext context;

		public bool ShowEvents => showEvents ?? (showEvents = template.GetShowEvents(scope, context)).Value;
		bool? showEvents;

		public bool ShowLastEventDetails => showLastEventDetails ?? (showLastEventDetails = template.GetShowLastEventDetails()).Value;
		bool? showLastEventDetails;

		public IEnumerable<IMenuItemDescriptor> MenuItems
		{
			get => menuItems ?? Enumerable.Empty<IMenuItemDescriptor>();
			set => menuItems = value;
		}
		IEnumerable<IMenuItemDescriptor> menuItems;
	}
}