
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class SupportingDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentLookups
	{
		public SupportingDocumentLookups(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument parent) : base(parent)
		{
		}

		protected override ZString UnitOfQuantityListDataGroupingCode => Core.Constants.CountryCodes.Ireland;
	}
}
