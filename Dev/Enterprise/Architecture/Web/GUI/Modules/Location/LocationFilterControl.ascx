<%@ Control Language="c#" AutoEventWireup="false" Codebehind="LocationFilterControl.ascx.cs" Inherits="Enterprise.ZArchitecture.Web.GUI.Modules.Location.LocationFilterControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<%@ Register TagPrefix="edi" NameSpace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<table class="ResultsTable">
	<tr>
		<td>
			<edi:ztextlabel runat="server" id="LocationTypeLabel">Location Type: </edi:ztextlabel>
		</td>
		<td>
			<edi:zradiobutton id="PortRadio" runat="server" Text="Port/UNLOCO" GroupName="LocationType" BindTo="IsPort"></edi:zradiobutton>
			<edi:zradiobutton id="RegionRadio" runat="server" Text="Zone" GroupName="LocationType" BindTo="IsRegion"></edi:zradiobutton>
			<edi:zradiobutton id="CountryRadio" runat="server" Text="Country" GroupName="LocationType" BindTo="IsCountry"></edi:zradiobutton>
		</td>
	</tr>
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
