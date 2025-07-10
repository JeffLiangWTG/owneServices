using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[SingleObjectAroundARow()]
	[CodeProperty(ExportCustomsManifestHeader.Schema.ED_BGMReference)]
	public class ExportCustomsManifestHeader :
		Customs.Business.ExportCustomsManifestHeader,
		IDocManagerSupport,
		ICMRMessageRespondee,
		Integration.Customs.AU.IExportCustomsManifestHeader
	{
		public ExportCustomsManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region TypeDecider

		public static readonly TypeDecider TypeDecider = new ExportCustomsManifestHeaderTypeDecider();

		#endregion

		#region Schema

		public new class Schema : Customs.Business.ExportCustomsManifestHeader.Schema
		{
			public const string DepartureStatus = "DepartureStatus";
			public const string ManifestStatus = "ManifestStatus";
		}

		#endregion

		#region Constants

		public static class Constants
		{
			public const string WaitingForResponse = "Waiting for response";
		}

		#endregion

		#region Load / Create

		public ExportCustomsManifestHeader[] LoadFromVesselVoyageDepartureDestination()
		{
			ZQuery query = new ZQuery(ExportCustomsManifestHeaderSchema.ED_VesselName, this.ED_VesselName);
			query.AddToFilter(ExportCustomsManifestHeaderSchema.ED_LloydsIMO, this.ED_LloydsIMO);
			query.AddToFilter(ExportCustomsManifestHeaderSchema.ED_VoyageNumber, this.ED_VoyageNumber);
			query.AddToFilter(ExportCustomsManifestHeaderSchema.ED_RL_NKPortOfDeparture, this.ED_RL_NKPortOfDeparture);
			query.AddToFilter(ExportCustomsManifestHeaderSchema.ED_RN_NKCountryOfDestination, this.ED_RN_NKCountryOfDestination);
			query.AddToFilter(ExportCustomsManifestHeaderSchema.PK, SQLComparisonOperator.NotEqual, PK);
			query.AddToFilter(ExportCustomsManifestHeaderSchema.ED_ManifestType, this.ED_ManifestType);
			return Factory.Load<ExportCustomsManifestHeader>(query);
		}

		public static ExportCustomsManifestHeader Load(BusinessObjectFactory factory, ZString bGMReference)
		{
			if (!bGMReference.IsEmpty)
			{
				ZQuery filter = new ZQuery(ExportCustomsManifestHeaderSchema.ED_BGMReference, bGMReference);
				return factory.LoadTop1<ExportCustomsManifestHeader>(filter);
			}
			return null;
		}

		public void LoadOrCreateLineWithCANIntoCurrentManifestLine(ZString cAN)
		{
			foreach (ExportCustomsManifestLines line in Lines)
			{
				if (line.IsCANLine && line.EL_CAN == cAN)
				{
					fCurrentManifestLine = line;
					return;
				}
			}
			fCurrentManifestLine = Lines.AddNew();
			fCurrentManifestLine.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
			fCurrentManifestLine.EL_CAN = cAN;
		}

		public void CreateExemptLineAndSetCurrentManifestLine(ZString exemption)
		{
			fCurrentManifestLine = Lines.AddNew();
			fCurrentManifestLine.EL_TypeOfCAN = exemption;
		}

		internal void CreateEmptyCANLineAndSetCurrentManifestLine()
		{
			fCurrentManifestLine = Lines.AddNew();
			fCurrentManifestLine.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
		}

		public void CreateLinesFromJobDeclarations(IEnumerable<JobDeclaration> declarations)
		{
			var skippedDecs = new List<string>();
			var existingCANsHashList = Lines.Cast<ExportCustomsManifestLines>().Select(x => x.EL_CAN).Distinct().ToDictionary(x => x, null);

			foreach (var dec in declarations)
			{
				var canNumber = dec.DeclarationNumber.Left(ExportCustomsManifestLines.Schema.EL_CANMaxLength);
				if (canNumber.IsEmpty || !existingCANsHashList.ContainsKey(canNumber))
				{
					((Integration.Customs.IJobDeclarationWithShipmentSynchonisation)dec).SynchroniseWithShipmentIfNeeded();

					var line = Lines.AddNew();
					line.EL_CAN = canNumber;
					line.EL_TypeOfCAN = canNumber.IsEmpty ? CANType.Exemptions.EXLV.Code : CANType.CustomsAuthorityNumber.Code;

					line.EL_AirWayBill = dec.JE_HouseBill;
					line.EL_GoodsDescription = dec.JE_GoodsDescription;
					line.EL_RN_NKCountryOfDestination = dec.JE_RL_NKFinalDestination.Left(2);
					line.EL_Volume = dec.JE_TotalVolume;
					line.EL_VolumeUQ = dec.JE_TotalVolumeUnit;
					line.EL_Weight = dec.JE_TotalWeight;
					line.EL_WeightUQ = dec.JE_TotalWeightUnit;
					line.EL_NumberOfPackages = dec.JE_TotalNoOfPacks;
					line.EL_NumberOfContainers = dec.JE_ContainerCount;

					line.ConsignorDocumentaryAddress.E2_OA_Address = (dec.Supplier?.MainAddress.PK ?? ZGuid.Empty);
					line.ConsigneeDocumentaryAddress.E2_OA_Address = (dec.Importer?.MainAddress.PK ?? ZGuid.Empty);

					if (canNumber.IsEmpty)
					{
						line.EL_OH_Owner = dec.JE_OH_Supplier;
						line.EL_GoodsOwner = (dec.Supplier?.OH_FullName ?? ZString.Empty);
					}
					else
					{
						existingCANsHashList.Add(canNumber, null);
					}
				}
				else
				{
					skippedDecs.Add(ZString.Format("{0} ({1})", dec.JE_DeclarationReference, canNumber));
				}
			}

			if (skippedDecs.Any())
			{
				var reference = "Duplicate CANs prevented some Declarations being imported: ";
				reference += string.Join(", ", skippedDecs);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, reference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		#endregion

		#region Properties

		protected override ZString HumanReadableNameCore => Res.GetString("5B7B1315-0488-4174-89A5-342C055DCC3E", "Customs Export Manifest {0}", ED_BGMReference);

		public override void Delete()
		{
			if (Messages.Count > 0)
			{
				throw new CannotDeleteException("You cannot delete this Manifest because there are messages associated with it.");
			}
			base.Delete();
		}

		public override ZDateTime ED_DepartureDate
		{
			get { return base.ED_DepartureDate; }
			set
			{
				bool isDiff = base.ED_DepartureDate != value;
				base.ED_DepartureDate = value;
				if (isDiff && !IsCopying)
				{
					Validation.ValidateED_ManifestType();
				}
			}
		}

		public override ZString ED_FlightNumber
		{
			get { return base.ED_FlightNumber; }
			set
			{
				bool isDiff = base.ED_FlightNumber != value;
				base.ED_FlightNumber = value;
				if (isDiff && !IsCopying)
				{
					Validation.ValidateED_ManifestType();
				}
			}
		}

		public override ZString ED_VesselName
		{
			get { return base.ED_VesselName; }
			set
			{
				bool isDiff = base.ED_VesselName != value;
				base.ED_VesselName = value;
				if (isDiff && !IsCopying)
				{
					Validation.ValidateED_ManifestType();
				}
			}
		}

		public override ZString ED_VoyageNumber
		{
			get { return base.ED_VoyageNumber; }
			set
			{
				bool isDiff = base.ED_VoyageNumber != value;
				base.ED_VoyageNumber = value;
				if (isDiff && !IsCopying)
				{
					Validation.ValidateED_ManifestType();
				}
			}
		}

		public override ZGuid ED_OA_CTOAddress
		{
			get { return base.ED_OA_CTOAddress; }
			set
			{
				bool isDiff = base.ED_OA_CTOAddress != value;
				base.ED_OA_CTOAddress = value;
				if (isDiff && !IsCopying)
				{
					Validation.ValidateED_ManifestType();
				}
			}
		}

		public override ZString ED_ManifestType
		{
			get { return base.ED_ManifestType; }
			set
			{
				bool isDiff = base.ED_ManifestType != value;
				base.ED_ManifestType = value;
				if (isDiff && !IsCopying)
				{
					Lines.MarkAsNeedingValidation();
				}
			}
		}

		protected bool ED_ManifestType_ReadOnly
		{
			get { return HaveMessagesBeenSentToCustoms; }
		}

		protected bool ED_TransportMode_ReadOnly
		{
			get { return TransportModeReadOnly; }
		}

		protected virtual bool TransportModeReadOnly
		{
			get { return HaveMessagesBeenSentToCustoms; }
		}

		public virtual bool HaveMessagesBeenSentToCustoms
		{
			get
			{
				return !IsDeleted
								&& (IsWaitingForDepartureReportResponse
										|| IsWaitingForManifestResponse
										|| IsManifestDeclaredAtCustoms
										|| IsDepartureReportDeclaredAtCustoms);
			}
		}

		public override ZString ED_TransportMode
		{
			get { return base.ED_TransportMode; }
			set
			{
				bool different = base.ED_TransportMode != value;
				base.ED_TransportMode = value;
				if (different && !IsCopying)
				{
					if (IsAir)
					{
						ED_NoOfContainer = 0;
						ED_NoOfEmptyContainers = 0;
					}
					Lines.MarkAsNeedingValidation();
					Validation.ValidateED_ManifestType();
				}
			}
		}

		public bool IsAir
		{
			get { return ED_TransportMode == Core.Constants.TransportModes.Air; }
		}

		public bool IsMainManifest
		{
			get { return ED_ManifestType == ManifestTypeList.Codes.ExportMainManifest; }
		}

		public bool IsConsolidation
		{
			get { return ED_ManifestType == ManifestTypeList.Codes.ConsolidationExportSubManifest; }
		}

		public bool IsSlot
		{
			get { return ED_ManifestType == ManifestTypeList.Codes.SlotExportSubManifest; }
		}

		public bool IsDeparture
		{
			get { return ED_ManifestType == ManifestTypeList.Codes.DepartureReport; }
		}

		public bool IsOld
		{
			get { return !ED_DocumentStatus.IsEmpty && !ED_DepartureReportStatus.IsEmpty; }
		}

		public bool IsCTO
		{
			get { return ED_ManifestType == AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone; }
		}

		public bool IsAirCTOHeader
		{
			get { return IsAirCTOHeaderCore; }
		}

		protected virtual bool IsAirCTOHeaderCore
		{
			get { return false; }
		}

		protected override Customs.Business.ExportCustomsManifestHeaderLookups GetNewLookups()
		{
			return new ExportCustomsManifestHeaderLookups(this);
		}

		public ZString Details
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("Reference: " + ED_BGMReference + "\r\n");

				if (!ED_CAN.IsEmpty)
				{
					builder.Append("CAN: " + ED_CAN + "\r\n");
				}

				if (!ED_DepartureDate.IsEmpty)
				{
					builder.Append("Departure Date: " + ED_DepartureDate + "\r\n");
				}

				if (!ED_RL_NKPortOfDeparture.IsEmpty)
				{
					builder.Append("Port of Departure: " + ED_RL_NKPortOfDeparture + "\r\n");
				}

				if (!ED_RN_NKCountryOfDestination.IsEmpty)
				{
					builder.Append("Country/Region of Destination: " + ED_RN_NKCountryOfDestination + "\r\n");
				}

				if (!ED_AirWayBill.IsEmpty)
				{
					builder.Append("Air Waybill: " + ED_AirWayBill + "\r\n");
				}

				if (!ED_FlightNumber.IsEmpty)
				{
					builder.Append("Flight Number: " + ED_FlightNumber + "\r\n");
				}

				if (Vessel != null)
				{
					builder.Append("Vessel: " + Vessel.RV_Code + "\r\n");
				}

				if (!ED_VoyageNumber.IsEmpty)
				{
					builder.Append("Voyage Number: " + ED_VoyageNumber + "\r\n");
				}

				return builder.ToString();
			}
		}

		public ZString ShortDescription
		{
			get { return ED_CAN.IsEmpty ? ZString.Empty : new ZString("CAN: " + ED_CAN); }
		}

		public ZString CarrierPartyID
		{
			get
			{
				ZString result = GlbCompany.CurrentCompany.OrgProxy.LocalBusinessRegNo;
				if (result.IsEmpty)
				{
					result = GlbCompany.CurrentCompany.OrgProxy.LocalCustomsClientCode;
				}

				return result;
			}
		}

		public override ZShort ED_NoOfContainer
		{
			get { return base.ED_NoOfContainer; }
			set
			{
				bool different = base.ED_NoOfContainer != value;
				base.ED_NoOfContainer = value;
				if (different)
				{
					Validation.ValidateED_NoOfPacks();
				}
			}
		}

		public override ZInt ED_NoOfPacks
		{
			get { return base.ED_NoOfPacks; }
			set
			{
				bool different = base.ED_NoOfPacks != value;
				base.ED_NoOfPacks = value;
				if (different)
				{
					Validation.ValidateED_NoOfContainer();
				}
			}
		}

		public override ZShort ED_NoOfEmptyContainers
		{
			get { return base.ED_NoOfEmptyContainers; }
			set
			{
				base.ED_NoOfEmptyContainers = value;
				CalculateTotalContainersFromLines();
			}
		}

		public ExportCustomsManifestLines CurrentManifestLine
		{
			get { return fCurrentManifestLine; }
		}

		protected ExportCustomsManifestLines fCurrentManifestLine;

		#endregion

		#region Totals Calculation

		public void CalculateTotalPackagesFromLines()
		{
			ZInt total = 0;
			foreach (ExportCustomsManifestLines line in Lines)
			{
				total += line.EL_NumberOfPackages;
			}
			ED_NoOfPacks = total;
		}

		public void CalculateTotalContainersFromLines()
		{
			ZShort total = 0;
			foreach (ExportCustomsManifestLines line in Lines)
			{
				total += line.EL_NumberOfContainers;
			}
			ED_NoOfContainer = total;
		}

		#endregion

		#region Related Business Objects

		public bool CanImport
		{
			get { return Lines.Count == 0; }
		}

		[ChildEditable(true)]
		public new ExportCustomsManifestLinesCollection Lines
		{
			get { return (ExportCustomsManifestLinesCollection)base.Lines; }
		}

		public virtual RefUNLOCOCollection PortOfDepartureList
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		#endregion

		#region Validation

		protected override Customs.Business.ExportCustomsManifestHeaderValidation GetNewValidation()
		{
			return new ExportCustomsManifestHeaderValidation(this);
		}

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ED_TransportMode = Core.Constants.TransportModes.Sea;
		}

		#endregion

		#region Messaging

		public override bool IsWaitingForManifestResponse
		{
			get { return ManifestStatus.StartsWith(Constants.WaitingForResponse); }
		}

		public override bool IsWaitingForDepartureReportResponse
		{
			get { return DepartureStatus.StartsWith(Constants.WaitingForResponse); }
		}

		public override bool IsManifestDeclaredAtCustoms
		{
			get { return !ED_CAN.IsEmpty; }
		}

		[ChildEditable]
		public new EDIMessageCollection Messages
		{
			get
			{
				EDIMessageCollection result = base.Messages;
				RegisterEditableChildObject(result);
				return result;
			}
		}

		public override bool IsDepartureReportDeclaredAtCustoms
		{
			get
			{
				bool result = false;
				EDIMessage[] departMessages = Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.CMR, new ZString[] { CMRMessage.CMRMessageTypes.DEPART }, ZString.Empty);

				int index = 0;
				EDIMessage lastReceivedMessage = null;
				do
				{
					if (index >= departMessages.Length)
					{
						break;
					}

					if (departMessages[index].EM_ReceiveTransmit == EDIMessage.Direction.Receive)
					{
						lastReceivedMessage = departMessages[index];
					}

					index++;
				}
				while (lastReceivedMessage == null);

				if (lastReceivedMessage != null)
				{
					EDIMessage lastRespondedToMessage = null;
					do
					{
						if (index >= departMessages.Length)
						{
							break;
						}

						if (departMessages[index].EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
						{
							lastRespondedToMessage = departMessages[index];
						}

						index++;
					}
					while (lastRespondedToMessage == null);
					if (lastRespondedToMessage != null)
					{
						result = lastReceivedMessage.EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Rejected && lastRespondedToMessage.EM_MessageSubType != CMRMessage.MessageSubTypes.Original ||
							lastReceivedMessage.EM_MessageSubType != CMRMessage.ManifestResponseSubTypes.Rejected && lastRespondedToMessage.EM_MessageSubType != CMRMessage.MessageSubTypes.Withdraw;
					}
				}
				return result;
			}
		}

		public ExportCustomsManifestHeaderMessageManager MessageManager
		{
			get { return new ExportCustomsManifestHeaderMessageManager(this); }
		}

		protected ExportCustomsManifestHeaderMessageManager GetMessageManager()
		{
			return new ExportCustomsManifestHeaderMessageManager(this);
		}

		protected bool ED_CCAN_ReadOnly
		{
			get { return !ED_CAN.IsEmpty; }
		}

		protected override Customs.Business.ExportCustomsManifestLinesCollection CreateNewExportCustomsManifestLinesCollection()
		{
			return new ExportCustomsManifestLinesCollection(this, Factory);
		}

		#region Message Statuses

		public ZString DepartureStatus
		{
			get { return GetMessageStatus(Messages.GetLastMessage(EDIMessage.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.DEPART)); }
		}

		public ZPropertyInfo DepartStatusInfo
		{
			get { return GetZPropertyInfo(Schema.DepartureStatus); }
		}

		public ZString ManifestStatus
		{
			get
			{
				ZString messageType = IsMainManifest ? CMRMessage.CMRMessageTypes.EMM : CMRMessage.CMRMessageTypes.ESM;
				var excludedStatuses = new ZString[] { EDIMessage.Status.Discarded };
				return GetMessageStatus(Messages.GetLastMessage(EDIMessage.ApplicationCodes.CMR, messageType, "", System.Array.Empty<ZString>(), System.Array.Empty<ZString>(), excludedStatuses, System.Array.Empty<ZString>()));
			}
		}

		public ZPropertyInfo ManifestStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ManifestStatus); }
		}

		#endregion

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CustomsExportManifest);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion
	}
}
