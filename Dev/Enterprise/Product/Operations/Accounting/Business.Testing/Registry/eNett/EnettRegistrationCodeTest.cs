using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(EnettRegistrationCode))]
	public class EnettRegistrationCodeTest : RegistryBusinessObjectTemplateTestCase<EnettRegistrationCode>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override EnettRegistrationCode GetBusinessObjectToClone()
		{
			return new EnettRegistrationCode { AuthenticationCode = "awthome", RegistrationCode = "codejack", OrganisationPK = new ZGuid("A232D857-8C3E-4FDB-BA32-12EDED63EA22") };
		}

		protected override EnettRegistrationCode GetBusinessObjectToSerialise()
		{
			return new EnettRegistrationCode { AuthenticationCode = "hoopless", RegistrationCode = "wonder", OrganisationPK = new ZGuid("B9D361F8-7A2E-4744-840E-625C6A0644FF") };
		}
		public void TestPreSaveValidation()
		{
			BizObj.RunPreSaveValidation();
			Assert("Shouldn't have errors", !BizObj.HasErrors());

			BizObj.AuthenticationCode = "Auth_Code";
			BizObj.RunPreSaveValidation();
			Assert("Should have error", BizObj.OrganisationPKInfo.HasError("ComPay Organization must be entered"));

			BizObj.AuthenticationCode = "";
			BizObj.RegistrationCode = "Reg_Code";
			BizObj.RunPreSaveValidation();
			Assert("Should have error", BizObj.OrganisationPKInfo.HasError("ComPay Organization must be entered"));

			BizObj.AuthenticationCode = "Auth_Code";
			BizObj.OrganisationPK = new ZGuid("B9D361F8-7A2E-4744-840E-625C6A0644FF");
			BizObj.RunPreSaveValidation();
			Assert("Shouldn't have error", !BizObj.OrganisationPKInfo.HasError("ComPay Organization must be entered"));
		}
	}
}
