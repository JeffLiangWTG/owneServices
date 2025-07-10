using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.HR;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIGlbStaff : GlbStaff
	{
		public EDIGlbStaff(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void UpdateFromPerson(GlbPerson person)
		{
			if (!Factory.HasContext(EDIConstants.BusinessContext.VersionReportContactImporter))
			{
				base.UpdateFromPerson(person);
			}
		}

		public override ZString GS_MobilePhone
		{
			get =>  base.GS_MobilePhone;
			set
			{
				if (base.GS_MobilePhone != value & string.IsNullOrEmpty(value.ToString()))
				{
					CargoWise.Common.ErrorReporter.ReportOnce("For WI00680086 - A service task is editing staff incorrectly, try to identify which service task is setting staff detail to null");
				}

				base.GS_MobilePhone = value;
			}
		}

		#region StaffEx

		public EdiGlbStaffEx StaffEx => GetEdiGlbStaffEx(true);
		public EdiGlbStaffEx ReadonlyStaffEx => GetEdiGlbStaffEx(false);
		EdiGlbStaffEx staffEx;
		bool staffExIsLoaded;

		public bool CanHaveStaffEx => !GS_IsSystemAccount && !GS_IsResource;

		EdiGlbStaffEx GetEdiGlbStaffEx(bool create)
		{
			if (staffEx != null || (!create && staffExIsLoaded))
			{
				return staffEx;
			}

			if (!CanHaveStaffEx)
			{
				return null;
			}

			if (!staffExIsLoaded)
			{
				staffEx = Factory.LoadTop1<EdiGlbStaffEx>(new ZQuery(EdiGlbStaffExSchema.GS9_GS, PK));
				staffExIsLoaded = true;
			}
			if (staffEx == null && create)
			{
				CreateAndSetStaffExWithoutSettingHasChanges(Factory);
			}
			if (staffEx != null)
			{
				RegisterEditableChildObject(staffEx);
			}

			return staffEx;
		}

		public void SetStaffEx(EdiGlbStaffEx val) => staffEx = val;

		public EdiGlbStaffEx CreateAndSetStaffExWithoutSettingHasChanges(BusinessObjectFactory factory)
		{
			staffEx = factory.New<EdiGlbStaffEx>();
			using (staffEx.SuspendSettingHasChanges())
			{
				staffEx.GS9_GS = PK;
			}
			return staffEx;
		}

		#endregion
		public override ZBlob GS_ProfilePhoto
		{
			get => base.GS_ProfilePhoto;
			set
			{
				base.GS_ProfilePhoto = value;
				if (!value.IsEmpty)
				{
					profilePhotoWasSet = true;
				}
			}
		}
		bool profilePhotoWasSet;

		bool contactCreated;
		public override void OnSaving()
		{
			CheckSavingNewProfilePhoto();
			base.OnSaving();

			if (!GS_IsResource && !GS_IsSystemAccount)
			{
				if (!contactCreated && !IsInDatabase)
				{
					GetContactImporter().CreateOrUpdateContactFromStaff(this);
					contactCreated = true;
				}
				else if (IsInDatabase && !GS_EmailAddress.IsEmpty && GS_EmailAddressInfo.HasChanges)
				{
					GetContactImporter().CreateOrUpdateContactFromStaff(this);
				}
			}
		}

		protected virtual StaffContactImporter GetContactImporter()
		{
			return new StaffContactImporter(Factory);
		}

		void CheckSavingNewProfilePhoto()
		{
			if (profilePhotoWasSet && !IsDeleted && !GS_ProfilePhoto.IsEmpty)
			{
				profilePhotoWasSet = false;
				var change = Factory.New<EdiStaffChange>();
				change.ES9_GS = PK;
				change.ES9_IsProfilePhoto = true;
			}
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection types = new NoteTypeCollection();
				types.Add(EDIPredefinedNoteTypes.Instance.StaffCalendarEmailAddress);
				return types;
			}
		}

		protected override bool ShouldCreateAutoLogIfOnlyChildrenHaveChanges => true;
		protected override bool ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges => true;

		public ZString DomesticName => StaffEx != null ? StaffEx.GS9_DomesticName : ZString.Empty;

		#region Properties

		public ZString CalendarEmailAddress
		{
			get { return CalendarEmailAddressNote != null ? CalendarEmailAddressNote.ST_NoteText : ZString.Empty; }
		}

		public StmNote CalendarEmailAddressNote
		{
			get
			{
				StmNote result = null;
				StmNote[] foundNotes = Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.StaffCalendarEmailAddress.Description);
				if (foundNotes.Length > 0)
				{
					result = foundNotes[0];
				}
				return result;
			}
		}

		#endregion

		#region Validation

		protected override GlbStaffValidation GetNewValidation()
		{
			return GS_IsResource ? new GlbResourceValidation(this) : new EDIGlbStaffValidation(this);
		}

		#endregion

		public void SendGitHubInvite()
		{
			var githubGroupPK = EDIDataRegistry.Instance.GitHubUsersGroup.Value;
			if (githubGroupPK != ZGuid.Empty)
			{
				var githubGroup = Groups.Where(g => g.PK == githubGroupPK).FirstOrDefault();
				if (githubGroup != null)
				{
					Groups.Remove(githubGroup);
					Factory.Save();
				}

				Factory.SuspendValidation();
				try
				{
					Groups.AddFromDatabase(githubGroupPK);
				}
				finally
				{
					Factory.ResumeValidation();
				}

				Factory.Save();
			}
		}
	}
}

