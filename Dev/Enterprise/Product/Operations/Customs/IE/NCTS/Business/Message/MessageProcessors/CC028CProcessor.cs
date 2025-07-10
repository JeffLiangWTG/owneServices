using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC028CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC028CProvider>
	{
		public CC028CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("19628491-DA7C-409D-9E86-1DD45A3A840A", "CC028C: MRN ALLOCATED");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC028CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC028CProvider provider) => NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;

		protected override Type MessageInterpreterType => typeof(CC028CMessageInterpreter);

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, CC028CProvider provider)
		{
			if (messageAttachee is NctsDepartureMovementHeader movementHeader)
			{
				var header = movementHeader.Header;
				if (header?.MovementReferenceEntryNumber is CusEntryNumber entryNumber)
				{
					entryNumber.CE_EntryNum = provider.MRN;
				}

				movementHeader.BM_EntryDate = provider.DeclarationAcceptanceDate;

				if (movementHeader.IsSimplifiedNctsProcedure)
				{
					CreateCustomsRegistryNumber(header, movementHeader);
				}
			}
		}

		void CreateCustomsRegistryNumber(NctsHeader nctsHeader, NctsDepartureMovementHeader movementHeader)
		{
			ZString organisationCode = ZString.Empty;

			if (movementHeader.Representative.Organisation is OrgHeader representative)
			{
				organisationCode = representative.OH_Code;
			}
			else if (nctsHeader.Principal.Organisation is OrgHeader principal)
			{
				organisationCode = principal.OH_Code;
			}

			if (!organisationCode.IsEmpty)
			{
				var regNumber = CusEntryNumber.New(nctsHeader, CusEntryNumberTypes.EU.CustomsRegistry, Core.Constants.CountryCodes.Ireland);
				regNumber.CE_EntryLineReference = TransitDeparture + "-" + organisationCode;
				regNumber.CE_EntryNum = nctsHeader.LocalReferenceNumber;
				regNumber.CE_IssueDate = ZDateTime.Now;
			}
		}

		const string TransitDeparture = "TD";
	}
}
