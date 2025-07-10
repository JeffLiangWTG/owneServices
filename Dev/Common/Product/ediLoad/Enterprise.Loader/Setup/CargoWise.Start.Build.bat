..\..\..\..\..\packages\WiX\tools\candle -fips -out CargoWiseSetup.wixobj CargoWiseSetup.wxs
..\..\..\..\..\packages\WiX\tools\light -out ..\..\..\..\..\bin\CargoWiseSetup.msi CargoWiseSetup.wixobj -ext WixNetFxExtension -cultures:en-US
