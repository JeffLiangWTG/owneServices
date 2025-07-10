<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Control Language="c#" AutoEventWireup="True" Codebehind="IncidentSearchUserControl.ascx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.IncidentSearchUserControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Register Assembly="ZClientWebEDI" Namespace="Enterprise.ZClientWebCargoWiseEDI" TagPrefix="cc1" %>
<table class="ResultsTable" id="Table2" style="width: 380px;">
	<tr>
		<td style="width: 54px; height: 24px;">
			<asp:Label ID="Label2" CssClass="DetailsItem" runat="server" Width="110px">Incident Num: </asp:Label></td>
		<td colspan="5" style="width: 300px; height: 24px">
			<edi:ZTextBox ID="IncidentNumberTextBox" runat="server" BindTo="IncidentNumber" Width="117px"></edi:ZTextBox></td>
	</tr>
	<tr>
		<td style="width: 54px; height: 24px">
			<asp:Label ID="Label7" CssClass="DetailsItem" runat="server" Width="110px">Product: </asp:Label></td>
		<td style="height: 24px; width: 150px;">
			<edi:ZDropDownList ID="ProductDropDown" runat="server" BindTo="IM_Product" BindToList="ProductList"
				AutoPostBack="True" Width="120px" ShowEmptyItem="true" EmptyItemText="All Products">
			</edi:ZDropDownList></td>
		<td style="height: 24px; width: 72px;">&nbsp;
			</td>
		<td style="height: 24px; width: 91px;">
			<asp:Label ID="Label11" CssClass="DetailsItem" runat="server">Module: </asp:Label></td>
		<td style="height: 24px" align="right">
            <edi:ZDropDownList ID="ModuleDropDown" runat="server" BindTo="IM_Module" BindToList="ModuleList" Width="120px">
			</edi:ZDropDownList></td>
	</tr>
	<tr>
		<td style="width: 54px; height: 24px;">
			<asp:Label ID="Label9" CssClass="DetailsItem" runat="server" Width="110px">Placed by: </asp:Label></td>
		<td style="width: 80px; height: 24px">
			<edi:ZDropDownList ID="ContactNameDropDown" runat="server" BindTo="ContactName" BindToList="ContactNameList" 
				ShowEmptyItem="True" EmptyItemText="Anybody" Width="120px">
			</edi:ZDropDownList>
			</td>
		<td style="height: 24px; width: 72px;">&nbsp;
			</td>
		<td style="height: 24px; width: 91px;">
			<asp:Label ID="Label8" CssClass="DetailsItem" runat="server">Status: </asp:Label></td>
		<td style="height: 24px;" align="right">
            <edi:ZDropDownList ID="StatusDropDown" runat="server" BindTo="Status" BindToList="StatusList"
				DataTextField="Description" Width="120px">
			</edi:ZDropDownList></td>
	</tr>
	<tr>
		<td style="height: 24px; width: 54px;">
			<asp:Label ID="Label3" runat="server" CssClass="DetailsItem" Width="110px">Organization:</asp:Label>
		</td>
		<td colspan="4" style="height: 24px">
			<edi:ZGuidDropDownList ID="ZGuidDropDownList2" runat="server" BindTo="Organisation" BindToList="OrganisationList"
				DataTextField="OH_FullName" DataValueField="PK" CodeFieldName="OH_FullName"
				DescriptionFieldName="OH_FullName" ShowDescription="False" ReadOnly="False"
				ShowEmptyItem="True" EmptyItemText="All Organizations" Width="379px">
			</edi:ZGuidDropDownList>
		</td>
	</tr>
	<tr>
		<td style="height: 24px; width: 54px;">
			<asp:Label ID="Label6" CssClass="DetailsItem" runat="server" Width="110px">Criticality: </asp:Label></td>
		<td colspan="4" style="height: 24px">
			<edi:ZDropDownList ID="CriticalityDropDown" runat="server" BindTo="IM_Priority" BindToList="CriticalityList"
				DataTextField="CodeAndDescription" Width="379px">
			</edi:ZDropDownList>
		</td>		
	</tr>
	<tr>
		<td style="width: 54px; height: 24px;">
			<asp:Label ID="Label19" CssClass="DetailsItem" runat="server" Width="110px">Date added: </asp:Label></td>
		<td style="height: 24px">
			<edi:ZDateEdit ID="FromDateEdit" runat="server" BindTo="DateFrom" ToolTip="Select a date" Width="120px">
			</edi:ZDateEdit>			            
		</td>
		<td style="height: 24px; width: 72px;">&nbsp;
			</td>
		<td style="height: 24px; width: 91px;">
		    <asp:Label ID="Label1" CssClass="DetailsItem" runat="server">To: </asp:Label>
		</td>
		<td style="height: 24px" align="right">
		    <edi:ZDateEdit ID="ToDateEdit" runat="server" BindTo="DateTo" ToolTip="Select a date" Width="120px"></edi:ZDateEdit>
		</td>		
	</tr>
	<tr>
		<td style="width: 54px; height: 24px;">
			<asp:Label ID="DescLabel" CssClass="DetailsItem" runat="server" Width="110px">Description contains: </asp:Label></td>
		<td colspan="4" style="height: 24px">
			<edi:ZTextBox ID="DescriptionTextBox" runat="server" BindTo="Description" Width="99%"></edi:ZTextBox></td>
	</tr>
</table>
