using System;
using System.Collections.Specialized;
using Enterprise.ZArchitecture.Web.Business.Testing;
using static Enterprise.ZArchitecture.Web.GUI.WebControls.ZTextIFramePopup;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public abstract class ZTextIFramePopupTest : ZTextPopupTest
	{
		public virtual void TestEnableFrameScrolling()
		{
			AssertEquals(FrameScroll.no, TextIFramePopup.EnableFrameScrollingInternal);
		}

		#region Overrides

		[HttpContextEnabledTest]
		public override void TestClickHandlerAssignment()
		{
			string expectedClickHandler;

			if (TextIFramePopup.LoadIFrameOnDemand)
			{
				expectedClickHandler = string.Format("ZTextPopup_ShowIFramePopup('ctl01_TextBox', 'ctl01_ctl00', '{0}', '{1}', true);", ExpectedPopupID, ExpectedIFrameSourceString);
			}
			else
			{
				expectedClickHandler = string.Format("ZTextPopup_ShowPopup('ctl01_TextBox', 'ctl01_ctl00', '{0}');", ExpectedPopupID) + TextIFramePopup.AdditionalButtonClickHandlerInternal;
			}

			AssertEquals(expectedClickHandler, TextIFramePopup.ButtonClickHandlerInternal);
		}

		#endregion

		#region AdditionalParameters

		public void TestAdditionalParametersCallsBase()
		{
			AssertNotNull("AdditionalParameters should be not null", TextIFramePopup.AdditionalParametersInternal);
			Assert("AdditionalParameters should call base to get base additional parameters", TextIFramePopup.BaseAdditionalParametersCalled);
		}

		public void TestAdditionalParameters()
		{
			AssertNotNull("AdditionalParameters should be not null", TextIFramePopup.AdditionalParametersInternal);
			NameValueCollection expectedParams = ExpectedAdditionalParameters;
			AssertNotNull("Expected AdditionalParameters should not be null", expectedParams);

			AssertEquals("Count should be as expected", expectedParams.Count, TextIFramePopup.AdditionalParametersInternal.Count);
			foreach (string key in expectedParams.AllKeys)
			{
				AssertNotNull("Should be a parameter with expected Key: " + key, TextIFramePopup.AdditionalParametersInternal[key]);
				AssertEquals("Value for the Key: " + key + " should be as expected", expectedParams[key], TextIFramePopup.AdditionalParametersInternal[key]);
			}
		}

		#endregion AdditionalParameters

		#region IFrameSource/EventHandlers

		public void TestIFrameSourcePage()
		{
			AssertEquals("IFrame Source Page", ExpectedIFrameSourcePage, GetResourceName(TextIFramePopup.IFrameSourcePage.FileName));
		}

		protected ZTextIFramePopup TextIFramePopup
		{
			get { return (ZTextIFramePopup)Control; }
		}

		public void TestIFrameSourcePageContainerType()
		{
			AssertEquals("ContainerType", ExpectedIFrameSourcePageContainerType, TextIFramePopup.IFrameSourcePageContainerTypeInternal);
		}

		protected void TestIFrameEventHandlers()
		{
			AssertEquals("IFrameEventHandlers", ExpectedIFrameEventHandlers, TextIFramePopup.IFrameEventHandlersInternal);
		}

		#endregion IFrameSource/EventHandlers

		#region OK/Cancel FunctionNames

		protected void TestCancelFunctionName()
		{
			AssertEquals("CancelFunctionName", ExpectedCancelFunctionName, TextIFramePopup.CancelFunctionNameInternal);
		}

		protected void TestOKFunctionName()
		{
			AssertEquals("OKFunctionName", ExpectedOKFunctionName, TextIFramePopup.OKFunctionNameInternal);
		}

		#endregion OK/Cancel FunctionNames

		#region Implementation

		protected virtual string ExpectedIFrameSourcePage
		{
			get { return "ChangeCategoryPage.aspx"; }
		}

		protected virtual string ExpectedPathToIFrameSourcePage
		{
			get { return "/Runtime/Enterprise_ZArchitecture_Web_GUI/" + RuntimeVersion + "/ZTextBoxButton/ZTextPopup/ZTextIFramePopup/ZButtonPopup/ChangeCategoryPopup/"; }
		}

		protected virtual string ExpectedIFrameSourcePageParameters
		{
			get
			{
				string formatString = "OKFunction={0}&CancelFunction={1}&ControlID=ctl01&CallerPK={2}";
				return string.Format(formatString, ExpectedOKFunctionName, ExpectedCancelFunctionName, ExpectedCallerPK);
			}
		}

		protected virtual NameValueCollection ExpectedAdditionalParameters
		{
			get
			{
				NameValueCollection result = new NameValueCollection();
				result.Add(ZIFramePage.CallerPKQuery, ExpectedCallerPK);
				return result;
			}
		}

		protected string ExpectedIFrameSourceString
		{
			get
			{
				string sourceString = ExpectedPathToIFrameSourcePage + ExpectedIFrameSourcePage + "?" + ExpectedIFrameSourcePageParameters;

				foreach (string key in ExpectedAdditionalParameters.AllKeys)
				{
					if (!sourceString.Contains("&" + key + "="))
					{
						sourceString += "&" + key + "=" + ExpectedAdditionalParameters[key];
					}
				}

				return sourceString;
			}
		}

		protected virtual Type ExpectedIFrameSourcePageContainerType
		{
			get { return TextIFramePopup.GetType(); }
		}

		protected virtual string ExpectedIFrameEventHandlers
		{
			get { return ""; }
		}

		protected virtual string ExpectedCallerPK
		{
			get { return TextIFramePopup.CallerPKInternal; }
		}

		protected virtual string ExpectedOKFunctionName
		{
			get { return "ZTextPopup_SetValueAndHidePopup"; }
		}

		protected virtual string ExpectedCancelFunctionName
		{
			get { return "ZTextPopup_HidePopup"; }
		}

		#endregion
	}
}
