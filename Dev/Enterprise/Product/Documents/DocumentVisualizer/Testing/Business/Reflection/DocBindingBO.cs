using System.Collections.Generic;

namespace Enterprise.DocumentVisualizer.Testing.Business.Reflection
{
	internal class DocBindingBO
	{
		public IReadOnlyCollection<ReadonlyBO> ReadonlyBOs
		{
			get => readonlyBOs;
			set => readonlyBOs = value;
		}

		IReadOnlyCollection<ReadonlyBO> readonlyBOs;

		public CustomReadonlyCollection ReadonlyCollection
		{
			get => readonlyCollection;
			set => readonlyCollection = value;
		}

		CustomReadonlyCollection readonlyCollection;
	}
}
