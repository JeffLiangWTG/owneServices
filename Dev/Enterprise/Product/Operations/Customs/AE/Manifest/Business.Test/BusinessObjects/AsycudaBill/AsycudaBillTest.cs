using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.AE.Registry;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(AsycudaBill))]
sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
{
	[ExpectNoExceptions]
	public void TestCodePropertyAttribute() => CombineAssertions(() =>
	{
		var attribute = (CodePropertyAttribute)Attribute.GetCustomAttribute(typeof(AsycudaBill), typeof(CodePropertyAttribute));
		NUnit.Framework.Assert.That(attribute, Is.Not.Null);
		NUnit.Framework.Assert.That(attribute.PropertyName, Is.EqualTo(nameof(AsycudaBill.ABL_BillNumber)));
	});

	[ExpectNoExceptions]
	public void TestCodeDescriptionAttribute() => CombineAssertions(() =>
	{
		var attribute = (DescriptionPropertyAttribute)Attribute.GetCustomAttribute(typeof(AsycudaBill), typeof(DescriptionPropertyAttribute));
		NUnit.Framework.Assert.That(attribute, Is.Not.Null);
		NUnit.Framework.Assert.That(attribute.PropertyName, Is.EqualTo(nameof(AsycudaBill.ABL_BillNumber)));
	});

	[ExpectNoExceptions]
	public void TestGetPackedItemTypeCore()
	{
		NUnit.Framework.Assert.That(Bill.GetPackedItemType(), Is.EqualTo(typeof(AsycudaPackedItem)));
	}

	[ExpectNoExceptions]
	public void TestABL_CargoTypeAttributes()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Bill.ABL_CargoTypeInfo);
		NUnit.Framework.Assert.That(resData.Caption, Is.EqualTo("Cargo Type"), "Caption");
		NUnit.Framework.Assert.That(Bill.ABL_CargoTypeInfo.MaxLength, Is.EqualTo(ManifestBase.AutoAsycudaBill.Schema.ABL_CargoTypeMaxLength), "MaxLength");
	}

	[ExpectNoExceptions]
	public void TestABL_SpecialCargoCodeCaption()
	{
		CombineAssertions(() =>
		{
			var resData = DataBoundResourceStrings.GetDataForProperty(Bill.ABL_SpecialCargoCodeInfo);
			NUnit.Framework.Assert.That(resData.Caption, Is.EqualTo("Service Requirement"), "Caption");
			NUnit.Framework.Assert.That(resData.MediumCaption, Is.EqualTo("Service Req."), "Medium Caption");
			NUnit.Framework.Assert.That(resData.ShortCaption, Is.EqualTo("Serv. Req."), "Short Caption");
		});
	}

	[ExpectNoExceptions]
	public void TestABL_BillStatus_ReadOnly() => NUnit.Framework.Assert.That(Bill.ABL_BillStatusInfo.ReadOnly, Is.EqualTo(true));

	[ExpectNoExceptions]
	public void TestABL_FreightValueCaption()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Bill.ABL_FreightValueInfo);
		NUnit.Framework.Assert.That(resData.Caption, Is.EqualTo("Goods Value"), "Caption");
	}

	[ExpectNoExceptions]
	public void TestABL_RX_NKFreightValueCurrencyCaption()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Bill.ABL_RX_NKFreightValueCurrencyInfo);
		NUnit.Framework.Assert.That(resData.Caption, Is.EqualTo("Goods Currency"), "Caption");
	}

	[ExpectNoExceptions]
	public void TestLookups() => NUnit.Framework.Assert.That(Bill.Lookups, Is.TypeOf<AsycudaBillLookups>());

	[ExpectNoExceptions]
	public void TestValidation() => CombineAssertions(() =>
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var child = header.MasterBill;
		var realBill = header.Bills.AddNew();
		NUnit.Framework.Assert.That(realBill.Validation, Is.TypeOf<AsycudaBillValidationForRegularBill>(), "Regular");
		NUnit.Framework.Assert.That(child.Validation, Is.TypeOf<AsycudaBillValidationForMasterChild>(), "Master");
	});

	[ExpectNoExceptions]
	public void TestOnSaving_ABL_SenderReference()
	{
		CombineAssertions(() =>
		{
			using (AECustomsRegistry.Instance.NAICServiceProviderCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SENDER"))
			{
				SaveBillWithReference(ZString.Empty);
				NUnit.Framework.Assert.That(Bill.ABL_SenderReference, Is.EqualTo("0SENDER0000000000001").Using(CustomComparers.TypeComparison), "Bill Sender Reference set using NumberFountain");

				SaveBillWithReference("REF123");
				NUnit.Framework.Assert.That(Bill.ABL_SenderReference, Is.EqualTo("REF123").Using(CustomComparers.TypeComparison), "Sender Reference already set");

				SaveBillWithReference(ZString.Empty);
				NUnit.Framework.Assert.That(Bill.ABL_SenderReference, Is.EqualTo("0SENDER0000000000002").Using(CustomComparers.TypeComparison), "Same key, next Reference sequence");
			}

			using (AECustomsRegistry.Instance.NAICServiceProviderCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "NEW_SENDER_ID"))
			{
				SaveBillWithReference(ZString.Empty);
				NUnit.Framework.Assert.That(Bill.ABL_SenderReference, Is.EqualTo("NEW_SEN0000000000001").Using(CustomComparers.TypeComparison), "Different key, Reference sequence reset");
			}
		});

		void SaveBillWithReference(ZString reference)
		{
			Bill.ABL_SenderReference = reference;
			Bill.OnSaving();
		}
	}

	[ExpectNoExceptions]
	public void TestIMessageAttachee_JobNumber()
	{
		Bill.ABL_BillNumber = "XXX";
		NUnit.Framework.Assert.That(((IMessageAttachee)Bill).JobNumber, Is.EqualTo("XXX"));
	}

	[ExpectNoExceptions]
	public void TestIMessageAttachee_EntryStatus() => CombineAssertions(() =>
	{
		var messageAttachee = (IMessageAttachee)Bill;

		Bill.ABL_BillStatus = "UNK";
		NUnit.Framework.Assert.That(messageAttachee.EntryStatus, Is.EqualTo("UNK").Using(CustomComparers.TypeComparison), "EntryStatus Getter");

		var manifestHeader = Bill.Header;
		manifestHeader.AMA_ManifestType = AEManifestTypes.Codes.ACI;
		messageAttachee.EntryStatus = "ACK";
		NUnit.Framework.Assert.That(Bill.ABL_BillStatus, Is.EqualTo("ACK").Using(CustomComparers.TypeComparison), "EntryStatus Setter - Bill Status is set to ACK");
		NUnit.Framework.Assert.That(manifestHeader.RegistrationStatus, Is.EqualTo("ACK").Using(CustomComparers.TypeComparison), "EntryStatus Setter - Header Customs Status is set to ACK");

		var bill2 = manifestHeader.Bills.AddNew();
		((IMessageAttachee)bill2).EntryStatus = "ERR";
		NUnit.Framework.Assert.That(manifestHeader.RegistrationStatus.ToString(), Is.EqualTo(AsycudaManifestHeader.StatusCodeMultiple), "EntryStatus Setter - Header Customs Status is updated to MULTIPLE");
	});

	[ExpectNoExceptions]
	public void TestIMessageAttachee_MessageStatus() => CombineAssertions(() =>
	{
		var messageAttachee = (IMessageAttachee)Bill;

		Bill.ABL_MessageStatus = "CTL";
		NUnit.Framework.Assert.That(messageAttachee.MessageStatus, Is.EqualTo("CTL").Using(CustomComparers.TypeComparison), "MessageStatus Getter");

		var manifestHeader = Bill.Header;
		manifestHeader.AMA_ManifestType = AEManifestTypes.Codes.ACI;
		messageAttachee.MessageStatus = "RES";
		NUnit.Framework.Assert.That(Bill.ABL_MessageStatus, Is.EqualTo("RES").Using(CustomComparers.TypeComparison), "MessageStatus Setter - Bill Message Status is set to RES");
		NUnit.Framework.Assert.That(manifestHeader.MessageStatus, Is.EqualTo("RES").Using(CustomComparers.TypeComparison), "MessageStatus Setter - Header Message Status is set to RES");

		var bill2 = manifestHeader.Bills.AddNew();
		((IMessageAttachee)bill2).MessageStatus = "CTL";
		NUnit.Framework.Assert.That(manifestHeader.MessageStatus.ToString(), Is.EqualTo(AsycudaManifestHeader.StatusCodeMultiple), "MessageStatus Setter - Header Message Status is updated to MULTIPLE");
	});

	[ExpectNoExceptions]
	public void TestIMessageAttachee_DocumentIdentifier() => CombineAssertions(() =>
	{
		var billIdentifierProvider = (IMessageAttachee)Bill;
		NUnit.Framework.Assert.That(billIdentifierProvider.DocumentIdentifier.ToString(), Is.Null.Or.Empty, "Document Identifier not set - should be [null] or [empty]");

		Bill.ABL_SenderReference = "ABC";
		NUnit.Framework.Assert.That(billIdentifierProvider.DocumentIdentifier, Is.EqualTo("ABC").Using(CustomComparers.TypeComparison), "Document Identifier set");
	});

	[ExpectNoExceptions]
	public void TestDataGrouping()
	{
		var manifestHeader = Factory.New<AsycudaManifestHeader>();
		manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedArabEmirates;
		var bill = manifestHeader.Bills.AddNew();
		NUnit.Framework.Assert.That(bill.DataGrouping, Is.EqualTo(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UAEManifest).Using(CustomComparers.TypeComparison), "Data Grouping");
	}

	[ExpectNoExceptions]
	public void TestIMessageAttachee_Messages() => NUnit.Framework.Assert.That(((IMessageAttachee)Bill).Messages, Is.SameAs(Bill.Messages));

	[ExpectNoExceptions]
	public void TestPacks() => NUnit.Framework.Assert.That(Bill.Packs, Is.TypeOf<ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>>());

	[ExpectNoExceptions]
	public void TestABL_OA_ContainerAgent_ZAddress()
	{
		var orgAddress = Factory.New<OrgAddress>();
		var orgHeader = Factory.New<OrgHeader>();
		orgAddress.OA_OH = orgHeader.PK;
		Bill.ABL_OA_ContainerAgent = orgAddress.PK;
		NUnit.Framework.Assert.That(Bill.ABL_OA_ContainerAgent_ZAddress.DefaultAddressType, Is.EqualTo(AddressType.DLV), "Default Address Type");
	}

	[ExpectNoExceptions]
	public void TestABL_OA_DeliveryAgent_ZAddress()
	{
		var orgAddress = Factory.New<OrgAddress>();
		var orgHeader = Factory.New<OrgHeader>();
		orgAddress.OA_OH = orgHeader.PK;
		Bill.ABL_OA_DeliveryAgent = orgAddress.PK;
		NUnit.Framework.Assert.That(Bill.ABL_OA_DeliveryAgent_ZAddress.DefaultAddressType, Is.EqualTo(AddressType.DLV), "Default Address Type");
	}

	[ExpectNoExceptions]
	public void TestIsTSS() => CombineAssertions(() =>
	{
		Bill.ABL_ShipmentType = ShipmentTypeList.Codes.Transhipment28;
		NUnit.Framework.Assert.That(Bill.IsTSS, Is.True, "Is transshipment");

		Bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
		NUnit.Framework.Assert.That(!Bill.IsTSS, Is.True, "Is not transshipment");
	});

	[ExpectNoExceptions]
	public void TestNegotiable_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.Negotiable)
			.WithCaption("Negotiable?")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 1)
			.WithList(nameof(AsycudaBill.Lookups) + "." + nameof(AsycudaBillLookups.NegotiableList)));

	[ExpectNoExceptions]
	public void TestPayer_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.Payer)
			.WithCaption("Payer")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 70)
			.WithAttribute<ReadOnlyMemberAttribute>(x => x.Member == nameof(AsycudaBill.Payer_ReadOnly)));

	[ExpectNoExceptions]
	public void TestPayer() => CombineAssertions(() =>
	{
		Bill.Negotiable = NegotiableList.Codes.No;
		NUnit.Framework.Assert.That(Bill.PayerInfo.ReadOnly, Is.True, "Negotiable is No, Payer is non-editable");

		Bill.Negotiable = NegotiableList.Codes.Yes;
		NUnit.Framework.Assert.That(Bill.PayerInfo.ReadOnly, Is.False, "Negotiable is Yes, Payer is editable");
		Bill.Payer = "ABC";
		Bill.Negotiable = NegotiableList.Codes.No;
		NUnit.Framework.Assert.That(Bill.Payer, Is.EqualTo(ZString.Empty), "Negotiable changes, Payer is cleared");
	});

	[ExpectNoExceptions]
	public void TestABL_SplitBillNumber() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ABL_SplitBillNumber)
			.WithCaption("Split Bill Number")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 34)
			.WithList(nameof(AsycudaBill.Lookups) + "." + nameof(AsycudaBillLookups.SplitBills)));

	[ExpectNoExceptions]
	public void TestABL_SplitBillNumberChanged()
	{
		const string testValue = "test";
		CombineAssertions(() =>
		{
			Bill.ABL_SplitBill = true;
			Bill.ABL_SplitBillNumber = "test";
			AssertEquals(testValue, Bill.ABL_SplitBillNumber);

			Bill.ABL_SplitBill = false;
			AssertEquals("Split Bill Number changed", ZString.Empty, Bill.ABL_SplitBillNumber);
		});
	}

	[ExpectNoExceptions]
	public void TestABL_SplitBill() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ABL_SplitBill)
			.WithCaption("Split Bill"));

	[ExpectNoExceptions]
	public void TestSetABL_BolType_DefaultingForwarder() => CombineAssertions(() =>
	{
		var orgHeader = Factory.New<OrgHeader>();
		var orgAddress = orgHeader.Addresses.AddNew();
		Bill.Header.AMA_OA_ShippingAgent = orgAddress.PK;

		Bill.ABL_BolType = "CLD";
		NUnit.Framework.Assert.That(Bill.ABL_OA_Forwarder, Is.EqualTo(orgAddress.PK).Using(CustomComparers.TypeComparison), "Defaulting ABL_OA_Forwarder to Header's AMA_OA_ShippingAgent when ABL_BolType set to CLD");

		Bill.ABL_OA_Forwarder = ZGuid.Empty;
		Bill.ABL_BolType = "STD";
		NUnit.Framework.Assert.That(Bill.ABL_OA_Forwarder, Is.EqualTo(ZGuid.Empty).Using(CustomComparers.TypeComparison), "No Defaulting when ABL_BolType is not CLD");
	});

	[ExpectNoExceptions]
	public void TestForwarderMPCI_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaBill>()
			.HasProperty(x => x.ForwarderMPCI)
			.WithCaption("FF MPCI ID"));

	protected override BusinessObject GetNewBusinessObject()
	{
		return CreateBusinessObject(Factory);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<AsycudaManifestHeader>().Bills.AddNew();

	AsycudaBill CreateBusinessObject(BusinessObjectFactory factory)
	{
		var manifestHeader = factory.New<AsycudaManifestHeader>();
		var bill = manifestHeader.Bills.AddNew();
		return bill;
	}

	AsycudaBill Bill => bill ??= CreateBusinessObject(Factory);
	AsycudaBill bill;
}
