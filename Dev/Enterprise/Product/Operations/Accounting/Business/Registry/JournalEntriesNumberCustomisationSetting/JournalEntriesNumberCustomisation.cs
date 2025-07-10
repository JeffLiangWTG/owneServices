using System;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JournalEntriesNumberCustomisation : TransactionNumberSequenceCustomisation
	{
		public JournalEntriesNumberCustomisation()
		{
		}

		public JournalEntriesNumberCustomisation(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard coded element name")]
		public static class JournalEntriesNumberCustomisationElementNames
		{
			public const string CalendarMonthAs2Digits = "Calendar Month as 2 Digits";
			public const string CalendarMonthAsLetter = "Calendar Month as Letter";
			public const string CalendarYearAsDigits = "Calendar Year as Digit(s)";
			public const string CalendarYearAsLetter = "Calendar Year as Letter";
			public const string JournalEntriesBranchCode = "Journal Entries Branch Code";
			public const string JournalEntriesClassificationGroupCode = "Journal Entries Classification Group Code";
			public const string JournalEntriesDepartmentCode = "Journal Entries Department Code";
			public const string JournalEntriesClassificationGroupCodePrefix = "Journal Entries Classification Group Code Prefix";
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateInclude();
		}

		protected override bool IsYearElement => base.IsYearElement || ElementName == JournalEntriesNumberCustomisationElementNames.CalendarYearAsDigits;

		#region Description

		public override ZString Description
		{
			get
			{
				string result = base.Description;

				switch (ElementName)
				{
					case ElementNames.AccountingPeriodAs2Digits:
							result = Res.GetString("F4BC5E07-3B24-471F-850D-7866AD02E4A1", "Accounting Period as 2 digits, 202401 = 01, 202402 = 02 ... 202412 = 12");
							break;
					case ElementNames.AccountingYearAsDigits:
						result = Res.GetString("DA888AE0-312C-45AA-BB9D-B1A8E19C5309", "Accounting Year as 1, 2 or 4 digits. For example: as a single digit(2024 = 4, 2020 = 0), as a double digit(2024 = 24, 2020 = 20) and as 4 digits(2024 = 2024)");
						break;
					case JournalEntriesNumberCustomisationElementNames.CalendarMonthAsLetter:
							result = Res.GetString("86BE69A0-2589-4b01-B88E-68BBAEC150DB", "Jan = A, Feb = B ... Dec = L");
							break;
					case JournalEntriesNumberCustomisationElementNames.CalendarYearAsLetter:
							result = Res.GetString("4B9FCF31-C8FC-468a-B644-D6C133B8E381", "2001 = A, 2002 = B ... 2026 = Z");
							break;
					case JournalEntriesNumberCustomisationElementNames.CalendarMonthAs2Digits:
							result = Res.GetString("0D3BC10F-0ADB-48A4-B2D2-C0C8E20B7F82", "Calendar Month as 2 digits, JAN = 01, FEB = 02 ... DEC = 12");
							break;
					case JournalEntriesNumberCustomisationElementNames.CalendarYearAsDigits:
							result = Res.GetString("1A736AB0-0940-47DC-9A19-6A8F6B5AC9D8", "Calendar Year as 1, 2 or 4 digits. For example: as a single digit(2024 = 4, 2020 = 0), as a double digit(2024 = 24, 2020 = 20) and as 4 digits(2024 = 2024)");
							break;
					case JournalEntriesNumberCustomisationElementNames.JournalEntriesClassificationGroupCode:
							result = Res.GetString("7756B9BD-7E8A-47BF-8861-168A226EC5C0", "Group codes as configured in the 'Journal Entries Classification Group' registry");
							break;
					case ElementNames.TransactionTypePrefix:
						result = Res.GetString("466B7EE5-7ADA-4C68-AAE3-6FD209A1B193", "Prefix as configured in the 'Transaction Type Prefix' Registry");
						break;
				}

				return result;
			}
		}

		#endregion

		#region Length

		public override ZInt Length
		{
			get
			{
				var result = base.Length;

				switch (ElementName)
				{
					case JournalEntriesNumberCustomisationElementNames.JournalEntriesBranchCode:
						result = GlbBranchSchema.GB_Code.MaxLength;
						break;
					case JournalEntriesNumberCustomisationElementNames.JournalEntriesDepartmentCode:
						result = GlbDepartmentSchema.GE_Code.MaxLength;
						break;
					case JournalEntriesNumberCustomisationElementNames.JournalEntriesClassificationGroupCode:
						result = 3;
						break;
					case JournalEntriesNumberCustomisationElementNames.CalendarMonthAs2Digits:
						result = 2;
						break;
					case JournalEntriesNumberCustomisationElementNames.CalendarMonthAsLetter:
					case JournalEntriesNumberCustomisationElementNames.CalendarYearAsLetter:
						result = 1;
						break;
					case JournalEntriesNumberCustomisationElementNames.JournalEntriesClassificationGroupCodePrefix:
						result = GetJournalEntriesClassificationGroupCodePrefixLength();
						break;
				}

				return result;
			}
			set
			{
				base.Length = value;
			}
		}

		ZInt GetJournalEntriesClassificationGroupCodePrefixLength()
		{
			var result = 0;

			if (CurrentFallbackLevel != null)
			{
				var registryValue = AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode.GetFallBackValueAtAllLevels(CurrentFallbackLevel.CompanyPK(true), CurrentFallbackLevel.BranchPK, CurrentFallbackLevel.DepartmentPK);
				if (registryValue.Any())
				{
					result = registryValue.Cast<JournalEntriesClassificationGroupCode>().Max(x => x.Prefix.Length);
				}
			}

			return result;
		}

		public override void ValidateLength()
		{
			base.ValidateLength();

			if (ElementName == ElementNames.SequenceNumber)
			{
				LengthInfo.ClearAllNotifications();

				if (Length < 5)
				{
					LengthInfo.AddError(ResString.GetMultilingualString("4E2A5740-3F7F-43D2-9891-41E470E46ED9", "The Length of 'Sequence Number' cannot be smaller than 5."));
				}

				if (Length < 10
					&& (ParentCollection.Parent.NumberRule == AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL
						|| ParentCollection.Parent.NumberRule == AccountingConstants.JournalEntriesNumberCustomisationNumberRule.GRP)
					&& ParentCollection.Cast<JournalEntriesNumberCustomisation>().Count(x => x.Fountain) == 1)
				{
					LengthInfo.AddWarning(ResString.GetMultilingualString("DC7AC655-ABCB-47D0-9F09-2A3D73FB856E", "The Length of 'Sequence Number' should not be smaller than 10 if there are no other data element used to generate the number fountain when the 'Number Rule' is set to 'ALL' or 'GRP'."));
				}
			}

			if (ElementName == JournalEntriesNumberCustomisationElementNames.CalendarYearAsDigits)
			{
				CompareValidation.CheckWithinRange(LengthInfo, 1, 4);
			}
		}

		#endregion

		#region Include

		protected override void ValidateInclude()
		{
			base.ValidateInclude();
			IncludeInfo.ClearAllNotifications();
			if (Include
				&& ElementName == JournalEntriesNumberCustomisationElementNames.JournalEntriesClassificationGroupCode
				&& AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroup
					.GetFallBackValueAtAllLevels(CurrentFallbackLevel.CompanyPK(true), Guid.Empty, Guid.Empty)
					.Cast<JournalEntriesClassificationGroup>()
					.Any(x => string.IsNullOrEmpty(x.GroupCode)))
			{
				IncludeInfo.AddError(ResString.GetMultilingualString("F3796F2A-7834-4F7D-B1A3-A5F2766FA19E", "Group Code in registry 'Journal Entries Classification Group' should be set when include 'Journal Entries Classification Group Code'."));
			}
			else if (Include
				&& ElementName == JournalEntriesNumberCustomisationElementNames.JournalEntriesClassificationGroupCodePrefix
				&& AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode
					.GetFallBackValueAtAllLevels(CurrentFallbackLevel.CompanyPK(true), Guid.Empty, Guid.Empty)
					.Cast<JournalEntriesClassificationGroupCode>()
					.Any(x => string.IsNullOrEmpty(x.Prefix)))
			{
				IncludeInfo.AddError(ResString.GetMultilingualString("CA9AFBDF-C983-453E-9C17-9B7B176E357D", "Group Code Prefix in registry 'Journal Entries Classification Group Code' should be set when include 'Journal Entries Classification Group Code Prefix'."));
			}
		}

		#endregion

		#region Fountain

		protected override void ValidateFountain()
		{
			base.ValidateFountain();

			var sequenceNumberCustomisation = ParentCollection.Cast<JournalEntriesNumberCustomisation>().FirstOrDefault(x => x.ElementName == ElementNames.SequenceNumber);
			if (sequenceNumberCustomisation != null)
			{
				sequenceNumberCustomisation.ValidateLength();
			}
		}

		protected override bool Fountain_ReadOnly
		{
			get
			{
				return !Include ||
						ElementName == ElementNames.SequenceNumber ||
						ElementName == ElementNames.TransactionTypePrefix ||
						ElementName == JournalEntriesNumberCustomisationElementNames.JournalEntriesClassificationGroupCode ||
						ElementName == JournalEntriesNumberCustomisationElementNames.JournalEntriesClassificationGroupCodePrefix;
			}
		}

		#endregion

		#region Code

		protected override bool Code_ReadOnly
		{
			get
			{
				return !Include || (base.Code_ReadOnly && ElementName != JournalEntriesNumberCustomisationElementNames.CalendarYearAsDigits);
			}
		}

		#endregion

		public JournalEntriesNumberCustomisationCollection ParentCollection
		{
			get { return (JournalEntriesNumberCustomisationCollection)(GetParentCollection(this, typeof(JournalEntriesNumberCustomisationCollection)) ?? new JournalEntriesNumberCustomisationCollection(null, CurrentFallbackLevel)); }
		}

		#region Xml Serialisation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard coded element name")]
		public const string OriginalCalendarYearAsDigitsName = "Calendar Year as Digits";

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			if (ElementName == OriginalCalendarYearAsDigitsName)
			{
				ElementName = JournalEntriesNumberCustomisationElementNames.CalendarYearAsDigits;
				Code = "4";
			}
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JournalEntriesNumberCustomisation(fallbackLevel);
		}

		#endregion
	}
}
