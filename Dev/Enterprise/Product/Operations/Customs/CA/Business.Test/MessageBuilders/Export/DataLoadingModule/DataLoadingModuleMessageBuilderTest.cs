using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class DataLoadingModuleMessageBuilderTest : TestCaseWithFactory
	{
		public void TestPopulateMessage()
		{
			var header = GetDLMHeaderForTesting();
			var builder = new DataLoadingModuleMessageBuilder(header);
			var message = builder.PopulateMessage();
			var expectedMessage = GetExpectedMessage();
			AssertMultilineASCIIEquals("EM_MessageText", expectedMessage, message.EM_MessageText);
			AssertCollectionContains(message, header.Messages);
			AssertEquals("EM_MessageSubType", MessageTypeList.Codes.DataLoadingModule, message.EM_MessageSubType);
		}

		string GetExpectedMessage()
		{
			var builder = new ZStringBuilder();

			#region Setup Header

			var header = new DLMHeader();
			header.ExporterAuthorizationId = "SC0001";
			header.ExporterName = "Exporter Name";
			header.ExporterBusinessNumber = "123456789RM0001";
			header.ExporterStreet = "Exporter Street";
			header.ExporterCity = "Exporter City";
			header.ExporterProvinceState = "Quebec";
			header.ExporterCountry = "Canada";
			header.ExporterPostalZipCode = "J1J2J3";
			header.ExporterTelephone = "1111111111";
			header.ExporterTelephoneExtension = "";
			header.ExporterFax = "3333333333";

			header.ConsigneeName = "Consignee Name";
			header.ConsigneeStreet = "Consignee Street";
			header.ConsigneeCity = "Consignee City";
			header.ConsigneeProvinceState = "Consignee Province State";
			header.ConsigneeCountry = "Cambodia";

			header.ServiceProviderAuthorizationId = "SA0001";
			header.ServiceProviderName = "ServiceProviderName";
			header.ServiceProviderStreet = "Service Provider Street";
			header.ServiceProviderCity = "Service Provider City";
			header.ServiceProviderProvinceState = "SP Province";
			header.ServiceProviderCountry = "Saint Lucia";
			header.ServiceProviderPostalZipCode = "A9A9A9";
			header.ServiceProviderTelephone = "4444444444";
			header.ServiceProviderTelephoneExtension = "";

			header.CertifierName = "CERTIFIER NAME";
			header.CertifierStreet = "Certifier Street";
			header.CertifierCity = "Certifier City";
			header.CertifierProvinceState = "Alberta";
			header.CertifierCountry = "Canada";
			header.CertifierPostalZipCode = "K4K4K4";
			header.CertifierTelephone = "6666666666";
			header.CertifierTelephoneExtension = "";
			header.CertifierFax = "8888888888";
			header.CertifierCompanyName = "CERTIFIER COMPANY NAME";
			header.CertifierStatus = "2";

			header.FormKey = EDIMessage.FormKeyPlaceHolder;
			header.CommodityGrossWeight = 267.8m;
			header.CommodityGrossWeightUnitOfMeasure = "Kilogram";
			header.FreightCharges = 34.5m;
			header.CommodityCurrencyOfDeclaredValue = "Canadian Dollar";
			header.ModeOfTransport = "Water";
			header.ReasonForExport = "Reason For Export";
			header.VesselName = "Vessel Name";
			header.CountryOfFinalDestination = "Afghanistan";
			header.DateOfExportation = new ZDate(2005, 1, 18);
			header.PortOfExit = "Aden";
			header.PlaceOfReport = "Coutts";
			header.NumberOfPackages = 4;
			header.KindOfPackages = "paquets";
			header.NameOfExportingCompany = "EXPORTING COMPANY NAME";
			header.TransportationDocumentNumber = "Transp. Doc#1";

			builder.Append(header.Serialise());

			#endregion

			#region Setup Detail Line

			var detail = new DLMDetail();
			detail.CountryOfOrigin = "Canada";
			detail.ProvinceOfOrigin = "British Columbia";
			detail.HarmonizedSystemCode = "87019010";
			detail.ProductDescription = "Tracteur";
			detail.ConveyanceIdentificationNumber = "ID#123";
			detail.Quantity = 3;
			detail.UnitOfMeasure = "Number";
			detail.ValueFOBPointOfExit = 5000.33m;

			var detail2 = new DLMDetail();
			detail2.CountryOfOrigin = "Canada";
			detail2.ProvinceOfOrigin = "Manitoba";
			detail2.HarmonizedSystemCode = "87011000";
			detail2.ProductDescription = "Tractors";
			detail2.ConveyanceIdentificationNumber = "ID#345";
			detail2.Quantity = 1.2m;
			detail2.UnitOfMeasure = "Number";
			detail2.ValueFOBPointOfExit = 300.03m;

			builder.Append(detail.Serialise());
			builder.Append(detail2.Serialise());

			#endregion

			#region Setup Permits

			var permit1 = new DLMPermit();
			permit1.PermitNumber = "PRM12345";

			var permit2 = new DLMPermit();
			permit2.PermitNumber = "PRM#67890";

			builder.Append(permit1.Serialise());
			builder.Append(permit2.Serialise());

			#endregion

			#region Setup Containers

			var container1 = new DLMContainer();
			container1.ContainerNumber = "TURE1234560";

			var container2 = new DLMContainer();
			container2.ContainerNumber = "TURE7234232";

			builder.Append(container1.Serialise());
			builder.Append(container2.Serialise());

			#endregion

			#region Setup References

			var reference1 = new DLMReference();
			reference1.ReferenceNumber = "INV1234567";

			var reference2 = new DLMReference();
			reference2.ReferenceNumber = "PO23423322";

			builder.Append(reference1.Serialise());
			builder.Append(reference2.Serialise().TrimEnd());

			#endregion

			return builder.ToStringWithNewLineBetweenAppends();
		}

		IDLMHeader GetDLMHeaderForTesting()
		{
			var mock = new Mock<IDLMHeader>();
			var bizObj = Factory.New<DummyBusinessObject>();
			var messages = new EDIMessageCollection(bizObj);

			mock.Setup(m => m.Messages).Returns(messages);
			mock.Setup(m => m.Factory).Returns(Factory);

			#region Setup Header

			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "Exporter Name";
			exporter.MainAddress.OA_Address1 = "Exporter Street";
			exporter.OH_RL_NKClosestPort = "CAQUE";
			exporter.MainAddress.OA_City = "Exporter City";
			exporter.MainAddress.OA_State = "Quebec";
			exporter.MainAddress.OA_PostCode = "J1J2J3";
			exporter.MainAddress.OA_Phone = "21111-111111";
			exporter.MainAddress.OA_Fax = "3333-333 333";
			exporter.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.AuthorizationID, "SC0001");
			exporter.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForExport, "123456789RM0001");
			mock.Setup(m => m.DLMExporter).Returns(OrgWrapper.New(exporter));

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Consignee Name";
			consignee.MainAddress.OA_Address1 = "Consignee Street";
			consignee.OH_RL_NKClosestPort = "KHPNH";
			consignee.MainAddress.OA_City = "Consignee City";
			consignee.MainAddress.OA_State = "Consignee Province State";
			mock.Setup(m => m.DLMConsignee).Returns(OrgWrapper.New(consignee));

			var serviceProvider = Factory.New<OrgHeader>();
			serviceProvider.OH_FullName = "ServiceProviderName";
			serviceProvider.MainAddress.OA_Address1 = "Service Provider Street";
			serviceProvider.OH_RL_NKClosestPort = "LCCDS";
			serviceProvider.MainAddress.OA_City = "Service Provider City";
			serviceProvider.MainAddress.OA_State = "SP Province";
			serviceProvider.MainAddress.OA_PostCode = "A9A9A9";
			serviceProvider.MainAddress.OA_Phone = "4444444444";
			serviceProvider.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.AuthorizationID, "SA0001");
			mock.Setup(m => m.DLMServiceProvider).Returns(OrgWrapper.New(serviceProvider));

			var certifier = Factory.New<OrgHeader>();
			certifier.OH_FullName = "CERTIFIER COMPANY NAME";
			certifier.MainAddress.OA_Address1 = "Certifier Street";
			certifier.OH_RL_NKClosestPort = "CAABT";
			certifier.MainAddress.OA_City = "Certifier City";
			certifier.MainAddress.OA_State = "Alberta";
			certifier.MainAddress.OA_PostCode = "K4K4K4";
			certifier.MainAddress.OA_Phone = "+1 (666) 666-6666";
			certifier.MainAddress.OA_Fax = "8888888888";
			mock.Setup(m => m.DLMCertifier).Returns(OrgWrapper.New(certifier));
			mock.Setup(m => m.CertifierStatus).Returns(new ZString("2"));
			mock.Setup(m => m.CertifierName).Returns(new ZString("CERTIFIER NAME"));

			mock.Setup(m => m.CommodityGrossWeight).Returns(new ZDecimal(267.8m));
			mock.Setup(m => m.CommodityGrossWeightUnitOfMeasure).Returns(new ZString("Kilogram"));
			mock.Setup(m => m.FreightCharges).Returns(new ZDecimal(34.5m));
			mock.Setup(m => m.CommodityCurrencyOfDeclaredValue).Returns(new ZString("Canadian Dollar"));
			mock.Setup(m => m.ModeOfTransport).Returns(new ZString("Water"));
			mock.Setup(m => m.ReasonForExport).Returns(new ZString("Reason For Export"));
			mock.Setup(m => m.VesselName).Returns(new ZString("Vessel Name"));
			mock.Setup(m => m.CountryOfFinalDestination).Returns(new ZString("Afghanistan"));
			mock.Setup(m => m.DateOfExportation).Returns(new ZDateTime(2005, 1, 18));
			mock.Setup(m => m.PortOfExit).Returns(new ZString("Aden"));
			mock.Setup(m => m.PlaceOfReport).Returns(new ZString("Coutts"));
			mock.Setup(m => m.NumberOfPackages).Returns(new ZInt(4));
			mock.Setup(m => m.KindOfPackages).Returns(new ZString("paquets"));
			mock.Setup(m => m.NameOfExportingCompany).Returns(new ZString("EXPORTING COMPANY NAME"));
			mock.Setup(m => m.TransportationDocumentNumber).Returns(new ZString("Transp. Doc#1"));

			#endregion

			#region Setup Detail Line
			var mockDetailLine1 = new Mock<IDLMDetailLine>();
			mockDetailLine1.Setup(m => m.CountryOfOrigin).Returns(new ZString("Canada"));
			mockDetailLine1.Setup(m => m.ProvinceOfOrigin).Returns(new ZString("British Columbia"));
			mockDetailLine1.Setup(m => m.HarmonizedSystemCode).Returns(new ZString("87019010"));
			mockDetailLine1.Setup(m => m.ProductDescription).Returns(new ZString("Tracteur"));
			mockDetailLine1.Setup(m => m.ConveyanceIdentificationNumber).Returns(new ZString("ID#123"));
			mockDetailLine1.Setup(m => m.Quantity).Returns(new ZDecimal(3m));
			mockDetailLine1.Setup(m => m.UnitOfMeasure).Returns(new ZString("Number"));
			mockDetailLine1.Setup(m => m.ValueFOBPointOfExit).Returns(new ZDecimal(5000.33m));

			var mockDetailLine2 = new Mock<IDLMDetailLine>();
			mockDetailLine2.Setup(m => m.CountryOfOrigin).Returns(new ZString("Canada"));
			mockDetailLine2.Setup(m => m.ProvinceOfOrigin).Returns(new ZString("Manitoba"));
			mockDetailLine2.Setup(m => m.HarmonizedSystemCode).Returns(new ZString("87011000"));
			mockDetailLine2.Setup(m => m.ProductDescription).Returns(new ZString("Tractors"));
			mockDetailLine2.Setup(m => m.ConveyanceIdentificationNumber).Returns(new ZString("ID#345"));
			mockDetailLine2.Setup(m => m.Quantity).Returns(new ZDecimal(1.2m));
			mockDetailLine2.Setup(m => m.UnitOfMeasure).Returns(new ZString("Number"));
			mockDetailLine2.Setup(m => m.ValueFOBPointOfExit).Returns(new ZDecimal(300.03m));

			var details = new[] { mockDetailLine1.Object, mockDetailLine2.Object };
			mock.Setup(m => m.Details).Returns(details);
			#endregion

			mock.Setup(m => m.DLMPermits).Returns(new[] { new ZString("PRM12345"), new ZString("PRM#67890") });
			mock.Setup(m => m.DLMContainers).Returns(new[] { new ZString("TURE1234560"), new ZString("TURE7234232") });
			mock.Setup(m => m.DLMReferences).Returns(new[] { new ZString("INV1234567"), new ZString("PO23423322") });

			return mock.Object;
		}
	}
}
