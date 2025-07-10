# Update our Toxiproxy mirror with toxiproxy server version required

$ToxiProxyServerVersion = 'v2.5.0'

$url = "https://github.com/Shopify/toxiproxy/releases/download/$ToxiProxyServerVersion/toxiproxy-server-windows-amd64.exe"
$output = "\\datfiles.wtg.zone\ThirdParty\Web\toxiproxy-server-mirror\$ToxiProxyServerVersion\toxiproxy-server-windows-amd64.exe"

New-Item -ItemType Directory -Path "\\datfiles.wtg.zone\ThirdParty\Web\toxiproxy-server-mirror\$ToxiProxyServerVersion"
Invoke-WebRequest $url -OutFile $output
