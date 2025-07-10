using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public abstract class BaseArrivalLineWrapper<T> : IArrivalLine where T : NctsCommonCargoDesc
	{
		protected BaseArrivalLineWrapper(T goodsItem)
		{
			GoodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}
		protected readonly T GoodsItem;

		public ZInt GoodsItemNumber => GoodsItem.BY_LineNo;

		public ZString GoodsCustomsProcedureCategory1 => GoodsItem.BY_HarmonisedTariff;

		public ZString GoodsCustomsProcedureCategory2 => GoodsItem.BY_Type;

		public abstract ZString GoodsCustomsProcedureCategory3 { get; }

		public abstract ZString GoodsCustomsProcedureCategory4 { get; }

		public abstract ZString GoodsCustomsProcedureCategory5 { get; }

		public ZString GoodsDescription => GoodsItem.BY_Description;

		public IReadOnlyCollection<ZString> NotSubmittedC44Documents => notSubmittedC44Documents ?? (notSubmittedC44Documents = GetNotSubmittedC44Documents());
		IReadOnlyCollection<ZString> notSubmittedC44Documents;

		protected abstract List<ZString> GetNotSubmittedC44Documents();

		public ZDecimal GrossWeightInKG
		{
			get
			{
				var result = GoodsItem.GrossMassInKilograms;
				return result > 0 && result <= 1 ? 1 : Math.Ceiling(result);
			}
		}

		public ZDecimal NetWeightInKG => GoodsItem.NetMassInKilograms;

		public IExternalPackagesInfoCommon ExternalPackages => externalPackages ?? (externalPackages = GetExternalPackagesInfoCommonWrapper());
		ExternalPackagesInfoCommonWrapper externalPackages;

		protected abstract ExternalPackagesInfoCommonWrapper GetExternalPackagesInfoCommonWrapper();

		public IInternalPackagesInfoCommon InternalPackages => internalPackages ?? (internalPackages = GetNCTSInternalPackagesInfoCommonWrapper());
		IInternalPackagesInfoCommon internalPackages;

		protected abstract IInternalPackagesInfoCommon GetNCTSInternalPackagesInfoCommonWrapper();

		public ZDecimal TotalGoodValueInEuros => GoodsItem.BY_MonetaryValue;

		public IReadOnlyCollection<IDocumentsCommon> Documents => documents ?? (documents = GetDocuments());
		IReadOnlyCollection<DocumentCommonWrapper> documents;

		protected abstract List<DocumentCommonWrapper> GetDocuments();
	}
}
