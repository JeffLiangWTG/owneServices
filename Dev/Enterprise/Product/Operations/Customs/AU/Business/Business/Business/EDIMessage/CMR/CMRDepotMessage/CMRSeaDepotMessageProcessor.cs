using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSeaDepotMessageProcessor
	{
		#region Constructor

		public CMRSeaDepotMessageProcessor(ICMRDepotMessage message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}
			else if (message.Factory == null)
			{
				throw new ArgumentNullException(nameof(message), "Message.Factory is null");
			}

			this.Message = message;
			this.Factory = message.Factory;
			this.Creator = new CFSRecordLoaderAndCreator(Factory);
		}

		#endregion

		#region Public: Process

		public void Process()
		{
			if (Message.IsSea)
			{
				LockOutturnHeaderOrThrowConcurrencyException();

				EDIInterchange interchange = null;
				var seiMessage = Message as CMRSEIMessage;
				var isSEIResponse = seiMessage != null && seiMessage.EM_LinkedObject == null;

				if (isSEIResponse)
				{
					// Move Interchange to Linked Object for processing
					interchange = seiMessage.Interchange;
					seiMessage.EM_EI = ZGuid.Empty;
					seiMessage.EM_LinkedObject = interchange;
				}

				foreach (var line in Message.Lines)
				{
					ProcessLine(line);
				}

				if (isSEIResponse)
				{
					if (interchange != null)
					{
						seiMessage.EM_LinkedObject = null;
						seiMessage.EM_EI = interchange.PK;
					}

					var header = FindHeader();

					if (header != null && (header.HasChanges || header.Outturns.HasChanges))
					{
						header.RegisterSyncData();
					}
				}
			}
		}

		void LockOutturnHeaderOrThrowConcurrencyException()
		{
			var events = Factory.GetValue<IMessageProcessorEvents>();
			if (events != null)
			{
				var key = string.Format("CusOutturnHeader:{0}:{1}:{2}", Message.LloydsNumber, Message.OurPremiseID, Message.VoyageNumber);
				SqlApplicationLock mutex;
				if (!Db.Connection.TryGetLock(key, out mutex))
				{
					var message = string.Format("There is another message being processed for the same outturn with Vessel Lloyds = {0}, Premise ID = {1}, Voyage Number = {2}.",
												Message.LloydsNumber, Message.OurPremiseID, Message.VoyageNumber);
					throw new ConcurrencyConflictException(message);
				}

				events.BatchOfMessagesProcessed += mutex.Dispose;
			}
			else
			{
				if (!Globals.IsTest)
				{
					ErrorReporter.ReportOnce("IMessageProcessorEvents should be set in Enterprise.Messaging.Business.BaseMessageProcessor before processing message.");
				}
			}
		}

		#endregion

		#region ProcessLine

		internal void ProcessLine(ICMRDepotMessageLine line)
		{
			var outturn = FindOutturn(line);

			if (ShouldProcess(outturn))
			{
				if (outturn == null)
				{
					var header = FindHeader();

					if (header == null)
					{
						header = Factory.New<CusOutturnHeader>();
						header.C6_LloydsIMO = Message.LloydsNumber;
						header.C6_OutturningPremiseID = Message.OurPremiseID;
						header.C6_VoyageNum = Message.VoyageNumber;
					}

					outturn = header.Outturns.AddNew();
					outturn.C5_CargoType = line.ContainerMode;
					outturn.C5_ContainerNumber = line.ContainerNumber;
					if (!IsFCL_FCX(line.ContainerMode))
					{
						outturn.C5_MasterBill = line.OceanBillNumber;
						outturn.C5_HouseBill = line.HouseBillNumber;
					}
				}

				if (Message is CMRSEIMessage)
				{
					if (IsFCL_FCX(line.ContainerMode))
					{
						outturn.C5_MasterBill = outturn.C5_HouseBill = ZString.Empty;
					}
					else
					{
						outturn.C5_MasterBill = line.OceanBillNumber;
					}

					if (!line.GoodsDescription.IsEmpty)
					{
						outturn.C5_GoodsDescription = line.GoodsDescription;
					}

					if (!line.MarksAndNumbers.IsEmpty)
					{
						outturn.C5_MarksAndNumbers = line.MarksAndNumbers.Left(outturn.C5_MarksAndNumbersInfo.MaxLength);
					}

					if (!line.NumberOfPackages.IsEmpty)
					{
						outturn.C5_OuterPacks = line.NumberOfPackages;
					}

					if (!line.PackageType.IsEmpty)
					{
						outturn.C5_OuterPackUnits = line.PackageType;
					}
				}
				else
				{
					if (outturn.C5_GoodsDescription.IsEmpty)
					{
						outturn.C5_GoodsDescription = line.GoodsDescription;
					}

					if (outturn.C5_MarksAndNumbers.IsEmpty)
					{
						outturn.C5_MarksAndNumbers = line.MarksAndNumbers.Left(outturn.C5_MarksAndNumbersInfo.MaxLength);
					}

					if (outturn.C5_OuterPacks.IsEmpty)
					{
						outturn.C5_OuterPacks = line.NumberOfPackages;
					}

					if (outturn.C5_OuterPackUnits.IsEmpty)
					{
						outturn.C5_OuterPackUnits = line.PackageType;
					}
				}

				CARSTRecord carstRecord;
				ICusUnderbondDependentCollectionParent parent = Creator.GetOrCreateCFSRecord(line, out carstRecord);
				if (carstRecord != null)
				{
					Message.AddUnmatchedContainer(carstRecord);
				}

				CusUnderbond underbond = null;
				var pivot = parent as CusSCAPivot;
				if (pivot == null)
				{
					underbond = FindUnderbond(outturn, parent, line);
					if (!underbond.Outturns.Contains(outturn))
					{
						underbond.Outturns.Add(outturn);
					}
				}

				outturn.Parent = parent;
				Message.LinkOrCloneMessage(outturn);
				DoOldAuditLogging(underbond, parent);

				if (AUCustomsDataRegistry.Instance.SendAutoSEQRequests.Value && Message.IsSea)
				{
					if ((Message is CMRUBMREQRMessage && ((CMRUBMREQRMessage)Message).GetStatusCode() == CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived) ||
							(Message is CMRCARSTMessage && !outturn.C5_ContainerNumber.IsEmpty && outturn.C5_MasterBill.IsEmpty && outturn.C5_HouseBill.IsEmpty))
					{
						SendSEQMessage(outturn);
					}
				}
			}
		}

		void SendSEQMessage(DepotCusOutturn outturn)
		{
			if (outturn.Header != null)
			{
				EDIMessage[] relevantMessages = outturn.Header.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.CMR,
					new ZString[] { CMRMessage.CMRMessageTypes.SEQ, CMRMessage.CMRMessageTypes.SEI }, string.Empty);
				if (relevantMessages.Length == 0 || relevantMessages[0].EM_MessageType != CMRMessage.CMRMessageTypes.SEQ)
				{
					SeaCargoEstablishmentQueryManager manager = new SeaCargoEstablishmentQueryManager(outturn);
					if (manager.CanSendOriginal)
					{
						manager.GenerateOriginalMessages(outturn);
					}
				}
			}
		}

		void DoOldAuditLogging(CusUnderbond underbond, ICusUnderbondDependentCollectionParent parent)
		{
			// To be refactored
			CMRSeaDepotOldAuditLogging logging = new CMRSeaDepotOldAuditLogging();
			if (Message.MessageType == CMRDepotMessageType.Status)
			{
				CMRCARSTMessage carstMessage = Message as CMRCARSTMessage;
				BusinessObject parentBusinessObject = parent as BusinessObject;
				if (carstMessage != null && parentBusinessObject != null)
				{
					logging.AuditLogCargoStatusResponse(parentBusinessObject, carstMessage);
				}
			}
			else
			{
				CMRUBMREQRMessage ubmMessage = Message as CMRUBMREQRMessage;
				if (ubmMessage != null && underbond != null)
				{
					logging.AuditLogUnderbondResponse(underbond, ubmMessage);
				}
			}
		}

		bool IsFCL_FCX(string containerMode)
		{
			return containerMode == CMRImportCargoTypes.Codes.FullContainerLoad || containerMode == CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills;
		}

		#region Finding Methods

		CusUnderbond FindUnderbond(DepotCusOutturn outturn, ICusUnderbondDependentCollectionParent parent, ICMRDepotMessageLine line)
		{
			CusUnderbond result = null;

			if (outturn.Underbond != null)
			{
				result = outturn.Underbond;
			}

			if (result == null)
			{
				ZQuery underbondQuery = new ZQuery();
				underbondQuery.AddToFilter(CusUnderbondSchema.C4_C6, outturn.Header.PK);
				underbondQuery.AddToFilter(CusUnderbondSchema.C4_ParentID, parent.LinkPK);

				CusUnderbond[] underbonds = Factory.Load<CusUnderbond>(underbondQuery);

				if (underbonds.Length > 1)
				{
					ErrorReporter.ReportOnce("CMRSeaDepotMessageProcessor.FindUnderbond(DepotCusOutturn Outturn)"
						+ outturn.Header.PK.ToString(),
						"More than one underbond found for header: "
						+ outturn.Header.PK.ToString());
				}

				if (underbonds.Length > 0)
				{
					result = underbonds[0];
				}
			}

			if (result == null)
			{
				result = outturn.Header.Underbonds.AddNew();
				result.Outturns.Add(outturn);
				result.LinkedObject = parent;

				result.C4_DestinationPremiseID = Message.OurPremiseID;
				if (Message.OurPremiseID != Message.OriginPremiseID)
				{
					result.C4_OriginPremiseID = Message.OriginPremiseID;
				}

				result.C4_PackageType = line.PackageType;
				result.C4_PiecesManifested = line.NumberOfPackages;
			}

			return result;
		}

		internal CusOutturnHeader FindHeader()
		{
			ZQuery headerQuery = new ZQuery(CusOutturnHeaderSchema.C6_LloydsIMO, Message.LloydsNumber);
			headerQuery.AddToFilter(CusOutturnHeaderSchema.C6_OutturningPremiseID, Message.OurPremiseID);
			headerQuery.AddToFilter(CusOutturnHeaderSchema.C6_VoyageNum, Message.VoyageNumber);

			CusOutturnHeader[] headers = Factory.Load<CusOutturnHeader>(headerQuery);

			if (headers.Length > 1)
			{
				ErrorReporter.ReportOnce("CMRSeaDepotMessageProcessor.FindHeader()"
					+ Message.LloydsNumber
					+ Message.OurPremiseID
					+ Message.VoyageNumber,

					"More than one header found for criteria: "
					+ " " + Message.LloydsNumber
					+ " " + Message.OurPremiseID
					+ " " + Message.VoyageNumber);
			}

			if (headers.Length > 0)
			{
				return headers[0];
			}

			return null;
		}

		DepotCusOutturn FindOutturn(ICMRDepotMessageLine line)
		{
			ZQuery outturnQuery = new ZQuery();
			CMRContainerModeChecker checker = new CMRContainerModeChecker(line.ContainerMode);
			if (checker.IsBulk || checker.IsBreakBulk)
			{
				outturnQuery.AddToFilter(CusOutturnSchema.C5_MasterBill, line.OceanBillNumber);
			}

			if (checker.IsLCL || checker.IsBulk || checker.IsBreakBulk)
			{
				outturnQuery.AddToFilter(CusOutturnSchema.C5_HouseBill, line.HouseBillNumber);
			}

			if (checker.IsLCL || checker.IsFCL || checker.IsFCX)
			{
				outturnQuery.AddToFilter(CusOutturnSchema.C5_ContainerNumber, line.ContainerNumber);
			}

			outturnQuery.AddToFilter(CusOutturnSchema.C5_CargoType, line.ContainerMode);

			CusOutturnHeader header = FindHeader();
			if (header != null)
			{
				outturnQuery.FilterByForeignKey(CusOutturnSchema.C5_C6, new CusOutturnHeader[] { header });
				DepotCusOutturn[] outturns = Factory.Load<DepotCusOutturn>(outturnQuery);

				if (outturns.Length > 1 && checker.IsLCL)
				{
					outturnQuery.AddToFilter(CusOutturnSchema.C5_MasterBill, line.OceanBillNumber);
					outturns = Factory.Load<DepotCusOutturn>(outturnQuery);
				}

				if (outturns.Length > 1)
				{
					string exceptionMsg = "More than one outturn found for criteria: " +
						Message.OurPremiseID + " " +
						Message.VoyageNumber + " " +
						Message.LloydsNumber + " " +
						line.ContainerMode + " " +
						line.ContainerNumber + " " +
						line.HouseBillNumber + " " +
						line.OceanBillNumber;

					ErrorReporter.ReportOnce("CMRSeaDepotMessageProcessor.FindOutturn", exceptionMsg);
				}

				if (outturns.Length > 0)
				{
					return outturns[0];
				}
			}

			return null;
		}

		#endregion

		#endregion

		#region Should Process?

		protected bool ShouldProcess(DepotCusOutturn outturn)
		{
			bool result = false;

			if (outturn != null && Message.MessageType == CMRDepotMessageType.Status)
			{
				result = true;
			}

			if (!result)
			{
				result = !Message.OurPremiseID.IsEmpty && PremiseIDMatcher.CompanyHasDepotWithPremiseID(GlbCompany.GetCurrentCompany(Message.Factory), Message.OurPremiseID);
			}

			return result;
		}

		#endregion

		#region Implementation

		protected readonly CFSRecordLoaderAndCreator Creator;
		protected readonly ICMRDepotMessage Message;
		protected readonly BusinessObjectFactory Factory;

		#endregion
	}
}
