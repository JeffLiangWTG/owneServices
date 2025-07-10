using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.IL.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.IL.Business.XmlSerializers")]
	public class DCAParameters : RegistryBusinessObjectTemplate
	{
		public DCAParameters()
			: base(RegistryFactory.Instance)
		{
		}

		public DCAParameters(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Properties
		[ChildEditable(true)]
		[List(nameof(PeekWayLookups))]
		[ResourceStringData("C3285F34-EFA9-4172-B55B-184D3D9F2ABE", Caption = "Peek Way")]
		public ZString PeekWay
		{
			get
			{
				return peekWay;
			}
			set
			{
				if (PeekWay != value)
				{
					SetNonPersistentPropertyValue(PeekWayInfo, ref peekWay, value);

					AllServices = (PeekWay == PeekWayList.Codes._2);
				}
			}
		}
		ZString peekWay = PeekWayList.Codes._3;

		public ZPropertyInfo PeekWayInfo => GetZPropertyInfo(nameof(PeekWay));

		public CodeDescriptionPairList PeekWayLookups => CurrentFactory.GetCachedValue<PeekWayList>();

		[ChildEditable(true)]
		[ReadOnly(true)]
		[ResourceStringData("61EF4A02-01C6-468E-9DBA-D4CDA0C9CBFE", Caption = "All Services")]
		public ZBool AllServices
		{
			get
			{
				return allServices;
			}
			set
			{
				if (AllServices != value)
				{
					SetNonPersistentPropertyValue(AllServicesInfo, ref allServices, value);

					if (AllServices)
					{
						Services.RemoveAndDeleteAll();
					}
					else
					{
						AddDefaultServices();
					}
				}
			}
		}
		ZBool allServices = Schema.DefaultAllServices;

		public ZPropertyInfo AllServicesInfo => GetZPropertyInfo(nameof(AllServices));

		[XmlIgnore]
		[ChildEditable(true)]
		[ReadOnlyMember(nameof(IsSpecificServicesReadOnly))]
		[ResourceStringData("FF7757FC-E918-421D-9406-3ED9CE25B99F", Caption = "Specific Services")]
		public ZBool SpecificServices
		{
			get
			{
				return !AllServices;
			}
			set
			{
				AllServices = !value;
			}
		}
		public ZPropertyInfo SpecificServicesInfo => GetZPropertyInfo(nameof(AllServices));

		protected bool IsSpecificServicesReadOnly => peekWay == PeekWayList.Codes._2;

		public DCAServicesCollection Services
		{
			get
			{
				if (services == null)
				{
					services = new DCAServicesCollection(this, CurrentFallbackLevel, CurrentFactory);
					RegisterEditableChildObject(services);
				}

				return services;
			}
		}
		DCAServicesCollection services;

		ZXmlSerializer ServicesCollectionSerialiser => servicesSerialiser ?? (servicesSerialiser = ZXmlSerializer.New(typeof(DCAServicesCollection)));
		ZXmlSerializer servicesSerialiser;

		void CloneServicesFrom(DCAParameters parameter, DCAServicesCollection collection)
		{
			services = collection.Clone(parameter, CurrentFallbackLevel, CurrentFactory);
			RegisterEditableChildObject(services);
		}

		[ChildEditable(true)]
		[ResourceStringData("3807D14D-592A-40AD-9396-B0E8217F738A", Caption = "Maximum Messages Per Iteration")]
		public ZInt MaxMessagesPerIteration
		{
			get => maxMessagesPerIteration;
			set
			{
				SetNonPersistentPropertyValue(MaxMessagesPerIterationInfo, ref maxMessagesPerIteration, value);
				if (!IsValidationSuspended)
				{
					ValidateMaxMessagesPerIteration();
				}
				MaxMessagesPerIterationInfo.RefreshBinding();
			}
		}
		ZInt maxMessagesPerIteration = Schema.DefaultMaxMessagesPerIteration;

		public ZPropertyInfo MaxMessagesPerIterationInfo => GetZPropertyInfo(nameof(MaxMessagesPerIteration));

		#endregion

		internal void AddDefaultServices()
		{
			var defaultNames = new[]
			{
				Constants.CustomsServiceName.DeliveryOrderResponse,
				Constants.CustomsServiceName.GatePassMovementResponse,
				Constants.CustomsServiceName.ImportDeclarationResponse,
				Constants.CustomsServiceName.RequiredDocumentResponse,
				Constants.CustomsServiceName.RequiredDocumentVerificationDecisionResponse,
			};

			var existingServicesNames = Services.Cast<DCAService>().Select(x => x.Name);

			foreach (var defaultName in defaultNames)
			{
				if (!existingServicesNames.Contains(defaultName))
				{
					var service = Services.AddNew();
					using (service.GetValidationSuspender())
					{
						service.Name = defaultName;
					}
				}
			}
		}

		public new DCAParameters Clone() => (DCAParameters)GetClone(null, null);

		#region Xml Serialisation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new DCAParameters(fallbackLevel, factory);

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var parameter = clone as DCAParameters;
			parameter?.CloneServicesFrom(parameter, Services);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			var xmlReader = reader.Reader;

			do
			{
				switch (xmlReader.Name)
				{
					case Schema.PeekWay:
						PeekWay = xmlReader.ReadElementContentAsString();
						break;
					case Schema.AllServices:
						if (ZBool.TryParse(xmlReader.ReadElementContentAsString(), out var allServices))
						{
							AllServices = allServices;
						}
						break;
					case Schema.MaxMessagesPerIteration:
						if (ZInt.TryParse(xmlReader.ReadElementContentAsString(), out var maxMessagesPerIteration))
						{
							MaxMessagesPerIteration = maxMessagesPerIteration;
						}
						break;
					case Schema.Services:
						var newServices = (DCAServicesCollection)ServicesCollectionSerialiser.Deserialize(reader);
						CloneServicesFrom(this, newServices);
						break;
					default:
						xmlReader.Skip();
						break;
				}
			} while (!xmlReader.EOF);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.PeekWay, PeekWay);
			writer.WriteElementString(Schema.AllServices, AllServices.ToString());
			writer.WriteElementString(Schema.MaxMessagesPerIteration, MaxMessagesPerIteration.ToString());
			ServicesCollectionSerialiser.Serialize(writer, services);
		}

		#endregion

		#region Comparison

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;

				hash = hash * 23 + PeekWay.GetHashCode();
				hash = hash * 23 + AllServices.GetHashCode();
				hash = hash * 23 + MaxMessagesPerIteration.GetHashCode();

				if (Services != null)
				{
					foreach (var service in Services)
					{
						hash = hash * 23 + (service != null ? service.GetHashCode() : 0);
					}
				}

				return hash;
			}
		}

		public override bool Equals(object obj)
		{
			var other = obj as DCAParameters;
			if (other == null)
			{
				return false;
			}

			return other.PeekWay == PeekWay
				&& other.AllServices == AllServices
				&& other.MaxMessagesPerIteration == MaxMessagesPerIteration
				&& other.Services.SequenceEqual(Services);
		}

		#endregion Comparison

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateMaxMessagesPerIteration();
			ValidatePeekWay();
		}

		void ValidateMaxMessagesPerIteration()
		{
			MaxMessagesPerIterationInfo.ClearAllNotifications();
			if (MaxMessagesPerIteration < DCAParameters.Schema.MaxMessagesPerIterationMinValue)
			{
				MaxMessagesPerIterationInfo.AddError(Constants.DCAParametersValidation.MaxMessagesPerIterationMinValue);
			}
			else if (MaxMessagesPerIteration > DCAParameters.Schema.MaxMessagesPerIterationMaxValue)
			{
				MaxMessagesPerIterationInfo.AddError(Constants.DCAParametersValidation.MaxMessagesPerIterationMaxValue);
			}
		}

		void ValidatePeekWay()
		{
			PeekWayInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCodeOrEmpty(PeekWayInfo);
		}

		internal static class Schema
		{
			public const string PeekWay = "PeekWay";
			public const string AllServices = "AllServices";
			public const string Services = "Services";
			public const string ServiceName = "Name";
			public const string MaxMessagesPerIteration = "MaxMessagesPerIteration";
			public const string DefaultPeekWay = "3";
			public const bool DefaultAllServices = false;
			public const int DefaultMaxMessagesPerIteration = 500;
			public const int MaxMessagesPerIterationMinValue = 100;
			public const int MaxMessagesPerIterationMaxValue = 999;
		}
	}
}
