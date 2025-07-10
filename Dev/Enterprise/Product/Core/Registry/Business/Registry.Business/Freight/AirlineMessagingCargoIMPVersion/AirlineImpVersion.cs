using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business.Freight.AirlineMessagingCargoIMPVersion
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]

	public class AirlineImpVersion : RegistryBusinessObjectTemplate
	{
		public AirlineImpVersion()
		{
		}

		public AirlineImpVersion(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AirlineImpVersion(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		ZString airlinePrefix;
		[List("AirlinePrefixes")]
		public ZString AirlinePrefix
		{
			get => airlinePrefix;
			set
			{
				SetNonPersistentPropertyValue(AirlinePrefixInfo, ref airlinePrefix, value);
				if (!IsValidationSuspended)
				{
					ValidateAirlinePrefix();
				}
			}
		}

		public void ValidateAirlinePrefix()
		{
			AirlinePrefixInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AirlinePrefixInfo);
			ListValidation.ErrorIfInvalidCode(AirlinePrefixInfo, AirlinePrefixes);
			if (ParentCollection != null && ParentCollection.OfType<AirlineImpVersion>().Any(x => x.AirlinePrefix == AirlinePrefix && x != this))
			{
				AirlinePrefixInfo.AddError(ResString.GetMultilingualString("CBA59980-FDB8-43A9-83D9-63F26D19A6D8",
					"Airline prefix of '{0}' has already been specified.", AirlinePrefix));
			}
		}

		AirlineImpVersionCollection ParentCollection
		{
			get { return (AirlineImpVersionCollection)GetParentCollection(
				this, typeof(AirlineImpVersionCollection)); }
		}

		CodeDescriptionPairList airlinePrefixes;
		public CodeDescriptionPairList AirlinePrefixes
		{
			get
			{
				if (airlinePrefixes == null)
				{
					var zQuery = new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.NotEqual, string.Empty);
					var airlines = CurrentFactory.Load<IRefAirline>(zQuery)?.GroupBy(x => x.RM_EagleAddedAirlinePrefixOrAccountingCode)?.Select(g => g.First());
					airlinePrefixes = new CodeDescriptionPairList();
					if (airlines?.Any() == true)
					{
						foreach (var airline in airlines.OrderBy(x => x.RM_EagleAddedAirlinePrefixOrAccountingCode))
						{
							airlinePrefixes.AddPair(airline.RM_EagleAddedAirlinePrefixOrAccountingCode, airline.HumanReadableName);
						}
					}
				}
				return airlinePrefixes;
			}
		}

		public ZPropertyInfo AirlinePrefixInfo => GetZPropertyInfo(nameof(AirlinePrefix));

		ZString impVersion = (NoResString)"V16";
		[List("ImpVersions")]
		public ZString ImpVersion
		{
			get => impVersion;
			set
			{
				SetNonPersistentPropertyValue(ImpVersionInfo, ref impVersion, value);
				if (!IsValidationSuspended)
				{
					ImpVersionInfo.ClearAllNotifications();
					MandatoryValidation.CheckEntered(ImpVersionInfo);
					ListValidation.ErrorIfInvalidCode(ImpVersionInfo, ImpVersions);
				}
			}
		}

		CodeDescriptionPairList impVersions;
		public CodeDescriptionPairList ImpVersions
		{
			get
			{
				if (impVersions == null)
				{
					impVersions = new CodeDescriptionPairList();
					impVersions.AddPair((NoResString)"V16", ResString.GetMultilingualString("DB943A25-F796-475C-B222-3FD06DBEB52D", "FWB/16 and FHL/4"));
					impVersions.AddPair((NoResString)"V17", ResString.GetMultilingualString("5798518C-3951-4073-B7EA-D7008914165C", "FWB/17 and FHL/5"));
				}
				return impVersions;
			}
		}

		public ZPropertyInfo ImpVersionInfo => GetZPropertyInfo(nameof(ImpVersion));

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AirlineImpVersion(fallbackLevel);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			AirlinePrefix = reader.ReadElementString(nameof(AirlinePrefix));
			ImpVersion = reader.ReadElementString(nameof(ImpVersion));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(nameof(AirlinePrefix), AirlinePrefix);
			writer.WriteElementString(nameof(ImpVersion), ImpVersion);
		}
	}
}
