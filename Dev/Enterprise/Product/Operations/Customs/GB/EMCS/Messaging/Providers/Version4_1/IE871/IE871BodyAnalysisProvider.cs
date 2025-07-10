using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie871;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE871BodyAnalysisProvider : IIE871BodyAnalysis
	{
		public IE871BodyAnalysisProvider(BodyAnalysisType line)
		{
			this.line = Argument.NotNull(line, nameof(line));
		}
		readonly BodyAnalysisType line;

		public ZDecimal ActualQuantity => line.ActualQuantity ?? ZDecimal.Zero;

		public ZString Explanation => line.Explanation.Value;

		public ZString LineNumber => line.BodyRecordUniqueReference;

		public ZString ExciseProductCode => line.ExciseProductCode;
	}
}
