using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class EALAESTransportEquipmentWrapper : AESCommonTransportEquipmentWrapper
	{
		EALAESTransportEquipmentWrapper(ZShort seqNum, ZString containerNum, ZString sealAmount, IEnumerable<(ZShort seqNum, ZString sealNumber)> seals, List<ZShort> goodsReference)
			: base(seqNum, containerNum, sealAmount, new List<ZString>(), goodsReference)
		{
			sealsListEAL = seals;
		}
		readonly IEnumerable<(ZShort seqNum, ZString sealNumber)> sealsListEAL;

		protected override IReadOnlyCollection<ISealCommon> SealsCore
		{
			get
			{
				if (seals == null)
				{
					var sealsList = new List<SealCommonWrapper>();

					sealsListEAL.ForEach(l => sealsList.Add(new SealCommonWrapper(l.seqNum, l.sealNumber)));

					seals = sealsList.AsReadOnly();
				}
				return seals;
			}
		}
		IReadOnlyCollection<SealCommonWrapper> seals;

		public static IReadOnlyCollection<EALAESTransportEquipmentWrapper> GetTransportEquipmentList(CusExitReport exitReport)
		{
			var transportEquipmentList = new List<EALAESTransportEquipmentWrapper>();

			var header = exitReport.Header;

			if (header != null)
			{
				var containersToSend = header.CusExitContainers.Where(c => c.CXN_Status == EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared
																		|| c.CXN_Status == EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing
																		|| !c.CXN_SealCount.IsEmpty
																		|| c.AllSealNumbers.Any(s => ((CusExitSeal)s).BK_UnloadingState == EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared
																									|| ((CusExitSeal)s).BK_UnloadingState == EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing));

				if (exitReport.CER_Calc_Discrepancies && containersToSend.Any())
				{
					foreach (var container in containersToSend)
					{
						var containerNum = container.CXN_Status == EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared && !container.CXN_IsEquipment ? container.CXN_ContainerNumber : ZString.Empty;
						var sealAmount = container.CXN_Status != EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing && !container.CXN_SealCount.IsEmpty ? (ZString)container.CXN_SealCount.ToString() : ZString.Empty;

						var sealsList = container.CXN_Status == EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing
											? new List<(ZShort seqNum, ZString sealNumber)>()
											: container.AllSealNumbers.Where(s => s.BK_UnloadingState == EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared
																			|| s.BK_UnloadingState == EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing)
																.Select(s => (s.BK_SequenceNumber, s.BK_UnloadingState == EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared ? s.BK_SealNumber : ZString.Empty));

						var goodsReferenceList = GetGoodsReferenceList(header, (CusExitContainer)container);

						transportEquipmentList.Add(new EALAESTransportEquipmentWrapper(container.CXN_Sequence, containerNum, sealAmount, sealsList, goodsReferenceList));
					}
				}
			}
			return transportEquipmentList.AsReadOnly();
		}

		static List<ZShort> GetGoodsReferenceList(CusExitHeader header, CusExitContainer container)
		{
			var result = new List<ZShort>();

			foreach (var consignment in header.CusExitConsignments)
			{
				if (container.CXN_Status == EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared)
				{
					var consignmentItemPKs = consignment.CusExitConsignmentItems.Where(x => x.StatusIsDifferencesToDeclared).Select(x => x.PK).ToList();

					result = container.CusExitConsignmentPivots
								.Where(p => consignmentItemPKs.Contains(p.CNP_CCI_ConsignmentItem))
								.Select(p => p.ConsignmentItem.CCI_LineNumber).OrderBy(x => x).ToList();
				}
			}

			return result;
		}
	}
}
