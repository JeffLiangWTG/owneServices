<%@ Register TagPrefix="edi" NameSpace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Control Language="c#" AutoEventWireup="false" Codebehind="RefContainerFilterControl.ascx.cs" Inherits="Enterprise.ZArchitecture.Web.GUI.WebControls.RefContainerFilterControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<table id="Table1" class="ResultsTable">
    <tr>
        <td>Code/Description:</td>
        <td>
            <edi:ztextbox id="Details" runat="server" BindTo="RC_Description"></edi:ztextbox>
	    </td>
	</tr>
	<tr>
	    <td></td>
	    <td>
            <edi:zradiobutton id="StartsWithRadioButton" runat="server" Text="Starts With" GroupName="SearchType"
	            BindTo="RC_DescriptionStartsWith"></edi:zradiobutton><edi:zradiobutton id="ContainsRadioButton" runat="server" Text="Contains" GroupName="SearchType" BindTo="RC_DescriptionContains"></edi:zradiobutton>
	    </td>
	</tr>
	<tr>
	    <td>Transport Mode:</td>
	    <td>
	        <edi:zdropdownlist id="ShippingMode" runat="server" Width="128px" BindTo="RC_ShippingMode" BindToList="ShippingModes" ShowEmptyItem="true" EmptyItemText="All"></edi:zdropdownlist>
	    </td>
    </tr>
</table>