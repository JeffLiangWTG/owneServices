using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class InterchangeObj : RegistryBusinessObjectTemplate
	{
		#region Schema

		protected abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string Selected = "Selected";
			public const string PurgeTime = "PurgeTime";
			public const string PurgeTimeUnit = "PurgeTimeUnit";
			public const string LatestPurgedMessageTimeUtc = "LatestPurgedMessageTimeUtc";
		}

		#endregion

		#region Properties

		#region Selected

		public ZBool Selected
		{
			get { return selected; }
			set { SetNonPersistentPropertyValue(SelectedInfo, ref selected, value); }
		}
		ZBool selected;

		public ZPropertyInfo SelectedInfo
		{
			get { return GetZPropertyInfo(Schema.Selected); }
		}

		#endregion

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
				PurgeTimeInfo.AddError(Res.GetString("322afde2-70aa-4acb-af90-c5462be59119", "1 week is the minimum allowed."));
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

		#region LatestPurgedMessageTime

		public ZDateTime LatestPurgedMessageTimeUtc
		{
			get { return latestPurgedMessageTimeUtc; }
			set { SetNonPersistentPropertyValue(LatestPurgedMessageTimeUtcInfo, ref latestPurgedMessageTimeUtc, value); }
		}
		ZDateTime latestPurgedMessageTimeUtc;

		public ZPropertyInfo LatestPurgedMessageTimeUtcInfo
		{
			get { return GetZPropertyInfo(Schema.LatestPurgedMessageTimeUtc); }
		}

		#endregion

		#endregion

		#region Override

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new InterchangeObj();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Selected, Selected.ToString());
			writer.WriteElementString(Schema.PurgeTime, PurgeTime.ToString());
			writer.WriteElementString(Schema.PurgeTimeUnit, PurgeTimeUnit.ToString());
			writer.WriteElementString(Schema.LatestPurgedMessageTimeUtc, LatestPurgedMessageTimeUtc.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Selected = new ZBool(reader.ReadElementString(Schema.Selected));
			PurgeTime = new ZShort(reader.ReadElementString(Schema.PurgeTime));
			PurgeTimeUnit = new ZGuid(reader.ReadElementString(Schema.PurgeTimeUnit));
			LatestPurgedMessageTimeUtc = new ZDateTime(reader.ReadElementString(Schema.LatestPurgedMessageTimeUtc));
		}

		#endregion
	}
}
