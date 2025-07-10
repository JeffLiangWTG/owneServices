<%@ Register TagPrefix="edi" NameSpace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Control Language="c#" AutoEventWireup="false" Codebehind="OrganisationFilterControl.ascx.cs" Inherits="Enterprise.ZArchitecture.Web.GUI.WebControls.OrganisationFilterControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<edi:zdropdownlist id="DetailsList" runat="server" Width="128px" BindTo="OH_DetailsFilter" BindToList="OH_DetailsFilter_List"></edi:zdropdownlist>
<edi:ztextbox id="Details" runat="server" BindTo="OH_Details"></edi:ztextbox><br />
<edi:zradiobutton id="StartsWithRadioButton" runat="server" Text="Starts With" GroupName="SearchType"
	BindTo="OH_Calc_StartsWith"></edi:zradiobutton><edi:zradiobutton id="ContainsRadioButton" runat="server" Text="Contains" GroupName="SearchType" BindTo="OH_Calc_Contains"></edi:zradiobutton>
