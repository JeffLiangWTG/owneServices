using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ApplicationIdentifier : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string ApplicationID = "ApplicationID";
			public const string FullTitle = "FullTitle";
			public const string EnglishFullTitle = "EnglishFullTitle";
			public const string DataTitle = "DataTitle";
			public const string EnglishDataTitle = "EnglishDataTitle";
			public const string DataType = "DataType";
			public const string MinFieldLength = "MinFieldLength";
			public const string MaxFieldLength = "MaxFieldLength";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			ApplicationIdentifier result = new ApplicationIdentifier();
			result.ApplicationID = ApplicationID;
			result.DataTitle = DataTitle;
			result.DataType = DataType;
			result.FullTitle = FullTitle;
			result.MinFieldLength = MinFieldLength;
			result.MaxFieldLength = MaxFieldLength;
			return result;
		}

		#endregion

		#region Properties

		#region ApplicationID

		[MaxLength(4)]
		public ZString ApplicationID
		{
			get { return applicationID; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(ApplicationIDInfo, ref applicationID, value);
				if (!IsValidationSuspended)
				{
					ValidateApplicationID();
				}
			}
		}

		ZString applicationID;

		public ZPropertyInfo ApplicationIDInfo
		{
			get { return GetZPropertyInfo(Schema.ApplicationID); }
		}

		public void ValidateApplicationID()
		{
			ApplicationIDInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ApplicationIDInfo);
		}

		#endregion

		#region ApplicationIDCode

		public ZString ApplicationIDCode
		{
			get { return ApplicationID.ExcludeChars((NoResString)"ns"); }
		}

		#endregion

		#region ApplicationIDHasDecimalPointIndicator

		public bool ApplicationIDHasDecimalPointIndicator
		{
			get { return ApplicationID.EndsWith((NoResString)"n"); }
		}

		#endregion

		#region ApplicationIDHasSequenceNumber

		public bool ApplicationIDHasSequence
		{
			get { return ApplicationID.EndsWith((NoResString)"s"); }
		}

		#endregion

		#region LengthOfFieldPlusAnyIndicator

		public int LengthOfFieldPlusAnyIndicator
		{
			get
			{
				int result = MinFieldLength;
				if (ApplicationIDHasSequence || ApplicationIDHasDecimalPointIndicator)
				{
					result += 1;
				}
				return result;
			}
		}

		#endregion

		#region FullTitle

		[MaxLength(MaxFullTitleLength)]
		public MultilingualString FullTitle
		{
			get { return fullTitle ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				CheckMaximumLength(FullTitleInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(FullTitleInfo, ref fullTitle, value, false);
				EnglishFullTitleInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateFullTitle();
				}
			}
		}

		public ZPropertyInfo FullTitleInfo
		{
			get { return GetZPropertyInfo(Schema.FullTitle); }
		}

		public void ValidateFullTitle()
		{
			FullTitleInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FullTitleInfo);
		}

		//

		[MaxLength(MaxFullTitleLength)]
		public virtual ZString EnglishFullTitle
		{
			get { return FullTitle.GetUnresolvedString(); }
			set { FullTitle = (NoResString)value; }
		}

		public virtual ZPropertyInfo EnglishFullTitleInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishFullTitle); }
		}

		internal const int MaxFullTitleLength = 100;
		MultilingualString fullTitle;

		#endregion

		#region DataTitle

		[MaxLength(MaxDataTitleLength)]
		public MultilingualString DataTitle
		{
			get { return dataTitle ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				CheckMaximumLength(DataTitleInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(DataTitleInfo, ref dataTitle, value, false);
				EnglishDataTitleInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DataTitleInfo
		{
			get { return GetZPropertyInfo(Schema.DataTitle); }
		}

		//

		[MaxLength(MaxDataTitleLength)]
		public ZString EnglishDataTitle
		{
			get { return DataTitle.GetUnresolvedString(); }
			set { DataTitle = (NoResString)value; }
		}

		public ZPropertyInfo EnglishDataTitleInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishDataTitle); }
		}

		MultilingualString dataTitle;
		internal const int MaxDataTitleLength = 50;

		#endregion

		#region DataType

		[MaxLength(3)]
		public ZString DataType
		{
			get { return dataType; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(DataTypeInfo, ref dataType, value);
				if (!IsValidationSuspended)
				{
					ValidateDataType();
				}
			}
		}

		ZString dataType;

		public ZPropertyInfo DataTypeInfo
		{
			get { return GetZPropertyInfo(Schema.DataType); }
		}

		public void ValidateDataType()
		{
			DataTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DataTypeInfo);
			ListValidation.ErrorIfInvalidCode(DataTypeInfo, DataTypesList);
		}

		#endregion

		#region MinFieldLength

		public ZInt MinFieldLength
		{
			get { return minFieldLength; }
			set
			{
				SetNonPersistentPropertyValue<ZInt>(MinFieldLengthInfo, ref minFieldLength, value);
				if (!IsValidationSuspended)
				{
					ValidateMinFieldLength();
				}
			}
		}

		public void ValidateMinFieldLength()
		{
			MinFieldLengthInfo.ClearAllNotifications();
			CompareValidation.CheckNumberNotNegative(MinFieldLengthInfo);
		}

		ZInt minFieldLength;

		public ZPropertyInfo MinFieldLengthInfo
		{
			get { return GetZPropertyInfo(Schema.MinFieldLength); }
		}

		#endregion

		#region MaxFieldLength

		public ZInt MaxFieldLength
		{
			get { return maxFieldLength; }
			set
			{
				SetNonPersistentPropertyValue<ZInt>(MaxFieldLengthInfo, ref maxFieldLength, value);
				if (!IsValidationSuspended)
				{
					ValidateMaxFieldLength();
				}
			}
		}

		public void ValidateMaxFieldLength()
		{
			MaxFieldLengthInfo.ClearAllNotifications();
			CompareValidation.CheckNumberNotNegative(MaxFieldLengthInfo);
		}

		ZInt maxFieldLength;

		public ZPropertyInfo MaxFieldLengthInfo
		{
			get { return GetZPropertyInfo(Schema.MaxFieldLength); }
		}

		#endregion

		#region IsFixedLengthField

		public bool IsFixedLengthField
		{
			get { return MinFieldLength == MaxFieldLength; }
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateApplicationID();
			ValidateFullTitle();
			ValidateDataType();
			ValidateMinFieldLength();
			ValidateMaxFieldLength();
		}

		#endregion

		#region BindToLists

		public CodeDescriptionPairList DataTypesList
		{
			get
			{
				if (dataTypesList == null)
				{
					dataTypesList = new Enterprise.Registry.Business.Warehouse.DataTypeCodeList();
				}
				return dataTypesList;
			}
		}

		CodeDescriptionPairList dataTypesList;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ApplicationID, ApplicationID);
			writer.WriteElementString(Schema.FullTitle, EnglishFullTitle);
			writer.WriteElementString(Schema.DataTitle, EnglishDataTitle);
			writer.WriteElementString(Schema.DataType, DataType);
			writer.WriteElementString(Schema.MinFieldLength, MinFieldLength.ToString());
			writer.WriteElementString(Schema.MaxFieldLength, MaxFieldLength.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ApplicationID = reader.ReadElementString(Schema.ApplicationID);
			EnglishFullTitle = reader.ReadElementString(Schema.FullTitle);
			EnglishDataTitle = reader.ReadElementString(Schema.DataTitle);
			DataType = reader.ReadElementString(Schema.DataType);
			MinFieldLength = reader.ReadElementStringAsZInt(Schema.MinFieldLength);
			MaxFieldLength = reader.ReadElementStringAsZInt(Schema.MaxFieldLength);
		}

		#endregion
	}
}
