using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	public class UniversalDataObjectWriterHelper : EU.DataTransfer.Universal.UniversalDataObjectWriterHelper
	{
		public UniversalDataObjectWriterHelper(BusinessObjectFactory factory, ZString countryCode) : base(factory, countryCode)
		{
		}

		protected override IEnumerable<KeyValuePair<ZString, IZType>> GetOldSupportingDocumentAddInfo(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument supportingDocument)
		{
			foreach (var pair in base.GetOldSupportingDocumentAddInfo(supportingDocument))
			{
				yield return pair;
			}
			var gbSupportingDocument = (SupportingDocument)supportingDocument;
			yield return new KeyValuePair<ZString, IZType>(Enterprise.Customs.EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Part, supportingDocument.CSI_SubType);
			yield return new KeyValuePair<ZString, IZType>(Enterprise.Customs.EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Reason, supportingDocument.CSI_Description);
			yield return new KeyValuePair<ZString, IZType>(Enterprise.Customs.EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Actions, gbSupportingDocument.CSI_Actions);
			yield return new KeyValuePair<ZString, IZType>(Enterprise.Customs.EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Availability, gbSupportingDocument.CSI_Availability);
		}
	}
}
