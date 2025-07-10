using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import934_5SMFormD : IImport934_5SMFormD
	{
		public string ValuationMethod { get; set; }
		public string ValuationSupportingDocument1 { get; set; }
		public string ValuationSupportingDocument2 { get; set; }
		public ValueDeclarationCode[] UseCodes { get; set; }
		public ValueDeclarationCode[] GoodsPricingBasis { get; set; }

		ZString IImport934_5SMFormD.ValuationMethod => ValuationMethod;
		ZString IImport934_5SMFormD.ValuationSupportingDocument1 => ValuationSupportingDocument1;
		ZString IImport934_5SMFormD.ValuationSupportingDocument2 => ValuationSupportingDocument2;
		IEnumerable<IValueDeclarationCode> IImport934_5SMFormD.UseCodes => UseCodes;
		IEnumerable<IValueDeclarationCode> IImport934_5SMFormD.GoodsPricingBasis => GoodsPricingBasis;
	}
}
