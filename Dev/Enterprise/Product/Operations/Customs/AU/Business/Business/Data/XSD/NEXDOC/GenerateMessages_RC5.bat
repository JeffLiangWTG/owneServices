echo off
echo Message generator for AU Customs NEXDOC

SETLOCAL
SET BaseDirectory=%CD%
SET ImportXsds=common/rex/CommonTypes.xsd common/rex/Fish.xsd common/rex/SkinsAndHides.xsd common/rex/Wool.xsd common/CommonTypes.xsd common/EnumTypes.xsd

CD "%BaseDirectory%"\RC5

echo Rex Ownership message
xsd %ImportXsds% RexOwnershipSoap.xsd /c /f /namespace:Enterprise.Customs.AU.Declaration.Business.NEXDOC.RC5.RexOwnership /language:CS
del RexOwnershipSoap.cs
ren EnumTypes_RexOwnershipSoap.cs RexOwnershipSoap.cs

echo Read Rex message
xsd %ImportXsds% ReadRexSoap.xsd /c /f /namespace:Enterprise.Customs.AU.Declaration.Business.NEXDOC.RC5.ReadRex /language:CS
del ReadRexSoap.cs
ren EnumTypes_ReadRexSoap.cs ReadRexSoap.cs

echo Done
