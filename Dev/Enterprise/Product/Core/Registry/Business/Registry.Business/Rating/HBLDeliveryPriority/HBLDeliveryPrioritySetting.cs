using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HBLDeliveryPrioritySetting : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string HBLDeliveryModePriority = nameof(HBLDeliveryModePriority);
			public const int HBLDeliveryModeMaxLength = 9;
		}

		#endregion

		public HBLDeliveryPrioritySetting()
			: this(null, null)
		{
		}

		public HBLDeliveryPrioritySetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region public (for test)
#if DEBUG
		public
#endif
		#endregion
		HBLDeliveryPriorityConfig ParentConfiguration
		{
			get
			{
				var parentCollection = GetParentCollection(this, typeof(HBLDeliveryPrioritySettingCollection));

				return (parentCollection as HBLDeliveryPrioritySettingCollection)?.ParentConfiguration;
			}
		}

		CodeDescriptionPairList ParentHBLDeliveryModeList => ParentConfiguration?.HBLDeliveryModeList;

		#region HBLDeliveryModePriority

		[MaxLength(Schema.HBLDeliveryModeMaxLength)]
		[List("HBLDeliveryModePriorityList")]
		[ResourceStringData("HBLDeliveryPrioritySetting|HBLDeliveryModePriority", Caption = "Fallback Priority")]
		public ZString HBLDeliveryModePriority
		{
			get { return hblDeliveryModePriority; }
			set
			{
				CheckMaximumLength(HBLDeliveryModePriorityInfo, value);
				SetNonPersistentPropertyValue(HBLDeliveryModePriorityInfo, ref hblDeliveryModePriority, value);
				ValidateHBLDeliveryModePriority();
			}
		}

		ZString hblDeliveryModePriority;

		public ZPropertyInfo HBLDeliveryModePriorityInfo => GetZPropertyInfo(Schema.HBLDeliveryModePriority);

		public void ValidateHBLDeliveryModePriority()
		{
			if (!IsValidationSuspended)
			{
				HBLDeliveryModePriorityInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCode(HBLDeliveryModePriorityInfo, HBLDeliveryModePriorityList);

				ValidateDuplicate();
			}
		}

		public CodeDescriptionPairList HBLDeliveryModePriorityList => ParentHBLDeliveryModeList ?? new CodeDescriptionPairList();

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateHBLDeliveryModePriority();
			ValidateDuplicate();
		}

		void ValidateDuplicate()
		{
			var parentCollection = GetParentCollection(this, typeof(HBLDeliveryPrioritySettingCollection)) as HBLDeliveryPrioritySettingCollection;
			var settings = parentCollection?.Cast<HBLDeliveryPrioritySetting>() ?? Enumerable.Empty<HBLDeliveryPrioritySetting>();

			var count = settings.Count(x => string.Equals(HBLDeliveryModePriority, x.HBLDeliveryModePriority, System.StringComparison.OrdinalIgnoreCase));

			if (count > 1)
			{
				AddRowError(IdenticalSettingExists);
			}
			else
			{
				RemoveRowError(IdenticalSettingExists);
			}
		}

		public static readonly MultilingualString IdenticalSettingExists =
			ResString.GetMultilingualString(
				"a4430fee-e023-4e3f-bc93-ab4b31b691e1",
				"The same HBL Delivery Mode Priority setting already exists");

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HBLDeliveryPrioritySetting(fallbackLevel, factory);
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.HBLDeliveryModePriority, HBLDeliveryModePriority);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			HBLDeliveryModePriority = reader.ReadElementString(Schema.HBLDeliveryModePriority);
		}

		#endregion
	}
}
