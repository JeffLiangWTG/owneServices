using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class AsycudaContainerValidation : ASYCUDA.Business.AsycudaContainerValidation
	{
		public AsycudaContainerValidation(AsycudaContainer parent)
			: base(parent)
		{
		}

		protected new AsycudaContainer Parent => (AsycudaContainer)base.Parent;

		protected override void CheckACN_GoodsWeight()
		{
			base.CheckACN_GoodsWeight();

			if (Parent.Header.IsSea)
			{
				if (Parent.ACN_GoodsWeight == 0 && Parent.ACN_EmptyFullIndicator != EmptyFullIndicatorList.Codes.EmptyContainer)
				{
					Parent.ACN_GoodsWeightInfo.AddMessageError(Res.GetString("509C4681-7DAC-47BE-B6BC-D6C192CDA499", "If the container is not declared as empty, it cannot have a gross weight 0"));
				}
			}
		}

		protected override void CheckACN_RC_ContainerType()
		{
			base.CheckACN_RC_ContainerType();

			if (Parent.Header.IsSea)
			{
				var container = Parent.ContainerType;
				if (container != null && container.RC_ISOType.IsEmpty)
				{
					Parent.ACN_RC_ContainerTypeInfo.AddMessageError(Res.GetString("A3FEF4E3-59CB-4DFB-B2A4-34C59D113E04", "The Container Type selected should have a ISO Type entered"));
				}
			}
		}

		protected override void CheckACN_Seal1()
		{
			base.CheckACN_Seal1();

			if (Parent.Header.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ACN_Seal1Info);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateACN_ExpireDate();
			ValidateACN_ACEP();
		}

		#region ACN_ExpireDate

		protected void CheckACN_ExpireDate()
		{
			if (Parent.Header.AMA_TransportMode == Core.Constants.TransportModes.Sea && Parent.ACN_ACEP.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ACN_ExpireDateInfo);
			}
		}

		public void ValidateACN_ExpireDate()
		{
			ValidateCalculatedProperty(Parent.ACN_ExpireDateInfo);
		}

		#endregion

		#region ACN_ACEP

		protected void CheckACN_ACEP()
		{
			if (Parent.Header.AMA_TransportMode == Core.Constants.TransportModes.Sea && Parent.ACN_ExpireDate.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ACN_ACEPInfo);
			}
		}

		public void ValidateACN_ACEP()
		{
			ValidateCalculatedProperty(Parent.ACN_ACEPInfo);
		}

		#endregion
	}
}
