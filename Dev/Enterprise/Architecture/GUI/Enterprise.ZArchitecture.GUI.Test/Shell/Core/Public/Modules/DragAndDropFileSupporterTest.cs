using System.IO;
using System.Text;
using System.Windows.Forms;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DragAndDropFileSupporterTest : TestCase
	{
		public void TestFileDrop()
		{
			var hasCalledDelegate = false;
			using (Control control = new Form())
			{
				var supporter = new DragAndDropFileSupporter(control, delegate(string fileName)
					{
						hasCalledDelegate = true;
						AssertEquals("Blah.txt", fileName);
					}
				);
				var mock = new Mock<IDataObject>();
				mock.Setup(o => o.GetDataPresent(DataFormats.FileDrop)).Returns(true);
				mock.Setup(o => o.GetData(DataFormats.FileDrop)).Returns(new string[] { "Blah.txt" });
				var args = new DragEventArgs(mock.Object, 0, 0, 0, DragDropEffects.All, DragDropEffects.All);
				supporter.EmbeddedControl_DragDrop(this, args);
			}
			Assert("Delegate failed to be called", hasCalledDelegate);
		}

		public void TestFileGroupDrop()
		{
			var hasCalledDelegate = false;
			using (Control control = new Form())
			{
				var supporter = new DragAndDropFileSupporter(control, delegate(string fileName)
				{
					hasCalledDelegate = true;
					try
					{
						Assert(fileName.EndsWith("Blah.txt"));
						AssertEquals("HELLO", File.ReadAllText(fileName));
					}
					finally
					{
						File.Delete(fileName);
					}
				}
				);
				var mock = new Mock<IDataObject>();
				mock.Setup(o => o.GetData(DataFormats.FileDrop)).Returns(false);
				var blah = new string('\0', 76) + "Blah.txt" + new string('\0', 256);
				var buffer = Encoding.ASCII.GetBytes(blah);
				var stream = new MemoryStream(buffer);
				mock.Setup(o => o.GetData("FileGroupDescriptor")).Returns(stream);

				var bufferFileContents = Encoding.ASCII.GetBytes("HELLO");
				var streamFileContents = new MemoryStream(bufferFileContents);
				mock.Setup(o => o.GetData("FileContents")).Returns(streamFileContents);

				var args = new DragEventArgs(mock.Object, 0, 0, 0, DragDropEffects.All, DragDropEffects.All);
				supporter.EmbeddedControl_DragDrop(this, args);
			}
			Assert("Delegate failed to be called", hasCalledDelegate);
		}
	}
}
