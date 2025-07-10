using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	class ZoneMultiplierProvider
	{
		internal ReservedAllocatedCapacityPair AccumulateCapacities(ReservedAllocatedCapacityPair accumulated, WorkflowDTO workflowDTO, BMComponent buffer, WorkingTimeContext context, TaskDTO taskDTO = null)
		{
			if (context == null)
			{
				context = WorkingTimeContext.Create(buffer);
			}

			var bufferPenetration = GetBufferPenetration(workflowDTO, buffer, context);
			var zoneId = ZoneCalculator.CalculateZone(bufferPenetration);
			var multiplier = GetZoneMultiplier(workflowDTO, buffer, zoneId);
			var estimate = GetEstimate(workflowDTO, taskDTO);

			if (zoneId >= BMConstants.NumberOfZonesIncludingZoneZero)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unable to find a zone for a workflow. Zone passed was: {0}", zoneId));
			}

			AddToDictionaryOrAccumulate(accumulated.AllocatedCapacity, zoneId, estimate);
			AddToDictionaryOrAccumulate(accumulated.ReservedCapcity, zoneId, GetReservedCapacity(estimate, multiplier));

			return accumulated;
		}

		internal decimal GetZoneMultipliedEstimate(WorkflowDTO workflowDTO, BMComponent buffer, WorkingTimeContext context = null, TaskDTO taskDTO = null)
		{
			var zoneID = ZoneCalculator.CalculateZone(GetBufferPenetration(workflowDTO, buffer, context));
			var multiplier = GetZoneMultiplier(workflowDTO, buffer, zoneID);
			var estimate = GetEstimate(workflowDTO, taskDTO);

			return GetReservedCapacity(estimate, multiplier);
		}

		#region Implementation

		static void AddToDictionaryOrAccumulate(Dictionary<int, decimal> capacity, int key, decimal value)
		{
			if (capacity.ContainsKey(key))
			{
				capacity[key] += value;
			}
			else
			{
				capacity.Add(key, value);
			}
		}

		static decimal GetEstimate(WorkflowDTO workflow, TaskDTO task)
		{
			return ((IEstimatable)task ?? workflow).EstimateHours;
		}

		static decimal GetReservedCapacity(decimal estimate, decimal multiplier)
		{
			return estimate * multiplier;
		}

		decimal GetBufferPenetration(WorkflowDTO workflowDTO, BMComponent buffer, WorkingTimeContext context)
		{
			if (workflowDTO.ShapeForZoneCalculation.IsValid)
			{
				var shape = buffer.Factory.Load<IBMNCNShape>(workflowDTO.ShapeForZoneCalculation);
				if (shape.PenetratingBufferSizeInMinutes != 0)
				{
					return shape.BufferPenetration;
				}
			}

			var bufferedItem = (IBufferedItem)workflowDTO;

			var cacheKey = workflowDTO.PK;
			var bufferPenetration = bufferPenetrationCache.ContainsKey(cacheKey)
				? bufferPenetrationCache[cacheKey]
				: bufferPenetrationCache[cacheKey] = BufferPenetrationCalculator.CalculatePenetrationPercentage(bufferedItem, context, buffer.Factory).Penetration;

			return bufferPenetration;
		}

		ZDecimal GetZoneMultiplier(WorkflowDTO workflowDTO, BMComponent buffer, int zone)
		{
			var key = zone + ":" + workflowDTO.ReleaseGroup.ToString() + buffer.PK.ToString();
			return zoneMultiplierCache.ContainsKey(key)
				? zoneMultiplierCache[key]
				: zoneMultiplierCache[key] = BMZoneCapacityMultiplier.GetZoneMultiplier(buffer.Factory, buffer.PK, workflowDTO.ReleaseGroup, zone);
		}

		readonly Dictionary<ZGuid, decimal> bufferPenetrationCache = new Dictionary<ZGuid, decimal>();
		readonly Dictionary<string, ZDecimal> zoneMultiplierCache = new Dictionary<string, ZDecimal>();

		#endregion
	}
}
