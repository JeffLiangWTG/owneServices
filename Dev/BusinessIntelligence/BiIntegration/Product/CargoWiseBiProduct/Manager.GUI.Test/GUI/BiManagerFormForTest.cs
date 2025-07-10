using System;
using CargoWise.Bi.Product.Manager.Business;

namespace CargoWise.Bi.Product.Manager.GUI.Testing
{
	public class BiManagerFormForTest : BiManagerForm
	{
		public BiManagerFormForTest(BiMonitorBusinessObject businessEntity) : base(businessEntity)
		{
		}

		protected override bool IsAuditEnabled
		{
			set
			{
				throw new Exception("The server was not found or was not accessible");
			}
		}
	}
}
