using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestHeader()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<AsycudaManifestHeader>(bill.Header);
		}

		public void TestCusGoodsLocationType()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var cusGoodsLocationProvider = bill as ICusGoodsLocationProvider;

			CombineAssertions("CusGoodsLocation Type", () =>
			{
				AssertType<CusGoodsLocation>("ICusGoodsLocationProvider.GoodsLocation", cusGoodsLocationProvider.GoodsLocation);
				AssertType<CusGoodsLocation>("CusGoodsLocation", bill.CusGoodsLocation);
			});
		}

		public void TestDocumentationRequired()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals("Default value is empty", string.Empty, bill.DocumentationRequired);
			bill.DocumentationRequired = "S";
			Factory.Save();

			var billInNewFactory = NewFactory().Load<AsycudaBill>(bill.PK);
			AssertEquals("DocumentationRequired is persisted", "S", billInNewFactory.DocumentationRequired);

			var listAttribute = bill.DocumentationRequiredInfo.PropertyDescriptor.Attributes[typeof(ListAttribute)] as ListAttribute;
			AssertNotNull(listAttribute);
			AssertEquals("Lookups.DocumentationRequired", listAttribute.ListDataSourceMember);
		}

		public void TestDocumentationRequiredDescription()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals("Default value is empty", string.Empty, bill.DocumentationRequiredDescription);

			bill.DocumentationRequired = ESH7DocumentationRequiredList.Codes.Yes;
			AssertEquals("Yes", bill.DocumentationRequiredDescription);

			bill.DocumentationRequired = ESH7DocumentationRequiredList.Codes.No;
			AssertEquals("No", bill.DocumentationRequiredDescription);

			this.AssertDataBoundResourceStringsWithMultipleResourceKey(bill.DocumentationRequiredDescriptionInfo, multipleResourceKey: null, caption: "Documentation Required", shortCaption: "Doc. Req.", mediumCaption: "Doc. Required", fullDescription: "Indicates if documentation is required.");
			Assert("Is Readonly", bill.DocumentationRequiredDescriptionInfo.ReadOnly);
		}

		public void TestIsDocumentationRequested()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			Assert("Default is no", !bill.IsDocumentationRequested);

			bill.DocumentationRequired = "N";
			Assert("IsDocumentationRequested is false when DocumentationRequired = N", !bill.IsDocumentationRequested);

			bill.DocumentationRequired = "S";
			Assert("IsDocumentationRequested is true when DocumentationRequired = S", bill.IsDocumentationRequested);
		}

		public void TestLookups()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<AsycudaBillLookups>(bill.Lookups);
		}

		public void TestABLProcedureList()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var propertyInfo = bill.ABL_ProcedureInfo;
			var listAttribute = propertyInfo.GetAttribute<ListAttribute>();
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaBill), AsycudaBill.Schema.ABL_Procedure, true, attrib => attrib.ListDataSourceMember == "Lookups.AdditionalProcedureList");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			return header.Bills.AddNew();
		}

		public void TestABL_ConsigneeRegNoType_Caption()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var resourceStringDataAttribute = bill.ABL_ConsigneeRegNoTypeInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("ID NO. Type", resourceStringDataAttribute.Caption);
		}

		public void TestABL_SellerRegNoType()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals("IOS", bill.ABL_SellerRegNoType);
		}

		public void TestConsigneeRegNoAndType_CountryCodeIsES()
		{
			TestConsigneeRegNoAndType("ES", "EOR", "ES1234560", "EOR", "NIF", "TIN");
			TestConsigneeRegNoAndType("ES", "NIF", "1234560", "NIF", "TIN");
		}

		public void TestConsigneeRegNoAndType_CountryCodeIsNotES()
		{
			TestConsigneeRegNoAndType("AU", "EOR", "AU1234560", "EOR", "NIF", "TIN");
			TestConsigneeRegNoAndType("AU", "", "", "NIF", "TIN");
			TestConsigneeRegNoAndType("AU", "", "", "TIN");
		}

		void TestConsigneeRegNoAndType(string countryCode, string expectedType, string expectedRegNo, params string[] types)
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_OA_Consignee = GetOrgAddress(countryCode, types).PK;

			AssertEquals(expectedType, bill.ABL_ConsigneeRegNoType);
			AssertEquals(expectedRegNo, bill.ABL_ConsigneeRegNo);
		}

		public void TestHasAcceptedG3DMessage_WhenBillHasNoCusEntryNum_ShouldReturnFalse()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			Assert(!bill.HasAcceptedG3DMessage());
		}

		public void TestHasAcceptedG3DMessage_WhenBillHasEmptyCusEntryNum_ShouldReturnFalse()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.Header.G3MRNToRevoke = "MRN000123";
			var cusEntryNum = bill.CustomsEntryNumbers.AddNew();
			cusEntryNum.CE_EntryNum = string.Empty;
			cusEntryNum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNum.CE_EntryLineReference = G3EntryLineReference;

			Assert(!bill.HasAcceptedG3DMessage());
		}

		public void TestHasAcceptedG3DMessage_WhenBillHasCusEntryNumWithTypeOtherThanMRN_ShouldReturnFalse()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.Header.G3MRNToRevoke = "MRN000123";
			var cusEntryNum = bill.CustomsEntryNumbers.AddNew();
			cusEntryNum.CE_EntryNum = "MRN000123";
			cusEntryNum.CE_EntryType = CusEntryNumberTypes.EU.LocalReferenceNumber;
			cusEntryNum.CE_EntryLineReference = G3EntryLineReference;

			Assert(!bill.HasAcceptedG3DMessage());
		}

		public void TestHasAcceptedG3DMessage_WhenBillHasCusEntryNumWithLineReferenceOtherThanG3_ShouldReturnFalse()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.Header.G3MRNToRevoke = "MRN000123";
			var cusEntryNum = bill.CustomsEntryNumbers.AddNew();
			cusEntryNum.CE_EntryNum = "MRN000123";
			cusEntryNum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNum.CE_EntryLineReference = H7EntryLineReference;

			Assert(!bill.HasAcceptedG3DMessage());
		}

		public void TestHasAcceptedG3DMessage_WhenHeaderHasNoG3MRNToRevokeAndBillHasCusEntryNumWithTypeMRNAndLineReferenceG3_ShouldReturnFalse()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var cusEntryNum = bill.CustomsEntryNumbers.AddNew();
			cusEntryNum.CE_EntryNum = "MRN000123";
			cusEntryNum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNum.CE_EntryLineReference = G3EntryLineReference;

			Assert(!bill.HasAcceptedG3DMessage());
		}

		public void TestHasAcceptedG3DMessage_WhenBillHasValidCusEntryNumWithTypeMRNAndLineReferenceG3_ShouldReturnTrue()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.G3MRNToRevoke = "MRN000123";

			var bill1 = header.Bills.AddNew();
			var cusEntryNum1 = bill1.CustomsEntryNumbers.AddNew();
			cusEntryNum1.CE_EntryNum = "MRN000123";
			cusEntryNum1.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNum1.CE_EntryLineReference = G3EntryLineReference;

			var bill2 = header.Bills.AddNew();
			var cusEntryNum2 = bill2.CustomsEntryNumbers.AddNew();
			cusEntryNum2.CE_EntryNum = "MRN000456";
			cusEntryNum2.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNum2.CE_EntryLineReference = G3EntryLineReference;

			CombineAssertions(() =>
			{
				Assert("CE_EntryNum equals G3MRNToRevoke", bill1.HasAcceptedG3DMessage());
				Assert("CE_EntryNum not equals G3MRNToRevoke", !bill2.HasAcceptedG3DMessage());
			});
		}

		public void TestClearanceReferenceNumber()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;

			AssertNullOrEmpty("ClearanceReferenceNumber doesn't exist", bill.ClearanceReferenceNumber);

			var clearanceReferenceNumber = bill.CustomsEntryNumbers.AddNew();
			clearanceReferenceNumber.CE_EntryType = "CLR";
			clearanceReferenceNumber.CE_EntryLineReference = "H7";
			clearanceReferenceNumber.CE_EntryNum = "12345";

			AssertEquals("ClearanceReferenceNumber fetches H7 CLR entryNum", "12345", bill.ClearanceReferenceNumber);
		}

		public void TestG3LRN()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.G3LocalReferenceNumber = "G3123";
			bill.LocalReferenceNumber = "123";

			CombineAssertions(() =>
			{
				var entryNum = Factory.LoadTop1<ABLEntryNum>(CreateCusEntryNumFilter(bill, CusEntryNumberTypes.EU.LocalReferenceNumber));
				AssertEquals("LRN", "123", entryNum.CE_EntryNum);

				var g3EntryNum = Factory.LoadTop1<ABLEntryNum>(CreateCusEntryNumFilter(bill, CusEntryNumberTypes.EU.LocalReferenceNumber, G3EntryLineReference));
				AssertEquals("G3 LRN", "G3123", g3EntryNum.CE_EntryNum);
			});
		}

		public void TestG3LocalReferenceNumberCaptions()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var resourceStringDataAttribute = bill.G3LocalReferenceNumberInfo.GetAttribute<ResourceStringDataAttribute>();

			CombineAssertions(() =>
			{
				AssertEquals("G3 Local Reference Number", resourceStringDataAttribute.Caption);
				AssertEquals("LRN (G3)", resourceStringDataAttribute.ShortCaption);
				AssertEquals("LRN (G3)", resourceStringDataAttribute.MediumCaption);
				AssertEquals("A system-generated local reference number to uniquely identify each single G3 declaration.", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestG3LocalReferenceNumberMaxLength()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var descriptor = bill.G3LocalReferenceNumberInfo.PropertyDescriptor;
			var maxLengthAttribute = descriptor.Attributes[typeof(MaxLengthAttribute)] as MaxLengthAttribute;

			AssertEquals(35, maxLengthAttribute.MaxLength);
		}

		public void TestG3MRN()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.G3MovementReferenceNumber = "123";

			var mrn = Factory.LoadTop1<ABLEntryNum>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertEquals("G3 MRN EntryNum", "123", mrn.CE_EntryNum);
				AssertEquals("G3 MRN EntryType", "MRN", mrn.CE_EntryType);
				AssertEquals("G3 MRN CountryCode", "ES", mrn.CE_RN_NKCountryCode);
				AssertEquals("G3 MRN EntryLineReference", "G3", mrn.CE_EntryLineReference);
				AssertEquals("G3 MRN ParentID", bill.PK, mrn.CE_ParentID);
				AssertEquals("bill.G3RevokedMovementReferenceNumber", "123", bill.G3MovementReferenceNumber);
			});
		}

		public void TestG3RevokedMRN()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.G3RevokedMovementReferenceNumber = "123";

			var mrn = Factory.LoadTop1<ABLEntryNum>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertEquals("G3REVOKED MRN EntryNum", "123", mrn.CE_EntryNum);
				AssertEquals("G3REVOKED MRN EntryType", "MRN", mrn.CE_EntryType);
				AssertEquals("G3REVOKED MRN CountryCode", "ES", mrn.CE_RN_NKCountryCode);
				AssertEquals("G3REVOKED MRN EntryLineReference", "G3REVOKED", mrn.CE_EntryLineReference);
				AssertEquals("G3REVOKED MRN ParentID", bill.PK, mrn.CE_ParentID);
				AssertEquals("bill.G3RevokedMovementReferenceNumber", "123", bill.G3RevokedMovementReferenceNumber);
			});
		}

		public void TestG3RevokedMRNAndMRNSetCorrectly()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.G3MovementReferenceNumber = "123";
			bill.G3RevokedMovementReferenceNumber = "456";

			var mrns = Factory.Load<ABLEntryNum>(new ZQuery());
			var mrn = mrns.FirstOrDefault(m => m.CE_EntryLineReference == "G3");
			var revokedMRN = mrns.FirstOrDefault(m => m.CE_EntryLineReference == "G3REVOKED");

			CombineAssertions(() =>
			{
				AssertEquals("G3 MRN EntryNum", "123", mrn.CE_EntryNum);
				AssertEquals("G3 MRN EntryType", "MRN", mrn.CE_EntryType);
				AssertEquals("G3 MRN CountryCode", "ES", mrn.CE_RN_NKCountryCode);
				AssertEquals("G3 MRN EntryLineReference", "G3", mrn.CE_EntryLineReference);
				AssertEquals("G3 MRN ParentID", bill.PK, mrn.CE_ParentID);
				AssertEquals("bill.G3MovementReferenceNumber", "123", bill.G3MovementReferenceNumber);

				AssertEquals("G3REVOKED MRN EntryNum", "456", revokedMRN.CE_EntryNum);
				AssertEquals("G3REVOKED MRN EntryType", "MRN", revokedMRN.CE_EntryType);
				AssertEquals("G3REVOKED MRN CountryCode", "ES", revokedMRN.CE_RN_NKCountryCode);
				AssertEquals("G3REVOKED MRN EntryLineReference", "G3REVOKED", revokedMRN.CE_EntryLineReference);
				AssertEquals("G3REVOKED MRN ParentID", bill.PK, revokedMRN.CE_ParentID);
				AssertEquals("bill.G3RevokedMovementReferenceNumber", "456", bill.G3RevokedMovementReferenceNumber);
			});
		}

		public void TestG3MovementReferenceNumberCaptions()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var resourceStringDataAttribute = bill.G3MovementReferenceNumberInfo.GetAttribute<ResourceStringDataAttribute>();

			CombineAssertions(() =>
			{
				AssertEquals("G3 Movement Reference Number", resourceStringDataAttribute.Caption);
				AssertEquals("MRN (G3)", resourceStringDataAttribute.ShortCaption);
				AssertEquals("MRN (G3)", resourceStringDataAttribute.MediumCaption);
				AssertEquals("A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestG3MovementReferenceNumberMaxLength()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var descriptor = bill.G3MovementReferenceNumberInfo.PropertyDescriptor;
			var maxLengthAttribute = descriptor.Attributes[typeof(MaxLengthAttribute)] as MaxLengthAttribute;

			AssertEquals(35, maxLengthAttribute.MaxLength);
		}

		public void TestH7MovementReferenceNumber()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.MovementReferenceNumber = "0001";

			CombineAssertions(() =>
			{
				bill.H7MovementReferenceNumber = "H70001";

				var h7EntryNum = Factory.LoadTop1<ABLEntryNum>(CreateCusEntryNumFilter(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, H7EntryLineReference));
				AssertEquals("CusEntryNumber should be created", "H70001", h7EntryNum.CE_EntryNum);
				AssertEquals("Expected created H7MRN", "H70001", bill.H7MovementReferenceNumber);

				bill.H7MovementReferenceNumber = "H70001-new";

				AssertEquals("CusEntryNumber should be updated", "H70001-new", h7EntryNum.CE_EntryNum);
				AssertEquals("Expected updated H7MRN", "H70001-new", bill.H7MovementReferenceNumber);
			});
		}

		public void TestH7MovementReferenceNumberCaptions()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var resourceStringDataAttribute = bill.H7MovementReferenceNumberInfo.GetAttribute<ResourceStringDataAttribute>();

			CombineAssertions(() =>
			{
				AssertEquals("H7 Movement Reference Number", resourceStringDataAttribute.Caption);
				AssertEquals("MRN (H7)", resourceStringDataAttribute.ShortCaption);
				AssertEquals("MRN (H7)", resourceStringDataAttribute.MediumCaption);
				AssertEquals("A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestH7MovementReferenceNumberMaxLength()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var descriptor = bill.H7MovementReferenceNumberInfo.PropertyDescriptor;
			var maxLengthAttribute = descriptor.Attributes[typeof(MaxLengthAttribute)] as MaxLengthAttribute;

			AssertEquals(35, maxLengthAttribute.MaxLength);
		}

		public void TestPreviousDocuments()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<PreviousDocumentCollection<PreviousDocument>>(bill.PreviousDocuments);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var bill = Factory.New<AsycudaBill>();
			var actualTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes();
			AssertEquals(typeof(SupportingDocument), actualTypes[CusSupportingInfoTypeList.Codes.SupportingDocument]);
			AssertEquals(typeof(PreviousDocument), actualTypes[CusSupportingInfoTypeList.Codes.PreviousDocument]);
		}

		#region IESMessageInfoProvider

		public void TestBroker()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var staffWithCertificateHelper = new StaffWithCertificateTestHelper(Factory);
			bill.Header.AMA_GS_NKCustomsAgent = staffWithCertificateHelper.Staff.GS_Code;

			var messageInfoProvider = bill as IESMessageInfoProvider;
			AssertEquals(staffWithCertificateHelper.Staff.GS_Code, messageInfoProvider.Broker.GS_Code);
		}

		public void TestMRN()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var cusEntryNum = bill.CustomsEntryNumbers.AddNew();
			cusEntryNum.CE_EntryNum = "MRN000123";
			cusEntryNum.CE_EntryType = "MRN";

			var messageInfoProvider = bill as IESMessageInfoProvider;
			AssertEquals("MRN000123", messageInfoProvider.MRN);
		}

		public void TestDocumentJobReference()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var cusEntryNum = bill.CustomsEntryNumbers.AddNew();
			cusEntryNum.CE_EntryNum = "MRN000123";
			cusEntryNum.CE_EntryType = "MRN";

			var messageInfoProvider = bill as IESMessageInfoProvider;
			AssertEquals("MRN000123", messageInfoProvider.DocumentJobReference);
		}

		#endregion

		#region IESResponseBOMessageStatus

		public void TestMessageStatus()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_MessageStatus = string.Empty;

			var responseBO = bill as IESResponseBOMessageStatus;
			responseBO.MessageStatus = "ACC";
			AssertEquals("ACC", bill.ABL_MessageStatus);
		}

		public void TestBranchPK()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var branch = Factory.New<GlbBranch>();
			bill.Header.AMA_GB = branch.PK;

			var responseBO = bill as IESResponseBusinessObject;
			AssertEquals(branch.PK, responseBO.BranchPK);
		}

		public void TestMessageCollection()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.Messages.AddNew();
			bill.Messages.AddNew();

			var responseBO = bill as IESResponseBusinessObject;
			AssertEquals(2, responseBO.MessageCollection.Count);
		}

		public void TestEntryReference()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_BillNumber = "B00000123";

			var messageBO = bill as IESMessageBusinessObject;
			AssertEquals("B00000123", messageBO.EntryReference);
		}

		#endregion

		OrgAddress GetOrgAddress(string countryCode, params string[] types)
		{
			var org = Factory.New<OrgHeader>();
			var address = org.Addresses.AddNew();
			var count = 0;

			foreach (var type in types)
			{
				var config = org.CustomsCodes.AddNew();
				config.OK_RN_NKCodeCountry = countryCode;
				config.OK_CodeType = type;
				config.SecuredCustomsRegNo = "123456" + count;
				count++;
			}

			return address;
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString> { AutoAsycudaBill.Schema.ABL_SellerRegNoType };
		}

		ZQuery CreateCusEntryNumFilter(AsycudaBill bill, string entryType, string entryLineReference = "")
		{
			var filter2 = new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, bill.PK);
			filter2.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, entryType);
			filter2.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryLineReference, SQLComparisonOperator.Equal, entryLineReference);
			return filter2;
		}

		public void TestTryConvert()
		{
			var bill = BuildBill();
			var converter = new StandAloneDeclarationConverter();
			converter.TryConvert(new List<AsycudaBill> { bill, BuildBill() });

			var newFactory = new BusinessObjectFactory();
			var reloadedBill = newFactory.Load<AsycudaBill>(bill.PK);
			var declaration = newFactory.LoadTop1<ES.Business.Declaration.JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, reloadedBill.EntrySummaryReferenceNumber));

			foreach (var instruction in declaration.CustomsEntryInstructions)
			{
				AssertEquals("IM", instruction.CEI_Style);
				AssertEquals("A", instruction.CEI_SubStyle);
			}
		}

		AsycudaBill BuildBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			var pack2 = bill.Packs.AddNew();
			var item1 = bill.PackedItems.AddNew();
			var item2 = bill.PackedItems.AddNew();

			bill.ABL_BillNumber = "BN123";
			bill.ABL_GoodsDescription = "goods description";
			bill.ABL_GrossWeight = 0.9;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_Volume = 1.1;
			bill.ABL_VolumeUQ = "L";
			bill.ABL_ManifestQty = 13;
			bill.ABL_GoodsValue = 110;
			bill.ABL_RX_NKGoodsValueCurrency = "AUD";
			bill.ABL_PrepaidCollect = "PPD";
			bill.ABL_SellerRegNo = "1234";

			pack1.APA_PackUQ = "BX";
			pack2.APA_PackUQ = "FR";
			item1.API_FormattedTariff = "11081990009";
			item1.API_GoodsDescription = "item1 goods description";
			item1.API_RN_NKGoodsOrigin = "AU";
			item1.API_RX_NKGoodsValueCurrency = "AUD";
			item2.API_FormattedTariff = "11081990010";
			item2.API_GoodsDescription = "item2 goods description";
			item2.API_RN_NKGoodsOrigin = "US";
			item2.API_RX_NKGoodsValueCurrency = "USD";
			Factory.Save();
			return bill;
		}

		const string H7EntryLineReference = "H7";
		const string G3EntryLineReference = "G3";
	}
}
