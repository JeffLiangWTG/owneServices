using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public class DataLoadingModuleMessageBuilder
	{
		public DataLoadingModuleMessageBuilder(IDLMHeader messageAttachee)
		{
			CargoWise.Common.Argument.NotNull(messageAttachee, "messageAttachee");
			this.messageAttachee = messageAttachee;
		}

		public EDIMessage PopulateMessage()
		{
			var message = Block.CreateMessage(messageAttachee.Factory);
			messageAttachee.Messages.Add(message);
			return message;
		}

		public ZString GetHumanFriendlyMessageText()
		{
			return Block.Serialise(true);
		}

		#region Block

		MessageBlockGenerator Block
		{
			get
			{
				if (block == null)
				{
					block = new MessageBlockGenerator();
					UpdateMessageBlocks();
				}
				return block;
			}
		}
		MessageBlockGenerator block;

		#region UpdateMessageBlocks

		void UpdateMessageBlocks()
		{
			AddDLMHeader();

			foreach (var detailLine in messageAttachee.Details)
			{
				AddDLMDetail(detailLine);
			}

			foreach (var permit in messageAttachee.DLMPermits)
			{
				AddDLMPermit(permit);
			}

			foreach (var container in messageAttachee.DLMContainers)
			{
				AddDLMContainer(container);
			}

			foreach (var reference in messageAttachee.DLMReferences)
			{
				AddDLMReference(reference);
			}
		}

		void AddDLMReference(ZString reference)
		{
			if (!reference.IsEmpty)
			{
				var messageBlock = new DLMReference();
				messageBlock.ReferenceNumber = reference;
				Block.AddMessageBlock(messageBlock);
			}
		}

		void AddDLMContainer(ZString container)
		{
			if (!container.IsEmpty)
			{
				var messageBlock = new DLMContainer();
				messageBlock.ContainerNumber = container;
				Block.AddMessageBlock(messageBlock);
			}
		}

		void AddDLMPermit(ZString permit)
		{
			if (!permit.IsEmpty)
			{
				var messageBlock = new DLMPermit();
				messageBlock.PermitNumber = permit;
				Block.AddMessageBlock(messageBlock);
			}
		}

		void AddDLMDetail(IDLMDetailLine detailLine)
		{
			var detail = new DLMDetail();
			detail.CountryOfOrigin = detailLine.CountryOfOrigin;
			detail.ProvinceOfOrigin = detailLine.ProvinceOfOrigin;
			detail.HarmonizedSystemCode = detailLine.HarmonizedSystemCode;
			detail.ProductDescription = detailLine.ProductDescription;
			detail.ConveyanceIdentificationNumber = detailLine.ConveyanceIdentificationNumber;
			detail.Quantity = detailLine.Quantity;
			detail.UnitOfMeasure = detailLine.UnitOfMeasure;
			detail.ValueFOBPointOfExit = detailLine.ValueFOBPointOfExit;
			Block.AddMessageBlock(detail);
		}

		#region AddDLMHeader

		void AddDLMHeader()
		{
			var header = new DLMHeader();
			header.FormKey = EDIMessage.FormKeyPlaceHolder;

			AddExporter(header);
			AddConsignee(header);
			AddServiceProvider(header);
			AddCertifier(header);

			header.CommodityGrossWeight = messageAttachee.CommodityGrossWeight;
			header.CommodityGrossWeightUnitOfMeasure = messageAttachee.CommodityGrossWeightUnitOfMeasure;
			header.FreightCharges = messageAttachee.FreightCharges;
			header.CommodityCurrencyOfDeclaredValue = messageAttachee.CommodityCurrencyOfDeclaredValue;
			header.ModeOfTransport = messageAttachee.ModeOfTransport;
			header.ReasonForExport = messageAttachee.ReasonForExport;
			header.VesselName = messageAttachee.VesselName;
			header.CountryOfFinalDestination = messageAttachee.CountryOfFinalDestination;
			header.DateOfExportation = messageAttachee.DateOfExportation.Date;
			header.PortOfExit = messageAttachee.PortOfExit;
			header.PlaceOfReport = messageAttachee.PlaceOfReport;
			header.NumberOfPackages = messageAttachee.NumberOfPackages;
			header.KindOfPackages = messageAttachee.KindOfPackages;
			header.NameOfExportingCompany = messageAttachee.NameOfExportingCompany;
			header.TransportationDocumentNumber = messageAttachee.TransportationDocumentNumber;
			Block.AddMessageBlock(header);
		}

		void AddCertifier(DLMHeader header)
		{
			var certifier = messageAttachee.DLMCertifier;
			if (certifier != null)
			{
				header.CertifierName = messageAttachee.CertifierName;
				header.CertifierStreet = certifier.Street;
				header.CertifierCity = certifier.City;
				header.CertifierProvinceState = certifier.ProvinceState;
				header.CertifierCountry = certifier.Country;
				header.CertifierPostalZipCode = certifier.PostalZipCode;
				header.CertifierTelephone = certifier.Telephone;
				header.CertifierTelephoneExtension = ZString.Empty;
				header.CertifierFax = certifier.Fax;
				header.CertifierCompanyName = certifier.CompanyName;
				header.CertifierStatus = messageAttachee.CertifierStatus;
			}
		}

		void AddServiceProvider(DLMHeader header)
		{
			var serviceProvider = messageAttachee.DLMServiceProvider;
			if (serviceProvider != null)
			{
				header.ServiceProviderAuthorizationId = serviceProvider.AuthorizationId;
				header.ServiceProviderName = serviceProvider.CompanyName;
				header.ServiceProviderStreet = serviceProvider.Street;
				header.ServiceProviderCity = serviceProvider.City;
				header.ServiceProviderProvinceState = serviceProvider.ProvinceState;
				header.ServiceProviderCountry = serviceProvider.Country;
				header.ServiceProviderPostalZipCode = serviceProvider.PostalZipCode;
				header.ServiceProviderTelephone = serviceProvider.Telephone;
				header.ServiceProviderTelephoneExtension = ZString.Empty;
			}
		}

		void AddConsignee(DLMHeader header)
		{
			var consignee = messageAttachee.DLMConsignee;
			if (consignee != null)
			{
				header.ConsigneeName = consignee.CompanyName;
				header.ConsigneeStreet = consignee.Street;
				header.ConsigneeCity = consignee.City;
				header.ConsigneeProvinceState = consignee.ProvinceState;
				header.ConsigneeCountry = consignee.Country;
			}
		}

		void AddExporter(DLMHeader header)
		{
			var exporter = messageAttachee.DLMExporter;
			if (exporter != null)
			{
				header.ExporterAuthorizationId = exporter.AuthorizationId;
				header.ExporterBusinessNumber = exporter.BusinessNumberForImportExport;
				header.ExporterName = exporter.CompanyName;
				header.ExporterStreet = exporter.Street;
				header.ExporterCity = exporter.City;
				header.ExporterProvinceState = exporter.ProvinceState;
				header.ExporterCountry = exporter.Country;
				header.ExporterPostalZipCode = exporter.PostalZipCode;
				header.ExporterTelephone = exporter.Telephone;
				header.ExporterTelephoneExtension = ZString.Empty;
				header.ExporterFax = exporter.Fax;
			}
		}
		#endregion

		#endregion

		#endregion

		protected readonly IDLMHeader messageAttachee;
	}
}
