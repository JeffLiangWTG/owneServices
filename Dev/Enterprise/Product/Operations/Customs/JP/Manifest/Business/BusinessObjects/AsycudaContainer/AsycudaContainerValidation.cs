using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class AsycudaContainerValidation : ASYCUDA.Business.AsycudaContainerValidation
	{
		public AsycudaContainerValidation(AsycudaContainer parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateVanningLocationCode();
			ValidateACN_MoveOutDate();
		}

		public void ValidateVanningLocationCode()
		{
			ValidateCalculatedProperty(Parent.VanningLocationCodeInfo);
		}

		public void ValidateACN_MoveOutDate()
		{
			ValidateCalculatedProperty(Parent.ACN_MoveOutDateInfo);
		}

		protected void CheckVanningLocationCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.VanningLocationCodeInfo, ResString.GetMultilingualString("530C7291-EED7-4AD5-8237-14B9DE56A0FA", "The entered Move-In Destination is invalid. Please select a value from the list."));
		}

		protected void CheckACN_MoveOutDate()
		{
			var parent = Parent;
			var targetInfo = parent.ACN_MoveOutDateInfo;
			var moveOutDate = parent.ACN_MoveOutDate;
			if (moveOutDate.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("AED74619-2947-4F2F-8DD7-0116547A59E7", "Please enter Move Out Date."));
			}
			else if (moveOutDate.IsValid && ZDate.Today.AddDays(2).CompareTo(moveOutDate.Date) < 0)
			{
				targetInfo.AddMessageError(Res.GetString("7A2BB1D5-52F9-4305-8936-B42067F60C55", "Please enter Move Out Date within 2 days of the system date."));
			}
		}

		protected override void CheckACN_Seal1()
		{
			base.CheckACN_Seal1();
			var parnet = Parent;
			if ((parnet.Header?.ViaLocationCode.IsEmpty ?? false) && !parnet.HasSealNumber)
			{
				parnet.ACN_Seal1Info.AddMessageError(Res.GetString("0E0EC52E-C3E9-4572-939C-B7A3A49A860E", "At least one seal number is required when the via location code is empty."));
			}
		}

		public new AsycudaContainer Parent => (AsycudaContainer)base.Parent;
	}
}
