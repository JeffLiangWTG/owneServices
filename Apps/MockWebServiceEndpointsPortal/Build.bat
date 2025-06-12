@ECHO OFF
REM This batch file does a webpack build for the HTML site.

REM Check install path
CALL :GetNodeJSInstallPath
IF ERRORLEVEL 1 (
	ECHO Unable to find Node.js installation path on local machine
	POPD
	EXIT /B 1
)

ECHO Found Node.js at "%NodeJSExePath%"

ECHO Disabling update notifier
SET NO_UPDATE_NOTIFIER=1

set profile=%1
echo Profile = "%profile%"

ECHO Setting working directory to %~dp0
PUSHD %~dp0

ECHO Packaging - all
CMD /c "%NpmPath%" run package
IF ERRORLEVEL 1 (
	ECHO Failed to package
	POPD
	EXIT /B 1
)

POPD

EXIT /B 0

:GetNodeJSInstallPath
REM Search for Node.js in HKLM and HCU in both 64 and 32 bit views
FOR /F "tokens=1,2*" %%i IN ('reg query "HKEY_LOCAL_MACHINE\Software\Node.js" /v "InstallPath" /reg:64') DO (
	IF "%%i" == "InstallPath" ( 
		SET "NodeJSExePath=%%knode.exe"
		SET "NpmPath=%%knpm"
	)
)

IF "%NodeJSExePath%" == "" (
	FOR /F "tokens=1,2*" %%i IN ('reg query "HKEY_LOCAL_MACHINE\Software\Node.js" /v "InstallPath" /reg:32') DO (
		IF "%%i" == "InstallPath" ( 
			SET "NodeJSExePath=%%knode.exe"
			SET "NpmPath=%%knpm"
		)
	)	
)

IF "%NodeJSExePath%" == "" (
	FOR /F "tokens=1,2*" %%i IN ('reg query "HKEY_CURRENT_USER\Software\Node.js" /v "InstallPath" /reg:64') DO (
		IF "%%i" == "InstallPath" ( 
			SET "NodeJSExePath=%%knode.exe"
			SET "NpmPath=%%knpm"
		)
	)	
)

IF "%NodeJSExePath%" == "" (
	FOR /F "tokens=1,2*" %%i IN ('reg query "HKEY_CURRENT_USER\Software\Node.js" /v "InstallPath" /reg:32') DO (
		IF "%%i" == "InstallPath" ( 
			SET "NodeJSExePath=%%knode.exe"
			SET "NpmPath=%%knpm"
		)
	)	
)

IF ("%NodeJSPath%"=="") EXIT /B 1
IF ("%NpmPath%"=="") EXIT /B 1
EXIT /B 0

