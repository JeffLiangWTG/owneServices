using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgHeaderValidation : OrgHeaderValidationReal
	{
		public EDIOrgHeaderValidation(EDIOrgHeader parent)
			: base(parent)
		{
		}

		public new EDIOrgHeader Parent
		{
			get { return (EDIOrgHeader)base.Parent; }
		}

		protected override void CheckOH_FullName()
		{
			base.CheckOH_FullName();
			if (Parent.OH_FullName.Length > 50)
			{
				Parent.OH_FullNameInfo.AddWarning("Length exceeds 50 characters. An ediEnterprise license cannot be generated.");
			}
		}

		#region LicenceEnterpriseCode

		public void ValidateLicenceEnterpriseCode()
		{
			ValidateCalculatedProperty(Parent.LicenceEnterpriseCodeInfo);
		}

		protected void CheckLicenceEnterpriseCode()
		{
			if (Parent.LicenceEnterpriseCodeGenerationFailure)
			{
				Parent.LicenceEnterpriseCodeInfo.AddError("Cannot generate an Enterprise Code: All available combinations for this Organisation's Code have been exceeded");
			}

			if (Parent.OH_Code.IsEmpty)
			{
				Parent.LicenceEnterpriseCodeInfo.AddError("You must enter this Organisation's Code before a Licence Enterprise Code can be generated");
				return;
			}

			var products = EDIDataRegistry.Instance.ProductsRequiringEnterpriseCode.Value;
			var databaseRequiringEntCode = Parent.LicCompany?.ActiveOrAllLicDatabases.OfType<LicenceDatabase>()
				.FirstOrDefault(x => products.Contains(x.LD_Product.ToString()));

			var entCode = Parent.LicenceEnterpriseCode;

			if (!entCode.IsEmpty && databaseRequiringEntCode == null)
			{
				var productsAsString = products.Length > 1 ? string.Join(", ", products.Take(products.Length - 1)) + " and " + products.Last() : products.FirstOrDefault();
				Parent.LicenceEnterpriseCodeInfo.AddWarning(ZString.Format("Enterprise Code is only necessary for products {0}.", productsAsString));
			}
			else if (entCode.IsEmpty && databaseRequiringEntCode != null)
			{
				Parent.LicenceEnterpriseCodeInfo.AddError(ZString.Format("Enterprise Code is mandatory if a database with product {0} is attached", databaseRequiringEntCode.LD_Product));
			}
		}
		#endregion
	}
}

