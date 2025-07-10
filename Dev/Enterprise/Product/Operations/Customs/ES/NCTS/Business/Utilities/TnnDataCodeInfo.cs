using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class TnnDataCodeInfo : NonPersistentBusinessObject
	{
		TnnDataCodeInfo(NctsHeader header)
			: base(header.Factory)
		{
			this.header = header;
			Argument.NotNull(header, nameof(header));
		}
		public readonly NctsHeader header;

		public static TnnDataCodeInfo LoadNew(NctsHeader header)
		{
			Argument.NotNull(header, nameof(header));
			var tnnDataCodeInfo = new TnnDataCodeInfo(header);
			tnnDataCodeInfo.AcceptanceDate = header.AcceptanceDate;
			tnnDataCodeInfo.ClearanceDate = header.ClearanceDate;
			return tnnDataCodeInfo;
		}

		public ZDateTime AcceptanceDate { get; set; }
		public ZDateTime ClearanceDate { get; set; }
	}
}
