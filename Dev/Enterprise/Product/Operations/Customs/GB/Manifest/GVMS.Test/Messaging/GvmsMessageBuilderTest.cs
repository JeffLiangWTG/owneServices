using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	class GvmsMessageBuilderTest : TestCaseWithFactory
	{
		public void TestSerializeATAExample()
		{
			var gvmsMessageDataObject = new GVMSMessageDataObject();
			var ataDeclaration = new Atadeclaration { ataCarnetId = "SDOEFOK2982893", sAndSMasterRefNum = "20GB01I0XLM976S003" };
			var plannedCrossing = new Plannedcrossing { routeId = "61", localDateTimeOfDeparture = "2021-08-11T10:58" };
			var containerReferenceNums = new List<string> { "Cont 1", "Cont 2" };
			gvmsMessageDataObject.gmrStatusVersion = 0;
			gvmsMessageDataObject.ataDeclarations = new List<Atadeclaration> { ataDeclaration };
			gvmsMessageDataObject.plannedCrossing = plannedCrossing;
			gvmsMessageDataObject.containerReferenceNums = containerReferenceNums;
			gvmsMessageDataObject.direction = "UK_OUTBOUND";
			gvmsMessageDataObject.isUnaccompanied = true;
			gvmsMessageDataObject.containerReferenceNums = new List<string> { "CSQU3054383" };

			var result = GVMSEDIMessage.Serialize(gvmsMessageDataObject);

			AssertEquals(ATAJsonExample, result);
		}

		public void TestEIDRExample()
		{
			var gvmsMessageDataObject = new GVMSMessageDataObject();
			var eidrDeclaration = new Eidrdeclaration { traderEORI = "GB123456789012", sAndSMasterRefNum = "20GB01I0XLM976S003" };
			var plannedCrossing = new Plannedcrossing { routeId = "1", localDateTimeOfDeparture = "2021-08-11T10:58" };
			gvmsMessageDataObject.eidrDeclarations = new List<Eidrdeclaration> { eidrDeclaration };
			gvmsMessageDataObject.plannedCrossing = plannedCrossing;
			gvmsMessageDataObject.direction = "GB_TO_NI";
			gvmsMessageDataObject.isUnaccompanied = false;
			gvmsMessageDataObject.vehicleRegNum = "AB19DEF";
			gvmsMessageDataObject.trailerRegistrationNums = new List<string> { "KF293716", "GH28372S" };

			var result = GVMSEDIMessage.Serialize(gvmsMessageDataObject);

			AssertEquals(EIDRJsonExample, result);
		}

		public void TestEmptyExample()
		{
			var gvmsMessageDataObject = new GVMSMessageDataObject();
			var emptyVehicle = new Emptyvehicle { isOwnVehicle = true, sAndSMasterRefNum = "20GB01I0XLM976S003" };
			var plannedCrossing = new Plannedcrossing { routeId = "61", localDateTimeOfDeparture = "2021-08-11T10:58" };
			gvmsMessageDataObject.emptyVehicle = emptyVehicle;
			gvmsMessageDataObject.plannedCrossing = plannedCrossing;
			gvmsMessageDataObject.direction = "UK_OUTBOUND";
			gvmsMessageDataObject.isUnaccompanied = false;
			gvmsMessageDataObject.vehicleRegNum = "AB19DEF";

			var result = GVMSEDIMessage.Serialize(gvmsMessageDataObject);

			AssertEquals(EmptyJsonExample, result);
		}

		public void TestStandardExample()
		{
			var gvmsMessageDataObject = new GVMSMessageDataObject();
			var customsDeclaration1 = new Customsdeclaration() { customsDeclarationId = "0GB689223596000-SE119404", sAndSMasterRefNum = "20GB01I0XLM976S001" };
			var customsDeclaration2 = new Customsdeclaration() { customsDeclarationId = "0GB689223596000-SE119405", sAndSMasterRefNum = "20GB01I0XLM976S002" };
			var customsDeclaration3 = new Customsdeclaration() { customsDeclarationId = "0GB689223596000-SE119406", sAndSMasterRefNum = "20GB01I0XLM976S003" };
			var customsDeclaration4 = new Customsdeclaration() { customsDeclarationId = "0GB689223596000-SE119406", sAndSMasterRefNum = "20GB01I0XLM976S003", customsDeclarationPartId = "34J" };
			var transitDeclaration1 = new Transitdeclaration() { transitDeclarationId = "10GB00002910B75BE5", isTSAD = true };
			var transitDeclaration2 = new Transitdeclaration() { transitDeclarationId = "10GB00002910B75BE6", sAndSMasterRefNum = "20GB01I0XLM976S004", isTSAD = false };
			var plannedCrossing = new Plannedcrossing { routeId = "1", localDateTimeOfDeparture = "2021-08-11T10:58" };
			gvmsMessageDataObject.customsDeclarations = new List<Customsdeclaration> { customsDeclaration1, customsDeclaration2, customsDeclaration3, customsDeclaration4 };
			gvmsMessageDataObject.transitDeclarations = new List<Transitdeclaration> { transitDeclaration1, transitDeclaration2 };
			gvmsMessageDataObject.plannedCrossing = plannedCrossing;
			gvmsMessageDataObject.direction = "GB_TO_NI";
			gvmsMessageDataObject.isUnaccompanied = false;
			gvmsMessageDataObject.vehicleRegNum = "AB19DEF";

			var result = GVMSEDIMessage.Serialize(gvmsMessageDataObject);

			AssertEquals(StandardJsonExample, result);
		}

		public void TestTIRExample()
		{
			var gvmsMessageDataObject = new GVMSMessageDataObject();
			var tirDeclaration = new Tirdeclaration() { tirCarnetId = "FEOKEOK092927" };
			var plannedCrossing = new Plannedcrossing { routeId = "1", localDateTimeOfDeparture = "2021-08-11T10:58" };
			gvmsMessageDataObject.tirDeclarations = new List<Tirdeclaration> { tirDeclaration };
			gvmsMessageDataObject.plannedCrossing = plannedCrossing;
			gvmsMessageDataObject.direction = "GB_TO_NI";
			gvmsMessageDataObject.isUnaccompanied = true;
			gvmsMessageDataObject.trailerRegistrationNums = new List<string> { "KF293716", "GH28372S" };
			gvmsMessageDataObject.sAndSMasterRefNum = "20GB01I0XLM976S001";

			var result = GVMSEDIMessage.Serialize(gvmsMessageDataObject);

			AssertEquals(TIRJsonExample, result);
		}

		public void TestTransitExample()
		{
			var gvmsMessageDataObject = new GVMSMessageDataObject();
			var transitDeclaration1 = new Transitdeclaration() { transitDeclarationId = "10GB00002910B75BE5", isTSAD = true };
			var transitDeclaration2 = new Transitdeclaration() { transitDeclarationId = "10GB00002910B75BE6", sAndSMasterRefNum = "20GB01I0XLM976S004", isTSAD = false };
			var plannedCrossing = new Plannedcrossing { routeId = "61", localDateTimeOfDeparture = "2021-08-11T10:58" };
			gvmsMessageDataObject.transitDeclarations = new List<Transitdeclaration> { transitDeclaration1, transitDeclaration2 };
			gvmsMessageDataObject.plannedCrossing = plannedCrossing;
			gvmsMessageDataObject.direction = "UK_OUTBOUND";
			gvmsMessageDataObject.isUnaccompanied = false;
			gvmsMessageDataObject.vehicleRegNum = "AB19DEF";

			var result = GVMSEDIMessage.Serialize(gvmsMessageDataObject);

			AssertEquals(TransitJsonExample, result);
		}

		public void TestIndirectExportExample()
		{
			var gvmsMessageDataObject = new GVMSMessageDataObject();
			var indirectExportDeclaration1 = new IndirectExportdeclaration() { eadMasterRefNum = "20IE01I0XLM976S003" };
			var indirectExportDeclaration2 = new IndirectExportdeclaration() { eadMasterRefNum = "20IE01I0XLM976S004" };
			gvmsMessageDataObject.indirectExportDeclarations = new List<IndirectExportdeclaration> { indirectExportDeclaration1, indirectExportDeclaration2 };
			gvmsMessageDataObject.direction = "NI_TO_GB";
			gvmsMessageDataObject.vehicleRegNum = "AB19DEF";
			var plannedCrossing = new Plannedcrossing { routeId = "7", localDateTimeOfDeparture = "2021-08-11T10:58" };
			gvmsMessageDataObject.plannedCrossing = plannedCrossing;

			var result = GVMSEDIMessage.Serialize(gvmsMessageDataObject);

			AssertEquals(IndirectExportExample, result);
		}

		public void TestDbcExample()
		{
			var gvmsMessageDataObject = new GVMSMessageDataObject();
			DbcGoodsItem goodsItem1 = new DbcGoodsItem() { sAndSMasterRefNum = "GB12323020392" };
			DbcGoodsItem goodsItem2 = new DbcGoodsItem() { sAndSMasterRefNum = "GB12323020393" };
			var dbcDeclaration = new Dbcdeclaration() { isOwnVehicle = false, dbcGoods = new List<DbcGoodsItem>() { goodsItem1, goodsItem2 } };
			gvmsMessageDataObject.dbcDeclaration = dbcDeclaration;
			gvmsMessageDataObject.direction = "UK_OUTBOUND";
			gvmsMessageDataObject.isUnaccompanied = true;
			gvmsMessageDataObject.vehicleRegNum = "AB19DEF";
			gvmsMessageDataObject.trailerRegistrationNums = new List<string> { "KF293716", "GH28372S" };
			var plannedCrossing = new Plannedcrossing { routeId = "61", localDateTimeOfDeparture = "2021-08-11T10:58" };
			gvmsMessageDataObject.plannedCrossing = plannedCrossing;

			var result = GVMSEDIMessage.Serialize(gvmsMessageDataObject);

			AssertEquals(DbcExample, result);
		}

		public void TestMtpDeclaration()
		{
			var dataObject = new GVMSMessageDataObject();
			dataObject.mtpDeclaration = new()
			{
				mtpGoods =
				[
					new() { sAndSMasterRefNum = "1234" },
					new() { sAndSMasterRefNum = "5678" },
				]
			};

			var result = GVMSEDIMessage.Serialize(dataObject);
			AssertEquals("{\"mtpDeclaration\":{\"mtpGoods\":[{\"sAndSMasterRefNum\":\"1234\"},{\"sAndSMasterRefNum\":\"5678\"}]}}", result);

			dataObject.mtpDeclaration = null;
			result = GVMSEDIMessage.Serialize(dataObject);
			AssertEquals("{}", result);
		}

		public void TestUkCarrier()
		{
			var dataObject = new GVMSMessageDataObject();
			dataObject.ukcDeclaration = new() { fpoEORI = "GB12345678" };

			var result = GVMSEDIMessage.Serialize(dataObject);
			AssertEquals("{\"ukcDeclaration\":{\"fpoEORI\":\"GB12345678\"}}", result);

			dataObject.ukcDeclaration = null;
			result = GVMSEDIMessage.Serialize(dataObject);
			AssertEquals("{}", result);
		}

		public void TestHaulierType()
		{
			var dataObject = new GVMSMessageDataObject();
			dataObject.haulierType = "STANDARD";

			var result = GVMSEDIMessage.Serialize(dataObject);
			AssertEquals("{\"haulierType\":\"STANDARD\"}", result);

			dataObject.haulierType = null;
			result = GVMSEDIMessage.Serialize(dataObject);
			AssertEquals("{}", result);
		}

		string ATAJsonExample => @"{""isUnaccompanied"":true,""direction"":""UK_OUTBOUND"",""containerReferenceNums"":[""CSQU3054383""],""plannedCrossing"":{""routeId"":""61"",""localDateTimeOfDeparture"":""2021-08-11T10:58""},""ataDeclarations"":[{""ataCarnetId"":""SDOEFOK2982893"",""sAndSMasterRefNum"":""20GB01I0XLM976S003""}]}";
		string EIDRJsonExample => @"{""direction"":""GB_TO_NI"",""vehicleRegNum"":""AB19DEF"",""trailerRegistrationNums"":[""KF293716"",""GH28372S""],""plannedCrossing"":{""routeId"":""1"",""localDateTimeOfDeparture"":""2021-08-11T10:58""},""eidrDeclarations"":[{""traderEORI"":""GB123456789012"",""sAndSMasterRefNum"":""20GB01I0XLM976S003""}]}";
		string EmptyJsonExample => @"{""direction"":""UK_OUTBOUND"",""vehicleRegNum"":""AB19DEF"",""plannedCrossing"":{""routeId"":""61"",""localDateTimeOfDeparture"":""2021-08-11T10:58""},""emptyVehicle"":{""isOwnVehicle"":true,""sAndSMasterRefNum"":""20GB01I0XLM976S003""}}";
		string StandardJsonExample => @"{""direction"":""GB_TO_NI"",""vehicleRegNum"":""AB19DEF"",""plannedCrossing"":{""routeId"":""1"",""localDateTimeOfDeparture"":""2021-08-11T10:58""},""customsDeclarations"":[{""customsDeclarationId"":""0GB689223596000-SE119404"",""sAndSMasterRefNum"":""20GB01I0XLM976S001""},{""customsDeclarationId"":""0GB689223596000-SE119405"",""sAndSMasterRefNum"":""20GB01I0XLM976S002""},{""customsDeclarationId"":""0GB689223596000-SE119406"",""sAndSMasterRefNum"":""20GB01I0XLM976S003""},{""customsDeclarationId"":""0GB689223596000-SE119406"",""sAndSMasterRefNum"":""20GB01I0XLM976S003"",""customsDeclarationPartId"":""34J""}],""transitDeclarations"":[{""transitDeclarationId"":""10GB00002910B75BE5"",""isTSAD"":true},{""transitDeclarationId"":""10GB00002910B75BE6"",""isTSAD"":false,""sAndSMasterRefNum"":""20GB01I0XLM976S004""}]}";
		string TIRJsonExample => @"{""isUnaccompanied"":true,""direction"":""GB_TO_NI"",""trailerRegistrationNums"":[""KF293716"",""GH28372S""],""plannedCrossing"":{""routeId"":""1"",""localDateTimeOfDeparture"":""2021-08-11T10:58""},""tirDeclarations"":[{""tirCarnetId"":""FEOKEOK092927""}],""sAndSMasterRefNum"":""20GB01I0XLM976S001""}";
		string TransitJsonExample => @"{""direction"":""UK_OUTBOUND"",""vehicleRegNum"":""AB19DEF"",""plannedCrossing"":{""routeId"":""61"",""localDateTimeOfDeparture"":""2021-08-11T10:58""},""transitDeclarations"":[{""transitDeclarationId"":""10GB00002910B75BE5"",""isTSAD"":true},{""transitDeclarationId"":""10GB00002910B75BE6"",""isTSAD"":false,""sAndSMasterRefNum"":""20GB01I0XLM976S004""}]}";
		string IndirectExportExample => @"{""direction"":""NI_TO_GB"",""vehicleRegNum"":""AB19DEF"",""plannedCrossing"":{""routeId"":""7"",""localDateTimeOfDeparture"":""2021-08-11T10:58""},""indirectExportDeclarations"":[{""eadMasterRefNum"":""20IE01I0XLM976S003""},{""eadMasterRefNum"":""20IE01I0XLM976S004""}]}";
		string DbcExample => @"{""isUnaccompanied"":true,""direction"":""UK_OUTBOUND"",""vehicleRegNum"":""AB19DEF"",""trailerRegistrationNums"":[""KF293716"",""GH28372S""],""plannedCrossing"":{""routeId"":""61"",""localDateTimeOfDeparture"":""2021-08-11T10:58""},""dbcDeclaration"":{""isOwnVehicle"":false,""dbcGoods"":[{""sAndSMasterRefNum"":""GB12323020392""},{""sAndSMasterRefNum"":""GB12323020393""}]}}";
	}
}
