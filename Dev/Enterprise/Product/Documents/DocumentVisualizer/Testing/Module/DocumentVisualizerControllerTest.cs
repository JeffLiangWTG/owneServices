using Enterprise.DocumentVisualizer.Module;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.Module
{
	[TestedType(typeof(DocumentVisualizerController))]
	sealed class DocumentVisualizerControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DocumentVisualizer;
		}
	}
}
