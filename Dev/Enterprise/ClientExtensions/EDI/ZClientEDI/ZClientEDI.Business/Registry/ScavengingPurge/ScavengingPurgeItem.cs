using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = ZClientEDI.Business.Res;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public sealed class ScavengingPurgeItem : RegistryBusinessObject
	{
		#region Schema

		new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string PurgeTime = "PurgeTime";
			public const string PurgeTimeUnit = "PurgeTimeUnit";
		}

		#endregion

		#region Time Unit Codes

		public abstract class TimeUnit
		{
			public static readonly ZGuid Week = new ZGuid("8c338a00-2dc1-486c-b473-fe88cd53e4b9");
			public static readonly ZGuid Month = new ZGuid("a25a1a69-19e8-499b-bd74-053cccd00f7f");
			public static readonly ZGuid Year = new ZGuid("2c390ccd-40aa-4a9a-a05c-488759ae820f");

			internal static readonly ResourceString WeekDescription = ResString.GetMultilingualString("2def97f5-4e99-4c13-afbe-db267ee48750", "Week");
			internal static readonly ResourceString MonthDescription = ResString.GetMultilingualString("87e51c05-0a62-4b5c-8259-e977478ade43", "Month");
			internal static readonly ResourceString YearDescription = ResString.GetMultilingualString("fd8ec295-d5d4-49f8-bde1-843c0f90014f", "Year");
		}

		#endregion

		#region Properties

		#region PurgeTime

		public ZShort PurgeTime
		{
			get { return purgeTime; }
			set
			{
				SetNonPersistentPropertyValue(PurgeTimeInfo, ref purgeTime, value);

				if (!IsValidationSuspended)
				{
					ValidatePurgeTime();
				}
			}
		}
		ZShort purgeTime;

		public ZPropertyInfo PurgeTimeInfo
		{
			get { return GetZPropertyInfo(Schema.PurgeTime); }
		}

		public void ValidatePurgeTime()
		{
			PurgeTimeInfo.ClearAllNotifications();
			if (PurgeTime < 1)
			{
				PurgeTimeInfo.AddError(Res.GetString("d014f8c1-d8a9-46d2-ac62-fbc0d962e460", "1 week is the minimum allowed."));
			}
		}

		#endregion

		#region PurgeTimeUnit

		[List("PurgeTimeUnits")]
		public ZGuid PurgeTimeUnit
		{
			get { return purgeTimeUnit; }
			set
			{
				SetNonPersistentPropertyValue(PurgeTimeUnitInfo, ref purgeTimeUnit, value);
				if (!IsValidationSuspended)
				{
					ValidatePurgeTimeUnit();
				}
			}
		}
		ZGuid purgeTimeUnit;

		public ZPropertyInfo PurgeTimeUnitInfo
		{
			get { return GetZPropertyInfo(Schema.PurgeTimeUnit); }
		}

		public void ValidatePurgeTimeUnit()
		{
			PurgeTimeUnitInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PurgeTimeUnitInfo);
			ListValidation.ErrorIfInvalidPK(PurgeTimeUnitInfo, PurgeTimeUnits);
		}

		public CodeDescriptionPairList PurgeTimeUnits
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(TimeUnit.Week, TimeUnit.WeekDescription, TimeUnit.WeekDescription);
				result.AddPair(TimeUnit.Month, TimeUnit.MonthDescription, TimeUnit.MonthDescription);
				result.AddPair(TimeUnit.Year, TimeUnit.YearDescription, TimeUnit.YearDescription);
				return result;
			}
		}

		#endregion

		#endregion // Properties

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ScavengingPurgeItem();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.PurgeTime, PurgeTime.ToString());
			writer.WriteElementString(Schema.PurgeTimeUnit, PurgeTimeUnit.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);

			PurgeTime = new ZShort(reader.ReadElementString(Schema.PurgeTime));
			PurgeTimeUnit = new ZGuid(reader.ReadElementString(Schema.PurgeTimeUnit));
		}

		#endregion // Implementation
	}
}

