using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseResponseObject : NonPersistentBusinessObject
	{
		public ImportLicenseResponseObject(BusinessObjectFactory factory) : base(factory)
		{
		}

		public static class Schema
		{
			public const string ReferenceNumber = "ReferenceNumber";
			public const string EntryNumber = "EntryNumber";
			public const string RegistrationDate = "RegistrationDate";
			public const string Diagnosis = "Diagnosis";
			public const string Status = "Status";
		}

		#region ReferenceNumber

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseResponseObject|ReferenceNumber", Caption = "Import License Id.")]
		public ZString ReferenceNumber
		{
			get { return fReferenceNumber; }
			internal set { SetNonPersistentPropertyValue(ReferenceNumberInfo, ref fReferenceNumber, value); }
		}

		ZString fReferenceNumber;

		public ZPropertyInfo ReferenceNumberInfo => GetZPropertyInfo(Schema.ReferenceNumber);

		#endregion

		#region EntryNumber

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseResponseObject|EntryNumber", Caption = "Import License No.")]
		public ZString EntryNumber
		{
			get { return fEntryNumber; }
			internal set { SetNonPersistentPropertyValue(EntryNumberInfo, ref fEntryNumber, value); }
		}

		ZString fEntryNumber;

		public ZPropertyInfo EntryNumberInfo => GetZPropertyInfo(Schema.EntryNumber);

		#endregion

		#region RegistrationDate

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseResponseObject|RegistrationDate", Caption = "Registration Date")]
		public ZString RegistrationDate
		{
			get { return fRegistrationDate; }
			internal set { SetNonPersistentPropertyValue(RegistrationDateInfo, ref fRegistrationDate, value); }
		}

		ZString fRegistrationDate;

		public ZPropertyInfo RegistrationDateInfo => GetZPropertyInfo(Schema.RegistrationDate);

		#endregion

		#region Diagnosis

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseResponseObject|Diagnosis", Caption = "Diagnosis")]
		public ZString Diagnosis
		{
			get { return fDiagnosis; }
			internal set { SetNonPersistentPropertyValue(DiagnosisInfo, ref fDiagnosis, value); }
		}

		ZString fDiagnosis;

		public ZPropertyInfo DiagnosisInfo => GetZPropertyInfo(Schema.Diagnosis);

		#endregion

		#region Status

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseResponseObject|Status", Caption = "Status")]
		public ZString Status
		{
			get { return fStatus; }
			internal set { SetNonPersistentPropertyValue(StatusInfo, ref fStatus, value); }
		}

		ZString fStatus;

		public ZPropertyInfo StatusInfo => GetZPropertyInfo(Schema.Status);

		#endregion
	}
}
