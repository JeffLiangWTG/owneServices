using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CLREGMessageBuilderTest : TestCaseWithFactory
	{
		public void TestIsIndividual()
		{
			var organisation = Factory.New<OrgHeader>();
			var messages = new EDIMessageCollection(organisation);
			var dataProvider = GetMessageData(organisation);

			var text = BuildMessageText(dataProvider, messages);
			var messageNumber = ExtractDocumentMessageNumber(text, "UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'BGM+101:::CLREG+", ":1+9'RFF+AQU:IND'FTX+AFM+++MRS:TEST:TEST1:USER:SF'FTX+CNP+++TEST USER:CP'GIS+EVI::95'GIS+EXD::95'CNI+1'RFF+1'LOC+DN+123456'LOC+ISS+SGP'RFF+1'LOC+DN+8546'LOC+ISS+MDA'GID+1'FTX+ATY+BA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+ATY+PA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+CAT+BA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+CAT+PA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+CAT+BP+02+123456789456123:CONTACT PH COMMENT'FTX+CAT+FA+123+456789123:CONTACT FAX COMMENT'FTX+CAT+AP+654+987654321:CONTACT AH COMMENT'FTX+CAT+MO++04122526321:CONTACT MOBILE COMMENT'FTX+CAT+EA++TEST@TEST.COM'MEA+RN+:::EXPORTER'MEA+RN+:::IMPORTER'MEA+RN+:::SEA_CG_RPT'AUT+GE+M'DTM+329:20110101:102'UNT+30+<<MSGNO PLACEHOLDER>>'");

			AssertNotEquals("Message Number is not organisationPK", organisation.PK.ToString().Replace("-", "").ToUpper(), messageNumber);

			var genAddOnColumns = Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_Data, messageNumber));
			AssertEquals("One row in GenAddOnColumn", 1, genAddOnColumns.Length);
			AssertEquals("Organisation PK saved in GenAddOnColumn", organisation.PK, genAddOnColumns[0].XA_ParentID);
		}

		public void TestIsOrganisation()
		{
			var organisation = Factory.New<OrgHeader>();
			var messages = new EDIMessageCollection(organisation);
			var dataProvider = GetMessageData(organisation);
			dataProvider.ZA_IsIndiv = false;
			dataProvider.ZA_IsOrg = true;

			var text = BuildMessageText(dataProvider, messages);
			var messageNumber = ExtractDocumentMessageNumber(text, "UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'BGM+101:::CLREG+", ":1+9'RFF+AQU:ORG'FTX+CNP+++TEST USER:CP'TDT+1'LOC+ZZZ+:::TEST ORGANISATION'GIS+EVI::95'GIS+EXD::95'CNI+1'RFF+1'GID+1'FTX+ATY+BA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+ATY+PA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+CAT+BA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+CAT+PA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+CAT+BP+02+123456789456123:CONTACT PH COMMENT'FTX+CAT+FA+123+456789123:CONTACT FAX COMMENT'FTX+CAT+AP+654+987654321:CONTACT AH COMMENT'FTX+CAT+MO++04122526321:CONTACT MOBILE COMMENT'FTX+CAT+EA++TEST@TEST.COM'MEA+RN+:::EXPORTER'MEA+RN+:::IMPORTER'MEA+RN+:::SEA_CG_RPT'UNT+24+<<MSGNO PLACEHOLDER>>'");

			AssertNotEquals("Message Number is not organisationPK", organisation.PK.ToString().Replace("-", "").ToUpper(), messageNumber);
		}

		public void TestIsABN()
		{
			var organisation = Factory.New<OrgHeader>();
			var messages = new EDIMessageCollection(organisation);
			var dataProvider = GetMessageData(organisation);
			dataProvider.ZA_Bsn1 = ZString.Empty;
			dataProvider.ZA_Bsn2 = ZString.Empty;
			dataProvider.ZA_BsnCity = ZString.Empty;
			dataProvider.ZA_BsnPostCode = ZString.Empty;
			dataProvider.ZA_BsnPort = ZString.Empty;
			dataProvider.ZA_BsnState = ZString.Empty;
			dataProvider.ZA_IsIndiv = false;
			dataProvider.ZA_ABN = "ASDFG123HJK";
			dataProvider.ZA_CAC = "ERT";
			dataProvider.ZA_CACType = "BA";

			var text = BuildMessageText(dataProvider, messages);
			var messageNumber = ExtractDocumentMessageNumber(text, "UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'BGM+101:::CLREG+", ":1+9'RFF+ABN:ASDFG123HJK:ERT:BA'FTX+CNP+++TEST USER:CP'GIS+EVI::95'GIS+EXD::95'CNI+1'RFF+1'GID+1'FTX+ATY+PA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+CAT+BA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+CAT+PA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+CAT+BP+02+123456789456123:CONTACT PH COMMENT'FTX+CAT+FA+123+456789123:CONTACT FAX COMMENT'FTX+CAT+AP+654+987654321:CONTACT AH COMMENT'FTX+CAT+MO++04122526321:CONTACT MOBILE COMMENT'FTX+CAT+EA++TEST@TEST.COM'MEA+RN+:::EXPORTER'MEA+RN+:::IMPORTER'MEA+RN+:::SEA_CG_RPT'AUT+GE+M'DTM+329:20110101:102'UNT+23+<<MSGNO PLACEHOLDER>>'");

			AssertNotEquals("Message Number is not organisationPK", organisation.PK.ToString().Replace("-", "").ToUpper(), messageNumber);
		}

		public void TestMessageNumberIsNotRepeatedAndOrganizationCanBeFound()
		{
			var organization = Factory.New<OrgHeader>();

			var prefix = "UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'BGM+101:::CLREG+";
			var suffix = "+9'RFF+ABN:ASDFG123HJK:ERT:BA'FTX+CNP+++TEST USER:CP'GIS+EVI::95'GIS+EXD::95'CNI+1'RFF+1'GID+1'FTX+ATY+PA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+CAT+BA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+CAT+PA++ADDRESS1:BUSINESS ADDRESS 2:BUSINESS CITY:2018:NSW+AU'FTX+CAT+BP+02+123456789456123:CONTACT PH COMMENT'FTX+CAT+FA+123+456789123:CONTACT FAX COMMENT'FTX+CAT+AP+654+987654321:CONTACT AH COMMENT'FTX+CAT+MO++04122526321:CONTACT MOBILE COMMENT'FTX+CAT+EA++TEST@TEST.COM'MEA+RN+:::EXPORTER'MEA+RN+:::IMPORTER'MEA+RN+:::SEA_CG_RPT'AUT+GE+M'DTM+329:20110101:102'UNT+23+<<MSGNO PLACEHOLDER>>'";

			var messageNumbers = new System.Collections.Generic.HashSet<string>();

			var messages = new EDIMessageCollection(organization);
			var dataProvider = GetMessageData(organization);
			dataProvider.ZA_Bsn1 = ZString.Empty;
			dataProvider.ZA_Bsn2 = ZString.Empty;
			dataProvider.ZA_BsnCity = ZString.Empty;
			dataProvider.ZA_BsnPostCode = ZString.Empty;
			dataProvider.ZA_BsnPort = ZString.Empty;
			dataProvider.ZA_BsnState = ZString.Empty;
			dataProvider.ZA_IsIndiv = false;
			dataProvider.ZA_ABN = "ASDFG123HJK";
			dataProvider.ZA_CAC = "ERT";
			dataProvider.ZA_CACType = "BA";

			for (var i = 1; i < 4; ++i)
			{
				var text = BuildMessageText(dataProvider, messages);
				var messageNumber = ExtractDocumentMessageNumber(text, prefix, ":" + i + suffix);

				Assert("GUID should not repeated", messageNumbers.Add(messageNumber));

				var message = GenerateDummyResponseMessage(messageNumber);
				AssertEquals($"Organization should be found: {i}", organization, message.GetWrappedObject());
			}

			CMRCLREGRMessage GenerateDummyResponseMessage(string docMessageNumber)
			{
				var message = Factory.New<CMRCLREGRMessage>();
				message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::CLREGR+2F8D 6599 866A:001+11'
FTX+CCI++AAA3366766M'
NAD+MR+AAA374M::95'
RFF+ACW:CLREG'
RFF+AFM:9'
RFF+ABO:DOCMSGNUM::001'
DTM+310:20101221232955:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5202:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITH ERRORS AND/OR WARNINGS'
ERP+1'
ERC+ADVICE:80:95'
ERC+CL0378:6:95'
FTX+AAO+++CCID =AAA3366766M CREATED SUCCESSFULLY'
CNT+55:000'
UNT+18+000001'".Replace("\r\n", "").Replace("DOCMSGNUM", docMessageNumber);
				return message;
			}
		}

		[ExpectNoExceptions()]
		public void TestCLREGMessageBuilderWithNulls()
		{
			var organisation = Factory.New<OrgHeader>();
			var messages = new EDIMessageCollection(Factory.New<OrgHeader>());
			var queryInfo = new OrgHeaderWrapper(organisation).CLREGInfoProvider;
			queryInfo.ZA_ABN = "";
			queryInfo.ZA_BusinessName = "";
			queryInfo.ZA_CAC = "";
			queryInfo.ZA_CACType = "";
			queryInfo.ZA_DateofBirth = ZDate.Empty;
			queryInfo.ZA_FamilyName = "";
			queryInfo.ZA_FirstName = "";
			queryInfo.ZA_Gender = "";
			queryInfo.ZA_IsEvidenceOfID = false;
			queryInfo.ZA_IsExDocsUser = false;
			queryInfo.ZA_IsIndiv = false;
			queryInfo.ZA_SecondName = "";
			queryInfo.ZA_Suffix = "";
			queryInfo.ZA_Title = "";

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CODE";
			var mainAddress = header.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			queryInfo.ZA_Bsn1 = mainAddress.OA_Address1;
			queryInfo.ZA_Bsn2 = mainAddress.OA_Address2;
			queryInfo.ZA_BsnCity = mainAddress.OA_City;
			queryInfo.ZA_BsnPostCode = mainAddress.OA_PostCode;
			queryInfo.ZA_BsnPort = mainAddress.OA_RL_NKRelatedPortCode;
			queryInfo.ZA_BsnState = mainAddress.OA_State;

			var contactDataProvider = queryInfo.CLREGContactInfoProvider;
			contactDataProvider.ZA_ContAH = "";
			contactDataProvider.ZA_ContAHComment = "";
			contactDataProvider.ZA_ContAHPref = "";
			contactDataProvider.ZA_ContEmail = "";
			contactDataProvider.ZA_ContFax = "";
			contactDataProvider.ZA_ContFaxComment = "";
			contactDataProvider.ZA_ContFaxPref = "";
			contactDataProvider.ZA_ContMob = "";
			contactDataProvider.ZA_ContMobComment = "";
			contactDataProvider.ZA_ContName = "";
			contactDataProvider.ZA_ContPh = "";
			contactDataProvider.ZA_ContPhComment = "";
			contactDataProvider.ZA_ContPhPref = "";
			contactDataProvider.ZA_ContPurpose = "";

			var builder = new CLREGMessageBuilder(queryInfo, Factory);
			AssertEquals("Sub Type should be 'Create'", Common.MessageBuilders.MessageSubTypes.Create, builder.MessageSubType);
			builder.Messages = messages;
			builder.PopulateMessages();
		}

		ZString BuildMessageText(CLREGInfoProvider dataProvider, EDIMessageCollection messages)
		{
			dataProvider.RunPreSaveValidation();
			AssertEquals("Errors", "", dataProvider.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString());
			AssertEquals("Other notifications", 0, dataProvider.NotificationsIncludingChildren.GetUniqueMessageList().Length);

			ISupplyCLREGInfo queryInfo = dataProvider;

			var builder = new CLREGMessageBuilder(queryInfo, Factory);
			AssertEquals("Sub Type should be 'Create'", Common.MessageBuilders.MessageSubTypes.Create, builder.MessageSubType);

			builder.Messages = messages;
			builder.PopulateMessages();
			return builder.MessageText;
		}

		ZString ExtractDocumentMessageNumber(string messageText, string expectedPrefix, string expectedSuffix)
		{
			Assert("Message Length", messageText.Length > expectedPrefix.Length + 32);
			AssertEquals("Message Prefix", expectedPrefix, messageText.Substring(0, expectedPrefix.Length));
			AssertEquals("Message Suffix", expectedSuffix, messageText.Substring(expectedPrefix.Length + 32));

			var messageNumber = messageText.Substring(expectedPrefix.Length, 32);
			Assert("Valid GUID", Regex.IsMatch(messageNumber, "^[A-F0-9]{32}$"));

			return messageNumber;
		}

		CLREGInfoProvider GetMessageData(OrgHeader organisation)
		{
			var messageData = new OrgHeaderWrapper(organisation).CLREGInfoProvider;
			messageData.ZA_IsIndiv = true;
			messageData.ZA_Title = "Mrs";
			messageData.ZA_FirstName = "TEST";
			messageData.ZA_SecondName = "TEST1";
			messageData.ZA_FamilyName = "USER";
			messageData.ZA_Suffix = "SF";
			messageData.ZA_Gender = CMRGenderCodes.Codes.Male;
			messageData.ZA_DateofBirth = new ZDateTime(2011, 01, 01);

			messageData.ZA_BusinessName = "Test Organisation";

			messageData.ZA_IsEvidenceOfID = true;
			messageData.ZA_IsExDocsUser = true;

			var travelDocument = messageData.TravelDocuments.AddNew();
			travelDocument.ZA_Country = CMRICAOCountryCodes.Codes.SINGAPORE;
			travelDocument.ZA_DocumentNo = "123456";

			travelDocument = messageData.TravelDocuments.AddNew();
			travelDocument.ZA_Country = CMRICAOCountryCodes.Codes.MOLDOVAREPUBLICOF;
			travelDocument.ZA_DocumentNo = "8546";

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CODE";
			var mainAddress = header.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var deliveryAddress = header.Addresses.AddNew();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			deliveryAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery);
			deliveryAddress.OA_Address1 = "Address1";
			deliveryAddress.OA_Address2 = "Business Address 2";
			deliveryAddress.OA_City = "Business City";
			deliveryAddress.OA_PostCode = "2018";
			deliveryAddress.OA_State = "NSW";
			deliveryAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			messageData.ZA_Bsn1 = deliveryAddress.OA_Address1;
			messageData.ZA_Bsn2 = deliveryAddress.OA_Address2;
			messageData.ZA_BsnCity = deliveryAddress.OA_City;
			messageData.ZA_BsnPostCode = deliveryAddress.OA_PostCode;
			messageData.ZA_BsnPort = deliveryAddress.OA_RL_NKRelatedPortCode;
			messageData.ZA_BsnState = deliveryAddress.OA_State;

			var contactDataProvider = messageData.CLREGContactInfoProvider;
			contactDataProvider.ZA_ContName = "Test User";
			contactDataProvider.ZA_ContPurpose = "CP";
			contactDataProvider.ZA_Cont1 = deliveryAddress.OA_Address1;
			contactDataProvider.ZA_Cont2 = deliveryAddress.OA_Address2;
			contactDataProvider.ZA_ContCity = deliveryAddress.OA_City;
			contactDataProvider.ZA_ContPostCode = deliveryAddress.OA_PostCode;
			contactDataProvider.ZA_ContPort = deliveryAddress.OA_RL_NKRelatedPortCode;
			contactDataProvider.ZA_ContState = deliveryAddress.OA_State;

			contactDataProvider.ZA_ContPost1 = deliveryAddress.OA_Address1;
			contactDataProvider.ZA_ContPost2 = deliveryAddress.OA_Address2;
			contactDataProvider.ZA_ContPostCity = deliveryAddress.OA_City;
			contactDataProvider.ZA_ContPostPostCode = deliveryAddress.OA_PostCode;
			contactDataProvider.ZA_ContPostPort = deliveryAddress.OA_RL_NKRelatedPortCode;
			contactDataProvider.ZA_ContPostState = deliveryAddress.OA_State;

			messageData.ZA_Post1 = deliveryAddress.OA_Address1;
			messageData.ZA_Post2 = deliveryAddress.OA_Address2;
			messageData.ZA_PostCity = deliveryAddress.OA_City;
			messageData.ZA_PostPostCode = deliveryAddress.OA_PostCode;
			messageData.ZA_PostPort = deliveryAddress.OA_RL_NKRelatedPortCode;
			messageData.ZA_PostState = deliveryAddress.OA_State;

			var roll = Factory.New<Roll>();
			roll.ZA_Roll = CMRClientRolls.Codes.Exporter;
			messageData.Rolls.Add(roll);

			roll = Factory.New<Roll>();
			roll.ZA_Roll = CMRClientRolls.Codes.Importer;
			messageData.Rolls.Add(roll);

			roll = Factory.New<Roll>();
			roll.ZA_Roll = CMRClientRolls.Codes.SeaCargoReporter;
			messageData.Rolls.Add(roll);

			contactDataProvider.ZA_ContPh = "123456789456123";
			contactDataProvider.ZA_ContPhPref = "02";
			contactDataProvider.ZA_ContPhComment = "Contact Ph Comment";

			contactDataProvider.ZA_ContFax = "456789123";
			contactDataProvider.ZA_ContFaxPref = "123";
			contactDataProvider.ZA_ContFaxComment = "Contact Fax Comment";

			contactDataProvider.ZA_ContAH = "987654321";
			contactDataProvider.ZA_ContAHPref = "654";
			contactDataProvider.ZA_ContAHComment = "Contact AH Comment";

			contactDataProvider.ZA_ContMob = "04122526321";
			contactDataProvider.ZA_ContMobComment = "Contact Mobile Comment";
			contactDataProvider.ZA_ContEmail = "test@test.com";
			return messageData;
		}
	}
}
