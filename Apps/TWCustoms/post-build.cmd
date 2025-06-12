set BinDir=%~dp0Bin
set RelativePackageOutputPath=BAT
set PackageOutputPath=%BinDir%\%RelativePackageOutputPath%

set dotnet="%ProgramFiles%\dotnet\dotnet.exe"

set fciv_md5="%BinDir%\Deployment\Tools\fciv\fciv_md5.cmd"
call %fciv_md5% "%BinDir%"

call %BinDir%\Deployment\SetMSBuild.cmd
%msbuild% "%BinDir%\Deployment\BAT\Bat.proj" /t:bat:package:discover /p:Operator=datbackground /p:Configuration=%QGL_BUILD_CONFIGURATION%

cmd /c exit 0