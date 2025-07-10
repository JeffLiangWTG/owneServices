using System;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class HeaderDataProviderTest : ExportSbHeaderDataProviderAbstractClassBase
{
	[TestDate(2024, 6, 13, 8, 8, 3)]
	public override void TestDate()
	{
		AssertEquals(new DateTime(2024, 6, 13), CreateDataProvider().Date);
	}

	public override void TestPlaceHolder()
	{
		AssertNullOrEmpty(CreateDataProvider().PlaceHolder);
	}

	public override void TestReceiverId()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals("JE_CustomsOffice empty", ZString.Empty, dataProvider.ReceiverId);

			declaration.JE_CustomsOffice = "ABC";
			AssertEquals("JE_CustomsOffice ABC", "ABC", dataProvider.ReceiverId);
		});
	}

	public override void TestSenderid()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertNullOrEmpty("LoginPassword not setup", dataProvider.Senderid);

			var loginPassword = GlbStaffWrapper.GetWrapperForCurrentUser().LoginPassword;
			AssertEquals("LoginPassword set, GP_UserID not set", ZString.Empty, dataProvider.Senderid);

			loginPassword.GP_UserID = "IcegateUser";
			AssertEquals("LoginPassword and GP_UserID set", "IcegateUser", dataProvider.Senderid);
		});
	}

	public override void TestSequenceOrControlNo()
	{
		AssertEquals(Constants.Messaging.INMessageNumPlaceHolder, CreateDataProvider().SequenceOrControlNo);
	}

	[TestDate(2024, 6, 13, 8, 8, 3)]
	public override void TestTime()
	{
		AssertEquals(new DateTime(2024, 6, 13, 8, 8, 3), CreateDataProvider().Time);
	}

	public override void TestTOrP()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			AssertEquals("For Production", true, dataProvider.TOrP);

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);
			AssertEquals("For Test", false, dataProvider.TOrP);

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Training);
			AssertEquals("For Training", false, dataProvider.TOrP);

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Demo);
			AssertEquals("For Demo", false, dataProvider.TOrP);
		});
	}

	protected override HeaderDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbCACHE01DataProvider.CreateProvider(header, new ExportSbCACHE01AdditionalDataProvider(messageSendingObject)).Header;
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.CustomsEntryHeaders.Add(header);
	}

	JobDeclaration declaration;
}
