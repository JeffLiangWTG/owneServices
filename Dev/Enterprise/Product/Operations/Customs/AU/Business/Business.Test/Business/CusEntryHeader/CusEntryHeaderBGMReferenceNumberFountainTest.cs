using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusEntryHeaderBGMReferenceNumberFountainTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestSetBGMReference_NoDuplicateReferenceException()
		{
			var factory = new BusinessObjectFactory();
			var dbConnection = ((CargoWise.Data.IDbConnected)factory).Connection;

			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.Factory.Save();

			var header1 = declaration.CustomsEntryHeaders.AddNew();
			ZString bgmReference;

			AssertEquals("NUnit.Framework.TestCase should not be in a transaction", false, dbConnection.IsInTransaction);
			try
			{
				dbConnection.BeginTransaction();
				header1.PopulateCH_BGMReferenceIfNeeded();
				bgmReference = header1.CH_BGMReference;
				AssertEndsWith("Message reference number ends with '/1'", "/1", bgmReference);
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}

			var header2 = declaration.CustomsEntryHeaders.AddNew();
			declaration.Factory.Save();
			AssertEquals("MessageReference is unchanged", bgmReference, header1.CH_BGMReference);
			AssertNotEquals("A new number is allocated", bgmReference, header2.CH_BGMReference);

			bgmReference = header2.CH_BGMReference;
			header2.PopulateCH_BGMReferenceIfNeeded();
			AssertEquals("Not allocated twice", bgmReference, header2.CH_BGMReference);
			AssertEndsWith("Message reference number ends with '/2'", "/2", bgmReference);
		}
	}
}
