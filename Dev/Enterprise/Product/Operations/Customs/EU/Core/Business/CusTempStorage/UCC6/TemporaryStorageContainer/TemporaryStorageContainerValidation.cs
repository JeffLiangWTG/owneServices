using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageContainerValidation : AsycudaContainerValidation
	{
		public TemporaryStorageContainerValidation(TemporaryStorageContainer parent) : base(parent)
		{
		}

		protected new TemporaryStorageContainer Parent => (TemporaryStorageContainer)base.Parent;

		ZString MessageIfAdditionalSealsIsUsedButOneOfFirstSealsIsEmpty => Res.GetString("9535B0A9-47AB-4847-8864-24C4598181DF", "Please use this seal before using the additional seals.");

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();

			base.ValidateAll();

			CheckContainerHasBeenAssigned();
		}

		protected override void CheckACN_ContainerNumber()
		{
			base.CheckACN_ContainerNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ACN_ContainerNumberInfo);
			ContainerNumberValidation.WarnIfInvalid(Parent.ACN_ContainerNumberInfo);
		}

		protected override void CheckACN_Seal1()
		{
			base.CheckACN_Seal1();

			var parent = Parent;
			if (parent.ACN_Seal1.IsEmpty)
			{
				if (!parent.ACN_Seal2.IsEmpty || !parent.ACN_Seal3.IsEmpty)
				{
					parent.ACN_Seal1Info.AddMessageError(Res.GetString("E98BA8C5-F0A2-4C48-B3ED-993563A3C973", "Please fill Seal 1 before filling Seals 2 or 3"));
				}

				if (parent.AdditionalSeals.Count > 0)
				{
					parent.ACN_Seal1Info.AddMessageError(MessageIfAdditionalSealsIsUsedButOneOfFirstSealsIsEmpty);
				}
			}
		}

		protected override void CheckACN_Seal2()
		{
			base.CheckACN_Seal2();

			var parent = Parent;
			if (parent.ACN_Seal2.IsEmpty)
			{
				if (!parent.ACN_Seal3.IsEmpty)
				{
					parent.ACN_Seal2Info.AddMessageError(Res.GetString("90672D4B-4E33-42E8-A333-2D857993F3F2", "Please fill Seal 2 before filling Seal 3"));
				}

				if (parent.AdditionalSeals.Count > 0)
				{
					parent.ACN_Seal2Info.AddMessageError(MessageIfAdditionalSealsIsUsedButOneOfFirstSealsIsEmpty);
				}
			}
		}

		protected override void CheckACN_Seal3()
		{
			base.CheckACN_Seal3();

			var parent = Parent;
			if (parent.ACN_Seal3.IsEmpty && parent.AdditionalSeals.Count > 0)
			{
				parent.ACN_Seal3Info.AddMessageError(MessageIfAdditionalSealsIsUsedButOneOfFirstSealsIsEmpty);
			}
		}

		protected override void CheckACN_RC_ContainerType()
		{
			base.CheckACN_RC_ContainerType();

			MandatoryValidationOfACN_RC_ContainerTypeCore();
			var parent = Parent;
			if (!parent.ACN_RC_ContainerType.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(parent.ACN_RC_ContainerTypeInfo);
			}
		}

		protected virtual void MandatoryValidationOfACN_RC_ContainerTypeCore()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ACN_RC_ContainerTypeInfo);
		}

		protected override void CheckACN_EmptyFullIndicator()
		{
			base.CheckACN_EmptyFullIndicator();

			if (!Parent.ACN_EmptyFullIndicator.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ACN_EmptyFullIndicatorInfo);
			}
		}

		void CheckContainerHasBeenAssigned()
		{
			var container = Parent;
			var packages = container.Header?.Bills?.SelectMany(x => x.Packs.Cast<TemporaryStoragePack>());

			if (packages != null && packages.Any())
			{
				var packagesAssigned = packages.Where(p => p.ContainerPK == container.PK);

				var containerNum = container.ACN_ContainerNumber;
				if (!packagesAssigned.Any())
				{
					container.AddRowWarning(Res.GetString("2CC17737-3A68-44D0-9C5A-1DD624EBFE01", "The container {0} is not assigned to any Pack.", containerNum));
				}
			}
		}
	}
}
