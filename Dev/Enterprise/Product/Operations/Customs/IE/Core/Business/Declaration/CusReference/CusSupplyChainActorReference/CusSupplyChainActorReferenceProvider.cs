using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class CusSupplyChainActorReferenceProvider : EU.Business.Declaration.CusSupplyChainActorReferenceProvider
	{
		protected CusSupplyChainActorReferenceProvider(ZString dataGroupingCode) : base(dataGroupingCode)
		{
		}

		protected override EU.Business.Declaration.CusSupplyChainActorReferenceValidation GetNewValidationCore(CusSupplyChainActorReference reference) => new CusSupplyChainActorReferenceValidation(reference);

		protected override ZString OverwrittenReferenceColumnCaptionCore => Res.GetString("7317326E-3DDF-4DC4-8131-188B7FE9CA0A", "Identification (TCUI/EORI)");

		protected override ZBool ReferenceColumnCasingToUpperCore => ZBool.True;
	}
}
