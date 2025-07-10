using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Common.GUI.Import
{
	public class ImportStatus : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region ImportedRecords

		[ReadOnly(true)]
		public ZInt ImportedRecords
		{
			get { return importedRecords; }
			set { SetNonPersistentPropertyValue(RecordsAddedInfo, ref importedRecords, value); }
		}
		ZInt importedRecords;

		public ZPropertyInfo RecordsAddedInfo
		{
			get { return GetZPropertyInfo(nameof(ImportedRecords)); }
		}

		#endregion

		#region ErrorRecords

		[ReadOnly(true)]
		public ZInt ErrorRecords
		{
			get { return errorRecords; }
			set { SetNonPersistentPropertyValue(RecordsUpdatedInfo, ref errorRecords, value); }
		}
		ZInt errorRecords;

		public ZPropertyInfo RecordsUpdatedInfo
		{
			get { return GetZPropertyInfo(nameof(ErrorRecords)); }
		}

		#endregion

		public void Reset()
		{
			ImportedRecords = 0;
			ErrorRecords = 0;
		}
	}
}
