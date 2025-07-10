using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;
using CusEntryNumHelperForCargoControlNumber = Enterprise.Customs.Business.CusEntryNumHelperForCargoControlNumber;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CargoControlNumber))]
	sealed class CACargoControlNumberTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<CargoControlNumber>
	{
		public void TestGetCargoControlNumberFromCusEntryNumberQuery()
		{
			const string expect1 = " IN (SELECT  FROM dbo.CusEntryNum WHERE CE_EntryNum = 'CCN001' and CE_ParentTable = 'JobDeclaration' and CE_RN_NKCountryCode = 'CA' and CE_EntryType = 'CCN')";
			AssertEquals(expect1, CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusEntryNumberQuery(JobDeclaration.Schema.TableName, SQLComparisonOperator.Equal, "CCN001").LiteralTextADO);

			const string expect2 = " NOT IN (SELECT  FROM dbo.CusEntryNum WHERE CE_EntryNum = 'CCN001' and CE_ParentTable = 'JobDeclaration' and CE_RN_NKCountryCode = 'CA' and CE_EntryType = 'CCN')";
			AssertEquals(expect2, CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusEntryNumberQuery(JobDeclaration.Schema.TableName, SQLComparisonOperator.NotEqual, "CCN001").LiteralTextADO);

			const string expect3 = " NOT IN (SELECT  FROM dbo.CusEntryNum WHERE CE_EntryNum <> '' and CE_ParentTable = 'JobDeclaration' and CE_RN_NKCountryCode = 'CA' and CE_EntryType = 'CCN')";
			AssertEquals(expect3, CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusEntryNumberQuery(JobDeclaration.Schema.TableName, SQLComparisonOperator.IsBlank, "").LiteralTextADO);

			const string expect4 = " IN (SELECT  FROM dbo.CusEntryNum WHERE CE_EntryNum <> '' and CE_ParentTable = 'JobDeclaration' and CE_RN_NKCountryCode = 'CA' and CE_EntryType = 'CCN')";
			AssertEquals(expect4, CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusEntryNumberQuery(JobDeclaration.Schema.TableName, SQLComparisonOperator.IsNotBlank, "").LiteralTextADO);
		}

		public void TestGetGetCargoControlNumberFromCusAddInfoQuery()
		{
			const string expect1 = " IN (SELECT  FROM dbo.CusAddInfo WHERE B7_Type = 'CAC' and B7_ParentTableCode = 'JE' and (B7_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_ParentTableCode = 'B7' and XA_Name = 'CA_CCNInfoNumber' and XA_Data = 'CCN001')))";
			AssertEquals(expect1, CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusAddInfoQuery(SQLComparisonOperator.Equal, "CCN001").LiteralTextADO);

			const string expect2 = " NOT IN (SELECT  FROM dbo.CusAddInfo WHERE B7_Type = 'CAC' and B7_ParentTableCode = 'JE' and (B7_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_ParentTableCode = 'B7' and XA_Name = 'CA_CCNInfoNumber' and XA_Data = 'CCN001')))";
			AssertEquals(expect2, CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusAddInfoQuery(SQLComparisonOperator.NotEqual, "CCN001").LiteralTextADO);
			const string expect3 = " NOT IN (SELECT  FROM dbo.CusAddInfo WHERE B7_Type = 'CAC' and B7_ParentTableCode = 'JE' and (B7_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_ParentTableCode = 'B7' and XA_Name = 'CA_CCNInfoNumber' and XA_Data <> '')))";
			AssertEquals(expect3, CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusAddInfoQuery(SQLComparisonOperator.IsBlank, "").LiteralTextADO);

			const string expect4 = " IN (SELECT  FROM dbo.CusAddInfo WHERE B7_Type = 'CAC' and B7_ParentTableCode = 'JE' and (B7_PK IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_ParentTableCode = 'B7' and XA_Name = 'CA_CCNInfoNumber' and XA_Data <> '')))";
			AssertEquals(expect4, CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusAddInfoQuery(SQLComparisonOperator.IsNotBlank, "").LiteralTextADO);
		}

		public void TestCargoControlNumber()
		{
			var ccn = Factory.New<JobDeclaration>().CargoControlNumbers.AddNew();
			ccn.CY_CargoControlNumber = " 123456789 ";
			AssertEquals(" 123456789", ccn.CY_CargoControlNumber);
			ccn.CY_CargoControlNumber = " 123456789 0123";
			AssertEquals(" 123456789 0123", ccn.CY_CargoControlNumber);
		}

		#region Implementation

		protected override IEnumerable<CargoControlNumber> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().CargoControlNumbers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().CargoControlNumbers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<JobDeclaration>().CargoControlNumbers.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return Factory.New<JobDeclaration>().CargoControlNumbers.AddNew();
		}
		#endregion
	}
}
