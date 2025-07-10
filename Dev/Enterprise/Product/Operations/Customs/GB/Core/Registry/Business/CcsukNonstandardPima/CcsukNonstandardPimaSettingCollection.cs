using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.GB.Registry.XmlSerializers")]
	public class CcsukNonstandardPimaSettingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CcsukNonstandardPimaSettingCollection()
			: base()
		{
		}

		public CcsukNonstandardPimaSettingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new CcsukNonstandardPimaSetting this[int i]
		{
			get { return (CcsukNonstandardPimaSetting)Elements[i]; }
		}

		public new CcsukNonstandardPimaSetting AddNew()
		{
			return (CcsukNonstandardPimaSetting)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CcsukNonstandardPimaSettingCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CcsukNonstandardPimaSetting(CurrentFallbackLevel, CurrentFactory);
		}

		public CcsukNonstandardPimaSettingCollection GetDefaultValues()
		{
			var defaultCollection = new CcsukNonstandardPimaSettingCollection(CurrentFallbackLevel, CurrentFactory);
			defaultCollection.Add(new CcsukNonstandardPimaSetting("LHRBAC", NonstandardPimaMessageTypeList.Codes.BTH, "CUKAIR01LONFMBA", CurrentFactory));
			defaultCollection.Add(new CcsukNonstandardPimaSetting("STNBAC", NonstandardPimaMessageTypeList.Codes.BTH, "CUKAIR01LONFMBA", CurrentFactory));
			defaultCollection.Add(new CcsukNonstandardPimaSetting("LHRAAS", NonstandardPimaMessageTypeList.Codes.BTH, "CUKAIR01HDQFMAA", CurrentFactory));
			defaultCollection.Add(new CcsukNonstandardPimaSetting("LHRFDX", NonstandardPimaMessageTypeList.Codes.BTH, "CUKAIR01HDQFMFX", CurrentFactory));
			defaultCollection.Add(new CcsukNonstandardPimaSetting("STNFDS", NonstandardPimaMessageTypeList.Codes.BTH, "CUKAIR01HDQFMFX", CurrentFactory));
			defaultCollection.Add(new CcsukNonstandardPimaSetting("LHRCSS", NonstandardPimaMessageTypeList.Codes.BTH, "CUKAIR01LONCS8X", CurrentFactory));
			defaultCollection.Add(new CcsukNonstandardPimaSetting("LHRHCS", NonstandardPimaMessageTypeList.Codes.BTH, "CUKAIR01LHRCS8X", CurrentFactory));
			defaultCollection.Add(new CcsukNonstandardPimaSetting("MANHCS", NonstandardPimaMessageTypeList.Codes.BTH, "CUKAIR01MANCS8X", CurrentFactory));
			defaultCollection.Add(new CcsukNonstandardPimaSetting("LGWHCS", NonstandardPimaMessageTypeList.Codes.BTH, "CUKAIR01LGWCS8X", CurrentFactory));
			defaultCollection.Add(new CcsukNonstandardPimaSetting("LHRHPS", NonstandardPimaMessageTypeList.Codes.BTH, "CUKAIR01LONSP8X", CurrentFactory));
			defaultCollection.Add(new CcsukNonstandardPimaSetting("LHRUAS", NonstandardPimaMessageTypeList.Codes.BTH, "CUKAIR01CHICBUA", CurrentFactory));
			defaultCollection.Add(new CcsukNonstandardPimaSetting("LHRSLS", NonstandardPimaMessageTypeList.Codes.BTH, "CUKAIR01LHRSP8X", CurrentFactory));
			defaultCollection.Add(new CcsukNonstandardPimaSetting("LHRJAS", NonstandardPimaMessageTypeList.Codes.FRN, "CUKAIR01TYOFMJL", CurrentFactory));// LhrJas is excluded frm FRD ....
			defaultCollection.Add(new CcsukNonstandardPimaSetting("MANSLS", NonstandardPimaMessageTypeList.Codes.BTH, "CUKAIR01MANCS8X", CurrentFactory));
			return defaultCollection;
		}
	}

	public static class CcsukNonstandardPimaSettingCollectionExtensions
	{
		public static string ConvertAirportAndShedForMessageType(this CcsukNonstandardPimaSettingCollection col, string airportAndShed, string messageType, ZString defaultValueIfNoMapFound)
		{
			var setting = (from CcsukNonstandardPimaSetting c in col
						   where c.AirportAndShed == airportAndShed
								   && (c.MessageType == NonstandardPimaMessageTypeList.Codes.BTH || c.MessageType == messageType)
						   select c).FirstOrDefault();
			return setting != null ? setting.TypeBPima : defaultValueIfNoMapFound;
		}
	}
}
