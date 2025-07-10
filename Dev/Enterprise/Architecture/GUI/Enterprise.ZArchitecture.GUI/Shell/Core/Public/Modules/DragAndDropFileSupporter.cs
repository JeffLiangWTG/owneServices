using System.IO;
using System.Text;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Modules
{
	public delegate void DragAndDropSingleFileHandler(string fileName);

	/// <summary>
	/// Establishes events for a control and fires a handler when a single file is dragged onto the control surface
	/// Can be dragged from email or filesystem.
	/// </summary>
	public class DragAndDropFileSupporter
	{
		public DragAndDropFileSupporter(Control parentControl, DragAndDropSingleFileHandler handler)
		{
			if (handler != null)
			{
				parentControl.AllowDrop = true;
				parentControl.DragDrop += new DragEventHandler(EmbeddedControl_DragDrop);
				parentControl.DragOver += new DragEventHandler(EmbeddedControl_DragOver);
				this.handler = handler;
			}
		}
		readonly DragAndDropSingleFileHandler handler;

#if DEBUG
		internal
#endif
 void EmbeddedControl_DragOver(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.None;    // By default nothing happens
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				var files = (string[])e.Data.GetData(DataFormats.FileDrop);
				var hasFile = files.Length > 0;
				foreach (var file in files)
				{
					if (!File.Exists(file)) // Must be a real file - not a directory
					{
						hasFile = false;
						break;
					}
				}
				if (hasFile)
				{
					e.Effect = DragDropEffects.Copy;
				}
			}
			else
			{
				var fileName = GetFileGroupDescriptorFileName(e.Data);
				if (!string.IsNullOrEmpty(fileName))
				{
					e.Effect = DragDropEffects.Copy;
				}
			}
		}

#if DEBUG
		internal
#endif
 void EmbeddedControl_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				var files = (string[])e.Data.GetData(DataFormats.FileDrop);
				foreach (var fileName in files)
				{
					handler(fileName);
				}
			}
			else    // Try to process as file group
			{
				var fileName = GetFileGroupDescriptorFileName(e.Data);
				if (!string.IsNullOrEmpty(fileName))
				{
					var tempFileName = Path.Combine(EnvProxy.Instance.TempPath, fileName);
					long memoryStreamLength = -1;
					var fileContentsStream = (MemoryStream)e.Data.GetData("FileContents");
					if (fileContentsStream != null)
					{
						memoryStreamLength = fileContentsStream.Length;
						using (var tempFileStream = new FileStream(tempFileName, FileMode.Create))
						{
							fileContentsStream.WriteTo(tempFileStream);
						}
						fileContentsStream.Dispose();
						handler(tempFileName);
					}
				}
			}
		}

		const int GroupDescFileNameOffset = 76;
		const int fileNameMaxLength = 256;

		string GetFileGroupDescriptorFileName(IDataObject data)
		{
			using (var fileGroupDescriptorStream = (MemoryStream)data.GetData("FileGroupDescriptor"))
			{
				fileGroupDescriptorStream.Seek(GroupDescFileNameOffset, SeekOrigin.Begin);

				var fileNameBuffer = new byte[fileNameMaxLength];
				var offset = 0;
				while (offset < fileNameMaxLength)
				{
					offset += fileGroupDescriptorStream.Read(fileNameBuffer, offset, fileNameMaxLength - offset);
				}
				return Encoding.ASCII.GetString(fileNameBuffer).TrimEnd('\0');
			}
		}
	}
}
