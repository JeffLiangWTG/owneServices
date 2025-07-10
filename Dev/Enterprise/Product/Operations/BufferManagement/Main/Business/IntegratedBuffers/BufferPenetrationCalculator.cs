using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using CommonBufferPenetrationCalculator = CargoWise.PAVE.Common.Implementation.BufferPenetrationCalculator;

namespace Enterprise.BufferManagement.Business
{
	public static class BufferPenetrationCalculator
	{
		public static BufferPenetrationResult CalculatePenetrationPercentage(IBuffer buffer, WorkingTimeContext context, BusinessObjectFactory factory, bool includeNetworkBuffers = true)
		{
			Argument.NotNull(buffer, "buffer");

			var workingTimeContext = new WorkingTimeContextWithFactory(context, factory);
			var result = GetCache(factory).GetOrAdd(buffer, () => CalculateNonCachedPenetrationPercentage(buffer, workingTimeContext, includeNetworkBuffers));

			var bizO = result.PenetratingBuffer as BusinessObject;
			if (bizO != null && bizO.IsDeleted)
			{
				GetCache(factory).Remove(buffer);
				result = GetCache(factory)
					.GetOrAdd(buffer, () => CalculateNonCachedPenetrationPercentage(buffer, workingTimeContext, includeNetworkBuffers));
			}

			return result;
		}

		public static BufferPenetrationResult CalculatePenetrationPercentage(IBufferedItem buffered, WorkingTimeContext context, BusinessObjectFactory factory, bool includeNetworkBuffers = true)
		{
			Argument.NotNull(buffered, "buffered");

			var workingTimeContext = new WorkingTimeContextWithFactory(context, factory);
			var result = GetCache(factory).GetOrAdd(buffered, () => CalculateNonCachedPenetrationPercentage(buffered, workingTimeContext, includeNetworkBuffers));

			var bizO = result.PenetratingBuffer as BusinessObject;
			if (bizO != null && bizO.IsDeleted)
			{
				GetCache(factory).Remove(buffered);
				result = GetCache(factory)
					.GetOrAdd(buffered, () => CalculateNonCachedPenetrationPercentage(buffered, workingTimeContext, includeNetworkBuffers));
			}

			return result;
		}

		public static BufferPenetrationResult CalculatePenetrationPercentage(IBufferedItem buffered, IBuffer buffer, WorkingTimeContext context, BusinessObjectFactory factory, bool includeNetworkBuffers = true)
		{
			Argument.NotNull(buffer, "buffer");
			Argument.NotNull(buffered, "buffered");

			var workingTimeContext = new WorkingTimeContextWithFactory(context, factory);
			var key = Tuple.Create(buffered, buffer);

			var result = GetCache(factory).GetOrAdd(key, () => CalculateNonCachedPenetrationPercentage(buffered, buffer, workingTimeContext, includeNetworkBuffers));

			var bizO = result.PenetratingBuffer as BusinessObject;
			if (bizO != null && bizO.IsDeleted)
			{
				GetCache(factory).Remove(key);
				result = GetCache(factory)
					.GetOrAdd(key, () => CalculateNonCachedPenetrationPercentage(buffered, buffer, workingTimeContext, includeNetworkBuffers));
			}

			return result;
		}

		public static BufferPenetrationResult CalculateNonCachedPenetrationPercentage(IBufferedItem buffered, IBuffer buffer, WorkingTimeContext context, BusinessObjectFactory factory, bool includeNetworkBuffers = true)
		{
			var workingTimeContext = new WorkingTimeContextWithFactory(context, factory);

			return CalculateNonCachedPenetrationPercentage(buffered, buffer, workingTimeContext, includeNetworkBuffers);
		}

		public static BufferPenetrationResult CalculateNonCachedPenetrationPercentage(IBufferedItem buffered, WorkingTimeContext context, BusinessObjectFactory factory, bool includeNetworkBuffers = true)
		{
			var workingTimeContext = new WorkingTimeContextWithFactory(context, factory);

			return CalculateNonCachedPenetrationPercentage(buffered, workingTimeContext, includeNetworkBuffers);
		}

		public static BufferPenetrationResult CalculateNonCachedPenetrationPercentage(IBuffer buffer, WorkingTimeContext context, BusinessObjectFactory factory, bool includeNetworkBuffers = true)
		{
			var workingTimeContext = new WorkingTimeContextWithFactory(context, factory);

			return CalculateNonCachedPenetrationPercentage(buffer, workingTimeContext, includeNetworkBuffers);
		}

		static BufferPenetrationResult CalculateNonCachedPenetrationPercentage(IBufferedItem buffered, IBuffer buffer, IWorkingTimeContext workingTimeContext, bool includeNetworkBuffers)
		{
			var nonCachedCalculator = new CommonBufferPenetrationCalculator();

			return nonCachedCalculator.CalculatePenetrationPercentage(buffered, buffer, workingTimeContext, ZDateTime.UtcNow.ToDateTime(), includeNetworkBuffers);
		}

		static BufferPenetrationResult CalculateNonCachedPenetrationPercentage(IBufferedItem buffered, IWorkingTimeContext workingTimeContext, bool includeNetworkBuffers)
		{
			var nonCachedCalculator = new CommonBufferPenetrationCalculator();

			return nonCachedCalculator.CalculatePenetrationPercentage(buffered, workingTimeContext, ZDateTime.UtcNow.ToDateTime(), includeNetworkBuffers);
		}

		static BufferPenetrationResult CalculateNonCachedPenetrationPercentage(IBuffer buffer, IWorkingTimeContext workingTimeContext, bool includeNetworkBuffers)
		{
			var nonCachedCalculator = new CommonBufferPenetrationCalculator();

			return nonCachedCalculator.CalculatePenetrationPercentage(buffer, workingTimeContext, ZDateTime.UtcNow.ToDateTime(), includeNetworkBuffers);
		}

		static BufferPenetrationCache GetCache(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<BufferPenetrationCache>(CacheStalenessPolicy.StaleOnFactorySave);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public static int GetAgeInMinutes(IBufferedItem bufferedItem, IWorkingTimeContext workingTimeContext, DateTime? utcNow = null)
		{
			return CommonBufferPenetrationCalculator.GetAgeInMinutes(bufferedItem, workingTimeContext, utcNow);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Need a named class since there could be other cached Dictionary<`2> instances in BusinessObjectFactory")]
		public class BufferPenetrationCache : Dictionary<object, BufferPenetrationResult>
		{
		}
	}
}
