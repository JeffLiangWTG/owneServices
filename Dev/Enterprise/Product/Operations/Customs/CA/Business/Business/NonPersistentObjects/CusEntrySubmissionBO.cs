using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using IManualSubmissionNote = Enterprise.Integration.Customs.CA.IManualSubmissionNote;

namespace Enterprise.Customs.CA.Business
{
	public class CusEntrySubmissionBO : NonPersistentBusinessObject, IObsoleteValidation, IManualSubmissionNote
	{
		#region Schema

		public abstract class Schema
		{
			public const string ManualSubmissionDate = "ManualSubmissionDate";
			public const string PortOfClearanceOverride = "PortOfClearanceOverride";
			public const int PortOfClearanceOverrideMaxLength = 4;
		}

		#endregion

		#region Constructor

		public CusEntrySubmissionBO(ZString messageType, IEnumerable<ZString> noteText, BusinessObjectFactory factory)
			: base(factory)
		{
			MessageType = messageType;
			var zStrings = noteText as ZString[] ?? noteText.ToArray();
			if (zStrings.Length == 5)
			{
				ZDateTime submittedDateTime;
				ZDateTime.TryParseExact(zStrings.ElementAt(1), out submittedDateTime, DateFormat);
				manualSubmissionDate = submittedDateTime;
				portOfClearanceOverride = zStrings.ElementAt(2);
			}
		}

		#endregion

		#region Properties

		public ZString MessageType { get; }

		public const string DateFormat = "yyyy-MM-dd HH:mm";

		#region Manual Submission Date

		public ZDateTime ManualSubmissionDate
		{
			get { return manualSubmissionDate; }
			set
			{
				SetNonPersistentPropertyValue(ManualSubmissionDateInfo, ref manualSubmissionDate, value);
				ValidateManualSubmissionDate();
				ManualSubmissionDateInfo.RefreshBinding();
			}
		}

		void ValidateManualSubmissionDate()
		{
			if (!IsValidationSuspended)
			{
				ManualSubmissionDateInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(ManualSubmissionDateInfo);
				TypeValidation.CheckValidZDateTimeWithoutRange(ManualSubmissionDateInfo);
			}
		}

		ZDateTime manualSubmissionDate;

		public ZPropertyInfo ManualSubmissionDateInfo => GetZPropertyInfo(Schema.ManualSubmissionDate);

		#endregion

		#region Port Of Clearance Override

		[MaxLength(Schema.PortOfClearanceOverrideMaxLength)]
		[RelatedBusinessObject(nameof(PortOfClearance))]
		[List(nameof(CustomOffices))]
		public ZString PortOfClearanceOverride
		{
			get { return portOfClearanceOverride; }
			set
			{
				SetNonPersistentPropertyValue(PortOfClearanceOverrideInfo, ref portOfClearanceOverride, value);
				CheckMaximumLength(PortOfClearanceOverrideInfo, value);
				ValidatePortOfClearanceOverride();
				PortOfClearanceOverrideInfo.RefreshBinding();
			}
		}

		void ValidatePortOfClearanceOverride()
		{
			if (!IsValidationSuspended)
			{
				PortOfClearanceOverrideInfo.ClearAllNotifications();
				if (!PortOfClearanceOverride.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(PortOfClearanceOverrideInfo);
				}
			}
		}

		ZString portOfClearanceOverride;

		public ZPropertyInfo PortOfClearanceOverrideInfo => GetZPropertyInfo(Schema.PortOfClearanceOverride);

		public ZZRefCusCodeListCombined PortOfClearance => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, PortOfClearanceOverride, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);

		public ZZRefCusCodeListCombinedCollection CustomOffices => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);

		#endregion

		#endregion

		#region Methods

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateManualSubmissionDate();
			ValidatePortOfClearanceOverride();
		}

		public ZString NoteText
		{
			get
			{
				var builder = new ZStringBuilder();
				if (ManualSubmissionDate.IsValid)
				{
					builder.AppendLine(MessageType);
					builder.AppendLine(ManualSubmissionDate.ToString(DateFormat, CultureInfo.InvariantCulture));
					builder.AppendLine(PortOfClearanceOverride);
					builder.AppendLine(Env.CurrentUser.LoginName);
					builder.AppendLine(ZDateTime.UtcNow.ToString(DateFormat, CultureInfo.InvariantCulture));
				}
				return builder.ToString();
			}
		}

		#endregion
	}
}
