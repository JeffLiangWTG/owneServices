using System.Collections;
using System.IO;
using CargoWise.Common;
using ICSharpCode.SharpZipLib.Zip;

namespace Enterprise.ZArchitecture.Core
{
	public class ZipFileCore : Disposable, IEnumerable
	{
		readonly ZipFile zip;
		TempFile tempFile;
		string tempFileName;

		#region Constructors

		public ZipFileCore(string filename)
		{
			zip = new ZipFile(filename, null);
			tempFileName = filename;
		}

		public ZipFileCore(Stream stream)
		{
			if (stream is FileStream)
			{
				zip = new ZipFile(stream as FileStream);
			}
			else
			{
				zip = new ZipFile(stream);
			}
			CreateTempFile(stream);
		}

		public ZipFileCore(byte[] data)
		{
			CreateTempFile(data);
			zip = new ZipFile(tempFile.Filename, null);
		}

		void CreateTempFile(Stream stream)
		{
			stream.Position = 0;
			byte[] buffer = new byte[stream.Length];
			int offset = 0;
			while (offset < buffer.Length)
			{
				offset += stream.Read(buffer, offset, buffer.Length - offset);
			}

			CreateTempFile(buffer);
		}

		void CreateTempFile(byte[] data)
		{
			tempFile = TempFile.New();
			File.WriteAllBytes(tempFile.Filename, data);
			tempFileName = tempFile.Filename;
		}

		#endregion

		#region Disposable Members

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				zip.Close();
				if (tempFile != null)
				{
					tempFile.Dispose();
					tempFile = null;
					tempFileName = "";
				}
			}
		}

		#endregion

		#region Properties

		public void SetPassword(string password)
		{
			zip.Password = password;
		}

		public long Count
		{
			get { return zip.Count; }
		}

		public ZipEntry this[int i]
		{
			get { return zip[i]; }
		}

		public string TempFileName
		{
			get { return tempFile != null ? tempFile.Filename : tempFileName ?? ""; }
		}

		#endregion

		#region New methods

		public void RemoveEntries(ZipEntry[] entries)
		{
			foreach (ZipEntry entry in entries)
			{
				zip.Delete(entry);
			}
		}

		public void ExtractEntry(string filename, string destination)
		{
			ZipEntry entry = zip.GetEntry(filename) ?? throw new FileNotFoundException("File not found in zip archive", filename);

			using (Stream entryStream = zip.GetInputStream(entry))
			{
				FileStream sw = File.Create(Path.Combine(destination, Path.GetFileName(filename)));
				try
				{
					byte[] buffer = new byte[4096];

					int size;

					do
					{
						size = entryStream.Read(buffer, 0, buffer.Length);
						sw.Write(buffer, 0, size);
					}
					while (size > 0);
				}
				finally
				{
					sw.Close();
				}
			}
		}

		public void SaveToFile(string filename)
		{
			if (tempFile != null)
			{
				File.Copy(tempFile.Filename, filename, true);
			}
			else if (!string.IsNullOrEmpty(tempFileName))
			{
				File.Copy(tempFileName, filename, true);
			}
		}

		public bool HasEntry(string name)
		{
			return zip.GetEntry(name) != null;
		}

		#endregion

		#region Proxy methods

		public void BeginUpdate()
		{
			zip.BeginUpdate();
		}

		public void CommitUpdate()
		{
			zip.CommitUpdate();
		}

		public void Add(ZipEntry entry)
		{
			zip.Add(entry);
		}

		public void Add(string file)
		{
			zip.Add(file);
		}

		public IEnumerator GetEnumerator()
		{
			return zip.GetEnumerator();
		}

		#endregion
	}
}
