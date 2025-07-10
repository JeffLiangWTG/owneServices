using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class PBNTransitDeclarationItem : PBNReferenceItem
	{
		public PBNTransitDeclarationItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override CusSupportingInfoLookups GetNewLookups() => new PBNTransitDeclarationItemLookups(this);

		protected override ZString HumanReadableNameCore => Res.GetString("57AC5F1D-803F-4B22-B9E0-A5D0F78C26E4", "Transit Reference");

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Code = IEPBNDeclarationTypes.Codes.NCTS;
		}
	}
}
