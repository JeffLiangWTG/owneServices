using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public abstract class WebServiceResponseToken
	{
		#region Constructors

		public WebServiceResponseToken()
		{
		}

		public WebServiceResponseToken(string controlID, string value)
			: this()
		{
			this.ControlID = controlID;
			this.Value = value;
		}

		#endregion

		#region Casting

		public static implicit operator string(WebServiceResponseToken value)
		{
			return value.ToString();
		}

		#endregion

		#region Properties

		public List<WebServiceResponseConditionToken> Conditions
		{
			get
			{
				return conditions ?? (conditions = new List<WebServiceResponseConditionToken>());
			}
		}

		List<WebServiceResponseConditionToken> conditions;

		public string ControlID { get; set; }

		public string Value { get; set; }

		#endregion

		#region Overrides

		public override string ToString()
		{
			return new JavaScriptSerializer().Serialize(this);
		}

		#endregion
	}
}
