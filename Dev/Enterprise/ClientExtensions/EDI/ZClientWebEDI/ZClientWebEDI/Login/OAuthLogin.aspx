<%@ Page Language="C#" AutoEventWireup="true" EnableViewState="false" CodeBehind="OAuthLogin.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.OAuthLogin" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>

<!DOCTYPE html>
<html class="login">
<head runat="server">
	<title>CargoWise | Support Login</title>
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
	<link type="text/css" rel="stylesheet" href="/myaccount/BaseStyle.css" /></HEAD></head>
<body>
	<form id="loginForm" runat="server" method="post" class="login-form" enableviewstate="false">
		<!-- Theme Container -->
		<div id="login">
			<div id="login-company-logo-container">
				<img id="login-company-logo" alt="CargoWise" src="/myaccount/Images/cargowise-logo.svg" />
			</div>
			<div id="login-content">
				<h1>Support Login</h1>

				<input type="hidden" name="redirect_uri" runat="server" id="redirect_uri" />
				<input type="hidden" name="state" runat="server" id="state" />
				<input type="hidden" name="scope" runat="server" id="scope" />
				<input type="hidden" name="client_id" runat="server" id="client_id" />

				<edi:ZTextLabel ID="errorMessage" runat="server" Mode="Encode" CssClass="ErrorMessage" />

				<input type="text" name="userName" autofocus placeholder="Username" />
				<input type="password" name="password" placeholder="Password" />

				<input type="submit" value="Log In" />

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
