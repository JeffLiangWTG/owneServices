using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.Statement.Testing;

[TestedType(typeof(DocStatementChargesCollection))]
sealed class DocStatementChargesCollectionTest : DocBaseWrapperCollectionTest<DocStatementChargesCollection>
{
	protected override DocStatementChargesCollection GetNewDocumentWrapperCollection()
	{
		return new DocStatementChargesCollection(CusStatementLineChargeCollection, Factory);
	}

	protected override object GetNewObjectToWrap()
	{
		return null;
	}

	CusStatementLineChargeCollection CusStatementLineChargeCollection
	{
		get
		{
			if (cusStatementLineChargeCollection == null)
			{
				var header = Factory.New<CusStatementHeader>();
				header.B2_StatementType = StatementPeriodicityList.Codes.Day;
				cusStatementLineChargeCollection = header.ChargesDetail.Charges;
			}
			return cusStatementLineChargeCollection;
		}
	}
	CusStatementLineChargeCollection cusStatementLineChargeCollection;
}
