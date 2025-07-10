..\packages\WiX\tools\candle -out ..\Enterprise\Product\Documents\RemotePrinting\Client\Setup\Setup.wixobj ..\Enterprise\Product\Documents\RemotePrinting\Client\Setup\Setup.wxs  -fips
..\packages\WiX\tools\light -out CargoWiseOneWebPrintClientSetup.msi ..\Enterprise\Product\Documents\RemotePrinting\Client\Setup\Setup.wixobj -ext WixUiExtension -ext WixNetFxExtension -cultures:en-US

copy ..\Enterprise\Product\Documents\RemotePrinting\Client\Setup\Setup.version CargoWiseOneWebPrintClientSetup.version

del /F /Q ..\Enterprise\Product\Documents\RemotePrinting\Client\Setup\Setup.wixobj
