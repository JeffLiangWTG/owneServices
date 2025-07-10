using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DbHealthWarningRegistryElement : RegistryBusinessObjectTemplate
	{
		public DbHealthWarningRegistryElement()
			: this("", "", "", ZBool.False)
		{
		}

		public DbHealthWarningRegistryElement(ZString warningSource, ZString warningType, ZString warningDescription, ZBool isAckowledgeable)
			: this(warningSource, warningType, warningDescription, isAckowledgeable, "", ZDateTime.Empty)
		{
		}

		public DbHealthWarningRegistryElement(ZString warningSource, ZString warningType, ZString warningDescription, ZBool isAckowledgeable, ZString acknowledgedBy, ZDateTime acknowledgedDate)
		{
			this.source = warningSource;
			this.warningType = warningType;
			this.description = warningDescription;
			this.acknowledgedBy = acknowledgedBy;
			this.acknowledgedDate = acknowledgedDate;
			this.isAcknowledgeable = isAckowledgeable;
		}

		#region Schema

		public static class Schema
		{
			public const string Source = "Source";
			public const string WarningType = "WarningType";
			public const string Description = "Description";
			public const string IsAcknowledgeable = "IsAcknowledgeable";
			public const string AcknowledgedBy = "AcknowledgedBy";
			public const string AcknowledgedDate = "AcknowledgedDate";
		}

		#endregion

		#region Properties

		public ZString Source
		{
			get { return source; }
		}

		public ZPropertyInfo SourceInfo
		{
			get { return GetZPropertyInfo(Schema.Source); }
		}

		ZString source;

		public ZString WarningType
		{
			get { return warningType; }
		}

		public ZPropertyInfo WarningTypeInfo
		{
			get { return GetZPropertyInfo(Schema.WarningType); }
		}

		ZString warningType;

		public ZString Description
		{
			get { return description; }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		ZString description;

		public ZBool IsAcknowledgeable
		{
			get { return isAcknowledgeable; }
		}

		ZBool isAcknowledgeable;

		public ZString AcknowledgedBy
		{
			get { return acknowledgedBy; }
		}

		public ZPropertyInfo AcknowledgedByInfo
		{
			get { return GetZPropertyInfo(Schema.AcknowledgedBy); }
		}

		ZString acknowledgedBy;

		public ZDateTime AcknowledgedDate
		{
			get { return acknowledgedDate; }
		}

		public ZPropertyInfo AcknowledgedDateInfo
		{
			get { return GetZPropertyInfo(Schema.AcknowledgedDate); }
		}

		ZDateTime acknowledgedDate;

		public ZString AcknowledgedByUser
		{
			get
			{
				var staff = (IGlbStaff)LoadStaffFactory.LoadFromNaturalKey(
					ObjectFactory.GetType(typeof(IGlbStaff)),
					GlbStaffSchema.GS_Code,
					acknowledgedBy);
				return (staff == null) ? acknowledgedBy : staff.GS_FullName;
			}
		}

		BusinessObjectFactory LoadStaffFactory
		{
			get { return loadStaffFactory ?? (loadStaffFactory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory loadStaffFactory;

		public ZPropertyInfo AcknowledgedByUserInfo
		{
			get { return GetZPropertyInfo(nameof(AcknowledgedByUser)); }
		}

		[BusinessObjectTestExclude()]
		public ZBool IsAcknowledged
		{
			get { return acknowledgedDate.IsValid && !acknowledgedDate.IsEmpty && !String.IsNullOrWhiteSpace(acknowledgedBy); }
			set
			{
				if (value != IsAcknowledged)
				{
					if (IsAcknowledgeable)
					{
						if (value)
						{
							acknowledgedBy = (Environment.Env.CurrentUser == null) ? "(UNKNOWN)" : Environment.Env.CurrentUser.Initials;
							acknowledgedDate = ZDateTime.Now;
						}
						else
						{
							acknowledgedBy = "";
							acknowledgedDate = ZDateTime.Empty;
						}
					}

					AcknowledgedByInfo.RefreshBinding();
					AcknowledgedDateInfo.RefreshBinding();
					IsAcknowledgedInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsAcknowledgedInfo
		{
			get { return GetZPropertyInfo(nameof(IsAcknowledged)); }
		}

		#endregion

		public void SetAcknowledgementInfo(ZString acknowledgedBy, ZDateTime acknowledgedDate)
		{
			if (!IsAcknowledgeable)
			{
				throw new InvalidOperationException(WarningType + " warning cannot be ackowledged");
			}

			this.acknowledgedBy = acknowledgedBy;
			this.acknowledgedDate = acknowledgedDate;
		}

		protected override RegistryBusinessObjectTemplate GetClone(Enterprise.ZArchitecture.Environment.FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DbHealthWarningRegistryElement(this.source, this.warningType, this.description, this.isAcknowledgeable, this.acknowledgedBy, this.acknowledgedDate);
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			writer.WriteStartElement(Schema.WarningType);
			writer.WriteValue(WarningType);
			writer.WriteEndElement();

			writer.WriteStartElement(Schema.Source);
			writer.WriteValue(Source);
			writer.WriteEndElement();

			writer.WriteStartElement(Schema.Description);
			writer.WriteValue(Description);
			writer.WriteEndElement();

			writer.WriteStartElement(Schema.IsAcknowledgeable);
			writer.WriteValue(IsAcknowledgeable.ToString());
			writer.WriteEndElement();

			writer.WriteStartElement(Schema.AcknowledgedBy);
			writer.WriteValue(AcknowledgedBy);
			writer.WriteEndElement();

			writer.WriteStartElement(Schema.AcknowledgedDate);
			writer.WriteValue((acknowledgedDate.IsValid) ? CargoWise.Data.SqlFormatInfo.ToSqlDateTimeString(AcknowledgedDate.ToDateTime()) : "");
			writer.WriteEndElement();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			try
			{
				ReadElementsCurrentSchema(reader);
			}
			catch (XmlException)
			{
				ReadElementsOldSchema(reader);
			}
		}

		void ReadElementsCurrentSchema(XmlReaderWrapper reader)
		{
			warningType = reader.Reader.ReadElementString(Schema.WarningType);
			source = reader.Reader.ReadElementString(Schema.Source);
			description = reader.Reader.ReadElementString(Schema.Description);
			isAcknowledgeable = new ZBool(reader.Reader.ReadElementString(Schema.IsAcknowledgeable));
			acknowledgedBy = reader.Reader.ReadElementString(Schema.AcknowledgedBy);
			string rawAcknowledgedDate = reader.Reader.ReadElementString(Schema.AcknowledgedDate);
			acknowledgedDate = (String.IsNullOrWhiteSpace(rawAcknowledgedDate)) ? ZDateTime.Empty : new ZDateTime(CargoWise.Data.SqlFormatInfo.FromSqlDateTime(rawAcknowledgedDate));
		}

		void ReadElementsOldSchema(XmlReaderWrapper reader)
		{
			source = reader.Reader.ReadElementString(Schema.Source);
			warningType = reader.Reader.ReadElementString(Schema.WarningType);
			description = reader.Reader.ReadElementString(Schema.Description);
		}

		public override bool ReadOnly
		{
			get { return !IsAcknowledgeable; }
		}
	}
}
