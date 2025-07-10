using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSJobDeclaration.Loader))]
	sealed class EMCSJobDeclarationLoaderTest : LoaderTestCase
	{
		public void TestNoEADNumber()
		{
			var declaration = SetupDeclaration("MRN98761234");
			Factory.Save();
			AssertEquals("Empty Array", 0, EMCSJobDeclaration.Loader.LoadFromEadAndSequenceNumber<EMCSJobDeclaration>(Factory, string.Empty, "1", declaration.Company).Length);
		}

		public void TestNoSequenceNumber()
		{
			var declaration = SetupDeclaration("MRN98761234");
			Factory.Save();
			AssertEquals("Empty Array", 0, EMCSJobDeclaration.Loader.LoadFromEadAndSequenceNumber<EMCSJobDeclaration>(Factory, "MRN98761234", string.Empty, declaration.Company).Length);
		}

		public void TestIncorrectSequenceNumber()
		{
			var declaration = SetupDeclaration("MRN98761234");
			Factory.Save();
			AssertEquals("Empty Array", 0, EMCSJobDeclaration.Loader.LoadFromEadAndSequenceNumber<EMCSJobDeclaration>(Factory, "MRN98761234", "2", declaration.Company).Length);
		}

		public void TestIncorrectCountry()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			var cusEntryNumber = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Gambia);
			cusEntryNumber.CE_EntryNum = "MRN98761234";
			Factory.Save();
			AssertEquals("Empty Array", 0, EMCSJobDeclaration.Loader.LoadFromEadAndSequenceNumber<EMCSJobDeclaration>(Factory, "MRN98761234", string.Empty, declaration.Company).Length);
		}

		public void TestIncorrectEntryNumberType()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			var cusEntryNumber = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Latvia);
			cusEntryNumber.CE_EntryNum = "MRN98761234";
			cusEntryNumber.CE_EntryLineReference = "1";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Entry Number persisted", true, cusEntryNumber.IsInDatabase);
				AssertEquals("Empty Array", 0, EMCSJobDeclaration.Loader.LoadFromEadAndSequenceNumber<EMCSJobDeclaration>(Factory, "MRN98761234", "1", declaration.Company).Length);
			});
		}

		public void TestNonEMCSDeclarationParent()
		{
			var exportDeclaration = Factory.New<JobDeclaration>();
			exportDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			exportDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			var cusEntryNumber = CusEntryNumber.New(exportDeclaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Latvia);
			cusEntryNumber.CE_EntryNum = "MRN98761234";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Entry Number persisted", true, cusEntryNumber.IsInDatabase);
				AssertEquals("Empty Array", 0, EMCSJobDeclaration.Loader.LoadFromEadAndSequenceNumber<EMCSJobDeclaration>(Factory, "MRN98761234", "1", exportDeclaration.Company).Length);
			});
		}

		public void TestDifferentCompany()
		{
			var declaration = SetupDeclaration("MRN98761234");
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Declaration persisted", true, declaration.IsInDatabase);
				AssertEquals("Empty Array", 0, EMCSJobDeclaration.Loader.LoadFromEadAndSequenceNumber<EMCSJobDeclaration>(Factory, "MRN98761234", "1", Factory.New<GlbCompany>()).Length);
			});
		}

		public void TestEADNumberValidAndOrderedDescending()
		{
			var declaration1 = SetupDeclaration("MRN98761234");
			Factory.Save();

			var declaration2 = Factory.New<EMCSJobDeclaration>();
			declaration2.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			var cusEntryNumber = CusEntryNumber.New(declaration2, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Latvia);
			cusEntryNumber.CE_EntryNum = "MRN98761234";
			cusEntryNumber.CE_EntryLineReference = "1";
			Factory.Save();

			var emcsDeclarations = EMCSJobDeclaration.Loader.LoadFromEadAndSequenceNumber<EMCSJobDeclaration>(Factory, "MRN98761234", "1", declaration1.Company);
			AssertContainsExactElementsInExactOrder(new EMCSJobDeclaration[] { declaration2, declaration1 }, emcsDeclarations);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new EMCSJobDeclaration.Loader(Factory);

		EMCSJobDeclaration SetupDeclaration(string eadNumber)
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			var cusEntryNumber = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Latvia);
			cusEntryNumber.CE_EntryNum = eadNumber;
			cusEntryNumber.CE_EntryLineReference = "1";
			return declaration;
		}
	}
}
