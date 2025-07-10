..\..\..\..\..\packages\WiX\tools\candle -fips -out CargoWiseOneSetup.wixobj CargoWiseOneSetup.wxs
..\..\..\..\..\packages\WiX\tools\light -out ..\..\..\..\..\bin\CargoWiseOneSetup.msi CargoWiseOneSetup.wixobj -ext WixNetFxExtension -cultures:en-US
