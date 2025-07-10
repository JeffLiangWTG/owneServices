using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.GUI
{
	public sealed class ControlBehaviourContainerCollection : Dictionary<string, IControlBehaviourContainer>
	{
		public void Add(IControlBehaviourContainer controlBehaviourContainer)
		{
			Argument.NotNull(controlBehaviourContainer, nameof(controlBehaviourContainer));
			Argument.NotNull(controlBehaviourContainer.ControlBehaviour, nameof(controlBehaviourContainer.ControlBehaviour));

			var behaviorName = controlBehaviourContainer.ControlBehaviour.Name;

			if (ContainsKey(behaviorName))
			{
				this[behaviorName] = controlBehaviourContainer;
				return;
			}

			Add(behaviorName, controlBehaviourContainer);
		}

		public IReadOnlyCollection<IControlBehaviourContainer> GetAll()
		{
			return Values;
		}
	}
}
