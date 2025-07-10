<%@ Register TagPrefix="edi" NameSpace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Control Language="c#" AutoEventWireup="false" Codebehind="RefCountryFilterControl.ascx.cs" Inherits="Enterprise.ZArchitecture.Web.GUI.Modules.Location.RefCountryFilterControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<edi:zradiobutton id="StartsWith" runat="server" Text="Starts With" GroupName="StartsContains" BindTo="StartsWith"></edi:zradiobutton>
<edi:zradiobutton id="Contains" runat="server" Text="Contains" GroupName="StartsContains" BindTo="Contains"></edi:zradiobutton>
<edi:ztextbox id="Description" runat="server" BindTo="RN_Desc"></edi:ztextbox>
