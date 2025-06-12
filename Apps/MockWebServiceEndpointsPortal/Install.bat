@ECHO OFF
REM This batch file downloads npm packages for the HTML site.

REM Check install path
CALL :GetNodeJSInstallPath
IF ERRORLEVEL 1 (
	ECHO Unable to find Node.js installation path on local machine
	POPD
	EXIT /B 1
)

ECHO Found Node.js at "%NodeJSExePath%"

ECHO Setting working directory to %~dp0
PUSHD %~dp0
ECHO Installing Modules...
CALL "%NpmPath%" ci --registry https://proget.wtg.zone/npm/Registry/
IF ERRORLEVEL 1 (
	ECHO Failed to install node modules
	POPD
	EXIT /B %ERRORLEVEL%
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
