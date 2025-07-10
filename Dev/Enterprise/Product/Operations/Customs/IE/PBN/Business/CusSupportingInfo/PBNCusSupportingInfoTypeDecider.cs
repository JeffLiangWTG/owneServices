using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class PBNCusSupportingInfoTypeDecider : ApplicationSpecificTypeDecider
	{
		protected override IEnumerable<ApplicationSpecificType> ApplicationSpecificTypesCore
		{
			get
			{
				yield return new ApplicationSpecificType(IEPBNDeclarationTypes.Codes.NCTS, () => typeof(PBNTransitDeclarationItem));
				foreach (var code in IEPBNDeclarationTypes.CustomsDeclarationItemCodes)
				{
					yield return new ApplicationSpecificType(code, () => typeof(PBNCustomsDeclarationItem));
				}
			}
		}
	}
}
