using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DataTransferRegistryBusinessObject : AutomaticProcessRegistryBusinessObject
	{
		public DataTransferRegistryBusinessObject()
			: base()
		{
		}

		public DataTransferRegistryBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema

		public new abstract class Schema : AutomaticProcessRegistryBusinessObject.Schema
		{
			public const string Directory = "Directory";
			public const string GroupPK = "GroupPK";
			public const string GroupList = "GroupList";
		}

		#endregion

		public virtual ZBool IsGoodToGo
		{
			get
			{
				ValidateTransferSettings();
				return !IntervalInfo.HasErrors() &&
					!IntervalTypeInfo.HasErrors() &&
					!NextRunDateTimeInfo.HasErrors() &&
					!DirectoryInfo.HasErrors() &&
					!GroupPKInfo.HasErrors();
			}
		}

		protected void ValidateTransferSettings()
		{
			Validate(ValidateInterval);
			Validate(ValidateIntervalType);
			Validate(ValidateNextRunDateTime);
			Validate(ValidateDirectory);
			Validate(ValidateGroupPK);
		}

		#region GroupPK

		public ZGuid GroupPK
		{
			get { return groupPK; }
			set
			{
				SetNonPersistentPropertyValue<ZGuid>(GroupPKInfo, ref groupPK, value);
				Validate(ValidateGroupPK);
			}
		}

		ZGuid groupPK;

		public ZPropertyInfo GroupPKInfo
		{
			get { return GetZPropertyInfo(Schema.GroupPK, "Notify Group"); }
		}

		public void ValidateGroupPK()
		{
			GroupPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(GroupPKInfo);
			ListValidation.ErrorIfInvalidPK(GroupPKInfo, GroupList);
		}

		public IBusinessObjectCollection GroupList
		{
			get { return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IGlbGroupCollection>(), new object[] { CurrentFactory }); }
		}

		#endregion

		#region Directory

		[MaxLength(300)]
		public ZString Directory
		{
			get { return directory; }
			set
			{
				CheckMaximumLength(DirectoryInfo, value);
				SetNonPersistentPropertyValue<ZString>(DirectoryInfo, ref directory, value);
				Validate(ValidateDirectory);
			}
		}

		ZString directory;

		public ZPropertyInfo DirectoryInfo
		{
			get { return GetZPropertyInfo(Schema.Directory); }
		}

		public void ValidateDirectory()
		{
			DirectoryInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DirectoryInfo);
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DataTransferRegistryBusinessObject();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.Directory, Directory.ToString());
			ZString nextRun = NextRunDateTime.IsEmpty ? "" : (string)NextRunDateTime.SqlFormat;
			ZString lastRun = LastRunDateTime.IsEmpty ? "" : (string)LastRunDateTime.SqlFormat;
			writer.WriteElementString(Schema.NextRunDateTime, nextRun);
			writer.WriteElementString(Schema.LastRunDateTime, lastRun);
			writer.WriteElementString(Schema.Interval, Interval.ToString());
			writer.WriteElementString(Schema.GroupPK, GroupPK.ToString());
			writer.WriteElementString(Schema.IntervalType, IntervalType.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			directory = new ZString(reader.ReadElementString(Schema.Directory));
			ZString nextRun = reader.ReadElementString(Schema.NextRunDateTime);
			ZString lastRun = reader.ReadElementString(Schema.LastRunDateTime);
			NextRunDateTime = nextRun.IsEmpty ? ZDateTime.Empty : ZDateTime.FromSqlFormat(nextRun);
			lastRunDateTime = lastRun.IsEmpty ? ZDateTime.Empty : ZDateTime.FromSqlFormat(lastRun);
			Interval = ZInt.Parse(reader.ReadElementString(Schema.Interval));
			groupPK = new ZGuid(reader.ReadElementString(Schema.GroupPK));
			IntervalType = new ZString(reader.ReadElementString(Schema.IntervalType));
		}

		#endregion
	}
}
