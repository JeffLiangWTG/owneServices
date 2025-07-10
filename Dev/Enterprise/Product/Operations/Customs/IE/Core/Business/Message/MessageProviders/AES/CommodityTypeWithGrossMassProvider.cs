using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	internal class CommodityTypeWithGrossMassProvider : CommodityTypeProvider, ICommodityTypeWithGrossMass
	{
		public CommodityTypeWithGrossMassProvider(EntryLineWrapper entryLineWrapper)
			: base(entryLineWrapper)
		{
		}

		public decimal GrossMass
		{
			get
			{
				if (!grossMass.HasValue)
				{
					grossMass = GetGrossMass();
				}
				return grossMass.Value;
			}
		}
		decimal? grossMass;

		ZDecimal GetGrossMass()
		{
			var result = ZDecimal.Zero;
			var packageType = entryLine.RandomMainPackLineOrRandomLine.OverallPackageType;
			if (packageType != PackageType.Packed)
			{
				result = entryLine.EffectiveGrossWeight.InKilogramsSafe;
			}
			else
			{
				var packageRelatedEntryLines = entryLineWrapper.EntryHeader.PackageRelatedEntryLines;
				if (packageRelatedEntryLines.MainPackEntryLines.TryGetValue(entryLine, out var allEntryLines))
				{
					result = allEntryLines.Sum(line => line.EffectiveGrossWeight.InKilogramsSafe);
				}
				else if (!packageRelatedEntryLines.EntryLnesRelatedToMainPack.Contains(entryLine))
				{
					result = entryLine.EffectiveGrossWeight.InKilogramsSafe;
				}
			}
			return result;
		}
	}
}
