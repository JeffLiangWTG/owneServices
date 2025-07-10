<%@ Control Language="c#" AutoEventWireup="false" Codebehind="SimpleFilterControl.ascx.cs" Inherits="Enterprise.ZArchitecture.Web.GUI.Modules.Base.SimpleFilterControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<%@ Register TagPrefix="edi" NameSpace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<edi:zradiobutton id="rbStartsWith" runat="server" Text="Starts With" GroupName="StartsContains" BindTo="StartsWith"></edi:zradiobutton>
<edi:zradiobutton id="rbContains" runat="server" Text="Contains" GroupName="StartsContains" BindTo="Contains"></edi:zradiobutton>
<edi:ztextbox id="tbSoughtText" runat="server" BindTo="SoughtText"></edi:ztextbox>
