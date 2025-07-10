using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Common.CH;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(MessageSendingDeclaration))]
sealed class MessageSendingDeclarationTest : NonPersistentBusinessObjectTestCase
{
	public void TestJE_DeclarationLanguageInfo() => CombineAssertions(() =>
	{
		Declaration.JE_DeclarationLanguage = SwissCustomsLanguageList.Codes.Italian;
		AssertEquals("Initial value", SwissCustomsLanguageList.Codes.Italian, SendingDeclaration.JE_DeclarationLanguage);
		AssertSame("Same lookup as declaration", MetaData.GetListDataSource(Declaration, Declaration.JE_DeclarationLanguageInfo.PropertyDescriptor), MetaData.GetListDataSource(SendingDeclaration, SendingDeclaration.JE_DeclarationLanguageInfo.PropertyDescriptor));
		CaptionTestHelper.AssertCaptions(SendingObjectParent.SendingDeclaration.JE_DeclarationLanguageInfo, caption: "Language");
		AssertEquals("MaxLength", Declaration.JE_DeclarationLanguageInfo.MaxLength, SendingDeclaration.JE_DeclarationLanguageInfo.MaxLength);
	});

	public void TestJE_LocationOfGoods() => CombineAssertions(() =>
	{
		Declaration.JE_LocationOfGoods = "LOC123";
		AssertEquals("Initial value", "LOC123", SendingDeclaration.JE_LocationOfGoods);
		AssertSame("Same  Lookup as declaration", MetaData.GetListDataSource(Declaration, Declaration.JE_LocationOfGoodsInfo.PropertyDescriptor), MetaData.GetListDataSource(SendingDeclaration, SendingDeclaration.JE_LocationOfGoodsInfo.PropertyDescriptor));
		CaptionTestHelper.AssertCaptions(SendingDeclaration.JE_LocationOfGoodsInfo, caption: "Goods Location");
		AssertEquals("MaxLength", Declaration.JE_LocationOfGoodsInfo.MaxLength, SendingDeclaration.JE_LocationOfGoodsInfo.MaxLength);
	});

	public void TestJE_TransportMode() => CombineAssertions(() =>
	{
		Declaration.JE_TransportMode = TransportTypeGenericList.Codes.OwnPropulsion; AssertEquals("Initial value", TransportTypeGenericList.Codes.OwnPropulsion, SendingDeclaration.JE_TransportMode);
		AssertSame("Same  Lookup as declaration", MetaData.GetListDataSource(Declaration, Declaration.JE_TransportModeInfo.PropertyDescriptor), MetaData.GetListDataSource(SendingDeclaration, SendingDeclaration.JE_TransportModeInfo.PropertyDescriptor));
		CaptionTestHelper.AssertCaptions(SendingDeclaration.JE_TransportModeInfo, caption: "Transport Mode");
		AssertEquals("MaxLength", Declaration.JE_TransportModeInfo.MaxLength, SendingDeclaration.JE_TransportModeInfo.MaxLength);
	});

	public void TestJE_MasterBill() => CombineAssertions(() =>
	{
		Declaration.JE_MasterBill = "MASTER-BILL";
		AssertEquals("Initial value", "MASTER-BILL", SendingDeclaration.JE_MasterBill);
		CaptionTestHelper.AssertCaptions(SendingDeclaration.JE_MasterBillInfo, caption: "Master Bill");
		AssertEquals("MaxLength", Declaration.JE_MasterBillInfo.MaxLength, SendingDeclaration.JE_MasterBillInfo.MaxLength);
	});

	public void TestJE_VoyageFlightNo() => CombineAssertions(() =>
	{
		Declaration.JE_VoyageFlightNo = "FLIGHT-NO";
		AssertEquals("Initial value", "FLIGHT-NO", SendingDeclaration.JE_VoyageFlightNo);
		CaptionTestHelper.AssertCaptions(SendingDeclaration.JE_VoyageFlightNoInfo, caption: "Voyage");
		AssertEquals("MaxLength", Declaration.JE_VoyageFlightNoInfo.MaxLength, SendingDeclaration.JE_VoyageFlightNoInfo.MaxLength);
	});

	public void TestJE_RN_NKTransportNationality() => CombineAssertions(() =>
	{
		Declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Austria;
		AssertEquals("Initial value", Core.Constants.CountryCodes.Austria, SendingDeclaration.JE_RN_NKTransportNationality);
		AssertType<RefCountryCollection>("Pre-condition", MetaData.GetListDataSource(Declaration, Declaration.JE_RN_NKTransportNationalityInfo.PropertyDescriptor));
		AssertType<RefCountryCollection>("Same collection as declaration", MetaData.GetListDataSource(SendingDeclaration, SendingDeclaration.JE_RN_NKTransportNationalityInfo.PropertyDescriptor));
		CaptionTestHelper.AssertCaptions(SendingDeclaration.JE_RN_NKTransportNationalityInfo, caption: "Nationality");
		AssertEquals("MaxLength", Declaration.JE_RN_NKTransportNationalityInfo.MaxLength, SendingDeclaration.JE_RN_NKTransportNationalityInfo.MaxLength);
	});

	public void TestJE_TransportMeans() => CombineAssertions(() =>
	{
		Declaration.JE_TransportMeans = "TM";
		AssertEquals("Initial value", "TM", SendingDeclaration.JE_TransportMeans);
		CaptionTestHelper.AssertCaptions(SendingDeclaration.JE_TransportMeansInfo, caption: "Type of ID");
		AssertEquals("MaxLength", Declaration.JE_TransportMeansInfo.MaxLength, SendingDeclaration.JE_TransportMeansInfo.MaxLength);
	});

	public void TestJE_VesselName() => CombineAssertions(() =>
	{
		Declaration.JE_VesselName = "VESSEL-NAME";
		AssertEquals("Initial value", "VESSEL-NAME", SendingDeclaration.JE_VesselName);
		CaptionTestHelper.AssertCaptions(SendingDeclaration.JE_VesselNameInfo, caption: "Transport ID");
		AssertEquals("MaxLength", Declaration.JE_VesselNameInfo.MaxLength, SendingDeclaration.JE_VesselNameInfo.MaxLength);
	});

	public void TestTransportModeIsProperties() => CombineAssertions(() =>
	{
		AssertProperties(TransportModes.Air, expectedIsAir: true);
		AssertProperties(TransportModes.OwnPropulsion, expectedIsOwnPropulsion: true);
		AssertProperties(TransportModes.Rail);
		AssertProperties(TransportModes.Road);
		AssertProperties(TransportModes.FixedTransportInstallations);
		AssertProperties(TransportModes.InlandWaterwayTransport);

		void AssertProperties(string transportMode, bool expectedIsAir = false, bool expectedIsOwnPropulsion = false)
		{
			SendingObjectParent.SendingDeclaration.JE_TransportMode = transportMode;
			AssertEquals($"TransportMode={transportMode} IsAir", expectedIsAir, SendingDeclaration.IsAir);
			AssertEquals($"TransportMode={transportMode} IsOwnPropulsion", expectedIsOwnPropulsion, SendingDeclaration.IsOwnPropulsion);
		}
	});

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var sendingObjectParent = new ExportDeclarationMessageSendingObjectParent(declaration);
		return sendingObjectParent.SendingDeclaration;
	}

	JobDeclaration GetNewJobDeclaration(BusinessObjectFactory factory)
	{
		var jobDeclaration = factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		return jobDeclaration;
	}

	JobDeclaration Declaration => declaration ??= GetNewJobDeclaration(Factory);
	JobDeclaration declaration;

	ExportDeclarationMessageSendingObjectParent SendingObjectParent => sendingObjectParent ??= new ExportDeclarationMessageSendingObjectParent(Declaration);
	ExportDeclarationMessageSendingObjectParent sendingObjectParent;

	MessageSendingDeclaration SendingDeclaration => SendingObjectParent.SendingDeclaration;
}
