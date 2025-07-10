using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface IArrivalLine : INctsLineMessageProvider
	{
		#region Fields For CST

		ZString GoodsCustomsProcedureCategory2 { get; }
		ZString GoodsCustomsProcedureCategory3 { get; }
		ZString GoodsCustomsProcedureCategory4 { get; }
		ZString GoodsCustomsProcedureCategory5 { get; }

		#endregion

		#region Fields For FTX

		IReadOnlyCollection<ZString> NotSubmittedC44Documents { get; }

		#endregion

		#region Fields For MEA

		ZDecimal NetWeightInKG { get; }

		#endregion

		#region Fields For PAC

		IInternalPackagesInfoCommon InternalPackages { get; }

		#endregion

		#region Fields For MOA

		ZDecimal TotalGoodValueInEuros { get; }

		#endregion

		#region Fields For DOC

		IReadOnlyCollection<IDocumentsCommon> Documents { get; }

		#endregion
	}
}
