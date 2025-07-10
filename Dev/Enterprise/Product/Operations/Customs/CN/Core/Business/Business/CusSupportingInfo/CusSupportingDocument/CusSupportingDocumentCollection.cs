using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class CusSupportingDocumentCollection : Customs.Business.CusSupportingInfoCollection<CusSupportingDocument>
	{
		public CusSupportingDocumentCollection(BusinessObject parent)
			: base(parent, Constants.CusSupportingInfoTypes.CusSupportingDocument)
		{
		}

		public ZBool IsDocumentProvided(ZString documentType)
		{
			return this.OfType<CusSupportingDocument>().Any(x => x.CSI_Code == documentType && !x.CSI_ReferenceNumber.IsEmpty);
		}

		public CusSupportingDocument AddNew(ZString code, ZString documentNumber)
		{
			var result = AddNew();
			result.CSI_Code = code;
			result.CSI_ReferenceNumber = documentNumber;
			return result;
		}
	}
}
