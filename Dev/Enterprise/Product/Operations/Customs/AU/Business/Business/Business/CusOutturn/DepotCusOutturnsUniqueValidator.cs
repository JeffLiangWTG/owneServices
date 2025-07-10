using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class DepotCusOutturnsUniqueValidator
	{
		public DepotCusOutturnsUniqueValidator(HashSet<DepotCusOutturn> outturns)
		{
			this.outturns = Argument.NotNull(outturns, nameof(outturns));
			repeatedElements = new ConcurrentDictionary<ZGuid, ZGuid>();
		}

		readonly ConcurrentDictionary<ZGuid, ZGuid> repeatedElements;
		readonly HashSet<DepotCusOutturn> outturns;

		public void BuildRepeatedElements()
		{
			repeatedElements.Clear();

			if (outturns.Count > 1)
			{
				BuildIndexGroups();
			}
		}

		public bool HasError(ZGuid pk)
		{
			return repeatedElements.ContainsKey(pk);
		}

		void BuildIndexGroups()
		{
			var groups = outturns.Cast<DepotCusOutturn>()
				.Where(c => c != null && !c.IsDeleted && c.C5_MessageStatus != CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived)
				.Select(c => new DepotCusOutturnIndexer(c))
				.GroupBy(c => c.UniqueIndex);

			foreach (var group in groups)
			{
				var count = group.Count();

				if (count > 1)
				{
					BuildRepeatedElements(group, count);
				}
			}
		}

		void BuildRepeatedElements(IGrouping<ZString, DepotCusOutturnIndexer> group, int count)
		{
			void BuildRepeatedElementsCore(DepotCusOutturnIndexer indexer)
			{
				var shouldAddError = true;

				if (indexer.CargoType == Core.Constants.ContainerModes.FCLMixedShipper)
				{
					shouldAddError = group.Any(x => x.PK != indexer.PK && x.CargoType == Core.Constants.ContainerModes.FCLMixedShipper);
				}
				else if (group.Any(x => x.PK != indexer.PK
								&& x.CargoType == Core.Constants.ContainerModes.FCLMixedShipper
								&& x.ContainerNumber == indexer.ContainerNumber
								&& x.HouseBill.IsEmpty
								&& x.MasterBill.IsEmpty))
				{
					shouldAddError = count > 2;
				}

				if (shouldAddError)
				{
					repeatedElements.GetOrAdd(indexer.PK, indexer.PK);
				}
			}

			Parallel.ForEach(group, BuildRepeatedElementsCore);
		}
	}

	sealed class DepotCusOutturnIndexer
	{
		const string split = "-";

		public DepotCusOutturnIndexer(DepotCusOutturn outturn)
		{
			PK = outturn.PK;
			HouseBill = outturn.C5_HouseBill;
			MasterBill = outturn.C5_MasterBill;
			CargoType = outturn.C5_CargoType;
			ContainerNumber = outturn.C5_ContainerNumber;
			MessageStatus = outturn.C5_MessageStatus;

			var keys = new[]
			{
				!HouseBill.IsEmpty || !MasterBill.IsEmpty ? CargoType : ZString.Empty,
				ContainerNumber,
				HouseBill,
				MasterBill
			};

			UniqueIndex = ZString.Join(split, keys);
		}

		public ZGuid PK { get; }

		public ZString HouseBill { get; }

		public ZString MasterBill { get; }

		public ZString CargoType { get; }

		public ZString ContainerNumber { get; }

		public ZString MessageStatus { get; }

		public ZString UniqueIndex { get; }
	}
}
