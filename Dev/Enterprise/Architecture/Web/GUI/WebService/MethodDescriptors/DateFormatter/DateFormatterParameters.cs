using System;

namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public class DateFormatterParameters : WebServiceParameters
	{
		#region Properties

		public string DateValue { get; set; }
		public string DateControlID { get; set; }
		public string DateFormatType { get; set; }
		public string TimeControlID { get; set; }
		public string TimeValue { get; set; }

		#endregion

		#region Override

		protected override void ValidateCore()
		{
			base.ValidateCore();
			if (string.IsNullOrEmpty(DateControlID))
			{
				throw new ArgumentNullException("DateControlID");
			}
		}

		#endregion
	}
}
