using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class Licence3rdPartySoftwareValidation : AutoLicence3rdPartySoftwareValidation
	{
		public Licence3rdPartySoftwareValidation(AutoLicence3rdPartySoftware parent)
			: base(parent)
		{
		}

		public new Licence3rdPartySoftware Parent
		{
			get { return (Licence3rdPartySoftware)base.Parent; }
		}

		protected override void CheckL3_OP_ProductSKU()
		{
			base.CheckL3_OP_ProductSKU();
			MandatoryValidation.CheckEntered(Parent.L3_OP_ProductSKUInfo);
			ListValidation.ErrorIfInvalidPK(Parent.L3_OP_ProductSKUInfo, Parent.Lookups.Products);
		}

		protected override void CheckL3_OSType()
		{
			base.CheckL3_OSType();
			MandatoryValidation.CheckEntered(Parent.L3_OSTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.L3_OSTypeInfo, Parent.Lookups.OSType);
		}

		protected override void CheckL3_LicenceType()
		{
			base.CheckL3_LicenceType();
			MandatoryValidation.CheckEntered(Parent.L3_LicenceTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.L3_LicenceTypeInfo, Parent.Lookups.LicenceType);
		}

		protected override void CheckL3_LicenceIssuedIsValidZDateTime()
		{
			base.CheckL3_LicenceIssuedIsValidZDateTime();
			MandatoryValidation.CheckEntered(Parent.L3_LicenceIssuedInfo);
		}

		protected override void CheckL3_LicenceCount()
		{
			base.CheckL3_LicenceCount();
			MandatoryValidation.CheckEntered(Parent.L3_LicenceCountInfo);
			if (Parent.L3_LicenceCount < 1)
			{
				Parent.L3_LicenceCountInfo.AddError("The number of licences must be greater than zero.");
			}
		}

		protected override void CheckL3_UpgradeAssuranceStartsOn()
		{
			base.CheckL3_UpgradeAssuranceStartsOnIsValidZDateTime();
			if (!Parent.L3_UpgradeAssuranceEndsOn.IsEmpty)
			{
				if (!(Parent.L3_UpgradeAssuranceEndsOn > Parent.L3_UpgradeAssuranceStartsOn))
				{
					Parent.L3_UpgradeAssuranceStartsOnInfo.AddError("Upgrade Assurance Starts should be less then Ends date");
				}
			}
		}

		protected override void CheckL3_UpgradeAssuranceEndsOn()
		{
			if (!Parent.L3_UpgradeAssuranceEndsOn.IsEmpty)
			{
				base.CheckL3_UpgradeAssuranceEndsOn();
				if (Parent.L3_UpgradeAssuranceStartsOn.IsEmpty)
				{
					Parent.L3_UpgradeAssuranceEndsOnInfo.AddError("You should enter 'Upgrade Assurance Starts' date first");
				}
				else if (!(Parent.L3_UpgradeAssuranceEndsOn > Parent.L3_UpgradeAssuranceStartsOn))
				{
					Parent.L3_UpgradeAssuranceEndsOnInfo.AddError("Upgrade Assurance Starts should be less then Ends date");
				}
			}
		}
	}
}

