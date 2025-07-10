using System;
using System.Reflection;
using System.Web.UI;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZPageDeleteTest : ZPageLifeCycleTest
	{
		protected override ZPage GetNewPage()
		{
			return new DummyPage();
		}

		protected new DummyPage TestPage
		{
			get { return base.TestPage as DummyPage; }
		}

		[ExpectNoExceptions("ProcessRequestMain should not throw exceptions")]
		public void TestDeleteBusinessObject()
		{
			var siteUser = new OrgContactWebUser();
			siteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var dummyTestGlobal = (ZDummyTestGlobal)TestPage.AppInstance;
			dummyTestGlobal.SiteUserOverride = siteUser;

			var allowedLanguages = WebDataRegistry.Instance.AllowedLanguages.Value;
			allowedLanguages.RemoveAll();
			var allowedItem = allowedLanguages.AddNew();
			allowedItem.Code = Enterprise.Core.SharedConstants.Languages.English;
			allowedItem = allowedLanguages.AddNew();
			allowedItem.Code = Enterprise.Core.SharedConstants.Languages.ChineseSimplified;
			allowedItem = allowedLanguages.AddNew();
			allowedItem.Code = Enterprise.Core.SharedConstants.Languages.French;
			allowedItem = allowedLanguages.AddNew();
			allowedItem.Code = Enterprise.Core.SharedConstants.Languages.German;
			allowedItem = allowedLanguages.AddNew();
			allowedItem.Code = Enterprise.Core.SharedConstants.Languages.Spanish;
			WebDataRegistry.Instance.AllowedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedLanguages);
			RunPageLifeCycle();

			this.AssertRenderComplete += new RenderCompleteEventHandler(ZPageLifeCycleTest_AssertRenderComplete);
			PostData.Add(TestPage.DeleteButton.ClientID, "Delete");
			IsPostBack = true;
			RunPageLifeCycle();
		}

		void ZPageLifeCycleTest_AssertRenderComplete(object sender, RenderCompleteEventArgs e)
		{
			string eventValidationValue = typeof(ClientScriptManager).GetMethod("GetEventValidationFieldValue", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(TestPage.ClientScriptInternal, null) as string;
			Assert("EventValidationValue should not be empty", !string.IsNullOrEmpty(eventValidationValue));

			#region ExpectedOutput

			Control validationControlHolder = TestPage.FindControl("ValidationControls");
			string validationControlID = validationControlHolder.Controls.Count == 1 ? validationControlHolder.Controls[0].ClientID : "";

			AssemblyFileVersionAttribute assemblyVersionAttribute = (AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(typeof(ZDateEdit).Assembly, typeof(AssemblyFileVersionAttribute));
			string runtimeVersion = (assemblyVersionAttribute != null) ? assemblyVersionAttribute.Version.Replace(".", "_") : "";
			string callerPK = TestPage.DataSourceIndexer.ToString();
			string expDate = ZDateTime.Now.AddDays(-1).ToString(ZDateTime.ShortDateFormat, ObjectCache.CultureProvider.Culture);

			string expectedOutput = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"" >
<HTML>
	<HEAD>
		<title>Login</title>
		<meta name=""GENERATOR"" Content=""Microsoft Visual Studio .NET 7.1"">
		<meta name=""CODE_LANGUAGE"" Content=""C#"">
		<meta name=""vs_defaultClientScript"" content=""JavaScript"">
		<meta name=""vs_targetSchema"" content=""http://schemas.microsoft.com/intellisense/ie5"">
	<link type=""text/css"" rel=""stylesheet"" href=""/BaseStyle.css"" />
</HEAD>
<Body id=""MainBodyControl"" MS_POSITIONING=""GridLayout"" runat=""server""><form>
<div class=""aspNetHidden"">
<input type=""hidden"" name=""__VIEWSTATE"" id=""__VIEWSTATE"" value="""" />
</div>

<SCRIPT>

                    function AttachToScrollEvent(EventHandlerFunction)
                    {
                        if(window.addEventListener) // Firefox 1+, Opera 9, Safari 3+, etc.
                        {
                            window.addEventListener(""scroll"", EventHandlerFunction, false);
                        }
                        else if(document.addEventListener) // Opera 7, Opera 8
                        {
                            document.addEventListener(""scroll"", EventHandlerFunction, false);
                        }
                        else if(""onscroll"" in self) // MSIE 6, 7 and MSIE 8
                        {
                            var oldonscroll = self.onscroll; 
                            if (typeof self.onscroll != 'function') 
                            { 
                                self.onscroll = EventHandlerFunction; 
                            } else 

                            { 
                                self.onscroll = function() 
                                    { 
                                        if (oldonscroll) 
                                        { 
                                            oldonscroll(); 
                                        } 
                                        EventHandlerFunction(); 
                                    } 
                            } 
                        };
                    }

                    function AttachToResizeEvent(EventHandlerFunction)
                    {
                        if(window.addEventListener) // Firefox 1+, Opera 9, Safari 3+, etc.
                        {
                            window.addEventListener(""resize"", EventHandlerFunction, false);
                        }
                        else if(document.addEventListener) // Opera 7, Opera 8
                        {
                            document.addEventListener(""resize"", EventHandlerFunction, false);
                        }
                        else if(""onresize"" in self) // MSIE 6, 7 and MSIE 8
                        {
                            var oldonresize = self.onscroll; 
                            if (typeof self.onresize != 'function') 
                            { 
                                self.onresize = EventHandlerFunction; 
                            } else 

                            { 
                                self.onresize = function() 
                                    { 
                                        if (oldonresize) 
                                        { 
                                            oldonresize(); 
                                        } 
                                        EventHandlerFunction(); 
                                    } 
                            } 
                        };
                    }

                    function DisablePage()
                    {
                        var PageSmokeScreen = $('_PageSmokeScreen');
                        if (PageSmokeScreen)
                        {
                            var DocumentSize = documentSize();
                            $(PageSmokeScreen).setStyle('left', '0px');
                            $(PageSmokeScreen).setStyle('top', '0px');
                            $(PageSmokeScreen).setStyle('width', DocumentSize.width + 'px');
                            $(PageSmokeScreen).setStyle('height', DocumentSize.height + 'px');
                            $(PageSmokeScreen).setStyle('z-index', '1500');
                            $(PageSmokeScreen).setStyle('display', 'block');
                        }
                    }

                    function EnablePage()
                    {
                        var PageSmokeScreen = $('_PageSmokeScreen');
                        if (PageSmokeScreen)
                        {
                            $(PageSmokeScreen).setStyle('z-index', 0);
                            $(PageSmokeScreen).setStyle('display', 'none');
                        }
                    }

                    

					</SCRIPT><SCRIPT>
                    function ResizeDetected(evt) 
                    {
                        PositionDebugConsole();
                    }

                    function ScrollingDetected(evt) 
                    {
                        PositionDebugConsole();
                    }

                    function PositionDebugConsole()
                    {
                        var DebugConsole = $('DebugConsole');
                        if (DebugConsole)
                        {
                            DebugConsole.style.left=posRight()-parseInt(DebugConsole.style.width)-15+'px';
                            DebugConsole.style.top=posTop()+'px';
                        }
                    }

                    function OnDebugLoad()
                    {
                        PositionDebugConsole();
                    }

                    function addDebugLoadEvent(func) 
                    { 
                        var oldonload = window.onload; 
                        if (typeof window.onload != 'function') 
                        { 
                            window.onload = func; 
                        } 
                        else 
                        { 
                            window.onload = function() 
                                { 
                                    if (oldonload) 
                                    { 
                                        oldonload(); 
                                    } 
                                    func(); 
                                } 
                        } 
                    }

                    addDebugLoadEvent(PositionDebugConsole);

                    AttachToScrollEvent(ScrollingDetected);

                    AttachToResizeEvent(ResizeDetected);

					function DebugConsole_NewMessage(Message)
					{
                        try{
                            if (Message)
                            {
                                var currentTime = new Date();
                                var hours = currentTime.getHours();
                                var minutes = currentTime.getMinutes();
                                var seconds = currentTime.getSeconds();
                                Message='<b>' + hours.toPrecision(2) + ':' + minutes.toPrecision(2) + ':' + parseInt(seconds).toPrecision(2) + ' ' + (hours > 11 ? 'PM' : 'AM') + '</b> - ' + Message;

                                var Str = $('DebugConsoleOutput').innerHTML;
                                Str += Message.toString() + '<br />';
                                $('DebugConsoleOutput').innerHTML = Str;
                            }
                        } catch(err) { }
					}

					function DebugConsole_Clear()
					{
                        $('DebugConsoleOutput').innerHTML = '';
					}
                    </SCRIPT><SCRIPT>

                    function PositionLightBox()
                    {
                        var LightBox = $('_LightBox');
                        if (LightBox)
                        {
                            var LightBoxLeft = posLeft()+(pageWidth()-parseInt(LightBox.getStyle('width')))/2;
                            var LightBoxTop = posTop()+300;
                            if (LightBoxLeft<0)
                            {
                                LightBoxLeft=0;
                            }
                            $(LightBox).setStyle('left', LightBoxLeft + 'px');
                            $(LightBox).setStyle('top', LightBoxTop + 'px');
                        }
                    }                    

					function ShowLightBox(Title, Message, Buttons, ResponseHolderID, AutoPostBack, ImageURL)
					{
                        var LightBox = $('_LightBox');
                        if (LightBox)
                        {
                            var LightBoxMessage = $('MessageBox');
                            var TitleBoxMessage = $('TitleBox');
                            var LightBoxButtons = $('ButtonBox');
                            if (LightBoxMessage && LightBoxButtons && TitleBoxMessage)
                            {
                                TitleBoxMessage.innerHTML = Title;
                                var MessageHTML = '<span class=""LightBoxMessageText"">' + Message + '</span>';
                                if (ImageURL!=null && ImageURL!='')
                                {
                                    MessageHTML='<table cellpading=""0"" cellspacing=""0""><tr><td valign=""top"" valign=""center"" width=""50""><img src=""' + ImageURL + '"" alt=""""></td><td valign=""top"" align=""center"">' + MessageHTML + '</td></tr></table>';
                                }
                                LightBoxMessage.innerHTML = MessageHTML;
                                var ButtonElement;
                                var ButtonsHTML = '';
                                var ButtonID = '';
                                var FirstButtonID = '';
                                for(var i=0; i<Buttons.length; i++)
                                {
                                    ButtonID = 'ButtonBox_' + i;
                                    if (i==0)
                                    {
                                        FirstButtonID=ButtonID;
                                    }
                                    ButtonElement = '<input type=""button"" id=""' + ButtonID + '"" value=""' + Buttons[i].caption + '"" style=""width: 100px;height:25px;margin:5px;"" onclick=""HideLightBox(); ' + Buttons[i].onclick + '"">';
                                    ButtonsHTML = ButtonsHTML + ButtonElement;
                                }
                                LightBoxButtons.innerHTML = ButtonsHTML;
                                DisablePage();
                                PositionLightBox();
                                $(LightBox).setStyle('z-index', '1700');
                                $(LightBox).setStyle('display', 'block');
                                setTimeout(""SetFocusOnLightBoxButton('"" + FirstButtonID + ""');"", 500);
                            }
                        }
					}

                    function SetFocusOnLightBoxButton(ButtonID)
                    {
                        try
                        {
                            $(ButtonID).focus();
                        }
                        catch(e) {}
                    }

                    AttachToScrollEvent(PositionLightBox);

                    AttachToResizeEvent(PositionLightBox);                    

					function HideLightBox()
					{
                        var LightBox = $('_LightBox');
                        if (LightBox)
                        {
                            $(LightBox).setStyle('display', 'none');
                        }
                        EnablePage();
					}

					</SCRIPT><SCRIPT src='/Runtime/Enterprise_ZArchitecture_Web_GUI/" + runtimeVersion + @"/ZPage/mootools.js'></SCRIPT>
<script src=""/Runtime/Enterprise_ZArchitecture_Web_GUI/" + runtimeVersion + @"/ZPage/ZScreen.js"" type=""text/javascript""></script>
<script src=""/Runtime/Enterprise_ZArchitecture_Web_GUI/" + runtimeVersion + @"/ZPage/ZPage.js"" type=""text/javascript""></script><SCRIPT>
                        var originalDoPostBack;

                        function customDoPostBack(eventTarget, eventArgument)
                        {
                            var performPostBack = true;
                            if (getElement(eventTarget))
                            {
                                try
                                {
                                    var postbackCondition = getElement(eventTarget).getProperty('postbackCondition'); 
                                    performPostBack = evaluateTrueFalseFunction(postbackCondition)
                                }
                                catch(e){}
                            }
                            if (performPostBack)
                            {
								
                                originalDoPostBack(eventTarget, eventArgument);
                            }
                        }

                        function evaluateTrueFalseFunction(functionCode)
                        {
                            if (functionCode && functionCode!='' && functionCode!='undefined')
                            {
                                var newFunction = new Function(functionCode);
                                try
                                {
                                    return newFunction();
                                }
                                catch(e){}
                            }
                            return true;
                        }

                        function setupPostBackHandlers()
                        {
                            originalDoPostBack = __doPostBack;
                            __doPostBack = customDoPostBack;
                        }

                        addLoadEvent(setupPostBackHandlers);
						
						

                        </SCRIPT>
<script src=""/Runtime/Enterprise_ZArchitecture_Web_GUI/" + runtimeVersion + @"/IWebServiceMethod/WebServiceMethod.js"" type=""text/javascript""></script>
<script src=""/Runtime/Enterprise_ZArchitecture_Web_GUI/" + runtimeVersion + @"/IWebServiceMethod/FormatDateWebServiceMethod.js"" type=""text/javascript""></script><SCRIPT>
                                        function ClearValidation(NotificationID)
                                        { 
                                            var Notification=getElement(NotificationID);
                                            if (Notification!=null)
                                            {
                                                Notification.setStyle('display', 'none');
                                            }
                                            return true;
                                        }
                                        </SCRIPT>
<script type='text/javascript'>        
function autoSizeIframe(frameId){
	try{
		var iframe = parent.document.getElementById(frameId);
		var innerDoc = (iframe.contentDocument) ? iframe.contentDocument : iframe.contentWindow.document;
        var objToResize = (iframe.style) ? iframe.style : iframe;

		objToResize.height = innerDoc.body.offsetHeight + GetMarginsSize(innerDoc.body);
	}
	catch(err){
		window.status = err.message;
	}
}

function GetMarginsSize(control){
	var margins = 0;
	var marginTop;
	var marginBottom;
	if (control.currentStyle){
		marginTop = control.currentStyle.marginTop;
		marginBottom = control.currentStyle.marginBottom;
	}
	else {
		marginTop = control.getStyle('margin-Top');
		marginBottom = control.getStyle('margin-Bottom');
	}

	if (marginTop){
		margins = parseInt(marginTop);
	}
	if (marginBottom){
		margins = margins + parseInt(marginBottom);
	}
	return margins;
}
</script><SCRIPT src='/Runtime/Enterprise_ZArchitecture_Web_GUI/" + runtimeVersion + @"/ZTextBoxButton/ZTextPopup/opener.js'></SCRIPT>
<script src=""/Runtime/Enterprise_ZArchitecture_Web_GUI/" + runtimeVersion + @"/ZTextBoxButton/ZTextPopup/ZTextIFramePopup/ZDateEditBox/calopener.js"" type=""text/javascript""></script><Div id=""ValidationControls""></Div><input type=""hidden"" name=""Testing"" id=""Testing"" /><input type=""hidden"" name=""__BEFORELASTFOCUSID"" id=""__BEFORELASTFOCUSID"" /><input type=""hidden"" name=""__LASTFOCUSID"" id=""__LASTFOCUSID"" /><input type=""hidden"" name=""__UPDATELASTFOCUSID"" id=""__UPDATELASTFOCUSID"" /><input name=""TextBox"" type=""text"" value=""Splatty"" maxlength=""2147483647"" id=""TextBox"" onchange=""ClearValidation(&#39;TextBox_NotificationID&#39;)"" /><span id=""Date"" style=""display:inline;""><span style=""display:inline;white-space:nowrap;""><span id=""Date_ctl00"" align=""left"" style=""display:inline-block;height:20px;width:90px;display:inline;""><input name=""Date$ctl00$TextBox"" type=""text"" value=""" + expDate + @""" maxlength=""9"" id=""Date_ctl00_TextBox"" onchange=""ClearValidation(&#39;Date_ctl00_TextBox_NotificationID&#39;);if (NormalizeAndValidateDate(&#39;Date_ctl00_TextBox&#39;,&#39;&#39;)) ZDateEdit_SetTime(&#39;Date_ctl01_TextBox&#39;,&#39;00:00&#39;); else ZDateEdit_SetTime(&#39;Date_ctl01_TextBox&#39;,&#39;&#39;);"" style=""width:68px;"" /><input name=""Date$ctl00$ctl00"" type=""button"" id=""Date_ctl00_ctl00"" style=""background:url(/Runtime/Enterprise_ZArchitecture_Web_GUI/" + runtimeVersion + @"/ZTextBoxButton/ZTextPopup/ZTextIFramePopup/ZDateEditBox/ZDateEditButton.gif) no-repeat center center;width:18px;height:20px;"" Class=""PopupButton"" Tabindex=""-1"" onclick=""ZTextPopup_ShowPopup(&#39;Date_ctl00_TextBox&#39;, &#39;Date_ctl00_ctl00&#39;, &#39;ZTextPopupZDateEditBox_1&#39;);ZDateEdit_ShowCalendar();"" /></span><img id=""Date_ctl00_TextBox_NotificationID"" src=""/Runtime/Enterprise_ZArchitecture_Web_GUI/" + runtimeVersion + @"/ZControlNotifications/MessageError.ico"" alt=""Message Error"" title=""Z0_Date should be in the future"" /></span><IFRAME id='ZTextPopupZDateEditBox_1' src='/Runtime/Enterprise_ZArchitecture_Web_GUI/" + runtimeVersion + @"/ZTextBoxButton/ZTextPopup/ZTextIFramePopup/ZDateEditBox/calendar.htm?OKFunction=ZTextPopup_SetValueAndHidePopup&CancelFunction=ZTextPopup_HidePopup&ControlID=Date_ctl00&CallerPK=" + callerPK + @"'  frameborder='no' style='display:none;' scrolling='no' class='ZFindBox PopupCalendar' ></IFRAME></span><span><input type=""submit"" name=""Save"" value=""Save"" id=""Save"" /></span><span><input type=""submit"" name=""Delete"" value=""Delete"" id=""Delete"" /></span><div id=""_LightBox"" class=""LightBox"" style=""display:none;position:absolute;"">
	<div id=""TitleBox"" class=""LightBoxTitle"">

	</div><table class=""LightBoxMessage"">
		<tr>
			<td align=""center"" valign=""top""><div id=""MessageBox"">

			</div></td>
		</tr><tr>
			<td align=""center"" valign=""middle""><div id=""ButtonBox"">

			</div></td>
		</tr>
	</table>
</div><Div id=""PageFooter""><span>&copy; <a href=""http://www.wisetechglobal.com"">WiseTech Global</a> " + DateTime.Now.Year + @". All rights reserved. </span><span id=""LanguageSelection"">Language: <select name=""LanguageList"" id=""LanguageList"" onchange=""changeLanguage(this.options[this.selectedIndex].value);"">
	<option selected=""selected"" value=""EN"">English</option>
	<option value=""ZH-CN"">简体中文</option>
	<option value=""FR-FR"">Fran&#231;ais</option>
	<option value=""DE-DE"">Deutsch</option>
	<option value=""ES-ES"">Espa&#241;ol</option>

</select></span></Div><div id=""DebugConsole"" style=""position:absolute;border:1px solid #000000;width:250px;height:250px;background-color:#ffffff;display:inline;padding:5px;"">
	<span style=""height:20px;"">JavaScript Console Started</span><div id=""DebugConsoleOutput"" style=""width:250px;height:230px;overflow:auto;"">

	</div>
</div>
<div class=""aspNetHidden"">

	<input type=""hidden"" name=""__EVENTVALIDATION"" id=""__EVENTVALIDATION"" value=""" + eventValidationValue + @""" />
</div></form></Body></HTML><div id=""_PageSmokeScreen"" class=""PageSmokeScreen"" style=""display:none;position:absolute;"">

</div><Noscript><div style=""z-index:999;background:LightYellow;border:solid;border-width:1px;border-color:black;position:absolute;top:0px;left:0px;"">
	<span>You do not have javascript enabled. This site requires javascript in order to operate correctly.<br>
Please enable javascript or add this site to the list of trusted sites.<p>
To enable javascript, do the following:<br>
<ol>
<li>Select the Tools Menu in Internet Explorer</li>
<li>Select <b>Internet Options</b></li>
<li>Select the <b>Security</b> tab</li>
<li>Click the <b>Custom Level</b> button</li>
<li>Scroll down to the <b>Scripting</b> heading</li>
<li>Select the <b>enable</b> option of <b>Active Scripting</b></li>
<li>Click <b>OK</b> in all open dialogs</li>
</ol>
For more detailed instructions regarding configuring trusted sites please <a href=http://www.microsoft.com/windows/ie/using/howto/security/settings.mspx>click here.</a></span>
</div></Noscript>";
			#endregion

			AssertMultilineASCIIEquals("RenderedOutput should match", expectedOutput, e.RenderedOutput);
		}
	}
}
