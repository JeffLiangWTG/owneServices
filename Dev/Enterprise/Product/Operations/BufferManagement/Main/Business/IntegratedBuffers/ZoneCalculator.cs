using System;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;

namespace Enterprise.BufferManagement.Business
{
	public static class ZoneCalculator
	{
		public static int CalculateZone(decimal bufferPenetration)
		{
			var elapsedZones = (int)(bufferPenetration / (1 / (decimal)BMConstants.NumberOfZones));
			var proposedZone = BMConstants.NumberOfZones - elapsedZones;

			return Math.Min(BMConstants.NumberOfZones, Math.Max(0, proposedZone));
		}

		public static int CalculateZone(IBufferedItem buffered, IBuffer buffer, WorkingTimeContext context, BusinessObjectFactory factory, bool useCachedPenetration = true)
		{
			var bufferPenetration = useCachedPenetration
				? BufferPenetrationCalculator.CalculatePenetrationPercentage(buffered, buffer, context, factory)
				: BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(buffered, buffer, context, factory);

			return CalculateZone(bufferPenetration.Penetration);
		}

		public static int CalculateZone(IBufferedItem buffered, WorkingTimeContext context, BusinessObjectFactory factory, bool useCachedPenetration = true)
		{
			var bufferPenetration = useCachedPenetration
				? BufferPenetrationCalculator.CalculatePenetrationPercentage(buffered, context, factory).Penetration
				: BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(buffered, context, factory).Penetration;

			return CalculateZone(bufferPenetration);
		}
	}
}
