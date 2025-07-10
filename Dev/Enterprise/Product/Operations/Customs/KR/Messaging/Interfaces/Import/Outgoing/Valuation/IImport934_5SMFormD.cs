using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport934_5SMFormD
	{
		ZString ValuationMethod { get; }
		ZString ValuationSupportingDocument1 { get; }
		ZString ValuationSupportingDocument2 { get; }
		IEnumerable<IValueDeclarationCode> UseCodes { get; }
		IEnumerable<IValueDeclarationCode> GoodsPricingBasis { get; }
	}
}
