using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class TemporaryStorageHeaderValueSetStrategy : EU.Business.CusTempStorage.TemporaryStorageHeaderValueSetStrategy
	{
		public TemporaryStorageHeaderValueSetStrategy(TemporaryStorageHeader header) : base(header)
		{
		}

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValueSetCore(valueThatHasChanged, oldValue);
			switch (valueThatHasChanged.Name)
			{
				case TemporaryStorageHeader.Schema.AMA_OA_Declarant:
				case TemporaryStorageHeader.Schema.AMA_OA_Representative:
					ChangeLocationOfGoods();
					break;
			}
		}

		void ChangeLocationOfGoods()
		{
			var zQuery = new ZQuery(CusPermitHeaderSchema.CPH_Type, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage)
				.AddToFilter(CusPermitHeaderSchema.CPH_OA_AppliesTo, Header.AMA_OA_Declarant_ZAddress.AddressFK);
			switch (Header.AMA_ManifestType)
			{
				case FRConstants.TemporaryStorage.AppCodeIST:
					zQuery.AddToFilter(CusPermitHeaderSchema.CPH_Number, SQLComparisonOperator.StartsWith, "FRTST");
					break;
				case FRConstants.TemporaryStorage.AppCodeLAD:
					zQuery.AddToFilter(CusPermitHeaderSchema.CPH_Number, SQLComparisonOperator.StartsWith, "LADT");
					break;
			}
			if (Header.Declarant != null)
			{
				var cusPermitHeaders = Header.Factory.Load<CusAuthorisationHeader>(new ZQuery(zQuery).AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, Header.Declarant.Header.PK));
				SetLocationOfGoodsAndReturnIfCountIsOne(cusPermitHeaders);
			}
			if (Header.Representative != null)
			{
				var cusPermitHeaders = Header.Factory.Load<CusAuthorisationHeader>(new ZQuery(zQuery).AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, Header.Representative.Header.PK));
				SetLocationOfGoodsAndReturnIfCountIsOne(cusPermitHeaders);
			}

			void SetLocationOfGoodsAndReturnIfCountIsOne(CusAuthorisationHeader[] cusPermitHeaders)
			{
				if (cusPermitHeaders != null && cusPermitHeaders.Length == 1)
				{
					Header.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
					switch (Header.AMA_ManifestType)
					{
						case FRConstants.TemporaryStorage.AppCodeIST:
							Header.GoodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
							break;
						case FRConstants.TemporaryStorage.AppCodeLAD:
							Header.GoodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
							break;
					}
					Header.GoodsLocation.Address.IdentificationHolderPK = cusPermitHeaders.First().CPH_OH_PermitHolder;
					Header.GoodsLocation.Address.AuthorisationNumber = cusPermitHeaders.First().CPH_Number;
					return;
				}
			}
		}
	}
}
