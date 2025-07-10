SET Namespace="Enterprise.Customs.DE.Business.MonthlyClosing"
SET Namespace2="Enterprise.Customs.DE.Business.MonthlyClosing.CusRecon"

for /F %%f in ('dir /b /s %~dp0\*.xsd') do (
  if not "%%~nf" == "cusrecon_snapshot" (xsd %%f /c /language:"CS" /namespace:%Namespace% /o:%%~dpf) else (xsd %%f /c /language:"CS" /namespace:%Namespace2% /o:%%~dpf)
) 
