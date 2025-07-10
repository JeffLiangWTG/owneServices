using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonTransportEquipmentWrapper : INCTSCommonTransportEquipment
	{
		public NCTS5CommonTransportEquipmentWrapper(NctsContainer container, ZShort seqNum)
		{
			this.container = Argument.NotNull(container, nameof(container));
			this.seqNum = seqNum;
			isNctsContainer = true;
			isNctsArrivalContainer = false;
		}

		public NCTS5CommonTransportEquipmentWrapper(NctsArrivalHeaderContainer arrivalHeaderContainer)
		{
			this.arrivalHeaderContainer = Argument.NotNull(arrivalHeaderContainer, nameof(arrivalHeaderContainer));
			seqNum = arrivalHeaderContainer.BC_SequenceNumber;
			isNctsContainer = false;
			isNctsArrivalContainer = true;
		}

		public NCTS5CommonTransportEquipmentWrapper(NctsDepartureHeaderContainer departureHeaderContainer)
		{
			this.departureHeaderContainer = Argument.NotNull(departureHeaderContainer, nameof(departureHeaderContainer));
			seqNum = departureHeaderContainer.BC_SequenceNumber;
			isNctsContainer = false;
			isNctsArrivalContainer = false;
		}
		readonly NctsDepartureHeaderContainer departureHeaderContainer;
		readonly NctsArrivalHeaderContainer arrivalHeaderContainer;
		protected readonly NctsContainer container;
		readonly ZShort seqNum;
		readonly ZBool isNctsContainer;
		readonly ZBool isNctsArrivalContainer;

		public ZString SequenceNumber => seqNum.ToString();

		public ZString ContainerIdentificationNumber
		{
			get
			{
				if (isNctsContainer)
				{
					return IsContainerised(container.BC_Mode) ? container.BC_ContainerNum : ZString.Empty;
				}
				else if (isNctsArrivalContainer)
				{
					return IsContainerised(arrivalHeaderContainer.BC_Mode) && arrivalHeaderContainer.BC_UnloadedState.IsUnloadingStateNEWorDIF() ? arrivalHeaderContainer.BC_ContainerNum : ZString.Empty;
				}
				else
				{
					return IsContainerised(departureHeaderContainer.BC_Mode) ? departureHeaderContainer.BC_ContainerNum : ZString.Empty;
				}

				ZBool IsContainerised(ZString mode) => mode == Core.Constants.ContainerModes.Containerised;
			}
		}

		public ZString NumberOfSeals => isNctsContainer
											? container.TotalSealCount.ToString()
											: isNctsArrivalContainer
													? (arrivalHeaderContainer.BC_UnloadedState.IsUnloadingStateNEWorDIF()
																? arrivalHeaderContainer.TotalSealCount.ToString()
																: string.Empty)
													: departureHeaderContainer.TotalSealCount.ToString();

		public IReadOnlyCollection<ISealCommon> Seals
		{
			get
			{
				if (seals == null)
				{
					seals = isNctsContainer
										? GetSealsForContainer()
										: isNctsArrivalContainer
												? GetSealsForArrivalHeaderContainer()
												: GetSealsForDepartureHeaderContainer();
				}
				return seals;
			}
		}
		IReadOnlyCollection<SealCommonWrapper> seals;

		IReadOnlyCollection<SealCommonWrapper> GetSealsForContainer()
		{
			var sealsToReturn = new List<SealCommonWrapper>();

			var sealList = new List<ZString>();
			var additionalSeals = new List<ZString>();

			sealList = GetSealList(container.BC_Seal1, container.BC_Seal2);
			additionalSeals = GetSealList(container.SealsForMessaging.Select(p => p.BK_SealNumber).ToArray());
			additionalSeals.Sort();

			sealList.AddRange(additionalSeals);
			var sealListDistinct = sealList.Distinct().ToList();

			ZShort seqNum = 1;
			foreach (var seal in sealListDistinct)
			{
				sealsToReturn.Add(new SealCommonWrapper(seqNum, seal));
				seqNum++;
			}
			return sealsToReturn.AsReadOnly();
		}

		IReadOnlyCollection<SealCommonWrapper> GetSealsForArrivalHeaderContainer()
		{
			var sealsToReturn = new List<SealCommonWrapper>();

			if (!arrivalHeaderContainer.BC_UnloadedState.IsUnloadingStateMIS())
			{
				var sealList = new List<Tuple<ZShort, ZString>>();

				sealList.AddRange(arrivalHeaderContainer.Seals.Where(s => s.BK_UnloadingState.IsUnloadingStateNEWorMISorDIF())
																.Select(s => new Tuple<ZShort, ZString>(s.BK_SequenceNumber,
																										s.BK_UnloadingState.IsUnloadingStateNEWorDIF() ? s.BK_SealNumber : ZString.Empty))
																.ToList());

				var orderedSealsList = sealList.OrderBy(s => s.Item1);

				foreach (var (seqNum, seal) in orderedSealsList)
				{
					sealsToReturn.Add(new SealCommonWrapper(seqNum, seal));
				}
			}

			return sealsToReturn.AsReadOnly();
		}

		IReadOnlyCollection<SealCommonWrapper> GetSealsForDepartureHeaderContainer()
		{
			var sealsToReturn = new List<SealCommonWrapper>();

			var seqNumForSeal1 = (ZShort)1;
			var seqNumForSeal2 = (ZShort)2;

			var sealList = new List<Tuple<ZShort, ZString>>();

			AddFirstSeals(seqNumForSeal1, departureHeaderContainer.Seal1);
			AddFirstSeals(seqNumForSeal2, departureHeaderContainer.Seal2);
			sealList.AddRange(departureHeaderContainer.AdditionalSeals.Select(s => new Tuple<ZShort, ZString>(s.BK_SequenceNumber, s.BK_SealNumber)).ToList());

			var orderedSealsList = sealList.OrderBy(s => s.Item1);

			foreach (var (seqNum, seal) in orderedSealsList)
			{
				sealsToReturn.Add(new SealCommonWrapper(seqNum, seal));
			}
			return sealsToReturn.AsReadOnly();

			void AddFirstSeals(ZShort sequenceNumber, ZString sealCode)
			{
				if (!sealCode.IsEmpty)
				{
					sealList.Add(new Tuple<ZShort, ZString>(sequenceNumber, sealCode));
				}
			}
		}

		public IReadOnlyCollection<INCTSCommonGoodsReference> GoodsReference => GoodsReferenceCore;

		protected virtual IReadOnlyCollection<INCTSCommonGoodsReference> GoodsReferenceCore
		{
			get
			{
				if (goodsReference == null)
				{
					if (isNctsArrivalContainer)
					{
						goodsReference = GetGoodsReferenceForArrivalHeaderContainer();
					}
					else
					{
						goodsReference = GetGoodsReferenceForDepartureHeaderContainer();
					}
				}
				return goodsReference;
			}
		}
		IReadOnlyCollection<NCTS5CommonGoodsReferenceWrapper> goodsReference;

		IReadOnlyCollection<NCTS5CommonGoodsReferenceWrapper> GetGoodsReferenceForArrivalHeaderContainer()
		{
			var goodsReference = new List<NCTS5CommonGoodsReferenceWrapper>();

			if (arrivalHeaderContainer.BC_UnloadedState.IsUnloadingStateNEW())
			{
				var goodsReferenceList = new List<ZInt>();

				foreach (var bill in arrivalHeaderContainer.NctsArrival.Bills)
				{
					foreach (var goodsItem in bill.ArrivalGoodsItems)
					{
						if (goodsItem.Packages.Cast<NctsPackage>().Any(x => x.ContainersPivotsForBindingOnly.Cast<NonPersistentContainerPivotPhase5>().Any(y => y.Container.PK == arrivalHeaderContainer.PK && y.ContainerSelected)))
						{
							goodsReferenceList.Add(goodsItem.BY_DeclarationGoodsItemNumber);
						}
					}
				}

				ZShort seqNum = 1;
				foreach (var reference in goodsReferenceList)
				{
					goodsReference.Add(new NCTS5CommonGoodsReferenceWrapper(seqNum, reference));
					seqNum++;
				}
			}

			return goodsReference.AsReadOnly();
		}

		IReadOnlyCollection<NCTS5CommonGoodsReferenceWrapper> GetGoodsReferenceForDepartureHeaderContainer()
		{
			var goodsReference = new List<NCTS5CommonGoodsReferenceWrapper>();

			var containersInHeader = departureHeaderContainer.Header.DepartureHeaderContainers;
			var onlyOneCNTcontainerInHeader = containersInHeader.Count == 1 && containersInHeader[0].BC_Mode == Core.Constants.ContainerModes.Containerised;

			var onlyOneCNTcontainerInHeaderAndSelectedInAllGoodsItems = onlyOneCNTcontainerInHeader && departureHeaderContainer.Header.Bills.All(b => b.GoodsItems.All(i => i.Packages.Cast<NctsPackage>().Any(x => x.ContainersPivotsForBindingOnly.Cast<NonPersistentContainerPivotPhase5>().Any(y => y.ContainerSelected))));

			if (!onlyOneCNTcontainerInHeaderAndSelectedInAllGoodsItems)
			{
				var goodsReferenceList = new List<ZInt>();

				foreach (var bill in departureHeaderContainer.Header.Bills)
				{
					foreach (var goodsItem in bill.GoodsItems)
					{
						if (goodsItem.Packages.Cast<NctsPackage>().Any(x => x.ContainersPivotsForBindingOnly.Cast<NonPersistentContainerPivotPhase5>().Any(y => y.Container.PK == departureHeaderContainer.PK && y.ContainerSelected)))
						{
							goodsReferenceList.Add(goodsItem.BY_DeclarationGoodsItemNumber);
						}
					}
				}

				ZShort seqNum = 1;
				foreach (var reference in goodsReferenceList)
				{
					goodsReference.Add(new NCTS5CommonGoodsReferenceWrapper(seqNum, reference));
					seqNum++;
				}
			}

			return goodsReference.AsReadOnly();
		}

		protected List<ZString> GetSealList(params ZString[] seals)
		{
			var list = new List<ZString>();
			foreach (var seal in seals)
			{
				if (!seal.IsEmpty)
				{
					list.Add(seal);
				}
			}
			return list.Distinct().ToList();
		}
	}
}
