
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLDetailCargoLineValidation : BaseCusSeaManOBLDetailValidation
	{
		public CusSeaManOBLDetailCargoLineValidation(CusSeaManOBLDetailCargoLine parent)
			: base(parent)
		{
		}

		protected override void CheckBD_LineCargoType()
		{
			base.CheckBD_LineCargoType();
			if (Parent.BD_LineCargoType.IsEmpty)
			{
				MessageValidation.CheckEntered(Parent.BD_LineCargoTypeInfo, "Cargo Type is required.");
			}
			else
			{
				if (Parent.BD_LineCargoType != CMRCargoTypes.Codes.FullContainerLoad
					&& Parent.Header != null && Parent.Header.BO_HeaderCargoType == CMRCargoCodes.Codes.Empty)
				{
					Parent.BD_LineCargoTypeInfo.AddMessageError("Cargo Type should be FCL if Cargo Code is set to Empty.");
				}

				ListValidation.ErrorIfInvalidCode(Parent.BD_LineCargoTypeInfo, Parent.Lookups.CargoTypes);
			}
		}

		protected override void CheckBD_ContainerNumber()
		{
			base.CheckBD_ContainerNumber();
			MessageValidation.CheckEntered(Parent.BD_ContainerNumberInfo, "Cargo Identifier is required.");
			if (Parent.Header != null && Parent.Header.Port != null)
			{
				foreach (CusSeaManOBLDetailCargoLine cargoLine in Parent.Header.Port.GetCargoListDetails())
				{
					if (cargoLine.BD_ContainerNumber.EqualsIgnoringCase(Parent.BD_ContainerNumber) && cargoLine.PK != Parent.PK)
					{
						Parent.BD_ContainerNumberInfo.AddError("This Cargo Identifier has already been used on another line of this Cargo List.");
						break;
					}
				}
			}
		}

		protected override void CheckBD_PackType()
		{
			base.CheckBD_PackType();
			if (Parent.BD_LineCargoType == CMRCargoTypes.Codes.BreakBulk)
			{
				MessageValidation.CheckEntered(Parent.BD_PackTypeInfo, "Required if Break Bulk.");
			}
			if (Parent.Header != null)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.BD_PackTypeInfo, Parent.Header.Lookups.PackageTypes);
			}
		}

		protected override void CheckBD_NoOfPacks()
		{
			base.CheckBD_NoOfPacks();
			if (Parent.BD_LineCargoType == CMRCargoTypes.Codes.BreakBulk)
			{
				MessageValidation.CheckEntered(Parent.BD_NoOfPacksInfo, "Required if Break Bulk.");
			}
			else if (Parent.BD_LineCargoType != CMRCargoTypes.Codes.BreakBulk && !Parent.BD_NoOfPacks.IsEmpty)
			{
				Parent.BD_NoOfPacksInfo.AddMessageError("Only required if Break Bulk.");
			}
		}

		new CusSeaManOBLDetailCargoLine Parent
		{
			get { return (CusSeaManOBLDetailCargoLine)base.Parent; }
		}

		protected MessageValidation MessageValidation
		{
			get
			{
				if (messageValidation == null)
				{
					messageValidation = new MessageValidation(Parent);
				}
				return messageValidation;
			}
		}
		MessageValidation messageValidation;
	}
}
