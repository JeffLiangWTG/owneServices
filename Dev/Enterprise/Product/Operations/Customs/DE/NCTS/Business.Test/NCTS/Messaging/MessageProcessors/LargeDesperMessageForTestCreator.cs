using System;
using System.Text;

namespace Enterprise.Customs.DE.NCTS.Business.Testing;

static class LargeDesperMessageForTestCreator
{
	public static string GetLargeMessage()
	{
		var sb = new StringBuilder(4488535);
		AddStart(sb);
		AddTransportEquipment(sb, 1, 10, 1000, 1);
		for (var i = 2; i <= 1000; i++)
		{
			AddTransportEquipment(sb, i, 1, 1, 2);
		}
		for (var i = 1; i <= 100; i++)
		{
			AddDepartureTransportMeans(sb, i);
		}
		AddDocuments(sb);

		AddIncidents(sb);
		var declarationGoodsItemCounter = 0;
		AddHouseConsignment(sb, 1, 999, ref declarationGoodsItemCounter);
		for (var i = 2; i <= 10; i++)
		{
			AddHouseConsignment(sb, i, 5, ref declarationGoodsItemCounter);
		}

		AddEnd(sb);
		var message = sb.ToString();
		return message;
	}

	static void AddHouseConsignment(StringBuilder sb, int seqNum, int consignmentsCount, ref int declarationGoodsItemCounter)
	{
		AddHouseConsignmentStart(sb, seqNum);
		if (seqNum == 1)
		{
			AddDocuments(sb);
		}
		else
		{
			AddDocuments(sb, 2);
		}
		for (var i = 1; i <= consignmentsCount; i++)
		{
			AddConsignmentItem(sb, i, ref declarationGoodsItemCounter);
		}
		sb.Append("""
				</HouseConsignment>

""");
	}

	static void AddConsignmentItem(StringBuilder sb, int seqNum, ref int declarationGoodsItemCounter)
	{
		declarationGoodsItemCounter++;
		sb.AppendFormat("""
					<ConsignmentItem>
						<sequenceNumber>{0}</sequenceNumber>
						<goodsItemNumber>{0}</goodsItemNumber>
						<declarationGoodsItemNumber>{1}</declarationGoodsItemNumber>
						<Commodity>
							<descriptionOfGoods>1234567Hier_könnte_ihre_Warenbezeichnung_zur_Position_stehen_laut_Tarif_oder_eigener_Beschreibung_Hier_könnte_ihre_Warenbezeichnung_zur_Position_stehen_laut_Tarif_oder_eigener_Beschreibung_Hier_könnte_ihre_Warenbezeichnung_zur_Position_stehen_laut_Tarif_oder_eigener_Beschreibung_</descriptionOfGoods>
							<CommodityCode>
								<harmonizedSystemSubHeadingCode>220840</harmonizedSystemSubHeadingCode>
								<combinedNomenclatureCode>11</combinedNomenclatureCode>
							</CommodityCode>
							<DangerousGoods>
								<UNNumber>3400</UNNumber>
							</DangerousGoods>
							<DangerousGoods>
								<UNNumber>0012</UNNumber>
							</DangerousGoods>
							<GoodsMeasure>
								<grossMass>100.456</grossMass>
								<netMass>100.001123</netMass>
							</GoodsMeasure>
						</Commodity>

""", seqNum, declarationGoodsItemCounter);
		var packageCount = declarationGoodsItemCounter == 1 ? 10 : 2;
		for (var i = 1; i <= packageCount; i++)
		{
			AddPackaging(sb, i);
		}

		if (seqNum == 1)
		{
			AddItemDocuments(sb);
		}
		sb.Append("""
					</ConsignmentItem>

""");
	}

	static void AddPackaging(StringBuilder sb, int seqNum) => sb.AppendFormat("""
					<Packaging>
						<sequenceNumber>{0}</sequenceNumber>
						<typeOfPackages>4C</typeOfPackages>
						<numberOfPackages>12345678</numberOfPackages>
						<shippingMarks>Hier_könnte_ihr_Packstück_Zeichen_als_näh{0}</shippingMarks>
					</Packaging>

""", seqNum);

	static void AddHouseConsignmentStart(StringBuilder sb, int seqNum) => sb.AppendFormat("""
				<HouseConsignment>
					<sequenceNumber>{0}</sequenceNumber>
					<grossMass>80000000000</grossMass>
					<Consignee>
						<name>Empfänger_Einzelsendung_Name_Phase_5_Versand</name>
						<Address>
							<streetAndNumber>Empfänger_Einzelsendung_Strasse_Hausnummer_70</streetAndNumber>
							<postcode>EC3M 7AY_LondonGB</postcode>
							<city>London_Baker_City</city>
							<country>FR</country>
						</Address>
					</Consignee>

""", seqNum);

	static void AddDocuments(StringBuilder sb, int documentsCountPerKind = 10)
	{
		for (var i = 1; i <= documentsCountPerKind; i++)
		{
			AddPreviousDocuments(sb, i);
		}
		for (var i = 1; i <= documentsCountPerKind; i++)
		{
			AddSupportingDocuments(sb, i);
		}
		for (var i = 1; i <= documentsCountPerKind; i++)
		{
			AddTransportDocuments(sb, i);
		}
		for (var i = 1; i <= documentsCountPerKind; i++)
		{
			AddAdditionalReference(sb, i);
		}
	}

	static void AddItemDocuments(StringBuilder sb)
	{
		for (var i = 1; i <= 2; i++)
		{
			AddPreviousDocuments(sb, i);
		}
		for (var i = 1; i <= 2; i++)
		{
			AddSupportingDocuments(sb, i);
		}
		for (var i = 1; i <= 2; i++)
		{
			AddAdditionalReference(sb, i);
		}
	}

	static void AddTransportEquipment(StringBuilder sb, int seqNum, int sealCount, int goodsRefCount, int contNum)
	{
		sb.AppendFormat("""
				<TransportEquipment>
					<sequenceNumber>{0}</sequenceNumber>
					<containerIdentificationNumber>Containernummer_{2}</containerIdentificationNumber>
					<numberOfSeals>{1}</numberOfSeals>

""", seqNum, sealCount, contNum);
		for (var i = 1; i <= sealCount; i++)
		{
			sb.AppendFormat("""
					<Seal>
						<sequenceNumber>{0}</sequenceNumber>
						<identifier>Tyden_Seal_Vers_{0:D4}</identifier>
					</Seal>

""", i);
		}

		for (var i = 1; i <= goodsRefCount; i++)
		{
			sb.AppendFormat("""
					<GoodsReference>
						<sequenceNumber>{0}</sequenceNumber>
						<declarationGoodsItemNumber>{0}</declarationGoodsItemNumber>
					</GoodsReference>

""", i);
		}

		sb.Append("""
				</TransportEquipment>

""");
	}
	static void AddDepartureTransportMeans(StringBuilder sb, int seqNum) => sb.AppendFormat("""
				<DepartureTransportMeans>
					<sequenceNumber>{0}</sequenceNumber>
					<typeOfIdentification>20</typeOfIdentification>
					<identificationNumber>WAGON NR {0:D3}</identificationNumber>
					<nationality>GB</nationality>
				</DepartureTransportMeans>

""", seqNum);

	static void AddPreviousDocuments(StringBuilder sb, int seqNum) => sb.AppendFormat("""
				<PreviousDocument>
					<type>N325</type>
					<referenceNumber>Sammelsendung_Vorpapier_{0:D3}</referenceNumber>
					<complementOfInformation>Zusatz_Vorpapier_{0:D3}</complementOfInformation>
				</PreviousDocument>

""", seqNum);
	static void AddSupportingDocuments(StringBuilder sb, int seqNum) => sb.AppendFormat("""
				<SupportingDocument>
					<type>N933</type>
					<referenceNumber>Sammelsendung_Unterlage_{0:D3}</referenceNumber>
					<documentLineItemNumber>100{0:D2}</documentLineItemNumber>
					<complementOfInformation>Zusatz_Unterlage_Sammelse_{0:D3}</complementOfInformation>
				</SupportingDocument>

""", seqNum);
	static void AddTransportDocuments(StringBuilder sb, int seqNum) => sb.AppendFormat("""
				<TransportDocument>
					<type>N730</type>
					<referenceNumber>Sammelsendung_Transportdokument_{0:D3}</referenceNumber>
				</TransportDocument>

""", seqNum);
	static void AddAdditionalReference(StringBuilder sb, int seqNum) => sb.AppendFormat("""
				<AdditionalReference>
					<type>9ZZZ</type>
					<referenceNumber>Sammelsendung_zusätzlicher_Vermerk_{0:D3}</referenceNumber>
				</AdditionalReference>

""", seqNum);

	static void AddIncidents(StringBuilder sb) => sb.Append("""
			<Incident>
				<code>3</code>
				<text>Erläuterung zum Sachverhalt aufgrund eines Vorfalls während des Transportes der Waren für dieses Versandverfahren. Der LKW hatte eine Panne und musste abgeschleppt werden zum nächstgelegenen Zollamt. Hier erfolgte eine Umladung der Waren auf ein neues Transportfahrzeug. Durch die Umladung mussten neue Plomben angelegt werden und die alten entfernt. Der Zollbeamte vor Ort hat dies entsprechend protokolliert und die neuen Verschlüsse eingetragen. Nach einer Pause konnte der Fahrer seine Fahrt fortsetzen.Ende0</text>
				<Endorsement>
					<date>2023-05-09</date>
					<authority>Zollamt in Paris</authority>
					<place>Amtsplatz beim Zollamt</place>
					<country>FR</country>
				</Endorsement>
				<Location>
					<qualifierOfIdentification>W</qualifierOfIdentification>
					<country>FR</country>
					<GNSS>
						<latitude>+51.4899172</latitude>
						<longitude>+147.4607103</longitude>
					</GNSS>
				</Location>
				<TransportEquipment>
					<containerIdentificationNumber>CSQU3054383478956</containerIdentificationNumber>
					<numberOfSeals>1</numberOfSeals>
					<Seal>
						<identifier>Verschluss_1_F74354G</identifier>
					</Seal>
					<GoodsReference>
						<declarationGoodsItemNumber>1045</declarationGoodsItemNumber>
					</GoodsReference>
				</TransportEquipment>
				<Transhipment>
					<containerIndicator>1</containerIndicator>
					<TransportMeans>
						<typeOfIdentification>30</typeOfIdentification>
						<identificationNumber>Kennzeichen_neu_Umladung_ZG 7890 RT</identificationNumber>
						<nationality>FR</nationality>
					</TransportMeans>
				</Transhipment>
			</Incident>
			<Incident>
				<code>2</code>
				<text>Verschlussverletzung nach Umladung festgestellt</text>
				<Endorsement>
					<date>2023-05-09</date>
					<authority>Zollamt in Paris</authority>
					<place>Amtsplatz beim Zollamt</place>
					<country>FR</country>
				</Endorsement>
				<Location>
					<qualifierOfIdentification>U</qualifierOfIdentification>
					<UNLocode>BMPA5</UNLocode>
					<country>FR</country>
				</Location>
				<TransportEquipment>
					<numberOfSeals>1</numberOfSeals>
					<Seal>
						<identifier>Verschluss_2_F74354G</identifier>
					</Seal>
					<GoodsReference>
						<declarationGoodsItemNumber>1</declarationGoodsItemNumber>
					</GoodsReference>
				</TransportEquipment>
			</Incident>

""");

	static void AddStart(StringBuilder sb) => sb.Append("""
<DECustomsData xmlns="">
	<LogbookTime>2023-05-17T12:15:42.8011482+02:00</LogbookTime>
	<CustomsData>
		<DETSPC>
			<preparationDateAndTime>2023-05-17T08:27:43</preparationDateAndTime>
			<messageIdentification>7000000000007</messageIdentification>
			<messageGroup>DES</messageGroup>
			<messageType>DETSPC</messageType>
			<messageVersion>C.1.9</messageVersion>
			<correlationIdentifier>HYEZNTCMT00000000002089</correlationIdentifier>
			<MessageSender>
				<referenceNumber>DE005875</referenceNumber>
			</MessageSender>
			<MessageRecipient>
				<identificationNumber>DE9000348</identificationNumber>
				<subsidiaryNumber>0001</subsidiaryNumber>
			</MessageRecipient>
			<TransitOperation>
				<MRN>23DE587500031238M9</MRN>
				<declarationType>T1</declarationType>
				<declarationAcceptanceDate>2023-05-09</declarationAcceptanceDate>
				<limitDate>2023-06-30</limitDate>
				<security>3</security>
				<reducedDatasetIndicator>0</reducedDatasetIndicator>
			</TransitOperation>
			<CustomsOfficeOfDestinationActual>
				<referenceNumber>DE005875</referenceNumber>
			</CustomsOfficeOfDestinationActual>
			<HolderOfTheTransitProcedure>
				<identificationNumber>DE8998272</identificationNumber>
				<name>ZERTIFIZIERUNG PHASE 5 FIRMENKURZNAME IN FORMATAUSSCHOEPFUNG MIT 70 ZE</name>
				<Address>
					<streetAndNumber>Dr.-Pfleger-Strasse 36 ğİôŔęŁæÐﯼﯽ㓢㓣</streetAndNumber>
					<postcode>92637</postcode>
					<city>Weiden in der schönen ğİôŔęŁæÐﯼﯽ㓢㓣3</city>
					<country>DE</country>
				</Address>
			</HolderOfTheTransitProcedure>
			<TraderAtDestination>
				<identificationNumber>DE8999120</identificationNumber>
				<subsidiaryNumber>0000</subsidiaryNumber>
			</TraderAtDestination>
			<Consignment>
				<countryOfDispatch>DE</countryOfDispatch>
				<countryOfDestination>RU</countryOfDestination>
				<containerIndicator>1</containerIndicator>
				<inlandModeOfTransport>2</inlandModeOfTransport>
				<grossMass>999999999999</grossMass>
				<Consignor>
					<name>Versender_Sammelsendung_Name_Phase_5_Versand_1㐿™€чШÅËöÅçËöŠïÔŠïÔчШĢøÅ€</name>
					<Address>
						<streetAndNumber>Versender_Sammelsendung_Strasse_Hausnummer_70_㐿™€чШÅËöÅçËöŠïÔŠïÔчШĢøÅ€</streetAndNumber>
						<postcode>92637</postcode>
						<city>Weiden</city>
						<country>DE</country>
					</Address>
				</Consignor>

""");
	public static void AddEnd(StringBuilder sb) => sb.AppendFormat("""
			</Consignment>
		</DETSPC>
	</CustomsData>
	<AttachedDocumentCollection>
		<AttachedDocument>
			<FileName>DES-1-DE9000348-0001-DE005875_7000000000007.pdf</FileName>
			<Type>
				<Code>CAU</Code>
				<Description>Report DETSPC</Description>
			</Type>
			<ImageData>{0}</ImageData>
		</AttachedDocument>
	</AttachedDocumentCollection>
</DECustomsData>
""", GetLargeBase64FileString());

	static string GetLargeBase64FileString()
	{
		// 2MB base64 = ~1.5MB raw bytes (since base64 expands by 4/3)
		const int rawSize = 2 * 1024 * 1024 * 3 / 4;
		var bytes = new byte[rawSize];
		for (var i = 0; i < bytes.Length; i++)
		{
			bytes[i] = (byte)(i % 256);
		}
		return Convert.ToBase64String(bytes);
	}
}

