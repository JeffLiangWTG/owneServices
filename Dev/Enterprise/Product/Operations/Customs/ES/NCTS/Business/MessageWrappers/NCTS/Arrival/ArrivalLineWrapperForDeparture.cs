using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalLineWrapperForDeparture : BaseArrivalLineWrapper<NctsDepartureCargoDesc>
	{
		public ArrivalLineWrapperForDeparture(NctsDepartureCargoDesc goodsItem)
			: base(goodsItem)
		{
		}

		public override ZString GoodsCustomsProcedureCategory3 => ZString.Empty;

		public override ZString GoodsCustomsProcedureCategory4 => ZString.Empty;

		public override ZString GoodsCustomsProcedureCategory5 => ZString.Empty;

		protected override ExternalPackagesInfoCommonWrapper GetExternalPackagesInfoCommonWrapper() => new ExternalPackagesInfoCommonWrapper(GoodsItem.ContainersSelected);

		protected override IInternalPackagesInfoCommon GetNCTSInternalPackagesInfoCommonWrapper() => new NctsDepartureInternalPackagesInfoCommonWrapper(GoodsItem);

		protected override List<ZString> GetNotSubmittedC44Documents() => new List<ZString>();

		protected override List<DocumentCommonWrapper> GetDocuments() => new List<DocumentCommonWrapper>();
	}
}
