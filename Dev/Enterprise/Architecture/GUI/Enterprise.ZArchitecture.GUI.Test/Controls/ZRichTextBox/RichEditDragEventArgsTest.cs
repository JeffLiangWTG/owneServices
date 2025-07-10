using System;
using System.Windows.Forms;
using CargoWise.Interop.DataObjects;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class RichEditDragEventArgsTest : TestCase
	{
		public void TestConstructor()
		{
			var e = new DragEventArgs(new DataObject("a"), 0, 0, 0, DragDropEffects.All, DragDropEffects.None);
			var testGuid = Guid.NewGuid();
			using (var args = new RichEditDragEventArgs("a", testGuid, e))
			{
				Assert("Data object should be converted to a z data object", args.Data is ZDataObject);
				AssertEquals("Data Object should have parent link data set", testGuid, ((ZDataObject)args.Data).ParentIDLink);
				AssertEquals("Data Object should have parent link data set", "a", ((ZDataObject)args.Data).ParentTableLink);
			}
		}
	}
}
