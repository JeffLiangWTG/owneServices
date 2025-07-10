<%@ Control Language="c#" AutoEventWireup="false" Codebehind="RefUNLOCOFilterControl.ascx.cs" Inherits="Enterprise.ZArchitecture.Web.GUI.Modules.Location.RefUNLOCOFilterControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<%@ Register TagPrefix="edi" NameSpace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<TABLE id="Table1" class="ResultsTable">
	<TR>
		<td>Port Name:</td>
		<TD><edi:ztextbox id="Description" runat="server" BindTo="RL_PortName"></edi:ztextbox></TD>
		<td>&nbsp;</td>
		<TD>
			<edi:zradiobutton id="StartsWith" runat="server" Text="Starts With" GroupName="StartsContains" BindTo="StartsWith"></edi:zradiobutton>
			<edi:zradiobutton id="Contains" runat="server" Text="Contains" GroupName="StartsContains" BindTo="Contains"></edi:zradiobutton>
		</TD>
	</TR>
	<tr>
		<td>Code:</td>
		<td colspan="3"><edi:ztextbox id="Ztextbox1" runat="server" BindTo="RL_Code"></edi:ztextbox></td>
	</tr>
	<tr>
		<td>IATA:</td>
		<td colspan="3"><edi:ztextbox id="Ztextbox2" runat="server" BindTo="RL_IATA"></edi:ztextbox></td>
	</tr>
</TABLE>
