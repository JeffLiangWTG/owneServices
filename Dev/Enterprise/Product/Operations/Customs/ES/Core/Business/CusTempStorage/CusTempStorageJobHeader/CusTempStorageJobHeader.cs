using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using CusTempStorageDecCollection = Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageDecCollection<Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageDec, Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader>;

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	public class CusTempStorageJobHeader : EU.Business.CusTempStorage.CusTempStorageJobHeader,
		Integration.Customs.ES.ICusTempStorageJobHeader
	{
		public CusTempStorageJobHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static CusTempStorageJobHeader New(BusinessObjectFactory factory)
		{
			var header = factory.New<CusTempStorageJobHeader>();
			header.SJH_AppCode = "IST";
			var storageDec = CusTempStorageDec.New(header);
			storageDec.CusTempStorageLines.AddNew();
			return header;
		}

		public ZBool IsIST => true;

		#region Validation

		protected override EU.Business.CusTempStorage.CusTempStorageJobHeaderValidation GetNewValidation() => new CusTempStorageJobHeaderValidation(this);

		public new CusTempStorageJobHeaderValidation Validation => (CusTempStorageJobHeaderValidation)base.Validation;

		#endregion

		#region Lookups

		protected override EU.Business.CusTempStorage.CusTempStorageJobHeaderLookups GetNewLookups() => new CusTempStorageJobHeaderLookups(this);

		public new CusTempStorageJobHeaderLookups Lookups => (CusTempStorageJobHeaderLookups)base.Lookups;

		#endregion

		#region CusTempStorageDecs

		public new CusTempStorageDecCollection CusTempStorageDecs => (CusTempStorageDecCollection)base.CusTempStorageDecs;

		protected override EU.Business.CusTempStorage.CusTempStorageDecCollection CreateNewCusTempStorageDecs() => new CusTempStorageDecCollection(this);

		public new CusTempStorageDec CusTempStorageDec => (CusTempStorageDec)base.CusTempStorageDec;

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SJH_AppCode = "IST";
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateJobReferenceIfNeeded();
		}
		public void PopulateJobReferenceIfNeeded()
		{
			PopulateNumberPropertyIfRequired<ZString>(SJH_JobReferenceInfo, x => Env.NumberFountains.FRTempStorageJobReference.GetNextFormatted(x));
		}

		protected override ZString HumanReadableNameCore => SJH_JobReference;

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override DocumentSupporter GetDocumentSupporter() => new CusTempStorageJobHeaderDocumentSupporter(this);

		protected override EU.Business.CusTempStorage.CusTempStorageDec LoadCusTempStorageDec() => EU.Business.CusTempStorage.CusTempStorageDec.Load<CusTempStorageDec>(this);

		#endregion

		#region Properties
		[List(nameof(Lookups) + "." + nameof(CusTempStorageJobHeaderLookups.GuaranteeList))]
		public override ZGuid SJH_CPH_Guarantee { get => base.SJH_CPH_Guarantee; set => base.SJH_CPH_Guarantee = value; }

		#region SJH_TempStorageEndDateUtc
		[ReadOnlyMember(nameof(TempStorageEndDateUtcReadOnly))]
		[ResourceStringData("ES.CusTempStorageJobHeader.SJH_TempStorageEndDateUtc", Caption = "Temporary Storage End Date", MediumCaption = "End Date", ShortCaption = "End")]
		public override ZDateTime SJH_TempStorageEndDateUtc { get => base.SJH_TempStorageEndDateUtc; set => base.SJH_TempStorageEndDateUtc = value; }

		ZBool TempStorageEndDateUtcReadOnly
		{
			get
			{
				if (!IsInDatabase)
				{
					return false;
				}
				return !SJH_TempStorageEndDateUtcInfo.OriginalValue.IsEmpty;
			}
		}

		public ZBool ShowPreSaveDialogIfTempStorageEndDateUtcIsNotEmpty => !TempStorageEndDateUtcReadOnly && !SJH_TempStorageEndDateUtc.IsEmpty;

		#endregion

		public override ZGuid SJH_OH_Customer
		{
			get => base.SJH_OH_Customer;
			set
			{
				var oldValue = SJH_OH_Customer;
				base.SJH_OH_Customer = value;
				if (!IsCopying && oldValue != SJH_OH_Customer)
				{
					SetCustomsProfileDefaultValue();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateSJH_CustomsProfile();
				}
			}
		}

		#region SJH_CustomsProfile
		[List(nameof(Lookups) + "." + nameof(CusTempStorageJobHeaderLookups.CustomsProfileList))]
		[ResourceStringData("ES.CusTempStorageJobHeader.SJH_CustomsProfile", Caption = "Customs Profile", ShortCaption = "Profile")]
		public override ZString SJH_CustomsProfile { get => base.SJH_CustomsProfile; set => base.SJH_CustomsProfile = value; }

		void SetCustomsProfileDefaultValue()
		{
			if (Lookups.CustomsProfileList.Count == 1)
			{
				SJH_CustomsProfile = Lookups.CustomsProfileList[0].Code;
			}
		}

		#endregion
		#endregion
	}
}
