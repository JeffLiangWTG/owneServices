<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
	<xsl:output method="xml" />
	<xsl:template match="@* | node()">
		<Entity DataMajorVersion="2" DataMinorVersion="0">
			<xsl:call-template name ="CopyId">
				<xsl:with-param name="idNode" select="/Entity/Id" />
			</xsl:call-template>

			<xsl:variable name="paymentHandlingInstructionItems">
				<xsl:variable name="PrepaidCode" select="'PPD'" />
				<xsl:variable name="PrepaidDesc" select="'Prepaid'" />

				<xsl:variable name="CollectCode" select="'CCX'" />
				<xsl:variable name="CollectDesc" select="'Collect'" />

				<xsl:call-template name ="CopyPrepaidCollectProperty">
					<xsl:with-param name="source" select="'BasicFreightPrepaid'"/>
					<xsl:with-param name="categoryCode" select="'FRT'" />
					<xsl:with-param name="categoryDesc" select="'Freight Charge'"/>
					<xsl:with-param name="paymentMethodCode" select="$PrepaidCode"/>
					<xsl:with-param name="paymentMethodDesc" select="$PrepaidDesc"/>
				</xsl:call-template>

				<xsl:call-template name ="CopyPrepaidCollectProperty">
					<xsl:with-param name="source" select="'BasicFreightCollect'"/>
					<xsl:with-param name="categoryCode" select="'FRT'" />
					<xsl:with-param name="categoryDesc" select="'Freight Charge'"/>
					<xsl:with-param name="paymentMethodCode" select="$CollectCode"/>
					<xsl:with-param name="paymentMethodDesc" select="$CollectDesc"/>
				</xsl:call-template>

				<xsl:call-template name ="CopyPrepaidCollectProperty">
					<xsl:with-param name="source" select="'OriginPortChargePrepaid'"/>
					<xsl:with-param name="categoryCode" select="'OPC'" />
					<xsl:with-param name="categoryDesc" select="'Origin Port Charge'"/>
					<xsl:with-param name="paymentMethodCode" select="$PrepaidCode"/>
					<xsl:with-param name="paymentMethodDesc" select="$PrepaidDesc"/>
				</xsl:call-template>

				<xsl:call-template name ="CopyPrepaidCollectProperty">
					<xsl:with-param name="source" select="'OriginPortChargeCollect'"/>
					<xsl:with-param name="categoryCode" select="'OPC'" />
					<xsl:with-param name="categoryDesc" select="'Origin Port Charge'"/>
					<xsl:with-param name="paymentMethodCode" select="$CollectCode"/>
					<xsl:with-param name="paymentMethodDesc" select="$CollectDesc"/>
				</xsl:call-template>

				<xsl:call-template name ="CopyPrepaidCollectProperty">
					<xsl:with-param name="source" select="'OriginHaulagePrepaid'"/>
					<xsl:with-param name="categoryCode" select="'OHC'" />
					<xsl:with-param name="categoryDesc" select="'Origin Haulage Charge'"/>
					<xsl:with-param name="paymentMethodCode" select="$PrepaidCode"/>
					<xsl:with-param name="paymentMethodDesc" select="$PrepaidDesc"/>
				</xsl:call-template>

				<xsl:call-template name ="CopyPrepaidCollectProperty">
					<xsl:with-param name="source" select="'OriginHaulageCollect'"/>
					<xsl:with-param name="categoryCode" select="'OHC'" />
					<xsl:with-param name="categoryDesc" select="'Origin Haulage Charge'"/>
					<xsl:with-param name="paymentMethodCode" select="$CollectCode"/>
					<xsl:with-param name="paymentMethodDesc" select="$CollectDesc"/>
				</xsl:call-template>

				<xsl:call-template name ="CopyPrepaidCollectProperty">
					<xsl:with-param name="source" select="'DestinationPortChargePrepaid'"/>
					<xsl:with-param name="categoryCode" select="'DPC'" />
					<xsl:with-param name="categoryDesc" select="'Destination Port Charge'"/>
					<xsl:with-param name="paymentMethodCode" select="$PrepaidCode"/>
					<xsl:with-param name="paymentMethodDesc" select="$PrepaidDesc"/>
				</xsl:call-template>

				<xsl:call-template name ="CopyPrepaidCollectProperty">
					<xsl:with-param name="source" select="'DestinationPortChargeCollect'"/>
					<xsl:with-param name="categoryCode" select="'DPC'" />
					<xsl:with-param name="categoryDesc" select="'Destination Port Charge'"/>
					<xsl:with-param name="paymentMethodCode" select="$CollectCode"/>
					<xsl:with-param name="paymentMethodDesc" select="$CollectDesc"/>
				</xsl:call-template>

				<xsl:call-template name ="CopyPrepaidCollectProperty">
					<xsl:with-param name="source" select="'DestinationHaulagePrepaid'"/>
					<xsl:with-param name="categoryCode" select="'DHC'" />
					<xsl:with-param name="categoryDesc" select="'Destination Haulage Charge'"/>
					<xsl:with-param name="paymentMethodCode" select="$PrepaidCode"/>
					<xsl:with-param name="paymentMethodDesc" select="$PrepaidDesc"/>
				</xsl:call-template>

				<xsl:call-template name ="CopyPrepaidCollectProperty">
					<xsl:with-param name="source" select="'DestinationHaulageCollect'"/>
					<xsl:with-param name="categoryCode" select="'DHC'" />
					<xsl:with-param name="categoryDesc" select="'Destination Haulage Charge'"/>
					<xsl:with-param name="paymentMethodCode" select="$CollectCode"/>
					<xsl:with-param name="paymentMethodDesc" select="$CollectDesc"/>
				</xsl:call-template>
			</xsl:variable>
			<xsl:if test="$paymentHandlingInstructionItems != ''">
				<Property Name="PaymentHandlingInstructionCollection">
					<EntityCollection>
						<Items>
							<xsl:copy-of select="$paymentHandlingInstructionItems"/>
						</Items>
					</EntityCollection>
				</Property>
			</xsl:if>

			<xsl:call-template name ="CopyMappingProperty">
				<xsl:with-param name="source" select="'NoOfOriginalsOrCopies'" />
				<xsl:with-param name="target" select="'NoOriginalBills'" />
			</xsl:call-template>

			<xsl:call-template name ="CopyMappingProperty">
				<xsl:with-param name="source" select="'NoOfOriginalsOrCopies'" />
				<xsl:with-param name="target" select="'NoCopyBills'" />
			</xsl:call-template>

			<xsl:call-template name ="CopyMappingProperty">
				<xsl:with-param name="source" select="'InstructionGoodsValue'" />
				<xsl:with-param name="target" select="'GoodsValue'" />
			</xsl:call-template>

			<xsl:call-template name ="CopyUnLocoProperty">
				<xsl:with-param name="source" select="'PlaceOfReceipt'" />
				<xsl:with-param name="target" select="'PlaceOfReceipt'" />
			</xsl:call-template>

			<xsl:call-template name ="CopyUnLocoProperty">
				<xsl:with-param name="source" select="'PlaceOfDelivery'" />
				<xsl:with-param name="target" select="'PlaceOfDelivery'" />
			</xsl:call-template>

			<xsl:call-template name ="CopyUnLocoProperty">
				<xsl:with-param name="source" select="'Origin'" />
				<xsl:with-param name="target" select="'PortOfOrigin'" />
			</xsl:call-template>

			<xsl:call-template name ="CopyUnLocoProperty">
				<xsl:with-param name="source" select="'Destination'" />
				<xsl:with-param name="target" select="'PortOfDestination'" />
			</xsl:call-template>

			<xsl:call-template name ="CopyCodeProperty">
				<xsl:with-param name="source" select="'RequestedPlaceOfIssue'" />
				<xsl:with-param name="target" select="'PlaceOfIssue'" />
			</xsl:call-template>

			<xsl:call-template name ="CopyCodeProperty">
				<xsl:with-param name="source" select="'BOLCurrency'" />
				<xsl:with-param name="target" select="'GoodsValueCurrency'" />
			</xsl:call-template>

			<xsl:variable name="transportLegCollection" select="/Entity/Property[@Name='TransportLegCollection']" />
			<xsl:if test="$transportLegCollection">
				<Property Name="TransportLegCollection">
					<EntityCollection>
						<Items>
							<Entity>
								<Property Name="LegType" NaturalKey="true">
									<Value>Main</Value>
								</Property>

								<xsl:variable name="mainTransport" select="$transportLegCollection/EntityCollection/Items/Entity" />
								<xsl:call-template name ="CopyId">
									<xsl:with-param name="idNode" select="$mainTransport/Id" />
								</xsl:call-template>
								<xsl:call-template name="CopySimpleProperty">
									<xsl:with-param name="parentNode" select="$mainTransport"/>
									<xsl:with-param name="elementName" select="'PortOfLoading'"/>
								</xsl:call-template>
								<xsl:call-template name="CopySimpleProperty">
									<xsl:with-param name="parentNode" select="$mainTransport"/>
									<xsl:with-param name="elementName" select="'PortOfDischarge'"/>
								</xsl:call-template>
								<xsl:call-template name="CopySimpleProperty">
									<xsl:with-param name="parentNode" select="$mainTransport"/>
									<xsl:with-param name="elementName" select="'VesselName'"/>
								</xsl:call-template>
								<xsl:call-template name="CopySimpleProperty">
									<xsl:with-param name="parentNode" select="$mainTransport"/>
									<xsl:with-param name="elementName" select="'VoyageFlightNo'"/>
								</xsl:call-template>
							</Entity>
						</Items>
					</EntityCollection>
				</Property>
			</xsl:if>

			<xsl:variable name="masterEntities" select="/Entity/Property[@Name='ContainerCollection']/EntityCollection/Items/Entity" />
			<xsl:if test="$masterEntities">
				<Property Name="ContainerCollection">
					<EntityCollection>
						<Items>
							<xsl:for-each select="$masterEntities">
								<Entity>
									<xsl:call-template name ="CopyId">
										<xsl:with-param name="idNode" select="./Id" />
									</xsl:call-template>

									<xsl:variable name="containerType" select="./Property[@Name='ContainerType']" />
									<xsl:if test="$containerType">
										<xsl:copy-of select="$containerType"/>
									</xsl:if>
								</Entity>
							</xsl:for-each>
						</Items>
					</EntityCollection>
				</Property>
			</xsl:if>

			<xsl:variable name="carrierPickupAtDoor" select="/Entity/Property[@Name='CarrierPickupAtDoor' and @State='Added']"/>
			<xsl:variable name="carrierDeliveryAtDoor" select="/Entity/Property[@Name='CarrierDeliveryAtDoor' and @State='Added']"/>
			<xsl:if test="$carrierPickupAtDoor or $carrierDeliveryAtDoor">

				<xsl:variable name="pickupAtDoor">
					<xsl:choose>
						<xsl:when test ="$carrierPickupAtDoor = 'Y'">Door</xsl:when>
						<xsl:otherwise>Peer</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:variable name="deliveryAtDoor">
					<xsl:choose>
						<xsl:when test ="$carrierDeliveryAtDoor = 'Y'">Door</xsl:when>
						<xsl:otherwise>Pier</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>

				<Property Name="DeliveryMode">
					<Entity>
						<Property Name="Code">
							<Value>
								<xsl:value-of select="concat(substring($pickupAtDoor, 1, 1), 'T', substring($deliveryAtDoor, 1, 1))" />
							</Value>
						</Property>
						<Property Name="Description">
							<Value>
								<xsl:value-of select="concat($pickupAtDoor, ' To ', $deliveryAtDoor)" />
							</Value>
						</Property>
					</Entity>
				</Property>

			</xsl:if>

			<xsl:variable name="dateOfIssue" select="/Entity/Property[@Name='RequestedDateOfIssue' and @State='Added']"/>

			<xsl:if test="$dateOfIssue">
				<Property Name="DateCollection">
					<EntityCollection>
						<Items>
							<Entity>
								<Property Name="Type" NaturalKey="true">
									<Value>BillIssued</Value>
								</Property>
								<Property Name="Value">
									<Value>
										<xsl:value-of select="$dateOfIssue" />
									</Value>
								</Property>
							</Entity>
						</Items>
					</EntityCollection>
				</Property>
			</xsl:if>

			<xsl:variable name="subShipments">

				<xsl:variable name="subShipmentEntities" select="/Entity/Property[@Name='SubShipmentCollection']/EntityCollection/Items/Entity" />
				<xsl:if test="$subShipmentEntities">
					<xsl:for-each select="$subShipmentEntities">

						<xsl:variable name="currentEntity" select="." />

						<Entity>

							<xsl:call-template name ="CopyId">
								<xsl:with-param name="idNode" select="$currentEntity/Id" />
							</xsl:call-template>

							<xsl:call-template name ="CopyMappingProperty">
								<xsl:with-param name="source" select="'NoOfOriginalsOrCopies'" />
								<xsl:with-param name="target" select="'NoOriginalBills'" />
							</xsl:call-template>

							<xsl:call-template name ="CopyMappingProperty">
								<xsl:with-param name="source" select="'NoOfOriginalsOrCopies'" />
								<xsl:with-param name="target" select="'NoCopyBills'" />
							</xsl:call-template>

							<xsl:variable name="specialInstructionNote" select="/Entity/Property[@Name='SpecialInstruction' and @State='Added']" />

							<xsl:if test="$specialInstructionNote">
								<Property Name="NoteCollection">
									<EntityCollection>
										<Items>
											<xsl:call-template name ="CopyNoteEntity">
												<xsl:with-param name="sourceName" select="'SpecialInstruction'" />
												<xsl:with-param name="descValue" select="'Special Instructions'" />
											</xsl:call-template>
										</Items>
									</EntityCollection>
								</Property>
							</xsl:if>

							<xsl:variable name="packingLineEntities" select="$currentEntity/Property[@Name='PackingLineCollection']/EntityCollection/Items/Entity"/>
							<xsl:call-template name="CopyPackLines">
								<xsl:with-param name="packingLineEntities" select="$packingLineEntities" />
							</xsl:call-template>

							<xsl:variable name="childShipments">
								<xsl:variable name="childSubShipmentEntities" select="$currentEntity/Property[@Name='SubShipmentCollection']/EntityCollection/Items/Entity"/>
								<xsl:if test="$childSubShipmentEntities">
									<xsl:for-each select="$childSubShipmentEntities">

										<xsl:variable name="idAndPacklines">
											<xsl:call-template name ="CopyId">
												<xsl:with-param name="idNode" select="./Id" />
											</xsl:call-template>

											<xsl:variable name="childPackingLineEntities" select="./Property[@Name='PackingLineCollection']/EntityCollection/Items/Entity" />

											<xsl:if test="$childPackingLineEntities">
												<xsl:call-template name="CopyPackLines">
													<xsl:with-param name="packingLineEntities" select="$childPackingLineEntities" />
												</xsl:call-template>
											</xsl:if>
										</xsl:variable>

										<xsl:if test="$idAndPacklines">
											<Entity>
												<xsl:copy-of select="$idAndPacklines"/>
											</Entity>
										</xsl:if>

									</xsl:for-each>

								</xsl:if>

							</xsl:variable>

							<xsl:if test="$childShipments != ''">
								<Property Name="SubShipmentCollection">
									<EntityCollection>
										<Items>
											<xsl:copy-of select="$childShipments"/>
										</Items>
									</EntityCollection>
								</Property>
							</xsl:if>

							<xsl:variable name ="subShipmentOrgAdrItems">

								<xsl:variable name="shpOrgAdrItems" select="$currentEntity/Property[@Name='OrganizationAddressCollection']/EntityCollection/Items" />

								<xsl:variable name="cneAddress">
									<xsl:call-template name ="CopyId">
										<xsl:with-param name="idNode" select="$shpOrgAdrItems/Entity/Property[@Name='AddressType' and Value='ConsigneeDocumentaryAddress']/parent::*/Id" />
									</xsl:call-template>
									<xsl:call-template name="CopySimpleAddress">
										<xsl:with-param name="parentNode" select="$shpOrgAdrItems" />
										<xsl:with-param name="source" select="'ConsigneeDocumentaryAddress'" />
									</xsl:call-template>
									<xsl:call-template name="CopyAddressFromCustomFields">
										<xsl:with-param name="sourcePrefixName" select="'Consignee_'" />
									</xsl:call-template>
								</xsl:variable>

								<xsl:if test="$cneAddress != ''">
									<Entity State="Added">
										<Property Name="AddressType" NaturalKey="true">
											<Value>ConsigneeDocumentaryAddress</Value>
										</Property>
										<xsl:copy-of select="$cneAddress"/>
									</Entity>
								</xsl:if>

								<xsl:variable name="cnrAddress">
									<xsl:call-template name ="CopyId">
										<xsl:with-param name="idNode" select="$shpOrgAdrItems/Entity/Property[@Name='AddressType' and Value='ConsignorDocumentaryAddress']/parent::*/Id" />
									</xsl:call-template>
									<xsl:call-template name="CopySimpleAddress">
										<xsl:with-param name="parentNode" select="$shpOrgAdrItems" />
										<xsl:with-param name="source" select="'ConsignorDocumentaryAddress'" />
									</xsl:call-template>
									<xsl:call-template name="CopyAddressFromCustomFields">
										<xsl:with-param name="sourcePrefixName" select="'Shipper_'" />
									</xsl:call-template>
								</xsl:variable>

								<xsl:if test="$cnrAddress != ''">
									<Entity State="Added">
										<Property Name="AddressType" NaturalKey="true">
											<Value>ConsignorDocumentaryAddress</Value>
										</Property>
										<xsl:copy-of select="$cnrAddress"/>
									</Entity>
								</xsl:if>

								<xsl:variable name="cnePickDeliveryAdr">
									<xsl:call-template name="CopyAddressFromCustomFields">
										<xsl:with-param name="sourcePrefixName" select="'ShipToParty_'" />
									</xsl:call-template>
								</xsl:variable>

								<xsl:if test="$cnePickDeliveryAdr != ''">
									<Entity State="Added">
										<Property Name="AddressType" NaturalKey="true">
											<Value>ConsigneePickupDeliveryAddress</Value>
										</Property>
										<xsl:copy-of select="$cnePickDeliveryAdr"/>
									</Entity>
								</xsl:if>

								<xsl:variable name="cnrPickDeliveryAdr">
									<xsl:call-template name="CopyAddressFromCustomFields">
										<xsl:with-param name="sourcePrefixName" select="'CarrierPickupAddress_'" />
									</xsl:call-template>
								</xsl:variable>

								<xsl:if test="$cnrPickDeliveryAdr != ''">
									<Entity State="Added">
										<Property Name="AddressType" NaturalKey="true">
											<Value>ConsignorPickupDeliveryAddress</Value>
										</Property>
										<xsl:copy-of select="$cnrPickDeliveryAdr"/>
									</Entity>
								</xsl:if>

								<xsl:variable name="notifyParty">
									<xsl:call-template name ="CopyId">
										<xsl:with-param name="idNode" select="$shpOrgAdrItems/Entity/Property[@Name='AddressType' and Value='NotifyParty']/parent::*/Id" />
									</xsl:call-template>
									<xsl:call-template name="CopySimpleAddress">
										<xsl:with-param name="parentNode" select="$shpOrgAdrItems" />
										<xsl:with-param name="source" select="'NotifyParty'" />
									</xsl:call-template>
								</xsl:variable>

								<xsl:if test="$notifyParty != ''">
									<Entity State="Added">
										<Property Name="AddressType" NaturalKey="true">
											<Value>NotifyParty</Value>
										</Property>
										<xsl:copy-of select="$notifyParty"/>
									</Entity>
								</xsl:if>

								<xsl:variable name="notifyParty2">
									<xsl:call-template name ="CopyId">
										<xsl:with-param name="idNode" select="$shpOrgAdrItems/Entity/Property[@Name='AddressType' and Value='NotifyParty2']/parent::*/Id" />
									</xsl:call-template>
									<xsl:call-template name="CopySimpleAddress">
										<xsl:with-param name="parentNode" select="$shpOrgAdrItems" />
										<xsl:with-param name="source" select="'NotifyParty2'" />
									</xsl:call-template>
								</xsl:variable>

								<xsl:if test="$notifyParty2 != ''">
									<Entity State="Added">
										<Property Name="AddressType" NaturalKey="true">
											<Value>NotifyParty2</Value>
										</Property>
										<xsl:copy-of select="$notifyParty2"/>
									</Entity>
								</xsl:if>

								<xsl:variable name="notifyParty3">
									<xsl:call-template name ="CopyId">
										<xsl:with-param name="idNode" select="$shpOrgAdrItems/Entity/Property[@Name='AddressType' and Value='NotifyParty3']/parent::*/Id" />
									</xsl:call-template>
									<xsl:call-template name="CopySimpleAddress">
										<xsl:with-param name="parentNode" select="$shpOrgAdrItems" />
										<xsl:with-param name="source" select="'NotifyParty3'" />
									</xsl:call-template>
								</xsl:variable>

								<xsl:if test="$notifyParty3 != ''">
									<Entity State="Added">
										<Property Name="AddressType" NaturalKey="true">
											<Value>NotifyParty3</Value>
										</Property>
										<xsl:copy-of select="$notifyParty3"/>
									</Entity>
								</xsl:if>

							</xsl:variable>

							<xsl:if test="$subShipmentOrgAdrItems != ''">
								<Property Name="OrganizationAddressCollection">
									<EntityCollection>
										<Items>
											<xsl:copy-of select="$subShipmentOrgAdrItems"/>
										</Items>
									</EntityCollection>
								</Property>
							</xsl:if>

							<xsl:variable name="entryNumberCollection" select="$currentEntity/Property[@Name='EntryNumberCollection']" />

							<xsl:if test="$entryNumberCollection">
								<xsl:copy-of select="$entryNumberCollection"/>
							</xsl:if>

						</Entity>

					</xsl:for-each>
				</xsl:if>
			</xsl:variable>

			<xsl:if test="$subShipments != ''">
				<Property Name="SubShipmentCollection">
					<EntityCollection>
						<Items>
							<xsl:copy-of select="$subShipments"/>
						</Items>
					</EntityCollection>
				</Property>
			</xsl:if>

			<xsl:variable name="orgAddressItems" select="/Entity/Property[@Name='OrganizationAddressCollection']/EntityCollection/Items" />

			<xsl:variable name="orgAdrItems">

				<xsl:variable name="shippingLineEntity" select="$orgAddressItems/Entity/Property[@Name='AddressType' and Value='ShippingLineAddress']/parent::*" />

				<xsl:variable name="shippingLineAdr">
					<xsl:call-template name ="CopyId">
						<xsl:with-param name="idNode" select="$shippingLineEntity/Id" />
					</xsl:call-template>
					<xsl:call-template name="CopySimpleAddress">
						<xsl:with-param name="parentNode" select="$orgAddressItems" />
						<xsl:with-param name="source" select="'ShippingLineAddress'" />
					</xsl:call-template>
					<xsl:call-template name="CopyAddressFromCustomFields">
						<xsl:with-param name="sourcePrefixName" select="'Carrier_'" />
					</xsl:call-template>
				</xsl:variable>

				<xsl:variable name="shippingLinePort" select="$shippingLineEntity/Property[@Name='Port']"/>

				<xsl:if test="$shippingLineAdr != ''">
					<Entity State="Added">
						<Property Name="AddressType" NaturalKey="true">
							<Value>ShippingLineAddress</Value>
						</Property>
						<xsl:copy-of select="$shippingLineAdr"/>

						<xsl:if test="$shippingLinePort != ''">
							<xsl:copy-of select="$shippingLinePort"/>
						</xsl:if>
					</Entity>
				</xsl:if>

				<xsl:variable name="sendingForwarderAdr">
					<xsl:call-template name ="CopyId">
						<xsl:with-param name="idNode" select="$orgAddressItems/Entity/Property[@Name='AddressType' and Value='ConsignorDocumentaryAddress']/parent::*/Id" />
					</xsl:call-template>
					<xsl:call-template name="CopySimpleAddress">
						<xsl:with-param name="parentNode" select="$orgAddressItems" />
						<xsl:with-param name="source" select="'SendingForwarderAddress'" />
					</xsl:call-template>
					<xsl:call-template name="CopyAddressFromCustomFields">
						<xsl:with-param name="sourcePrefixName" select="'Shipper_'" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:if test="$sendingForwarderAdr != ''">
					<Entity State="Added">
						<Property Name="AddressType" NaturalKey="true">
							<Value>SendingForwarderAddress</Value>
						</Property>
						<xsl:copy-of select="$sendingForwarderAdr"/>
					</Entity>
				</xsl:if>

				<xsl:variable name="receivingForwarderAdr">
					<xsl:call-template name ="CopyId">
						<xsl:with-param name="idNode" select="$orgAddressItems/Entity/Property[@Name='AddressType' and Value='ConsigneeDocumentaryAddress']/parent::*/Id" />
					</xsl:call-template>
					<xsl:call-template name="CopySimpleAddress">
						<xsl:with-param name="parentNode" select="$orgAddressItems" />
						<xsl:with-param name="source" select="'ReceivingForwarderAddress'" />
					</xsl:call-template>
					<xsl:call-template name="CopyAddressFromCustomFields">
						<xsl:with-param name="sourcePrefixName" select="'Consignee_'" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:if test="$receivingForwarderAdr != ''">
					<Entity State="Added">
						<Property Name="AddressType" NaturalKey="true">
							<Value>ReceivingForwarderAddress</Value>
						</Property>
						<xsl:copy-of select="$receivingForwarderAdr"/>
					</Entity>
				</xsl:if>

				<xsl:variable name="arrivalCFSAdr">
					<xsl:call-template name="CopyAddressFromCustomFields">
						<xsl:with-param name="sourcePrefixName" select="'ShipToParty_'" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:if test="$arrivalCFSAdr != ''">
					<Entity State="Added">
						<Property Name="AddressType" NaturalKey="true">
							<Value>ArrivalCFSAddress</Value>
						</Property>
						<xsl:copy-of select="$arrivalCFSAdr"/>
					</Entity>
				</xsl:if>

				<xsl:variable name="departureCFSAdr">
					<xsl:call-template name="CopyAddressFromCustomFields">
						<xsl:with-param name="sourcePrefixName" select="'CarrierPickupAddress_'" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:if test="$departureCFSAdr != ''">
					<Entity State="Added">
						<Property Name="AddressType" NaturalKey="true">
							<Value>DepartureCFSAddress</Value>
						</Property>
						<xsl:copy-of select="$departureCFSAdr"/>
					</Entity>
				</xsl:if>

				<xsl:variable name="forwarder">
					<xsl:call-template name="CopyAddressFromCustomFields">
						<xsl:with-param name="sourcePrefixName" select="'Forwarder_'" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:if test="$forwarder != ''">
					<Entity State="Added">
						<Property Name="AddressType" NaturalKey="true">
							<Value>Forwarder</Value>
						</Property>
						<xsl:copy-of select="$forwarder"/>
					</Entity>
				</xsl:if>

				<xsl:variable name="freightPayer">
					<xsl:call-template name="CopyAddressFromCustomFields">
						<xsl:with-param name="sourcePrefixName" select="'FreightPayer_'" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:if test="$freightPayer != ''">
					<Entity State="Added">
						<Property Name="AddressType" NaturalKey="true">
							<Value>FreightPayer</Value>
						</Property>
						<xsl:copy-of select="$freightPayer"/>
					</Entity>
				</xsl:if>

				<xsl:variable name="notifyParty">
					<xsl:call-template name ="CopyId">
						<xsl:with-param name="idNode" select="$orgAddressItems/Entity/Property[@Name='AddressType' and Value='NotifyParty']/parent::*/Id" />
					</xsl:call-template>
					<xsl:call-template name="CopySimpleAddress">
						<xsl:with-param name="parentNode" select="$orgAddressItems" />
						<xsl:with-param name="source" select="'NotifyParty'" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:if test="$notifyParty != ''">
					<Entity State="Added">
						<Property Name="AddressType" NaturalKey="true">
							<Value>NotifyParty</Value>
						</Property>
						<xsl:copy-of select="$notifyParty"/>
					</Entity>
				</xsl:if>

				<xsl:variable name="notifyParty2">
					<xsl:call-template name ="CopyId">
						<xsl:with-param name="idNode" select="$orgAddressItems/Entity/Property[@Name='AddressType' and Value='NotifyParty2']/parent::*/Id" />
					</xsl:call-template>
					<xsl:call-template name="CopySimpleAddress">
						<xsl:with-param name="parentNode" select="$orgAddressItems" />
						<xsl:with-param name="source" select="'NotifyParty2'" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:if test="$notifyParty2 != ''">
					<Entity State="Added">
						<Property Name="AddressType" NaturalKey="true">
							<Value>NotifyParty2</Value>
						</Property>
						<xsl:copy-of select="$notifyParty2"/>
					</Entity>
				</xsl:if>

				<xsl:variable name="notifyParty3">
					<xsl:call-template name ="CopyId">
						<xsl:with-param name="idNode" select="$orgAddressItems/Entity/Property[@Name='AddressType' and Value='NotifyParty3']/parent::*/Id" />
					</xsl:call-template>
					<xsl:call-template name="CopySimpleAddress">
						<xsl:with-param name="parentNode" select="$orgAddressItems" />
						<xsl:with-param name="source" select="'NotifyParty3'" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:if test="$notifyParty3 != ''">
					<Entity State="Added">
						<Property Name="AddressType" NaturalKey="true">
							<Value>NotifyParty3</Value>
						</Property>
						<xsl:copy-of select="$notifyParty3"/>
					</Entity>
				</xsl:if>

				<xsl:variable name="buyerAddress">
					<xsl:call-template name="CopyAddressFromCustomFields">
						<xsl:with-param name="sourcePrefixName" select="'GoodsOwnerParty_'" />
					</xsl:call-template>
				</xsl:variable>
				<xsl:if test="$buyerAddress != ''">
					<Entity State="Added">
						<Property Name="AddressType" NaturalKey="true">
							<Value>BuyerDocumentaryAddress</Value>
						</Property>
						<xsl:copy-of select="$buyerAddress"/>
					</Entity>
				</xsl:if>

			</xsl:variable>
			<xsl:if test="$orgAdrItems != ''">
				<Property Name="OrganizationAddressCollection">
					<EntityCollection>
						<Items>
							<xsl:copy-of select="$orgAdrItems"/>
						</Items>
					</EntityCollection>
				</Property>
			</xsl:if>

			<xsl:variable name="noteItems">
				<xsl:call-template name ="CopyNoteEntity">
					<xsl:with-param name="sourceName" select="'OtherBillClauses'" />
					<xsl:with-param name="descValue" select="'OtherBillClauses'" />
				</xsl:call-template>
				<xsl:call-template name ="CopyNoteEntity">
					<xsl:with-param name="sourceName" select="'ForwardingInstructionNotes'" />
					<xsl:with-param name="descValue" select="'Forwarding Instruction Notes'" />
				</xsl:call-template>
				<xsl:call-template name ="CopyNoteEntity">
					<xsl:with-param name="sourceName" select="'SpecialInstruction'" />
					<xsl:with-param name="descValue" select="'Special Instructions'" />
				</xsl:call-template>
				<xsl:call-template name ="CopyNoteEntity">
					<xsl:with-param name="sourceName" select="'USCanadaManifestSelfFilerID'" />
					<xsl:with-param name="descValue" select="'USCanadaManifestSelfFilerID'" />
				</xsl:call-template>
				<xsl:call-template name ="CopyNoteEntity">
					<xsl:with-param name="sourceName" select="'ChargesFreighted'" />
					<xsl:with-param name="descValue" select="'ChargesFreighted'" />
				</xsl:call-template>
				<xsl:call-template name ="CopyNoteEntity">
					<xsl:with-param name="sourceName" select="'GoodsHandlingInstruction'" />
					<xsl:with-param name="descValue" select="'Goods Handling Instructions'" />
				</xsl:call-template>
				<xsl:call-template name ="CopyNoteEntity">
					<xsl:with-param name="sourceName" select="'WoodenPackage'" />
					<xsl:with-param name="descValue" select="'WoodenPackageProcessType'" />
				</xsl:call-template>
			</xsl:variable>
			<xsl:if test="$noteItems != ''">
				<Property Name="NoteCollection">
					<EntityCollection>
						<Items>
							<xsl:copy-of select="$noteItems"/>
						</Items>
					</EntityCollection>
				</Property>
			</xsl:if>

			<xsl:variable name="billOfLadingClauseItems">
				<xsl:call-template name ="CopyClauseEntity">
					<xsl:with-param name="sourceName" select="'FreightPrepaid'" />
					<xsl:with-param name="codeValue" select="'FPP'" />
					<xsl:with-param name="descValue" select="'Freight Prepaidt'" />
				</xsl:call-template>
				<xsl:call-template name ="CopyClauseEntity">
					<xsl:with-param name="sourceName" select="'FreightCollect'" />
					<xsl:with-param name="codeValue" select="'FCL'" />
					<xsl:with-param name="descValue" select="'Freight Collect'" />
				</xsl:call-template>
				<xsl:call-template name ="CopyClauseEntity">
					<xsl:with-param name="sourceName" select="'FreightAsAgreed'" />
					<xsl:with-param name="codeValue" select="'FAA'" />
					<xsl:with-param name="descValue" select="'Freight As Agreed'" />
				</xsl:call-template>
				<xsl:call-template name ="CopyClauseEntity">
					<xsl:with-param name="sourceName" select="'ReceivedForShipment'" />
					<xsl:with-param name="codeValue" select="'RFS'" />
					<xsl:with-param name="descValue" select="'Received for Shipment'" />
				</xsl:call-template>
				<xsl:call-template name ="CopyClauseEntity">
					<xsl:with-param name="sourceName" select="'OnBoard_Rail'" />
					<xsl:with-param name="codeValue" select="'OBR'" />
					<xsl:with-param name="descValue" select="'On Board Rail'" />
				</xsl:call-template>
				<xsl:call-template name ="CopyClauseEntity">
					<xsl:with-param name="sourceName" select="'OnBoard_Vessel'" />
					<xsl:with-param name="codeValue" select="'OBV'" />
					<xsl:with-param name="descValue" select="'On Board Vessel'" />
				</xsl:call-template>
				<xsl:call-template name ="CopyClauseEntity">
					<xsl:with-param name="sourceName" select="'LadenOnBoard'" />
					<xsl:with-param name="codeValue" select="'LOB'" />
					<xsl:with-param name="descValue" select="'Laden on Board'" />
				</xsl:call-template>
				<xsl:call-template name ="CopyClauseEntity">
					<xsl:with-param name="sourceName" select="'LadenOnBoardVessel'" />
					<xsl:with-param name="codeValue" select="'LBV'" />
					<xsl:with-param name="descValue" select="'Laden on Board Vessel'" />
				</xsl:call-template>
				<xsl:call-template name ="CopyClauseEntity">
					<xsl:with-param name="sourceName" select="'LadenOnBoardNamedVessel'" />
					<xsl:with-param name="codeValue" select="'LNV'" />
					<xsl:with-param name="descValue" select="'Laden on Board Named Vessel'" />
				</xsl:call-template>
				<xsl:call-template name ="CopyClauseEntity">
					<xsl:with-param name="sourceName" select="'ShippersLoadAndCount'" />
					<xsl:with-param name="codeValue" select="'SLC'" />
					<xsl:with-param name="descValue">Shipper's Load and Count</xsl:with-param>
				</xsl:call-template>
				<xsl:call-template name ="CopyClauseEntity">
					<xsl:with-param name="sourceName" select="'ShippersLoadStowageAndCount'" />
					<xsl:with-param name="codeValue" select="'LSC'" />
					<xsl:with-param name="descValue">Shipper's Load, Stowage and Count</xsl:with-param>
				</xsl:call-template>
				<xsl:call-template name ="CopyClauseEntity">
					<xsl:with-param name="sourceName" select="'NoShippersExportDeclarationRequired'" />
					<xsl:with-param name="codeValue" select="'NSD'" />
					<xsl:with-param name="descValue">No Shipper's Export Declaration Required</xsl:with-param>
				</xsl:call-template>
			</xsl:variable>
			<xsl:if test="$billOfLadingClauseItems != ''">
				<Property Name="BillOfLadingClauseCollection">
					<EntityCollection>
						<Items>
							<xsl:copy-of select="$billOfLadingClauseItems"/>
						</Items>
					</EntityCollection>
				</Property>
			</xsl:if>

		</Entity>
	</xsl:template>

	<xsl:template name="CopyId">
		<xsl:param name="idNode" />
		<xsl:if test="$idNode">
			<Id>
				<xsl:value-of select="$idNode" />
			</Id>
		</xsl:if>
	</xsl:template>

	<xsl:template name="CopySimpleProperty">
		<xsl:param name="parentNode" />
		<xsl:param name="elementName" />
		<xsl:variable name="source" select="$parentNode/Property[@Name=$elementName]" />
		<xsl:if test="$source">
			<xsl:copy-of select="$source"/>
		</xsl:if>
	</xsl:template>

	<xsl:template name="CopyCodeProperty">
		<xsl:param name="source" />
		<xsl:param name="target" />
		<xsl:variable name="sourceProperty" select="/Entity/Property[@Name=$source and @State='Added']"/>
		<xsl:if test="$sourceProperty">
			<Property>
				<xsl:attribute name="Name">
					<xsl:value-of select="$target" />
				</xsl:attribute>
				<Entity>
					<Property Name="Code">
						<Value>
							<xsl:value-of select="$sourceProperty/Value" />
						</Value>
					</Property>
				</Entity>
			</Property>
		</xsl:if>
	</xsl:template>

	<xsl:template name="CopyNoteEntity">
		<xsl:param name="sourceName" />
		<xsl:param name="descValue" />
		<xsl:variable name="sourceProperty" select="/Entity/Property[@Name=$sourceName and @State='Added']" />
		<xsl:if test="$sourceProperty">
			<Entity>
				<Property Name="Description" NaturalKey="true">
					<Value>
						<xsl:value-of select="$descValue" />
					</Value>
				</Property>
				<Property Name="NoteText">
					<Value>
						<xsl:value-of select="$sourceProperty/Value" />
					</Value>
				</Property>
			</Entity>
		</xsl:if>
	</xsl:template>

	<xsl:template name="CopyClauseEntity">
		<xsl:param name="sourceName" />
		<xsl:param name="codeValue" />
		<xsl:param name="descValue" />
		<xsl:variable name="sourseProperty" select="/Entity/Property[@Name=$sourceName]" />
		<xsl:if test="$sourseProperty">
			<xsl:choose>
				<xsl:when test="$sourseProperty/Value/text() = 'Y'">
					<Entity State="Added">
						<Property Name="Type">
							<Entity>
								<Property Name="Code" NaturalKey="true">
									<Value>
										<xsl:value-of select="$codeValue"/>
									</Value>
								</Property>
								<Property Name="Description">
									<Value>
										<xsl:value-of select="$descValue"/>
									</Value>
								</Property>
							</Entity>
						</Property>
					</Entity>
				</xsl:when>
				<xsl:otherwise>
					<Entity State="Removed">
						<Property Name="Type">
							<Entity>
								<Property Name="Code" NaturalKey="true">
									<Value>
										<xsl:value-of select="$codeValue"/>
									</Value>
								</Property>
							</Entity>
						</Property>
					</Entity>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

	<xsl:template name="CopyMappingProperty">
		<xsl:param name="source" />
		<xsl:param name="target" />
		<xsl:variable name="property" select="/Entity/Property[@Name=$source]" />
		<xsl:if test="$property">
			<Property>
				<xsl:attribute name="Name">
					<xsl:value-of select="$target" />
				</xsl:attribute>
				<Value>
					<xsl:value-of select="$property/Value" />
				</Value>
			</Property>
		</xsl:if>
	</xsl:template>

	<xsl:template name="CopyUnLocoProperty">
		<xsl:param name="source" />
		<xsl:param name="target" />

		<xsl:variable name="codeAtr" select="concat($source, '_Code')" />
		<xsl:variable name="nameAtr" select="concat($source, '_Name')" />

		<xsl:variable name="codeProperty" select="/Entity/Property[@Name=$codeAtr]" />
		<xsl:variable name="nameProperty" select="/Entity/Property[@Name=$nameAtr]" />
		<xsl:if test="$codeProperty or $nameProperty">
			<Property>
				<xsl:attribute name="Name">
					<xsl:value-of select="$target" />
				</xsl:attribute>
				<Entity>
					<xsl:if test="$codeProperty">
						<Property Name="Code">
							<Value>
								<xsl:value-of select="$codeProperty/Value"/>
							</Value>
						</Property>
					</xsl:if>
					<xsl:if test="$nameProperty">
						<Property Name="Name">
							<Value>
								<xsl:value-of select="$nameProperty/Value"/>
							</Value>
						</Property>
					</xsl:if>
				</Entity>
			</Property>
		</xsl:if>
	</xsl:template>

	<xsl:template name="CopySimpleAddress">
		<xsl:param name="source"/>
		<xsl:param name="parentNode" />
		<xsl:variable name="property" select="$parentNode/Entity/Property[@Name='AddressType' and Value=$source]" />
		<xsl:if test="$property">
			<xsl:variable name="sourceOrgAddress" select="$property/.." />
			<xsl:call-template name="CopySimpleProperty">
				<xsl:with-param name="parentNode" select="$sourceOrgAddress" />
				<xsl:with-param name="elementName" select="'Address1'" />
			</xsl:call-template>
			<xsl:call-template name="CopySimpleProperty">
				<xsl:with-param name="parentNode" select="$sourceOrgAddress" />
				<xsl:with-param name="elementName" select="'Address2'" />
			</xsl:call-template>
			<xsl:call-template name="CopySimpleProperty">
				<xsl:with-param name="parentNode" select="$sourceOrgAddress" />
				<xsl:with-param name="elementName" select="'City'" />
			</xsl:call-template>
			<xsl:call-template name="CopySimpleProperty">
				<xsl:with-param name="parentNode" select="$sourceOrgAddress" />
				<xsl:with-param name="elementName" select="'CompanyName'" />
			</xsl:call-template>
			<xsl:call-template name="CopySimpleProperty">
				<xsl:with-param name="parentNode" select="$sourceOrgAddress" />
				<xsl:with-param name="elementName" select="'Country'" />
			</xsl:call-template>
			<xsl:call-template name="CopySimpleProperty">
				<xsl:with-param name="parentNode" select="$sourceOrgAddress" />
				<xsl:with-param name="elementName" select="'Fax'" />
			</xsl:call-template>
			<xsl:call-template name="CopySimpleProperty">
				<xsl:with-param name="parentNode" select="$sourceOrgAddress" />
				<xsl:with-param name="elementName" select="'Postcode'" />
			</xsl:call-template>
			<xsl:call-template name="CopySimpleProperty">
				<xsl:with-param name="parentNode" select="$sourceOrgAddress" />
				<xsl:with-param name="elementName" select="'State'" />
			</xsl:call-template>
			<xsl:call-template name="CopySimpleProperty">
				<xsl:with-param name="parentNode" select="$sourceOrgAddress" />
				<xsl:with-param name="elementName" select="'Contact'" />
			</xsl:call-template>
			<xsl:call-template name="CopySimpleProperty">
				<xsl:with-param name="parentNode" select="$sourceOrgAddress" />
				<xsl:with-param name="elementName" select="'Phone'" />
			</xsl:call-template>
			<xsl:call-template name="CopySimpleProperty">
				<xsl:with-param name="parentNode" select="$sourceOrgAddress" />
				<xsl:with-param name="elementName" select="'Email'" />
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="CopyAddressFromCustomFields">
		<xsl:param name ="sourcePrefixName"/>
		<xsl:variable name="sourceFields" select="/Entity/Property[starts-with(@Name, $sourcePrefixName)]" />
		<xsl:if test="$sourceFields">
			<xsl:for-each select="$sourceFields">
				<xsl:variable name="propertyName" select="substring-after(@Name, '_')" />
				<xsl:if test="$propertyName">
					<xsl:choose>
						<xsl:when test="$propertyName != 'CountryName'">
							<xsl:call-template name="CopyMappingProperty">
								<xsl:with-param name="source" select="@Name" />
								<xsl:with-param name="target" select="$propertyName" />
							</xsl:call-template>
						</xsl:when>
						<xsl:otherwise>
							<Property Name="Country">
								<Entity>
									<Property Name="Name">
										<Value>
											<xsl:value-of select="."/>
										</Value>
									</Property>
								</Entity>
							</Property>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>

	<xsl:template name="CopyPrepaidCollectProperty">
		<xsl:param name="source" />
		<xsl:param name="categoryCode" />
		<xsl:param name="categoryDesc" />
		<xsl:param name="paymentMethodCode" />
		<xsl:param name="paymentMethodDesc" />
		<xsl:variable name="property" select="/Entity/Property[@Name=$source]" />
		<xsl:if test="$property">
			<xsl:choose>
				<xsl:when test="$property/Value/text() = 'Y'">
					<Entity State="Added">
						<Property Name="Category">
							<Entity>
								<Property Name="Code" NaturalKey="true">
									<Value>
										<xsl:value-of select="$categoryCode"/>
									</Value>
								</Property>
								<Property Name="Description">
									<Value>
										<xsl:value-of select="$categoryDesc"/>
									</Value>
								</Property>
							</Entity>
						</Property>
						<Property Name="PaymentMethod">
							<Entity>
								<Property Name="Code">
									<Value>
										<xsl:value-of select="$paymentMethodCode"/>
									</Value>
								</Property>
								<Property Name="Description">
									<Value>
										<xsl:value-of select="$paymentMethodDesc"/>
									</Value>
								</Property>
							</Entity>
						</Property>
					</Entity>
				</xsl:when>
				<xsl:otherwise>
					<Entity State="Removed">
						<Property Name="Category">
							<Entity>
								<Property Name="Code" NaturalKey="true">
									<Value>
										<xsl:value-of select="$categoryCode"/>
									</Value>
								</Property>
							</Entity>
						</Property>
					</Entity>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

	<xsl:template name="CopyPackLines">
		<xsl:param name="packingLineEntities"/>
		<xsl:if test="$packingLineEntities">
			<Property Name ="PackingLineCollection">
				<EntityCollection>
					<Items>
						<xsl:for-each select="$packingLineEntities">
							<Entity>
								<xsl:call-template name ="CopyId">
									<xsl:with-param name="idNode" select="./Id" />
								</xsl:call-template>
								<xsl:variable name="marksAndNOs" select="./Property[@Name='PackingLineMarksAndNos']"/>
								<xsl:if test="$marksAndNOs">
									<Property Name="MarksAndNos">
										<Value>
											<xsl:value-of select="$marksAndNOs"/>
										</Value>
									</Property>
								</xsl:if>
								<xsl:variable name="detailedDesc" select="./Property[@Name='PackingLineGoodsDescription']"/>
								<xsl:if test="$detailedDesc">
									<Property Name="DetailedDescription">
										<Value>
											<xsl:value-of select="$detailedDesc"/>
										</Value>
									</Property>
								</xsl:if>
							</Entity>
						</xsl:for-each>
					</Items>
				</EntityCollection>
			</Property>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>