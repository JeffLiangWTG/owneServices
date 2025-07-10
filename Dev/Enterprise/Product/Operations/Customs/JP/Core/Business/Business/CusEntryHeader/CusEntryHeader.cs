using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.JP.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Business
{
	public partial class CusEntryHeader : AutoJPCusEntryHeader, IInputReferenceProvider, INACCSStatus, IErrorMessageProcessingStrategyParent
	{
		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("1928D00D-978C-4F40-A1D5-5978FB9DFE16", Caption = "Inspection Status")]
		public override ZString CH_InspectionStatus { get => base.CH_InspectionStatus; set => base.CH_InspectionStatus = value; }

		[ResourceStringData("3710EE82-3629-494A-959F-9CEC5E52AB43", Caption = "Inspection Status Description", ShortCaption = "Desc.")]
		public ZString InspectionStatusDescription => Factory.GetValue(ref inspectionStatusDescriptionCached, () =>
		{
			var result = ZString.Empty;
			if (CH_InspectionStatus.Length == 4)
			{
				var types = new CodeDescriptionPairList[]
				{
							new InspectionStatus_1_CargoType(),
							new InspectionStatus_2_InspectionType(),
							new InspectionStatus_3_InspectionSubType(),
							new InspectionStatus_4_DocumentRequestType(),
				};

				result = CH_InspectionStatus.ToString().ToCharArray().Select((c, idx) => types[idx].GetDescriptionFromCode(c.ToString())).SkipWhile(s => s.IsNullOrEmpty()).ToStringWithSeparator(" * ");
			}
			return result;
		});
		CachedProperty<ZString> inspectionStatusDescriptionCached;

		[ResourceStringData("4AC2DE46-F0DE-4202-A461-D2CF103EBCA5", Caption = "Cargo Type")]
		public ZString CH_CargoType => CH_InspectionStatus.Left(1);

		[ResourceStringData("96D055D3-4D77-4EE9-A590-80B795BD3946", Caption = "Cargo Type Description", ShortCaption = "Desc.")]
		public ZString CargoTypeDescription => Factory.GetCachedValue<InspectionStatus_1_CargoType>().GetDescriptionFromCode(CH_CargoType);

		[ResourceStringData("6971BB72-EDFF-48A0-9156-0597D44A63E9", Caption = "Inspection Type")]
		public ZString CH_InspectionType => CH_InspectionStatus.SubstringSafe(1, 1);

		[ResourceStringData("143FC1EF-3351-4395-A4B1-895C30CB0427", Caption = "Inspection Type Description", ShortCaption = "Desc.")]
		public ZString InspectionTypeDescription => Factory.GetCachedValue<InspectionStatus_2_InspectionType>().GetDescriptionFromCode(CH_InspectionType);

		[ResourceStringData("115F7D5D-B59B-432C-852A-574629FFDBF8", Caption = " Inspection Sub Type")]
		public ZString CH_InspectionSubType => CH_InspectionStatus.SubstringSafe(2, 1);

		[ResourceStringData("1871DCA1-50F7-4C04-8D85-28188211BF80", Caption = "Inspection Sub Type Description", ShortCaption = "Desc.")]
		public ZString InspectionSubTypeDescription => Factory.GetCachedValue<InspectionStatus_3_InspectionSubType>().GetDescriptionFromCode(CH_InspectionSubType);

		[ResourceStringData("C69508BF-1B59-4629-89B5-63E9BE21E453", Caption = "Document Request Type")]
		public ZString CH_DocumentRequestType => CH_InspectionStatus.SubstringSafe(3, 1);

		[ResourceStringData("0FADBB6F-992D-4317-AC20-CB2482CE73E8", Caption = "Document Request Type Description", ShortCaption = "Desc.")]
		public ZString DocumentRequestTypeDescription => Factory.GetCachedValue<InspectionStatus_4_DocumentRequestType>().GetDescriptionFromCode(CH_DocumentRequestType);

		public ZBool IsCustomsDeclarationPhaseIDCOrEDC => Factory.GetValue(ref isCustomsDeclarationPhaseIDCOrEDC, () => CH_PhaseStatus == CustomsDeclarationPhases.Codes.IDC || CH_PhaseStatus == CustomsDeclarationPhases.Codes.EDC);
		CachedProperty<ZBool> isCustomsDeclarationPhaseIDCOrEDC;

		[ResourceStringData("1FCE0730-1DA8-420B-83F0-365DE7FD4043", Caption = "Declaration Number", MediumCaption = "Declaration No.", ShortCaption = "Decl. No.", FullDescription = "A 10-character unique identifier used for identifying this entry.")]
		public override ZString EntryNumber { get => base.EntryNumber; set => base.EntryNumber = value; }

		[ResourceStringData("D20C3267-10EE-4BD7-ACDC-0B640B7B017D", Caption = "Input Reference", MediumCaption = "Input Ref.", ShortCaption = "Input Ref.", FullDescription = "A 10-character unique identifier used for identifying this entry.")]
		public override ZString CH_BGMReference { get => base.CH_BGMReference; set => base.CH_BGMReference = value; }

		[ResourceStringData("DB9F7538-4CE7-4DF3-AF60-42E867FB33E2", Caption = "Message Status", MediumCaption = "Msg. Status", ShortCaption = "Status")]
		public override ZString CH_Status { get => base.CH_Status; set => base.CH_Status = value; }

		[ResourceStringData("7796E5EF-52E0-43B3-8D8A-51A9BCA5A1DC", Caption = "Message Status Description", MediumCaption = "Msg. Status Desc.", ShortCaption = "Desc.")]
		public ZString CH_StatusDescription => new JPMessageStatusList().GetDescriptionFromCode(CH_Status);

		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.PhaseList))]
		[ResourceStringData("3B2006F5-8544-4DA4-BF0C-22CF501B6C54", Caption = "Phase")]
		public override ZString CH_PhaseStatus { get => base.CH_PhaseStatus; set => base.CH_PhaseStatus = value; }

		[ResourceStringData("EFD900C1-3736-44D6-8419-EFDBC9B80A67", Caption = "Phase Description", ShortCaption = "Desc.")]
		public ZString PhaseDescription => Lookups.PhaseList.GetDescriptionFromCode(CH_PhaseStatus);

		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.CH_EntryStatusList))]
		public override ZString CH_EntryStatus { get => base.CH_EntryStatus; set => base.CH_EntryStatus = value; }

		[ResourceStringData("84639D7B-49BB-434B-AFF2-3825B7601359", Caption = "Entry Status Description")]
		public ZString CH_EntryStatusDescription => Lookups.CH_EntryStatusList.GetDescriptionFromCode(CH_EntryStatus);

		public override bool IsEntryStatusCleared => CurrentStatus == CustomsStatusList.Codes.Cleared;

		public override bool ShouldLogCustomsClearedToDeclarationOrShipment => Declaration?.ActiveEntryHeaders.AreAllEntriesCleared ?? false;

		protected override ZString PreviousStatus => (ZString)CH_EntryStatusInfo.OriginalValue;

		protected override ZString CurrentStatus => CH_EntryStatus;

		protected override bool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus) => originalStatus != newStatus && IsEntryStatusCleared;

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			PopulateNumberPropertyIfRequired<ZString>(CH_BGMReferenceInfo, factory => Env.NumberFountains.NACCSInputReference.GetNextFormatted(factory), ignoreInDatabaseCheck: true);
		}

		public override bool HasBeenWithdrawn => false;

		public override bool ShouldLogEntryStatus => true;

		protected override bool ShouldDeactivateAfterMergeIsDoneAsNoInvoiceLinesLinked => EntryInstruction == null;

		#region IInputReferenceProvider

		ZString IInputReferenceProvider.InputReference => CH_BGMReference;

		#endregion

		#region INACCSStatus

		public ZString MessageStatus
		{
			get => CH_Status;
			set => CH_Status = value;
		}
		public ZString CustomsStatus
		{
			get => CH_EntryStatus;
			set => CH_EntryStatus = value;
		}
		public ZString PhaseStatus
		{
			get => CH_PhaseStatus;
			set => CH_PhaseStatus = value;
		}
		ZBool INACCSStatus.IsExport => Declaration.IsExport;

		#endregion

		#region IErrorMessageProcessingStrategyParent

		IErrorMessageProcessingStrategy IErrorMessageProcessingStrategyParent.ProcessingStrategy => new ErrorMessageProcessingStrategy(this);

		#endregion

	}
}
