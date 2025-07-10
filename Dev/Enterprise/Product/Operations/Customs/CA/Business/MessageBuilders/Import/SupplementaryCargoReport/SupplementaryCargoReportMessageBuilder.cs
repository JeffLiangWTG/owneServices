//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using CargoWise.Types;
	using Enterprise.Customs.Business.MessageInterpretation;
	using Enterprise.Customs.CA.Edifact.GSMCAR;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Edifact.D00A.Elements;
	using Enterprise.Edifact.Utilities;

	public class SupplementaryCargoReportMessageBuilder : D00AMessageBuilder<ISupplementaryCargoReport, GSMCARMessage, SUPRPTMessage>
	{
		public SupplementaryCargoReportMessageBuilder(ISupplementaryCargoReport supplementaryCargoReportData, MessageSubTypes messageSubType)
			: base(supplementaryCargoReportData, messageSubType, DocumentNameCodeList.CustomsManifest)
		{
		}

		#region Overrides of EDIFACTMessageBuilder

		protected override void PopulateEdifactMessage()
		{
			#region PopulateUNH

			var unh = edifactMessage.UNH.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateUNH(unh, EDIMessage.MessageNumberPlaceHolder, "GSMCAR", "D", "00A", "UN", "SUPRPT");
			interpretation.AddUNHInterpretation(unh, unh.MessageReferenceNumber);

			#endregion

			#region PopulateBGM

			var bgm = edifactMessage.BGM.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateBGM(bgm, DocumentNameCode, "", data.DocumentMessageNumber, MessageFunctionCode);

			var bgmInterpretation = interpretation.AddNewSegmentInterpretation(bgm);
			bgmInterpretation.AddElementInterpretation(() => DocumentNameCode);
			bgmInterpretation.AddElementInterpretation(() => data.DocumentMessageNumber);
			bgmInterpretation.AddElementInterpretation(() => MessageFunctionCode);

			#endregion

			#region Service Option

			var cst = edifactMessage.CST.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateCST(cst, data.ServiceOption, CodeListResponsibleAgencyCodeList.CaRevenueCanadaCustomsAndExcise);
			interpretation.AddNewSegmentInterpretation(cst, () => data.ServiceOption);

			#endregion

			#region  Transport Details

			var group4 = edifactMessage.Group4.InstantiateAChildAndAddItToChildrenCollection();
			var tdt = group4.TDT.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateTDT(tdt, TransportStageCodeQualifierList.MainCarriageTransport, data.ModeOfTransport, data.CarrierCode);

			var tdtInterpretation = interpretation.AddNewSegmentInterpretation(tdt);
			tdtInterpretation.AddElementInterpretation(() => data.ModeOfTransport);
			tdtInterpretation.AddElementInterpretation(() => data.CarrierCode);

			#endregion

			#region Consignment

			var group7 = edifactMessage.Group7.InstantiateAChildAndAddItToChildrenCollection();

			#region Consignment Sequential Number

			var cni = group7.CNI.InstantiateAChildAndAddItToChildrenCollection();
			cni.ConsolidationItemNumber = "1";
			interpretation.AddNewSegmentInterpretation(cni, Res.GetString("f1a50165-8bc1-4b09-ac37-d3c1a115310e", "Consignment Sequential Number"), cni.ConsolidationItemNumber);

			#endregion

			#region Associated Transport Document Details

			var doc1 = group7.DOC.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateDOC(doc1, data.ModeOfTransport, data.OriginalCargoControlNumber);

			var doc1Interpretation = interpretation.AddNewSegmentInterpretation(doc1);
			doc1Interpretation.AddElementInterpretation(Res.GetString("a3d9c6c9-57d0-4015-b85e-787def4c25b8", "Associated Transport Document Type"), doc1.DocumentMessageName.DocumentNameCode);
			doc1Interpretation.AddElementInterpretation(() => data.OriginalCargoControlNumber);

			#endregion

			#region Unique Consignment Reference Number

			if (!data.UniqueConsignmentReference.IsEmpty)
			{
				var doc2 = group7.DOC.InstantiateAChildAndAddItToChildrenCollection();
				D00AMessageUtilities.PopulateDOC(doc2, DocumentNameCodeList.UniversalMultipurposeTransportDocument, data.UniqueConsignmentReference);
				interpretation.AddNewSegmentInterpretation(doc2, () => data.UniqueConsignmentReference);
			}

			#endregion

			#endregion

			#region Cargo Report

			var group8 = group7.Group8.InstantiateAChildAndAddItToChildrenCollection();

			#region Supplementary Reference Number

			var rff = group8.RFF.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.DeclarantsReferenceNumber, data.SupplementaryReferenceNumber);
			interpretation.AddNewSegmentInterpretation(rff, () => data.SupplementaryReferenceNumber);

			#endregion

			#region Place Of Destination

			var loc = group8.LOC.InstantiateAChildAndAddItToChildrenCollection();
			var destinationCityName = data.DestinationCityName.Left(25);
			D00AMessageUtilities.PopulateLOC(
				loc,
				LocationFunctionCodeQualifierList.PlaceOfDestination,
				data.DestinationCountryCode,
				destinationCityName,
				data.DestinationPortName);

			var locInterpretation = interpretation.AddNewSegmentInterpretation(loc);
			locInterpretation.AddElementInterpretation(() => data.DestinationCountryCode);
			locInterpretation.AddElementInterpretationIfNotEmpty(() => destinationCityName);
			locInterpretation.AddElementInterpretationIfNotEmpty(() => data.DestinationPortName);

			#endregion

			#region Customs Procedure

			var gei = group8.GEI.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateGEI(gei, data.CustomsProcedureCode);
			interpretation.AddNewSegmentInterpretation(gei, () => data.CustomsProcedureCode);

			#endregion

			#region Special Instructions

			if (!data.SpecialInstructions.IsEmpty)
			{
				var ftx = group8.FTX.InstantiateAChildAndAddItToChildrenCollection();
				D00AMessageUtilities.PopulateFTX(ftx, TextSubjectCodeQualifierList.SpecialInstructions, data.SpecialInstructions);
				interpretation.AddNewSegmentInterpretation(ftx, () => data.SpecialInstructions);
			}

			#endregion

			#endregion

			#region Mandatory Trigger Segment

			var group9 = group8.Group9.InstantiateAChildAndAddItToChildrenCollection();
			tdt = group9.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tdt.TransportStageCodeQualifier = TransportStageCodeQualifierList.AtDeparture;
			interpretation.AddMandatoryTriggerSegmentInterpretation(tdt);

			#endregion

			#region Bill Of Lading

			var billOfLading = data.BillOfLading.Trim();
			if (!billOfLading.IsEmpty)
			{
				var group10 = group9.Group10.InstantiateAChildAndAddItToChildrenCollection();
				rff = group10.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D00AMessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.CustomersIndividualTransactionReferenceNumber, billOfLading);
				interpretation.AddNewSegmentInterpretation(rff, () => billOfLading);
			}

			#endregion

			#region Addresses

			PopulateAddress(group8, PartyFunctionCodeQualifierList.Consignee, ContactFunctionCodeList.Consignee, () => data.Consignee);
			PopulateAddress(group8, PartyFunctionCodeQualifierList.Consignor, ContactFunctionCodeList.Consignor, () => data.Consignor);
			if (AddressNotEqual(data.DeliveryParty, data.Consignee))
			{
				PopulateAddress(group8, PartyFunctionCodeQualifierList.DeliveryParty, ContactFunctionCodeList.DeliveryContact, () => data.DeliveryParty);
			}
			PopulateAddress(group8, PartyFunctionCodeQualifierList.NotifyParty, ContactFunctionCodeList.NotificationContact, () => data.NotifyParty);

			#endregion

			#region Containers

			if (data.Containers != null)
			{
				foreach (var container in data.Containers)
				{
					PopulateContainer(group8, container);
				}
			}

			#endregion

			#region Goods Lines

			if (data.GoodsLines != null)
			{
				var lineNumber = 0;
				foreach (var goodsLine in data.GoodsLines)
				{
					PopulateGoodsLine(group8, goodsLine, ++lineNumber);
				}
			}

			#endregion

			#region Authentication

			if (!data.Authentication.IsEmpty)
			{
				var aut = edifactMessage.AUT.InstantiateAChildAndAddItToChildrenCollection();
				D00AMessageUtilities.PopulateAUT(aut, data.Authentication);
				interpretation.AddNewSegmentInterpretation(aut, () => data.Authentication);
			}

			#endregion

			#region PopulateUNT

			var unt = edifactMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateUNT(unt, edifactMessage.CountIncludingUNT.ToString(), unh.MessageReferenceNumber);
			interpretation.AddUNTInterpretation(unt, unt.MessageReferenceNumber);

			#endregion
		}

		#region Populate Address

		static bool AddressNotEqual(IAddressForACI address1, IAddressForACI address2)
		{
			return address1.Name != address2.Name
				   || address1.Address1 != address2.Address1
				   || address1.Address2 != address2.Address2
				   || address1.City != address2.City
				   || address1.State != address2.State
				   || address1.Contact != address2.Contact
				   || address1.Phone != address2.Phone;
		}

		void PopulateAddress(SegmentGroup8 group8, PartyFunctionCodeQualifierList addressType, ContactFunctionCodeList contactType, Expression<Func<IAddressForACI>> addressExpression)
		{
			var address = addressExpression.Compile()();
			if (address != null && !address.Name.IsEmpty && !address.Address1.IsEmpty)
			{
				#region  Company Name

				var group11 = group8.Group11.InstantiateAChildAndAddItToChildrenCollection();
				var nad = group11.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nad.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
				nad.PartyFunctionCodeQualifier = addressType;
				var nadInterpretation = interpretation.AddNewSegmentInterpretation(nad);

				const int companyNameAndAddressAndCityMaxLength = 35;
				var companyNameSplitter = new TextSplitter(companyNameAndAddressAndCityMaxLength) { Text = address.Name };
				nad.PartyName.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
				nad.PartyName.PartyName1 = companyNameSplitter[0].Trim();
				nad.PartyName.PartyName2 = companyNameSplitter[1].Trim();
				nadInterpretation.AddElementInterpretation(addressExpression, address.Name.Left(companyNameAndAddressAndCityMaxLength * 2));

				#endregion

				#region Address

				nad.Street.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
				if (address.Address1.Length > companyNameAndAddressAndCityMaxLength && address.Address2.IsEmpty)
				{
					companyNameSplitter = new TextSplitter(companyNameAndAddressAndCityMaxLength) { Text = address.Address1 };
					nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = companyNameSplitter[0].Trim();
					nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier2 = companyNameSplitter[1].Trim();

					nadInterpretation.AddElementInterpretation(() => address.Address1.Left(companyNameAndAddressAndCityMaxLength * 2));
				}
				else
				{
					nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = address.Address1.Left(companyNameAndAddressAndCityMaxLength);
					nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.Address1.Left(companyNameAndAddressAndCityMaxLength));

					nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier2 = address.Address2.Left(companyNameAndAddressAndCityMaxLength);
					nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.Address2.Left(companyNameAndAddressAndCityMaxLength));
				}

				#endregion

				#region City / State / Post Code / Country

				nad.CityName = address.City.Left(companyNameAndAddressAndCityMaxLength);
				nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.City.Left(companyNameAndAddressAndCityMaxLength));

				const int stateMaxLength = 4;
				nad.CountrySubEntityDetails.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
				nad.CountrySubEntityDetails.CountrySubEntityNameCode = address.State.Left(stateMaxLength);
				nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.State.Left(stateMaxLength));

				const int postcodeMaxLength = 9;
				nad.PostalIdentificationCode = address.PostCode.Left(postcodeMaxLength);
				nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.PostCode.Left(postcodeMaxLength));

				nad.CountryNameCode = address.CountryCode;
				nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.CountryCode);

				#endregion

				#region Contact Details

				var phone = address.Phone.KeepChars("1234567890").Left(12);
				if (!address.Contact.IsEmpty || !phone.IsEmpty)
				{
					var group12 = group11.Group12.InstantiateAChildAndAddItToChildrenCollection();
					var cta = group12.CTA.InstantiateAChildAndAddItToChildrenCollection();
					cta.DepartmentOrEmployeeDetails.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
					D00AMessageUtilities.PopulateCTA(cta, contactType, address.Contact);
					interpretation.AddNewSegmentInterpretation(cta, () => address.Contact);

					if (!phone.IsEmpty)
					{
						var com = group12.COM.InstantiateAChildAndAddItToChildrenCollection();
						D00AMessageUtilities.PopulateCOM(com, CommunicationNumberCodeQualifierList.Telephone, phone);
						interpretation.AddNewSegmentInterpretation(com, () => address.Phone, phone);
					}
				}

				#endregion
			}
		}

		#endregion

		#region Populate Container

		void PopulateContainer(SegmentGroup8 group8, ISCRContainer container)
		{
			var group14 = group8.Group14.InstantiateAChildAndAddItToChildrenCollection();
			var eqd = group14.EQD.InstantiateAChildAndAddItToChildrenCollection();

			D00AMessageUtilities.PopulateEQDContainer(
				eqd,
				container.ContainerNumber,
				container.CountryOfRegistration,
				container.ContainerSizeCode,
				container.IsEmpty);

			var eqdInterpretation = interpretation.AddNewSegmentInterpretation(eqd);
			eqdInterpretation.AddElementInterpretation(() => container.ContainerNumber);
			if (!container.CountryOfRegistration.IsEmpty && !container.ContainerSizeCode.IsEmpty)
			{
				eqdInterpretation.AddElementInterpretationIfNotEmpty(() => container.CountryOfRegistration);
				eqdInterpretation.AddElementInterpretationIfNotEmpty(() => container.ContainerSizeCode);
			}

			eqdInterpretation.AddElementInterpretation(Res.GetString("e3de9cfb-de3a-4fc5-ba12-7caf3f1ecfa8", "Container Status"), container.IsEmpty ? Res.GetString("e6880e02-8045-41d5-a9e7-c31b6c0a0154", "Empty") : Res.GetString("9dc34d10-e91f-4004-824d-9063c6b6640d", "Full"));
		}

		#endregion

		#region Populate Goods Line

		void PopulateGoodsLine(SegmentGroup8 group8, ISCRLine goodsLine, ZDecimal lineNumber)
		{
			var group15 = group8.Group15.InstantiateAChildAndAddItToChildrenCollection();

			#region Goods Item Number

			var gid = group15.GID.InstantiateAChildAndAddItToChildrenCollection();
			gid.GoodsItemNumber = lineNumber.ToString();
			interpretation.AddNewSegmentInterpretation(gid, () => gid.GoodsItemNumber);

			#endregion

			#region Packages

			var pac = group15.PAC.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulatePAC(pac, goodsLine.NumberOfPackages, goodsLine.TypeOfPackages);

			var pacInterpretation = interpretation.AddNewSegmentInterpretation(pac);
			pacInterpretation.AddElementInterpretation(() => goodsLine.NumberOfPackages);
			pacInterpretation.AddElementInterpretation(() => goodsLine.TypeOfPackages);

			#endregion

			#region Goods Description

			const int ftxCountMax = 9;
			int ftxCount = 0;
			var goodsDescriptions = goodsLine.GoodsDescription.Split(Environment.NewLine.ToCharArray());
			foreach (var goodsDescription in goodsDescriptions)
			{
				if (!goodsDescription.IsEmpty)
				{
					var friendlyName = PropertyNameProvider.GetFriendlyPropertyName(() => goodsLine.GoodsDescription);
					var splitter = new TextSplitter(50) { Text = goodsDescription };
					for (var i = 0; i < splitter.Count && ftxCount < ftxCountMax; i++)
					{
						var partOfDescription = splitter[i].Trim();
						if (!string.IsNullOrEmpty(partOfDescription))
						{
							ftxCount++;
							var ftx2 = group15.FTX.InstantiateAChildAndAddItToChildrenCollection();
							D00AMessageUtilities.PopulateFTX(ftx2, TextSubjectCodeQualifierList.GoodsDescription, partOfDescription);
							interpretation.AddNewSegmentInterpretation(ftx2, friendlyName, partOfDescription);
						}
						if (i == 0 && splitter.Count > 1)
						{
							friendlyName = GetContinued(friendlyName);
						}
					}
				}
				if (ftxCount == ftxCountMax)
				{
					break;
				}
			}

			#endregion

			#region Gross Weight

			var mea = group15.MEA.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateMEAWeight(mea, goodsLine.GrossWeight, goodsLine.WeightUnits);

			var meaInterpretation = interpretation.AddNewSegmentInterpretation(mea);
			meaInterpretation.AddElementInterpretation(() => goodsLine.GrossWeight, mea.ValueRange.MeasurementValue);
			meaInterpretation.AddElementInterpretation(() => goodsLine.WeightUnits, mea.ValueRange.MeasurementUnitCode);

			#endregion

			#region Volume

			if (!goodsLine.Volume.IsEmpty)
			{
				mea = group15.MEA.InstantiateAChildAndAddItToChildrenCollection();
				D00AMessageUtilities.PopulateMEAVolume(mea, goodsLine.Volume, goodsLine.VolumeUnits);

				meaInterpretation = interpretation.AddNewSegmentInterpretation(mea);
				meaInterpretation.AddElementInterpretation(() => goodsLine.Volume, mea.ValueRange.MeasurementValue);
				meaInterpretation.AddElementInterpretation(() => goodsLine.VolumeUnits, mea.MeasurementDetails.NonDiscreteMeasurementName);
			}

			#endregion

			#region Container Number

			if (!goodsLine.ContainerNumber.IsDefault)
			{
				var sgp = group15.SGP.InstantiateAChildAndAddItToChildrenCollection();
				D00AMessageUtilities.PopulateSGP(sgp, goodsLine.ContainerNumber);
				interpretation.AddNewSegmentInterpretation(sgp, () => goodsLine.ContainerNumber);
			}

			#endregion

			#region Dangerous Goods Code

			if (!goodsLine.DGCodes.IsEmpty)
			{
				var dgCodes = goodsLine.DGCodes.Split(',').Distinct();
				foreach (var dgCode in dgCodes)
				{
					var dgCodeTrimmed = dgCode.Trim().SubstringSafe(0, 4);
					var dgs = group15.DGS.InstantiateAChildAndAddItToChildrenCollection();
					D00AMessageUtilities.PopulateDGS(dgs, dgCodeTrimmed);
					interpretation.AddNewSegmentInterpretation(dgs, Res.GetString("da1e44b3-32a0-4838-bfaa-b863da9fce7b", "Dangerous Goods Code"), dgCodeTrimmed);
				}
			}

			#endregion

			#region Shipping Marks

			if (!goodsLine.ShippingMarks.IsEmpty)
			{
				var friendlyName = PropertyNameProvider.GetFriendlyPropertyName(() => goodsLine.ShippingMarks);
				var marksList = new List<ZString>(9);
				var builder = new ZStringBuilder();
				var marks = goodsLine.ShippingMarks.Split(Environment.NewLine.ToCharArray());

				foreach (var mark in marks)
				{
					if (!mark.IsEmpty)
					{
						var splitter = new TextSplitter(35) { Text = mark };
						for (var i = 0; i < splitter.Count; i++)
						{
							var partOfMark = (ZString)splitter[i];
							builder.Append(partOfMark);
							if (partOfMark.IsEmpty)
							{
								continue;
							}
							else
							{
								marksList.Add(partOfMark.Trim());
							}
							if (marksList.Count > 8)
							{
								PopulateShippingMarks(group15, marksList, friendlyName, builder);

								marksList = new List<ZString>(9);
								builder = new ZStringBuilder();
								if (group15.PCI.Count == 1)
								{
									friendlyName = GetContinued(friendlyName);
								}
							}
						}
					}
				}

				if (marksList.Count > 0)
				{
					PopulateShippingMarks(group15, marksList, friendlyName, builder);
				}
			}

			#endregion

			#region Tariff Numbers

			if (!goodsLine.TariffNumbers.IsEmpty)
			{
				var friendlyName = PropertyNameProvider.GetFriendlyPropertyName(() => goodsLine.TariffNumbers);
				var tariffList = new List<ZString>(5);
				var tariffNumbers = goodsLine.TariffNumbers.Split(',');

				foreach (var tariffNumber in tariffNumbers)
				{
					var tariffNumberKeepNumerics = tariffNumber.KeepNumericCharacters().Trim();
					if (!tariffNumberKeepNumerics.IsEmpty)
					{
						tariffList.Add(tariffNumberKeepNumerics);

						if (tariffList.Count > 4)
						{
							PopulateTariffNumbers(group15, tariffList, friendlyName);

							tariffList = new List<ZString>(5);
							if (group15.CST.Count == 1)
							{
								friendlyName = GetContinued(friendlyName);
							}
						}
					}
				}

				if (tariffList.Count > 0)
				{
					PopulateTariffNumbers(group15, tariffList, friendlyName);
				}
			}

			#endregion
		}

		static string GetContinued(string friendlyName)
		{
			return Res.GetString("03e854ce-518d-41f3-bdf9-aef40bb3cef0", "{0} (Continued)", friendlyName);
		}

		void PopulateShippingMarks(SegmentGroup15 group15, List<ZString> marksList, string friendlyName, ZStringBuilder value)
		{
			var pci = group15.PCI.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulatePCI(pci, marksList);
			interpretation.AddNewSegmentInterpretation(pci, friendlyName, value.ToString());
		}

		void PopulateTariffNumbers(SegmentGroup15 group15, List<ZString> tariffList, string friendlyName)
		{
			var cst = group15.CST.InstantiateAChildAndAddItToChildrenCollection();
			D00AMessageUtilities.PopulateCSTList(cst, tariffList);
			interpretation.AddNewSegmentInterpretation(cst, friendlyName, new ZStringBuilder(tariffList).ToStringWithDelimiterBetweenAppends(", "));
		}

		#endregion

		#endregion
	}
}
