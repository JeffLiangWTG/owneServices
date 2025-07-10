using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoCARSTBusinessObjectLoader : CARSTBusinessObjectLoaderOrCreator
	{
		protected internal override bool IsInterestedInCARST(CMRCARSTMessage message)
		{
			return message.IsSea && SeaCargoRecordExists(message);
		}

		protected internal override BusinessObject[] LoadOrCreateRecordForMessageCore(CMRCARSTMessage message)
		{
			var result = FindPivotOrHouse(message)
				?? FindBreakBulk(message);
			return new BusinessObject[] { result }; // Consider if you need to match more than one.
		}

		#region Implementation

		bool SeaCargoRecordExists(CMRCARSTMessage message)
		{
			return FindPivotOrHouse(message) != null || FindBreakBulk(message) != null;
		}

		BusinessObject FindPivotOrHouse(CMRCARSTMessage message)
		{
			BusinessObject matchHouseBillWithoutAPivotMatch = null;

			if (!message.HouseBillNumber.IsEmpty && !message.ContainerNumber.IsEmpty)
			{
				var filter = new ZQuery(CusSCAHouseSchema.CA_HouseBill, message.HouseBillNumber);
				filter.AddToFilter(CusSCAHouseSchema.CA_MessageStatus, SQLComparisonOperator.NotEqual, CMRBaseStatuses.Codes.NotSent);
				filter.AddToFilter(CusSCAHouseSchema.CA_MessageStatus, SQLComparisonOperator.IsNotBlank, ZString.Empty);

				var possibleMatches = message.Factory.Load<BaseCusSCAHouse>(filter)
					.OfType<CusSCAHouse>()
					.OrderByDescending(c => c.CA_SystemCreateTimeUtc);

				var senderReference = message.CARSTSendersReference;
				var isSenderReferenceMatchEnabled = !string.IsNullOrWhiteSpace(senderReference)
					&& AUCustomsDataRegistry.Instance.UseSenderReferenceToFilterSeaCargoReport.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

				if (isSenderReferenceMatchEnabled)
				{
					var houseBills = possibleMatches.Where(c => c.CA_BGMReference == senderReference);
					matchHouseBillWithoutAPivotMatch = FindPivotOrHouseCore(houseBills, message);
				}

				if (matchHouseBillWithoutAPivotMatch == null)
				{
					var houseBills = isSenderReferenceMatchEnabled ? possibleMatches.Where(c => c.CA_BGMReference != senderReference) : possibleMatches;
					matchHouseBillWithoutAPivotMatch = FindPivotOrHouseCore(houseBills, message);
				}
			}

			return matchHouseBillWithoutAPivotMatch;
		}

		BusinessObject FindPivotOrHouseCore(IEnumerable<CusSCAHouse> houseBills, CMRCARSTMessage message)
		{
			BusinessObject matchHouseBillWithoutAPivotMatch = null;

			foreach (var house in houseBills)
			{
				var oceanBill = house.OceanBill;

				if (oceanBill != null && oceanBill.CB_IsActive && oceanBill.CB_LloydsIMO == message.LloydsNumber && oceanBill.CB_Voyage.TrimStart(' ', '0').ToUpper() == message.VoyageNumber.TrimStart(' ', '0').ToUpper())
				{
					foreach (CusSCAPivot pivot in house.Pivot)
					{
						if (pivot.CN_ContainerNumber.EqualsIgnoringCase(message.ContainerNumber))
						{
							return pivot;
						}
					}

					if (matchHouseBillWithoutAPivotMatch == null && house.Pivot.Count == 0 && house.Messages.OfType<CMRCARSTMessage>().Any())
					{
						matchHouseBillWithoutAPivotMatch = house;
					}
				}
			}

			return matchHouseBillWithoutAPivotMatch;
		}

		BusinessObject FindBreakBulk(CMRCARSTMessage message)
		{
			if (!message.OceanBillNumber.IsEmpty)
			{
				var filter = new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, message.OceanBillNumber);
				filter.AddToFilter(CusSCAOceanBillSchema.CB_LloydsIMO, message.LloydsNumber);
				filter.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, CusSCAOceanBill.ApplicationCodes);
				filter.AddToFilter(CusSCAOceanBillSchema.CB_IsActive, true);

				var possibleMatches = message.Factory.Load<CusSCAOceanBill>(filter)
					.OrderByDescending(c => c.CB_SystemCreateTimeUtc);

				foreach (CusSCAOceanBill oceanBill in possibleMatches)
				{
					if (oceanBill.CB_Voyage.TrimStart(' ', '0').ToUpper() == message.VoyageNumber.TrimStart(' ', '0').ToUpper())
					{
						foreach (CusSCAContainer containerLine in oceanBill.Containers)
						{
							if (containerLine.IsBreakBulk && containerLine.Pivots.Count > 0)
							{
								return containerLine.Pivots[0];
							}
						}
					}
				}
			}
			return null;
		}

		#endregion
	}
}
