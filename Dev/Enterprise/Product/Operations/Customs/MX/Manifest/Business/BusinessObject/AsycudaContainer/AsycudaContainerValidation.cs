using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class AsycudaContainerValidation : ASYCUDA.Business.AsycudaContainerValidation
	{
		public AsycudaContainerValidation(AsycudaContainer parent)
			: base(parent)
		{
		}

		readonly ZInt minLengthSeals = 2;
		readonly ZInt maxLengthSeals = 15;

		protected new AsycudaContainer Parent => (AsycudaContainer)base.Parent;

		protected override void CheckACN_RC_ContainerType()
		{
			base.CheckACN_RC_ContainerType();

			var parent = Parent;
			var container = parent.ContainerType;
			if (parent.Header.IsSea && container != null && container.RC_ISOType.IsEmpty && (container.RC_Height.IsEmpty || container.RC_Length.IsEmpty || container.RC_Width.IsEmpty))
			{
				parent.ACN_RC_ContainerTypeInfo.AddMessageError(Res.GetString("3A46BC51-9EAD-4A3B-B62F-A6475B76C28E", "{0} must have an ISO type entered, or alternatively, length, width and height", parent.ACN_RC_ContainerTypeInfo.HumanReadableName));
			}
		}

		protected override void CheckACN_Seal1()
		{
			base.CheckACN_Seal1();

			var parent = Parent;
			if (parent.Header.IsSea && !parent.ACN_Seal1.IsEmpty)
			{
				ValidateAlphanumericTextField(parent.ACN_Seal1Info, parent.ACN_Seal1);
			}
		}

		protected override void CheckACN_Seal2()
		{
			base.CheckACN_Seal2();

			var parent = Parent;
			if (parent.Header.IsSea && !parent.ACN_Seal2.IsEmpty)
			{
				ValidateAlphanumericTextField(parent.ACN_Seal2Info, parent.ACN_Seal2);
			}
		}

		void ValidateAlphanumericTextField(ZPropertyInfo propertyInfo, ZString value)
		{
			if (!value.IsLettersAndNumbersOnlyOrEmpty || value.Length < minLengthSeals || value.Length > maxLengthSeals)
			{
				propertyInfo.AddMessageError(Res.GetString("AAA7179A-923C-4CD8-85E5-D6ED2C763BC7", "{0} must only contain letters or numbers and must be at least {1} and at most {2} characters in length", propertyInfo.HumanReadableName, minLengthSeals, maxLengthSeals));
			}
		}
	}
}
