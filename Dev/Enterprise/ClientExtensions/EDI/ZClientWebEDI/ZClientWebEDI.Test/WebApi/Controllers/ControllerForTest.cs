using System;
using System.Net.Http;
using CargoWise.EntityFramework;
using Newtonsoft.Json.Linq;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class ControllerForTest<T> : BusinessObjectController where T : class, IBusiness
	{
		readonly Action<T> updateAction;
		public ControllerForTest(Action<T> updateAction)
		{
			this.updateAction = updateAction;
			RouteName = Guid.NewGuid().ToString("N");
			Request = new HttpRequestMessage();
		}

		public string RouteName { get; }

		public HttpResponseMessage UpdateBusinessObject(JObject jsonObject)
		{
			return UpdateBusinessObject(jsonObject, RouteName, updateAction);
		}
	}
}