using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class UniversalDataObjectWriterHelper : Customs.DataTransfer.Universal.UniversalDataObjectWriterHelper
	{
		public UniversalDataObjectWriterHelper(BusinessObjectFactory factory, ZString countryCode) : base(factory, countryCode)
		{
		}

		public override ZString? GetReferencedEntityDescriptionForCusCodeData(CusCodeData cusCodeData) => (cusCodeData as EuOfficeCode)?.CY_OfficeDescription;

		protected override IEnumerable<UniversalDataBuss.DataObjects.Universal.Customs.CustomsReference> GetAdditionalCustomsReferenceDataForCore(BusinessObject bizObj, IDataWritingManager writeManager, string dataContext)
		{
			var result = new CustomsReferenceCollectionCreatorFromCusReference(this, bizObj, writeManager, dataContext).CreateCollection()
						.Union(new CustomsReferenceCollectionCreatorFromCusAuthorizationUsage(this, bizObj, writeManager, dataContext).CreateCollection());

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		internal protected virtual IEnumerable<KeyValuePair<ZString, IZType>> GetOldSupportingDocumentAddInfo(SupportingDocument supportingDocument)
		{
			yield return new KeyValuePair<ZString, IZType>(Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Qty, supportingDocument.CSI_Quantity);
			yield return new KeyValuePair<ZString, IZType>(Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Reference, supportingDocument.CSI_ReferenceNumber);
			yield return new KeyValuePair<ZString, IZType>(Constants.OldCusAddInfoTypes.SupportingDocument.Fields.TypeCode, supportingDocument.CSI_Code);
		}

		#region Dv1DetailsLink

		public ZInt? GetAllocatedDv1DetailsLink(ZGuid sourcePK)
		{
			ZInt? result = null;
			if (sourcePK.IsValid && dv1DetailsLinkMap.ContainsKey(sourcePK))
			{
				result = dv1DetailsLinkMap[sourcePK];
			}
			return result;
		}

		public ZInt? AllocateDv1DetailsLink(ZGuid sourcePK)
		{
			ZInt? result = null;
			if (sourcePK.IsValid)
			{
				if (dv1DetailsLinkMap.ContainsKey(sourcePK))
				{
					result = dv1DetailsLinkMap[sourcePK];
				}
				else
				{
					result = dv1DetailsLinkMap.Count + 1;
					dv1DetailsLinkMap[sourcePK] = (int)result;
				}
			}
			return result;
		}
		readonly Dictionary<ZGuid, int> dv1DetailsLinkMap = new Dictionary<ZGuid, int>();

		#endregion
	}
}
