using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	[HttpContextEnabledTest]
	public abstract class WebServiceMethodTest<T> : TestCaseWithFactory
			where T : IWebServiceMethod
	{
		#region Test Cases

		public virtual void TestExecute()
		{
			TestExecuteSetup();
			BeforeExecute = new Dictionary<string, Action>();
			var parametersAndResponseTokens = MethodParametersAndExpectedResponseTokens;
			foreach (string testMethodParameters in parametersAndResponseTokens.Keys)
			{
				if (BeforeExecute.TryGetValue(testMethodParameters, out Action beforeExecute))
				{
					beforeExecute?.Invoke();
				}

				WebServiceResponse expectedResponse = parametersAndResponseTokens[testMethodParameters];
				WebServiceResponse actualResponse = TestMethod.Execute(testMethodParameters);
				AssertEquals(testMethodParameters, expectedResponse.Count, actualResponse.Count);
				for (int i = 0; i < expectedResponse.Count; i++)
				{
					AssertWebServiceResponseToken(testMethodParameters, expectedResponse[i], actualResponse[i]);
				}
			}
		}

		public void TestExecuteWithInvalidParameters()
		{
			WebServiceResponse response = TestMethod.Execute(GetInvalidParameters());
			if (MethodNeverReturnsErrors)
			{
				AssertEquals(0, response.Count);
			}
			else
			{
				AssertEquals(1, response.Count);
				AssertEquals(WebServiceResponseActions.ShowError, response[0].Action);
				AssertEquals("Please provide valid parameters for this method", response[0].Value);
			}
		}

		public void TestServiceScripts()
		{
			ZPage testPage = new ZPage();
			if (ShouldUseCommonServiceScript())
			{
				Assert("ServiceScripts should contain Common Script Resource", TestMethod.ServiceScripts(testPage).ContainsKey("WebServiceMethodScriptKey"));
				ZWebResource commonResource = TestMethod.ServiceScripts(testPage)["WebServiceMethodScriptKey"];
				AssertNotNull(commonResource);
				AssertEquals(string.Format("/Runtime/Enterprise_ZArchitecture_Web_GUI/{0}/IWebServiceMethod/WebServiceMethod.js", commonResource.AssemblyVersion.Replace(".", "_")), commonResource.FileName);
			}
			else
			{
				Assert("ServiceScripts should not contain Common Script Resource", !TestMethod.ServiceScripts(testPage).ContainsKey("WebServiceMethodScriptKey"));
			}
			if (ShouldUseMethodSpecificServiceScript())
			{
				Assert("ServiceScripts should contain method specific Script Resource", TestMethod.ServiceScripts(testPage).ContainsKey(GetExpectedMethodSpecificServiceScriptKey()));
				ZWebResource specificResource = TestMethod.ServiceScripts(testPage)[GetExpectedMethodSpecificServiceScriptKey()];
				AssertNotNull(specificResource);
				AssertEquals(string.Format(GetExpectedRunTimeAssemblyFormatForScriptReference(), specificResource.AssemblyVersion.Replace(".", "_"), GetExpectedMethodSpecificServiceScriptFileName()), specificResource.FileName);
			}
			else
			{
				Assert("ServiceScripts should not contain Common Script Resource", !TestMethod.ServiceScripts(testPage).ContainsKey("WebServiceMethodScriptKey"));
			}
		}

		protected virtual string GetExpectedRunTimeAssemblyFormatForScriptReference()
		{
			return "/Runtime/Enterprise_ZArchitecture_Web_GUI/{0}/IWebServiceMethod/{1}";
		}

		public void TestMethodName()
		{
			AssertEquals(GetExpectedMethodName(), TestMethod.MethodName);
		}

		public void TestWebServiceReference()
		{
			AssertNotNull(TestMethod.WebServiceReference);
			AssertEquals(GetExpectedServiceReferencePath(), TestMethod.WebServiceReference.Path);
		}

		#endregion

		#region Implementation

		protected virtual void TestExecuteSetup()
		{
		}

		protected virtual void AssertWebServiceResponseToken(string testMethodParameters, WebServiceResponseActionToken expectedResponse, WebServiceResponseActionToken actualResponse)
		{
			AssertEquals(testMethodParameters, expectedResponse.Action, actualResponse.Action);
			AssertEquals(testMethodParameters, expectedResponse.ControlID, actualResponse.ControlID);
			AssertEquals(testMethodParameters, expectedResponse.Value, actualResponse.Value);

			AssertWebServiceResponseTokenConditions(testMethodParameters, expectedResponse.Conditions, actualResponse.Conditions);
		}

		protected virtual void AssertWebServiceResponseTokenConditions(string testMethodParameters, List<WebServiceResponseConditionToken> expectedConditions, List<WebServiceResponseConditionToken> actualConditions)
		{
			AssertEquals($"Conditions count should match: {testMethodParameters}", expectedConditions.Count, actualConditions.Count);

			var missingConditions = expectedConditions.Where(e => !actualConditions.All(a => a.Condition.Equals(e.Condition) && a.ControlID.Equals(e.ControlID) && a.ConditionValue.Equals(e.ConditionValue)));
			var newLine = System.Environment.NewLine;
			var missingConditionsDescription = string.Join(newLine, missingConditions.Select(m => $"ControlID: {m.ControlID}{newLine}Condition: {m.Condition}{newLine}Value: {m.ConditionValue}"));
			AssertEquals($"Conditions should match: {testMethodParameters}{newLine}Missing condition(s){newLine}{missingConditionsDescription}", true, missingConditionsDescription.IsNullOrEmpty());
		}

		protected Dictionary<string, WebServiceResponse> MethodParametersAndExpectedResponseTokens
		{
			get
			{
				Dictionary<string, WebServiceResponse> result = new Dictionary<string, WebServiceResponse>();
				SetMethodParametersAndExpectedResponseTokens(result);
				return result;
			}
		}

		protected Dictionary<string, Action> BeforeExecute { get; private set; }

		protected abstract void SetMethodParametersAndExpectedResponseTokens(Dictionary<string, WebServiceResponse> setting);

		protected virtual bool MethodNeverReturnsErrors
		{
			get { return false; }
		}

		protected virtual string GetInvalidParameters()
		{
			return "Test:Test";
		}

		protected virtual string GetExpectedMethodSpecificServiceScriptKey()
		{
			return GetExpectedMethodName() + "WebServiceMethodScriptKey";
		}

		protected virtual string GetExpectedMethodSpecificServiceScriptFileName()
		{
			return GetExpectedMethodName() + "WebServiceMethod.js";
		}

		protected virtual bool ShouldUseCommonServiceScript()
		{
			return true;
		}

		protected virtual bool ShouldUseMethodSpecificServiceScript()
		{
			return true;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestMethod = GetNewWebServiceMethod();
		}

		protected T TestMethod;

		protected abstract T GetNewWebServiceMethod();
		protected abstract string GetExpectedMethodName();

		protected virtual string GetExpectedServiceReferencePath()
		{
			return "/WebService/WebServiceShared.asmx";
		}

		#endregion
	}
}
