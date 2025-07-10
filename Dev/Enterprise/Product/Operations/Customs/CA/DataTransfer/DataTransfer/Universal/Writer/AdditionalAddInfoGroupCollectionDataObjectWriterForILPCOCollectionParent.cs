using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class AdditionalAddInfoGroupCollectionDataObjectWriterForILPCOCollectionParent : IAdditionalAddInfoGroupCollectionDataObjectWriter
	{
		public AdditionalAddInfoGroupCollectionDataObjectWriterForILPCOCollectionParent(ILPCOCollectionParent lpcoCollectionParent, IDataWritingManager writeManager)
		{
			this.lpcoCollectionParent = Argument.NotNull(lpcoCollectionParent, "LPCOCollectionParent");
			this.writeManager = Argument.NotNull(writeManager, "WriteManager");
		}
		readonly ILPCOCollectionParent lpcoCollectionParent;
		readonly IDataWritingManager writeManager;

		public IEnumerable<AddInfoGroup> CreateCollection()
		{
			foreach (var lpco in lpcoCollectionParent.LPCOViews.Cast<LPCOView>())
			{
				yield return CreateAddInfoGroup(lpco);
			}
		}

		public AddInfoGroup CreateAddInfoGroup(LPCOView lpcoView)
		{
			var addInfoGroup = new AddInfoGroup(writeManager.WriterStrategy)
			{
				Type = new CodeDescriptionPair() { Code = Constants.AddInfoKeys.CusCALPCO.CusAddInfoType, Description = Constants.AddInfoKeys.CusCALPCO.Description }
			};
			var addInfoCollection = GetAddInfoCollection(lpcoView);
			if (addInfoCollection.Any())
			{
				addInfoGroup.AddInfoCollection = addInfoCollection;
			}
			AddInfoGroupOrganizationAddressCollectionUpdator.UpdateForLPCO(addInfoGroup, lpcoView, writeManager);
			return addInfoGroup;
		}

		List<UniversalDataBuss.DataObjects.Universal.AddInfo> GetAddInfoCollection(LPCOView lpco)
		{
			var result = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>();
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.Type, lpco.CLP_Type, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.RefNo, lpco.CLP_RefNo, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.SecondaryRefNo, lpco.CLP_SecondaryRefNo, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.DIFRefNumberOrLocation, lpco.CLP_DIFRefNumberOrLocation, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.LPCOStartDate, lpco.CLP_StartDate, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.LPCOEndDate, lpco.CLP_EndDate, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.LPCOIssueDate, lpco.CLP_IssueDate, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.CountryOfIssuance, lpco.CLP_RN_NKIssuanceCountryCode, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.CountryOfOrigin, lpco.CLP_RN_NKOriginCountryCode, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.AuthorizationCountry, lpco.CLP_RN_NKAuthorizationCountry, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.IsMixedCountryOfOrigin, lpco.CLP_IsMixedCountryOfOrigin, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.CommodityTypeCode, lpco.CLP_CommodityTypeCode, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.Qty, lpco.CLP_AlternativeQuotaQuantity, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.UQ, lpco.CLP_AlternativeQuotaUQ, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.LPCOHolderType, lpco.CLP_HolderType, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.IsHolderOverridden, lpco.CLP_IsHolderOverridden, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.LPCOHolderName, lpco.CLP_HolderName, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.AuthorizedPartyContactName, lpco.CLP_HolderContactName, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.AuthorizedPartyContactEmail, lpco.CLP_HolderContactEmail, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.AuthorizedPartyContactPhone, lpco.CLP_HolderContactPhone, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.LPCOApplicant, lpco.CLP_ApplicantType, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.IsApplicantOverridden, lpco.CLP_IsApplicantOverridden, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.LPCOApplicantName, lpco.CLP_ApplicantName, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.ApplicantContactEmail, lpco.CLP_ApplicantContactEmail, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.ApplicantContactName, lpco.CLP_ApplicantContactName, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.ApplicantContactPhone, lpco.CLP_ApplicantContactPhone, result);
			CreateAddInfo(Constants.AddInfoKeys.CusCALPCO.RN_NKSmeltAndPourCountryCode, lpco.CLP_RN_NKSmeltAndPourCountryCode, result);
			return result;
		}

		static void CreateAddInfo(ZString key, IZType value, List<UniversalDataBuss.DataObjects.Universal.AddInfo> addInfoCollection)
		{
			UniversalDataBuss.DataObjects.Universal.AddInfo addInfo = null;
			if (value is ZString zString && !zString.IsEmpty)
			{
				addInfo = new UniversalDataBuss.DataObjects.Universal.AddInfo() { Key = key, Value = zString };
			}
			else if (value is ZDate zDate && !zDate.IsEmpty)
			{
				addInfo = new UniversalDataBuss.DataObjects.Universal.AddInfo() { Key = key, Value = zDate.ToString("yyyy-MM-dd HH:mm:ss.fff") };
			}
			else if (value is ZDateTime zDateTime && !zDateTime.IsEmpty)
			{
				addInfo = new UniversalDataBuss.DataObjects.Universal.AddInfo() { Key = key, Value = zDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") };
			}
			else if (value is ZDecimal zDecimal && !zDecimal.IsDefault)
			{
				addInfo = new UniversalDataBuss.DataObjects.Universal.AddInfo() { Key = key, Value = zDecimal.ToString() };
			}
			else if (value is ZInt zInt && !zInt.IsDefault)
			{
				addInfo = new UniversalDataBuss.DataObjects.Universal.AddInfo() { Key = key, Value = zInt.ToString() };
			}
			else if (value is ZBool zBool && !zBool.IsDefault)
			{
				addInfo = new UniversalDataBuss.DataObjects.Universal.AddInfo() { Key = key, Value = zBool.ToString() };
			}

			if (addInfo != null)
			{
				addInfoCollection.Add(addInfo);
			}
		}
	}
}
