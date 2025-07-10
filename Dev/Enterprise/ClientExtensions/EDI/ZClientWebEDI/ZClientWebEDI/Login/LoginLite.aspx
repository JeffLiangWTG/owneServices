<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginLite.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.LoginLite" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE html>
<html class="login" xmlns="http://www.w3.org/1999/xhtml">
<HEAD>
	<title>CargoWise | My Account > Home > My Account Login</title>
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
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

		function showCompanyCode() {
			const companyCodeDiv = document.getElementById("CompanyCodeDiv");
			const showCompanyCodeLabelDiv = document.getElementById("ShowCompanyCodeLabelDiv");
			if (companyCodeDiv.style.display === "none") {
				companyCodeDiv.style.display = "block";
				showCompanyCodeLabelDiv.style.display = "none";
			}
		}
	</script>

	<form id="Form1" runat="server">
		<!-- Theme Container -->
		<div id="login">
			<div id="login-company-logo-container">
				<img id="login-company-logo" alt="CargoWise" src="/myaccount/Images/cargowise-logo.svg" />
			</div>
			<div id="login-page">
				<div id="login-content">
					<h1>Log in to My Account</h1>

					<div style="text-align: center">
						<edi:ZTextLabel id="Message" runat="server" CssClass="ErrorMessage"/>
					</div>

					<label class="LoginLabel">Email address</label>
					<edi:ztextbox id="LoginNameTextBox" runat="server" BindTo="UserName" MaxLength="40" TabIndex="1"></edi:ztextbox>

					<label class="LoginLabel">Password</label><span class="ForgotPassword"><a href="RetrieveLogin.aspx" TabIndex="-1">Forgot your password?</a></span>
					<edi:ztextbox id="PasswordTextBox" textmode="password" runat="server" BindTo="Password" TabIndex="2"></edi:ztextbox>

					<div id="ShowCompanyCodeLabelDiv" runat="server">
						<label id="ShowCompanyCodeLabel" runat="server">Specify Company Code (optional)</label>
					</div>

					<div id="CompanyCodeDiv" runat="server" style="display: none">
						<label class="LoginLabel">Company code</label>
						<edi:ztextbox id="CompanyCodeTextBox" runat="server" BindTo="CompanyCode" MaxLength="12" Placeholder="Optional" TabIndex="3"></edi:ztextbox>
					</div>

					<div style="margin-top: 32px">
						<asp:Button id="SigninBtn" runat="server" Text="Sign in" onclick="SigninBtn_Click" TabIndex="4"></asp:Button>
					</div>

					<div id="login-options-row" style="justify-content: center">
						<edi:ZCheckBox id="RememberMeCheckBox" runat="server" CssClass="RememberMeBox" BindTo="RememberMe" Text="Remember me" TabIndex="5"></edi:ZCheckBox>
					</div>
				</div>
				<div id="login-footer">
					<strong>&copy; <edi:ZTextLabel id="CopyrightYear" runat="server"></edi:ZTextLabel> WiseTech Global</strong>
					<a target="_blank" href="https://www.wisetechglobal.com/legal#privacypolicy">Privacy Policy</a>
					<a target="_blank" href="https://www.wisetechglobal.com/legal#cookies">Cookie Usage</a>
					<a target="_blank" href="https://www.wisetechglobal.com/legal#termsofuse">Terms of Use</a>
				</div>
			</div>
		</div>
		<!-- End Theme Container -->
	</form>
	<script type="text/javascript" src="../Scripts/buttontoggler.js"></script>
	<script>
		document.getElementById('LoginNameTextBox').focus();
		setupToggleButtonAvailabilityEventListener(document.getElementById('SigninBtn'), [document.getElementById('LoginNameTextBox'), document.getElementById('PasswordTextBox')]);
	</script>
</body>
</html>
