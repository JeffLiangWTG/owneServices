using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusExitDetail))]
	class CusExitDetailTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestValidation()
		{
			NUnit.Framework.Assert.That(exitDetail.Validation, Is.TypeOf<CusExitDetailValidation>());
		}

		[ExpectNoExceptions]
		public void TestLookups()
		{
			NUnit.Framework.Assert.That(exitDetail.Lookups, Is.TypeOf<CusExitDetailLookups>());
		}

		[ExpectNoExceptions]
		public void TestCusExitItems()
		{
			NUnit.Framework.Assert.That(exitDetail.CusExitItems, Is.TypeOf<CusExitItemCollection>());
		}

		[ExpectNoExceptions]
		public void TestCED_MovementReferenceNumber_MaxLength()
		{
			NUnit.Framework.Assert.That(exitDetail.CED_MovementReferenceNumberInfo.MaxLength, Is.EqualTo(18));
		}

		[ExpectNoExceptions]
		public void TestCanDelete()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				exitDetail.CED_Status = "AAA";
				NUnit.Framework.Assert.That(exitDetail.CanDelete, Is.EqualTo(true), "Status not integer");

				exitDetail.CED_Status = "309";
				NUnit.Framework.Assert.That(exitDetail.CanDelete, Is.EqualTo(true), "Status < 310");

				exitDetail.CED_Status = UniversalReferenceConstants.CusExitDetailStatus._310;
				NUnit.Framework.Assert.That(exitDetail.CanDelete, Is.EqualTo(false), "Status = 310");

				exitDetail.CED_Status = "311";
				NUnit.Framework.Assert.That(exitDetail.CanDelete, Is.EqualTo(false), "Status > 310");
			});
		}

		[ExpectNoExceptions]
		public void TestReasonForNotAbleToDelete()
		{
			NUnit.Framework.Assert.That(exitDetail.ReasonForNotAbleToDelete, Is.EqualTo("Cannot delete Movements with Status >= 310").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCED_MovementReferenceNumber_ReadOnly()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				exitDetail.CED_Status = "AAA";
				NUnit.Framework.Assert.That(exitDetail.CED_MovementReferenceNumberInfo.ReadOnly, Is.EqualTo(false), "Status not integer");

				exitDetail.CED_Status = "309";
				NUnit.Framework.Assert.That(exitDetail.CED_MovementReferenceNumberInfo.ReadOnly, Is.EqualTo(false), "Status < 310");

				exitDetail.CED_Status = UniversalReferenceConstants.CusExitDetailStatus._310;
				NUnit.Framework.Assert.That(exitDetail.CED_MovementReferenceNumberInfo.ReadOnly, Is.EqualTo(true), "Status = 310");

				exitDetail.CED_Status = "311";
				NUnit.Framework.Assert.That(exitDetail.CED_MovementReferenceNumberInfo.ReadOnly, Is.EqualTo(true), "Status > 310");
			});
		}

		[ExpectNoExceptions]
		public void TestCED_CustomsOffice_ReadOnly()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				exitDetail.CED_Status = "AAA";
				NUnit.Framework.Assert.That(exitDetail.CED_CustomsOfficeInfo.ReadOnly, Is.EqualTo(false), "Status not integer");

				exitDetail.CED_Status = "309";
				NUnit.Framework.Assert.That(exitDetail.CED_CustomsOfficeInfo.ReadOnly, Is.EqualTo(false), "Status < 310");

				exitDetail.CED_Status = UniversalReferenceConstants.CusExitDetailStatus._310;
				NUnit.Framework.Assert.That(exitDetail.CED_CustomsOfficeInfo.ReadOnly, Is.EqualTo(true), "Status = 310");

				exitDetail.CED_Status = "311";
				NUnit.Framework.Assert.That(exitDetail.CED_CustomsOfficeInfo.ReadOnly, Is.EqualTo(true), "Status > 310");
			});
		}

		[ExpectNoExceptions]
		public void TestCED_LocationOfGoods_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(exitDetail.CED_LocationOfGoodsInfo).Caption, Is.EqualTo("Loading Place"));
		}

		[ExpectNoExceptions]
		public void TestDeclarantDocAddress()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var address = exitDetail.DeclarantDocAddress;
				NUnit.Framework.Assert.That(address, NUnit.Framework.Is.Not.EqualTo(default(JobDocAddress)), "Created when Null - should not be [null]");
				NUnit.Framework.Assert.That(address.DocAddressType, Is.EqualTo(DocAddressType.Declarant), "Type");
			});
		}

		[ExpectNoExceptions]
		public void TestDeclarantDocAddress_Delete()
		{
			var deletedDeclarant = exitDetail.DeclarantDocAddress;
			deletedDeclarant.Delete();
			NUnit.Framework.Assert.That(ReferenceEquals(exitDetail.DeclarantDocAddress, deletedDeclarant), Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestRepresentativeDocAddress()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var address = exitDetail.RepresentativeDocAddress;
				NUnit.Framework.Assert.That(address, NUnit.Framework.Is.Not.EqualTo(default(JobDocAddress)), "Created when Null - should not be [null]");
				NUnit.Framework.Assert.That(address.DocAddressType, Is.EqualTo(DocAddressType.Representative), "Type");
			});
		}

		[ExpectNoExceptions]
		public void TestRepresentativeDocAddress_Delete()
		{
			var deletedRepresentative = exitDetail.RepresentativeDocAddress;
			deletedRepresentative.Delete();
			NUnit.Framework.Assert.That(ReferenceEquals(exitDetail.RepresentativeDocAddress, deletedRepresentative), Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestDocAddresses()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(exitDetail.IsRegisteredEditableChildObject(exitDetail.DocAddresses), Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Registered Editable Child");
				NUnit.Framework.Assert.That(exitDetail.DocAddresses.IsLoaded, Is.EqualTo(true), "Is Loaded");
			});
		}

		[ExpectNoExceptions]
		public void TestGetDocAddressRequirement()
		{
			var iDocAddresses = (IDocAddresses)exitDetail;
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(iDocAddresses.GetDocAddressRequirement(DocAddressType.Declarant), Is.EqualTo(default(JobDocAddressRequirement)), "Declarant - should be [null]");
				NUnit.Framework.Assert.That(iDocAddresses.GetDocAddressRequirement(DocAddressType.Representative), Is.EqualTo(default(JobDocAddressRequirement)), "Representative - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestCanDeleteAddress()
		{
			var iDocAddresses = (IDocAddresses)exitDetail;
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(iDocAddresses.CanDeleteAddress(exitDetail.DeclarantDocAddress), Is.EqualTo(false), "Declarant");
				NUnit.Framework.Assert.That(iDocAddresses.CanDeleteAddress(exitDetail.RepresentativeDocAddress), Is.EqualTo(false), "Representative");
			});
		}

		[ExpectNoExceptions]
		public void TestGetCanOverrideCheckpoint()
		{
			var iDocAddresses = (IDocAddresses)exitDetail;
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(iDocAddresses.GetCanOverrideCheckpoint(exitDetail.DeclarantDocAddress), Is.EqualTo(Env.Security.None), "Declarant");
				NUnit.Framework.Assert.That(iDocAddresses.GetCanOverrideCheckpoint(exitDetail.RepresentativeDocAddress), Is.EqualTo(Env.Security.None), "Representative");
			});
		}

		[ExpectNoExceptions]
		public void TestPiggyBackedDocAddressValidation()
		{
			var iDocAddresses = (IDocAddresses)exitDetail;
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(iDocAddresses.PiggyBackedDocAddressValidation(exitDetail.DeclarantDocAddress), Is.EqualTo(default(ZValidation)), "Declarant - should be [null]");
				NUnit.Framework.Assert.That(iDocAddresses.PiggyBackedDocAddressValidation(exitDetail.RepresentativeDocAddress), Is.EqualTo(default(ZValidation)), "Representative - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestGetOrgHeaderList()
		{
			var iDocAddresses = (IDocAddresses)exitDetail;
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(iDocAddresses.GetOrgHeaderList(DocAddressType.Declarant), NUnit.Framework.Is.TypeOf<OrgHeaderCollection>(), "Declarant");
				NUnit.Framework.Assert.That(iDocAddresses.GetOrgHeaderList(DocAddressType.Representative), NUnit.Framework.Is.TypeOf<OrgHeaderCollection>(), "Representative");
			});
		}

		[ExpectNoExceptions]
		public void TestSupportedAddressTypes()
		{
			NUnit.Framework.Assert.That(((IDocAddresses)exitDetail).SupportedAddressTypes, NUnit.Framework.Is.EquivalentTo(new[] { DocAddressType.Declarant, DocAddressType.Representative }));
		}

		[ExpectNoExceptions]
		public void TestReferenceNumberUCR_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(exitDetail.ReferenceNumberUCRInfo).Caption, Is.EqualTo("Reference Number UCR"));
		}

		[ExpectNoExceptions]
		public void TestReferenceNumberUCR_MaxLength()
		{
			NUnit.Framework.Assert.That(exitDetail.ReferenceNumberUCRInfo.MaxLength, Is.EqualTo(35), "MaxLength");
		}

		[ExpectNoExceptions]
		public void TestReferenceNumberUCR_Getter()
		{
			var cusEntryNumber = CusEntryNumber.New(exitDetail, CusEntryNumberTypes.Standard.UniqueConsignementReference, Core.Constants.CountryCodes.Germany);
			cusEntryNumber.CE_EntryNum = "DEUCR123";
			NUnit.Framework.Assert.That(exitDetail.ReferenceNumberUCR, Is.EqualTo("DEUCR123").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestReferenceNumberUCR_Setter()
		{
			exitDetail.ReferenceNumberUCR = "DEUCR123";
			var cusEntryNumber = CusEntryNumber.Load(exitDetail, CusEntryNumberTypes.Standard.UniqueConsignementReference, Core.Constants.CountryCodes.Germany);
			NUnit.Framework.Assert.That(cusEntryNumber.CE_EntryNum, Is.EqualTo("DEUCR123").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRegistrationNumberAWB_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(exitDetail.RegistrationNumberAWBInfo).Caption, Is.EqualTo("Registration Number (ext.)"));
		}

		[ExpectNoExceptions]
		public void TestRegistrationNumberAWB_MaxLength()
		{
			NUnit.Framework.Assert.That(exitDetail.RegistrationNumberAWBInfo.MaxLength, Is.EqualTo(35), "MaxLength");
		}

		[ExpectNoExceptions]
		public void TestRegistrationNumberAWB_Getter()
		{
			var cusEntryNumber = CusEntryNumber.New(exitDetail, CusEntryNumberTypes.Germany.AirWaybillEntryNumber, Core.Constants.CountryCodes.Germany);
			cusEntryNumber.CE_EntryNum = "DEAWB123";
			NUnit.Framework.Assert.That(exitDetail.RegistrationNumberAWB, Is.EqualTo("DEAWB123").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRegistrationNumberAWB_Setter()
		{
			exitDetail.RegistrationNumberAWB = "DEAWB123";
			var cusEntryNumber = CusEntryNumber.Load(exitDetail, CusEntryNumberTypes.Germany.AirWaybillEntryNumber, Core.Constants.CountryCodes.Germany);
			NUnit.Framework.Assert.That(cusEntryNumber.CE_EntryNum, Is.EqualTo("DEAWB123").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdditionalInfos()
		{
			NUnit.Framework.Assert.That(exitDetail.AdditionalInfos, NUnit.Framework.Is.TypeOf<AdditionalInfoCollection>(), "AdditionalInfoCollection Type");
		}

		[ExpectNoExceptions]
		public void TestStatusDescription_Caption()
		{
			NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(exitDetail.StatusDescriptionInfo).Caption, Is.EqualTo("Status Description"));
		}

		[ExpectNoExceptions]
		public void TestStatusDescription_Getter()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "CSTEX");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "130", "Anmeldung angenommen", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			NUnit.Framework.Assert.Multiple(() =>
			{
				exitDetail.CED_Status = "130";
				NUnit.Framework.Assert.That(exitDetail.StatusDescription, Is.EqualTo("Anmeldung angenommen").Using(CustomComparers.TypeComparison), "Valid CED_Status");

				exitDetail.CED_Status = "ABC";
				NUnit.Framework.Assert.That(exitDetail.StatusDescription, Is.EqualTo(ZString.Empty), "Invalid CED_Status");
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			return exitHeader.CusExitDetails.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();

			exitDetail = Factory.NewWithValidTestData<CusExitDetail>();
		}
		CusExitDetail exitDetail;
	}
}
