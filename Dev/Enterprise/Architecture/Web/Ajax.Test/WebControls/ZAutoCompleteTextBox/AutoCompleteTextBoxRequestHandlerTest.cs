using System;
using System.IO;
using System.Web;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.Utilities.Test;
using Moq;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[HttpContextEnabledTest]
	sealed class AutoCompleteTextBoxRequestHandlerTest : TestCaseWithFactory
	{
		public void TestDisposableActionForDbConnectionForProcessRequest()
		{
			var errorReporterMock = new Mock<IErrorReporter>();
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				using (var env = new TestWebDbEnvironment())
				{
					env.SetServingWebBasedApp(true);
					Db.ResetAlreadyReported_ForTest();
					SetRequestQueryParams("austr", typeof(CountryAutoCompleteHelper), 1, null);
					AssertResponse("<li>Australia<div id=\"PK\" style=\"display:none\">*AU*</div></li>");
				}

				errorReporterMock.Verify(reporter => reporter.ReportDeveloperExceptionOrHandleSilently(It.IsAny<string>(), ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction, It.IsAny<InvalidOperationException>()), Times.Never);
			}
		}

		public void TestProcessRequest()
		{
			SetRequestQueryParams("austr", typeof(CountryAutoCompleteHelper), 10, null);
			AssertResponse("<li>Australia<div id=\"PK\" style=\"display:none\">*AU*</div></li><li>Austria<div id=\"PK\" style=\"display:none\">*AT*</div></li>");
		}

		public void TestCheckMaxOptionsCount()
		{
			SetRequestQueryParams("austr", typeof(CountryAutoCompleteHelper), 1, null);
			AssertResponse("<li>Australia<div id=\"PK\" style=\"display:none\">*AU*</div></li>");
		}

		public void TestCheckMultiLineOptions()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			OrgAddress address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "Address1";
			address.OA_Address2 = "Address2";
			address.OA_Code = "Code";
			address.OA_OH = org.PK;

			Factory.Save();

			OrgAddressAutoCompleteHelper autoCompleteHelper = new OrgAddressAutoCompleteHelper(null);
			autoCompleteHelper.ParentPK = org.PK;

			SetRequestQueryParams("addr", autoCompleteHelper.GetType(), 10, autoCompleteHelper.SerializeAdditionalParamsToString());
			AssertResponse(String.Format("<li><span>Address1<div id=\"PK\" style=\"display:none\">{0}</div></span><b>Address2<br />Code<br /></b></li>", address.PK));
		}

		#region Implementation

		void SetRequestQueryParams(string key, Type helperType, int itemsCount, string additionalParams)
		{
			QueryParamsEncoder encoder = new QueryParamsEncoder();

			HttpContext.Current.Request.QueryString[AutoCompleteTextBoxRequestHandler.Key] = key;
			HttpContext.Current.Request.QueryString[AutoCompleteTextBoxRequestHandler.Helper] = encoder.Encrypt(helperType.AssemblyQualifiedName);
			HttpContext.Current.Request.QueryString[AutoCompleteTextBoxRequestHandler.MaxItemsCount] = encoder.Encrypt(itemsCount.ToString());
			if (additionalParams != null)
			{
				HttpContext.Current.Request.QueryString[AutoCompleteTextBoxRequestHandler.Params] = encoder.Encrypt(additionalParams);
			}
		}

		void AssertResponse(string expectedHTML)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				HttpContext.Current.Response.Filter = new TestResponseFilter(HttpContext.Current.Response.Filter, ms);

				AutoCompleteTextBoxRequestHandler handler = new AutoCompleteTextBoxRequestHandler();
				handler.ProcessRequest(HttpContext.Current);

				AssertEquals("The content type should be text/plain.", "text/plain", HttpContext.Current.Response.ContentType);

				HttpContext.Current.Response.Flush();
				ms.Position = 0;
				string responseText;
				using (StreamReader reader = new StreamReader(ms))
				{
					responseText = reader.ReadToEnd();
				}

				AssertEquals("Expected HTML", expectedHTML, responseText);
			}
		}

		#endregion
	}
}
