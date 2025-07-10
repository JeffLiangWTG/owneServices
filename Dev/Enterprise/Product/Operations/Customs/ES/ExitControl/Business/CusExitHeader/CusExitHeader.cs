using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using CusEntryHeader = Enterprise.Customs.EU.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	[SystemDefinedValues]
	public class CusExitHeader : EU.ExitControl.Business.CusExitHeader
		, Integration.Customs.ESExitControl.ICusExitHeader
		, ICustomsFileParent
	{
		public CusExitHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : EU.ExitControl.Business.CusExitHeader.Schema
		{
			public const string TrainingEntry = "TrainingEntry";
		}

		#region GenAddOn
		public static class GenAddOnColumnConstants
		{
			public const string TrainingEntryColumnName = "ES_ExitControl_TrainingEntry";
		}
		#endregion

		#region CXH_CustomsProfile

		[List(nameof(Lookups) + "." + nameof(CusExitHeaderLookups.CertificateNames))]
		public override ZString CXH_CustomsProfile { get => base.CXH_CustomsProfile; set => base.CXH_CustomsProfile = value; }

		#endregion

		#region CXH_GS_NKCustomsAgent

		public override ZString CXH_GS_NKCustomsAgent
		{
			get => base.CXH_GS_NKCustomsAgent;
			set
			{
				var oldValue = base.CXH_GS_NKCustomsAgent;
				base.CXH_GS_NKCustomsAgent = value;

				if (!IsCopying && oldValue != value)
				{
					SetDefaultCXH_CustomsProfile();
				}
			}
		}

		void SetDefaultCXH_CustomsProfile()
		{
			var certificateNamesList = Lookups.CertificateNames;
			var customsProfile = CXH_CustomsProfile;
			if (customsProfile.IsEmpty || !certificateNamesList.GetAllCodesZString().Contains(customsProfile))
			{
				if (certificateNamesList.Count == 1)
				{
					CXH_CustomsProfile = certificateNamesList[0].Code;
				}
				else
				{
					CXH_CustomsProfile = ZString.Empty;
				}
			}
		}

		#endregion

		public ZBool TrainingEntry
		{
			get => this.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.TrainingEntryColumnName);
			set
			{
				var oldValue = TrainingEntry;
				if (oldValue != value)
				{
					this.SetSystemDefinedValue(GenAddOnColumnConstants.TrainingEntryColumnName, value);
					TrainingEntryInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo TrainingEntryInfo => GetZPropertyInfo(Schema.TrainingEntry);

		protected override EU.ExitControl.Business.CusExitConsignment CreateConsignmentFromEntryHeader(CusEntryHeader entryHeader)
			=> null;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SetDefaultTrainingEntry();
		}

		void SetDefaultTrainingEntry()
		{
			TrainingEntry = true;
		}

		protected override bool ShouldHaveSeqNumInContainersOrEquipmentsAndSealsCore => true;

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
				DefaultCusAgent();
			}

			if (HasUCKLog)
			{
				SetReadOnlyIncludingChildren(false);

				foreach (var report in CusExitReports.Where(x => x.IsSentOrAccepted))
				{
					report.SetReadOnlyIncludingChildren(true);
				}
			}
			else if (HasLCKLog)
			{
				SetReadOnlyIncludingChildren(true);
			}
			else
			{
				SetBOReadOnly();
			}

			base.OnSaving();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			if (!ReadOnly)
			{
				SetBOReadOnly();
			}
		}

		void DefaultCusAgent()
		{
			if (CXH_GS_NKCustomsAgent.IsEmpty && !GlbStaff.CurrentUser.GS_IsSystemAccount)
			{
				CXH_GS_NKCustomsAgent = GlbStaff.CurrentUser.GS_Code;
			}
		}

		void SetBOReadOnly() => SetReadOnlyIncludingChildren(AllReportsAreAccepted);

		bool AllReportsAreAccepted => CusExitReports.Count > 0 && !CusExitReports.Any(x => !x.IsAccepted);

		public new CusExitHeaderLookups Lookups => (CusExitHeaderLookups)base.Lookups;

		protected override ExitControlBase.Business.CusExitHeaderLookups GetNewLookups() => new CusExitHeaderLookups(this);

		public new CusExitHeaderValidation Validation => (CusExitHeaderValidation)base.Validation;

		protected override ExitControlBase.Business.CusExitHeaderValidation GetNewValidation() => new CusExitHeaderValidation(this);

		public new ICusExitReportCollection<CusExitReport> CusExitReports => (ICusExitReportCollection<CusExitReport>)base.CusExitReports;

		protected override ICusExitReportCollection<ExitControlBase.Business.CusExitReport> CreateNewCusExitReportCollection() => new CusExitReportCollection<CusExitReport>(this);

		public new ICusExitConsignmentCollection<CusExitConsignment> CusExitConsignments => (ICusExitConsignmentCollection<CusExitConsignment>)base.CusExitConsignments;

		protected override ICusExitConsignmentCollection<ExitControlBase.Business.CusExitConsignment> CreateNewCusExitConsignmentCollection() => new CusExitConsignmentCollection<CusExitConsignment>(this);

		protected override ICusExitConsignmentPackageCollection<ExitControlBase.Business.CusExitConsignmentPackage> CreateNewCusExitConsignmentPackageCollection()
			=> new CusExitConsignmentPackageCollection<CusExitConsignmentPackage>(this);

		protected override ICusExitContainerCollection<ExitControlBase.Business.CusExitContainer> CreateNewCusExitContainerCollection() => new CusExitContainerCollection<CusExitContainer>(this);

		protected override bool IsUCC6Core => true;

		#region ICustomsFileParent

		bool HasLCKLog => Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit) != null;
		bool HasUCKLog => Logs.MostRecentLogByEventTime(AutoEvents.UnlockForEdit) != null;

		ZBool ICustomsFileParent.IsLocked => HasLCKLog || ReadOnly;

		ZString ICustomsFileParent.DeclarationType => Declaration?.JE_MessageType ?? ZString.Empty;

		ZGuid ICustomsFileParent.BranchPk => Branch.PK;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names",
			Justification = "The param of GetZPropertyInfo will be 'DeclarationType' if use 'nameof(Enterprise.Customs.Business.ICustomsFileParent.DeclarationType)'  to replace it.")]
		ZPropertyInfo ICustomsFileParent.DeclarationTypeInfo => Declaration?.JE_MessageTypeInfo ?? GetZPropertyInfo("Enterprise.Customs.Business.ICustomsFileParent.DeclarationType");

		void ICustomsFileParent.LockFile(ZString reference)
		{
			this.AddLockEvent(reference);
			RefreshBindingIncludingChildren();
		}

		void ICustomsFileParent.UnlockFile(ZString reference)
		{
			this.AddUnlockEvent(reference);
			RefreshBindingIncludingChildren();
		}

		#endregion

		public ZString LockExitHeader(ZString reference)
		{
			var result = ZString.Empty;

			var fileParent = ((ICustomsFileParent)this);
			if (!fileParent.IsLocked)
			{
				fileParent.LockFile(reference);

				result = Res.GetString("7403DB84-65CB-4BAB-BF1D-E11F6763F65A", "This tab page has been locked for edit\r\nExit Control\r\n\r\nYou can click the Exit Control - Unlock Exit Control to unlock it.");
			}
			return result;
		}

		public ZString UnlockExitHeader(ZString reference)
		{
			var result = ZString.Empty;

			var fileParent = ((ICustomsFileParent)this);
			if (fileParent.IsLocked)
			{
				fileParent.UnlockFile(reference);

				result = Res.GetString("C512193F-7786-49F4-9F29-6102FB42178C", "The Exit Control is unlocked.");
			}
			return result;
		}
	}
}
