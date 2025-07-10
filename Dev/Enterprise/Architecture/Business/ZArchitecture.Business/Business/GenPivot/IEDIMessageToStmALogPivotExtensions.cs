using Enterprise.Core;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public static class IEDIMessageToStmALogPivotExtensions
	{
		public static void AddUniversalDataLink(this IEDIMessage message, StmALog stmALog)
		{
			var messageLogPivot = stmALog.Factory.New<GenPivot>();
			messageLogPivot.XX_RelationType = Constants.GenPivotTypes.XmlEdiMessage;

			messageLogPivot.XX_Relation1TableCode = StmALogSchema.Constants.Prefix;
			messageLogPivot.XX_Relation1ID = stmALog.PK;

			messageLogPivot.XX_Relation2TableCode = EDIMessageSchema.Constants.Prefix;
			messageLogPivot.XX_Relation2ID = message.PK;
		}
	}
}
