using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.Business
{
	public class DocManagerDataObject : GridRowsDataObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public DocManagerDataObject(ZGrid grid, ICollection<BusinessObject> selectedBusinessObjects)
			: base(grid, selectedBusinessObjects)
		{
			SetData(DataFormatType, null);
			SetData(DataFormats.FileDrop, null);
		}

		public DocManagerDataObject(SerializableEDoc serializableEDoc)
			: this(null, System.Array.Empty<BusinessObject>())
		{
			((SerializableEDocCollection)Elements).Add(serializableEDoc);
		}

		SerializableEDocCollection Data
		{
			get { return (SerializableEDocCollection)Elements; }
		}

		public new static string DataFormatType
		{
			get { return DataFormats.GetFormat(typeof(SerializableEDocCollection).FullName).Name; }
		}

		protected override WrappedBusinessObjectCollection GetNewSerializableBusinessObjectCollection()
		{
			return new SerializableEDocCollection();
		}

		/// <summary>
		/// When data is retrieved as FileDrop, the files are on timed delete and will be cleaned up after 60 seconds. 
		/// </summary>
		public override object GetData(string format)
		{
			if (format == DataFormatType)
			{
				return Data;
			}
			else if (format == DataFormats.FileDrop)
			{
				if (Files == null || Files.Length > 0 && !File.Exists(Files[0]))
				{
					Files = Data.GetContentsAsFiles();
				}
				return Files;
			}
			else
			{
				return base.GetData(format);
			}
		}

		string[] Files;
	}
}
