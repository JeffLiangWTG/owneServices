USE OceanMessagingDB


SELECT  CustomerCode,
       Customer,
	CASE WHEN 
      [XML Message].value (
'declare namespace PD="http://www.cargowise.com/Schemas/Universal/2011/11";(/PD:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:ShipmentType)[1]'
, 'nvarchar(max)') in ('CLD', 'GCL')
AND 
       [XML Message].value (
'declare namespace PD="http://www.cargowise.com/Schemas/Universal/2011/11";(/PD:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="CoLoadWith"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type/@Description="Standard Carrier Alpha Code" and *:CountryOfIssue="US"]/*:Value)[1]'
, 'nvarchar(max)') is NULL
THEN 
       [XML Message].value (
'declare namespace PD="http://www.cargowise.com/Schemas/Universal/2011/11";(/PD:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="CoLoadWith"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type/@Description="Standard Carrier Alpha Code (Sea)" and *:CountryOfIssue="US"]/*:Value)[1]'
, 'nvarchar(max)')
WHEN
      [XML Message].value (
'declare namespace PD="http://www.cargowise.com/Schemas/Universal/2011/11";(/PD:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:ShipmentType)[1]'
, 'nvarchar(max)') in ('CLD', 'GCL')
THEN
       [XML Message].value (
'declare namespace PD="http://www.cargowise.com/Schemas/Universal/2011/11";(/PD:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="CoLoadWith"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type/@Description="Standard Carrier Alpha Code" and *:CountryOfIssue="US"]/*:Value)[1]'
, 'nvarchar(max)')
WHEN
       [XML Message].value (
'declare namespace PD="http://www.cargowise.com/Schemas/Universal/2011/11";(/PD:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="ShippingLineAddress"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type/@Description="Standard Carrier Alpha Code" and *:CountryOfIssue="US"]/*:Value)[1]'
, 'nvarchar(max)') is NULL
THEN
       [XML Message].value (
'declare namespace PD="http://www.cargowise.com/Schemas/Universal/2011/11";(/PD:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="ShippingLineAddress"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type/@Description="Standard Carrier Alpha Code (Sea)" and *:CountryOfIssue="US"]/*:Value)[1]'
, 'nvarchar(max)')
ELSE
       [XML Message].value (
'declare namespace PD="http://www.cargowise.com/Schemas/Universal/2011/11";(/PD:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="ShippingLineAddress"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type/@Description="Standard Carrier Alpha Code" and *:CountryOfIssue="US"]/*:Value)[1]'
, 'nvarchar(max)')
END AS [SCAC],

[XML Message].value (
'declare namespace PD="http://www.cargowise.com/Schemas/Universal/2011/11";(/PD:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:ShipmentType)[1]'
, 'nvarchar(max)') AS [Shipment Type],
[XML Message].value (
'declare namespace PD="http://www.cargowise.com/Schemas/Universal/2011/11";(/PD:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:Workflow/*:Company/*:Country)[1]'
, 'nvarchar(max)')             AS CountryCode,
[XML Message].value (
'declare namespace PD="http://www.cargowise.com/Schemas/Universal/2011/11";(/PD:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]'
, 'nvarchar(max)')             AS TransactionType,
[Carriers],
AM_ReceivedFromSenderUTC AS [Date]
FROM   OceanMessaging WITH (nolock)
