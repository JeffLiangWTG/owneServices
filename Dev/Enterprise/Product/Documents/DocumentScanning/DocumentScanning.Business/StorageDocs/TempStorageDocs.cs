using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// This class is only used to create a temp doc for selected file in Preview Tab of Allocate eDocs module,
	/// which is then used to show preview of the selected file.
	/// </summary>
	[TestedAsNonPersistentBusinessObject]
	public class TempStorageDocs : StorageDocs
	{
		public TempStorageDocs(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}
	}
}
