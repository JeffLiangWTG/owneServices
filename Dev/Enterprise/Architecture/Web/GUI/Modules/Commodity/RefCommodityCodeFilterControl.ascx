<%@ Control Language="c#" AutoEventWireup="false" Codebehind="RefCommodityCodeFilterControl.ascx.cs" Inherits="Enterprise.ZArchitecture.Web.GUI.Modules.Commodity.RefCommodityCodeFilterControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Register TagPrefix="edi" NameSpace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<edi:zradiobutton id="StartsWith" runat="server" Text="Starts With" GroupName="StartsContains" BindTo="RH_DescriptionStartsWith"></edi:zradiobutton>
<edi:zradiobutton id="Contains" runat="server" Text="Contains" GroupName="StartsContains" BindTo="RH_DescriptionContains"></edi:zradiobutton>
<edi:ztextbox id="Description" runat="server" BindTo="RX_Desc"></edi:ztextbox>
