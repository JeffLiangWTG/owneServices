using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Business
{
	public class StmDataImportHistory : AutoStmDataImportHistory
	{
		public StmDataImportHistory(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG
		#region FillWithValidTestDataCore

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			var randomGenerator = System.Security.Cryptography.RandomNumberGenerator.Create();
			var dataHash = new byte[32];
			randomGenerator.GetBytes(dataHash);
			DIH_DataHash = new ZBlob(dataHash);
		}

		#endregion
#endif
	}
}
