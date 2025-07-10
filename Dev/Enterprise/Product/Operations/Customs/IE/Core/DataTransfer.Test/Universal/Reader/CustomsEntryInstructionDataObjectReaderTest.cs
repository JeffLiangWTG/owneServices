using System.Collections.Generic;
using Enterprise.Customs.EU.DataTransfer.Universal;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.IE.DataTransfer.Universal.Testing
{
	sealed class CustomsEntryInstructionDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestImportGoodsLocationInfo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				var declaration = Factory.New<JobDeclaration>();
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";
				Factory.SaveForTesting();

				var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Style = "A",
					Description = "GREETING",
				};
				entryInstructionDataObject.SetLocationOfGoodsCollection(() => CreateLocationOfGoodsDataObjects("Z"));
				var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
				AssertSame(existingEntryInstruction, entryInstruction);

				CombineAssertions("Goods location in EntryInstruction", () =>
				{
					var goodsLocation = existingEntryInstruction.GoodsLocation;

					AssertEquals("Qualifier", "Z", goodsLocation.CGL_Qualifier);
					AssertEquals("Type", "B", goodsLocation.CGL_Type);
					AssertEquals("AuthorisationNumber", "A123", goodsLocation.Address.AuthorisationNumber);
					AssertEquals("AdditionalIdentifier", "DUB", goodsLocation.CGL_AdditionalIdentifier);
					AssertEquals("CGL_CustomsOffice", "IEDUB0001", goodsLocation.CGL_CustomsOffice);
					AssertEquals("Contact Name", "Bob", goodsLocation.Address.E2_Contact);
					AssertEquals("Contact Phone", "923", goodsLocation.Address.E2_Phone);
					AssertEquals("Contact Email", "bob@gmail.in", goodsLocation.Address.E2_Email);
				});
			}
		}

		CusEntryInstruction GetEntryInstructionFromData(EntryInstruction entryInstruction, JobDeclaration declaration)
		{
			var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Ireland, Core.Constants.CountryCodes.Ireland);
			var reader = new Reader.CustomsEntryInstructionDataObjectReader(entryInstruction, new TestErrorLogger(), helper, Factory, declaration);
			return (CusEntryInstruction)reader.ReadIntoBusinessObject();
		}

		List<LocationOfGoods> CreateLocationOfGoodsDataObjects(string qualifier)
		{
			return new List<LocationOfGoods>
			{
				new LocationOfGoods
				{
					Qualifier = new CodeDescriptionPair1Char { Code = qualifier },
					LocationType = new CodeDescriptionPair1Char { Code = "B", Description = "Authorized Place" },
					AuthorizationNumber = "A123",
					AdditionalIdentifier = "DUB0002",
					CustomsOffice = "IEDUB0001",
					Contact = new Contact
					{
						Name = "Bob",
						PhoneNumber = "923",
						Email = "bob@gmail.in",
					},
				}
			};
		}
	}
}
