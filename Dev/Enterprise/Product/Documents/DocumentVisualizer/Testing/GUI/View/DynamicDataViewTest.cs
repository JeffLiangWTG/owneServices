using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Models;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	[TestedType(typeof(DynamicDataView))]
	sealed class DynamicDataViewTest : ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var document = new DummyDocument
			{
				Data = dummy.MakeDynamic()
			};

			return new DynamicDataView(document, DataViewModel.DataType.Messaging);
		}
	}
}
