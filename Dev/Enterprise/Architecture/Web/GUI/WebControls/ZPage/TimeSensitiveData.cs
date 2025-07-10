using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class TimeSensitiveData
	{
		public bool HasExpired => ZDateTime.UtcNow.ToDateTime().Subtract(LastAccess.ToDateTime()).TotalSeconds > 600;

		ZDateTime LastAccess { get; set; }

		public object Data
		{
			get
			{
				ResetAccess();

				return data;
			}
			set
			{
				ResetAccess();
				data = value;
			}
		}

		object data;

		void ResetAccess() => LastAccess = ZDateTime.UtcNow;
	}
}
