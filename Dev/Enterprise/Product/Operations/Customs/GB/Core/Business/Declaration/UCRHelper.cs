using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public static class UCRHelper
	{
		public static ucrBlock ComposeUcrBlock(ZString ucr, ZString ucrPartNo, ucrType ucrType)
		{
			if (ucrType == ucrType.D && !ucrPartNo.IsEmpty)
			{
				return new ucrBlock { ucr = ucr, ucrPartNo = ucrPartNo, ucrType = ucrType };
			}
			else
			{
				return new ucrBlock { ucr = ucr, ucrType = ucrType };
			}
		}

		public static ZString UcrBlockToString(ucrBlock block)
		{
			return UcrBlockToString(block.ucr, block.ucrPartNo, block.ucrType.ToString());
		}

		public static ZString UcrBlockToString(ZString ucr, ZString ucrPartNo, ZString ucrType)
		{
			return ucrType == nameof(CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking.ucrType.D) && !ucrPartNo.IsEmpty ? (ZString)$"{ucr}/{ucrPartNo}" : ucr;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("WTG.Analyzers", "WTG1013:AvoidTupleTypesInPublicInterfaces")]
		public static IEnumerable<(ZString key, ZString value)> GetUCRKeyValuePairs(ucrBlock block)
		{
			var dict = new Dictionary<ZString, ZString>();
			dict.Add("UCR", block.ucr);
			if (block.ucrPartNoSpecified)
			{
				dict.Add("UCR Part No", block.ucrPartNo);
			}
			dict.Add("UCR Type", block.ucrType.ToString());
			return dict.SelectMany(d => new (ZString key, ZString value)[] { (d.Key, d.Value) });
		}
	}
}
