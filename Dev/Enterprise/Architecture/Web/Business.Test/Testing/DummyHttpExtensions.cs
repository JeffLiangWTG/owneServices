#if NETFRAMEWORK
using System.Collections;
using System.Reflection;
using System.Web;
#elif NET
using Microsoft.AspNetCore.Http;
#endif

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
#if NETFRAMEWORK
	public static class DummyHttpExtensions
	{
		public static void SetServerVariableValue(this HttpRequest request, string key, string value)
		{
			var variables = request.ServerVariables;
			var type = variables.GetType();

			type.InvokeMember("MakeReadWrite", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, variables, null);
			type.InvokeMember("AddStatic", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, variables, new string[] { key, value });
			type.InvokeMember("MakeReadOnly", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, variables, null);
		}

		public static (string, string) GetHeaderForTest(this HttpResponse response, int index)
		{
			var headers = (ArrayList)typeof(HttpResponse).GetField("_customHeaders", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(response);
			var header = headers[index];

			var name = (string)header.GetType().GetProperty("Name", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(header);
			var value = (string)header.GetType().GetProperty("Value", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(header);

			return (name, value);
		}

		public static void SetRequestHeader(this HttpRequest request, string key, string value)
		{
			var headers = request.Headers;
			var type = headers.GetType();
			var item = new ArrayList { value };

			type.InvokeMember("MakeReadWrite", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, headers, null);
			type.InvokeMember("BaseAdd", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, headers, new object[] { key, item });
			type.InvokeMember("MakeReadOnly", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, headers, null);
		}
	}
#elif NET
	public static class DummyHttpExtensions
	{
		public static void SetServerVariableValue(this HttpRequest request, string key, string value)
		{
			request.Headers[key] = value;
		}

		public static void SetRequestHeader(this HttpRequest request, string key, string value)
		{
			request.Headers[key] = value;
		}

		public static (string, string) GetHeaderForTest(this HttpResponse response, int index)
		{
			int i = 0;
			foreach (var header in response.Headers)
			{
				if (i == index)
				{
					return (header.Key, header.Value.ToString());
				}
				i++;
			}

			return (null, null);
		}

		public static void SetRequestCookie(this HttpRequest request, string key, string value)
		{
			request.Headers["Cookie"] = $"{key}={value};";
		}

		public static void SetResponseCookie(this HttpResponse response, string key, string value)
		{
			response.Headers.Append("Cookie", $"{key}={value};");
		}
	}
#endif
}
