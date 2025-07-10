# ToxiProxy in DAT
## Background
[ToxiProxy]( https://github.com/shopify/toxiproxy) is a chaos testing framework created by Shopify and written in GO. It allows developer to simulate different "toxic" network conditions and test application's resiliency, availability and stability.

ToxiProxy has two main components:

1. ToxiProxy server - A TCP proxy written in GO
2. ToxiProxy client - A client to communicate with ToxiProxy server over HTTP

ToxiProxy client can be implemented using any language or framework supporting sending HTTP requests. However, ToxiProxy server is distributed as binary executable and needed to be downloaded manually. Therefore, we need to store this executable somewhere easily accessible to both developer and DAT.

we will manually copy ToxiProxy-server.exe from ToxiProxy releases to out datfiles mirror.

See [Update-ToxiProxy-Server.ps1](Update-ToxiProxy-Server.ps1).
