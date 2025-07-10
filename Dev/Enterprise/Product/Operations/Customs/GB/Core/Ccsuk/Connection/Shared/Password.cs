using System;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class Password
	{
		public string Existing
		{
			get
			{
				return Format(GB.Registry.GBCustomsDataRegistry.Instance.CcsukPassword.Value);
			}
		}

		public string Format(ZString p)
		{
			return p.Left(14).PadRight(14, ' ').ToUpper();
		}

		public void Save(string newPassword)
		{
			GB.Registry.GBCustomsDataRegistry.Instance.CcsukPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newPassword);
		}
	}
}
