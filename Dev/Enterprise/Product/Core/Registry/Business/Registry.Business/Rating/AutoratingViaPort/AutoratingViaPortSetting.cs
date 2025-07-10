using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Registry.Business.AutoratingViaPortHelper;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AutoratingViaPortSetting : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Direction = nameof(Direction);
			public const string OriginSourceOption = nameof(OriginSourceOption);
			public const string DestinationSourceOption = nameof(DestinationSourceOption);
			public const string ViaSourceOption = nameof(ViaSourceOption);

			public const int DirectionMaxLength = 3;
			public const int OriginSourceOptionMaxLength = 3;
			public const int DestinationSourceOptionMaxLength = 3;
			public const int ViaSourceOptionMaxLength = 3;
		}

		#endregion

		public AutoratingViaPortSetting()
			: this(null, null)
		{
		}

		public AutoratingViaPortSetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		AutoratingViaPortSettingCollection ParentCollection =>
			parentCollection ??= GetParentCollection(this, typeof(AutoratingViaPortSettingCollection)) as AutoratingViaPortSettingCollection;
		AutoratingViaPortSettingCollection parentCollection;

		#region public (for test)
#if DEBUG
		public
#endif
		#endregion
		AutoratingViaPortConfiguration ParentConfiguration => ParentCollection?.ParentConfiguration;

		ZString ParentJobType => new ZString(ParentConfiguration?.JobType);

		#region Direction

		[MaxLength(Schema.DirectionMaxLength)]
		[List("DirectionList")]
		[ResourceStringData("AutoratingViaPortSetting|Direction", Caption = "Direction")]
		public ZString Direction
		{
			get { return direction; }
			set
			{
				CheckMaximumLength(DirectionInfo, value);
				SetNonPersistentPropertyValue(DirectionInfo, ref direction, value);
				ValidateDirection();
			}
		}

		ZString direction;

		public ZPropertyInfo DirectionInfo => GetZPropertyInfo(Schema.Direction);

		public void ValidateDirection()
		{
			if (!IsValidationSuspended)
			{
				DirectionInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(DirectionInfo);
				ListValidation.ErrorIfInvalidCode(DirectionInfo, DirectionList);

				ValidateCompositeKey();
			}
		}

		public CodeDescriptionPairList DirectionList =>
			new CodeDescriptionPairList()
			{
				DirectionOption.All,
				DirectionOption.Export,
				DirectionOption.Import,
			};

		#endregion

		#region OriginSourceOption

		[MaxLength(Schema.OriginSourceOptionMaxLength)]
		[List("OriginSourceOptionList")]
		[ResourceStringData("AutoratingViaPortSetting|OriginSourceOption", Caption = "Origin")]
		public ZString OriginSourceOption
		{
			get { return originSourceOption; }
			set
			{
				CheckMaximumLength(OriginSourceOptionInfo, value);
				SetNonPersistentPropertyValue(OriginSourceOptionInfo, ref originSourceOption, value);
				ValidateOriginSourceOption();
			}
		}

		ZString originSourceOption;

		public ZPropertyInfo OriginSourceOptionInfo => GetZPropertyInfo(Schema.OriginSourceOption);

		public void ValidateOriginSourceOption()
		{
			if (!IsValidationSuspended)
			{
				OriginSourceOptionInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCode(OriginSourceOptionInfo, OriginSourceOptionList);

				ValidateCompositeKey();
			}
		}

		public CodeDescriptionPairList OriginSourceOptionList
		{
			get
			{
				var aplicableSourceOptionList = new CodeDescriptionPairList();

				switch (ParentJobType)
				{
					case JobType.Code.ForwardingConsol:
					case JobType.Code.QuotedBooking:
						aplicableSourceOptionList.Add(LocationSourceOption.VoyageLoad);
						aplicableSourceOptionList.Add(LocationSourceOption.NotVoyageLoad);
						break;

					case JobType.Code.Shipment:
						aplicableSourceOptionList.Add(LocationSourceOption.FirstLoad);
						aplicableSourceOptionList.Add(LocationSourceOption.NotFirstLoad);
						break;
				}

				return aplicableSourceOptionList;
			}
		}

		#endregion

		#region DestinationSourceOption

		[MaxLength(Schema.DestinationSourceOptionMaxLength)]
		[List("DestinationSourceOptionList")]
		[ResourceStringData("AutoratingViaPortSetting|DestinationSourceOption", Caption = "Destination")]
		public ZString DestinationSourceOption
		{
			get { return destinationSourceOption; }
			set
			{
				CheckMaximumLength(DestinationSourceOptionInfo, value);
				SetNonPersistentPropertyValue(DestinationSourceOptionInfo, ref destinationSourceOption, value);
				ValidateDestinationSourceOption();
			}
		}

		ZString destinationSourceOption;

		public ZPropertyInfo DestinationSourceOptionInfo => GetZPropertyInfo(Schema.DestinationSourceOption);

		public void ValidateDestinationSourceOption()
		{
			if (!IsValidationSuspended)
			{
				DestinationSourceOptionInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCode(DestinationSourceOptionInfo, DestinationSourceOptionList);

				ValidateCompositeKey();
			}
		}

		public CodeDescriptionPairList DestinationSourceOptionList
		{
			get
			{
				var aplicableSourceOptionList = new CodeDescriptionPairList();

				switch (ParentJobType)
				{
					case JobType.Code.ForwardingConsol:
					case JobType.Code.QuotedBooking:
						aplicableSourceOptionList.Add(LocationSourceOption.VoyageDischarge);
						aplicableSourceOptionList.Add(LocationSourceOption.NotVoyageDischarge);
						break;

					case JobType.Code.Shipment:
						aplicableSourceOptionList.Add(LocationSourceOption.LastDischarge);
						aplicableSourceOptionList.Add(LocationSourceOption.NotLastDischarge);
						break;
				}

				return aplicableSourceOptionList;
			}
		}

		#endregion

		#region ViaSourceOption

		[MaxLength(Schema.ViaSourceOptionMaxLength)]
		[List("ViaSourceOptionList")]
		[ResourceStringData("AutoratingViaPortSetting|ViaSourceOption", Caption = "Via")]
		public ZString ViaSourceOption
		{
			get { return viaSourceOption; }
			set
			{
				CheckMaximumLength(ViaSourceOptionInfo, value);
				SetNonPersistentPropertyValue(ViaSourceOptionInfo, ref viaSourceOption, value);
				ValidateViaSourceOption();
			}
		}

		ZString viaSourceOption;

		public ZPropertyInfo ViaSourceOptionInfo => GetZPropertyInfo(Schema.ViaSourceOption);

		public void ValidateViaSourceOption()
		{
			if (!IsValidationSuspended)
			{
				ViaSourceOptionInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(ViaSourceOptionInfo);
				ListValidation.ErrorIfInvalidCode(ViaSourceOptionInfo, ViaSourceOptionList);
			}
		}

		public CodeDescriptionPairList ViaSourceOptionList
		{
			get
			{
				var aplicableSourceOptionList = new CodeDescriptionPairList();

				switch (ParentJobType)
				{
					case JobType.Code.ForwardingConsol:
						aplicableSourceOptionList.Add(LocationSourceOption.VoyageLoad);
						aplicableSourceOptionList.Add(LocationSourceOption.VoyageDischarge);
						aplicableSourceOptionList.Add(LocationSourceOption.LastModeRouteSetDischarge);
						break;

					case JobType.Code.Shipment:
						aplicableSourceOptionList.Add(LocationSourceOption.FirstLoad);
						aplicableSourceOptionList.Add(LocationSourceOption.LastDischarge);
						aplicableSourceOptionList.Add(LocationSourceOption.LastModeRouteSetDischarge);
						break;

					case JobType.Code.QuotedBooking:
						aplicableSourceOptionList.Add(LocationSourceOption.VoyageLoad);
						aplicableSourceOptionList.Add(LocationSourceOption.VoyageDischarge);
						break;
				}

				return aplicableSourceOptionList;
			}
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			using (SuspendCompositeKeyValidation())
			{
				ValidateDirection();
				ValidateOriginSourceOption();
				ValidateDestinationSourceOption();
				ValidateViaSourceOption();
			}

			ValidateCompositeKey();
		}

		public IDisposable SuspendCompositeKeyValidation() =>
			new DisposableAction
			(
				() => ++compositeKeyValidationSuspenderIndex,
				() => --compositeKeyValidationSuspenderIndex
			);

		int compositeKeyValidationSuspenderIndex;

		public bool IsCompositeKeyValidationSuspend => compositeKeyValidationSuspenderIndex > 0;

		void ValidateCompositeKey()
		{
			if (IsCompositeKeyValidationSuspend || IsValidationSuspended)
			{
				return;
			}

			var settings = ParentCollection?.Cast<AutoratingViaPortSetting>() ?? Enumerable.Empty<AutoratingViaPortSetting>();

			var count = settings
				.Count(x =>
					string.Equals(Direction, x.Direction, System.StringComparison.OrdinalIgnoreCase) &&
					string.Equals(OriginSourceOption, x.OriginSourceOption, System.StringComparison.OrdinalIgnoreCase) &&
					string.Equals(DestinationSourceOption, x.DestinationSourceOption, System.StringComparison.OrdinalIgnoreCase));

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
				"C7636EAD-6271-47BC-930D-FCAC6A20EE3D",
				"The same Direction, Origin and Destination setting already exists");

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AutoratingViaPortSetting(fallbackLevel, factory);
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Direction, Direction);
			writer.WriteElementString(Schema.OriginSourceOption, OriginSourceOption);
			writer.WriteElementString(Schema.DestinationSourceOption, DestinationSourceOption);
			writer.WriteElementString(Schema.ViaSourceOption, ViaSourceOption);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Direction = reader.ReadElementString(Schema.Direction);
			OriginSourceOption = reader.ReadElementString(Schema.OriginSourceOption);
			DestinationSourceOption = reader.ReadElementString(Schema.DestinationSourceOption);
			ViaSourceOption = reader.ReadElementString(Schema.ViaSourceOption);
		}

		#endregion
	}
}
