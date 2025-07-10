using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.V921ES.Elements;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	class CommonExportMessageTextBuilderTest : TestCaseWithFactory
	{
		public void TestAddNewNADWithEmailInSG6Group_NoAddressDetails()
		{
			var group6 = new SegmentGroup6MessageSection(1);
			CommonExportMessageTextBuilder.AddNewNADWithEmailInSG6Group(group6, null, null);
			AssertEquals("", group6.ToString(characterSet));
		}

		public void TestAddNewNADWithEmailInSG6Group_NoAgencyCodedList()
		{
			var declarant = BuilderHelperTest.SetUpExportDeclarantPartyId("O", "MIDIRECCION@MIXMAIL.COM");
			var group6 = new SegmentGroup6MessageSection(1);
			CommonExportMessageTextBuilder.AddNewNADWithEmailInSG6Group(group6, null, declarant);

			AssertEquals("NAD+2+1210244B+MIDIRECCION@MIXMAIL.COM+GUTIERREZ S.A.:::::O'", group6.ToString(characterSet));
		}

		public void TestAddNewNADWithEmailInSG6Group_EmailAddressSplit()
		{
			var declarant = BuilderHelperTest.SetUpExportDeclarantPartyId("O", "MIDIRECCION.CORREO.EN.CASTILLAYLEON@MIXMAIL.COM");
			var group6 = new SegmentGroup6MessageSection(1);
			CommonExportMessageTextBuilder.AddNewNADWithEmailInSG6Group(group6, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, declarant);

			AssertEquals("NAD+2+1210244B::148+MIDIRECCION.CORREO.EN.CASTILLAYLEON:@MIXMAIL.COM+GUTIERREZ S.A.:::::O'", group6.ToString(characterSet));
		}

		public void TestAddNewNADWithEmailInSG6Group_NoNameCode()
		{
			var declarant = BuilderHelperTest.SetUpExportDeclarantPartyId(ZString.Empty, "MIDIRECCION@MIXMAIL.COM");
			var group6 = new SegmentGroup6MessageSection(1);
			CommonExportMessageTextBuilder.AddNewNADWithEmailInSG6Group(group6, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, declarant);

			AssertEquals("NAD+2+1210244B::148+MIDIRECCION@MIXMAIL.COM+GUTIERREZ S.A.'", group6.ToString(characterSet));
		}

		public void TestAddNewSG7Group_EmptyTermsOfDeliveryCode()
		{
			var group7 = new SegmentGroup7MessageSection(1);
			CommonExportMessageTextBuilder.AddNewSG7Group(group7, ZString.Empty, "BARCELONA", "3");
			AssertEquals("", group7.ToString(characterSet));
		}

		public void TestAddNewSG7Group_EmptyDeliveryLocation()
		{
			var group7 = new SegmentGroup7MessageSection(1);
			CommonExportMessageTextBuilder.AddNewSG7Group(group7, "CIF", ZString.Empty, "3");
			AssertEquals("TOD+++CIF:106'LOC'LOC+133+3::141'", group7.ToString(characterSet));
		}

		public void TestAddNewSG7Group_EmptyLocationId()
		{
			var group7 = new SegmentGroup7MessageSection(1);
			CommonExportMessageTextBuilder.AddNewSG7Group(group7, "CIF", "BARCELONA", ZString.Empty);
			AssertEquals("TOD+++CIF:106'LOC+7+:::BARCELONA'", group7.ToString(characterSet));
		}

		public void TestAddNewSG37NoDateOfIssue()
		{
			var group37 = new SegmentGroup37MessageSection(1);
			CommonExportMessageTextBuilder.AddNewSG37(group37, "X001", "ES3600000002", ZString.Empty, new ZDateTime(2020, 7, 16), ZDateTime.Empty);
			AssertEquals("DOC+:::X001+ES3600000002'DTM+137:200716:101'", group37.ToString(characterSet));
		}

		public void TestAddNewSG37NoDateOfExpiry()
		{
			var group37 = new SegmentGroup37MessageSection(1);
			CommonExportMessageTextBuilder.AddNewSG37(group37, "X001", "ES3600000002", ZString.Empty, ZDateTime.Empty, new ZDateTime(2020, 7, 16));
			AssertEquals("DOC+:::X001+ES3600000002'DTM+36:200716:101'", group37.ToString(characterSet));
		}

		protected override void SetUp()
		{
			base.SetUp();
			characterSet = new UNOAESCharacterSet();
		}
		UNOAESCharacterSet characterSet;
	}
}
