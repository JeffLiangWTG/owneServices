<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PasswordChangeRequirementsControl.ascx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.PasswordChangeRequirementsControl" %>

<div class="login-row">
	<span class="passwordRequirementSpan" id="MinLengthSpan">
		<asp:Image ID="MinLengthPass" runat="server" ImageUrl="../Images/passwordCheckPass.svg" ClientIDMode="Static" style="display: none;margin: 0px" />
		<asp:Image ID="MinLengthFail" runat="server" ImageUrl="../Images/passwordCheckFail.svg" ClientIDMode="Static" style="margin: 0px" />
		<asp:Label ID="MinLengthLabel" runat="server" />
		<br />
	</span>

	<span class="passwordRequirementSpan" id="AtLeastThreeSpan" style="display: none">
		<asp:Image ID="AtLeastThreePass" runat="server" ImageUrl="../Images/passwordCheckPass.svg" ClientIDMode="Static" style="display: none" />
		<asp:Image ID="AtLeastThreeFail" runat="server" ImageUrl="../Images/passwordCheckFail.svg" ClientIDMode="Static" />
		<asp:Label ID="AtLeastThreeLabel" runat="server" />
		<br />
	</span>

	<span class="passwordRequirementSpan" id="LowerSpan">
		<asp:Image ID="LowerPass" runat="server" ImageUrl="../Images/passwordCheckPass.svg" ClientIDMode="Static" style="display: none" />
		<asp:Image ID="LowerFail" runat="server" ImageUrl="../Images/passwordCheckFail.svg" ClientIDMode="Static" />
		<asp:Label ID="LowerLabel" runat="server" />
		<br />
	</span>

	<span class="passwordRequirementSpan" id="UpperSpan">
		<asp:Image ID="UpperPass" runat="server" ImageUrl="../Images/passwordCheckPass.svg" ClientIDMode="Static" style="display: none" />
		<asp:Image ID="UpperFail" runat="server" ImageUrl="../Images/passwordCheckFail.svg" ClientIDMode="Static" />
		<asp:Label ID="UpperLabel" runat="server" />
		<br />
	</span>

	<span class="passwordRequirementSpan" id="NumberSpan">
		<asp:Image ID="NumberPass" runat="server" ImageUrl="../Images/passwordCheckPass.svg" ClientIDMode="Static" style="display: none" />
		<asp:Image ID="NumberFail" runat="server" ImageUrl="../Images/passwordCheckFail.svg" ClientIDMode="Static" />
		<asp:Label ID="NumberLabel" runat="server" />
		<br />
	</span>

	<span class="passwordRequirementSpan" id="SpecialCharSpan">
		<asp:Image ID="SpecialCharPass" runat="server" ImageUrl="../Images/passwordCheckPass.svg" ClientIDMode="Static" style="display: none" />
		<asp:Image ID="SpecialCharFail" runat="server" ImageUrl="../Images/passwordCheckFail.svg" ClientIDMode="Static" />
		<asp:Label ID="SpecialCharLabel" runat="server" />
		<br />
	</span>

	<input id="CheckResult" type="hidden" value="pass" />
</div>
