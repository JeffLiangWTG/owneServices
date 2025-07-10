using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsEdiMessage))]
	class NctsEdiMessageTests : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<NctsEdiMessage>();
		}

		public void TestMessageDescriptionForEdocs()
		{
			var message = Factory.New<NctsEdiMessage>();
			AssertEquals("NCTS Message", message.MessageDescriptionForEdocs);
		}

		public void TestIsSecurityDeclaration()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(NctsMovementType.Codes.Departure);
				header.BH_FTZMove = true;
				var message = Factory.New<NctsEdiMessage>();
				message.EM_LinkedObject = header;
				AssertEquals("No security at header or goods item level", false, message.IsSecurityDeclaration);
				var line = header.MovementHeader.GoodsItems.AddNew();
				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "COS", line.SecurityConsignor);
				AssertEquals("Added security at header level", true, message.IsSecurityDeclaration);
			});
		}

		public void TestCountryCode()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branch = company.Branches.AddNew();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_GB = branch.PK;
			var message = Factory.New<NctsEdiMessage>();
			message.EM_LinkedObject = header;
			AssertEquals("Should have the same country as the NctsHeader", Core.Constants.CountryCodes.Ireland, message.CountryCode);
		}

		public void TestHeader()
		{
			var nctsEdiMessage = (NctsEdiMessage)GetNewBusinessObject();
			var header = Factory.New<NctsHeader>();
			nctsEdiMessage.EM_LinkedObject = header;
			AssertSame(header, nctsEdiMessage.Header);

			header.SetMovementType(NctsMovementType.Codes.Departure);
			nctsEdiMessage.EM_LinkedObject = header.MovementHeader;
			AssertEquals(header, nctsEdiMessage.Header);
		}

		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
			var nctsEdiMessage = (NctsEdiMessage)GetNewBusinessObject();
			AssertEquals(typeof(NctsEdiMessageDocumentSupporter), nctsEdiMessage.DocumentSupporter.GetType());
			AssertEquals(BusinessContext.CusInBondHeader, nctsEdiMessage.DocumentSupporter.BusinessContext);
		}
	}
}
