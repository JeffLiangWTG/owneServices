using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CusSupplyChainActorReferenceProvider : EU.Business.Declaration.CusSupplyChainActorReferenceProvider
	{
		protected CusSupplyChainActorReferenceProvider(ZString dataGroupingCode) : base(dataGroupingCode)
		{
		}

		protected override ZString OverwrittenReferenceColumnCaptionCore => Res.GetString("5bed6b54-17b9-41a3-87cc-8007f69f0043", "Identification (TCUI/EORI)");
	}
}
