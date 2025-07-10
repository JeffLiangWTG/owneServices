using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	[CodeProperty(CusTempStorageRegHeader.Schema.SRH_Reference), DescriptionProperty(nameof(CusTempStorageRegHeader.HumanReadableName))]
	public class CusTempStorageRegHeader : EU.TemporaryStorage.Business.CusTempStorageRegHeader, Integration.Customs.FR.ICusTempStorageRegHeader
	{
		public CusTempStorageRegHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : EU.TemporaryStorage.Business.CusTempStorageRegHeader.Schema
		{
			public const string PackageQty = "PackageQty";
			public const string PackageType = "PackageType";
			public const string LineCount = "LineCount";
			public const string RemainingPackagesQty = "RemainingPackagesQty";
		}

		[ReadOnly(true)]
		public override ZString SRH_Reference
		{
			get => base.SRH_Reference;
			set => base.SRH_Reference = value;
		}

		[ReadOnly(true)]
		public override ZString SRH_InternalReference
		{
			get => base.SRH_InternalReference;
			set => base.SRH_InternalReference = value;
		}

		[ReadOnly(true)]
		public override ZDate SRH_ArrivalDate
		{
			get => base.SRH_ArrivalDate;
			set => base.SRH_ArrivalDate = value;
		}

		[ReadOnly(true)]
		public override ZDateTime SRH_PresentationDate
		{
			get => base.SRH_PresentationDate;
			set => base.SRH_PresentationDate = value;
		}

		[ReadOnly(true)]
		public override ZString SRH_PreviousReferenceType
		{
			get => base.SRH_PreviousReferenceType;
			set => base.SRH_PreviousReferenceType = value;
		}

		[ReadOnly(true)]
		public override ZString SRH_PreviousReference
		{
			get => base.SRH_PreviousReference;
			set => base.SRH_PreviousReference = value;
		}

		[ReadOnly(true)]
		public override ZString SRH_Status
		{
			get => base.SRH_Status;
			set => base.SRH_Status = value;
		}

		public new CusTempStorageRegHeaderLookups Lookups => (CusTempStorageRegHeaderLookups)base.Lookups;

		public new CusTempStorageRegHeaderValidation Validation => (CusTempStorageRegHeaderValidation)base.Validation;

		public new CusTempStorageRegLineCollection CusTempStorageRegLines => (CusTempStorageRegLineCollection)base.CusTempStorageRegLines;

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderLookups GetNewLookups() => new CusTempStorageRegHeaderLookups(this);

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderValidation GetNewValidation() => new CusTempStorageRegHeaderValidation(this);

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineCollection CreateNewCusTempStorageRegLines()
		{
			var lines = new CusTempStorageRegLineCollection(this);
			lines.SetReadOnlyIncludingChildren(true);
			lines.OnLoadedIntoCollection = line => line.CusTempStorageRegLineTransactions.SetReadOnlyIncludingChildren(SRH_Status != TempStorageDeclarationStatusList.Codes.Open);
			return lines;
		}

		protected override Type GetStorageRegLineTypeCore() => typeof(CusTempStorageRegLine);

		protected new CusTempStorageJobHeader CusTempStorageHeader => (CusTempStorageJobHeader)base.CusTempStorageHeader;

		protected override ZString HumanReadableNameCore => Res.GetString("Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegHeader|HumanReadableName", "Temp. Storage Register {0}", SRH_Reference + "/" + SRH_InternalReference);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SRH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			SRH_Status = TempStorageDeclarationStatusList.Codes.Closed;
		}

		internal void UpdateStatus()
		{
			if (ShouldBeClosed)
			{
				SRH_Status = TempStorageDeclarationStatusList.Codes.Closed;
			}
			else
			{
				SRH_Status = TempStorageDeclarationStatusList.Codes.Open;
				CusTempStorageRegLines.OnLoadedIntoCollection = line => line.CusTempStorageRegLineTransactions.SetReadOnlyIncludingChildren(false);
			}
		}

		public void ReOpen()
		{
			SRH_Status = TempStorageDeclarationStatusList.Codes.Open;
			CusTempStorageRegLines.Cast<CusTempStorageRegLine>().ForEach(x => x.CusTempStorageRegLineTransactions.SetReadOnlyIncludingChildren(false));
			IsReopened = true;
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (IsReopened && !IsDeclarationClosed)
			{
				Logs.AddNew(AutoEvents.UnlockForEdit, "The temporary storage register is reopened for manual adjustment");
			}

			if (IsDeclarationClosed)
			{
				var storageDec = CusTempStorageHeader?.CusTempStorageDec;
				if (storageDec != null && storageDec.STH_DeclarationStatus != TempStorageDeclarationStatusList.Codes.Closed)
				{
					storageDec.STH_DeclarationStatus = TempStorageDeclarationStatusList.Codes.Closed;
				}
				CusTempStorageRegLines.OnLoadedIntoCollection = line => line.CusTempStorageRegLineTransactions.SetReadOnlyIncludingChildren(true);

				if (HasNotBookedEvent)
				{
					Logs.AddNew(AutoEvents.ClearanceCompleted, "The temporary storage declaration has been closed");
				}
			}
		}
		public bool ShouldBeClosed => RemainingPackagesQty <= 0;

		public bool IsDeclarationClosed => SRH_Status == TempStorageDeclarationStatusList.Codes.Closed;

		public bool IsReopened { get; set; }

		bool HasNotBookedEvent => !Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.ClearanceCompletedCode);
	}
}
