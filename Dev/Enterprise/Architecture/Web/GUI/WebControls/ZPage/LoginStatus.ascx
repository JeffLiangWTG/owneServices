<%@ Control Language="c#" AutoEventWireup="false" Codebehind="LoginStatus.ascx.cs" Inherits="Enterprise.ZArchitecture.Web.GUI.WebControls.LoginStatus" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<div id="LoginStatusString" align="right">
	<asp:Label ID="WelcomeLabel" runat="server" AssociatedControlID="WelcomeLabel"></asp:Label>
&nbsp;<asp:Label id="UserName" runat="server"></asp:Label>!&nbsp;&nbsp;
	<asp:Label id="CompanyName" Runat="server"></asp:Label>
	&nbsp;&nbsp;&nbsp;&nbsp;
	<asp:LinkButton ID="ChangePasswordLinkButton" runat="server"></asp:LinkButton>
	&nbsp;&nbsp;
	<asp:LinkButton ID="LogOffLinkButton" runat="server"></asp:LinkButton>
	<div id="AdditionalLoginContent" class="AdditionalLoginContent" runat="server"></div>
</div>
