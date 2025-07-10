using System;
using System.Web.Script.Serialization;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public abstract class WebServiceParameters
	{
		#region Static

		public static T Parse<T>(string stringValue)
			where T : WebServiceParameters
		{
			T result = null;
			try
			{
				result = new JavaScriptSerializer().Deserialize<T>(stringValue);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new Exception("Please provide valid parameters for this method", ex);
			}
			result.Validate();
			return result;
		}

		#endregion

		#region Methods

		public void Validate()
		{
			ValidateCore();
		}

		protected virtual void ValidateCore()
		{
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
