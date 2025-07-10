using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalNCTS5IndicatorsWrapper : IArrivalNCTSIndicators
	{
		public ArrivalNCTS5IndicatorsWrapper(NctsHeader header)
		{
			nctsHeader = Argument.NotNull(header, nameof(header));
			esNctsHeader = Argument.NotNull(nctsHeader.ESNctsHeader, nameof(nctsHeader.ESNctsHeader));
			arrivalMovementHeader = Argument.NotNull(nctsHeader.ArrivalMovementHeader, nameof(nctsHeader.ArrivalMovementHeader));
		}
		protected readonly NctsHeader nctsHeader;
		protected readonly CusESNctsHeader esNctsHeader;
		readonly NctsArrivalMovementHeader arrivalMovementHeader;

		public ZString GoodsDirectlyShipped => esNctsHeader.CEN_AutomaticTranshipment ? AutomaticTranshipment1 : AutomaticTranshipment0;

		public ZString AutomaticCompletion => esNctsHeader.CEN_AutomaticCompletion ? AutomaticCompletionUltimated : null;

		public ZString TIRPageCompletion => esNctsHeader.CEN_TIRArrival ? esNctsHeader.CEN_TIRCarnetPage.ToString() : null;

		public ZString TIRParcialTotalUnloading => esNctsHeader.CEN_TIRArrival ? esNctsHeader.CEN_TIRPartialUnloading ? TIRPartialUnloadingP : TIRPartialUnloadingT : null;

		public ZString ReceptionSummary => esNctsHeader.CEN_PreviousSummaryDeclaration;

		public ZString SummaryTypeIndicator => esNctsHeader.CEN_SummaryType;

		public IReadOnlyCollection<IArrivalNCTS5PreviousG4> PreviousG4
		{
			get
			{
				if (previousG4 == null)
				{
					var previousG4List = new List<ArrivalNCTS5PreviousG4Wrapper>();

					ZShort seqNum = 1;
					foreach (var doc in arrivalMovementHeader.G4PreviousDocuments)
					{
						previousG4List.Add(new ArrivalNCTS5PreviousG4Wrapper(seqNum, doc.CSI_ReferenceNumber));
						seqNum++;
					}

					previousG4 = previousG4List.AsReadOnly();
				}
				return previousG4;
			}
		}
		IReadOnlyCollection<ArrivalNCTS5PreviousG4Wrapper> previousG4;

		const string AutomaticTranshipment0 = "0";
		const string AutomaticTranshipment1 = "1";
		const string AutomaticCompletionUltimated = "U";
		const string TIRPartialUnloadingP = "P";
		const string TIRPartialUnloadingT = "T";
	}
}
