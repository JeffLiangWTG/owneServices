using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	public class TestImportingBizObj : DummyEnterpriseBusinessObject, ISupportDataImporting
	{
		public TestImportingBizObj(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "TestImportingBizObj " + Name; }
		}

		public ZString Name
		{
			get { return fName; }
			set { fName = value; }
		}

		ZString fName;

		#region ISupportDataImporting Members

		bool fIsImportingData;
		public bool IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}

		#endregion
	}
}
