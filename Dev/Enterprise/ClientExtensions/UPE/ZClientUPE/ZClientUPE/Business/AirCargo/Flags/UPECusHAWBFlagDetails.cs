using System.Drawing;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public abstract class UPECusHAWBFlagDetails : NonPersistentBusinessObject, IObsoleteValidation
	{
		public UPECusHAWBFlagDetails(UPECusHAWB uPECusHAWB)
			: base(uPECusHAWB.Factory)
		{
			this.UPECusHAWB = uPECusHAWB;
		}

		#region AuthorisationReceivedBy

		[MaxLength(3)]
		public ZString AuthorisationReceivedBy
		{
			get { return fAuthorisationReceivedBy; }
			set
			{
				if (fAuthorisationReceivedBy != value)
				{
					CheckMaximumLength(AuthorisationReceivedByInfo, value);
					SetNonPersistentPropertyValue(AuthorisationReceivedByInfo, ref fAuthorisationReceivedBy, value);
					ValidateAuthorisationReceivedBy();
				}
			}
		}

		public ZPropertyInfo AuthorisationReceivedByInfo
		{
			get { return GetZPropertyInfo(nameof(AuthorisationReceivedBy)); }
		}

		public void ValidateAuthorisationReceivedBy()
		{
			if (!IsValidationSuspended)
			{
				AuthorisationReceivedByInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(AuthorisationReceivedByInfo);
				ListValidation.ErrorIfInvalidCode(AuthorisationReceivedByInfo, Lookups.AuthReceivedByList);
			}
		}

		ZString fAuthorisationReceivedBy;

		#endregion

		#region PersonAuthorised

		[MaxLength(35)]
		public ZString PersonAuthorised
		{
			get { return fPersonAuthorised; }
			set
			{
				if (fPersonAuthorised != value)
				{
					CheckMaximumLength(PersonAuthorisedInfo, value);
					SetNonPersistentPropertyValue(PersonAuthorisedInfo, ref 	fPersonAuthorised, value);
					ValidatePersonAuthorised();
				}
			}
		}

		public ZPropertyInfo PersonAuthorisedInfo
		{
			get { return GetZPropertyInfo(nameof(PersonAuthorised)); }
		}

		public void ValidatePersonAuthorised()
		{
			if (!IsValidationSuspended)
			{
				PersonAuthorisedInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(PersonAuthorisedInfo);
			}
		}

		ZString fPersonAuthorised;

		#endregion

		#region Remarks

		[MaxLength(200)]
		public ZString Remarks
		{
			get { return fRemarks; }
			set
			{
				if (fRemarks != value)
				{
					CheckMaximumLength(RemarksInfo, value);
					SetNonPersistentPropertyValue(RemarksInfo, ref fRemarks, value);
				}
			}
		}

		public ZPropertyInfo RemarksInfo
		{
			get { return GetZPropertyInfo(nameof(Remarks)); }
		}

		ZString fRemarks;

		#endregion

		#region Abstract

		public abstract string FlagName { get; }

		protected abstract string NoteReference { get; }

		#endregion

		public UPECusHAWBFlagDetailsLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = GetNewLookups();
				}
				return fLookups;
			}
		}

		public virtual void ValidateAll()
		{
			if (!IsValidationSuspended)
			{
				ValidateAuthorisationReceivedBy();
				ValidatePersonAuthorised();
			}
		}

		public void CreateNote()
		{
			StmNote note = (StmNote)Factory.New(typeof(StmNote));
			note.ST_Description = NoteDescription;
			note.ST_NoteData = RtfStringAsBlob;
			note.ST_IsCustomDescription = true;
			UPECusHAWB.Notes.Add(note);
		}

		protected virtual UPECusHAWBFlagDetailsLookups GetNewLookups()
		{
			return new UPECusHAWBFlagDetailsLookups(this);
		}

		string NoteDescription
		{
			get { return FlagName + " Note"; }
		}

		string RequiredText
		{
			get
			{
				string format = "Authorisation Received By: {0}\r\nPerson Authorised: {1}\r\nRemarks: {2}\r\n\r\n";
				string authReceivedByDesc = Lookups.AuthReceivedByList.GetDescriptionFromCode(AuthorisationReceivedBy);
				return string.Format(format, authReceivedByDesc, PersonAuthorised, Remarks);
			}
		}

		protected FormattedRtfString RtfString
		{
			get
			{
				FormattedRtfString result = new FormattedRtfString(new Font("Courier New", 8));
				result = result.Add(string.Concat(RequiredText, NoteReference), FontStyle.Regular);
				return result;
			}
		}

		ZBlob RtfStringAsBlob
		{
			get { return ZBlob.FromAscii(RtfString.ToRtf()); }
		}

		public readonly UPECusHAWB UPECusHAWB;
		UPECusHAWBFlagDetailsLookups fLookups;
	}
}
