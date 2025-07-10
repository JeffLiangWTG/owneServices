using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class StmDataImportHistoryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDIH_DataHash()
		{
			var importHistory = Factory.New<StmDataImportHistory>();
			AssertNoErrors(importHistory.DIH_DataHashInfo);

			importHistory.DIH_DataHash = new ZBlob(Enumerable.Repeat<byte>(0, 16).ToArray());
			AssertHasErrorContaining(importHistory.DIH_DataHashInfo, "Data Hash must be 32 bytes; please use SHA256.");

			importHistory.DIH_DataHash = new ZBlob(Enumerable.Repeat<byte>(0, 32).ToArray());
			AssertNoErrors(importHistory.DIH_DataHashInfo);
		}
	}
}
