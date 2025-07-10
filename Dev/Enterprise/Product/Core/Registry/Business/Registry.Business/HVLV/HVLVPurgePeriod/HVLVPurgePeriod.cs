using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HVLVPurgePeriod : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string IsEnabled = "IsEnabled";
			public const string PurgePeriod = "PurgePeriod";
		}

		#endregion

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			IsEnabled = false;
			PurgePeriod = 6;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidatePurgePeriod();
		}

		#region Bound Properties

		#region IsEnabled

		public ZBool IsEnabled
		{
			get { return isEnabled; }
			set
			{
				SetNonPersistentPropertyValue(IsEnabledInfo, ref isEnabled, value);
			}
		}

		public ZPropertyInfo IsEnabledInfo
		{
			get { return GetZPropertyInfo(Schema.IsEnabled); }
		}

		ZBool isEnabled;

		#endregion

		#region PurgePeriod

		[ReadOnlyMember(nameof(PurgePeriod_ReadOnly))]
		public ZInt PurgePeriod
		{
			get { return purgePeriod; }
			set
			{
				SetNonPersistentPropertyValue(PurgePeriodInfo, ref purgePeriod, value);
			}
		}

		bool PurgePeriod_ReadOnly => !IsEnabled;

		public ZPropertyInfo PurgePeriodInfo
		{
			get { return GetZPropertyInfo(Schema.PurgePeriod); }
		}

		ZInt purgePeriod;

		void ValidatePurgePeriod()
		{
			PurgePeriodInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(PurgePeriodInfo, 3, 36);
		}

		#endregion

		#endregion

		#region Cloning

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HVLVPurgePeriod();
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IsEnabled, IsEnabled.ToString());
			writer.WriteElementString(Schema.PurgePeriod, PurgePeriod.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IsEnabled = new ZBool(reader.ReadElementString(Schema.IsEnabled));
			PurgePeriod = ZInt.Parse(reader.ReadElementString(Schema.PurgePeriod));
		}

		#endregion
	}
}
