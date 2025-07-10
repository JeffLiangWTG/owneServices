using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business.Customs.US
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class USPackageTypePairCollection : PackageTypePairCollection<USPackageTypePair>
	{
		protected override void AddDefaultValuesCore()
		{
			base.AddDefaultValuesCore();
			AddNew(PackType.Customs.BG, PackType.Freight.BAG);
			AddNew(PackType.Customs.BL, PackType.Freight.BLC);
			AddNew(PackType.Customs.BN, PackType.Freight.BLU);
			AddNew(PackType.Customs.BE, PackType.Freight.BND);
			AddNew(PackType.Customs.BX, PackType.Freight.BOX);
			AddNew(PackType.Customs.BK, PackType.Freight.BSK);
			AddNew(PackType.Customs.CS, PackType.Freight.CAS);
			AddNew(PackType.Customs.CON, PackType.Freight.CNT);
			AddNew(PackType.Customs.CL, PackType.Freight.COI);
			AddNew(PackType.Customs.CR, PackType.Freight.CRT);
			AddNew(PackType.Customs.CT, PackType.Freight.CTN);
			AddNew(PackType.Customs.CY, PackType.Freight.CYL);
			AddNew(PackType.Customs.DR, PackType.Freight.DRM);
			AddNew(PackType.Customs.EN, PackType.Freight.ENV);
			AddNew(PackType.Customs.KG, PackType.Freight.KEG);
			AddNew(PackType.Customs.PL, PackType.Freight.PAI);
			AddNew(PackType.Customs.PK, PackType.Freight.PKG);
			AddNew(PackType.Customs.PAL, PackType.Freight.PLT);
			AddNew(PackType.Customs.RL, PackType.Freight.REL);
			AddNew(PackType.Customs.RO, PackType.Freight.RLL);
			AddNew(PackType.Customs.ST, PackType.Freight.SHT);
			AddNew(PackType.Customs.TU, PackType.Freight.TUB);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(ZArchitecture.Environment.FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new USPackageTypePairCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			if (CurrentFactory == null)
			{
				throw new Exception("What the???");
			}
			return new USPackageTypePair();
		}
	}
}
