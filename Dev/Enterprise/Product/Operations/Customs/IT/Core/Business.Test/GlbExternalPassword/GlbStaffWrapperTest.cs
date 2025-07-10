using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.MasterFiles.Integration.CustomsIntegration.IT;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(GlbStaffWrapper))]
sealed class GlbStaffWrapperTest : MasterFiles.Business.Testing.GlbStaffWrapperTest<GlbStaffWrapper>
{
	#region HasChanges

	public void TestHasChanges_CryptokiCertificateCollection_AddNew()
	{
		AssertHasChanges(() => Wrapper.CryptokiCertificateCollection.AddNew());
	}

	public void TestHasChanges_AutomaticSignaturePasswordCollection_AddNew()
	{
		AssertHasChanges(() => Wrapper.AutomaticSignaturePasswordCollection.AddNew());
	}

	void AssertHasChanges(Action action)
	{
		AssertEquals("Precondition: No Changes", false, Wrapper.HasChanges);
		action();
		AssertEquals("Has Changes", true, Wrapper.HasChanges);
	}

	public void TestHasChanges_OneItemPasswordCollections_Load()
	{
		var wrapper = Wrapper;
		wrapper.CryptokiCertificateCollection.AddNew();
		wrapper.AutomaticSignaturePasswordCollection.AddNew();
		Factory.Save();

		var newFactory = new BusinessObjectFactory();
		var staff = newFactory.Load<GlbStaff>(Wrapper.Staff.PK);
		var newWrapper = GlbStaffWrapper.Get(staff);
		AssertEquals("Precondition: CryptokiCertificateCollection loaded", 1, newWrapper.CryptokiCertificateCollection.Count);
		AssertEquals("Precondition: AutomaticSignaturePasswordCollection loaded", 1, newWrapper.AutomaticSignaturePasswordCollection.Count);
		AssertEquals("HasChanges should not be set to true when loading", false, newWrapper.HasChanges);
	}

	#endregion

	public void TestIGlbStaffWrapperMembers()
	{
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		var wrapper = GlbStaffWrapper.Get(staff);
		IGlbStaffWrapper iWrapper = wrapper;
		AssertEquals(wrapper.PasswordCollection, iWrapper.PasswordCollection);
	}

	public void TestITBPasswordCollection()
	{
		var company1 = Factory.NewWithValidTestData<GlbCompany>();
		company1.GC_Code = "DK@";
		var staff1 = Factory.NewWithValidTestData<GlbStaff>();
		staff1.GS_Code = "DK1";
		var password1 = Factory.NewWithValidTestData<GlbBrokerExternalPassword>();
		password1.GP_GS = staff1.PK;
		password1.GP_UserID = "1";
		password1.GP_GC = company1.PK;
		var password2 = Factory.NewWithValidTestData<GlbBrokerExternalPassword>();
		password2.GP_GS = staff1.PK;
		password2.GP_UserID = "2";
		password2.GP_GC = company1.PK;
		var password3 = Factory.NewWithValidTestData<GlbBrokerExternalPassword>();
		password3.GP_GS = staff1.PK;
		password3.GP_UserID = "3";
		password3.GP_GC = GlbCompany.CurrentCompany.PK;
		var staff2 = Factory.NewWithValidTestData<GlbStaff>();
		staff2.GS_Code = "DK2";
		var password4 = Factory.NewWithValidTestData<GlbBrokerExternalPassword>();
		password4.GP_GS = staff2.PK;
		password4.GP_UserID = "4";
		password4.GP_GC = GlbCompany.CurrentCompany.PK;

		Factory.Save();

		var factory = new BusinessObjectFactory();
		staff1 = factory.Load<GlbStaff>(staff1.PK);
		password1 = factory.Load<GlbBrokerExternalPassword>(password1.PK);
		password2 = factory.Load<GlbBrokerExternalPassword>(password1.PK);
		password3 = factory.Load<GlbBrokerExternalPassword>(password1.PK);
		var wrapper1 = GlbStaffWrapper.Get(staff1);

		AssertEquals(3, wrapper1.PasswordCollection.Count);
		AssertCollectionContains(password1, wrapper1.PasswordCollection);
		AssertCollectionContains(password2, wrapper1.PasswordCollection);
		AssertCollectionContains(password3, wrapper1.PasswordCollection);
	}

	public void TestCryptokiCertificateCollection()
	{
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		var wrapper = GlbStaffWrapper.Get(staff);
		AssertType<CryptokiExternalPasswordCollection>("CryptokiCertificateCollection Type", wrapper.CryptokiCertificateCollection);
	}

	public void TestAutomaticSignaturePasswordCollection()
	{
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		var wrapper = GlbStaffWrapper.Get(staff);
		AssertType<AutomaticSignatureExternalPasswordCollection>("AutomaticSignaturePasswordCollection Type", wrapper.AutomaticSignaturePasswordCollection);
	}

	protected override GlbStaffWrapper CreateNewWrapper(GlbStaff staff)
	{
		return GlbStaffWrapper.Get(staff);
	}
}
