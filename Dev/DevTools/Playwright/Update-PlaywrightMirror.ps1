# Update our Playwright mirror with the browsers needed by our Playwright version.
# When Microsoft.Playwright package is updated run "playwright install" on PowerShell
# It will download and install browsers to %UserProfile%\AppData\Local\ms-playwright
# Note the revision numbers and update the revision numbers below before running this

$BrowsersInJson = '{
  "browsers": [
    {
      "name": "chromium",
      "revision": "1148",
      "files": [
        "chromium",
        "chromium-headless-shell"
      ]
    },
    {
      "name": "ffmpeg",
      "revision": "1010"
    },
    {
      "name": "firefox",
      "revision": "1466"
    },
    {
      "name": "webkit",
      "revision": "2104"
    }
  ]
}';

$Browsers = $BrowsersInJson | ConvertFrom-Json
$Browsers.browsers | ForEach-Object {
	$browserName = $_.name
	$browserRevision = $_.revision
	$fileNames = if ($_.files) { $_.files } else { @($browserName) }
	$fileNames | ForEach-Object {
		$fileName = $_
		$url = "https://playwright.azureedge.net/builds/$browserName/$browserRevision/$fileName-win64.zip"
		$output = "\\datfiles.wtg.zone\ThirdParty\Web\playwright-mirror\builds\$browserName\$browserRevision\$fileName-win64.zip"
		New-Item -ItemType Directory -Path "\\datfiles.wtg.zone\ThirdParty\Web\playwright-mirror\builds\$browserName\$browserRevision"
		Invoke-WebRequest $url -OutFile $output
	}
}
