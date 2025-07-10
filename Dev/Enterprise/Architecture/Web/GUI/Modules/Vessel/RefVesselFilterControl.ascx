<%@ Control Language="c#" AutoEventWireup="false" Codebehind="RefVesselFilterControl.ascx.cs" Inherits="Enterprise.ZArchitecture.Web.GUI.Modules.Vessel.RefVesselFilterControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Register TagPrefix="edi" NameSpace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<edi:zradiobutton id="StartsWith" runat="server" Text="Starts With" GroupName="StartsContains" BindTo="RV_CodeStartsWith"></edi:zradiobutton>
<edi:zradiobutton id="Contains" runat="server" Text="Contains" GroupName="StartsContains" BindTo="RV_CodeContains"></edi:zradiobutton>
<edi:ztextbox id="Code" runat="server" BindTo="RV_Code"></edi:ztextbox>
