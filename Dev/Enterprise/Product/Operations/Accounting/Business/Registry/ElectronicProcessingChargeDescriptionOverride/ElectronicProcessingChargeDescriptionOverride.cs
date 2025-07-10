using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ElectronicProcessingChargeDescriptionOverride : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Transport = "Transport";
			public const string Container = "Container";
			public const string ShipmentType = "ShipmentType";
			public const string Origin = "Origin";
			public const string Destination = "Destination";
			public const string PrefixSuffix = "PrefixSuffix";
			public const string Text = "Text";
			public const string IncludeShipmentNumber = "IncludeShipmentNumber";
		}

		#endregion Schema

		public ElectronicProcessingChargeDescriptionOverride()
		{
		}

		ElectronicProcessingChargeDescriptionOverride(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ElectronicProcessingChargeDescriptionOverride(fallbackLevel);
		}

		public ElectronicProcessingChargeDescriptionOverrideCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (ElectronicProcessingChargeDescriptionOverrideCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return null;
				}
			}
		}

		[List("Lookups.Transports")]
		public ZString Transport
		{
			get => transport;
			set
			{
				SetNonPersistentPropertyValue(TransportInfo, ref transport, value);
				if (!IsValidationSuspended)
				{
					ValidateTransport();
				}
			}
		}
		ZString transport = ZString.Empty;

		public ZPropertyInfo TransportInfo
		{
			get { return GetZPropertyInfo(Schema.Transport); }
		}

		[List("Lookups.Containers")]
		public ZString Container
		{
			get => container;
			set
			{
				SetNonPersistentPropertyValue(ContainerInfo, ref container, value);
				if (!IsValidationSuspended)
				{
					ValidateContainer();
				}
			}
		}
		ZString container = ZString.Empty;

		public ZPropertyInfo ContainerInfo
		{
			get { return GetZPropertyInfo(Schema.Container); }
		}

		[List("Lookups.ShipmentTypes")]
		public ZString ShipmentType
		{
			get => shipmentType;
			set
			{
				SetNonPersistentPropertyValue(ShipmentTypeInfo, ref shipmentType, value);
				if (!IsValidationSuspended)
				{
					ValidateShipmentType();
				}
			}
		}
		ZString shipmentType = ZString.Empty;

		public ZPropertyInfo ShipmentTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ShipmentType); }
		}

		[List("Lookups.Origins")]
		public ZString Origin
		{
			get => origin;
			set
			{
				SetNonPersistentPropertyValue(OriginInfo, ref origin, value);
				if (!IsValidationSuspended)
				{
					ValidateOrigin();
				}
			}
		}
		ZString origin = ZString.Empty;

		public ZPropertyInfo OriginInfo
		{
			get { return GetZPropertyInfo(Schema.Origin); }
		}

		public ZString OriginDescription
		{
			get
			{
				return Lookups.Origins.GetDescriptionFromCode(Origin);
			}
		}

		[List("Lookups.Destinations")]
		public ZString Destination
		{
			get => destination;
			set
			{
				SetNonPersistentPropertyValue(DestinationInfo, ref destination, value);
				if (!IsValidationSuspended)
				{
					ValidateDestination();
				}
			}
		}
		ZString destination = ZString.Empty;

		public ZPropertyInfo DestinationInfo
		{
			get { return GetZPropertyInfo(Schema.Destination); }
		}

		public ZString DestinationDescription
		{
			get
			{
				return Lookups.Destinations.GetDescriptionFromCode(Destination);
			}
		}

		[List("Lookups.PrefixSuffix")]
		public ZString PrefixSuffix
		{
			get => prefixSuffix;
			set
			{
				SetNonPersistentPropertyValue(PrefixSuffixInfo, ref prefixSuffix, value);
				if (!IsValidationSuspended)
				{
					ValidateDescriptionOverridePrefixSuffix();
				}
			}
		}
		ZString prefixSuffix = ZString.Empty;

		public ZPropertyInfo PrefixSuffixInfo
		{
			get { return GetZPropertyInfo(Schema.PrefixSuffix); }
		}

		[MaxLength(30)]
		public ZString Text
		{
			get => text;
			set
			{
				SetNonPersistentPropertyValue(TextInfo, ref text, value);
				if (!IsValidationSuspended)
				{
					ValidateDescriptionOverride();
				}
			}
		}
		ZString text = ZString.Empty;

		public ZPropertyInfo TextInfo
		{
			get { return GetZPropertyInfo(Schema.Text); }
		}

		public ZBool IncludeShipmentNumber
		{
			get => includeShipmentNumber;
			set
			{
				SetNonPersistentPropertyValue(IncludeShipmentNumberInfo, ref includeShipmentNumber, value);
			}
		}
		ZBool includeShipmentNumber = false;

		public ZPropertyInfo IncludeShipmentNumberInfo
		{
			get { return GetZPropertyInfo(Schema.IncludeShipmentNumber); }
		}

		public ElectronicProcessingChargeDescriptionOverrideLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new ElectronicProcessingChargeDescriptionOverrideLookups(this);
				}

				return lookups;
			}
		}
		ElectronicProcessingChargeDescriptionOverrideLookups lookups;

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Transport, Transport);
			writer.WriteElementString(Schema.Container, Container);
			writer.WriteElementString(Schema.ShipmentType, ShipmentType);
			writer.WriteElementString(Schema.Origin, Origin);
			writer.WriteElementString(Schema.Destination, Destination);
			writer.WriteElementString(Schema.PrefixSuffix, PrefixSuffix);
			writer.WriteElementString(Schema.Text, Text);
			writer.WriteElementString(Schema.IncludeShipmentNumber, IncludeShipmentNumber.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Transport = reader.ReadElementString(Schema.Transport);
			Container = reader.ReadElementString(Schema.Container);
			ShipmentType = reader.ReadElementString(Schema.ShipmentType);
			Origin = reader.ReadElementString(Schema.Origin);
			Destination = reader.ReadElementString(Schema.Destination);
			PrefixSuffix = reader.ReadElementString(Schema.PrefixSuffix);
			Text = reader.ReadElementString(Schema.Text);
			IncludeShipmentNumber = new ZBool(reader.ReadElementString(Schema.IncludeShipmentNumber));
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateTransport();
			ValidateContainer();
			ValidateShipmentType();
			ValidateOrigin();
			ValidateDestination();
			ValidateDescriptionOverridePrefixSuffix();
			ValidateDescriptionOverride();
			ValidateDuplicated();
		}

		void ValidateTransport()
		{
			TransportInfo.ClearAllNotifications();
			IMultilingualString humanReadableName = ResString.GetMultilingualString("19928489-AEB3-4053-B657-77E758964B8F", "Transport");
			MandatoryValidation.CheckEntered(TransportInfo, humanReadableName);
			ListValidation.ErrorIfInvalidCode(TransportInfo, Lookups.Transports, humanReadableName);

			ValidateDuplicated();
		}

		void ValidateContainer()
		{
			ContainerInfo.ClearAllNotifications();
			IMultilingualString humanReadableName = ResString.GetMultilingualString("E456D702-4116-4FB6-BF51-84A8364D97E7", "Container");

			MandatoryValidation.CheckEntered(ContainerInfo, humanReadableName);
			ListValidation.ErrorIfInvalidCode(ContainerInfo, Lookups.Containers, humanReadableName);
			ValidateDuplicated();
		}

		void ValidateShipmentType()
		{
			ShipmentTypeInfo.ClearAllNotifications();
			IMultilingualString humanReadableName = ResString.GetMultilingualString("AF044D81-75BC-4A42-B570-D13145F45BD3", "Shipment Type");

			MandatoryValidation.CheckEntered(ShipmentTypeInfo, humanReadableName);
			ListValidation.ErrorIfInvalidCode(ShipmentTypeInfo, Lookups.ShipmentTypes, humanReadableName);
			ValidateDuplicated();
		}

		void ValidateOrigin()
		{
			OriginInfo.ClearAllNotifications();
			IMultilingualString humanReadableName = ResString.GetMultilingualString("46BA8AB6-E39A-4108-B36C-B8D0E0E8C1BD", "Origin");

			MandatoryValidation.CheckEntered(OriginInfo, humanReadableName);
			ListValidation.ErrorIfInvalidCode(OriginInfo, Lookups.Origins, humanReadableName);
			ValidateOriginDestinationInSameList(OriginInfo);
			ValidateDuplicated();
		}

		void ValidateDestination()
		{
			DestinationInfo.ClearAllNotifications();
			IMultilingualString humanReadableName = ResString.GetMultilingualString("5ADCB1F8-9A26-47A0-88C8-2AD366BE6312", "Destination");

			MandatoryValidation.CheckEntered(DestinationInfo, humanReadableName);
			ListValidation.ErrorIfInvalidCode(DestinationInfo, Lookups.Destinations, humanReadableName);
			ValidateOriginDestinationInSameList(DestinationInfo);
			ValidateDuplicated();
		}

		void ValidateOriginDestinationInSameList(ZPropertyInfo prop)
		{
			if (!prop.HasErrors())
			{
				if ((AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareCurrentCompanyList.ContainsCode(Destination) && AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareJobBranchList.ContainsCode(Origin))
					|| (AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareCurrentCompanyList.ContainsCode(Origin) && AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareJobBranchList.ContainsCode(Destination)))
				{
					prop.AddError(Res.GetString("1D957D4F-649D-43B3-ABD6-3278387FC557", "Both the Origin and Destination Rule Option must be set based on current login company or job header branch value."));
				}
			}
		}

		void ValidateDescriptionOverridePrefixSuffix()
		{
			PrefixSuffixInfo.ClearAllNotifications();
			IMultilingualString humanReadableName = ResString.GetMultilingualString("F61A26F1-8AFA-4B00-868F-1D0243B876CF", "Prefix/Suffix");

			MandatoryValidation.CheckEntered(PrefixSuffixInfo, humanReadableName);
			ListValidation.ErrorIfInvalidCode(PrefixSuffixInfo, Lookups.PrefixSuffix, humanReadableName);
		}

		void ValidateDescriptionOverride()
		{
			TextInfo.ClearAllNotifications();
			IMultilingualString humanReadableName = ResString.GetMultilingualString("DAA8E31C-9BCD-4519-8DB1-F9E7A0334773", "Text");

			MandatoryValidation.CheckEntered(TextInfo, humanReadableName);
		}

		void ValidateDuplicated()
		{
			var errorMessage = Res.GetString("9ED8CAEC-934A-4C1C-9A10-8D6A1A606F24", "This row has been duplicated and must be unique.");
			ClearRowNotificationsContaining(errorMessage);

			if (!HasErrors && CheckDuplicated())
			{
				AddRowError(errorMessage);
			}

			bool CheckDuplicated()
			{
				if (ParentCollection == null)
				{
					return false;
				}

				return ParentCollection.OfType<ElectronicProcessingChargeDescriptionOverride>()
					.Except(new[] { this })
					.Any(item => item.Transport == Transport
						&& item.Container == Container
						&& item.ShipmentType == ShipmentType
						&& item.Origin == Origin
						&& item.Destination == Destination);
			}
		}
	}
}
