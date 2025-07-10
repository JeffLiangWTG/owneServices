param (
    [string]$Name
)

if ($Name) {
    if ($Env:QGL_IS_BUILDING -eq "True") {
        if (Test-Path -Path "$PSScriptRoot\BuildFlags.props") {
            $xml = [xml](Get-Content "BuildFlags.props")
            return $xml.Project.PropertyGroup.$Name
        }
        Write-Host "No BuildFlags.props file found"
    }
} else {
    Write-Host @"

USAGE:

    .\BuildFlags.ps1 -Name <FlagName>
    If the QGL_IS_BUILDING environment variable is not set to "True", this will return null.
    Otherwise it returns the value of the named build flag from BuildFlags.props as a string.
    If there is no associated entry in BuildFlags.props then it returns null.

"@
}