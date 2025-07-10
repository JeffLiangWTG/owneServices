using CargoWise.EntityFramework;
using Enterprise.Licensing.Billing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceEnterpriseValidation : Licensing.Billing.Business.LicenceEnterpriseValidation
	{
		public LicenceEnterpriseValidation(AutoLicenceEnterprise parent)
			: base(parent)
		{
		}

		new LicenceEnterprise Parent
		{
			get { return (LicenceEnterprise)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			if (Parent.Organisation != null)
			{
				Parent.Organisation.Validation.ValidateLicenceEnterpriseCode();
			}
		}

		protected override void CheckLE_EnterpriseCode()
		{
			base.CheckLE_EnterpriseCode();

			var entCode = Parent.LE_EnterpriseCode;

			if (!entCode.IsLettersAndNumbersOnlyOrEmpty)
			{
				Parent.LE_EnterpriseCodeInfo.AddError("Enterprise Code must consist of numbers and letters only");
			}

			if (entCode.Length != 0 && entCode.Length != 3)
			{
				Parent.LE_EnterpriseCodeInfo.AddError("Enterprise Code must be empty or 3 characters in length");
			}

			if (entCode.Length == 3)
			{
				var query = new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, entCode);
				query.AddToFilter(LicenceEnterpriseSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.ExistsInDatabase(LicenceEnterpriseSchema.Constants.TableName, query))
				{
					Parent.LE_EnterpriseCodeInfo.AddError("Code is in use");
				}
			}
		}
	}
}

