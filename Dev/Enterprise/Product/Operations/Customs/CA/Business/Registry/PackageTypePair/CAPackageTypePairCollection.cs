using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Customs.CA.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.CA.Business.XmlSerializers")]
	public class CAPackageTypePairCollection : PackageTypePairCollection<CAPackageTypePair>
	{
		#region Package Types

		public static class PackType
		{
			#region Customs

			public static class Customs
			{
				public const string BG = "BG";
				public const string BL = "BL";
				public const string BN = "BN";
				public const string BK = "BK";
				public const string BO = "BO";
				public const string BX = "BX";
				public const string BE = "BE";
				public const string CT = "CT";
				public const string CS = "CS";
				public const string CL = "CL";
				public const string CR = "CR";
				public const string CY = "CY";
				public const string DR = "DR";
				public const string EN = "EN";
				public const string KG = "KG";
				public const string PK = "PK";
				public const string PL = "PL";
				public const string RL = "RL";
				public const string RO = "RO";
				public const string ST = "ST";
				public const string TU = "TU";
				public const string EA = "EA";
			}

			#endregion

			#region Freight

			public static class Freight
			{
				public const string BAG = "BAG";
				public const string BLC = "BLC";
				public const string BLU = "BLU";
				public const string BSK = "BSK";
				public const string BOT = "BOT";
				public const string BOX = "BOX";
				public const string BND = "BND";
				public const string CTN = "CTN";
				public const string CAS = "CAS";
				public const string COI = "COI";
				public const string CRT = "CRT";
				public const string CYL = "CYL";
				public const string DRM = "DRM";
				public const string ENV = "ENV";
				public const string KEG = "KEG";
				public const string PKG = "PKG";
				public const string PAI = "PAI";
				public const string REL = "REL";
				public const string RLL = "RLL";
				public const string SHT = "SHT";
				public const string TUB = "TUB";
				public const string EA = "EA";
				public const string MIX = "MIX";
				public const string PAC = "PAC";
				public const string PC = "PC";
				public const string PCE = "PCE";
				public const string PCS = "PCS";
				public const string UNT = "UNT";
			}

			#endregion

		}

		#endregion

		protected override void AddDefaultValuesCore()
		{
			base.AddDefaultValuesCore();
			AddNew(PackType.Customs.BG, PackType.Freight.BAG);
			AddNew(PackType.Customs.BL, PackType.Freight.BLC);
			AddNew(PackType.Customs.BN, PackType.Freight.BLU);
			AddNew(PackType.Customs.BE, PackType.Freight.BND);
			AddNew(PackType.Customs.BO, PackType.Freight.BOT);
			AddNew(PackType.Customs.BX, PackType.Freight.BOX);
			AddNew(PackType.Customs.BK, PackType.Freight.BSK);
			AddNew(PackType.Customs.CS, PackType.Freight.CAS);
			AddNew(PackType.Customs.CL, PackType.Freight.COI);
			AddNew(PackType.Customs.CR, PackType.Freight.CRT);
			AddNew(PackType.Customs.CT, PackType.Freight.CTN);
			AddNew(PackType.Customs.CY, PackType.Freight.CYL);
			AddNew(PackType.Customs.DR, PackType.Freight.DRM);
			AddNew(PackType.Customs.PK, PackType.Freight.EA);
			AddNew(PackType.Customs.EN, PackType.Freight.ENV);
			AddNew(PackType.Customs.KG, PackType.Freight.KEG);
			AddNew(PackType.Customs.EA, PackType.Freight.MIX);
			AddNew(PackType.Customs.PK, PackType.Freight.PAC);
			AddNew(PackType.Customs.PL, PackType.Freight.PAI);
			AddNew(PackType.Customs.EA, PackType.Freight.PC);
			AddNew(PackType.Customs.EA, PackType.Freight.PCE);
			AddNew(PackType.Customs.EA, PackType.Freight.PCS);
			AddNew(PackType.Customs.PK, PackType.Freight.PKG);
			AddNew(PackType.Customs.RL, PackType.Freight.REL);
			AddNew(PackType.Customs.RO, PackType.Freight.RLL);
			AddNew(PackType.Customs.ST, PackType.Freight.SHT);
			AddNew(PackType.Customs.TU, PackType.Freight.TUB);
			AddNew(PackType.Customs.EA, PackType.Freight.UNT);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(ZArchitecture.Environment.FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new CAPackageTypePairCollection();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new CAPackageTypePair();
	}
}
