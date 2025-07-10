<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InitialLoginSetPassword.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.InitialLoginSetPassword" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>
<!DOCTYPE html>
<html class="login" xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
	<title>CargoWise | My Account > Initial Login Set Password</title>
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
</head>
<body>
	<form id="form1" runat="server">
		<!-- Theme Container -->
		<div id="login">
			<div id="login-company-logo-container">
				<img id="login-company-logo" alt="CargoWise" src="/myaccount/Images/cargowise-logo.svg" />
			</div>
			<div id="login-content">
				<h1><edi:ZTextLabel ID="SetPasswordHeadingLabel" runat="server" Text="MyAccount Password"></edi:ZTextLabel></h1>

				<div class="login-row">
					<edi:ZTextLabel ID="SetPasswordInstructionsLabel" runat="server" Text="Set a password for login to "></edi:ZTextLabel>
					<b>
						<edi:ZTextLabel ID="OrgNameText" runat="server" Text=""></edi:ZTextLabel>
					</b>
				</div>

				<div class="login-row">
					<edi:ZTextLabel ID="OrgCodeLabel" runat="server" Text="Your Company Code is "></edi:ZTextLabel>
					<b>
						<edi:ZTextLabel ID="OrgCodeText" runat="server" Text=""></edi:ZTextLabel>
					</b>
				</div>
					
				<div class="login-row">
					<asp:textbox id="NewPassword" runat="server" TextMode="Password" Placeholder="New password"></asp:textbox>
				</div>
					
				<div class="login-row">
					<asp:textbox id="NewPasswordConfirm" runat="server" TextMode="Password" Placeholder="Confirm new password"></asp:textbox>
				</div>

				<div class="login-row">
					<asp:Button id="Update" runat="server" Text="Set Password" onclick="Update_Click"></asp:Button>
				</div>

				<edi:ZTextLabel id="PasswordChangeMessage" runat="server" CssClass="ErrorMessage" />
				<div class="login-row">
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
	<script type="text/javascript" src="../Scripts/buttontoggler.js"></script>
	<script>
		setupToggleButtonAvailabilityEventListener(document.getElementById('Update'), [document.getElementById('NewPassword'), document.getElementById('NewPasswordConfirm')]);
	</script>
</body>
</html>
