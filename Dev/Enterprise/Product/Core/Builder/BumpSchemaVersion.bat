@echo off
rem call GenerateDbUpgraderResources.exe to BUMP SCHEMA VERSION only

pushd %~dp0

::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: Evaluate Arguments.

if /i "%1" == "-Checkin"      goto Checkin
if /i "%1" == "-UndoCheckout" goto UndoCheckout
if /i "%1" == "-Major"        goto BumpMajor
if /i NOT "%1" == ""          goto InvalidArgument


::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:Bump

..\..\..\..\Bin\GenerateDbUpgraderResources.exe -Bump -CheckOut
goto EndSetup


::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:BumpMajor

..\..\..\..\Bin\GenerateDbUpgraderResources.exe -BumpMajor -CheckOut
goto EndSetup


::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:Checkin

..\..\..\..\Bin\GenerateDbUpgraderResources.exe -CheckIn
goto EndSetup



::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:UndoCheckOut

..\..\..\..\Bin\SetupDbUpgrader.exe -UndoCheckOut
goto EndSetup



::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:InvalidArgument
echo.
echo Invalid BumpSchemaVersion argument
echo.
pause


::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:EndSetup
popd
