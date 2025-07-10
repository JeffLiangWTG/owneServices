using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class FRTemporaryStorageHeaderValidation : TemporaryStorageHeaderValidation
	{
		public FRTemporaryStorageHeaderValidation(TemporaryStorageHeader parent) : base(parent)
		{
		}

		protected new TemporaryStorageHeader Parent => (TemporaryStorageHeader)base.Parent;

		protected override void CheckAuthorizationNumber()
		{
			base.CheckAuthorizationNumber();

			var parent = Parent;
			if (parent.GoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber && !parent.AuthorizationNumber.IsEmpty)
			{
				parent.AuthorizationNumberInfo.AddMessageError(Res.GetString("1B4BAB82-3EE0-45D6-AA1D-7574ED525FBB", "Authorization Number must not be entered when qualifier of Location of Goods is Y."));
			}
		}

		protected override void CheckAuthorizationType()
		{
			base.CheckAuthorizationType();

			var parent = Parent;
			if (parent.GoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber && !parent.AuthorizationType.IsEmpty)
			{
				parent.AuthorizationTypeInfo.AddMessageError(Res.GetString("7FA58DAE-96F8-4B93-9884-E4A9BF152AAD", "Authorization Type must not be entered when qualifier of Location of Goods is Y."));
			}
		}

		protected override void CheckAuthorizationOwner()
		{
			base.CheckAuthorizationOwner();

			var parent = Parent;
			if (parent.GoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber && !parent.AuthorizationOwner.IsEmpty)
			{
				parent.AuthorizationOwnerInfo.AddMessageError(Res.GetString("5BD34153-ADCF-43B2-9A05-9BFE8643C99E", "Authorization Owner must not be entered when qualifier of Location of Goods is Y."));
			}
		}

		protected override void CheckGoodsLocationDescription()
		{
			base.CheckGoodsLocationDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.GoodsLocationDescriptionInfo);
		}

		protected override void CheckPlaceOfUnloading()
		{
			base.CheckPlaceOfUnloading();
			var parent = Parent;
			if (!parent.PlaceOfUnloading.IsEmpty && ListOfMessageTypeWithNoPlaceOfUnloading.Contains(parent.AMA_MessageType))
			{
				parent.PlaceOfUnloadingInfo.AddMessageError(Res.GetString("DC7F3821-D7B5-4F8E-8C0A-6DB2576D4697", "'PLACE OF UNLOADING' must not be entered."));
			}
		}

		ZString[] ListOfMessageTypeWithNoPlaceOfUnloading => new ZString[] { PNTSMessageTypeList.Codes.PreLodgedTempStorage , PNTSMessageTypeList.Codes.PresentationNotification , PNTSMessageTypeList.Codes.CombinedTemporaryStorage };
	}
}
