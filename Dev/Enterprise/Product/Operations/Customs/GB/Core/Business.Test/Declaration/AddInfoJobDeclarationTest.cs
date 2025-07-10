using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Test.Declaration
{
	[TestedType(typeof(JobDeclaration))]
	public class AddInfoJobDeclarationBOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestZG_ImportClearanceStatusICSList()
		{
			AssertEquals("AddInfoLookups.ImportClearanceStatusICSList", GetAddInfoJobDeclaration().ZG_ImportClearanceStatusICSInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestZG_VATDeferNumberMaxLength()
		{
			AssertEquals(7, GetAddInfoJobDeclaration().ZG_VATDeferNumberInfo.MaxLength);
		}

		public void TestDefaultingDeferralToVAT()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			RefCountry gb = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedKingdom);
			branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "1234567", gb);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.ZG_VATDeferType = "A";
			AssertEquals("1234567", declaration.ZG_VATDeferNumber);
		}

		public void TestDefaultingDeferralToOther()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			RefCountry gb = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedKingdom);
			branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "1234567", gb);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = "A";
			AssertEquals("1234567", declaration.JE_DefermentAccountNumber);
		}

		protected override BusinessObject GetNewBusinessObject() => GetAddInfoJobDeclaration();

		JobDeclaration GetAddInfoJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
	}
}

