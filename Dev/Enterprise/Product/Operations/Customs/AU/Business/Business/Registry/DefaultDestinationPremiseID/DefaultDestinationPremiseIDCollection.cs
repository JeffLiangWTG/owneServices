using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.AU.Declaration.Business.XmlSerializers")]
	public class DefaultDestinationPremiseIDCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DefaultDestinationPremiseIDCollection()
		{
		}

		public DefaultDestinationPremiseIDCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZString GetPremiseIDMatchingDischargePort(ZString flightNo, ZString dischargePort)
		{
			return (from DefaultDestinationPremiseID premiseID in this
					where flightNo.StartsWith(premiseID.AirlineCode) && premiseID.PortOfDischarge == dischargePort
					&& premiseID.UseDischargePort
					select premiseID.PremiseID).FirstOrDefault();
		}

		public ZString GetPremiseIDMatchingDestinationPort(ZString flightNo, ZString destination)
		{
			return (from DefaultDestinationPremiseID premiseID in this
					where flightNo.StartsWith(premiseID.AirlineCode) && premiseID.PortOfDischarge == destination
					&& !premiseID.UseDischargePort
					select premiseID.PremiseID).FirstOrDefault();
		}

		public new DefaultDestinationPremiseID this[int index]
		{
			get { return (DefaultDestinationPremiseID)Elements[index]; }
		}

		public new DefaultDestinationPremiseID AddNew()
		{
			return (DefaultDestinationPremiseID)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultDestinationPremiseIDCollection(factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DefaultDestinationPremiseID(CurrentFactory);
		}
	}
}
