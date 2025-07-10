using System;
using CargoWise.Customs.IN.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.AirCgm.Testing;

[TestedType(typeof(AirCgmCMCHI01DataProvider))]
sealed class HeaderDataProviderTest : AirCgmIHeaderDataProviderBase
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
		AssertEquals("CMCHI01", CreateDataProvider().MessageId);
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
			AssertEquals("AMA_CustomsOffice empty", ZString.Empty, dataProvider.ReceiverId);

			header.AMA_CustomsOffice = "ABC";
			AssertEquals("AMA_CustomsOffice ABC", "ABC", dataProvider.ReceiverId);
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

	protected override IHeaderDataProvider CreateDataProvider()
		=> AirCgmCMCHI01DataProvider.CreateProvider(header, Mock.Of<IAirCgmCMCHI01AdditionalDataProvider>()).Header;
}
