using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public class WebServiceResponse : List<WebServiceResponseActionToken>
	{
		#region Casting

		public static implicit operator string(WebServiceResponse value)
		{
			return value.ToString();
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return new JavaScriptSerializer().Serialize(this);
		}

		#endregion
	}
}
