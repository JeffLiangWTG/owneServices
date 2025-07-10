using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public interface ISequenceNumberHeaderWithFlagToRecalculate : ISequenceNumberHeader
{
	bool ShouldRecalculateLineNos { get; }
}

public class TemporaryStoragePackedItemSequenceNumberGenerator : HugeSequenceNumberGenerator
{
	public TemporaryStoragePackedItemSequenceNumberGenerator(ISequenceNumberHeaderWithFlagToRecalculate header) : base(header)
	{
		this.header = header;
	}

	readonly ISequenceNumberHeaderWithFlagToRecalculate header;

	int GetNextSequenceNumber()
	{
		var lines = Lines.ToArray();
		var nextSequenceNumber = (lines.Any() ? lines.Max(x => x.SequenceNumber) : ZInt.Zero) + 1;
		return EnsureValidSequenceNumber(nextSequenceNumber);
	}

	protected override void RecalculateWhenAddedCore(IHugeSequenceNumberLine lineToAdd)
	{
		if (header.ShouldRecalculateLineNos)
		{
			base.RecalculateWhenAddedCore(lineToAdd);
		}
		else
		{
			using (GetLineNumberSuspender())
			{
				lineToAdd.SequenceNumber = GetNextSequenceNumber();
			}
		}
	}

	protected override void RecalculateWhenRenumberedCore(IHugeSequenceNumberLine lineBeingRenumbered, ZInt oldValue)
	{
		if (header.ShouldRecalculateLineNos)
		{
			base.RecalculateWhenRenumberedCore(lineBeingRenumbered, oldValue);
		}
	}

	protected override void RecalculateWhenAboutToBeDetachedOrDeletedCore(IHugeSequenceNumberLine lineToBeDeleted)
	{
		if (header.ShouldRecalculateLineNos)
		{
			base.RecalculateWhenAboutToBeDetachedOrDeletedCore(lineToBeDeleted);
		}
	}

	protected override void ReCalculateAllCore()
	{
		if (header.ShouldRecalculateLineNos)
		{
			base.ReCalculateAllCore();
		}
	}
}
