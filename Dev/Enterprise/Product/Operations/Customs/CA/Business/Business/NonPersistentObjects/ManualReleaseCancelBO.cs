using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using IManualReleaseNote = Enterprise.Integration.Customs.CA.IManualReleaseNote;

namespace Enterprise.Customs.CA.Business
{
	public class ManualReleaseCancelBO : NonPersistentBusinessObject, IObsoleteValidation, IManualReleaseNote
	{
		#region Schema

		public abstract class Schema
		{
			public const string ManualReleaseDate = "ManualReleaseDate";
			public const string ManualReleaseReason = "ManualReleaseReason";

			public const int ManualReleaseReasonMaxLength = 50;
		}

		#endregion

		public ManualReleaseCancelBO(ZString noteText, BusinessObjectFactory factory)
			: base(factory)
		{
			if (!noteText.IsEmpty)
			{
				var indexList = GetIndexList(noteText);
				var isNoteTextValid = indexList.Count == 4;
				ZDateTime rlsdate;
				ZDateTime.TryParseExact(isNoteTextValid ? indexList[0] : ZString.Empty, out rlsdate, DateFormat);
				manualReleaseDate = rlsdate;
				manualReleaseReason = isNoteTextValid ? indexList[1] : ZString.Empty;
				manualReleaseUser = isNoteTextValid ? indexList[2] : ZString.Empty;
				ZDateTime sysdate;
				ZDateTime.TryParseExact(isNoteTextValid ? indexList[3] : ZString.Empty, out sysdate, DateFormat);
				manualReleaseSystemDate = sysdate;
			}
		}

		public ZDateTime ManualReleaseDate
		{
			get
			{
				return manualReleaseDate;
			}
			set
			{
				SetNonPersistentPropertyValue(ManualReleaseDateInfo, ref manualReleaseDate, value);
				ValidateManualReleaseDate();
				ManualReleaseDateInfo.RefreshBinding();
			}
		}

		void ValidateManualReleaseDate()
		{
			if (!IsValidationSuspended)
			{
				ManualReleaseDateInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(ManualReleaseDateInfo);
				TypeValidation.CheckValidZDateTimeWithoutRange(ManualReleaseDateInfo);
			}
		}

		ZDateTime manualReleaseDate;

		public ZPropertyInfo ManualReleaseDateInfo
		{
			get { return GetZPropertyInfo(Schema.ManualReleaseDate); }
		}

		[MaxLength(50)]
		public ZString ManualReleaseReason
		{
			get
			{
				return manualReleaseReason;
			}
			set
			{
				SetNonPersistentPropertyValue(ManualReleaseReasonInfo, ref manualReleaseReason, value);
				CheckMaximumLength(ManualReleaseReasonInfo, value);
				ValidateManualReleaseReason();
				ManualReleaseReasonInfo.RefreshBinding();
			}
		}

		void ValidateManualReleaseReason()
		{
			if (!IsValidationSuspended)
			{
				ManualReleaseReasonInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(ManualReleaseReasonInfo);
			}
		}
		ZString manualReleaseReason;

		public ZPropertyInfo ManualReleaseReasonInfo
		{
			get { return GetZPropertyInfo(Schema.ManualReleaseReason); }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateManualReleaseReason();
			ValidateManualReleaseDate();
		}

		public ZDateTime ManualReleaseSystemDate
		{
			get { return manualReleaseSystemDate; }
		}

		readonly ZDateTime manualReleaseSystemDate;

		public ZString ManualReleaseUser
		{
			get { return manualReleaseUser; }
		}

		readonly ZString manualReleaseUser;

		const string DateFormat = "yyyy-MM-dd HH:mm";

		List<ZString> GetIndexList(ZString noteText)
		{
			var indexList = new List<ZString>();
			var splitstr = noteText.Split('\r', '\n');
			foreach (var zString in splitstr.Where(zString => !zString.IsEmpty))
			{
				indexList.Add(zString);
			}
			return indexList;
		}

		public ZString NoteText
		{
			get
			{
				var builder = new ZStringBuilder();
				if (ManualReleaseDate.IsValid)
				{
					builder.Append(ManualReleaseDate.ToString(ManualReleaseCancelBO.DateFormat, CultureInfo.InvariantCulture));
					builder.Append(ManualReleaseReason);
					if (ManualReleaseUser.IsEmpty)
					{
						builder.Append(Env.CurrentUser.LoginName);
					}
					else
					{
						builder.Append(ManualReleaseUser);
					}

					if (ManualReleaseSystemDate.IsValid)
					{
						builder.Append(ManualReleaseSystemDate.ToString(DateFormat, CultureInfo.InvariantCulture));
					}
					else
					{
						builder.Append(ZDateTime.UtcNow.ToString(DateFormat, CultureInfo.InvariantCulture));
					}
				}

				return builder.ToStringWithNewLineBetweenAppends();
			}
		}
	}
}
