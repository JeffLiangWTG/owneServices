<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page Language="c#" EnableViewStateMac="false" CodeBehind="LoginSuperseded.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.ZClientWebCargoWiseEDI.LoginSuperseded" %>
<!DOCTYPE html>
<html class="login" xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>CargoWise | My Account > Home > Login Superseded</title>
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
</head>
<body>
<form id="Form1" runat="server">
	<div id="login" runat="server">
		<div id="login-company-logo-container">
			<img id="login-company-logo" alt="CargoWise" src="/myaccount/Images/cargowise-logo.svg" />
		</div>
		<div id="login-content">
			<div id="InstructionsBox" class="loginBox">
				<h1>User Account Update</h1>
				<edi:ZTextLabel ID="SupersededInstructionsLabel" runat="server" Text="User Account changes have occurred which affect the way you login into My Account.<br>One or more of your existing user accounts will no longer be accessible, but don't worry, you'll still have access to My Account.<br><br>First, a new password is required, that will be applied to all linked user accounts.<br><br>"></edi:ZTextLabel>
				<asp:button ID="SetMasterPasswordButton" runat="server" text="Continue" onclick="SetMasterPasswordButton_Click"></asp:button>
				<edi:ZTextLabel ID="ErrorMessage" runat="server" cssclass="ErrorMessage" />
			</div>
		</div>
		<div id="login-footer">
			<strong>&copy; <edi:ZTextLabel id="CopyrightYear" runat="server"></edi:ZTextLabel> WiseTech Global</strong>
			<a target="_blank" href="https://www.wisetechglobal.com/legal#privacypolicy">Privacy Policy</a>
			<a target="_blank" href="https://www.wisetechglobal.com/legal#cookies">Cookie Usage</a>
			<a target="_blank" href="https://www.wisetechglobal.com/legal#termsofuse">Terms of Use</a>
		</div>
	</div>
</form>
</body>
</html>
