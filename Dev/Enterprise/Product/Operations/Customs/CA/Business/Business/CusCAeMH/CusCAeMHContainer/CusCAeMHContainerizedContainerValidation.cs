using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHContainerizedContainerValidation : CusCAeMHContainerValidation
	{
		public CusCAeMHContainerizedContainerValidation(AutoCusCAeMHContainer parent)
			: base(parent)
		{
		}

		protected new CusCAeMHContainer Parent
		{
			get { return (CusCAeMHContainer)base.Parent; }
		}

		protected override void CheckBQ_ContainerNumber()
		{
			base.CheckBQ_ContainerNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BQ_ContainerNumberInfo);
			var containerNumber = Parent.BQ_ContainerNumber;
			if (!containerNumber.IsEmpty)
			{
				if (containerNumber.Length < 5 || containerNumber.Length > 16 || !containerNumber.IsLettersAndNumbersOnlyOrEmpty)
				{
					Parent.BQ_ContainerNumberInfo.AddMessageError(InvalidContainerNumberLength);
				}

				ContainerNumberValidation.WarnIfInvalid(Parent.BQ_ContainerNumberInfo);
			}
		}

		internal static string InvalidContainerNumberLength
		{
			get { return Res.GetString("A1B98BD0-EB8C-4D92-ACBD-7B140356327D", "The container number should be between 5 and 16 alphanumerical characters."); }
		}

		protected override void CheckBQ_RC_NKContainerType()
		{
			base.CheckBQ_RC_NKContainerType();
			ListValidation.MessageErrorIfInvalidCode(Parent.BQ_RC_NKContainerTypeInfo);
		}

		protected override void CheckBQ_Seal1()
		{
			base.CheckBQ_Seal1();
			CheckSealMaxLength(Parent.BQ_Seal1Info);
		}

		protected override void CheckBQ_Seal2()
		{
			base.CheckBQ_Seal2();
			CheckSealMaxLength(Parent.BQ_Seal2Info);
		}

		static void CheckSealMaxLength(ZPropertyInfo sealPropertyInfo)
		{
			var sealNumber = (ZString)sealPropertyInfo.Value;
			if (sealNumber.Length > 15)
			{
				sealPropertyInfo.AddMessageError(
					Res.GetString(
						"5751FBA0-3A5D-43A5-A4B1-A3F18FA871D3"
						, "Seal Number cannot be more than 15 characters."));
			}
		}
	}
}
