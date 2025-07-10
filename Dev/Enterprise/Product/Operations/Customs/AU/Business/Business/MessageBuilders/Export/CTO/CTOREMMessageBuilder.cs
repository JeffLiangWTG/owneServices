using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOREMMessageBuilder : CTOMessageBuilder
	{
		public CTOREMMessageBuilder(ExportCustomsManifestLines line)
			: this(new ExportCustomsManifestLinesWrapper(line))
		{
		}

		public CTOREMMessageBuilder(ICTOMessageLine line)
			: base(line)
		{
		}

		#region Implementation

		protected override void PopulateSegmentGroup8(SegmentGroup8 group8, ICTOMessageLine cTOItem, int lineNumber)
		{
			MessageUtilities.PopulateGID(group8.Group14[0].GID[0], "1");
		}

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.CTOREM;

		protected internal override ZString DocumentName => "CTOREM";

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.TransportMovementGateOutReport;

		protected internal override Type TypeOfMessage => typeof(CMRCTOREMMessage);

		#endregion
	}
}
