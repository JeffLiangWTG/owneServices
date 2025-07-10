using System;

namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public class PortFinderParameters : WebServiceParameters
	{
		#region Properties

		public string PortControlID { get; set; }
		public string PostalCode { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Country { get; set; }

		#endregion

		#region Override

		protected override void ValidateCore()
		{
			base.ValidateCore();
			if (string.IsNullOrEmpty(PortControlID))
			{
				throw new ArgumentNullException("PortControlID");
			}
		}

		#endregion
	}
}
