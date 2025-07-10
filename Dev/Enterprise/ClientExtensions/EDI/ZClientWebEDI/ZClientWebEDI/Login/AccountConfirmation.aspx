<%@ Page Language="c#" CodeBehind="AccountConfirmation.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.ZClientWebCargoWiseEDI.AccountConfirmation" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>
<!DOCTYPE html>
<html class="login" xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>CargoWise | My Account > Home > Account Confirmation</title>
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
</head>
<body>
	<form id="Form1" runat="server">
		<!-- Theme Container -->
		<div id="login" runat="server">
			<div id="login-company-logo-container">
				<img id="login-company-logo" alt="CargoWise" src="/myaccount/Images/cargowise-logo.svg" />
			</div>
			<div id="login-content">
				<h1>Account Verification</h1>
				<div id="AccountConfirmationBox" runat="server">
					<span>You have successfully logged in.<br><br>
						Since your last login, we've noticed some changes.<br>
					</span>
			
					<edi:ZTextLabel id="ActionMessage1_ACR" runat="server">The following system user account was previously deactivated and has since been reactivated.</edi:ZTextLabel>
					<edi:ZTextLabel id="ActionMessage1_EMC" runat="server">Auto login from the following system account was deactivated because your email address has changed without verification.</edi:ZTextLabel>
					<edi:ZTextLabel id="ActionMessage1_MUL" runat="server">The following new system user is pending association to this MyAccount user account.</edi:ZTextLabel>
					<span>
					System: <edi:ZTextLabel id="SystemLabel" runat="server" Font-Bold="true"></edi:ZTextLabel>
					<br/>
					User: <edi:ZTextLabel id="UserLabel" runat="server" Font-Bold="true"></edi:ZTextLabel>
					<br/>
					Email: <edi:ZTextLabel id="EmailLabel" runat="server" Font-Bold="true"></edi:ZTextLabel>
					</span>
					<edi:ZTextLabel id="ActionMessage2_ACR" runat="server">Do you want to reactivate MyAccount auto login from this system user?</edi:ZTextLabel>
					<edi:ZTextLabel id="ActionMessage2_EMC" runat="server">Do you want to update your MyAccount login email and reactivate MyAccount auto login from this system user?</edi:ZTextLabel>
					<edi:ZTextLabel id="ActionMessage2_MUL" runat="server">Do you want to confirm this relationship and activate auto login from this system user?</edi:ZTextLabel>
					
					<div align="center">
						<asp:Button id="ConfirmationYes" runat="server" Text="Yes" onclick="ConfirmationYes_Click"></asp:Button>
						<asp:Button id="ConfirmationNo" runat="server" Text="No" onclick="ConfirmationNo_Click"></asp:Button>
					</div>
				</div>
				<edi:ZTextLabel id="ErrorMessage" runat="server" CssClass="ErrorMessage" />
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
</body>
</html>
