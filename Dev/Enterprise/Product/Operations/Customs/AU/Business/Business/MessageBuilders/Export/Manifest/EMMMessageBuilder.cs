using System;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EMMMessageBuilder : CMRManifestMessageBuilder, IManifestMessageBuilder
	{
		public EMMMessageBuilder(ExportCustomsManifestHeader header) : base(header) { }

		public EMMMessageBuilder(ForwardingConsol consol) : base(consol) { }

		public ZString ManifestMessageTypeCode => "EMM";

		protected override void PopulateRFF()
		{
			if (MessageSubType != Common.MessageBuilders.MessageSubTypes.Create)
			{
				ZString cAN = manifestHeaderWrapper.CAN;
				if (!cAN.IsEmpty)
				{
					MessageUtilities.PopulateRFF(CUSCAR.Group1[0].RFF[0], ReferenceFunctionCodeQualifierList.CargoManifestNumber, cAN, null);
				}
			}
		}

		protected override bool IsSlotSubManifest => false;

		protected override bool IsConsolidationSubManifest => false;

		protected override bool IncludeGoodsDescription => false;

		protected override bool IsMainManifest => true;

		protected override bool NILIndicatorIsAllowed => true;

		protected override bool IncludeConsolLOCDetails => true;

		protected override bool IncludeAdditionalTransportInfo => true;

		protected internal override ZString DocumentName => CMRMessage.CMRMessageTypes.EMM;

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.CustomsManifest;

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.EMM;

		protected internal override Type TypeOfMessage => typeof(CMREMMMessage);
	}
}
