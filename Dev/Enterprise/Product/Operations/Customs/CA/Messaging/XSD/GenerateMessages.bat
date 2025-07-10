echo off
echo Messages generator for CA Customs

echo CAD Outbound Message

xsd CAD/Outbound/Outbound.xsd /outputdir:CAD/Outbound/ /c /namespace:Enterprise.Customs.CA.Messaging.CAD.Outbound /language:CS

echo Done
