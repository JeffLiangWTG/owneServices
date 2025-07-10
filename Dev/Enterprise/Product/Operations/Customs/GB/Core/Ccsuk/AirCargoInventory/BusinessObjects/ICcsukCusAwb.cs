using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory
{
	public interface ICcsukCusAwb : IBusiness, Integration.Customs.GB.CCSUK.ICcsukCusAwbBase, IDocumentSupportable, IBranchProvider, Customs.Business.IMessageManageableBizObj, IDocManagerSupport, IEDocsDelayedSaver
	{
		EDIMessageCollection Messages { get; }
		ZString MasterBill { get; }
		ZString SplitReference { get; }
		ZString ReferenceNumber { get; }
		ICuscar GetCuscarWrapper();
		ZString ReasonForNotAllowSplit { get; }
		CusUnderbondCollection<InterAirportRemoval> IARs { get; }
		CusUnderbondCollection<InterShedRemoval> ISRs { get; }
		CusUnderbondCollection<TranshipmentRemoval> TSRs { get; }
		CusUnderbondCollection<Fallback> FBKs { get; }
		bool UpdateStatusToCacIfAllowed(ZString customsActionCode, ZDateTime customsActionDate, ZString agentReference, ZString customsActionText);
		void SetCustomsActionCode(ZString code, ZDateTime date);
		GlbStaff UserInChargeOfJob { get; }
		void Split(List<ICuscarLine> fcsLinesHowToSplit);
		SplitCollection Splits { get; }
		IFSR GetAwbToFsrProvider(CcsukTransmissionMessageFunction how);
		ZBool IsThroughAwb { get; }
		ZString ConsignmentOrEntryType { get; set; }
		ControllerID ModuleControllerId { get; }
		CusOutTurnList OutTurns { get; }
		BusinessObjectCollection OutTurnsCollection { get; }
		ZString PresenceOnNetworkStatus { get; set; }
		ZBool IsLodgedAtCcsuk { get; }
		ZBool IsLodgedOrAssumedAtCcsuk { get; }
		CusAwbIsReadOnlyHelper ReadOnlyAndPermissionHelper { get; }
		ZInt NumberOfPiecesReleasedSoFarCumulative(Event eventType);
		void ReleaseThisNumberOfPieces(ZInt additionalPieces, Event typeOfRelease);
		JobDeclaration CreateNewStandaloneCDSDeclaration();
		ZString ChiefDeclarationUCR { get; }
		bool HasEntryWithLodgedOrPrelodgedWithCustoms { get; }
		ZBool IsCompleteOnCcsuk { get; }    // Is marked as complete - Status 1 set (all pieces received) and CAC is a final one (release, cleared, etc)
		void CompleteOnCcsuk(bool propagateToParent = true);            // Mark as complete
		void UncompleteOnCcsuk(bool propagateToParent = true);
		ZBool IsArchivedOnCcsuk { get; }    // Has been marked as being completed more than N days ago
		void ArchiveOnCcsuk(ReasonForArchiving why);    // Mark as old
		ZBool IsPrearrival { get; }
		ZBool IsEntryCancelled { get; }
		ZDateTime LocalCreationDate
		{
			get;
#if DEBUG
			set;
#endif
		}

		ZDateTime LastEditTime { get; }

		ZString ChiefMasterUCRReferenceSuffix { get; }  // Calculates the AWB number part of a job, e.g. 111222222223333333344.  No HBAC part, that's done in Chief.MasterUcrfCalculator. 
#if DEBUG
		new ZDateTime Status1Date { get; set; }  // adding the setter for testing
#endif
		void SetEcStatusRelease(bool isSetting);  // Records or removes 'EC Status release' to CAT and logs event
		bool NumberOfPiecesReceivedReadOnly { get; }
		bool AgentBadgeReadOnly { get; }
		CusAddInfoCollection<CommunityHandlingCode> CommunityHandlingCodes { get; }
		ZBool Status2Granted { get; }
		ZString ReferenceNumberWithShed { get; }
		ZInt NumberOfPiecesDelivered { get; }
		void CalculateNprFromReceiptsIfNecessary();
		ZPropertyInfo ProfileInfo { get; }
		CusOutTurn CreateNewOutTurn();
		void DeactivateByWtg();
		ZDateTime TemporaryStorageEndDate
		{
			get;
		}
		EnterpriseBusinessObject ForwardingParent { get; }
	}
}
