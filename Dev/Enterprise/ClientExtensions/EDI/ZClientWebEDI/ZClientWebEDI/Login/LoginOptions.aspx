<%@ Page language="c#" Codebehind="LoginOptions.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.ZClientWebCargoWiseEDI.LoginOptions" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Tagname="AccountVerification" src="AccountVerificationControl.ascx" %>
<!DOCTYPE html>
<html class="login" xmlns="http://www.w3.org/1999/xhtml">
<HEAD>
	<title>CargoWise | My Account > Home > Account Verification</title>
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
	<style type="text/css">
		body { counter-reset: AutoNumCounter; }
		.AutoNum:before {
			content: counter(AutoNumCounter) ". ";
			counter-increment: AutoNumCounter;
		}
	</style>
</HEAD>
<body>
	<!-- Google Tag Manager -->
	<noscript><iframe src="//www.googletagmanager.com/ns.html?id=GTM-PWS25L"
	height="0" width="0" style="display:none;visibility:hidden"></iframe></noscript>
	<script>(function (w, d, s, l, i) {
	w[l] = w[l] || []; w[l].push({
	'gtm.start':
	new Date().getTime(), event: 'gtm.js'
	}); var f = d.getElementsByTagName(s)[0],
	j = d.createElement(s), dl = l != 'dataLayer' ? '&l=' + l : ''; j.async = true; j.src =
	'//www.googletagmanager.com/gtm.js?id=' + i + dl; f.parentNode.insertBefore(j, f);
	})(window, document, 'script', 'dataLayer', 'GTM-PWS25L');</script>
	<!-- End Google Tag Manager -->

	<script>
		if (top.location != self.location) {
			top.location = self.location.href;
		}
	</script>

	<form id="Form1" runat="server">
		<!-- Theme Container -->
		<div id="login">
			<div id="login-company-logo-container">
				<img id="login-company-logo" alt="CargoWise" src="/myaccount/Images/cargowise-logo.svg" />
			</div>
			<div id="login-content">
				<div>
					<edi:AccountVerification id="AccountVerification" runat="server" />
				</div>
			</div>
			<div id="login-footer">
				<strong>&copy; <edi:ZTextLabel id="CopyrightYear" runat="server"></edi:ZTextLabel> WiseTech Global</strong>
				<a target="_blank" href="https://www.wisetechglobal.com/legal#privacypolicy">Privacy Policy</a>
				<a target="_blank" href="https://www.wisetechglobal.com/legal#cookies">Cookie Usage</a>
				<a target="_blank" href="https://www.wisetechglobal.com/legal#termsofuse">Terms of Use</a>
			</div>
		</div>
		<!-- End Theme Container -->
	</form>
	<script>
		document.getElementById('PasswordTextbox').focus();
	</script>
</body>
</html>
