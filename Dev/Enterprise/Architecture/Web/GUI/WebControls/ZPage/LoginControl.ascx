<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Control Language="c#" AutoEventWireup="false" Codebehind="LoginControl.ascx.cs" Inherits="Enterprise.ZArchitecture.Web.GUI.WebControls.LoginControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<div id="loginBox">
	Company Code:<br>
	<edi:ztextbox id="CompanyCodeTextBox" runat="server" BindTo="CompanyCode" MaxLength="12"></edi:ztextbox>
	<br>
	E-mail:<br>
	<edi:ztextbox id="LoginNameTextBox" runat="server" BindTo="UserName" MaxLength="40"></edi:ztextbox>
	<br>
	Password:<br>
	<edi:ztextbox id="PasswordTextBox" textmode="password" runat="server" BindTo="Password" MaxLength="40"></edi:ztextbox>
	<br>
	<br>
	<asp:Button id="SigninBtn" runat="server" Text="Login"></asp:Button>
	&nbsp;&nbsp;&nbsp;
	<asp:label id="Message" runat="server" CssClass="ErrorMessage" />
</div>
