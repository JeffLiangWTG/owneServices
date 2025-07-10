using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class PBNCustomsDeclarationItem : PBNReferenceItem
	{
		public PBNCustomsDeclarationItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("FEDF8A13-CAF2-4CED-9420-EDE8D65A587A", "Customs Reference");

		protected override CusSupportingInfoLookups GetNewLookups() => new PBNCustomsDeclarationItemLookups(this);
	}
}
