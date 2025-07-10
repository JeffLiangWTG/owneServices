using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class EALAESLocationOfGoodsWrapper : IEALAESLocationOfGoods
	{
		public EALAESLocationOfGoodsWrapper(CusExitReport exitReport)
		{
			this.exitReport = Argument.NotNull(exitReport, nameof(exitReport));
			SequenceNumber = sequenceNumber;
			LocationType = locationType;
			LocationQualifier = locationQualifier;
		}
		readonly CusExitReport exitReport;

		const string sequenceNumber = "1";
		const string locationType = "B";
		const string locationQualifier = "Y";

		public ZString SequenceNumber { get; }

		public ZString LocationType { get; }

		public ZString LocationQualifier { get; }

		public ZString LocationId => IsLocationLonger10AndStartsWithES() ? exitReport.CER_Location.SubstringSafe(4) : exitReport.CER_Location.SubstringSafe(0, 10);

		ZBool IsLocationLonger10AndStartsWithES() => exitReport.CER_Location.Length > 10 && exitReport.CER_Location.StartsWith("ES");
	}
}
