using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ComplXDVDDeclarantAndRepresentativeWrapperTest : WrapperHelperTest<ComplXDVDDeclarantAndRepresentativeWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if declaration is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","declaration"), () => GetWrapper(null));
		}

		public void TestNullDeclarant()
		{
			declaration.Declarant.OA_OH = ZGuid.Empty;
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Declarant.ToString());
		}

		public void TestDeclarant()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.Addresses.AddNew();
				declaration.Declarant.OA_OH = orgHeader.PK;

				var declarant = wrapper.Declarant;

				AssertNotNull("Expected filled Declarant", declarant);
				AssertSame("Cached Declarant", wrapper.Declarant, declarant);
			});
		}

		public void TestNullRepresentative()
		{
			declaration.JE_OA_Representative = ZGuid.Empty;
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Representative.ToString());
		}

		public void TestRepresentative()
		{
			CombineAssertions(() =>
			{
				var orgAddress = Factory.New<OrgAddress>();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgAddress.OA_OH = orgHeader.PK;
				declaration.JE_OA_Representative = orgAddress.PK;

				var representative = wrapper.Representative;

				AssertNotNull("Expected filled Representative", representative);
				AssertSame("Cached Representative", wrapper.Representative, representative);
			});
		}

		public void TestRepresentativeTypeAuthorization()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty RepresentativeTypeAuthorization when flag is not checked", ZString.Empty, wrapper.RepresentativeTypeAuthorization);

				declaration.ZG_AuthPerDeclaration = true;
				AssertEquals("Expected filled RepresentativeTypeAuthorization when flag is checked", "O", wrapper.RepresentativeTypeAuthorization);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			wrapper = GetWrapper(declaration);
		}

		JobDeclaration declaration;
		ComplXDVDDeclarantAndRepresentativeWrapper wrapper;

		ComplXDVDDeclarantAndRepresentativeWrapper GetWrapper(JobDeclaration declaration) => new ComplXDVDDeclarantAndRepresentativeWrapper(declaration);

		protected override ComplXDVDDeclarantAndRepresentativeWrapper GetProvider() => wrapper;
	}
}
