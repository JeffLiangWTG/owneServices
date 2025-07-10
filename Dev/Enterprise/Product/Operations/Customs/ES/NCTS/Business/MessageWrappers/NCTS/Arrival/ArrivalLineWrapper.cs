using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalLineWrapper : BaseArrivalLineWrapper<NctsArrivalAndUnloadingCargoDesc>
	{
		public ArrivalLineWrapper(NctsArrivalAndUnloadingCargoDesc goodsItem)
			: base(goodsItem)
		{
		}

		public override ZString GoodsCustomsProcedureCategory3
		{
			get
			{
				const string modifiedItem = "M";
				const string newItem = "N";
				const string notExistingItem = "B";

				var prefix = ZString.Empty;
				if (GoodsItem.HasDifferences)
				{
					prefix += modifiedItem;
				}

				if (GoodsItem.IsNew)
				{
					prefix += newItem;
				}

				if (GoodsItem.IsMissing)
				{
					prefix += notExistingItem;
				}

				var result = ZString.Empty;
				if (!prefix.IsEmpty)
				{
					result = prefix + GoodsItem.BY_LineNo;
				}

				return result;
			}
		}

		public override ZString GoodsCustomsProcedureCategory4 => GetCustomsProcedureCategory(0);

		public override ZString GoodsCustomsProcedureCategory5 => GetCustomsProcedureCategory(1);

		ZString GetCustomsProcedureCategory(ZInt position)
		{
			var result = ZString.Empty;
			var header = ((NctsHeader)GoodsItem.Header);
			if (!header.ESNctsHeader.CEN_PreviousSummaryDeclaration.IsEmpty)
			{
				var billOfLandingItem = GoodsItem.BillOfLadingItem;
				if (billOfLandingItem.Contains('/'))
				{
					result = billOfLandingItem.Split('/')[position];
				}
				else if (position == 0)
				{
					result = billOfLandingItem;
				}
			}
			return result;
		}

		protected override ExternalPackagesInfoCommonWrapper GetExternalPackagesInfoCommonWrapper() => new ExternalPackagesInfoCommonWrapper(GoodsItem.Containers.Select(x => x.ContainerNumber).ToArray());

		protected override IInternalPackagesInfoCommon GetNCTSInternalPackagesInfoCommonWrapper() => new NctsArrivalInternalPackagesInfoCommonWrapper(GoodsItem);

		protected override List<ZString> GetNotSubmittedC44Documents()
		{
			var notSubmittedC44Documents = new List<ZString>();
			if (GoodsItem.HasDifferences)
			{
				var arrivalGoodsItems = GoodsItem.Header.ArrivalMovementHeader.GoodsItems;
				if (arrivalGoodsItems.Count > 0 && GoodsItem.MoveHeader.BM_SubApplicationCode == Common.EU.NctsMoveHeaderType.Codes.Unloading)
				{
					var unloadedDocs = GoodsItem.SupportingDocuments;
					foreach (var item in arrivalGoodsItems.Where(line => line.BY_LineNo == GoodsItem.BY_LineNo))
					{
						notSubmittedC44Documents.AddRange(item.SupportingDocuments.Cast<NctsSupportingDocument>()
																					.Where(d => !unloadedDocs.Cast<NctsSupportingDocument>().Any(d2 => d2.KeyToDeterimeUniqueness == d.KeyToDeterimeUniqueness))
																						.Select(doc => (ZString)doc.CSI_LineNo.ToString()));
					}
				}
			}
			return notSubmittedC44Documents;
		}

		protected override List<DocumentCommonWrapper> GetDocuments()
		{
			var documents = new List<DocumentCommonWrapper>();
			if (GoodsItem.IsNew)
			{
				documents.AddRange(GoodsItem.SupportingDocuments.Cast<NctsSupportingDocument>()
																	.Select(doc => new DocumentCommonWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber)));
			}
			return documents;
		}
	}
}
