using Enterprise.DocumentVisualizer.Presentation;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(DataEditing))]
	sealed class DataEditingTest : CommandProviderTest
	{
		#region Implementation

		protected override ICommandProvider CreateNewModule()
		{
			return new DataEditing();
		}

		#endregion
	}
}
