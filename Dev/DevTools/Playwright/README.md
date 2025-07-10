# Playwright in DAT
## Background
[Playwright] is a browser automation testing framework by Microsoft. (If you're familiar with Selenium, it's similar and better.)

Playwright needs copies of Chromium, Firefox, and Webkit, depending which browser/s you choose to write your tests for.

Playwright's authors maintain optimised, standalone builds of these browsers which get stored in Playwright's cache. This means you don't need the browsers installed in Windows and they don't interfere with your normal browsers.

By default Playwright downloads its browsers at build time from its CDN.

This default doesn't work for us because DAT is firewalled from the internet, for security and reliability. Resources needed by DAT have to be preloaded on DAT VM images or downloaded from our mirrors.

Also, our developer PCs are behind a proxy server which Playwright's nodejs-based installer doesn't understand without further configuration.

## Browser build mirror
We keep a local mirror of Playwright's builds in \\\datfiles.wtg.zone\\ThirdParty\\Web\\playwright-mirror which is accessible to DAT and developer PCs (without a proxy server) at http://datfiles.wtg.zone/playwright-mirror/

Rather than automatically mirror every build for every platform for every browser, we will semi-manually copy builds we need from Playwright's CDN to our datfiles mirror.

Even though our initial usage of Playwright is only with Chromium, Playwright will also install Firefox and Webkit. There is a separate Playwright npm package which only installs Chromium, but it is not wrapped by the PlaywrightSharp NuGet package. I don't consider it worth the effort of re-packaging or overriding this to only install Chromium. It means any machine running the tests will download unnecessary copies of Firefox and Webkit, but they are cached locally so it is a one-time cost.

See Update-PlaywrightMirror.ps1 in this folder for a script to update the mirror.









[Playwright]: https://playwright.dev/
