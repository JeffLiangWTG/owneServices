<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AccountVerificationControl.ascx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.AccountVerificationControl" %>
<div runat="server" id="HeaderDiv" visible="true">
	<h1>Account Verification</h1>
</div>

<edi:ZTextLabel id="MessageLabel" runat="server" CssClass="ErrorMessage" style="font-weight:normal" />

<edi:ZTextLabelNoEncode id="ActionInstructionAccountReactivatedLabel" runat="server">We've noticed some changes to your account since your last login.<br/>
<br/>
<br/>Please verify your account through one of the following options:<br/>
<br/></edi:ZTextLabelNoEncode>
<edi:ZTextLabelNoEncode id="ActionInstructionEmailChangedLabel" runat="server">Since your last Login, we've noticed some changes.<br/>
Your email address has since changed without verification.<br/>
As a result, Auto Login has been disabled until we can make sure this is really you.<br/>
<br/>Please verify your account through one of the following options:<br/>
<br/></edi:ZTextLabelNoEncode>
<edi:ZTextLabelNoEncode id="ActionInstructionMultipleUIDLinkedLabel" runat="server">We've noticed that you are logging into MyAccount from a system that has not yet been associated to your user account.<br/>
As a result, auto login has been disabled until we can make sure this is really you.<br/>
<br/>Please verify your account through one of the following options:<br/>
<br/></edi:ZTextLabelNoEncode>
<edi:ZTextLabelNoEncode id="ActionInstructionContactMovedLabel" runat="server">Since your last Login, we've made some changes.<br/>
Your account was previously linked to a Company Code that is no longer supported for auto login.<br/>
As a result, we have merged your User Account into Company Code: <b>(*retainedOrgCode*)</b><br/><br/>
Auto login has been temporarily disabled until we can make sure this is really you.<br/>
<br/>Please verify your account through one of the following options:<br/>
<br/></edi:ZTextLabelNoEncode>
<edi:ZTextLabel id="PasswordMessageLabel" runat="server" CssClass="AutoNum">Enter your MyAccount Password.</edi:ZTextLabel><br/><br/>
<div class="login-row">
	<edi:ztextbox id="PasswordTextbox" textmode="password" runat="server" MaxLength="40" PlaceHolder="Password"></edi:ztextbox>
	<asp:Button id="VerifyPasswordButton" runat="server" Text="Sign in" onclick="VerifyPasswordButton_Click"></asp:Button>
</div>
<div class="login-row">
	<edi:ZTextLabel id="EmailMessageLabel" runat="server" CssClass="AutoNum">Verify  your account via the verification email sent to your recovery email</edi:ZTextLabel>
	<edi:ZTextLabel id="EmailLabel" runat="server" Font-Bold="true">**@**.com</edi:ZTextLabel>
	<asp:LinkButton id="SendVerificationEmailButton" runat="server" Text="Send Verification" onclick="SendVerificationEmailButton_Click"></asp:LinkButton>
</div>
<edi:ZTextLabelNoEncode id="ERequestMessageLabel" runat="server" CssClass="AutoNum">Contact your organisation administrator to create an eRequest on your behalf.<br/><br/>
In the eRequest please use one of the below criteria.<br/>
<br/>
Product: CargoWise<br/>
Criticality: CR9<br/>
Module/Service: My Account Authentication/Credentials<br/>
<br/>
Product: BorderWise<br/>
Criticality: CR9<br/>
Module/Service: License Management<br/>
</edi:ZTextLabelNoEncode>
