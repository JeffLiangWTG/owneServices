using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEJobComInvoiceHeaderLookups : JobComInvoiceHeaderLookups
	{
		public DeltaIEJobComInvoiceHeaderLookups(JobComInvoiceHeader parent) : base(parent)
		{
		}

		public override ICodeDescriptionPairList ValuationCodeList => Factory.GetCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, Parent.JobDeclaration?.JE_MessageType ?? ZString.Empty);
	}
}
