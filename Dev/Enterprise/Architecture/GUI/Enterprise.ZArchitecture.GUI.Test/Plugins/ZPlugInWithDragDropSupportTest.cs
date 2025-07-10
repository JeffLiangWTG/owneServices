using System.Windows.Forms;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.PlugIn.Testing
{
	class ZPlugInWithDragDropSupportTest : ZPlugInTest
	{
		public void TestHookFormEvents()
		{
			using (var form = new ZPlugInsTest.TestFormWithTabControl(Dummy))
			{
				form.PlugIns.Add(DummyControllerIDs.Dummy1); // dummy plug in inherits from this class

				var plugIn = form.PlugIns.Instances[0] as DummyPlugIn1;
				AssertNotNull("Plug in should be available", plugIn);

				var dataObject = new DataObject("hello");
				var dragArgs = new DragEventArgs(dataObject, 0, 0, 0, DragDropEffects.All, DragDropEffects.None);
				form.OnDragDrop_ForTesting(dragArgs);
				AssertEquals("Form's plugin should be receiving the form dragdrop event", 1, plugIn.OnParentFormDragDropCount);
				AssertEquals("other events shouldn't have been fired yet", 0, plugIn.OnParentFormDataObjectPastedCount);
				AssertEquals("other events shouldn't have been fired yet", 0, plugIn.OnParentFormDragOverCount);
				AssertNotNull("Form's plugin user control should be set up", plugIn.fUserControl);

				form.OnDragOver_ForTesting(dragArgs);
				AssertEquals("Form's plugin should be receiving the form dragover event", 1, plugIn.OnParentFormDragOverCount);
				AssertEquals("other events shouldn't have been fired yet", 0, plugIn.OnParentFormDataObjectPastedCount);

				form.OnDataObjectPasted_ForTesting(dataObject);
				AssertEquals("Form's plugin should be receivng the form's paste event", 1, plugIn.OnParentFormDataObjectPastedCount);
				AssertNotNull("Form's plugin user control should be set up", plugIn.fUserControl);
			}
		}
	}
}
