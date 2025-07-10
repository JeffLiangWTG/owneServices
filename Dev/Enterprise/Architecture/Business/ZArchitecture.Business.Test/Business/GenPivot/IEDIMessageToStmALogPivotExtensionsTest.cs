using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class IEDIMessageToStmALogPivotExtensionsTest : TestCaseWithFactory
	{
		public void TestExtensionMethodCreatesTheGenPivotCorrectly()
		{
			var message = Factory.New<IEDIMessage>();
			var log = Factory.New<StmALog>();

			message.AddUniversalDataLink(log);

			var query = new ZQuery(GenPivotSchema.XX_Relation1ID, log.PK);
			query.AddToFilter(GenPivotSchema.XX_Relation2ID, message.PK);
			var pivot = Factory.LoadTop1<GenPivot>(query);
			AssertNotNull("GenPivot matching PKs of Log and Message", pivot);
			AssertEquals("pivot.XX_RelationType", Constants.GenPivotTypes.XmlEdiMessage, pivot.XX_RelationType);
			AssertEquals("pivot.XX_Relation1TableCode", StmALogSchema.Constants.Prefix, pivot.XX_Relation1TableCode);
			AssertEquals("pivot.XX_Relation2TableCode", EDIMessageSchema.Constants.Prefix, pivot.XX_Relation2TableCode);
		}
	}
}
