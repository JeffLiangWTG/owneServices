using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	public class AsycudaManifestDataObjectReaderTestHelper : DataObjectReaderTestHelper
	{
		public string GetTestFilePathFor(string filename) => TestFiles.GetTestFilePath(filename);

		public Shipment SetupManifestHeader(ZString? wayBillNumber, UNLOCO portOfLoading, UNLOCO portOfDischarge, ZDateTime arrivalTime, ZDateTime departureTime, ZString reference, ZString manifestType, string messageType = "")
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(HeaderDataContextType, reference);
			var result = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = wayBillNumber,
				TransportMode = new CodeDescriptionPair() { Code = TransportTypeList.Codes.Air, Description = TransportTypeList.Descriptions.Air },
				DeclarantType = new CodeDescriptionPair() { Code = Core.Constants.AgentType.Agent, Description = Core.Constants.AgentTypeDescriptions.Agent },
				ContainerMode = new ContainerMode() { Code = Core.Constants.ContainerModes.Containerised, Description = Core.Constants.ContainerModeDescriptions.Containerised },
				VoyageFlightNo = "QTX370",
				PortOfLoading = portOfLoading,
				PortOfDischarge = portOfDischarge,
				VesselName = "TestVessel",
				IsBuyersConsol = ZBool.True,
				MessageType = new CodeDescriptionPair() { Code = messageType, Description = messageType }
			};
			result.SetEntryInstructionCollection(() => new List<EntryInstruction>(new[] { new EntryInstruction() { Style = manifestType, Link = 1 } }));
			result.SetEntryHeaderCollection(() => new List<EntryHeader>(new[] { new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance) { Type = new EntryType() { Code = portOfDischarge?.Code.GetValueOrDefault().Left(2) }, EntryInstructionLink = 1 } }));
			result.SetAddInfoCollection(() =>
			{
				var res = new List<AddInfo>();
				res.Add(new AddInfo() { Key = AddInfoConstants.Header.ConveyanceNationality, Value = "XX" });
				res.Add(new AddInfo() { Key = AddInfoConstants.Header.MasterInformation, Value = "Captain Kirk" });
				res.Add(new AddInfo() { Key = AddInfoConstants.Header.Trailer1, Value = "TRAILER001" });
				res.Add(new AddInfo() { Key = AddInfoConstants.Header.Trailer1CountryOfRegistration, Value = "TR" });
				res.Add(new AddInfo() { Key = AddInfoConstants.Header.Trailer2, Value = "TRAILER002" });
				res.Add(new AddInfo() { Key = AddInfoConstants.Header.Trailer2CountryOfRegistration, Value = "ZA" });
				res.Add(new AddInfo() { Key = AddInfoConstants.Header.Trailer2CountryOfRegistration, Value = "ZA" });
				res.Add(new AddInfo() { Key = "VesselRadioCallSign", Value = "9064384" });
				return res;
			});

			result.SetDateCollection(() =>
			{
				var res = new List<Date>();
				res.Add(new Date() { Type = DateType.Arrival, Value = arrivalTime });
				res.Add(new Date() { Type = DateType.Departure, Value = departureTime });
				res.Add(new Date() { Type = DateType.BillIssued, Value = ZDateTime.Today.AddDays(4) });
				return res;
			});
			return result;
		}

		public Shipment SetupManifestHeaderWithCustomsPortsOfManifestHeader(ZString? wayBillNumber, UNLOCO portOfLoading, UNLOCO portOfDischarge, ZDateTime arrivalTime, ZDateTime departureTime, ZString reference, ZString? customsPortOfLoading, ZString? customsPortOfDischarge, ZString manifestType)
		{
			var currentShipment = SetupManifestHeader(wayBillNumber, portOfLoading, portOfDischarge, arrivalTime, departureTime, reference, manifestType);
			currentShipment.CustomsDischargePort = new CodeDescriptionPair10Char() { Code = customsPortOfDischarge, Description = customsPortOfDischarge };
			currentShipment.CustomsLoadPort = new CodeDescriptionPair10Char() { Code = customsPortOfLoading, Description = customsPortOfLoading };
			return currentShipment;
		}

		public EntryHeader SetupCountryHeaderEntryHeader(ZString countryCode, ZInt entryInstructionLink)
		{
			var result = new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Type = new EntryType() { Code = countryCode + "X" },
				EntryInstructionLink = entryInstructionLink,
			};
			return result;
		}

		public EntryInstruction SetupCountryHeaderEntryInstruction(ZInt entryInstructionLink, UNLOCO firstArrival, ZString carrierCode, ZString countryCode, ZString manifestType, ZString nature)
		{
			var result = new EntryInstruction()
			{
				Link = entryInstructionLink,
				CustomsOffice = new CodeDescriptionPair5Char() { Code = "USIR", Description = "Customs Office" },
				DateAtCustomsOffice = ZDateTime.Today,
				FirstArrival = firstArrival,
				Style = manifestType,
			};
			result.AddInfoCollection = new List<AddInfo>();
			result.OrganizationAddressCollection = new List<OrganizationAddress>();
			var carrier = SetupOrganizationAddress(nameof(DocAddressType.Carrier));
			carrier.SetRegistrationNumberCollection(() =>
			{
				var temp = new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>();
				temp.Add(new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
				{
					CountryOfIssue = new Country() { Code = countryCode },
					Type = new RegistrationNumberType() { Code = OrgCusCode.CodeTypes.CarrierCode },
					Value = carrierCode,
				});
				return temp;
			});
			result.OrganizationAddressCollection.Add(carrier);
			result.AddInfoCollection.Add(new AddInfo() { Key = AsycudaManifestHeaderEntryInstructionDataObjectReader.AHC_Nature, Value = nature });
			return result;
		}

		public Shipment SetupBill(ZString? wayBillNumber, UNLOCO portOfOrigin, UNLOCO portOfDestination, ZDecimal totalWeight, ZString goodsDescription, ZDecimal totalVolume, ZString carrierReference, ZString bolType, ZString prepaidCollect)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.AsycudaBill, null);
			var result = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = wayBillNumber,
				WayBillType = new WayBillType { Code = "HWB" },
				PortOfOrigin = portOfOrigin,
				PortOfDestination = portOfDestination,
				TotalWeight = totalWeight,
				TotalWeightUnit = new UnitOfWeight { Code = Core.Constants.Weight.Kilograms, Description = "kilogram" },
				GoodsDescription = goodsDescription,
				TotalVolume = totalVolume,
				TotalVolumeUnit = new UnitOfVolume { Code = Core.Constants.Volume.CubicMetres, Description = "Cubic metre" },
			};
			result.SetAddInfoCollection(() =>
			{
				var temp = new List<AddInfo>();
				temp.Add(new AddInfo { Key = AsycudaBill.Schema.ABL_CarrierReference, Value = carrierReference });
				temp.Add(new AddInfo { Key = AsycudaBill.Schema.ABL_BolType, Value = bolType });
				temp.Add(new AddInfo { Key = AsycudaBill.Schema.ABL_PrepaidCollect, Value = prepaidCollect });
				return temp;
			});
			return result;
		}

		public EntryHeader SetupCountryBillEntryHeader(ZString countryCode, ZInt entryInstructionLink, ZString billStatus, ZString senderReference)
		{
			var result = new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Type = new EntryType() { Code = countryCode + "X" },
				EntryInstructionLink = entryInstructionLink,
				EntryStatus = new EntryStatus() { Code = billStatus },
			};
			result.CustomsReferenceCollection = new List<CustomsReference>();
			result.CustomsReferenceCollection.Add(new CustomsReference()
			{
				Type = new CodeDescriptionPair() { Code = Constants.CustomsReferenceType.ABL_SenderReferenceType },
				Reference = senderReference,
			});
			result.CustomsReferenceCollection.Add(new CustomsReference()
			{
				Type = new CodeDescriptionPair() { Code = "DUMMY" },
				Reference = "dummy test",
			});
			return result;
		}

		public EntryInstruction SetupCountryBillEntryInstruction(ZInt entryInstructionLink, ZString goodsLocation, ZString locationInformation, ZString shipmentType, ZString countryCode, ZString billIssureCode,
			ZDecimal dutyAmount, ZDecimal taxAmount, ZString payeeIndicator, ZString partyStatus, ZString partyID, ZDateTime cycleDate, ZString cycleNumber)
		{
			var result = new EntryInstruction()
			{
				Link = entryInstructionLink,
				LocationAtClearance = new CodeDescriptionPair35Char() { Code = goodsLocation, Description = "Goods Location" },
				Style = shipmentType,
			};

			var billIssuer = SetupOrganizationAddress(nameof(DocAddressType.HouseBillIssuingParty));
			billIssuer.SetRegistrationNumberCollection(() =>
			{
				var temp = new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>();
				temp.Add(new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
				{
					CountryOfIssue = new Country() { Code = countryCode },
					Type = new RegistrationNumberType() { Code = Constants.RegistrationTypes.BillIssuer },
					Value = billIssureCode,
				});
				return temp;
			});
			result.OrganizationAddressCollection = new List<OrganizationAddress>();
			result.OrganizationAddressCollection.Add(billIssuer);
			result.AddInfoCollection = new List<AddInfo>();
			result.AddInfoCollection.Add(new AddInfo() { Key = AsycudaBillSchema.Constants.ABL_LocationInformation, Value = locationInformation });
			result.AddInfoCollection.Add(new AddInfo() { Key = AddInfoConstants.Bill.DutyAmount, Value = dutyAmount.ToString() });
			result.AddInfoCollection.Add(new AddInfo() { Key = AddInfoConstants.Bill.TaxAmount, Value = taxAmount.ToString() });
			result.AddInfoCollection.Add(new AddInfo() { Key = "PayeeIndicator", Value = payeeIndicator });
			result.AddInfoCollection.Add(new AddInfo() { Key = "PartyStatus", Value = partyStatus });
			result.AddInfoCollection.Add(new AddInfo() { Key = "PartyIndicator", Value = partyID });
			result.AddInfoCollection.Add(new AddInfo() { Key = "CycleDate", Value = cycleDate.ToISO8601String() });
			result.AddInfoCollection.Add(new AddInfo() { Key = "CycleNumber", Value = cycleNumber });
			return result;
		}

		public Container SetupContainer(ZString containerNumber, ZString seal, ZDecimal goodsWeight, ZString commodityCode, ZString stowageLocation, ZString emptyFullIndicator,
			ZString sealingPartyName, ZString sealingPartyType, ZInt numberOfPackages)
		{
			var result = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = containerNumber,
				Seal = seal,
				GrossWeight = goodsWeight,
				Commodity = new Commodity() { Code = commodityCode },
				StowagePosition = stowageLocation,
			};

			result.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo() { Key = AsycudaContainer.Schema.ACN_EmptyFullIndicator, Value = emptyFullIndicator },
				new AddInfo() { Key = AsycudaContainer.Schema.ACN_SealingPartyName, Value = sealingPartyName },
				new AddInfo() { Key = AsycudaContainer.Schema.ACN_SealingPartyType, Value = sealingPartyType },
				new AddInfo() { Key = AsycudaContainer.Schema.ACN_NumberOfPackages, Value = numberOfPackages.ToString() }
			});

			return result;
		}

		public PackingLine SetupPackingLine(ZString commodityCode, ZString goodsDescription, ZString marksandNos, ZLong packQty, ZDecimal weight, ZString containerNumber, ZInt consignmentReference, ZString? matchingReference = null, ZShort? itemNo = null)
		{
			var result = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Commodity = new Commodity() { Code = commodityCode },
				GoodsDescription = goodsDescription,
				MarksAndNos = marksandNos,
				PackQty = packQty,
				PackType = new PackageType() { Code = "NO" },
				Weight = weight,
				WeightUnit = new UnitOfWeight() { Code = "KG" },
				ContainerNumber = containerNumber
			};
			if (itemNo.HasValue)
			{
				result.ItemNo = itemNo;
			}
			result.SetAddInfoGroupCollection(() => new List<AddInfoGroup>());
			var addInfoGroup = new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = AddInfoConstants.Pack.PackAddInfoType, Description = AddInfoConstants.Pack.PackAddInfoTypeDescription },
				AddInfoCollection = new List<AddInfo>(),
			};
			addInfoGroup.AddInfoCollection.Add(new AddInfo() { Key = AddInfoConstants.Pack.ConsignmentReference, Value = consignmentReference.ToString() });
			if (matchingReference.HasValue)
			{
				addInfoGroup.AddInfoCollection.Add(new AddInfo() { Key = AddInfoConstants.Pack.MatchingReference, Value = matchingReference });
			}
			result.AddInfoGroupCollection?.Add(addInfoGroup);
			return result;
		}

		public PackingLine SetupPackedItem(ZString goodsOrigin, ZString tariff, ZString country, ZDecimal customsValue, ZDecimal dutyAmount, ZDecimal taxAmount, ZString goodsType, ZString permitNumber, ZString goodsDescription, ZDecimal customsQty, ZString customsUQ)
		{
			var result = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CountryOfOrigin = new Country() { Code = goodsOrigin },
				HarmonisedCode = tariff,
				GoodsDescription = goodsDescription,
			};
			result.SetAddInfoGroupCollection(() => new List<AddInfoGroup>());
			var addInfoGroup = new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = AddInfoConstants.PackedItem.PackingItemAddInfoType, Description = AddInfoConstants.PackedItem.PackingItemAddInfoTypeDescription },
				AddInfoCollection = new List<AddInfo>(),
			};
			addInfoGroup.AddInfoCollection.Add(new AddInfo() { Key = AddInfoConstants.PackedItem.Country, Value = country });
			addInfoGroup.AddInfoCollection.Add(new AddInfo() { Key = AddInfoConstants.PackedItem.CustomsValue, Value = customsValue.ToString() });
			addInfoGroup.AddInfoCollection.Add(new AddInfo() { Key = AddInfoConstants.PackedItem.DutyValue, Value = dutyAmount.ToString() });
			addInfoGroup.AddInfoCollection.Add(new AddInfo() { Key = AddInfoConstants.PackedItem.TaxValue, Value = taxAmount.ToString() });
			addInfoGroup.AddInfoCollection.Add(new AddInfo() { Key = "GoodsType", Value = goodsType });
			addInfoGroup.AddInfoCollection.Add(new AddInfo() { Key = "PermitNumber", Value = permitNumber });
			addInfoGroup.AddInfoCollection.Add(new AddInfo() { Key = AddInfoConstants.PackedItem.CustomsQty, Value = customsQty.ToString() });
			addInfoGroup.AddInfoCollection.Add(new AddInfo() { Key = AddInfoConstants.PackedItem.CustomsUQ, Value = customsUQ.ToString() });

			result.AddInfoGroupCollection?.Add(addInfoGroup);
			return result;
		}

		public RefUNLOCO GetAirLocalPort1(ZString countryCode)
		{
			var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, countryCode);
			query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
			var airLocalPort1 = Factory.LoadTop1<RefUNLOCO>(query);
			return airLocalPort1;
		}

		public RefUNLOCO GetAirLocalPort2(ZGuid airLocalPort1PK)
		{
			var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
			query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, airLocalPort1PK);
			query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
			var airLocalPort2 = Factory.LoadTop1<RefUNLOCO>(query);
			return airLocalPort2;
		}

		protected virtual DataContextType HeaderDataContextType => DataContextType.AsycudaManifest;
	}
}
