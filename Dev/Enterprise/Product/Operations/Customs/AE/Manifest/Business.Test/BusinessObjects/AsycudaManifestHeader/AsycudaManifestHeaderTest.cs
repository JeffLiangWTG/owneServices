using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AE.Registry;
using Enterprise.Customs.ManifestBase;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(AsycudaManifestHeader))]
sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
{
	[ExpectNoExceptions]
	public void TestPackedItemRelationship()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		NUnit.Framework.Assert.That(header.PackedItemRelationship, Is.EqualTo(AsycudaPackPackedItemPivotCollection.RelationshipType.One));
	}

	[ExpectNoExceptions]
	public void TestShowPackedItems()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		NUnit.Framework.Assert.That(header.ShowPackedItems, Is.EqualTo(false));
	}

	[ExpectNoExceptions]
	public void TestGetNewMessageChooser()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		NUnit.Framework.Assert.That(header.GetNewMessageChooser(new[] { bill }, string.Empty, false), Is.TypeOf<MessageChooser>());
	}

	[ExpectNoExceptions]
	public void TestAMA_CustomOriginPortAttributes()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var resData = DataBoundResourceStrings.GetDataForProperty(header.AMA_CustomsOriginPortInfo);
		NUnit.Framework.Assert.That(resData.Caption, Is.EqualTo("Last Foreign Port"), "Caption");
		NUnit.Framework.Assert.That(header.AMA_CustomsOriginPortInfo.MaxLength, Is.EqualTo(AsycudaManifestHeader.Schema.AMA_CustomsOriginPortMaxLength), "MaxLength");
	}

	[ExpectNoExceptions]
	public void TestGetBillType()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		NUnit.Framework.Assert.That(header.GetBillType(), Is.EqualTo(typeof(AsycudaBill)), "Bill type");
	}

	[ExpectNoExceptions]
	public void TestGetContainerType()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		NUnit.Framework.Assert.That(header.GetContainerType(), Is.EqualTo(typeof(AsycudaContainer)), "Container type");
	}

	[ExpectNoExceptions]
	public void TestBills()
	{
		NUnit.Framework.Assert.That(Factory.New<AsycudaManifestHeader>().Bills, Is.TypeOf<AsycudaBillCollection>(), "Type");
	}

	[ExpectNoExceptions]
	public void TestContainers()
	{
		NUnit.Framework.Assert.That(Factory.New<AsycudaManifestHeader>().Containers, Is.TypeOf<AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>>(), "Type");
	}

	[ExpectNoExceptions]
	public void TestValidation()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		NUnit.Framework.Assert.That(header.Validation, Is.TypeOf<AsycudaManifestHeaderValidation>());
	}

	[ExpectNoExceptions]
	public void TestLookups()
	{
		var header = CreateBusinessObject(Factory);
		NUnit.Framework.Assert.That(header.Lookups, Is.TypeOf<AsycudaManifestHeaderLookups>());
	}

	[ExpectNoExceptions]
	public void TestAMA_CarrierCodeAttributes()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		NUnit.Framework.Assert.That(header.AMA_CarrierCodeInfo.MaxLength, Is.EqualTo(3), "MaxLength should be 3.");
	}

	[ExpectNoExceptions]
	public void TestAMA_OA_Carrier()
	{
		var org = Factory.New<OrgHeader>();
		org.OH_Code = "VVV";
		var address = org.Addresses.AddNew();
		org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "MSC", Core.Constants.CountryCodes.UnitedArabEmirates);

		var header = Factory.New<AsycudaManifestHeader>();
		header.AMA_OA_Carrier = address.PK;

		NUnit.Framework.Assert.That(header.AMA_CarrierCode, Is.EqualTo("MSC").Using(CustomComparers.TypeComparison));
	}

	[ExpectNoExceptions]
	public void TestAMA_OA_ShippingAgentAttributes()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var resData = DataBoundResourceStrings.GetDataForProperty(header.AMA_OA_ShippingAgentInfo);
		NUnit.Framework.Assert.That(resData.Caption, Is.EqualTo("Default Freight Forwarder"), "AMA_OA_ShippingAgent Full Caption");
		NUnit.Framework.Assert.That(resData.ShortCaption, Is.EqualTo("Default Frt. Forwarder"), "AMA_OA_ShippingAgent Short Caption");
	}

	[ExpectNoExceptions]
	public void TestMessageStatus()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		header.AMA_MessageStatus = ZString.Empty;
		NUnit.Framework.Assert.That(header.MessageStatus.ToString(), Is.Empty, "AMA_MessageStatus not set, no bills");

		var bill1 = header.Bills.AddNew();
		var bill2 = header.Bills.AddNew();
		bill1.ABL_MessageStatus = "A";
		bill2.ABL_MessageStatus = "B";
		NUnit.Framework.Assert.That(header.MessageStatus.ToString(), Is.EqualTo(AsycudaManifestHeader.StatusCodeMultiple)
						, "AMA_MessageStatus not set, MessageStatus from bills Message Statuses");

		header.AMA_MessageStatus = "C";
		NUnit.Framework.Assert.That(header.MessageStatus.ToString(), Is.EqualTo("C"), "AMA_MessageStatus is set");
	}

	[ExpectNoExceptions]
	public void TestCarrierMPCI_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaManifestHeader>()
			.HasProperty(x => x.CarrierMPCI)
			.WithCaption("MPCI ID"));

	[ExpectNoExceptions]
	public void TestCarrierMPCI()
	{
		var org = Factory.New<OrgHeader>();
		org.CustomsCodes.AddNew(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, "1234567", Core.Constants.CountryCodes.UnitedArabEmirates);
		var header = Factory.New<AsycudaManifestHeader>();
		header.AMA_OA_Carrier = org.Addresses.AddNew().PK;

		NUnit.Framework.Assert.That(header.CarrierMPCI, Is.EqualTo("1234567").Using(CustomComparers.TypeComparison));
	}

	[ExpectNoExceptions]
	public void TestShippingAgentMPCI_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaManifestHeader>()
			.HasProperty(x => x.ShippingAgentMPCI)
			.WithCaption("MPCI ID"));

	[ExpectNoExceptions]
	public void TestShippingAgentMPCI()
	{
		var org = Factory.New<OrgHeader>();
		org.CustomsCodes.AddNew(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, "1234567", Core.Constants.CountryCodes.UnitedArabEmirates);
		var header = Factory.New<AsycudaManifestHeader>();
		header.AMA_OA_ShippingAgent = org.Addresses.AddNew().PK;

		NUnit.Framework.Assert.That(header.ShippingAgentMPCI, Is.EqualTo("1234567").Using(CustomComparers.TypeComparison));
	}

	[ExpectNoExceptions]
	public void TestDataGrouping()
	{
		var manifestHeader = Factory.New<AsycudaManifestHeader>();
		manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedArabEmirates;
		NUnit.Framework.Assert.That(manifestHeader.DataGrouping, Is.EqualTo(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UAEManifest).Using(CustomComparers.TypeComparison), "Data Grouping");
	}

	public void TestGetExtraMessageSendingNotification()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedArabEmirates;
			company1.GC_Code = "DAE";
			var proxy1 = Factory.New<OrgHeader>();
			proxy1.OH_Code = "Proxy1";
			proxy1.CustomsCodes.AddNew(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, "AAALFQA", Core.Constants.CountryCodes.UnitedArabEmirates);
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = company1.PK;
			branch1.GB_Code = "AUH";
			branch1.GB_BranchName = "AE - Branch 1";
			branch1.GB_OH_OrgProxy = proxy1.PK;
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_CustomsOffice = "office";
			var warning = "To create a message for the United Arab Emirates you must link the current logged in company to an AE Branch in the Registry, Customs>Country or Region Specific>United Arab Emirates>Default Branch for Manifest Submission. This is so that the correct OrgProxy is selected for determining the Manifest EDI Profile.";
			AssertEquals("ManifestMessage should not be allowed to send when DefaultBranchForManifestSubmission registry not set", warning, header.MessageSendingNotificationHelper.GetNotifications());

			using (AECustomsRegistry.Instance.DefaultBranchForManifestSubmission.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "AUH"))
			{
				AssertNotEquals("ManifestMessage should be allowed to send when DefaultBranchForManifestSubmission registry set a value", warning, header.MessageSendingNotificationHelper.GetNotifications());
			}
		}
	}

	[ExpectNoExceptions]
	public void TestSynchroniser()
	{
		var consol = Factory.New<ForwardingConsol>();
		var header = CreateBusinessObject(Factory);
		header.SetParent(consol);

		NUnit.Framework.Assert.That(header.Synchroniser, Is.TypeOf<AsycudaManifestHeaderSynchroniser>());
	}

	[ExpectNoExceptions]
	public void TestCountryCode()
	{
		var manifestHeader = Factory.New<AsycudaManifestHeader>();
		AssertEquals(manifestHeader.CountryCode, Core.Constants.CountryCodes.UnitedArabEmirates);
	}

	protected override BusinessObject GetNewBusinessObject() => CreateBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateBusinessObject(factory);

	protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);

	AsycudaManifestHeader CreateBusinessObject(BusinessObjectFactory factory)
	{
		var manifestHeader = factory.New<AsycudaManifestHeader>();
		manifestHeader.AMA_JobReference = "C1234";
		return manifestHeader;
	}
}
