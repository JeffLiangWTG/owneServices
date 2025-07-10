using System;
using CargoWise.Customs.IN.MessageContracts.ExportSbGoodsRegistration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business.MessageSending.ExportSb;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSbGoodsRegistration.Testing;

[TestedType(typeof(ExportSbGoodsRegistrationCACHE05DataProvider))]
sealed class ExportSbGoodsRegistrationHeaderDataProviderTest : ExportSbGoodsRegistrationHeaderDataProviderAbstractClassBase
{
	[TestDate(2024, 6, 13, 8, 8, 3)]
	public override void TestDate()
	{
		AssertEquals(new DateTime(2024, 6, 13), CreateDataProvider().Date);
	}

	public override void TestHrec()
	{
		AssertEquals("HREC", CreateDataProvider().Hrec);
	}

	public override void TestMessageId()
	{
		AssertEquals("CACHE05", CreateDataProvider().MessageId);
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

	public override void TestVersionNo()
	{
		AssertEquals("ICES1_5", CreateDataProvider().VersionNo);
	}

	public override void TestZz1()
	{
		AssertEquals("ZZ", CreateDataProvider().Zz1);
	}

	public override void TestZz2()
	{
		AssertEquals("ZZ", CreateDataProvider().Zz2);
	}

	protected override HeaderDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbGoodsRegistrationCACHE05DataProvider.CreateProvider(messageSendingObject).Header;
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
