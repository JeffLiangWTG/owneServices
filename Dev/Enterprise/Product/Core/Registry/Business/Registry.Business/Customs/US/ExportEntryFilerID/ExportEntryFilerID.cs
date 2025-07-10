using System.Text.RegularExpressions;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs.US
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ExportEntryFilerID : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string EntryFilerID = "EntryFilerID";
			public const string EntryFilerIDType = "EntryFilerIDType";
		}

		#endregion

		#region EntryFilerID

		[BusinessObjectTestExclude]
		public ZString EntryFilerID
		{
			get { return entryFilerID; }
			set
			{
				SetNonPersistentPropertyValue(EntryFilerIDInfo, ref entryFilerID, value);

				if (!IsValidationSuspended)
				{
					ValidateEntryFilerID();
				}
			}
		}
		ZString entryFilerID;

		public ZPropertyInfo EntryFilerIDInfo
		{
			get { return GetZPropertyInfo(Schema.EntryFilerID); }
		}

		public void ValidateEntryFilerID()
		{
			EntryFilerIDInfo.ClearAllNotifications();

			if (!EntryFilerIDInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(EntryFilerIDInfo);

				if (!EntryFilerID.IsEmpty)
				{
					if (EntryFilerIDType == AESEntryFilerIDTypeList.Codes.EmployerIdentificationNumber)
					{
						if (!Regex.IsMatch(EntryFilerID, @"^[0-9]{2}-[0-9]{7}$", RegexOptions.IgnoreCase) &&
							!Regex.IsMatch(EntryFilerID, @"^[0-9]{9}$", RegexOptions.IgnoreCase))
						{
							EntryFilerIDInfo.AddError(EINNumberRightFormat);
						}
					}
					else if (EntryFilerIDType == AESEntryFilerIDTypeList.Codes.DataUniversalNumberingSystem)
					{
						if (!Regex.IsMatch(EntryFilerID, @"^[0-9]{9}$", RegexOptions.IgnoreCase))
						{
							EntryFilerIDInfo.AddError(DUNSRightFormat);
						}
					}
					else if (EntryFilerIDType == AESEntryFilerIDTypeList.Codes.SocialSecurityNumber)
					{
						if (!Regex.IsMatch(EntryFilerID, @"^[0-9]{3}-[0-9]{2}-[0-9]{4}$", RegexOptions.IgnoreCase) &&
							!Regex.IsMatch(EntryFilerID, @"^[0-9]{9}$", RegexOptions.IgnoreCase))
						{
							EntryFilerIDInfo.AddError(SSNRightFormat);
						}
					}
				}
			}
		}
		internal const string EINNumberRightFormat = "Employer Identification Number should be in the format, NN-NNNNNNN or NNNNNNNNN\nwhere N is a number.";
		internal const string DUNSRightFormat = "DUNS Number should be (9 digits) in the format: NNNNNNNNN, where N is a number.";
		internal const string SSNRightFormat = "Social Security Number should be in the format, NNN-NN-NNNN or NNNNNNNNN where N is a number.";

		#endregion

		#region EntryFilerIDType

		[List(nameof(EntryFilerIDTypeList))]
		[MaxLength(1)]
		public ZString EntryFilerIDType
		{
			get { return entryFilerIDType; }
			set
			{
				SetNonPersistentPropertyValue(EntryFilerIDTypeInfo, ref entryFilerIDType, value);
				if (!IsValidationSuspended)
				{
					ValidateEntryFilerIDType();
				}
			}
		}
		ZString entryFilerIDType;

		public ZPropertyInfo EntryFilerIDTypeInfo
		{
			get { return GetZPropertyInfo(Schema.EntryFilerIDType); }
		}

		public void ValidateEntryFilerIDType()
		{
			EntryFilerIDTypeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(EntryFilerIDTypeInfo, EntryFilerIDTypeList);
			MandatoryValidation.CheckEntered(EntryFilerIDTypeInfo);
		}

		public CodeDescriptionPairList EntryFilerIDTypeList
		{
			get { return CurrentFactory.GetCachedValue<AESEntryFilerIDTypeList>(); }
		}

		#endregion

		#region Overrides

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			EntryFilerIDType = AESEntryFilerIDTypeList.Codes.EmployerIdentificationNumber;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ExportEntryFilerID();
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearRowNotifications();
			base.RunPreSaveValidationCore();
			ValidateEntryFilerID();
			ValidateEntryFilerIDType();
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.EntryFilerID, EntryFilerID);
			writer.WriteElementString(Schema.EntryFilerIDType, EntryFilerIDType);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EntryFilerID = reader.ReadElementString(Schema.EntryFilerID);
			EntryFilerIDType = reader.ReadElementString(Schema.EntryFilerIDType);
		}

		#endregion
	}
}
