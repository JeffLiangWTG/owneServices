<%@ Control Language="c#" AutoEventWireup="false" Codebehind="RefServiceLevelFilterControl.ascx.cs" Inherits="Enterprise.ZArchitecture.Web.GUI.Modules.ServiceLevel.RefServiceLevelFilterControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<%@ Register TagPrefix="edi" NameSpace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<table class="ResultsTable">
	<tr>
		<td>
			<edi:ztextlabel runat="server" id="CodeLabel">Code: </edi:ztextlabel>
		</td>
		<td>
			<edi:ztextbox id="Code" runat="server" BindTo="Code"></edi:ztextbox>
		</td>
	</tr>
	<tr>
		<td>
			<edi:ztextlabel runat="server" id="DescriptionLabel">Description: </edi:ztextlabel>
		</td>
		<td>
			<edi:ztextbox id="Description" runat="server" BindTo="Description"></edi:ztextbox>
		</td>
	</tr>
</table>
